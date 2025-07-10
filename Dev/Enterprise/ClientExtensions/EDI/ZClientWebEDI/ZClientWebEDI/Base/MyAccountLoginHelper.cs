using System;
using System.Net;
using System.Web.Security;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.GUI.Login;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class MyAccountLoginHelper : OrgContactLoginHelper
	{
		public MyAccountLoginHelper(BasePage page, bool isAutoMaticallyHookupOnLoad = true)
			: base(page, isAutoMaticallyHookupOnLoad)
		{
		}

		protected new BasePage Page
		{
			get { return (BasePage)base.Page; }
		}

		protected override void TryToRedirectToCustomLoginPage()
		{
			if (Page.IsInLiteViewMode && Page.SiteUser != null && !Page.SiteUser.IsLoggedIn)
			{
				FormsAuthentication.SignOut();  //If session times out clear auth cookie before redirecting to hosting site login page to hide protected content
			}
		}

		protected override void OnLoginSucceed()
		{
			base.OnLoginSucceed();

			if (Page.SiteUser.IsSpecialUser)
			{
				Page.AppInstance.SetupSession(Page, EventArgs.Empty, false);
			}
		}

		public const string RefKey = "Ref";

		protected override bool ShouldSignInOnPageLoadWithNoCompanyCode => false;

		#region Redirect If Logged In

		protected override void RedirectAfterSignIn()
		{
			if (IsRedirectAfterSignIn)
			{
				Redirect();
			}
		}

		public void RedirectIfLoggedIn()
		{
			if (Page.SiteUser != null && Page.SiteUser.IsLoggedIn && IsAuthenticated)
			{
				Redirect();
			}
			else
			{
				ClearCookieIfNotLoggedIn();
			}
		}

		protected virtual void ClearCookieIfNotLoggedIn()
		{
		}

		protected virtual bool IsAuthenticated => Page.Request.IsAuthenticated;

		protected virtual void Redirect()
		{
			var url = Page.Request.QueryString["ReturnUrl"];
			if (url == null || MyAccountUtility.ContainsResizeInlineFramePage(url))
			{
				url = DefaultUrl;
			}
			else if (Page.IsSafeUrl(url))
			{
				url = Page.AppInstance.GetValidRedirectURL(WebUtility.UrlDecode(url));
			}

			SafeRedirect(url);
		}

		public override void RedirectViaLoginRouter(OrgContact contact)
		{
			var router = new MyAccountLoginRouter(new Uri(DefaultUrl, UriKind.RelativeOrAbsolute), contact, Page.AppInstance);
			var redirectUri = router.GetRoutingUrl();

			if (LoginMan.CompanyCode.IsEmpty)
			{
				LoginMan.CompanyCode = contact.OrganisationCode;
			}

			SetSwitchCompanyFunc(LoginMan.Password);
			SetRememberMe(false);
			RedirectToPage(redirectUri.IsAbsoluteUri ? redirectUri.AbsoluteUri : redirectUri.OriginalString);
		}

		protected override bool SetUpLoginDataFromCookie()
		{
			if (Page.AppInstance != null)
			{
				var cookie = Page.AppInstance.ApplicationCookie;
				if (LoginMan != null && cookie != null && cookie.CookieExist() && LoginMan.CompanyCode.IsEmpty && LoginMan.UserName.IsEmpty)
				{
					using (LoginMan.GetValidationSuspender())
					{
						LoginMan.CompanyCode = cookie.GetCompanyCode().Trim();
						LoginMan.UserName = cookie.GetUserEmail().Trim();
						LoginMan.RememberMe = true;
						var userPassword = cookie.GetUserPassword();
						if (!string.IsNullOrEmpty(userPassword))
						{
							LoginMan.Password = cookie.GetUserPassword();
							return true;
						}
					}
				}
			}
			return false;
		}

		protected void SafeRedirect(string url)
		{
			if (Page.IsSafeUrl(url))
			{
				Page.Response.Redirect(url);
			}
			else
			{
				Page.Response.Redirect(Page.AppInstance.HostingSiteHomePage);
			}
		}

		#endregion
	}
}
