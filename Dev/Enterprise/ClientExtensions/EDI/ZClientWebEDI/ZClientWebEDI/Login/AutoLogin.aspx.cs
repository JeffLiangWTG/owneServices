using System;
using CargoWise.Common;
using CargoWise.Definitions.Authentication;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZClientWebCargoWiseEDI.WebApi.Models.PortalAuth;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public partial class AutoLogin : BasePage
	{
		protected override bool PageRequiresLogin(Uri url)
		{
			return false;
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			bool isRedirected;

			var isTokenAccess = Request.QueryString.AllKeys.Length == 1 && Request.QueryString.AllKeys[0] == "token";
			if (isTokenAccess)
			{
				isRedirected = TokenLogin();
			}
			else
			{
				isRedirected = SecureQueryStringLogin();
			}

			if (!isRedirected)
			{
				Response.Redirect(EDIDataRegistry.Instance.MyAccountIndexPage.Value);
			}
		}

		bool TokenLogin()
		{
			bool isRedirected = false;

			string tokenQueryString = Request.QueryString["token"];
			ITokenizedAccessControl accesscontrol = new TokenizedAccessControl();
			var result = accesscontrol.TryConsume(tokenQueryString, AccessTokenTypes.MyAccountAutoLogin, out AccessTokenInfo info);

			if (result && !string.IsNullOrEmpty(info.Scope))
			{
				AutoLoginTokenScope scope = null;
				try
				{
					scope = AutoLoginHelper.DeserializeFromXml(info.Scope);
				}
				catch (InvalidOperationException ex)
				{
					scope = new AutoLoginTokenScope();
					ErrorReporter.ReportOnce($"Deserialization of XML failed during AutoLogin, The original xml is: {info.Scope}", ex);
				}

				var originalRequestUrl = scope.ReturnUrl;
				if (string.IsNullOrEmpty(originalRequestUrl))
				{
					originalRequestUrl = EDIDataRegistry.Instance.MyAccountIndexPage.Value;
				}

				if (!string.IsNullOrEmpty(scope.DatabaseNumber))
				{
					Session["DatabaseNumber"] = scope.DatabaseNumber;
				}

				if (SiteUser != null)
				{
					var previousLoggedInContactPK = SignOutForAutoLoginIfRequired();
					EdiCustomerUserAccount userAccount = null;
					OrgContact contact = null;

					if (info.ParentTableCode == OrgContactSchema.Constants.Prefix)
					{
						contact = Factory.Load<OrgContact>(info.ParentId);
					}
					else if (info.ParentTableCode == EdiCustomerUserAccountSchema.Constants.Prefix)
					{
						userAccount = Factory.Load<EdiCustomerUserAccount>(info.ParentId);
						contact = userAccount.WebAccessContact;
					}

					var redirectUrl = AutoLoginHelper.UserRoutingLogin(originalRequestUrl, userAccount, AppInstance, contact, previousLoggedInContactPK);
					Response.Redirect(redirectUrl.IsAbsoluteUri ? redirectUrl.AbsoluteUri : redirectUrl.OriginalString);
					isRedirected = true;
				}
			}

			return isRedirected;
		}

		bool SecureQueryStringLogin()
		{
			bool isRedirected = false;

			EdiCustomerUserAccount userAccount = null;
			OrgContact contact = null;

			SecureQueryString queryString = null;
			try
			{
				var queryStringData = Request.QueryString[SecureQueryString.QueryStringKey];
				queryString = new SecureQueryString(queryStringData);
			}
			catch (QueryStringException) { }

			if (queryString == null)
			{
				return false;
			}

			var originalRequestUrl = queryString["ReturnUrl"];
			if (string.IsNullOrEmpty(originalRequestUrl))
			{
				originalRequestUrl = EDIDataRegistry.Instance.MyAccountIndexPage.Value;
			}

			if (SiteUser != null)
			{
				var previousLoggedInContactPK = SignOutForAutoLoginIfRequired();
				var orgCode = queryString[OrgHeaderSchema.Constants.OH_Code];

				if (ZGuid.TryParse(queryString[EdiCustomerUserAccountSchema.Constants.Prefix], out var userAccountPK) && !userAccountPK.IsEmpty)
				{
					userAccount = Factory.Load<EdiCustomerUserAccount>(userAccountPK);
					contact = userAccount?.WebAccessContact;
				}
				else if (ZGuid.TryParse(queryString[OrgContactSchema.Constants.Prefix], out var contactPK))
				{
					if (contactPK == ZGuid.Missing)
					{
						SuperUserLogin(orgCode);
						return false;
					}

					if (!contactPK.IsEmpty)
					{
						contact = Factory.Load<OrgContact>(contactPK);
					}
				}

				var redirectUrl = AutoLoginHelper.UserRoutingLogin(originalRequestUrl, userAccount, AppInstance, contact, previousLoggedInContactPK);
				Response.Redirect(redirectUrl.IsAbsoluteUri ? redirectUrl.AbsoluteUri : redirectUrl.OriginalString);
				isRedirected = true;
			}

			return isRedirected;
		}

		void SuperUserLogin(string orgCode)
		{
			SiteUser.Login(orgCode, Enterprise.ZArchitecture.Environment.User.WebUserName, Enterprise.ZArchitecture.Environment.User.WebTransientPassword);
			if (SiteUser.IsLoggedIn && !Request.IsAuthenticated)
			{
				AutoLoginHelper.SetAuthCookie(this, SiteUser);
			}
		}
	}
}
