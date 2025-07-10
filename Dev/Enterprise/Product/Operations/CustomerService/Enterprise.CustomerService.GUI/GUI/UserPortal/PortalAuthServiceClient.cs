using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using CargoWise.Common;
using Enterprise.Registry.Business;

namespace Enterprise.UserPortal
{
	internal class PortalAuthServiceClient
	{
		static string BaseUri => WebDataRegistry.Instance.CargoWiseUserPortalUrl.Value.TrimEnd('/');
		internal static string AutoLoginUrl => BaseUri + "/api/PortalAuth/AutoLoginUrl";

		public Tuple<Uri, HttpStatusCode> GetUrl(string requestUri, SecureQueryString secureQueryString, CancellationToken cancelToken, int timeoutMs = 20000)
		{
			HttpStatusCode statusCode;
			Uri uri = null;

			using (var client = new HttpClient())
			{
				requestUri += '?' + SecureQueryString.QueryStringKey + '=' + WebUtility.UrlEncode(secureQueryString.ToString());
				var task = client.GetAsync(requestUri, cancelToken);

				if (!task.Wait(timeoutMs, cancelToken))
				{
					statusCode = HttpStatusCode.RequestTimeout;
				}
				else if (task.Result.IsSuccessStatusCode)
				{
					string content = task.Result.Content.ReadAsStringAsync().Result;
					uri = new Uri(content.Trim('\"'));
					statusCode = HttpStatusCode.OK;
				}
				else
				{
					statusCode = task.Result.StatusCode;
				}

				return new Tuple<Uri, HttpStatusCode>(uri, statusCode);
			}
		}
	}
}
