using System;
using System.Net;
using System.Threading.Tasks;

namespace Enterprise.Customs.GB.Registry
{
	public interface IWebClient : IDisposable
	{
		string DownloadString(string address);
		HttpStatusCode GetStatusCode(WebException ex);
		Task<HttpStatusCode> GetStatusCodeAsync(string url);

#if NET8_0_OR_GREATER
		HttpStatusCode GetStatusCode(System.Net.Http.HttpRequestException ex);
#endif
	}
}
