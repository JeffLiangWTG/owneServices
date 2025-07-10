using System;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.GUI.Login;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class RoutingEnabledPage : BasePage
	{
		protected void RedirectViaLoginRouter(string originalUrl, OrgContact contact)
		{
			var originalUri = string.IsNullOrEmpty(originalUrl) ? null : new Uri(originalUrl, UriKind.RelativeOrAbsolute);
			RedirectViaLoginRouter(originalUri, contact);
		}

		protected void RedirectViaLoginRouter(Uri originalUrl, OrgContact contact)
		{
			var router = new MyAccountLoginRouter(originalUrl, contact, AppInstance);
			var redirectUrl = router.GetRoutingUrl();
			Response.Redirect(redirectUrl.IsAbsoluteUri ? redirectUrl.AbsoluteUri : redirectUrl.OriginalString);
		}

		protected void RedirectViaLoginRouter(string originalUrl, string token)
		{
			var originalUri = string.IsNullOrEmpty(originalUrl) ? null : new Uri(originalUrl, UriKind.RelativeOrAbsolute);
			RedirectViaLoginRouter(originalUri, token);
		}

		protected void RedirectViaLoginRouter(Uri originalUrl, string token)
		{
			var router = new MyAccountLoginRouter(originalUrl, token);
			var redirectUrl = router.GetRoutingUrl();
			Response.Redirect(redirectUrl.IsAbsoluteUri ? redirectUrl.AbsoluteUri : redirectUrl.OriginalString);
		}

		protected void RedirectViaLoginRouter(string originalUrl, EdiCustomerUserAccount userAccount)
		{
			var originalUri = string.IsNullOrEmpty(originalUrl) ? null : new Uri(originalUrl, UriKind.RelativeOrAbsolute);
			RedirectViaLoginRouter(originalUri, userAccount);
		}

		protected void RedirectViaLoginRouter(Uri originalUrl, EdiCustomerUserAccount userAccount)
		{
			var router = new MyAccountLoginRouter(originalUrl, userAccount, AppInstance);
			var redirectUrl = router.GetRoutingUrl();
			Response.Redirect(redirectUrl.IsAbsoluteUri ? redirectUrl.AbsoluteUri : redirectUrl.OriginalString);
		}

		protected override bool ShouldSetupSessionOnLoad => IdentityManager.IsValidID();

		protected virtual MyAccountLoginRouterIdentityManager IdentityManager
		{
			get
			{
				if (identityManager == null)
				{
					identityManager = GetNewIdentityManager(new BusinessObjectFactory());
					identityManager.PopulatePropertiesFromToken(Token);
				}

				return identityManager;
			}
		}

		protected virtual MyAccountLoginRouterIdentityManager GetNewIdentityManager(BusinessObjectFactory factory)
		{
			return new MyAccountLoginRouterIdentityManager(factory);
		}

		public string Token
		{
			get
			{
				if (string.IsNullOrEmpty(token))
				{
					token = LoginRouter.GetIdentityTokenFromRequest(Request);
				}

				return token;
			}
		}

		string token;

		MyAccountLoginRouterIdentityManager identityManager;
	}
}
