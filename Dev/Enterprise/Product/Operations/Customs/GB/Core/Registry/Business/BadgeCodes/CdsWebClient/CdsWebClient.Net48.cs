#if NET48
using System.Net;
using System.Threading.Tasks;

namespace Enterprise.Customs.GB.Registry
{
	public partial class CdsWebClient : WebClient, IWebClient
	{
		public HttpStatusCode GetStatusCode(WebException ex)
		{
			return ex.Response is HttpWebResponse response
				? response.StatusCode
				: 0;
		}

		public Task<HttpStatusCode> GetStatusCodeAsync(string url)
		{
			return Task.Run(() =>
			{
				try
				{
					base.DownloadString(url);
					return HttpStatusCode.OK;
				}
				catch (WebException ex)
				{
					return GetStatusCode(ex);
				}
			});
		}
	}
}
#endif
