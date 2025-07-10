using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Web;
using System.Web.Security;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Login;
using Enterprise.ZClientWebCargoWiseEDI.WebApi.Models.PortalAuth;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public static class AutoLoginHelper
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings")]
		public static Uri UserRoutingLogin(string originalRequestUrl, EdiCustomerUserAccount userAccount, ZGlobal appInstance, OrgContact fallbackContact = null, ZGuid previousLoggedInContactPK = default)
		{
			return UserRoutingLogin(new Uri(originalRequestUrl, UriKind.RelativeOrAbsolute), userAccount, appInstance, fallbackContact, previousLoggedInContactPK);
		}

		public static Uri UserRoutingLogin(Uri originalRequestUrl, EdiCustomerUserAccount userAccount, ZGlobal appInstance, OrgContact fallbackContact = null, ZGuid previousLoggedInContactPK = default)
		{
			var contact = userAccount?.WebAccessContact ?? fallbackContact;
			var router = userAccount != null ? new MyAccountLoginRouter(originalRequestUrl, userAccount, appInstance) : new MyAccountLoginRouter(originalRequestUrl, contact, appInstance);

			if (contact != null && !previousLoggedInContactPK.IsEmpty && contact.PK == previousLoggedInContactPK)
			{
				router.AddUserDefinedQueryParameter(PreviouslyLoggedInParameterKey, PreviouslyLoggedInParameterValue);
			}

			var redirectUrl = router.GetRoutingUrl();
			return redirectUrl;
		}

		internal static void SetAuthCookie(BasePage page, OrgContactWebUser user)
		{
			if (!user.IsLoggedIn)
			{
				return;
			}

			try
			{
				FormsAuthentication.SetAuthCookie(user.LoggedInUserName, false);
				if (page.IsInLiteViewMode)
				{
					MyAccountLoginLiteHelper.CreateLiteViewModeCookies(page);
				}
			}
			catch (HttpException ex)
			{
				user.Logout();
				var queryString =
					new SecureQueryString
					{
						["title"] = "Failed to set authentication cookie",
						["message"] = ex.Message
					};

				string redirectUrl = FormattableString.Invariant($"~/Error.aspx?data={WebUtility.UrlEncode(queryString.ToString())}");
				page.Response.Redirect(redirectUrl);
			}
		}

		public static string SerializeToXml(AutoLoginTokenScope scope)
		{
			var xmlSerializer = new XmlSerializer(typeof(AutoLoginTokenScope));
			using (var stringWriter = new StringWriter())
			{
				xmlSerializer.Serialize(stringWriter, scope);
				return stringWriter.ToString();
			}
		}

		public static AutoLoginTokenScope DeserializeFromXml(string xml)
		{
			var xmlSerializer = new XmlSerializer(typeof(AutoLoginTokenScope));
			using (var stringReader = new StringReader(xml))
			{
				return (AutoLoginTokenScope)xmlSerializer.Deserialize(stringReader);
			}
		}

		internal static bool CheckPreviousLoginStatus(NameValueCollection requestQueryString)
			=> PreviouslyLoggedInParameterValue == LoginRouter.GetUserDefinedQueryParameterValue(requestQueryString, PreviouslyLoggedInParameterKey);

		const string PreviouslyLoggedInParameterKey = "MYA_PLI";
		const string PreviouslyLoggedInParameterValue = "1";
	}
}
