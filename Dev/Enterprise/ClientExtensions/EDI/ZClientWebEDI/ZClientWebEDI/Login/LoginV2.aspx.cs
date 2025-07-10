using System;
using System.Collections.Generic;
using System.Threading;
using System.Web;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZClientWebCargoWiseEDI.OIDC;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public partial class LoginV2 : LoginLite
	{
		public static class PageStatusList
		{
			public const string UserNameAndCompanyCode = "UserNameAndCompanyCode";
			public const string Password = "PasswordLogin";
		}

		public string CompanyCodeCache { get => ViewState["CompanyCode"] as string; protected set => ViewState["CompanyCode"] = value; }

		public string EmailAddressCache { get => ViewState["EmailAddress"] as string; protected set => ViewState["EmailAddress"] = value; }

		public string PageStatus { get => (ViewState["PageStatus"] as string) ?? PageStatusList.UserNameAndCompanyCode; protected set => ViewState["PageStatus"] = value; }

		protected override bool PageRequiresLogin(Uri url) => false;

		protected override string StyleSheetFileName => this.AppInstance.LoginPageStyleSheetPath;

		protected override void Page_PreInit(object sender, EventArgs e)
		{
			if (!OIDCLoginHelper.IsOIDCReady())
			{
				var uri = "~/Login/Login.aspx";
				if (Request.QueryString.Count > 0)
				{
					uri += $"?{Request.QueryString}";
				}

				Response.Redirect(uri);
				return;
			}
		}

		protected override BusinessObject GetNewDataSource()
		{
			var loginMan = new LoginManager { IsCompanyCodeRequired = false };
			PresetUserNameAndCompany(loginMan);
			using (loginMan.GetValidationSuspender())
			{
				switch (PageStatus)
				{
					case (PageStatusList.UserNameAndCompanyCode):
						if (LoginNameTextBox != null && !string.IsNullOrEmpty(LoginNameTextBox.Text))
						{
							loginMan.UserName = LoginNameTextBox.Text;
						}
						if (CompanyCodeTextBox != null && !string.IsNullOrEmpty(CompanyCodeTextBox.Text))
						{
							loginMan.CompanyCode = CompanyCodeTextBox.Text;
						}
						loginMan.Password = Guid.NewGuid().ToString(); //A fake password is set here to suppress LoginMan.Password's validation when jumping to password
						break;
					case (PageStatusList.Password):
						loginMan.UserName = EmailAddressCache;
						loginMan.CompanyCode = CompanyCodeCache;
						loginMan.Password = PasswordTextBox?.Text ?? string.Empty;
						break;
				}
			}
			return loginMan;
		}

		bool RedirectToAuthorizeUri(UserCredentialData userCredential)
		{
			try
			{
				AppInstance.ApplicationCookie.WriteUser(userCredential.OrganisationCode ?? string.Empty, userCredential.EmailUsername, "");
				var nonce = Guid.NewGuid().ToString();
				var (codeVerifier, codeChallenge) = OIDCLoginHelper.GenerateOIDCProofKeyPair();
				var authorityUri = OIDCLoginHelper.GetOIDCCodeUrl(
					new Dictionary<string, string>()
					{
						{ OIDCLoginHelper.Constants.NonceKey, nonce },
						{ OIDCLoginHelper.Constants.CodeChallengeKey, codeChallenge },
						{ OIDCLoginHelper.Constants.CodeChallengeMethodKey, "S256" },
						{ OIDCLoginHelper.Constants.LoginHintKey, userCredential.EmailUsername },
					},
					new NLogWrapper(GetType()),
					CancellationToken.None);
				var cookie = new HttpCookie(OIDCLoginHelper.Constants.OIDCCookieName, new OIDCAuthUserCookieData(codeVerifier, nonce, userCredential).ToString());
				cookie.Expires = DateTimeOffset.Now.AddMinutes(10).UtcDateTime;
				cookie.Path = HttpContext.Current.Request.ApplicationPath;
				cookie.Secure = true;
				Response.AppendCookie(cookie);
				Response.Redirect(authorityUri.AbsoluteUri);
				return true;
			}
			catch (Exception ex)
			{
				ErrorReporter.ReportOnce("An error occurred when generating authorize code uri", ex);
				return false;
			}
		}

		protected override void SigninBtn_Click(object sender, EventArgs e)
		{
			if (LoginMan.HasErrors)
			{
				return;
			}

			switch (PageStatus)
			{
				case PageStatusList.UserNameAndCompanyCode:
					var returnUrl = Request.QueryString["ReturnUrl"];
					var userCredentialData = new UserCredentialData(LoginNameTextBox.Text, CompanyCodeTextBox.Text, returnUrl);
					if (!OIDCLoginHelper.ShouldRedirectToIDP(Factory, userCredentialData.EmailUsername, userCredentialData.OrganisationCode))
					{
						PageStatus = PageStatusList.Password;
						EmailAddressCache = LoginNameTextBox.Text;
						CompanyCodeCache = CompanyCodeTextBox.Text;
						LoginNameTextLabel.Text = LoginNameTextBox.Text;
						CompanyCodeTextLabel.Text = CompanyCodeTextBox.Text.ToUpper();
					}
					else if (!RedirectToAuthorizeUri(userCredentialData))
					{
						Message.Text = OIDCLoginHelper.ErrorList.FailedToRedirectToIDP;
						return;
					}
					break;
				case PageStatusList.Password:
					if (OIDCLoginHelper.ShouldRedirectToIDP(Factory, LoginMan.UserName, LoginMan.CompanyCode))
					{
						Message.Text = OIDCLoginHelper.ErrorList.UserNotRedirectedToIDP;
						PageStatus = PageStatusList.UserNameAndCompanyCode;
						ErrorReporter.ReportOnce("Users matching OIDC cannot login using passwords, should redirect to IDP", FormattableString.Invariant($"Login Contact UserName: {LoginMan.UserName}, Login Contact CompanyCode: {LoginMan.CompanyCode}"));
						return;
					}
					base.SigninBtn_Click(sender, e);
					break;
			}
		}
	}
}
