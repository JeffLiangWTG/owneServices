using System;
using CargoWise.Common;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GlowInterop;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public partial class GlowPortalAutoLogin : BasePage
	{
		protected override bool PageRequiresLogin(Uri url) => false;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings")]
		protected void Page_Load(object sender, EventArgs e)
		{
			SecureQueryString secureQueryString;
			string encryptedData = Request.QueryString[SecureQueryString.QueryStringKey];
			try
			{
				secureQueryString = new SecureQueryString(encryptedData);
			}
			catch (QueryStringException)
			{
				secureQueryString = new SecureQueryString();
			}

			var glowUrl = secureQueryString["glowUrl"];
			var token = secureQueryString["token"];

			bool isRedirected = false;
			if (!string.IsNullOrEmpty(glowUrl) && !string.IsNullOrEmpty(token))
			{
				ITokenizedAccessControl accessControl = new TokenizedAccessControl();
				if (accessControl.TryConsume(token, AccessTokenTypes.GlowAutoLogin, out var accessToken))
				{
					var factory = new BusinessObjectFactory();
					var contact = factory.Load<OrgContact>(accessToken.ParentId);

					if (contact != null)
					{
						AppInstance?.SetupSession(null, EventArgs.Empty, false);

						EdiCustomerUserAccount userAccount = null;
						if (ZGuid.TryParse(accessToken.Scope, out var userAccountPK) && !userAccountPK.IsEmpty)
						{
							userAccount = factory.Load<EdiCustomerUserAccount>(userAccountPK);
						}

						var previousLoggedInContactPK = SignOutForAutoLoginIfRequired();
						var redirectUrl = AutoLoginHelper.UserRoutingLogin(glowUrl, userAccount, AppInstance, contact, previousLoggedInContactPK);
						Response.Redirect(redirectUrl.IsAbsoluteUri ? redirectUrl.AbsoluteUri : redirectUrl.OriginalString);
						isRedirected = true;
					}
				}
			}

			if (!isRedirected)
			{
				Response.Redirect(EDIDataRegistry.Instance.MyAccountIndexPage.Value);
			}
		}
	}
}
