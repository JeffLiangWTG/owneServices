#if NET8_0_OR_GREATER
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Enterprise.Customs.GB.Registry
{
	public partial class CdsWebClient
	{
		readonly HttpClient _httpClient;

		public CdsWebClient(HttpClient httpClient)
		{
			_httpClient = httpClient;
		}

		public string DownloadString(string address)
		{
			return _httpClient.GetStringAsync(address).GetAwaiter().GetResult();
		}

		public HttpStatusCode GetStatusCode(WebException ex) => 0;

		public async Task<HttpStatusCode> GetStatusCodeAsync(string url)
		{
			try
			{
				using var response = await _httpClient.GetAsync(url);
				return response.StatusCode;
			}
			catch (HttpRequestException ex)
			{
				return GetStatusCode(ex);
			}
		}

		public HttpStatusCode GetStatusCode(HttpRequestException ex)
		{
			return ex.StatusCode ?? 0;
		}

		public void Dispose() { /* Managed by DI or caller */ }
	}
}
#endif
