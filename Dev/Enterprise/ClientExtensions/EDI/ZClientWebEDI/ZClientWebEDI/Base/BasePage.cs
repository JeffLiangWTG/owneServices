using System;
using System.Web;
using System.Web.Security;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class BasePage : ZAjaxPage
	{
		public void Logout()
		{
			LoginStatus loginStatus = GetLoginStatus();
			if (loginStatus != null)
			{
				loginStatus.LogOff();
			}
		}

		#region Page setup

		public new Global AppInstance
		{
			get { return base.AppInstance as Global; }
		}

		protected virtual bool ShowNavigationButtons
		{
			get { return false; }
		}

		protected override bool ShowFooter
		{
			get { return false; }
		}

		protected override bool ShowLogOffLinkButton
		{
			get { return false; }
		}

		protected override bool ShowChangePasswordLinkButton
		{
			get { return false; }
		}

		protected override bool ShowLoginStatus
		{
			get { return !IsInLiteViewMode; }
		}

		#endregion

		protected override void InitializeCulture()
		{
			ObjectFactory.Get<IResourceStrings>().CurrentLanguage =	Res.DefaultLanguage;
			LanguageSet = true;
			base.InitializeCulture();
		}

		public bool IsInLiteViewMode
		{
			get { return !string.IsNullOrWhiteSpace(AppInstance.HostingSiteRoot); }
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			Response.Cache.SetExpires(DateTime.Now);

			if (!Request.IsAuthenticated && ShouldSetupSessionOnLoad && Env.CurrentUser == null)
			{
				AppInstance.SetupSession(null, EventArgs.Empty, false);
			}
		}

		protected virtual bool ShouldSetupSessionOnLoad => false;

		protected override void OnPreInit(EventArgs e)
		{
			var master = this.Master;
			base.OnPreInit(e);
		}

		protected override bool PageRequiresLogin(Uri url)
		{
			return true;
		}

		public new OrgContactWebUser SiteUser
		{
			get { return (OrgContactWebUser)base.SiteUser; }
		}

		protected SecureQueryString PageSecureQueryString
		{
			get
			{
				if (pageSecureQueryString == null)
				{
					var encryptedData = Request.QueryString["data"];
					try
					{
						pageSecureQueryString = new SecureQueryString(encryptedData);
					}
					catch (QueryStringException)
					{
						pageSecureQueryString = new SecureQueryString();
					}
				}
				return pageSecureQueryString;
			}
		}
		SecureQueryString pageSecureQueryString;

		protected void SignOutAndClearCookies()
		{
			FormsAuthentication.SignOut();
			if (SiteUser != null && SiteUser.IsLoggedIn)
			{
				SiteUser.Logout();
			}

			if (HttpContext.Current.Request.QueryString["ClearSaved"] != null)
			{
				if (AppInstance.ApplicationCookie.CookieExist())
				{
					AppInstance.ApplicationCookie.Remove();
				}

				Session.Abandon();
			}
		}

		protected ZGuid SignOutForAutoLoginIfRequired()
		{
			var loggedInOrgContactPK = ZGuid.Empty;
			if (SiteUser != null && SiteUser.IsLoggedIn)
			{
				loggedInOrgContactPK = SiteUser.LoggedInOrgContact?.PK ?? ZGuid.Empty;
				SignOutAndClearCookies();
				if (IsInLiteViewMode)
				{
					MyAccountLoginLiteHelper.ExpireLiteViewModeCookies(Request, Response);
				}
			}
			return loggedInOrgContactPK;
		}

		public static string SessionExpiredMessage => Res.GetString("1ea1b5b0-05d2-4c98-9676-f0f62a02d468", "Your session has expired. Please log in again.");
		public static string PageExpiredMessage => Res.GetString("6f0b7b14-0137-4609-b4d8-0145134d9cd9", "The page has expired. Please log in again.");
	}
}
