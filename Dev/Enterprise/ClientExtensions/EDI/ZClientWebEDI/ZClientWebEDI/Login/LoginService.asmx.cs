using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Web.Services;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	[WebService(Namespace = "http://tempuri.org/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	[ToolboxItem(false)]
	public class LoginService : WebService
	{
		[WebMethod(EnableSession = true)]
		public string GetAutoLoginUrl(string securedQueryString)
		{
			return GetAutoLoginUrlWithReturnUrl(securedQueryString, null);
		}

		[WebMethod(EnableSession = true)]
		[SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", Justification = "Used as a web api controller param.")]
		[SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings", Justification = "Returned from a web api controller.")]
		public string GetAutoLoginUrlWithReturnUrl(string staffSecuredQueryString, string baseUrl)
		{
			using (Db.DisposableActionForDbConnection())
			{
#if DEBUG
				if (!Globals.IsTest)
#endif
				{
					((ZGlobal)Context.ApplicationInstance).SetupEnvironment();
				}

				var helper = new StaffContactHelper(staffSecuredQueryString);
				if (helper.Database != null && helper.Database.LD_AllowAutoLogin)
				{
					var contactImportResult = helper.FindOrCreateContact();
					var (userAccount, contact) = (contactImportResult.UserAccount, contactImportResult.Contact);

					if (userAccount != null)
					{
						return BuildUrl(userAccount, baseUrl);
					}
					else if (contact != null)
					{
						return BuildUrl(contact, baseUrl);
					}
				}

				var path = EDIDataRegistry.Instance.MyAccountSiteRootUrl.Value.Trim('/') + "/Login/AutoLogin.aspx";
				var builder = new UriBuilder(path) { Port = -1 };
				return builder.ToString();
			}
		}

		[WebMethod(EnableSession = true)]
		[SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", Justification = "Used as a web api controller param.")]
		[SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings", Justification = "Returned from a web api controller.")]
		public string GetContactAutoLoginUrlWithReturnUrl(Guid contactPK, string baseUrl)
		{
			using (Db.DisposableActionForDbConnection())
			{
#if DEBUG
				if (!Globals.IsTest)
#endif
				{
					((ZGlobal)Context.ApplicationInstance).SetupEnvironment();
				}

				var contact = new BusinessObjectFactory().Load<OrgContact>(contactPK);
				if (contact != null)
				{
					return BuildUrl(contact, baseUrl);
				}
				else
				{
					var path = EDIDataRegistry.Instance.MyAccountSiteRootUrl.Value.Trim('/') + "/Login/AutoLogin.aspx";
					var builder = new UriBuilder(path) { Port = -1 };
					return builder.ToString();
				}
			}
		}

		string BuildUrl(OrgContact contact, string baseUrl)
		{
			var secureQueryString = new SecureQueryString
			{
				{ OrgContactSchema.Constants.Prefix, contact.PK.ToString() }
			};

			if (!string.IsNullOrEmpty(baseUrl))
			{
				secureQueryString.Add("ReturnUrl", baseUrl);
			}

			var queryString = string.Format(CultureInfo.InvariantCulture, "?{0}={1}", SecureQueryString.QueryStringKey, WebUtility.UrlEncode(secureQueryString.ToString()));
			var path = EDIDataRegistry.Instance.MyAccountSiteRootUrl.Value.Trim('/') + "/Login/AutoLogin.aspx" + queryString;
			var builder = new UriBuilder(path) { Port = -1 };
			return builder.ToString();
		}

		string BuildUrl(EdiCustomerUserAccount userAccount, string baseUrl)
		{
			var secureQueryString = new SecureQueryString
			{
				{ EdiCustomerUserAccountSchema.Constants.Prefix, userAccount.PK.ToString() }
			};

			if (!string.IsNullOrEmpty(baseUrl))
			{
				secureQueryString.Add("ReturnUrl", baseUrl);
			}

			var queryString = string.Format(CultureInfo.InvariantCulture, "?{0}={1}", SecureQueryString.QueryStringKey, WebUtility.UrlEncode(secureQueryString.ToString()));
			var path = EDIDataRegistry.Instance.MyAccountSiteRootUrl.Value.Trim('/') + "/Login/AutoLogin.aspx" + queryString;
			var builder = new UriBuilder(path) { Port = -1 };
			return builder.ToString();
		}
	}
}
