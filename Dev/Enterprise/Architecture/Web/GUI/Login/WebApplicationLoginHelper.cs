using System;
using System.Net;
using System.Web;
using System.Web.Security;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.Cookie;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

#if DEBUG
using System.Collections.Specialized;
#endif

namespace Enterprise.ZArchitecture.Web.GUI
{
	/// <summary>
	/// Class responsible for interaction of GUI during login
	/// Currently a Login page is in every project that requires login 
	/// and login forms are generally subclassed from a base form in its own project.
	/// As a result, it's impossible to have a generic login page for all solutions.
	/// This class solves the problem by moving all the logic into a special class
	/// </summary>
	public class WebApplicationLoginHelper
	{
		public WebApplicationLoginHelper(ZPage page) : this(page, true)
		{
		}

		public WebApplicationLoginHelper(ZPage page, bool isAutoMaticallyHookupOnLoad)
		{
			this.Page = page;

			if (isAutoMaticallyHookupOnLoad)
			{
				this.Page.Load += new EventHandler(Page_Load);
			}
		}

		public bool IsRedirectAfterSignIn
		{
			get { return fIsRedirectAfterSignIn; }
			set { fIsRedirectAfterSignIn = value; }
		}
		bool fIsRedirectAfterSignIn = true;

#if DEBUG

		#region Parameters for Test

		NameValueCollection ParamsForTest
		{
			get { return fParamsForTest ?? (fParamsForTest = new NameValueCollection()); }
		}
		NameValueCollection fParamsForTest;

		public void SetParamsValueForTest(string key, string value)
		{
			if (!Globals.IsTest)
			{
				throw new NotSupportedException("This SetParamsValueForTestForTest function is designed for using only in Tests");
			}

			ParamsForTest[key] = value;
		}

		#endregion

		#region Call Conters for Test

		public int OnLoginSucceed_CallCount
		{
			get { return onLoginSucceed_CallCount; }
		}
		internal int onLoginSucceed_CallCount;

		public int OnLoginFailure_CallCount
		{
			get { return onLoginFailure_CallCount; }
		}
		internal int onLoginFailure_CallCount;

		public int TryToRedirectToCustomLoginPage_CallCount
		{
			get { return tryToRedirectToCustomLoginPage_CallCount; }
		}
		int tryToRedirectToCustomLoginPage_CallCount;

		#endregion

#endif

		protected ZString GetParamValue(string name)
		{
			return
#if DEBUG
				Globals.IsTest ? new ZString(ParamsForTest[name]) :
#endif
				new ZString(HttpContext.Current.Request.Params[name]);
		}

		public void OnPageLoad()
		{
			var loginDataIsValid = SetUpLoginDataFromParams() || SetUpLoginDataFromCookie();

			if (Page != null && (!loginDataIsValid || (LoginMan != null && LoginMan.HasErrors)))
			{
				if (Page.AppInstance != null && Page.AppInstance.ApplicationCookie != null && Page.AppInstance.ApplicationCookie.CookieExist())
				{
					Page.AppInstance.ApplicationCookie.Remove();
				}
			}
			else if (ShouldSignInOnPageLoadWithNoCompanyCode || (LoginMan != null && !LoginMan.CompanyCode.IsEmpty))
			{
				if (SignInViaRouting())
				{
					return;
				}
			}

			LoadLoginHashCookie();
			TryToRedirectToCustomLoginPage();
		}

		protected virtual void TryToRedirectToCustomLoginPage()
		{
#if DEBUG
			if (Globals.IsTest)
			{
				tryToRedirectToCustomLoginPage_CallCount++;
			}
#endif
		}

