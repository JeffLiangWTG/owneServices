using System;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Security;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class MyAccountLoginLiteHelper : MyAccountLoginHelper
	{
		public MyAccountLoginLiteHelper(BasePage page, bool isAutoMaticallyHookupOnLoad = true)
			: base(page, isAutoMaticallyHookupOnLoad)
		{
		}

		protected override void TryToRedirectToCustomLoginPage()
		{
			//Do nothing because this page is the login lite page
		}

		protected override void SetAuthCookie()
		{
			base.SetAuthCookie();
			CreateLiteViewModeCookies(Page);
		}

		#region Redirect If Logged In

		protected override void Redirect()
		{
			SafeRedirect(DefaultUrl);
		}

		protected override void ClearCookieIfNotLoggedIn()
		{
			ExpireLiteViewModeCookies(Page.Request, Page.Response);
		}
		
		protected override string DefaultUrl => Page.GetLiteViewModeReturnUrl(WebUtility.UrlDecode(Page.Request["returnUrl"]));

		#endregion

		#region Cookies

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1060:Do not use System.DateTime.Now Rule", Justification = "ZDateTime.Now is branch local time. In web env for setting cookie expiry it needs to be aligned with form authentication expiry which is from the web server time.")]
		internal static void CreateLiteViewModeCookies(BasePage page)
		{
			HttpResponse response;
			try
			{
				response = page.Response;
			}
			catch (HttpException)
			{
				response = HttpContext.Current.Response;
			}

			var cookieExpires = DateTime.Now.AddMinutes(HttpContext.Current.Session.Timeout);

			var loggedinUserCookie = new HttpCookie("EDIPROD_LOGGED_IN_USER");
			loggedinUserCookie["ContactPk"] = page.SiteUser.LoggedInUserPK.ToString();
			loggedinUserCookie["IsSuperUser"] = page.SiteUser.IsSuperUser.ToString();

			var siteUser = page.SiteUser;
			var contact = siteUser.LoggedInUser as EDIOrgContact;
			if (contact != null)
			{
				loggedinUserCookie["ContactName"] = WebUtility.UrlEncode(contact.ContactNameWithoutNumberSuffix);
				loggedinUserCookie["CompanyName"] = WebUtility.UrlEncode(contact.CompanyName);
			}
			else
			{
				loggedinUserCookie["ContactName"] = WebUtility.UrlEncode(siteUser.LoggedInUserName);
				loggedinUserCookie["CompanyName"] = WebUtility.UrlEncode(siteUser.AffiliationName);
			}

			var loggedinUserInfoCookie = new HttpCookie(LoggedInUserInfoCookieName);
			loggedinUserInfoCookie.Value = HttpServerUtility.UrlTokenEncode(MachineKey.Protect(Encoding.ASCII.GetBytes(loggedinUserCookie.Value)));
			loggedinUserInfoCookie.Expires = cookieExpires;
			loggedinUserInfoCookie.Domain = FormsAuthentication.CookieDomain;
			response.Cookies.Add(loggedinUserInfoCookie);

			var securityRightsCookie = new HttpCookie(SecurityRightsCookieName);
			var grantedSecurityRights = "";
			foreach (WebSecurityRight securityRight in EDIWebSecurityRightsList.New())
			{
				if (page.SiteUser.AreSecurityRightsGranted(securityRight))
				{
					grantedSecurityRights += securityRight.Code + ",";
				}
			}
			grantedSecurityRights = grantedSecurityRights.TrimEnd(',');
			securityRightsCookie.Value = HttpServerUtility.UrlTokenEncode(MachineKey.Protect(Encoding.ASCII.GetBytes(grantedSecurityRights)));
			securityRightsCookie.Expires = cookieExpires;
			securityRightsCookie.Domain = FormsAuthentication.CookieDomain;
			response.Cookies.Add(securityRightsCookie);
		}

		internal static void ExpireLiteViewModeCookies(HttpRequest request, HttpResponse response)
		{
			var cookieExpires = ZDateTime.Now.AddDays(-1).ToDateTime();

			ExpireCookie(LoggedInUserInfoCookieName, request,response, cookieExpires);
			ExpireCookie(SecurityRightsCookieName, request, response,cookieExpires);
		}

		static void ExpireCookie(string cookieName, HttpRequest request, HttpResponse response, DateTime expires)
		{
			if (request.Cookies[cookieName] != null)
			{
				var expiredCookie = new HttpCookie(cookieName);
				expiredCookie.Expires = expires;
				if (request.Url.Host.ToLower().StartsWith("myaccount-portal"))
				{
					expiredCookie.Domain = FormsAuthentication.CookieDomain;
				}
				response.Cookies.Add(expiredCookie);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1060:Do not use System.DateTime.Now Rule", Justification = "ZDateTime.Now is branch local time. In web env for setting cookie expiry it needs to be aligned with form authentication expiry which is from the web server time.")]
		public static void RefreshCookiesExpire()
		{
			var securityRightCookie = HttpContext.Current.Request.Cookies[SecurityRightsCookieName];
			var userInfoCookie = HttpContext.Current.Request.Cookies[LoggedInUserInfoCookieName];
			var securityRightCookieInResponse = HttpContext.Current.Response.Cookies[SecurityRightsCookieName];
			var userInfoCookieInResponse = HttpContext.Current.Response.Cookies[LoggedInUserInfoCookieName];

			var newExpires = DateTime.Now.AddMinutes(Global.SessionTimeout);
			if (securityRightCookie != null && (securityRightCookieInResponse == null || securityRightCookieInResponse.Value == null))
			{
				var newSecurityRightCookie = new HttpCookie(securityRightCookie.Name, securityRightCookie.Value)
				{
					Expires = newExpires,
					Domain = FormsAuthentication.CookieDomain
				};
				HttpContext.Current.Response.Cookies.Set(newSecurityRightCookie);
			}

			if (userInfoCookie != null && (userInfoCookieInResponse == null || userInfoCookieInResponse.Value == null))
			{
				var newUserInfoCookie = new HttpCookie(userInfoCookie.Name, userInfoCookie.Value)
				{
					Expires = newExpires,
					Domain = FormsAuthentication.CookieDomain
				};
				HttpContext.Current.Response.Cookies.Set(newUserInfoCookie);
			}
		}

		public const string LoggedInUserInfoCookieName = "EDIPROD_LOGGED_IN_USER_INFO";
		public const string SecurityRightsCookieName = "SECURITYRIGHTS";

		#endregion
	}
}
