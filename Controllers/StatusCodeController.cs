using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http;
using System.Threading.Tasks;

namespace GET.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StatusCodeController : ControllerBase
    {
        private readonly IMemoryCache _cache;
        private static readonly HttpClient _httpClient = new HttpClient();

        public StatusCodeController(IMemoryCache cache)
        {
            _cache = cache;
        }

        [HttpGet]
        public async Task<IActionResult> GetStatusCodeImage([FromQuery] string? url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return BadRequest("URL не может быть пустым или NULL.");
            }

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.GetAsync(url);
            }
            catch
            {
                return BadRequest("Недопустимый URL или проблема с сетью.");
            }

            var statusCode = (int)response.StatusCode;

            if (!_cache.TryGetValue(statusCode, out byte[]? image))
            {
                try
                {
                    image = await _httpClient.GetByteArrayAsync($"https://http.cat/{statusCode}");
                    var cacheEntryOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                    };
                    _cache.Set(statusCode, image, cacheEntryOptions);
                }
                catch
                {
                    return StatusCode(500, "Ошибка при получении изображения.");
                }
            }

            return image == null ? StatusCode(500, "Ошибка при получении изображения.") : File(image, "image/jpeg");
        }
    }
}
