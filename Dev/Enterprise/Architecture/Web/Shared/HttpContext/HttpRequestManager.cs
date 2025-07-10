#if NETFRAMEWORK
using System.Web;
#else
using Microsoft.AspNetCore.Http;
#endif
using CargoWise.Common;

namespace Enterprise.ZArchitecture.Web.Shared
{
	public class HttpRequestManager : IHttpRequestManager
	{
#if NETFRAMEWORK
		public HttpContextBase GetHttpContextBase()
		{
			var request = HttpContext.Current?.Request;
			if (request == null)
			{
				ErrorReporter.ReportOnce("Http Context was null during login request.");
			}

			// We cannot create a HttpContextWrapper from a null HttpContext
			return request == null ? null : new HttpContextWrapper(HttpContext.Current);
		}
#else
		readonly HttpContext httpContext;

		public HttpRequestManager(HttpContext httpContext)
		{
			this.httpContext = httpContext;
		}

		public HttpContext GetHttpContext()
		{
			if (httpContext == null)
			{
				ErrorReporter.ReportOnce("HttpContext was null during request.");
			}

			return httpContext;
		}
#endif
	}
}
