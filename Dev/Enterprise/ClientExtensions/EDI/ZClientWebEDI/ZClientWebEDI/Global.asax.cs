using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZClientWebCargoWiseEDI.Base;
using Enterprise.ZClientWebCargoWiseEDI.Handlers;
using NLog;
using NLog.Layouts;
using NLog.Targets;
using NLog.Targets.Wrappers;
using WTG.Logging.NLog.Kafka;
using ZClientEDI.Business;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class Global : ZGlobal
	{
		//do not use SetSessionStateBehavior(...), it may cause high memory usage.

		static Global()
		{
			var sessionStateSection = (SessionStateSection)ConfigurationManager.GetSection("system.web/sessionState");
			var timeout = (int)sessionStateSection?.Timeout.TotalMinutes;
			SessionTimeout = timeout > 0 ? timeout : 20;
		}

		protected override void Application_Start(object sender, EventArgs e)
		{
			base.Application_Start(sender, e);

			GlobalConfiguration.Configure(ConfigureWebAPI);
			GlobalConfiguration.Configuration.EnsureInitialized();
			ConfigNLog();
			UpdateWebConfig();
			if (bool.TryParse(WebConfigurationManager.OpenWebConfiguration("~").AppSettings.Settings["SingleActiveUserSession"]?.Value, out var result))
			{
				EnableSingleActiveUserSession = result;
			}
		}

		public bool EnableSingleActiveUserSession { get; protected set; } = true;

		protected void Application_AcquireRequestState(object sender, EventArgs e)
		{
			base.Application_BeginRequest(sender, e);
			if ((SiteUser?.IsLoggedIn ?? false) && EnableSingleActiveUserSession)
			{
				var heartBeatManager = new MyAccountHeartBeatManager(Session);
				if (heartBeatManager.HasUserContext() && !heartBeatManager.IsCurrentContextValid)
				{
					this.SignOut(false);
					MyAccountLoginLiteHelper.ExpireLiteViewModeCookies(Request, Response);

					var message = "You have been logged out because your account has been logged in elsewhere.";
					Response.Redirect(GetLoginPageUrlWithMessage(message));
				}
			}
		}

		#region Web API

		static void ConfigureWebAPI(HttpConfiguration configuration)
		{
			configuration.Services.Replace(typeof(IExceptionHandler), new ZClientWebEDIExceptionHandler());

			configuration.MapHttpAttributeRoutes();

			configuration.EnableCors();
		}

		#endregion

		#region Page Constants

		public string LoginPageStyleSheetPath => ApplicationRoot + "StyleSheets/LoginV2BaseStyle.css";

		public override string HomePage
		{
			get { return DefaultPage; }
		}

		public override string DefaultPage
		{
			get { return ApplicationRoot + "Default.aspx"; }
		}

		public override string LogoImage
		{
			get { return ApplicationRoot + "Images/Logo.png"; }
		}

		public string TermsAndConditionsPage
		{
			get { return ApplicationRoot + "Login/TermsAndConditions.aspx"; }
		}

		public string IntroPage
		{
			get { return ApplicationRoot + "Intro.aspx"; }
		}

		public string ResetPasswordPage => EDIDataRegistry.Instance.MyAccountSiteRootUrl.Value.Trim('/') + "/Admin/ResetPassword.aspx";

		public string ResetPasswordKey => "ResetKey";

		public string SetPasswordPage => EDIDataRegistry.Instance.MyAccountSiteRootUrl.Value.Trim('/') + "/Admin/SetPassword.aspx";

		public string SetPasswordKey => "SetKey";

		public string ResetMasterPasswordPage => EDIDataRegistry.Instance.MyAccountSiteRootUrl.Value.Trim('/') + "/Admin/ResetMasterPassword.aspx";

		public string SetMasterPasswordPage => EDIDataRegistry.Instance.MyAccountSiteRootUrl.Value.Trim('/') + "/Admin/SetMasterPassword.aspx";

		public string RegisterPersonalEmailPage => EDIDataRegistry.Instance.MyAccountSiteRootUrl.Value.Trim('/') + "/Admin/RegisterPersonalEmail.aspx";

		public string RegisterKey => "RegisterKey";

		public string LoginOptionsPage => EDIDataRegistry.Instance.MyAccountSiteRootUrl.Value.Trim('/') + "/Login/LoginOptions.aspx";

		public string ChooseCompanyPage => EDIDataRegistry.Instance.MyAccountSiteRootUrl.Value.Trim('/') + "/Login/ChooseCompany.aspx";

		public static string IncidentActionLinkInvalidLinkPage => EDIDataRegistry.Instance.MyAccountSiteRootUrl.Value.Trim('/') + "/Incidents/InvalidLink.html";

		public static string IncidentActionLinkConfirmResolvedPage => EDIDataRegistry.Instance.MyAccountSiteRootUrl.Value.Trim('/') + "/Incidents/ConfirmResolved.html";

		public const string IncidentDetailsPagePath = "Incidents/IncidentDetails.aspx";
		public string IncidentDetailsPage
		{
			get { return ApplicationRoot + IncidentDetailsPagePath; }
		}

		public string DescriptionFolder
		{
			get { return ApplicationRoot + "Descriptions/"; }
		}

		public static int SessionTimeout { get; }

		public override string LoginPage
		{
			get
			{
				if (OIDCLoginHelper.IsOIDCReady())
				{
					return ApplicationRoot + "Login/LoginV2.aspx";
				}
				else
				{
					return ApplicationRoot + "Login/Login.aspx";
				}
			}
		}

		public string GetLoginPageUrlWithMessage(string message)
		{
			var uri = LoginPage;

			if (!string.IsNullOrEmpty(message))
			{
				var query = new QueryString();
				var secureQueryString = new SecureQueryString { { "message", message } };
				query.Add("data", secureQueryString.ToString());
				uri += $"?{query}";
			}

			return uri;
		}

		#endregion

		#region DNN My Account Page Constants

		public string HostingSiteRoot
		{
			get { return EDIDataRegistry.Instance.MyAccountHostingSiteRootUrl.Value; }
		}

		public string HostingSiteHomePage
		{
			get { return HostingSiteRoot + "Home.aspx"; }
		}

		public string HostingSiteLoginPage
		{
			get { return HostingSiteRoot + "Home/MyAccountLogin.aspx"; }
		}

		public static string GetAppSettingValue(string key)
		{
			var webConfig = WebConfigurationManager.OpenWebConfiguration("~");
			var setting = webConfig.AppSettings.Settings[key];
			return setting != null ? setting.Value : string.Empty;
		}

		#endregion

		#region Web Form Designer generated code

		public Global()
		{
		}

		#endregion

		#region Restricted Content Handler

		protected void Application_PreRequestHandlerExecute(object sender, EventArgs e)
		{
			using (Db.DisposableActionForDbConnection())
			{
				// Application_PreRequestHandlerExecute is used here because we need the Session state bag (SiteUser is stored in Session state)
				if (!AreSecurityRightsGrantedForCurrentRequest())
				{
					if (SiteUser == null || !SiteUser.IsLoggedIn)
					{
						RedirectUrl(LoginPage);
					}
					else
					{
						if (RequestPhysicalPath.ToLower() == MapPath("~/WiseTechAcademy/WiseTechAcademyAutoLogin.aspx").ToLower())
						{
							RedirectAsUnauthorizedPageRequest("You are not authorized to access WiseTech Academy, please contact your system administrator to allow the access.");
						}
						else
						{
							RedirectAsUnauthorizedPageRequest();
						}
					}
				}
			}
		}

		public void RedirectAsUnauthorizedPageRequest(string errorMessage = null)
		{
			SecureQueryString queryString = new SecureQueryString();
			queryString["title"] = "Unauthorized Page Request";
			queryString["message"] = errorMessage ?? "You are not authorized to view this page.";

			string redirectUrl = string.Format("~/Error.aspx?data={0}", WebUtility.UrlEncode(queryString.ToString()));
			RedirectUrl(redirectUrl);
		}

		bool AreSecurityRightsGrantedForCurrentRequest()
		{
			var result = true;

			var requestPath = RequestPhysicalPath.ToLower();

			if (requestPath == MapPath("~/Reports/Invoices.aspx").ToLower())
			{
				result = SiteUser != null
					&& SiteUser.IsLoggedIn
					&& ((OrgContactWebUser)SiteUser).LoggedInUser.Documents.Find(new ZQuery(OrgDocumentSchema.OD_DocumentGroup, ContactType.Receivables.Code)).Length > 0;
			}
			else if (RestrictedContentDictionary.TryGetValue(requestPath, out var securityRight))
			{
				result = SiteUser != null && SiteUser.AreSecurityRightsGranted(securityRight);
			}

			return result;
		}

		protected virtual void RedirectUrl(string url)
		{
			Response.Redirect(url);
		}

		protected virtual string RequestPhysicalPath
		{
			get { return Request.PhysicalPath; }
		}

		Dictionary<string, WebSecurityRight> RestrictedContentDictionary
		{
			get
			{
				if (restrictedContentDictionary == null)
				{
					restrictedContentDictionary = new Dictionary<string, WebSecurityRight>
					{
						{ MapPath("~/My-account/downloads.aspx").ToLower(), EDIWebSecurityRightsList.Downloads },
						{ MapPath("~/My-account/RequestUpgrade.aspx").ToLower(), EDIWebSecurityRightsList.Downloads },
						{ MapPath("~/Incidents/IncidentDetails.aspx").ToLower(), EDIWebSecurityRightsList.CustomerService },
						{ MapPath("~/Incidents/Incidents.aspx").ToLower(), EDIWebSecurityRightsList.CustomerService },
						{ MapPath("~/Incidents/IncidentsForReleaseRing.aspx").ToLower(), EDIWebSecurityRightsList.CustomerService },
						{ MapPath("~/Admin/WebSecurity.aspx").ToLower(), EDIWebSecurityRightsList.WebSecurityAdministration },
						{ MapPath("~/Admin/NotificationRoles.aspx").ToLower(), EDIWebSecurityRightsList.WebSecurityAdministration },
						{ MapPath("~/ReleaseNotes/ReleaseNotes.aspx").ToLower(), EDIWebSecurityRightsList.UpdateNotes },
						{ MapPath("~/Reports/Reports.aspx").ToLower(), EDIWebSecurityRightsList.EDIMyAccountReports },
						{ MapPath("~/Reports/UsageReports.aspx").ToLower(), EDIWebSecurityRightsList.LicenceUsageReports },
						{ MapPath("~/WiseTechAcademy/WiseTechAcademyAutoLogin.aspx").ToLower(), EDIWebSecurityRightsList.WiseTechAcademy }
					};
				}
				return restrictedContentDictionary;
			}
		}

		Dictionary<string, WebSecurityRight> restrictedContentDictionary;

		#endregion

		#region Site User

		public override WebUser GetNewSiteUser()
		{
			return new MyAccountWebUser();
		}

		#endregion

		#region Logging

		protected void ConfigNLog()
		{
			Layout layout = new JsonLayout
			{
				IncludeEventProperties = true
			};
			((JsonLayout)layout).Attributes.Add(new JsonAttribute("message", "${longdate} | ${level} | ${logger}: ${message} ${exception:format=tostring}"));

			try
			{
				var config = LogManager.Configuration;

				var filePath = EDIDataRegistry.Instance.MyAccountLoggerFileTargetPath.Value;
				if (!string.IsNullOrEmpty(filePath))
				{
					var fileLogTarget = new FileTarget();
					fileLogTarget.Name = "file";
					fileLogTarget.FileName = filePath;
					fileLogTarget.Layout = layout;
					var asyncFileTarget = new AsyncTargetWrapper(fileLogTarget)
					{
						Name = fileLogTarget.Name,
						QueueLimit = 50,
						OverflowAction = AsyncTargetWrapperOverflowAction.Discard
					};
					config.AddRule(LogLevel.Info, LogLevel.Fatal, asyncFileTarget, "*");
				}

				var topic = EDIDataRegistry.Instance.MyAccountLoggerKafkaTargetTopic.Value;
				var brokers = EDIDataRegistry.Instance.MyAccountLoggerKafkaTargetBrokers.Value;
				if (!string.IsNullOrEmpty(topic) && !string.IsNullOrEmpty(brokers))
				{
					var brokerAddresses = brokers.Split(',');
					var caFile = MapPath("~/WebApi/Helpers/Certificate/Kafka.pem");
					var kafkaTarget = KafkaLoggingTarget.GetSslTarget("kafka", topic, brokerAddresses, layout, caFile);
					var asyncKafkaTarget = new AsyncTargetWrapper(kafkaTarget)
					{
						Name = kafkaTarget.Name,
						QueueLimit = 50,
						OverflowAction = AsyncTargetWrapperOverflowAction.Discard
					};
					config.AddRule(LogLevel.Info, LogLevel.Fatal, asyncKafkaTarget, "*");
				}

				LogManager.Configuration.Reload();
				LogManager.ReconfigExistingLoggers();
			}
			catch (DatabaseUpgradedException)
			{
			}
			catch (Exception ex)
			{
				ErrorReporter.ReportOnce("Enterprise.ZClientWebCargoWiseEDI.Global.ConfigNLog", ex.Message, ex);
			}
		}

		#endregion

		#region Update web.config

#if DEBUG
		protected virtual
#endif
		void UpdateWebConfig()
		{
			try
			{
				if (EDIDataRegistry.Instance.EnableMyAccountWebConfigOveridden.Value)
				{
					var formAuthDomain = EDIDataRegistry.Instance.MyAccountFormAuthenticationCookieDomain.Value;
					var formAuthSameSite = EDIDataRegistry.Instance.MyAccountFormAuthenticationCookieSameSite.Value;
					var formAuthSSL = EDIDataRegistry.Instance.MyAccountFormAuthenticationCookieSSL.Value;

					var webConfig = OpenWebConfiguration();
					if (webConfig != null)
					{
						var authenticationSection = (AuthenticationSection)webConfig.GetSection("system.web/authentication");
						var formsAuthentication = authenticationSection?.Forms;
						if (formsAuthentication != null)
						{
							bool hasChanges = false;

							if (formsAuthentication.Domain != formAuthDomain)
							{
								formsAuthentication.Domain = formAuthDomain;
								hasChanges = true;
							}

							if (!string.IsNullOrEmpty(formAuthSameSite))
							{
								SameSiteMode sameSiteMode = SameSiteMode.Lax;
								switch (formAuthSameSite)
								{
									case "Lax":
										sameSiteMode = SameSiteMode.Lax;
										break;
									case "None":
										sameSiteMode = SameSiteMode.None;
										break;
									case "Strict":
										sameSiteMode = SameSiteMode.Strict;
										break;
									default:
										break;
								}

								if (formsAuthentication.CookieSameSite != sameSiteMode)
								{
									formsAuthentication.CookieSameSite = sameSiteMode;
									hasChanges = true;
								}
							}

							if (!string.IsNullOrEmpty(formAuthSSL))
							{
								var requireSSL = false;
								switch (formAuthSSL)
								{
									case "True":
										requireSSL = true;
										break;
									case "False":
										requireSSL = false;
										break;
									default:
										break;
								}

								if (formsAuthentication.RequireSSL != requireSSL)
								{
									formsAuthentication.RequireSSL = requireSSL;
									hasChanges = true;
								}
							}

							if (hasChanges)
							{
								webConfig.Save();
							}
						}
					}
				}
			}
			catch (DatabaseUpgradedException)
			{ }
			catch (Exception ex)
			{
				var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
				if (EdiProdDbHelper.IsRunningOnEdiProdDatabase && registrationKey.ServerCode != "UAT" &&
					registrationKey.DatabaseType != "TST")
				{
					ErrorReporter.ReportOnce("Unable to update web.config", ex);
				}
			}
		}

#if DEBUG
		protected virtual
#endif
		Configuration OpenWebConfiguration()
		{
			return WebConfigurationManager.OpenWebConfiguration("~");
		}

		#endregion

		#region Session Event Handler

		protected override void Session_Start(object sender, EventArgs e)
		{
			if (HttpContext.Current.Request.IsAuthenticated)
			{
				base.Session_Start(sender, e);
			}
			else
			{
				using (Db.DisposableActionForDbConnection())
				{
					if (RequestUrl.LocalPath.IndexOf(ErrorPage) == -1)
					{
						try
						{
							if (!DBVersionMatches)
							{
								RedirectToDBVersionPage();
							}
							else
							{
								SetupSiteUser();
							}
						}
						catch (HttpException ex)
						{
							if (ex.ErrorCode == unchecked((int)0x800704CD))//silently ignore 'An error occurred while communicating with the remote host' - root cause is unavoidable (request forcibly closed ie: browser closed or redirected, terminating socket).
							{
								HttpContext.Current.Session.Abandon();
							}
							throw;
						}
					}
					else
					{
						HttpContext.Current.Session.Abandon();
					}
				}
			}
		}

		protected override void Session_End(object sender, EventArgs e)
		{
			if (Session != null && EnableSingleActiveUserSession)
			{
				var heartBeatManager = new MyAccountHeartBeatManager(Session);
				heartBeatManager.DisposeCurrentLoginContext();
			}

			base.Session_End(sender, e);
		}

		#endregion

		#region Application Event Handler

		protected override void Application_AuthenticateRequest(object sender, EventArgs e)
		{
			if (HttpContext.Current.User?.Identity?.IsAuthenticated ?? false)
			{
				MyAccountLoginLiteHelper.RefreshCookiesExpire();
			}
		}

		#endregion
	}
}
