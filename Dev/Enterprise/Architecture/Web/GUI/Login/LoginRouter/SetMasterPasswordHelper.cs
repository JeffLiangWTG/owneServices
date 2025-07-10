using System;
using System.Net;
using System.Web;
using CargoWise.Common;
using CargoWise.Definitions.Authentication;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.ZArchitecture.Web.GUI.Login
{
	public static class SetMasterPasswordHelper
	{
		public static Uri GenerateSetMasterPasswordUrl(OrgContact contact, Uri passwordPageBaseUrl, Uri originalRequestUrl, string identityToken)
		{
			ITokenizedAccessControl accessControl = new TokenizedAccessControl();
			var originalRequestUrlString = originalRequestUrl.IsAbsoluteUri ? originalRequestUrl.AbsoluteUri : originalRequestUrl.OriginalString;
			var token = accessControl.CreateLimitedToken(AccessTokenTypes.SetMasterPassword, new AccessTokenInfo(originalRequestUrlString, contact.PK.ToGuid(), OrgContactSchema.Constants.Prefix), TimeSpan.FromHours(24), 1);

			var passwordSecureQueryString = new SecureQueryString
			{
				{ WebUserAdminManager.SetMasterPasswordKey, token },
				{ LoginRouter.IdentityTokenQueryStringKey, identityToken }
			};

			var uriDeconstructor = new UriDeconstructor(passwordPageBaseUrl);
			var passwordValues = HttpUtility.ParseQueryString(uriDeconstructor.Query);
			var encodedQueryString = WebUtility.UrlEncode(passwordSecureQueryString.ToString());
			passwordValues.Add(SecureQueryString.QueryStringKey, encodedQueryString);

			return uriDeconstructor.GetUriWithNewQuery(passwordValues.ToString());
		}
	}
}
