using System;
using System.Net;
using System.Net.Http;
using System.Web;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[HttpContextEnabledTest]
	public class SlidingExpirationControllerTest : TestCaseWithFactory
	{
		public void TestUpdateCookieExpiry()
		{
			using (var request = GetNewRequest())
			{
				var now = DateTime.Now;
				var securityRightCookie = new HttpCookie(MyAccountLoginLiteHelper.SecurityRightsCookieName, "test");
				securityRightCookie.Expires = now.AddMinutes(1);
				var loggedinUserInfoCookie = new HttpCookie(MyAccountLoginLiteHelper.LoggedInUserInfoCookieName, "test user");
				loggedinUserInfoCookie.Expires = now.AddMinutes(1);
				HttpContext.Current.Request.Cookies.Add(securityRightCookie);
				HttpContext.Current.Request.Cookies.Add(loggedinUserInfoCookie);
				var sessionTimeOut = Global.SessionTimeout;
				using (var response = Execute(request))
				{
					AssertEquals(HttpStatusCode.OK, response.StatusCode);
					var securityRightCookieAfterRequest = HttpContext.Current.Request.Cookies[MyAccountLoginLiteHelper.SecurityRightsCookieName];
					AssertGreaterThanOrEqualTo(securityRightCookieAfterRequest.Expires, now.AddMinutes(sessionTimeOut));

					var loggedinUserInfoCookieAfterRequest = HttpContext.Current.Request.Cookies[MyAccountLoginLiteHelper.LoggedInUserInfoCookieName];
					AssertGreaterThanOrEqualTo(loggedinUserInfoCookieAfterRequest.Expires, now.AddMinutes(sessionTimeOut));
				}
			}
		}

		public void TestPreflightOptionsSuccess()
		{
			using (var request = OptionNewRequest())
			{
				request.Headers.Add("Origin", "https://myaccount.cargowise.com");

				using (var response = Execute(request))
				{
					AssertEquals(HttpStatusCode.OK, response.StatusCode);
				}
			}

			using (var request = OptionNewRequest())
			{
				request.Headers.Add("Origin", "HTTPS://MYACCOUNT.CARGOWISE.COM");

				using (var response = Execute(request))
				{
					AssertEquals(HttpStatusCode.OK, response.StatusCode);
				}
			}

			using (var request = OptionNewRequest())
			{
				request.Headers.Add("Origin", "https://myaccount.CARGOWISE.com");

				using (var response = Execute(request))
				{
					AssertEquals(HttpStatusCode.OK, response.StatusCode);
				}
			}
		}

		public void TestPreflightCrossOriginFailure()
		{
			using (var request = OptionNewRequest())
			{
				request.Headers.Add("Origin", "https://unauthorized-origin.com");

				using (var response = Execute(request))
				{
					AssertEquals(HttpStatusCode.Forbidden, response.StatusCode);
				}
			}
		}

		HttpRequestMessage GetNewRequest() => new HttpRequestMessage(HttpMethod.Get, "http://unit-testing/api/sso/sliding-expiration");
		HttpRequestMessage OptionNewRequest() => new HttpRequestMessage(HttpMethod.Options, "http://unit-testing/api/sso/sliding-expiration");
		HttpResponseMessage Execute(HttpRequestMessage request)
		{
			HttpResponseMessage response;
			using (var controller = new SlidingExpirationController() )
			{
				response = ControllerTestHelper.Execute(controller, request);
			}

			return response;
		}
	}
}
