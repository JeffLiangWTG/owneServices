using System;
using System.Security.Principal;
using System.Web;
using System.Web.Security;
using Enterprise.ZArchitecture.Web.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Utilities.Testing
{
	[HttpContextEnabledTest]
	internal class UserActivityLoggerTest : TestCase
	{
		#region Implementation

		public void TestDoesNotStartLoggingForUnauthenticatedRequests()
		{
			Assert("Precondition", !HttpContext.Current.Request.IsAuthenticated);
			AssertNull("Precondition", HttpContext.Current.Request.Cookies[UserActivityLogger.CookieKey]?.Value);
			AssertNull("Precondition", HttpContext.Current.Response.Cookies[UserActivityLogger.CookieKey]?.Value);

			UserActivityLogger.AddNewLog();
			UserActivityLogger.SetLogActionTime(UserActivityLog.PageInitCall);

			CombineAssertions(() =>
			{
				AssertEquals(0, UserActivityLogger.ActivityLogs.Count);
				AssertNull("Should not have set cookie", HttpContext.Current.Response.Cookies[UserActivityLogger.CookieKey].Value);
			});
		}

		public void TestDoesStartLoggingForUnauthenticatedRequestsWithCookie()
		{
			var loggerKeyCookie = new HttpCookie(UserActivityLogger.CookieKey, Guid.NewGuid().ToString());
			HttpContext.Current.Request.Cookies.Add(loggerKeyCookie);
			Assert("Precondition", !HttpContext.Current.Request.IsAuthenticated);
			Assert("Precondition", Guid.TryParse(HttpContext.Current.Request.Cookies[UserActivityLogger.CookieKey].Value, out _));

			UserActivityLogger.AddNewLog();
			UserActivityLogger.SetLogActionTime(UserActivityLog.PageInitCall);

			AssertEquals(1, UserActivityLogger.ActivityLogs.Count);
		}

		public void TestDoesStartLoggingForAuthenticatedRequests()
		{
			var identity = new FormsIdentity(new FormsAuthenticationTicket("SomeWebUser", false, 1));
			var principal = new GenericPrincipal(identity, Array.Empty<string>());
			HttpContext.Current.User = principal;
			Assert("Precondition", HttpContext.Current.Request.IsAuthenticated);
			AssertNull("Precondition", HttpContext.Current.Request.Cookies[UserActivityLogger.CookieKey]?.Value);
			AssertNull("Precondition", HttpContext.Current.Response.Cookies[UserActivityLogger.CookieKey]?.Value);

			UserActivityLogger.AddNewLog();
			UserActivityLogger.SetLogActionTime(UserActivityLog.PageInitCall);

			CombineAssertions(() =>
			{
				AssertEquals(1, UserActivityLogger.ActivityLogs.Count);
				Assert("Should have set cookie to a Guid", Guid.TryParse(HttpContext.Current.Response.Cookies[UserActivityLogger.CookieKey].Value, out _));
			});
		}

		public void TestLogsWithCookieKeyAlreadyInResponse()
		{
			var expectedKeyValue = Guid.NewGuid();
			var loggerKeyCookie = new HttpCookie(UserActivityLogger.CookieKey, expectedKeyValue.ToString());
			HttpContext.Current.Response.Cookies.Add(loggerKeyCookie);
			AssertEquals("Precondition", expectedKeyValue.ToString(), HttpContext.Current.Response.Cookies[UserActivityLogger.CookieKey]?.Value);

			UserActivityLogger.AddNewLog();
			UserActivityLogger.SetLogActionTime(UserActivityLog.PageInitCall);

			AssertEquals(1, UserActivityLogger.ActivityLogs.Count);
			AssertEquals(expectedKeyValue, Guid.Parse(HttpContext.Current.Response.Cookies[UserActivityLogger.CookieKey].Value));
		}

		#endregion
	}
}
