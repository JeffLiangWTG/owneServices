using System.Net;

namespace Enterprise.Dash.Business.Extensions
{
	public static class HttpStatusCodeExtensions
	{
		public static bool CanRetry(this HttpStatusCode httpStatusCode) =>
			httpStatusCode is HttpStatusCode.RequestTimeout
				or HttpStatusCode.InternalServerError
				or HttpStatusCode.ServiceUnavailable
				or HttpStatusCode.GatewayTimeout;
	}
}
