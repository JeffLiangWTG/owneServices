using System;
using System.Collections.Generic;
using System.Net;
using CargoWise.Common;
using CargoWise.Definitions.Authentication;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZClientWebCargoWiseEDI.Gateway;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public partial class GatewayLogin : BasePage
	{
		protected override bool PageRequiresLogin(Uri url) => true;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings")]
		protected void Page_Load(object sender, EventArgs e)
		{
			var redirectUriString = Request.QueryString["redirect_uri"];
			var state = Request.QueryString["state"];

			if (!Uri.TryCreate(redirectUriString, UriKind.Absolute, out var redirectUri))
			{
				RedirectToErrorPage("Invalid redirect uri", "Your redirect uri is invalid. Try logging in again through GLOW");
				return;
			}

			if (string.IsNullOrEmpty(state))
			{
				RedirectToErrorPage("Invalid state", "Your state is invalid. Try logging in again through GLOW");
				return;
			}

			if (!SiteUser.IsLoggedIn || SiteUser.LoggedInOrgContact == null)
			{
				RedirectToErrorPage("User not logged in", "You are not logged in. Please try logging in again.");
				return;
			}

			var tokenInfo = new AccessTokenInfo(string.Empty, SiteUser.LoggedInOrgContact.PK.ToGuid(), OrgContactSchema.Constants.Prefix);
			var token = new TokenizedAccessControl().CreateLimitedToken(AccessTokenTypes.MyAccountGlowIdentity, tokenInfo, TimeSpan.FromMinutes(5), 1, TokenizedAccessControlExtensions.DefaultCharacterSet, 8);

			var redirectLocation = MyAccountGatewayLoginController.AppendKeyValuePairsToUri(redirectUri, new[]
			{
				new KeyValuePair<string, string>("code", token),
				new KeyValuePair<string, string>("state", state)
			});

			Response.Redirect(redirectLocation.IsAbsoluteUri ? redirectLocation.AbsoluteUri : redirectLocation.OriginalString);
		}

		void RedirectToErrorPage(string errorTitle, string errorMessage)
		{
			var queryString = new SecureQueryString
			{
				["title"] = errorTitle,
				["message"] = errorMessage
			};

			queryString.ExpireTime = TimeSpan.FromMinutes(10);
			Response.Redirect(AppInstance.ErrorPage + "?data=" + WebUtility.UrlEncode(queryString.ToString()));
		}
	}
}
