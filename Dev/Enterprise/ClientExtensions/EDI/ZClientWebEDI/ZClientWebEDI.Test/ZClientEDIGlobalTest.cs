using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;
using System.Xml;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.Client.EDI;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Microsoft.VisualStudio.Services.Common;
using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;
using ZClientEDI.Business;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	[HttpContextEnabledTest]
	public class ZClientEDIGlobalTest : ZGlobalTest
	{
		protected override string WebConfigPath
		{
			get
			{
				return @"Enterprise\ClientExtensions\EDI\ZClientWebEDI\ZClientWebEDI\Web.config";
			}
		}
		byte[] WebConfigBytes
		{
			get
			{
				if (webConfigBytes == null)
				{
					var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
					webConfigBytes = resourceRetriever.GetBytes("Web.config");
				}
				return webConfigBytes;
			}
		}
		byte[] webConfigBytes;

		string WebConfigString
		{
			get
			{
				if (webConfigString == null)
				{
					var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
					webConfigString = resourceRetriever.GetString("Web.config", System.Text.Encoding.UTF8);
				}
				return webConfigString;
			}
		}
		string webConfigString;

		public void TestDomainFormsAuthentication()
		{
			using (var stream = new MemoryStream(WebConfigBytes))
			{
				XmlDocument doc = new XmlDocument();
				XmlTextReader reader = new XmlTextReader(stream);
				try
				{
					doc.Load(reader);
					XmlNodeList list = doc.GetElementsByTagName("configuration");
					var forms = list[0].SelectSingleNode("/configuration/location/system.web/authentication/forms");
					AssertNotNull(forms);
					AssertEquals(".cargowise.com", forms.Attributes["domain"].Value);
				}
				finally
				{
					reader.Close();
				}
			}
		}

		public void TestBindingSecurity()
		{
			var doc = new XmlDocument();
			doc.LoadXml(WebConfigString);
			var nodes = doc.SelectNodes("/configuration/system.serviceModel/bindings/wsHttpBinding/binding/security").OfType<XmlNode>();
			AssertEquals(4, nodes.Count());
			AssertEquals(2, nodes.Count(x => x.Attributes["mode"].Value == "Transport"));
			AssertEquals(2, nodes.Count(x => x.Attributes["mode"].Value == "None"));
			nodes = doc.SelectNodes("/configuration/system.serviceModel/services/service/endpoint").OfType<XmlNode>();
			AssertEquals(10, nodes.Count());
			AssertEquals(5, nodes.Count(x => x.Attributes["bindingConfiguration"].Value.EndsWith("SSL")));
			AssertEquals(5, nodes.Count(x => !x.Attributes["bindingConfiguration"].Value.EndsWith("SSL")));
		}

		public void TestContentSecurityPolicy()
		{
			var doc = new XmlDocument();
			doc.LoadXml(WebConfigString);
			var nodes = doc.SelectNodes("/configuration/system.webServer/httpProtocol/customHeaders/add").OfType<XmlNode>();
			AssertEquals(6, nodes.Count());
			AssertEquals(true, nodes.Any(n => n.Attributes["name"].Value == "Content-Security-Policy" && n.Attributes["value"].Value == "frame-ancestors 'self' https://*.cargowise.com/;"));
		}

		public void TestAccessControlPolicy()
		{
			var doc = new XmlDocument();
			doc.LoadXml(WebConfigString);
			var nodes = doc.SelectNodes("/configuration/system.webServer/httpProtocol/customHeaders/add").OfType<XmlNode>();
			AssertEquals(6, nodes.Count());
			AssertEquals(true, nodes.Any(n => n.Attributes["name"].Value == "Access-Control-Allow-Origin" && n.Attributes["value"].Value == "https://myaccount.cargowise.com"));
			AssertEquals(true, nodes.Any(n => n.Attributes["name"].Value == "Access-Control-Allow-Methods" && n.Attributes["value"].Value == "GET, POST, PUT, DELETE, OPTIONS"));
			AssertEquals(true, nodes.Any(n => n.Attributes["name"].Value == "Access-Control-Allow-Headers" && n.Attributes["value"].Value == "Content-Type, Authorization, X-Requested-With"));
			AssertEquals(true, nodes.Any(n => n.Attributes["name"].Value == "Access-Control-Allow-Credentials" && n.Attributes["value"].Value == "true"));
		}

		protected override int NumberOfLocations => 65;
		public override void AssertLocations(XmlNodeList locations)
		{
			var locationNodes = locations.OfType<XmlNode>();
			AssertLocationNode(locationNodes, "Error.aspx", "?");
			AssertLocationNode(locationNodes, "Incidents", "?", isDeny: true);
			AssertLocationNode(locationNodes, "Admin/RegisterPersonalEmail.aspx", "?");
			AssertLocationNode(locationNodes, "Admin/ResetPassword.aspx", "?");
			AssertLocationNode(locationNodes, "Admin/SetPassword.aspx", "?");
			AssertLocationNode(locationNodes, "Admin/ResetMasterPassword.aspx", "?");
			AssertLocationNode(locationNodes, "Admin/SetMasterPassword.aspx", "?");
			AssertLocationNode(locationNodes, "Admin", "?", isDeny: true);
			AssertLocationNode(locationNodes, "Login", "*");
			AssertLocationNode(locationNodes, "Download.aspx", "*");
			AssertLocationNode(locationNodes, "Services", "*");
			AssertLocationNode(locationNodes, "WebService", "?");
			AssertLocationNode(locationNodes, "InfoRequest", "*");
			AssertLocationNode(locationNodes, "oauth", "*");
			AssertLocationNode(locationNodes, "api/PortalAuth", "*");
			AssertLocationNode(locationNodes, "Scripts", "?");
			AssertLocationNode(locationNodes, "StyleSheets", "?");
			AssertLocationNode(locationNodes, "api/PortalAuth/AutoLoginToken", "*");
			var node = locationNodes.FirstOrDefault(x => x.Attributes["path"].Value == "api/PortalAuth/AutoLoginToken");
			AssertEquals("2.0", node.SelectSingleNode("system.web/httpRuntime").Attributes["requestValidationMode"].Value);
			AssertLocationNode(locationNodes, "ProcessTaskService/api/WorkItem", "*");
			AssertLocationNode(locationNodes, "api/JobApplication", "*");
			AssertLocationNode(locationNodes, "api/UserAgreement", "*");
			AssertLocationNode(locationNodes, "api/FeatureControl", "*");
			AssertLocationNode(locationNodes, "api/LicenceInformation", "*");
			AssertLocationNode(locationNodes, "api/ContentTranslation", "*");
			AssertLocationNode(locationNodes, "api/WiseTechAcademy", "*");
			AssertLocationNode(locationNodes, "api/LoginService", "*");
			AssertLocationNode(locationNodes, "api/ProductRegistration", "*");
			AssertLocationNode(locationNodes, "api/TrustedMessaging", "*");
			AssertLocationNode(locationNodes, "api/ERequest", "*");
			AssertLocationNode(locationNodes, "api/BorderWiseLicence", "*");
			AssertLocationNode(locationNodes, "api/BorderWiseRegistration", "*");
			AssertLocationNode(locationNodes, "api/BorderWiseContacts", "*");
			AssertLocationNode(locationNodes, "api/BorderWiseOrganisations", "*");
			AssertLocationNode(locationNodes, "api/BorderWiseWorkItems", "*");
			AssertLocationNode(locationNodes, "api/WorkItem", "*");
			AssertLocationNode(locationNodes, "api/StaffDetails", "*");
			AssertLocationNode(locationNodes, "gateway", "*");
			AssertLocationNode(locationNodes, "wtg/status", "*");
			AssertLocationNode(locationNodes, "api/sso/v2/erequest", "*");
			AssertLocationNode(locationNodes, "api/sso/v3/erequest", "*");
			AssertLocationNode(locationNodes, "api/sso/v2/my-account", "*");
			AssertLocationNode(locationNodes, "api/sso/v3/my-account", "*");
			AssertLocationNode(locationNodes, "api/sso/v2/user-agreement", "*");
			AssertLocationNode(locationNodes, "api/sso/v3/user-agreement", "*");
			AssertLocationNode(locationNodes, "api/sso/v2/trusted-messaging", "*");
			AssertLocationNode(locationNodes, "api/sso/v2/registration", "*");
			AssertLocationNode(locationNodes, "api/sso/v3/registration", "*");
			AssertLocationNode(locationNodes, "api/incident/resolve", "*");
			AssertLocationNode(locationNodes, "api/incident/confirm-resolve", "*");
			AssertLocationNode(locationNodes, "Incidents/InvalidLink.html", "*");
			AssertLocationNode(locationNodes, "Incidents/ConfirmResolved.html", "*");
			AssertLocationNode(locationNodes, "my-account/Notification.aspx", "*");
			AssertLocationNode(locationNodes, "admin/UserAgreement.aspx", "*");
			AssertLocationNode(locationNodes, "admin/UserAgreementEDocRequestHandler.axd", "*");
		}

		void AssertLocationNode(IEnumerable<XmlNode> locations, string path, string usersValue, bool isDeny = false)
		{
			var locationNode = locations.FirstOrDefault(x => x.Attributes["path"].Value == path);
			AssertNotNull(locationNode);
			var userPath = "system.web/authorization/" + (isDeny ? "deny" : "allow");
			AssertEquals(usersValue, locationNode.SelectSingleNode(userPath).Attributes["users"].Value);
		}

		#region Application_PreRequestHandlerExecute
		public void TestApplication_PreRequestHandlerExecute_AllAccess()
		{
			GlobalForTest global = new GlobalForTest();
			global.SiteUserForTest.SecurityRights = new WebSecurityRight[] { EDIWebSecurityRightsList.CustomerService, EDIWebSecurityRightsList.ClassroomSessions, EDIWebSecurityRightsList.WebSecurityAdministration, EDIWebSecurityRightsList.Downloads, EDIWebSecurityRightsList.EDIMyAccountReports, EDIWebSecurityRightsList.LicenceUsageReports };
			global.SiteUserForTest.Login("tester", "123456");
			AssertRedirectUrlAfterApplication_PreRequestHandlerExecute(global, "www.cargowise.com/My-account/downloads.aspx", true);
			AssertRedirectUrlAfterApplication_PreRequestHandlerExecute(global, "www.cargowise.com/my-account/requestUpgrade.aspx", true);
			AssertRedirectUrlAfterApplication_PreRequestHandlerExecute(global, "www.cargowise.com/Incidents/IncidEntDetails.aspx", true);
			AssertRedirectUrlAfterApplication_PreRequestHandlerExecute(global, "www.cargowise.com/Incidents/IncidENTSFORRELEAseRing.aspx", true);
			AssertRedirectUrlAfterApplication_PreRequestHandlerExecute(global, "www.cargowise.com/Reports/Reports.aspx", true);
			AssertRedirectUrlAfterApplication_PreRequestHandlerExecute(global, "www.cargowise.com/Reports/UsageReports.aspx", true);
			AssertRedirectUrlAfterApplication_PreRequestHandlerExecute(global, "www.cargowise.com/Admin/WebSecurity.aspx", true);
			AssertRedirectUrlAfterApplication_PreRequestHandlerExecute(global, "www.cargowise.com/Admin/NotificationRoles.aspx", true);
		}

		public void TestApplication_PreRequestHandlerExecute_NoAccess()
		{
			GlobalForTest global = new GlobalForTest();
			global.SiteUserForTest.SecurityRights = Array.Empty<WebSecurityRight>();
			global.SiteUserForTest.Login("tester", "123456");
			AssertRedirectUrlAfterApplication_PreRequestHandlerExecute(global, "www.cargowise.com/My-account/downloads.aspx", false);
			AssertRedirectUrlAfterApplication_PreRequestHandlerExecute(global, "www.cargowise.com/my-account/requestUpgrade.aspx", false);
			AssertRedirectUrlAfterApplication_PreRequestHandlerExecute(global, "www.cargowise.com/Incidents/IncidEntDetails.aspx", false);
			AssertRedirectUrlAfterApplication_PreRequestHandlerExecute(global, "www.cargowise.com/Incidents/IncidENTSFORRELEAseRing.aspx", false);
			AssertRedirectUrlAfterApplication_PreRequestHandlerExecute(global, "www.cargowise.com/Reports/Reports.aspx", false);
			AssertRedirectUrlAfterApplication_PreRequestHandlerExecute(global, "www.cargowise.com/Reports/UsageReports.aspx", false);
			AssertRedirectUrlAfterApplication_PreRequestHandlerExecute(global, "www.cargowise.com/Admin/WebSecurity.aspx", false);
			AssertRedirectUrlAfterApplication_PreRequestHandlerExecute(global, "www.cargowise.com/Admin/NotificationRoles.aspx", false);
			AssertRedirectUrlAfterApplication_PreRequestHandlerExecute(global, "www.cargowise.com/WiseTechAcademy/WiseTechAcademyAutoLogin.aspx", false, errorMessage: "You are not authorized to access WiseTech Academy, please contact your system administrator to allow the access.");
			ErrorReporter.Clear();
		}

		public void TestApplication_PreRequestHandlerExecute_PartialAccess()
		{
			GlobalForTest global = new GlobalForTest();
			global.SiteUserForTest.SecurityRights = new WebSecurityRight[] { EDIWebSecurityRightsList.CustomerService, EDIWebSecurityRightsList.ClassroomSessions, EDIWebSecurityRightsList.WebSecurityAdministration, EDIWebSecurityRightsList.WiseTechAcademy };
			global.SiteUserForTest.Login("tester", "123456");
			AssertRedirectUrlAfterApplication_PreRequestHandlerExecute(global, "www.cargowise.com/My-account/downloads.aspx", false);
			AssertRedirectUrlAfterApplication_PreRequestHandlerExecute(global, "www.cargowise.com/my-account/requestUpgrade.aspx", false);
			AssertRedirectUrlAfterApplication_PreRequestHandlerExecute(global, "www.cargowise.com/Reports/Reports.aspx", false);
			AssertRedirectUrlAfterApplication_PreRequestHandlerExecute(global, "www.cargowise.com/Reports/UsageReports.aspx", false);
			AssertRedirectUrlAfterApplication_PreRequestHandlerExecute(global, "www.cargowise.com/Incidents/IncidEntDetails.aspx", true);
			AssertRedirectUrlAfterApplication_PreRequestHandlerExecute(global, "www.cargowise.com/Incidents/IncidENTSFORRELEAseRing.aspx", true);
			AssertRedirectUrlAfterApplication_PreRequestHandlerExecute(global, "www.cargowise.com/Admin/WebSecurity.aspx", true);
			AssertRedirectUrlAfterApplication_PreRequestHandlerExecute(global, "www.cargowise.com/Admin/NotificationRoles.aspx", true);
			AssertRedirectUrlAfterApplication_PreRequestHandlerExecute(global, "www.cargowise.com/WiseTechAcademy/WiseTechAcademyAutoLogin.aspx", true);
		}

		public void TestApplication_PreRequestHandlerExecute_InvoicePage()
		{
			GlobalForTest global = new GlobalForTest();
			AssertRedirectUrlAfterApplication_PreRequestHandlerExecute(global, "www.cargowise.com/Reports/Invoices.aspx", false, true);
			global.SiteUserForTest.Login("tester", "123456");
			AssertRedirectUrlAfterApplication_PreRequestHandlerExecute(global, "www.cargowise.com/Reports/Invoices.aspx", false);
			global.SiteUserForTest.Logout();
			global.SiteUserForTest.Login("AR account", "123456");
			AssertRedirectUrlAfterApplication_PreRequestHandlerExecute(global, "www.cargowise.com/Reports/Invoices.aspx", true);
		}

		public void TestApplication_PreRequestHandlerExecute_InvoicePage_NoUser()
		{
			GlobalForTest global = new GlobalForTest();
			global.IsNoUser = true;
			AssertRedirectUrlAfterApplication_PreRequestHandlerExecute(global, "www.cargowise.com/Reports/Invoices.aspx", false, true);
		}

		public void TestApplication_PreRequestHandlerExecute_NoUser()
		{
			GlobalForTest global = new GlobalForTest();
			global.IsNoUser = true;
			AssertRedirectUrlAfterApplication_PreRequestHandlerExecute(global, "www.cargowise.com/My-account/downloads.aspx", false, true);
		}

		public void TestApplication_PreRequestHandlerExecute_WTAPage()
		{
			GlobalForTest global = new GlobalForTest();
			global.SiteUserForTest.SecurityRights = Array.Empty<WebSecurityRight>();
			global.SiteUserForTest.Login("tester", "123456");
			AssertRedirectUrlAfterApplication_PreRequestHandlerExecute(global, "www.cargowise.com/WiseTechAcademy/WiseTechAcademyAutoLogin.aspx", false, false, "You are not authorized to access WiseTech Academy, please contact your system administrator to allow the access.");
			AssertEquals("", ErrorReporter.LastMessageReported);
		}

		void AssertRedirectUrlAfterApplication_PreRequestHandlerExecute(GlobalForTest global, string requestUrl, bool expectedToBeAllowed, bool shouldRedirectToLoginPage = false, string errorMessage = null)
		{
			global.RequestPathForTest = requestUrl;
			global.Response.RedirectLocation = null;
			global.Application_PreRequestHandlerExecute();
			if (expectedToBeAllowed)
			{
				AssertNull(global.Response.RedirectLocation);
			}
			else if (shouldRedirectToLoginPage)
			{
				AssertEquals(global.LoginPage, global.Response.RedirectLocation);
			}
			else
			{
				SecureQueryString queryString = new SecureQueryString();
				queryString["title"] = "Unauthorized Page Request";
				queryString["message"] = errorMessage ?? "You are not authorized to view this page.";
				string redirectUrl = string.Format("/webapp/Error.aspx?data={0}", WebUtility.UrlEncode(queryString.ToString()));
				AssertEquals(redirectUrl, global.Response.RedirectLocation);
			}
		}

		#endregion

		public void TestUpdateWebConfig()
		{
			using (var tempFile = TempFile.New())
			{
				using (var readSteam = new MemoryStream(WebConfigBytes))
				using (var writeStream = File.OpenWrite(tempFile.Filename))
				{
					readSteam.CopyTo(writeStream);
				}

				GlobalForTest global = new GlobalForTest()
				{ WebConfigAbsolutePath = tempFile.Filename };
				EDIDataRegistry.Instance.MyAccountFormAuthenticationCookieDomain.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ".wisetechglobal.com");
				EDIDataRegistry.Instance.MyAccountFormAuthenticationCookieSameSite.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Lax");
				EDIDataRegistry.Instance.MyAccountFormAuthenticationCookieSSL.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "False");
				global.UpdateWebConfig_Expose();
				var config = global.OpenWebConfiguration_Expose();
				var authenticationSection = (AuthenticationSection)config.GetSection("system.web/authentication");
				var formsAuthentication = authenticationSection.Forms;
				AssertEquals(".cargowise.com", formsAuthentication.Domain);
				AssertEquals(SameSiteMode.None, formsAuthentication.CookieSameSite);
				AssertEquals(true, formsAuthentication.RequireSSL);
				EDIDataRegistry.Instance.EnableMyAccountWebConfigOveridden.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				global.UpdateWebConfig_Expose();
				config = global.OpenWebConfiguration_Expose();
				authenticationSection = (AuthenticationSection)config.GetSection("system.web/authentication");
				formsAuthentication = authenticationSection.Forms;
				AssertEquals(".wisetechglobal.com", formsAuthentication.Domain);
				AssertEquals(SameSiteMode.Lax, formsAuthentication.CookieSameSite);
				AssertEquals(false, formsAuthentication.RequireSSL);
				EDIDataRegistry.Instance.MyAccountFormAuthenticationCookieDomain.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ".myaccount.com");
				EDIDataRegistry.Instance.MyAccountFormAuthenticationCookieSameSite.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Strict");
				EDIDataRegistry.Instance.MyAccountFormAuthenticationCookieSSL.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "True");
				global.UpdateWebConfig_Expose();
				config = global.OpenWebConfiguration_Expose();
				authenticationSection = (AuthenticationSection)config.GetSection("system.web/authentication");
				formsAuthentication = authenticationSection.Forms;
				AssertEquals(".myaccount.com", formsAuthentication.Domain);
				AssertEquals(SameSiteMode.Strict, formsAuthentication.CookieSameSite);
				AssertEquals(true, formsAuthentication.RequireSSL);
			}
		}

		public void TestUpdateWebConfigReportErrorWhenRunningOnEdiProdDatabase()
		{
			using (var tempFile = TempFile.New())
			{
				using (var readSteam = new MemoryStream(WebConfigBytes))
				using (var writeStream = File.OpenWrite(tempFile.Filename))
				{
					readSteam.CopyTo(writeStream);
				}

				EDIDataRegistry.Instance.EnableMyAccountWebConfigOveridden.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				GlobalForTest global = new GlobalForTest() { WebConfigAbsolutePath = tempFile.Filename };
				global.ShouldThrowExceptionOnOpenWebConfiguration = true;
				ObjectFactory.Get<IProductRegistration>().KeyForTest.DatabaseTypeForTest = "DAT";

				AssertEquals(false, EdiProdDbHelper.IsRunningOnEdiProdDatabase);
				global.UpdateWebConfig_Expose();
				AssertNull(ErrorReporter.LastExceptionReported);
				var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
				var code = registrationKey.EnterpriseCode + "XXX" + registrationKey.ServerCode;
				SystemDataRegistry.Instance.EdiProdLicenceIdentifier.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, code);
				AssertEquals(true, EdiProdDbHelper.IsRunningOnEdiProdDatabase);
				global.UpdateWebConfig_Expose();
				AssertEquals("ShouldThrowExceptionOnOpenWebConfiguration", ErrorReporter.LastExceptionReported.Message);
				ErrorReporter.Clear();
			}
		}

		public void TestUpdateWebConfigDoNotReportErrorWhenServerCodeIsUAT()
		{
			using (var tempFile = TempFile.New())
			{
				using (var readSteam = new MemoryStream(WebConfigBytes))
				using (var writeStream = File.OpenWrite(tempFile.Filename))
				{
					readSteam.CopyTo(writeStream);
				}

				EDIDataRegistry.Instance.EnableMyAccountWebConfigOveridden.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				GlobalForTest global = new GlobalForTest() { WebConfigAbsolutePath = tempFile.Filename };
				global.ShouldThrowExceptionOnOpenWebConfiguration = true;

				ObjectFactory.Get<IProductRegistration>().KeyForTest.ServerCodeForTest = "UAT";
				var registrationKeyForTest = ObjectFactory.Get<IProductRegistration>().KeyForTest;
				var codeForTest = registrationKeyForTest.EnterpriseCodeForTest + "XXX" + registrationKeyForTest.ServerCodeForTest;
				SystemDataRegistry.Instance.EdiProdLicenceIdentifier.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, codeForTest);
				AssertEquals(true, EdiProdDbHelper.IsRunningOnEdiProdDatabase);
				global.UpdateWebConfig_Expose();
				AssertNull(ErrorReporter.LastExceptionReported);
			}
		}

		public void TestUpdateWebConfigDoNotReportErrorWhenDatabaseTypeIsTST()
		{
			using (var tempFile = TempFile.New())
			{
				using (var readSteam = new MemoryStream(WebConfigBytes))
				using (var writeStream = File.OpenWrite(tempFile.Filename))
				{
					readSteam.CopyTo(writeStream);
				}

				EDIDataRegistry.Instance.EnableMyAccountWebConfigOveridden.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				GlobalForTest global = new GlobalForTest() { WebConfigAbsolutePath = tempFile.Filename };
				global.ShouldThrowExceptionOnOpenWebConfiguration = true;

				ObjectFactory.Get<IProductRegistration>().KeyForTest.ServerCodeForTest = "EDI";
				ObjectFactory.Get<IProductRegistration>().KeyForTest.DatabaseTypeForTest = "TST";
				var registrationKeyForTest = ObjectFactory.Get<IProductRegistration>().KeyForTest;
				var codeForTest = registrationKeyForTest.EnterpriseCodeForTest + "XXX" + registrationKeyForTest.ServerCodeForTest;
				SystemDataRegistry.Instance.EdiProdLicenceIdentifier.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, codeForTest);
				AssertEquals(true, EdiProdDbHelper.IsRunningOnEdiProdDatabase);
				global.UpdateWebConfig_Expose();
				AssertNull(ErrorReporter.LastExceptionReported);
			}
		}

		#region TestEnvironmentSetupForSessions
		public void TestEnvironmentShouldNotBeSetupForUnauthenticatedRequest()
		{
			EnvProxy.Instance.Registry.ExpectedClientDLL = "ZClientEDI";
			Env.ClearUserContext();
			AssertEquals("Precondition: User context should be unset", Guid.Empty, Env.CurrentUserPK);
			AssertEquals("Precondition: User context should be unset", Guid.Empty, Env.CurrentBranchPK);
			AssertEquals("Precondition: User context should be unset", Guid.Empty, Env.CurrentDepartmentPK);
			using (var global = new GlobalForSessionTest())
			{
				AssertEquals("Precondition", null, global.SiteUser);
				AssertEquals("Precondition: DBVersions should match", true, global.DBVersionMatchesExposed);
				AssertEquals("Should be authenticated", false, HttpContext.Current.Request.IsAuthenticated);
				global.Session_Start_Exposed();
				AssertNotEquals("Site user should be setup", null, global.SiteUser);
				AssertEquals("User context should remain unset", Guid.Empty, Env.CurrentUserPK);
				AssertEquals("User context should remain unset", Guid.Empty, Env.CurrentBranchPK);
				AssertEquals("User context should remain unset", Guid.Empty, Env.CurrentDepartmentPK);
			}
		}

		public void TestEnvironmentShouldBeSetupForAuthenticatedRequest()
		{
			EnvProxy.Instance.Registry.ExpectedClientDLL = "ZClientEDI";
			Env.ClearUserContext();
			AssertEquals("Precondition: User context should be unset", Guid.Empty, Env.CurrentUserPK);
			AssertEquals("Precondition: User context should be unset", Guid.Empty, Env.CurrentBranchPK);
			AssertEquals("Precondition: User context should be unset", Guid.Empty, Env.CurrentDepartmentPK);
			AuthenticateRequest();
			using (var global = new GlobalForSessionTest())
			{
				AssertEquals("Precondition", null, global.SiteUser);
				AssertEquals("Precondition: DBVersions should match", true, global.DBVersionMatchesExposed);
				AssertEquals("Should be authenticated", true, HttpContext.Current.Request.IsAuthenticated);
				global.Session_Start_Exposed();
				AssertNotEquals("Site user should be setup", null, global.SiteUser);
				AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentUserPK);
				AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentBranchPK);
				AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentDepartmentPK);
			}
		}
		#endregion

		public void TestCookiesExpiresWillBeRefreshedAfterRequest()
		{
			var now = DateTime.Now;
			var global = new GlobalForTest();
			global.SiteUserForTest.SecurityRights = new WebSecurityRight[] { EDIWebSecurityRightsList.CustomerService, EDIWebSecurityRightsList.ClassroomSessions, EDIWebSecurityRightsList.WebSecurityAdministration, EDIWebSecurityRightsList.Downloads, EDIWebSecurityRightsList.EDIMyAccountReports, EDIWebSecurityRightsList.LicenceUsageReports };
			global.SiteUserForTest.Login("tester", "123456");
			AuthenticateRequest();
			var securityRightCookie = new HttpCookie(MyAccountLoginLiteHelper.SecurityRightsCookieName, "test");
			securityRightCookie.Expires = now.AddMinutes(1);
			var loggedinUserInfoCookie = new HttpCookie(MyAccountLoginLiteHelper.LoggedInUserInfoCookieName, "test user");
			loggedinUserInfoCookie.Expires = now.AddMinutes(1);
			HttpContext.Current.Request.Cookies.Add(securityRightCookie);
			HttpContext.Current.Request.Cookies.Add(loggedinUserInfoCookie);
			global.Application_AuthenticateRequest_Exposed();
			var timeout = Global.SessionTimeout;
			Assert(timeout > 1);
			var securityRightCookieAfterRequest = HttpContext.Current.Request.Cookies[MyAccountLoginLiteHelper.SecurityRightsCookieName];
			AssertGreaterThanOrEqualTo(securityRightCookieAfterRequest.Expires, now.AddMinutes(timeout));

			var loggedinUserInfoCookieAfterRequest = HttpContext.Current.Request.Cookies[MyAccountLoginLiteHelper.LoggedInUserInfoCookieName];
			AssertGreaterThanOrEqualTo(loggedinUserInfoCookieAfterRequest.Expires, now.AddMinutes(timeout));
		}

		void AuthenticateRequest()
		{
			var identity = new FormsIdentity(new FormsAuthenticationTicket("testUser", false, 10));
			GenericPrincipal principal = new GenericPrincipal(identity, Array.Empty<string>());
			HttpContext.Current.User = principal;
		}

		class GlobalForSessionTest : Global
		{
			public void Session_Start_Exposed()
			{
				Session_Start(null, EventArgs.Empty);
			}

			public bool DBVersionMatchesExposed => DBVersionMatches;
		}

		public void TestConfigNLog()
		{
			using (EDIDataRegistry.Instance.MyAccountLoggerFileTargetPath.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, @"c:\log\log.txt"))
			using (EDIDataRegistry.Instance.MyAccountLoggerKafkaTargetTopic.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "sometopic1"))
			using (EDIDataRegistry.Instance.MyAccountLoggerKafkaTargetBrokers.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "127.0.0.1:8000,127.0.0.1:8001,127.0.0.1:8002"))
			{
				var originalConfiguration = LogManager.Configuration;
				try
				{
					var global = new GlobalForTest();
					LogManager.Configuration = new LoggingConfiguration();
					global.ConfigNLog_Exposed();

					var rules = LogManager.Configuration.LoggingRules;
					AssertEquals(2, rules.Count);

					var fileRule = LogManager.Configuration.LoggingRules.Single(x => x.Targets[0].Name == "file");
					var fileTarget = ((AsyncTargetWrapper)fileRule.Targets.Single()).WrappedTarget as FileTarget;
					AssertEquals("JsonLayout=message-${longdate} | ${level} | ${logger}: ${message} ${exception:format=tostring}", fileTarget.Layout.ToString());
					AssertEquals(@"c:\log\log.txt", fileTarget.FileName.ToString());
					AssertEquals("*", fileRule.LoggerNamePattern);
					AssertEquals(true, fileRule.IsLoggingEnabledForLevel(LogLevel.Info));

					var kafkaRule = LogManager.Configuration.LoggingRules.Single(x => x.Targets[0].Name == "kafka");
					using (var kafkaTarget = (((AsyncTargetWrapper)kafkaRule.Targets.Single()).WrappedTarget as BufferingTargetWrapper).WrappedTarget as TargetWithLayout)
					{
						AssertEquals("JsonLayout=message-${longdate} | ${level} | ${logger}: ${message} ${exception:format=tostring}", kafkaTarget.Layout.ToString());
						AssertEquals("*", kafkaRule.LoggerNamePattern);
						AssertEquals(true, kafkaRule.IsLoggingEnabledForLevel(LogLevel.Info));
						AssertEquals("sometopic1", kafkaTarget.GetType().GetProperty("Topic").GetValue(kafkaTarget, null).ToString());
					}
				}
				finally
				{
					LogManager.Configuration = originalConfiguration;
					LogManager.Configuration?.Reload();
					LogManager.ReconfigExistingLoggers();
				}
			}
		}

		public void TestConfigNLog_ExceptionHandling()
		{
			ErrorReporter.Clear();
			using (EDIDataRegistry.Instance.MyAccountLoggerFileTargetPath.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, @"c:\log\log.txt"))
			using (EDIDataRegistry.Instance.MyAccountLoggerKafkaTargetTopic.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "sometopic1"))
			using (EDIDataRegistry.Instance.MyAccountLoggerKafkaTargetBrokers.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "127.0.0.1:8000,127.0.0.1:8001,127.0.0.1:8002"))
			{
				var originalConfiguration = LogManager.Configuration;
				try
				{
					var global = new GlobalForTest();
					LogManager.Configuration = null;
					global.ConfigNLog_Exposed();
				}
				finally
				{
					LogManager.Configuration = originalConfiguration;
					LogManager.Configuration?.Reload();
					LogManager.ReconfigExistingLoggers();
				}
			}

			AssertEquals(typeof(NullReferenceException), ErrorReporter.LastExceptionReported.GetType());
			AssertEquals("Enterprise.ZClientWebCargoWiseEDI.Global.ConfigNLog", ErrorReporter.LastKeyReported);
			AssertEquals(ErrorReporter.LastExceptionReported.Message, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestExceptionShouldBeHandled()
		{
			var global = new GlobalForTest();
			AssertEquals(true, global.ExceptionShouldBeHandled_Expose(new InvalidCastException("abc")));
			AssertEquals(false, global.ExceptionShouldBeHandled_Expose(new Exception("abc", new COMException("def", unchecked((int)0x800703E3)))));
		}

		public void TestPageBaseClass()
		{
			var assembly = Assembly.GetAssembly(typeof(BasePage));
			AssertEquals("ZClientWebEDI", assembly.GetName().Name);

			var types = Assembly.GetAssembly(typeof(BasePage)).GetTypes().Where(x =>
				x.Namespace != null &&
				!x.FullName.Contains("test", StringComparison.OrdinalIgnoreCase));

			var pageTypes = types.Where(x => x.IsSubclassOf(typeof(ZPage)));
			Assert(pageTypes.Any());

			var resultList = pageTypes.Where(x =>
				!x.IsSubclassOf(typeof(BasePage)) &&
				!x.IsOfType(typeof(BasePage))).Select(y => y.FullName);
			Assert($"All Pages should be BasePage's subclass \r\n{string.Join(System.Environment.NewLine, resultList)}", resultList.IsNullOrEmpty());
		}

		class GlobalForTest : Global
		{
			public GlobalForTest()
			{
				typeof(HttpApplication).GetField("_context", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(this, HttpContext.Current);
			}

			public string RequestPathForTest { get; set; }

			protected override string RequestPhysicalPath
			{
				get
				{
					return RequestPathForTest;
				}
			}

			public WebUserForTest SiteUserForTest
			{
				get
				{
					return (WebUserForTest)SiteUser;
				}
			}

			public bool IsNoUser;
			public override WebUser SiteUser
			{
				get
				{
					return IsNoUser ? null : siteUser ?? (siteUser = new WebUserForTest());
				}
			}

			public void Application_PreRequestHandlerExecute()
			{
				base.Application_PreRequestHandlerExecute(this, EventArgs.Empty);
			}

			public void Application_AuthenticateRequest_Exposed()
			{
				Application_AuthenticateRequest(null, EventArgs.Empty);
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
			public override string MapPath(string virtualPath)
			{
				string result = virtualPath.Trim('~');
				if (result.EndsWith("/WebApi/Helpers/Certificate/Kafka.pem"))
				{
					return Path.Combine(BaseSourcePath, @"Enterprise\ClientExtensions\EDI\ZClientWebEDI\ZClientWebEDI\WebApi\Helpers\Certificate\Kafka.pem");
				}
				else
				{
					return "www.cargowise.com" + result;
				}
			}

			protected override void RedirectUrl(string url)
			{
				Response.Redirect(url, false);
			}

			public void UpdateWebConfig_Expose()
			{
				base.UpdateWebConfig();
			}

			public Configuration OpenWebConfiguration_Expose()
			{
				return OpenWebConfiguration();
			}

			public bool ExceptionShouldBeHandled_Expose(Exception unhandledException)
			{
				return ExceptionShouldBeHandled(unhandledException);
			}

			protected override Configuration OpenWebConfiguration()
			{
				if (ShouldThrowExceptionOnOpenWebConfiguration)
				{
					throw new Exception(nameof(ShouldThrowExceptionOnOpenWebConfiguration));
				}

				var configFile = new FileInfo(WebConfigAbsolutePath);
				var directoryMapping = new VirtualDirectoryMapping(configFile.DirectoryName, true, configFile.Name);
				var fileMap = new WebConfigurationFileMap();
				fileMap.VirtualDirectories.Add("/", directoryMapping);
				WebConfiguration = WebConfigurationManager.OpenMappedWebConfiguration(fileMap, "/");
				return WebConfiguration;
			}

			public bool ShouldThrowExceptionOnOpenWebConfiguration { get; set; }
			public void ConfigNLog_Exposed() => ConfigNLog();
			public string WebConfigAbsolutePath { get; set; }

			Configuration WebConfiguration { get; set; }

			WebUser siteUser;
		}

		class WebUserForTest : OrgContactWebUser
		{
			public WebSecurityRight[] SecurityRights;
			public override bool AreSecurityRightsGranted(WebSecurityRight securityRight)
			{
				return new List<WebSecurityRight>(SecurityRights).Contains(securityRight);
			}

			protected override IContactable GetNewContactableForSpecialUser(BusinessObjectFactory factory, string affiliationCode, string username)
			{
				throw new NotImplementedException();
			}

			protected override IContactable LoginCore(BusinessObjectFactory factory, string affiliationCode, string username, string password, byte[] loginHash, bool shouldRecordLoginFailAttempt = true)
			{
				OrgContact contact = factory.NewWithValidTestData<OrgContact>();
				if (username == "AR account")
				{
					contact.Documents.AddNew().OD_DocumentGroup = ContactType.Receivables.Code;
				}

				return contact;
			}
		}
	}
}
