#if NETFRAMEWORK
using System.Collections.Specialized;
using System.Web;
using Moq;
#else
using Microsoft.AspNetCore.Http;
#endif

namespace Enterprise.ZArchitecture.Web.Shared.Testing
{
	public static class MockHttpContext
	{
#if NETFRAMEWORK
		public static HttpContextBase PrepareMockHttpContextWrapper(bool isSecureConnection, bool isRedirectedFromLoadBalancer)
		{
			var httpRequest = new Mock<HttpRequestBase>();
			httpRequest.Setup(hr => hr.IsSecureConnection).Returns(isSecureConnection);

			var variables = new NameValueCollection { { "HTTP_X_FORWARDED_PROTO", isRedirectedFromLoadBalancer ? "https" : "http" } };
			httpRequest.Setup(mr => mr.ServerVariables).Returns(variables);

			var httpContext = new Mock<HttpContextBase>();
			httpContext.Setup(x => x.Request).Returns(httpRequest.Object);

			return httpContext.Object;
		}
#else
		public static HttpContext PrepareMockHttpContextWrapper(bool isSecureConnection, bool isRedirectedFromLoadBalancer)
		{
			var context = new DefaultHttpContext();

			context.Request.Scheme = isSecureConnection ? "https" : "http";

			context.Request.Headers["X-Forwarded-Proto"] = isRedirectedFromLoadBalancer ? "https" : "http";

			return context;
		}
#endif
	}
}
