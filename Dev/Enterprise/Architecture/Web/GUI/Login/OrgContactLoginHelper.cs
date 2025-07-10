using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using CargoWise.Common;
using CargoWise.Definitions.Authentication;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI.Login
{
	public abstract class OrgContactLoginHelper : WebApplicationLoginHelper
	{
		protected OrgContactLoginHelper(ZPage page) : base(page)
		{
		}

		protected OrgContactLoginHelper(ZPage page, bool isAutomaticallyHookupOnLoad) : base(page, isAutomaticallyHookupOnLoad)
		{
		}

		#region Login

		public bool IsSpecialLogin()
		{
			if (Page != null && CurrentUser != null && LoginMan != null)
			{
				return CurrentUser.IsSpecialLogin(LoginMan.UserName, LoginMan.Password);
			}

			return false;
		}

		public (LoginContactsResult, OrgContact[]) GetContactsFromCredentials()
		{
			if (Page != null && CurrentUser != null && LoginMan != null)
			{
				var (result, loginContactsContactable) = CurrentUser.GetLoginContacts(LoginMan.CompanyCode, LoginMan.UserName, LoginMan.Password, LoginMan.LoginHash, false);
				var loginContacts = loginContactsContactable.Cast<OrgContact>().ToArray();
				if (!loginContacts.Any())
				{
					OnLoginFailure();
				}

				return (result, loginContacts);
			}

			return (LoginContactsResult.Failure, Array.Empty<OrgContact>());
		}

		public OrgContact GetContactFromCredentials()
		{
			if (Page != null && CurrentUser != null && LoginMan != null)
			{
				var loginContact = CurrentUser.GetLoginContact(LoginMan.CompanyCode, LoginMan.UserName, LoginMan.Password, LoginMan.LoginHash) as OrgContact;
				if (loginContact == null)
				{
					OnLoginFailure();
				}

				return loginContact;
			}

			return null;
		}

		protected override bool SignInViaRoutingCore()
		{
			if (IsSpecialLogin())
			{
				return SignIn();
			}

			var contact = GetContactFromCredentials();

			if (contact != null)
			{
				WriteLoginHashCookie();
				RedirectViaLoginRouter(contact);
				return true;
			}

			return false;
		}

		protected override void OnLoginSucceed()
		{
			base.OnLoginSucceed();

			SetSwitchCompanyFunc(LoginMan.Password);
		}

		protected override void OnLoginFailure()
		{
			base.OnLoginFailure();

			ClearSwitchCompanyFunc();
		}

		public List<OrgContact> LoginContactCandidates { get; } = new List<OrgContact>();

		public void RedirectToChooseCompany(Uri chooseCompanyBasePageUrl, ZGlobal appInstance)
		{
			appInstance.SetupSession(null, EventArgs.Empty, false);
			var contacts = LoginContactCandidates;

			if (contacts.Count > 1)
			{
				ITokenizedAccessControl accessControl = new TokenizedAccessControl();
				var tokenInfo = new AccessTokenInfo(LoginMan.RememberMe + ":" + string.Join(",", contacts.Select(x => x.PK)), contacts[0].PK.ToGuid(), OrgContactSchema.Constants.Prefix);
				var token = accessControl.CreateLimitedToken(AccessTokenTypes.LoginRouterMultiContactIdentity, tokenInfo, TimeSpan.FromMinutes(15), 1);

				var passwordSecureQueryString = new SecureQueryString
				{
					{ LoginRouter.IdentityTokenQueryStringKey, token }
				};

				passwordSecureQueryString.Add(LoginRouter.OriginalUrlQueryStringKey, DefaultUrl);

				var uriDeconstructor = new UriDeconstructor(chooseCompanyBasePageUrl);
				var passwordValues = HttpUtility.ParseQueryString(uriDeconstructor.Query);
				var encodedQueryString = WebUtility.UrlEncode(passwordSecureQueryString.ToString());
				passwordValues.Add(SecureQueryString.QueryStringKey, encodedQueryString);

				var redirectUri = uriDeconstructor.GetUriWithNewQuery(passwordValues.ToString());

				SetSwitchCompanyFunc(LoginMan.Password);
				SetRememberMe(false);
				RedirectToPage(redirectUri.IsAbsoluteUri ? redirectUri.AbsoluteUri : redirectUri.OriginalString);
			}
		}

		protected override void ExecuteSiteUserLoginAction()
		{
			if (!string.IsNullOrEmpty(LoginMan.Password))
			{
				base.ExecuteSiteUserLoginAction();
			}
			else
			{
				((Action<WebUser, LoginManager>)HttpContext.Current.Session[SwitchCompanyFuncSessionKey])?.Invoke(Page.SiteUser, LoginMan);
			}
		}

		public const string SwitchCompanyFuncSessionKey = "SwitchCompanyFunc";

		protected void SetSwitchCompanyFunc(string switchKey)
		{
			if (!string.IsNullOrEmpty(switchKey))
			{
				Action<WebUser, LoginManager> switchCompanyFunc = (siteUser, loginMan) => siteUser.Login(loginMan.CompanyCode, loginMan.UserName, switchKey, loginMan.LoginHash, false);
				HttpContext.Current.Session[SwitchCompanyFuncSessionKey] = switchCompanyFunc;
			}
		}

		void ClearSwitchCompanyFunc()
		{
			HttpContext.Current.Session[SwitchCompanyFuncSessionKey] = null;
		}

		OrgContactWebUser CurrentUser
		{
			get { return Page.SiteUser as OrgContactWebUser; }
		}

		public abstract void RedirectViaLoginRouter(OrgContact contact);

		#endregion
	}
}