		protected virtual bool ShouldSignInOnPageLoadWithNoCompanyCode => true;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "http form request field data value")]
		protected virtual bool SetUpLoginDataFromParams()
		{
			bool result = false;

			ZString companyCode = GetParamValue("CompanyCode");
			ZString userEmail = GetParamValue("UserEmail");
			ZString userPassword = GetParamValue("UserPassword");
			ZBool rememberMe = GetParamValue("RememberMe").ToLower() == "on";

			if (!(companyCode.IsEmpty && LoginMan != null && LoginMan.IsCompanyCodeRequired) &&
				!userEmail.IsEmpty && !userPassword.IsEmpty)
			{
				LoginMan.CompanyCode = companyCode;
				LoginMan.UserName = userEmail;
				LoginMan.Password = userPassword;
				LoginMan.RememberMe = rememberMe;

				result = true;
			}

			if (!GetParamValue("ClearSaved").IsEmpty)
			{
				if (Page.AppInstance.ApplicationCookie.CookieExist())
				{
					Page.AppInstance.ApplicationCookie.Remove();
				}
			}

			return result;
		}

		protected virtual bool SetUpLoginDataFromCookie()
		{
			bool result = false;
			if (Page.AppInstance != null)
			{
				ZApplicationCookie cookie = Page.AppInstance.ApplicationCookie;
				if (LoginMan != null && cookie != null && cookie.CookieExist() && LoginMan.CompanyCode.IsEmpty && LoginMan.UserName.IsEmpty)
				{
					LoginMan.CompanyCode = cookie.GetCompanyCode().Trim();
					LoginMan.UserName = cookie.GetUserEmail().Trim();
					LoginMan.Password = cookie.GetUserPassword();
					LoginMan.RememberMe = true;
					result = true;
				}
			}
			return result;
		}

		protected virtual void OnLoginSucceed()
		{
#if DEBUG
			if (Globals.IsTest)
			{
				onLoginSucceed_CallCount++;
			}
#endif
		}

		protected virtual void OnLoginFailure()
		{
#if DEBUG
			if (Globals.IsTest)
			{
				onLoginFailure_CallCount++;
			}
#endif
		}

		void Page_Load(object sender, EventArgs e)
		{
			OnPageLoad();
		}

		public bool SignIn()
		{
			if (Page != null && Page.SiteUser != null && LoginMan != null)
			{
				return SignIn(ExecuteSiteUserLoginAction);
			}

			return false;
		}

		public bool SignInViaRouting()
		{
			return SignInViaRoutingCore();
		}

		protected virtual bool SignInViaRoutingCore()
		{
			return SignIn();
		}

		protected virtual void ExecuteSiteUserLoginAction() => Page.SiteUser.Login(LoginMan.CompanyCode, LoginMan.UserName, LoginMan.Password, LoginMan.LoginHash);

		protected void SetRememberMe(bool addPasswordCookie = true)
		{
			if (LoginMan.RememberMe && Page.AppInstance != null && Page.AppInstance.ApplicationCookie != null)
			{
				Page.AppInstance.ApplicationCookie.WriteUser(LoginMan.CompanyCode, LoginMan.UserName, addPasswordCookie ? LoginMan.Password : string.Empty);
			}
		}

		protected bool SignIn(Action login)
		{
			login();

			if (Page.SiteUser.IsLoggedIn)
			{
				OnLoginSucceed();
				WriteLoginHashCookie();
				SetAuthCookie();
				SetRememberMe(false);
				RedirectAfterSignIn();

				return true;
			}
			else
			{
				if (Page.AppInstance != null && Page.AppInstance.ApplicationCookie != null)
				{
					Page.AppInstance.ApplicationCookie.Remove();
				}
				OnLoginFailure();
			}

			return false;
		}

		protected virtual void SetAuthCookie()
		{
			FormsAuthentication.SetAuthCookie(LoginMan.UserName, false);
		}

		protected virtual void RedirectAfterSignIn()
		{
			if (IsRedirectAfterSignIn)
			{
				RedirectToPage(DefaultUrl);
			}
		}

		protected internal ZPage Page;

		protected internal LoginManager LoginMan
		{
			get { return Page.DataSource as LoginManager; }
		}

		public void RedirectToPage(string url)
		{
			Page.Response.Redirect(url);
		}

		protected virtual string DefaultUrl
		{
			get
			{
				if ((HttpContext.Current.Request.Params["ReturnURL"] != null) &&
					(HttpContext.Current.Request.Params["ReturnURL"].ToLower().IndexOf(Page.AppInstance.LoginPage.ToLower()) == -1))
				{
					return Page.AppInstance.GetValidRedirectURL(WebUtility.UrlDecode(Page.Request.Params["ReturnURL"]));
				}
				else
				{
					return Page.AppInstance.DefaultPage;
				}
			}
		}

		#region Login Hash Cookie

		static LoginSuccessCookieHelper LoginSuccessCookieHelper => loginSuccessCookieHelper ?? (loginSuccessCookieHelper = new LoginSuccessCookieHelper());

		[ThreadStatic]
		static LoginSuccessCookieHelper loginSuccessCookieHelper;

		protected void WriteLoginHashCookie()
		{
			LoginSuccessCookieHelper.TemporaryRemoveOldCookie();
			if (LoginMan != null)
			{
				WriteLoginHashToCookie(LoginMan.CompanyCode, LoginMan.UserName);
			}
		}

		public void LoadLoginHashCookie()
		{
			if (LoginMan != null)
			{
				LoginMan.LoginHash = RetrieveLoginHashFromCookie(LoginMan.CompanyCode, LoginMan.UserName);
			}
		}

		public static void WriteLoginHashToCookie(string companyCode, string userName)
		{
			LoginSuccessCookieHelper.WriteLoginHashCookie(OrgContactLoginAttemptRecorder.CombineLoginNameAndCompanyCode(companyCode, userName));
		}

		public static byte[] RetrieveLoginHashFromCookie(string companyCode, string userName)
		{
			if (!string.IsNullOrEmpty(userName))
			{
				return LoginSuccessCookieHelper.LoadLoginHash(OrgContactLoginAttemptRecorder.CombineLoginNameAndCompanyCode(companyCode, userName));
			}
			return null;
		}

		#endregion
	}
}
