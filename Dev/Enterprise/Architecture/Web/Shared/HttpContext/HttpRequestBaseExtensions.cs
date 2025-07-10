using System;
#if NETFRAMEWORK
using System.Web;
#else
using Microsoft.AspNetCore.Http;
#endif

namespace Enterprise.ZArchitecture.Web.Shared
{
#if NETFRAMEWORK
	public static class HttpRequestBaseExtensions
	{
		public static ConnectionType DetermineConnectionType(this HttpRequestBase request)
		{
			var result = ConnectionType.Undefined;
			if (request != null)
			{
				result = request.IsSecureConnection || IsSecureConnectionForwardedByLoadBalancer(request)
					? ConnectionType.HttpsConnection
					: ConnectionType.HttpConnection;
			}
			return result;
		}

		static bool IsSecureConnectionForwardedByLoadBalancer(HttpRequestBase request)
			=> string.Equals(request.ServerVariables["HTTP_X_FORWARDED_PROTO"], Uri.UriSchemeHttps, StringComparison.InvariantCultureIgnoreCase);
	}
#else
	public static class HttpRequestBaseExtensions
	{
		public static ConnectionType DetermineConnectionType(this HttpRequest request)
		{
			var result = ConnectionType.Undefined;
			if (request != null)
			{
				result = request.IsHttps || IsSecureConnectionForwardedByLoadBalancer(request)
					? ConnectionType.HttpsConnection
					: ConnectionType.HttpConnection;
			}
			return result;
		}

		static bool IsSecureConnectionForwardedByLoadBalancer(HttpRequest request) =>
			string.Equals(request.Headers["X-Forwarded-Proto"], Uri.UriSchemeHttps, StringComparison.InvariantCultureIgnoreCase);
	}
#endif
}
