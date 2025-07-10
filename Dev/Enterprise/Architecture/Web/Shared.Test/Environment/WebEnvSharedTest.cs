using System.Threading;
using CargoWise.EntityFramework.Testing;

#if NETFRAMEWORK
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.SessionState;
#else
using Microsoft.AspNetCore.Http;
#endif

namespace Enterprise.ZArchitecture.Web.Shared.Testing
{
	sealed class WebEnvSharedTest : TestCaseWithFactory
	{
#if NETFRAMEWORK
		#region Test Cases

		public void TestClientCultureShouldReturnDefaultCultureIfUnknownCulture()
		{
			HttpContext.Current = FakeHttpContext();
			HttpContext.Current.Request.GetType().GetField("_userLanguages", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy).SetValue(HttpContext.Current.Request, new string[] { "Japan", "XX-xx", "yo-ZZ", "fr-NO" });
			var culture = WebEnvShared.ClientCulture;
			AssertEquals(Thread.CurrentThread.CurrentCulture, culture);
		}

		#endregion

		#region Implementation
		public static HttpContext FakeHttpContext()
		{
			var httpRequest = new HttpRequest("", "http://wisetechglobal.com/", "");
			var stringWriter = new StringWriter();
			var httpResponce = new HttpResponse(stringWriter);
			var httpContext = new HttpContext(httpRequest, httpResponce);

			var sessionContainer = new HttpSessionStateContainer("id", new SessionStateItemCollection(),
													new HttpStaticObjectsCollection(), 10, true,
													HttpCookieMode.AutoDetect,
													SessionStateMode.InProc, false);

			httpContext.Items["AspSession"] = typeof(HttpSessionState).GetConstructor(
										BindingFlags.NonPublic | BindingFlags.Instance,
										null, CallingConventions.Standard,
										new[] { typeof(HttpSessionStateContainer) },
										null)
								.Invoke(new object[] { sessionContainer });

			return httpContext;
		}
		#endregion
#else
		public void TestClientCultureShouldReturnDefaultCultureIfUnknownCulture()
		{
			var context = FakeHttpContext();
			// Simulate Accept-Language header with unknown cultures
			context.Request.Headers["Accept-Language"] = "Japan,XX-xx,yo-ZZ,fr-NO";
			WebEnvShared.HttpContext = context;
			var culture = WebEnvShared.ClientCulture;
			AssertEquals(Thread.CurrentThread.CurrentCulture, culture);
		}

		// Helper to create a fake HttpContext for ASP.NET Core
		public static DefaultHttpContext FakeHttpContext()
		{
			var context = new DefaultHttpContext();
			// Add more setup as needed (e.g., session, items, etc.)
			return context;
		}
#endif
	}
}
