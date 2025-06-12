using System.Net.Http;
using System.Text;
using System.Threading;

namespace CargoWise.eHub.Products.TWCustoms.TWCustomsGatewayAdapter
{
	public interface IHttpClient
	{
		HttpResponseMessage PostAsync(string endpoint, string messageContent);
	}
	public class HttpClientWrapper : IHttpClient
	{
		readonly HttpClient httpClient;

		public HttpClientWrapper()
		{
			httpClient ??= new HttpClient();
		}

		public HttpResponseMessage PostAsync(string endpoint, string messageContent)
		{
			return (httpClient ?? new HttpClient()).PostAsync(endpoint, new StringContent(messageContent, Encoding.UTF8, "application/xml"), CancellationToken.None).Result;
		}
	}
}
