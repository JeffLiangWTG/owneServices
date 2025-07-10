using System;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Common;
using Enterprise.ZArchitecture.Web.GlobalBase;
using Enterprise.ZArchitecture.Web.GUI.Cookie;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Security;
using Enterprise.ZArchitecture.Web.Utilities.Environment;
using Enterprise.ZArchitecture.Web.Utilities.Exceptions;
using MailKit.Net.Smtp;
using MimeKit;
#if DEBUG
using System.Collections.Generic;
using CargoWise.Common.Testing;
using Enterprise.ZArchitecture.Modules;
#endif

namespace Enterprise.ZArchitecture.Web.GUI
{
	static class IISAndLeaveMyStaticsAlone
	{
		/// <summary>
		/// every time ASP recompiles the page we loose all static variables. therefore need a separate static class that
		/// will be there for the entire life off app domain.
		/// </summary>
#if DEBUG
		[SuppressThreadStaticFieldMessage] // Suppress because we need this static variable in order to stop spamming clients with WebTracker emails
#endif
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "need to suppress because ASP and IIS not smart enough to keep static values, every time we refresh the page values are dropped.")]
		public static bool HasDBVersionBeenReported;
	}

	/// <summary>
	/// Base application
	/// </summary>
	public abstract class ZGlobal : ZEnterpriseGlobal
	{
		protected virtual void OnCustomSessionStart(Object sender, EventArgs e)
		{
			base.Session_Start(sender, e);
		}

		public ZApplicationCookie ApplicationCookie
		{
			get
			{
				if (fApplicationCookie == null)
				{
					fApplicationCookie = new ZApplicationCookie(ApplicationCookieName);
				}

				return fApplicationCookie;
			}
		}
		ZApplicationCookie fApplicationCookie;

		public override WebUser GetNewSiteUser() => new OrgContactWebUser();

		protected virtual string ApplicationCookieName
		{
			get { return "CW1Application"; }
		}

		#region Licence Manager

		public IWebAccessManager WebAccessManager
		{
			get
			{
				if (webAccessManager == null)
				{
					webAccessManager = GetNewWebAccessManager();
				}
				return webAccessManager;
			}
		}

		protected virtual IWebAccessManager GetNewWebAccessManager()
		{
			return null;
		}

		IWebAccessManager webAccessManager;

		#endregion

		#region Paths and pages

		public virtual string ApplicationRoot
		{
			get
			{
				return (HttpContext.Current.Request.ApplicationPath != "/") ? HttpContext.Current.Request.ApplicationPath + "/" : HttpContext.Current.Request.ApplicationPath;
			}
		}

		public virtual string BaseStyleSheet
		{
			get { return ApplicationRoot + "BaseStyle.css"; }
		}

		public abstract string DefaultPage { get; }

		public virtual string ErrorPage
		{
			get { return ApplicationRoot + "Error.aspx"; }
		}

		public virtual string HomePage
		{
			get { return GlobalConfig.HomePage; }
		}

		public virtual string MapPath(string path)
		{
			return HttpContext.Current.Server.MapPath(path);
		}

		public virtual string CompanyName
		{
			get { return GlobalConfig.CompanyName; }
		}

		public virtual string LoginPage
		{
			get { return ""; }
		}

		public virtual string LogoImage
		{
			get { return ApplicationRoot + "Images/Logo.gif"; }
		}

		#endregion

		public void SetupEnvironment()
		{
			WebAppEnvironment.Setup();
		}

		public void SignOut()
		{
			SignOut(true);
		}

		public void SignOut(bool redirectToDefaultPage)
		{
			FormsAuthentication.SignOut();
			if (ApplicationCookie.CookieExist())
			{
				ApplicationCookie.Remove();
			}
			SiteUser.Logout();
			Session.Abandon();

			if (redirectToDefaultPage)
			{
				Response.Redirect(DefaultPage, false);
			}
		}

		#region ReportError

		public void ReportError()
		{
			ReportError(null, null);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "SecureQueryString uses DateTime.Now to determine validity of query string, URL Parameters")]
		public virtual void ReportError(string pageTitle, string message)
		{
			if (HttpContext.Current?.Response?.IsClientConnected ?? false)
			{
				if (RequestUrl?.LocalPath?.IndexOf(ErrorPage) > -1)
				{
					RenderSeriousErrorPage(pageTitle, message);
				}
				else
				{
					SecureQueryString qs = new SecureQueryString();

					if (pageTitle != null)
					{
						qs["title"] = pageTitle;
					}

					if (message != null)
					{
						qs["message"] = message;
					}

					qs.ExpireTime = TimeSpan.FromMinutes(10);
					HttpContext.Current.Response.Redirect(ErrorPage + "?data=" + WebUtility.UrlEncode(qs.ToString()));
				}
			}
		}

		#endregion

		#region Global Config

		public ZGlobalConfig GlobalConfig
		{
			get { return globalConfig ?? (globalConfig = GetNewGlobalConfig()); }
		}
		ZGlobalConfig globalConfig;

		protected virtual ZGlobalConfig GetNewGlobalConfig()
		{
			return new ZGlobalConfig();
		}

		#endregion

		#region Application events

		protected virtual bool IsApplicationUserInteractive => false;

		protected override void Application_Start(object sender, EventArgs e)
		{
			base.Application_Start(sender, e);
			ApplicationStart(sender, e);
		}

		[SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
		void ApplicationStart(object sender, EventArgs e)
		{
			Globals.IsUserInteractive = IsApplicationUserInteractive;
			NotificationHandler.Instance = new ZWebNotificationHandler(null);
			WebControls.TranslationFeedbackManager.CleanupAllResourceStringUsageFiles();
			WebExceptionReporter.SuppressExceptionsThatOccurWhilstReportingDeveloperExceptions = true;

			//Load Client specific module in debug mode only
#if DEBUG
			ZString clientDLLToLoad = GlobalConfig.Client;
			if (!clientDLLToLoad.IsEmpty)
			{
				try
				{
					ClientHookLoader.Instance.OverrideClientAssemblyForTest(ClientHookLoader.Instance.GetAssemblyFromFileName(Env.ApplicationStartupPath + "\\Bin\\ZClient" + clientDLLToLoad.ToUpper() + ".dll"));
				}
				catch (Exception exception) // CriticalExceptionIsHandled Reason = Debug.
				{
					throw new ApplicationException("***DEBUG MODE ONLY***\r\nFailed to load ZClient" + clientDLLToLoad + ".dll\r\n\r\nVerify that the Client configuration setting is correct and that the client dll exists.\r\nReported Exception:" + exception.Message);
				}
			}
#endif
			HasApplicationStartedSuccessfully = true;
		}

		protected override BaseExceptionReporter WebExceptionReporter => lazyWebExceptionReporter.Value;
		readonly Lazy<WebExceptionReporter> lazyWebExceptionReporter = new Lazy<WebExceptionReporter>(() => new WebExceptionReporter(TopLevelWebExceptionHandler.LazyInstance.Value));

#if DEBUG
		[SuppressThreadStaticFieldMessage]
#endif
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "It is a global flag which value should be shared between threads")]
		protected static bool HasApplicationStartedSuccessfully = false;

		public void SetupSiteUser()
		{
			HttpContext.Current.Session.Add(SiteUserSessionKey, GetNewSiteUser());
		}

		public void SetupSession(object sender, EventArgs e, bool shouldRecreateSiteUser)
		{
			try
			{
				using (Db.DisposableActionForDbConnection())
				{
					if (RequestUrl.LocalPath.IndexOf(ErrorPage) == -1)
					{
						try
						{
							if (DBVersionMatches)
							{
								SetupEnvironment();
							}
							else
							{
								RedirectToDBVersionPage();
							}

							if (SiteUser == null || shouldRecreateSiteUser)
							{
								OnCustomSessionStart(sender, e);
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

						if (!hasPopulatedWebRegistry)
						{
							lock (webRegistryLock)
							{
								if (!hasPopulatedWebRegistry)
								{
									hasPopulatedWebRegistry = true;
									PopulateWebRegistry(HttpContext.Current.Request.Url.Host);
								}
							}
						}
					}
					else
					{
						HttpContext.Current.Session.Abandon();
					}
					if (HttpContext.Current.Session != null)
					{
						var sessionId = HttpContext.Current.Session.SessionID; // flush might occur before the sessionID is used, So I'm poking it first. http://tinyurl.com/6nxky97
						HttpContext.Current.Session[ClickJackingProtectionModule.SessionKeyClickJackingProtectionDisabled] = WebDataRegistry.Instance.AllowInlineFrames.Value;
					}
				}
			}
			catch (Exception ex)
			{
				if (ExceptionShouldBeHandled(ex) && !HandleException(ex))
				{
					WebExceptionReporter.HandleUnhandledException(ex);
				}
			}
		}

		protected override void Session_Start(object sender, EventArgs e)
		{
			SetupSession(sender, e, true);
		}

#if DEBUG
		public
#else
		protected
#endif
 virtual void PopulateWebRegistry(string url)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var factory = new BusinessObjectFactory();
				if (DataRegistry.Instance.WebBranch == Guid.Empty)
				{
					var result = factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, ConfigurationManager.AppSettings[ZGlobalConfig.BranchKey]);
					if (result != null)
					{
						DataRegistry.Instance.WebBranch = result.PK.ToGuid();
					}
				}
			}
		}

		static bool hasPopulatedWebRegistry;
		static readonly Object webRegistryLock = new Object();

		protected override void Session_End(object sender, EventArgs e)
		{
			base.Session_End(sender, e);
			WebControls.TranslationFeedbackManager.CleanupResourceStringUsageFiles(Session.SessionID);
		}

		public bool IsDefaultPageURL(string url)
		{
			return string.IsNullOrEmpty(url) || url.ToLower().Replace("/", "") == ApplicationRoot.ToLower().Replace("/", "");
		}

		[SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", Justification = "Checking if it is well-formed here rather than depending on callers to do so")]
		[SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings")]
		public virtual string GetValidRedirectURL(string url)
		{
			if (IsDefaultPageURL(url) || !IsLocalUrl(url) || url.Contains("../"))
			{
				return DefaultPage;
			}
			return url;
		}

		bool IsLocalUrl(string url)
		{
			// From https://docs.microsoft.com/en-us/aspnet/mvc/overview/security/preventing-open-redirection-attacks
			if (string.IsNullOrEmpty(url))
			{
				return false;
			}
			else
			{
				return (url[0] == '/' && (url.Length == 1 || (url[1] != '/' && url[1] != '\\'))) || // "/" or "/foo" but not "//" or "/\"
					(url.Length > 1 && url[0] == '~' && url[1] == '/');   // "~/" or "~/foo"
			}
		}

		protected
#if DEBUG
 virtual
#endif
		Uri RequestUrl
		{
			get { return HttpContext.Current?.Request?.Url; }
		}

		protected override void Application_BeginRequest(Object sender, EventArgs e)
		{
			if (RequestUrl == null || RequestUrl?.LocalPath?.IndexOf(ErrorPage) == -1)
			{
				base.Application_BeginRequest(sender, e);
			}

			if (!HasApplicationStartedSuccessfully)
			{
				lock (ApplicationStartLock)
				{
					if (!HasApplicationStartedSuccessfully)
					{
						ApplicationStart(sender, e);
					}
				}
			}

			using (Db.DisposableActionForDbConnection())
			{
				try
				{
					RefreshRegistryItemCache();

					Server.ScriptTimeout = IsWarmupUserAgent ? OneHourInSeconds : WebDataRegistry.Instance.RequestTimeout.Value;

					if (!Globals.IsTest)
					{
						AssemblyLoader.Instance = new WebAssemblyLoader();
					}

					try
					{
						if (Request.Path.IndexOf('\\') >= 0 || System.IO.Path.GetFullPath(Request.PhysicalPath) != Request.PhysicalPath)
						{
							throw new HttpException(404, "not found");
						}
					}
					catch (ArgumentException)
					{
						throw new HttpException(404, "not found");
					}

					if (IsDefaultPageURL(Request.Path))
					{
						Response.Redirect(DefaultPage, false);
						Context.ApplicationInstance.CompleteRequest();

						return;
					}

					if (RequestUrl.LocalPath.IndexOf(ErrorPage) == -1)
					{
						// Check that WebConfiguration is OK
						if (!ConfigurationOK)
						{
							SendEmail(Res.GetString("052f0c80-f060-43fb-8918-008184c4e323", "Web site {0} configuration error", GlobalConfig.HomePage), GlobalConfig.ConfigurationError);
							ReportError(Res.GetString("aa6115d3-c6ac-4cb0-a71b-3748fb7c7602", "Configuration Error"), Res.GetString("cf6187cc-879d-4828-a4ec-fc77fe290ce4", "The site is currently experiencing a misconfiguration. We regret any inconvenience caused. Please try again later.<br />{0}", String.Join("<br />", GlobalConfig.ConfigurationError.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.None))));
						}

						OnCustomApplicationBeginRequest(sender, e);
					}
					else
					{
						if (!ErrorPageOK)
						{
							throw new HttpException(404, "Unable to display detailed error message. Please contact server administrator. We regret any inconvenience caused. Please try again later.");
						}
					}
				}
				catch (Exception ex)
				{
					if (ExceptionShouldBeHandled(ex) && !HandleException(ex))
					{
						WebExceptionReporter.HandleUnhandledException(ex);
					}
				}
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1050:UseSystemTimeSpanTypeForDuration", Justification = "Baseline")]
		const int OneHourInSeconds = 3600;

		readonly static object ApplicationStartLock = new object();

		/// <summary>
		/// You should override this method for all custom error processing on Begin request
		/// </summary>
		protected virtual void OnCustomApplicationBeginRequest(Object sender, EventArgs e) { }

		#region DB version check

		protected internal bool DBVersionMatches
		{
			get
			{
				try
				{
					var result =
						SchemaVersion.Application.CompareTo(Env.Registry.DatabaseMajorSchemaVersion, Env.Registry.DatabaseMinorSchemaVersion) == 0
						&& DataVersion.Application.CompareTo(Env.Registry.DatabaseSystemDataVersionMajor, Env.Registry.DatabaseSystemDataVersionMinor) == 0
						&& ClientDllChecker.CheckRegistry().IsOKToRun;

					HasDBVersionBeenReported &= !result;
					return result;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
				}

				return false;
			}
		}

		ZDateTime dbMismatchErrorTime = ZDateTime.MinSmallDateTimeValue;

		void ReportDBMismatchError()
		{
			if (!HasDBVersionBeenReported)
			{
				var now = ZDateTime.Now;
				if (dbMismatchErrorTime == ZDateTime.MinSmallDateTimeValue)
				{
					dbMismatchErrorTime = now;
					return;
				}

				if (dbMismatchErrorTime > now.AddMinutes(-1))
				{
					return;
				}

				dbMismatchErrorTime = now;

				string hostnameToResolve;
				if (Db.ServerName.IndexOf("\\") >= 0)
				{
					hostnameToResolve = Db.ServerName.Substring(0, Db.ServerName.IndexOf("\\"));
				}
				else
				{
					hostnameToResolve = Db.ServerName;
				}

				String resolvedDBServerHostName;
				try
				{
					resolvedDBServerHostName = System.Net.Dns.GetHostEntry(hostnameToResolve).HostName;
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					resolvedDBServerHostName = hostnameToResolve;
				}

				Exception ex = new ApplicationException(
					"Web Application error on " + Server.MachineName +
					": Database " + Db.DatabaseName + " on server " + resolvedDBServerHostName +
					" requires upgrading (" + GetEnvironmentText() + "). " +
					GetVersionMismatchText("schema", SchemaVersion.Application, Env.Registry.DatabaseMajorSchemaVersion, Env.Registry.DatabaseMinorSchemaVersion) +
					GetVersionMismatchText("data", DataVersion.Application, Env.Registry.DatabaseSystemDataVersionMajor, Env.Registry.DatabaseSystemDataVersionMinor) +
					(ClientDllChecker.CheckRegistry().IsOKToRun ? "" : ClientDllChecker.CheckRegistry().ErrorMessage));

				HasDBVersionBeenReported = SendEmail(Res.GetString("b584a871-f9fe-40ed-94ed-75a44b0cd933", "Database version error"), ex.ToString());
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "not translating this error message")]
		static string GetEnvironmentText()
		{
			string text = "AppPool " + System.Environment.UserName;
			try
			{
				text += ", process " + System.Diagnostics.Process.GetCurrentProcess().Id;
			}
			catch (Exception e) when (!e.IsCriticalException()) { }
			try
			{
				if (HttpContext.Current != null && HttpContext.Current.Server != null)
				{
					text += ", folder " + HttpContext.Current.Server.MapPath("~");
				}
			}
			catch (Exception e) when (!e.IsCriticalException()) { }

			return text;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "not translating this error message")]
		static string GetVersionMismatchText(string versionType, VersionLabel expected, int actualMajor, int actualMinor)
		{
			return expected.CompareTo(actualMajor, actualMinor) != 0
				? "Expected " + versionType + " version " + expected + ", found " + actualMajor + "." + actualMinor + ". "
				: "";
		}

		static bool HasDBVersionBeenReported
		{
			get { return IISAndLeaveMyStaticsAlone.HasDBVersionBeenReported; }
			set { IISAndLeaveMyStaticsAlone.HasDBVersionBeenReported = value; }
		}

		protected internal bool SendEmail(string title, string message, bool isHtml = false)
		{
			bool result = false;

			using (var smtp = new SmtpClient())
			{
				try
				{
					var returnAddress = Env.Registry.SMTPDefaultReturnEmailAddress;
					if (String.IsNullOrEmpty(returnAddress))
					{
						returnAddress = Env.Registry.MailboxEmailAddress;
					}

					var bodyBuilder = new BodyBuilder();
					var mimeMessage = new MimeMessage();

					if (isHtml)
					{
						bodyBuilder.HtmlBody = message;
					}
					else
					{
						bodyBuilder.TextBody = message;
					}
					mimeMessage.Body = bodyBuilder.ToMessageBody();

					if (InternetAddress.TryParse(returnAddress, out var fromAddress))
					{
						mimeMessage.From.Add(fromAddress);
					}
					mimeMessage.Subject = title;

					var recipients = EnvProxy.IsHostedWithCargowise ?
						new EmailGroupUtility().GetHostedNotificationsEmailOverride() :
						new EmailGroupUtility().GetGroupEmailCollection(WebDataRegistry.Instance.WebAdminsEmailNotificationGroup.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty), false);
					if (!string.IsNullOrEmpty(Env.Registry.EmailDestinationOverride))
					{
						if (InternetAddress.TryParse(Env.Registry.EmailDestinationOverride, out var toAddress))
						{
							mimeMessage.To.Add(toAddress);
						}
						var originalRecipientsMessage = Res.GetString("b5b296ba-d9c8-4d5c-8452-1696a57434c2",
							"(This message was redirected to {0} as it was sent from a non-production system. Originally the email was addressed to {1}.)",
							Env.Registry.EmailDestinationOverride, string.Join(", ", recipients.Cast<string>()));
						if (isHtml)
						{
							bodyBuilder.HtmlBody = message + "<br />" + originalRecipientsMessage;
						}
						else
						{
							bodyBuilder.TextBody = message + System.Environment.NewLine + originalRecipientsMessage;
						}
						mimeMessage.Body = bodyBuilder.ToMessageBody();
					}
					else
					{
						foreach (var recipient in recipients)
						{
							if (InternetAddress.TryParse(recipient, out var toAddress))
							{
								mimeMessage.To.Add(toAddress);
							}
						}
					}

#if DEBUG
					if (Globals.IsTest)
					{
						EmailsNotSentBecauseInTestMode.Add(mimeMessage);
					}
					else
#endif
					{
						smtp.Connect(Env.Registry.SMTPServer, Env.Registry.SMTPPort);
						var user = Env.Registry.SMTPUsername;
						var password = Env.Registry.SMTPPassword;
						if (!String.IsNullOrEmpty(user) && !String.IsNullOrEmpty(password))
						{
							smtp.Authenticate(user, password);
						}
						smtp.Send(mimeMessage);
						smtp.Disconnect(true);
					}
					result = true;
				}
				catch (DatabaseUpgradeInProgressException) { } // this is fine.
				catch (Exception e) when (!e.IsCriticalException())
				{
					//If the Email sender dies (SMTP is not configured or any other error, web site user is not going to see an exception
				}
			}
			return result;
		}

#if DEBUG
		internal List<MimeMessage> EmailsNotSentBecauseInTestMode = new List<MimeMessage>();
#endif

		protected internal void RedirectToDBVersionPage()
		{
#if DEBUG
			if (Globals.IsTest)
			{
#endif
				using (Db.DisposableActionForDbConnection())
				{
					if (!EnvProxy.IsHostedWithCargowise && !DataUtils.IsWiseTechGlobalDatabaseServer(Db.Connection))
					{
						ReportDBMismatchError();
					}
				}
#if DEBUG
			}
#endif
			ReportError(Res.GetString("43c09fbe-9916-4dd1-aa6b-960b7e6e2983", "Under Maintenance"), Res.GetString("1b6808e1-c3df-4f24-b564-8e23a51e58aa", "This site is currently under maintenance. We regret any inconvenience caused. Please try again later."));
			if (HttpContext.Current != null && HttpContext.Current.Session != null)
			{
				HttpContext.Current.Session.Abandon();
			}
		}

		#endregion

		/// <summary>
		/// Prevents 404 errors from being mailed to EDI
		/// </summary>
		[SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		protected override void Application_Error(object sender, EventArgs e)
		{
			var lastError = Server.GetLastError();
			var unhandledException = lastError.GetBaseException();

			if (ExceptionShouldBeHandled(unhandledException) && !HandleException(unhandledException))
			{
				base.Application_Error(sender, e);
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "I have only found a way to identify this exception by the message, Microsoft exception message, This is the only other way to identify the exception, Some formatting to separate out message text from other details, to distinguish it from other InvalidOperationExceptions that may occur, App isn't a real word so we have to put it outside of the resource string, to find out the access issue on the path, This is a url")]
		bool HandleException(Exception unhandledException)
		{
			if (unhandledException is RegistryJsonException registryJsonException)
			{
				Server.ClearError();

				if (RequestUrl.LocalPath.IndexOf(ErrorPage) > -1)
				{
					var message = Res.GetString("bec7103c-d321-43e3-a06f-ed36945410e7",
						"The value for registry item ({0} - {1}) contains invalid JSON. (Maybe you copied data from an old database and it missed the data transformation that converted it from Binary to JSON format.) Fix or delete and re-enter it.",
						registryJsonException.RegistryName,
						registryJsonException.RegistryCaption);
					SendEmail(Res.GetString("fbce00ed-9138-49ba-ab11-13212e45ce35", "Invalid JSON Registry!"), message);
				}
				ReportError(Res.GetString("fbce00ed-9138-49ba-ab11-13212e45ce35", "Invalid JSON Registry!"),
					Res.GetString("8f90b09d-7942-49a6-a55a-d691cba9ca06",
						"The value for registry item ({0} - {1}) contains invalid JSON. Please contact your system administrator.",
						registryJsonException.RegistryName, registryJsonException.RegistryCaption));

				return true;
			}
			else if (unhandledException is FileNotFoundException)
			{
				if (RequestUrl.LocalPath.IndexOf(ErrorPage) == -1)
				{
					ReportError(Res.GetString("4f7babdb-e5b3-47e1-9b35-93dae7566242", "Page not found"), Res.GetString("f40acdfa-d126-4b5c-a3fe-80e8170fe9bf", "The page you requested was not found."));
				}
				else
				{
					RenderSeriousErrorPage(Res.GetString("aa5057a7-3070-419f-be36-1bd3aee5231f", "Web Application Error"), Res.GetString("82d00265-7a8a-45bd-918c-eb2b6fb0df4a", "The Web Application you attempted to access is currently unavailable"));
					Server.ClearError();
				}

				return true;
			}
			else if (unhandledException is ViewStateException)
			{
				ReportError(Res.GetString("A8A3F801-93A7-47F3-A08E-3C5457495507", "Logged Off"), Res.GetString("8AA76F7A-0B09-4F0A-BB07-00CA040A3E55", "You logged off in another window."));
				return true;
			}
			else if (unhandledException is SqlException)
			{
				ReportError(Res.GetString("aa5057a7-3070-419f-be36-1bd3aee5231f", "Web Application Error"), Res.GetString("82d00265-7a8a-45bd-918c-eb2b6fb0df4a", "The Web Application you attempted to access is currently unavailable"));
				HttpContext.Current.Response.End();
				return true;
			}
			else if (unhandledException is HttpException && unhandledException.Message.Contains("Request timed out"))
			{
				string m1 = Res.GetString("85c76cd0-0387-41cb-96df-dc1d0ce8ca9a", "Your request has timed out.");
				string m2 = string.Empty;
				if (Session[SearchControl.SearchControlIsSearchingIndexer] != null && (bool)Session[SearchControl.SearchControlIsSearchingIndexer])
				{
					Session.Remove(SearchControl.SearchControlIsSearchingIndexer);
					m2 = " " + Res.GetString("b4c85473-4468-4198-acc6-2af149bbedc5", "Please try to add more filtering to make your search more specific.");
				}
				string message = m1 + m2;
				string path = HttpContext.Current.Request.UrlReferrer == null ? string.Empty : HttpContext.Current.Request.UrlReferrer.AbsoluteUri;

				if (!string.IsNullOrEmpty(path))
				{
					Session[ZPage.zPageCustomAlertMessageIndexer] = message;
					Response.Redirect(path);
				}
				else
				{
					ReportError(Res.GetString("9c75e26d-ea09-4349-b9d4-483b273c6c15", "Request Timed Out"), message);
					HttpContext.Current.Response.End();
				}
				return true;
			}
			else if (unhandledException is HttpException && unhandledException.Message.Contains("is in the special directory"))
			{
				ReportError(Res.GetString("4f7babdb-e5b3-47e1-9b35-93dae7566242", "Page not found"), Res.GetString("f40acdfa-d126-4b5c-a3fe-80e8170fe9bf", "The page you requested was not found."));
				return true;
			}
			else if (unhandledException is InvalidOperationException
				&& unhandledException.Message.Contains("Timeout expired.")
				&& unhandledException.Message.Contains("The timeout period elapsed prior to obtaining a connection from the pool."))
			{
				var userVisibleMessage = Res.GetString("6C4B0562-0B16-4FBD-A5BB-CDA5F9866144", "All pooled database connections are in use, please try again later or contact us if the problem persists.", unhandledException.Message);
				ReportError(Res.GetString("879E12F3-9DE6-451E-9C85-9F03C780013E", "DB connection error"), userVisibleMessage);
				return true;
			}
			else if (unhandledException is InvalidOperationException
				&& ((unhandledException.Message.Contains("Operation is not valid due to the current state of the object."))
				|| (unhandledException.StackTrace != null && unhandledException.StackTrace.Contains("System.Web.HttpValueCollection.ThrowIfMaxHttpCollectionKeysExceeded()"))))
			{
				SendEmail(Res.GetString("0766AB53-D88B-485B-A105-575CC8A29413", "Web server needs to upgrade to Latest .Net framework  version"),
					Res.GetString("FA77EDDC-030B-46ED-AE48-C3066D602F75", "The web form has exceeded its limit of 1000 items to handle. This is due to a limitation of older versions of the .NET Framework. Please upgrade the web server to the latest .NET Framework version."));
				return true;
			}
			else if (unhandledException is InvalidOperationException && ((unhandledException.Message.Contains("HRESULT: 0x80070032"))))
			{
				var userVisibleMessage = Res.GetString("1AE80FB7-BEC4-45CA-9B60-BFD9D2579CE7", "The application has encountered a proxy error, HRESULT: 0x80070032. Please contact your administrator.");
				ReportError(Res.GetString("A56BE533-AAF6-4DA6-8221-72945706A39D", "Proxy Error, HRESULT: 0x80070032"), userVisibleMessage);
				return true;
			}
			else if (unhandledException is System.ServiceModel.EndpointNotFoundException)
			{
				string htmlErrorInfo = Res.GetString("052f0c80-f060-43fb-8918-008184c4e323", "Web site {0} configuration error", GlobalConfig.HomePage);
				htmlErrorInfo += "<br/><br/><b>Error message:</b> " + unhandledException.Message + "<br/><br/><hr/><br/>";
				htmlErrorInfo += ExceptionToHtmlTable(unhandledException);

				SendEmail(Res.GetString("052f0c80-f060-43fb-8918-008184c4e323", "Web site {0} configuration error", GlobalConfig.HomePage), htmlErrorInfo, true);

				var userVisibleMessage = Res.GetString("cf6187cc-879d-4828-a4ec-fc77fe290ce4", "The site is currently experiencing a misconfiguration. We regret any inconvenience caused. Please try again later.<br />{0}", String.Join("<br />", GlobalConfig.ConfigurationError.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.None)));
				ReportError(Res.GetString("aa6115d3-c6ac-4cb0-a71b-3748fb7c7602", "Configuration Error"), userVisibleMessage);
				return true;
			}
			else if (ExceptionVisibilityAttribute.Evaluate(unhandledException) == ExceptionVisibility.User)
			{
				var userVisibleMessage = Res.GetString(
					"ef599ab1-4439-4688-820a-ceb620e89869",
					"An error has occurred and we couldn't process your request, please try again or contact us if the problem persists.<br><br>{0}",
					unhandledException.Message);
				ReportError(Res.GetString("71e1a9a3-fc21-40cb-a190-295da86c35f2", "Error"), userVisibleMessage);
				return true;
			}
			else if (unhandledException is ArgumentNullException && unhandledException.Message.Contains("Value cannot be null."))
			{
				ReportError(Res.GetString("aa5057a7-3070-419f-be36-1bd3aee5231f", "Web Application Error"), Res.GetString("82d00265-7a8a-45bd-918c-eb2b6fb0df4a", "The Web Application you attempted to access is currently unavailable"));
				return true;
			}
			else if (unhandledException is System.Security.Cryptography.CryptographicException)
			{
				ReportError(Res.GetString("aa5057a7-3070-419f-be36-1bd3aee5231f", "Web Application Error"), unhandledException.Message);
				return true;
			}
			else if (unhandledException is InvalidOperationException && unhandledException.Message.StartsWith("Failed to map the path"))
			{
				SendEmail(Res.GetString("8bd6842c-0e9f-4342-8d42-7a12302747f2", "Intermittent web server error"),
					Res.GetString("c4d2ce92-b35c-44f4-b499-d18c52685f30", "The following exception was encountered by a user: {0}.\r\nThis is due to one of two reasons: 1) The {1} inside of IIS is not correctly authorized. 2) An intermittent error in IIS, which can be fixed by restarting IIS.", unhandledException.Message, "App Pool identity"));

				var userVisibleMessage = Res.GetString("06affebd-706f-4820-9bd0-7c83d57f0bdf", "An intermittent error has occurred. Please try again in a few minutes. If problems persist, contact web hosting.");
				ReportError(Res.GetString("ea3f9a42-8d84-4aac-bba3-088c58dbe241", "Intermittent Error"), userVisibleMessage);
				return true;
			}
			else if (unhandledException is UnauthorizedAccessException && unhandledException.Message.StartsWith("Access to the path") && unhandledException.Message.EndsWith("denied."))
			{
				SendEmail(Res.GetString("89f479f6-078b-41c7-a71a-b41e0e94445a", "Access on Path Denied Error"),
					 Res.GetString("9496b6a7-ff42-48fc-9543-64875b6e9099", "The following exception occurred {0}.<br/>The web application doesn't have enough permission on the path<br/><br/>Web site:{1}<br/><br/>{2}", unhandledException.ToString(), GlobalConfig.HomePage, ExceptionToHtmlTable(unhandledException)), true);

				var userVisibleMessage = Res.GetString("a40cd3e6-2759-4d35-ba79-50e54f266b13", "The application has encountered permission error. Please contact your administrator.");
				ReportError(Res.GetString("452be4c8-c088-457a-901a-05b9a52de9a1", "Permission error on system"), userVisibleMessage);
				return true;
			}
			else if (unhandledException is IOException && unhandledException.Message.StartsWith("The process cannot access the file") && unhandledException.Message.EndsWith("because it is being used by another process."))
			{
				ReportError(Res.GetString("d6187351-0238-4d46-a221-587c9b94d745", "File Access Error"), Res.GetString("ba7525d6-b2be-4f9c-8171-66be26dcd1be", "The web application cannot access an internal file because it is being used by another process. Please Contact your Web Administrator to Restart the IIS."));
				return true;
			}
			else if (unhandledException is HttpException && Regex.IsMatch(unhandledException.Message, "The file '.+\\/Runtime\\/.+' does not exist."))
			{
				ReportError(Res.GetString("3cb84d48-f3d0-4157-81a6-97a40865d89a", "Runtime file extraction error"), Res.GetString("55174815-9476-447d-bc5c-83f9cff3afe9", "The web application cannot access a file that was extracted by the runtime. This is very likely due to the extraction path becoming too long: '{0}'.", unhandledException.ToString()));
				return true;
			}
			else if (unhandledException is Win32Exception && (((Win32Exception)unhandledException).NativeErrorCode == Win32_WAIT_TIMEOUT || ((Win32Exception)unhandledException).NativeErrorCode == Win32_ERROR_LOGON_FAILURE))
			{
				SendEmail(unhandledException.Message, String.Format(CultureInfo.CurrentCulture, Res.GetString("37799AF7-415F-4423-A6AA-14FA963FC5EE", "The application has encountered a Exception with Error Code: {0}. Please visit {1} to find out more about this error."), ((Win32Exception)unhandledException).NativeErrorCode, "https://msdn.microsoft.com/en-us/library/windows/desktop/ms681382(v=vs.85).aspx"));
				return true;
			}
			else if (unhandledException is UriFormatException)
			{
				var referrer = Res.GetString("ab89aae3-5a22-4cfe-a85d-bd6a7c4f77dc", "Request Referrer: ");
				Func<bool, string> exceptionMessage = (isHtml) =>
				{
					string separator = isHtml ? "<br /><br /><hr /><hr /><br />" : "\r\n";
					string result = unhandledException.Message;
					try
					{
						result = string.Join(separator, result, referrer + (HttpContext.Current.Request.UrlReferrer == null ? string.Empty : HttpContext.Current.Request.UrlReferrer.AbsoluteUri));
					}
					catch (UriFormatException)
					{
						//If the HTTP Referer request header is malformed and cannot be converted to a System.Uri, web site user is not going to see an exception
					}
					return result;
				};
				SendEmail(Res.GetString("6fb5340e-bdb0-4b3c-9c7f-19bbc37044e1", "Web Application Error"), exceptionMessage(true), true);
				ReportError(Res.GetString("6640fde1-b69f-41d3-910a-d02f7cb10489", "Uri Format Error"), exceptionMessage(false));
				return true;
			}
			else if (unhandledException is IOException && unhandledException.IsOutOfDiskSpaceException())
			{
				var errCaption = Res.GetString("2B8D3FC8-02E5-4FE9-B6A3-83379CE9329F", "Out of Disk Space Error");
				var errMsg = Res.GetString("84487189-F634-4731-84DF-FE518F4A528F", "There is not enough space on the disk or you have exceeded your quota. Please contact your system administrator.");

				ReportError(errCaption, errMsg);
				SendEmail(errCaption, errMsg + unhandledException);
				return true;
			}
			else if (unhandledException is OutOfMemoryException)
			{
				var errCaption = Res.GetString("ABC5A9AF-09A0-450D-912E-9E0D8B705085", "Out of Memory Error");
				var errMsg = Res.GetString("23832FBE-53DA-43C0-A8FB-9C214F52CF24", "An error has occurred because this website is running low on memory. ({0} MB left out of {1} MB) Please contact your system administrator."
					, ZSystemInformation.Instance.AvailableVirtualMemory, ZSystemInformation.Instance.TotalVirtualMemory);

				ReportError(errCaption, errMsg);
				SendEmail(errCaption, errMsg + unhandledException);
				return true;
			}
			else if (unhandledException is NullReferenceException && unhandledException.Source == typeof(Brettle.Web.NeatUpload.UploadHttpModule).Namespace)
			{
				ReportError(Res.GetString("8a80fdd2-9723-495a-afe6-d1ee6002f605", "File Upload Error"), Res.GetString("6f51a15a-c539-4a6a-bce3-36a6357b87d1", "An error occurred while processing this request. Please try again or contact your Administrator if the error persists."));
				return true;
			}

			return unhandledException.IsIgnorable();
		}

		#region

		const int Win32_WAIT_TIMEOUT = 258;
		const int Win32_ERROR_LOGON_FAILURE = 1326;
		const int Win32_ERROR_BAD_NETPATH = 53;
		const int Win32_ERROR_NETNAME_DELETED = 64;
		const int Win32_WRITE_PROTECT = 19;

		#endregion

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "html formatting")]
		static string XmlToHtmlTable(string xml)
		{
			StringBuilder html = new StringBuilder("<table align='left' " + "border=0 cellspacing=0 cellpadding=1 class='xmlTable'>\r\n");
			XDocument xDocument = XDocument.Parse(xml);
			XElement root = xDocument.Root;
			var xmlAttributeCollection = root.Elements().Attributes();
			foreach (var el in root.Elements())
			{
				string elename = el.Name.ToString();
				html.Append("<tr><td><b>" + elename + "</b></td>");
				if (!el.HasElements)
				{
					html.Append("<td>" + el.Value + "</td>");
				}
				else
				{
					html.Append("<td>" + XmlToHtmlTable(el.ToString()) + "</td>");
				}
				html.Append("</tr>");
			}
			html.Append("</table>");
			return html.ToString();
		}

		static string ExceptionToHtmlTable(Exception ex)
		{
			var webexp = new WebExceptionDetails(ex);
			var strWebInfo = new StringWriter(CultureInfo.InvariantCulture);
			webexp.WriteWebInfo(new XmlTextWriter(strWebInfo));
			return XmlToHtmlTable(strWebInfo.GetStringBuilder().ToString());
		}

		[SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
		[SuppressMessage("CargoWiseOne", "CW1054:DoNotHardcodePaths", Justification = "Exception message we dont want to send")]
		[SuppressMessage("CargoWiseOne", "CW1079:DoNotCompareOnExceptionMessage", Justification = "Exception message we dont want to send")]
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Known workaround for ASP.net version check errors, see http://support.microsoft.com/default.aspx?scid=kb;en-us;825792, Exception message we dont wont to send, Exception message we dont want to send, Local path, Exception message. This can happen for requests during IIS startup, Url path, Exception message, Exception messages")]
		protected virtual bool ExceptionShouldBeHandled(Exception unhandledException)
		{
			if (Db.IsDatabaseUpgraded && unhandledException is ConfigurationErrorsException)
			{
				return false;
			}
			if (unhandledException.Message?.IndexOf("get_aspx_ver.aspx") > -1)
			{
				return false;
			}
			else if (unhandledException is FormatException)
			{
				if (unhandledException.Message.Contains("Invalid length for a Base-64 char array") ||
					unhandledException.Message.Contains("Invalid character in a Base-64 string") ||
					unhandledException.Message.Contains("The input is not a valid Base-64 string"))
				{
					return !TryToReloadPage();
				}
			}
			else if (unhandledException.InnerException is COMException { HResult: unchecked((int)0x800703E3) })
			{
				// Client has aborted the connection
				return false;
			}
			else if (unhandledException is HttpException httpException)
			{
				var httpCode = httpException.GetHttpCode();

				if (httpCode == HttpInternalServerError && HandledHttpHResultCodes.Contains(httpException.HResult))
				{
					return false;
				}

				if (httpException.Message == "Invalid viewstate." || httpException.Message.Contains("Failed to load viewstate"))
				{
					return !TryToReloadPage();
				}

				if (HttpContext.Current.Session == null &&
					(httpException.Message == "This is an invalid script resource request." ||
					httpException.Message == "This is an invalid webresource request."))
				{
					return false;
				}

				if (httpException.Message.Contains("A potentially dangerous Request"))
				{
					return false;
				}

				if (httpException.Message.Contains("user probably cancelled upload"))
				{
					return false;
				}

				if (httpException.Message.Contains("The length of the URL for this request exceeds the configured maxUrlLength value"))
				{
					return false;
				}

				if (httpException.Message.Contains("Session state has created a session id, but cannot save it because the response was already flushed by the application"))
				{
					return false;
				}

				if (httpException.Message.Contains("Exception of type 'System.Web.HttpException' was thrown") &&
					(RequestUrl.LocalPath.Contains("/WebService/Trace.axd", StringComparison.OrdinalIgnoreCase) ||
					RequestUrl.LocalPath.Contains("/admin/Trace.axd", StringComparison.OrdinalIgnoreCase) ||
					RequestUrl.LocalPath.Contains("/WebService/downloads/Trace.axd", StringComparison.OrdinalIgnoreCase) ||
					RequestUrl.LocalPath.Equals("/Trace.axd", StringComparison.OrdinalIgnoreCase)))
				{
					return false;
				}
			}
			else if (unhandledException is System.Security.Cryptography.CryptographicException)
			{
				if (unhandledException.Message == "Padding is invalid and cannot be removed.")
				{
					return false;
				}
			}
			else if (unhandledException is SqlLockLostException) // SqlLockLostException derives from InvalidOperationException so it needs to be checked before
			{
				if (unhandledException.Message.Contains("A db reconnect was attempted while undisposed SqlLocks existed"))
				{
					return false;
				}
			}
			else if (unhandledException is InvalidOperationException)
			{
				if (unhandledException.Message.Contains("No web service found at") ||
					unhandledException.Message.Contains("Request format is unrecognized for URL unexpectedly ending in") ||
					unhandledException.Message.Contains("ExecuteReader requires an open and available Connection. The connection's current state is closed.") ||
					(unhandledException.StackTrace != null && unhandledException.Message == "Collection was modified; enumeration operation may not execute." && unhandledException.StackTrace.Contains("defaultwsdlhelpgenerator_aspx.Page_Load")))
				{
					return false;
				}
			}
			else if (unhandledException is PathTooLongException)
			{
				if (unhandledException.Message.Contains("The specified path, file name, or both are too long. The fully qualified file name must be less than 260 characters, and the directory name must be less than 248 characters."))
				{
					return false;
				}
			}
			else if (unhandledException is BadImageFormatException)
			{
				if (unhandledException.Message.Equals("Bad IL range.", StringComparison.OrdinalIgnoreCase))
				{
					return false;
				}
			}
			else if (unhandledException is ArgumentException)
			{
				if (unhandledException.Message.Contains("Invalid postback or callback argument."))
				{
					return false;
				}
				else if (unhandledException.Message.Equals("Illegal characters in path.", StringComparison.OrdinalIgnoreCase) &&
					string.IsNullOrEmpty(HttpContext.Current.Request?.UrlReferrer?.AbsoluteUri))
				{
					return false;
				}
				else if (unhandledException.Message.StartsWith("Unknown web method", StringComparison.OrdinalIgnoreCase) &&
					unhandledException.Message.EndsWith("Parameter name: methodName", StringComparison.OrdinalIgnoreCase))
				{
					return false;
				}
				else if (unhandledException.Message == "Font '?' cannot be found.")
				{
					return false;
				}
			}
			else if (unhandledException is Exception && unhandledException.Message == "Tried to read a line. No data received.")
			{
				return false;
			}
			else if (unhandledException is Win32Exception win32Ex && HandledSystemErrorCodes.Contains(win32Ex.NativeErrorCode))
			{
				return false;
			}
			else if (unhandledException is XmlSchemaException && unhandledException.StackTrace != null && unhandledException.Message.StartsWith("The global", StringComparison.OrdinalIgnoreCase) && unhandledException.StackTrace.Contains("defaultwsdlhelpgenerator_aspx.Page_Load", StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}
			else if (unhandledException is ExternalException && unhandledException.Message.Contains("Timed out waiting for a program to execute."))
			{
				return false;
			}
			else if (unhandledException is DirectoryNotFoundException && unhandledException.Message.StartsWith(@"Could not find a part of the path 'C:\ProgramData\WiseTech Global\CargoWiseOneWeb", StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}
			else if (unhandledException is NullReferenceException &&
				unhandledException.StackTrace != null &&
				(unhandledException.StackTrace.Contains("System.Web.Caching.UsageBucket.GetFreeUsageEntry()") || unhandledException.StackTrace.Contains("defaultwsdlhelpgenerator_aspx.Page_Load", StringComparison.OrdinalIgnoreCase)))
			{
				return false;
			}

			return true;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "User agent")]
		bool IsWarmupUserAgent => HttpContext.Current?.Request?.ServerVariables["HTTP_USER_AGENT"] == "IIS Application Initialization Warmup";

		const int HttpInternalServerError = 500;
		static int[] HandledHttpHResultCodes => new[]
		{
			unchecked((int)0x80072746),
			unchecked((int)0x80070057),
			unchecked((int)0x800703E3),
			unchecked((int)0x800704CD),
			unchecked((int)0x80070016),
			unchecked((int)0x80070006),
		};

		static int[] HandledSystemErrorCodes => new[]
		{
			Win32_WAIT_TIMEOUT,
			Win32_ERROR_LOGON_FAILURE,
			Win32_ERROR_BAD_NETPATH, // When we get a SQL connection unavailable exception there is no reason to report it.
			Win32_ERROR_NETNAME_DELETED,
			Win32_WRITE_PROTECT
		};

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "javascript")]
		public bool TryToReloadPage()
		{
			if (HttpContext.Current != null && HttpContext.Current.Response != null)
			{
				HttpContext.Current.Response.Write("<script language=\"javascript\">\nalert(\"An error occurred during information submission. Please try again.\");\njavascript:history.go(-1);\n</script>\n");
				HttpContext.Current.Response.End();

				return true;
			}

			return false;
		}

		#endregion

		#region Helper Methods

		protected internal virtual bool ConfigurationOK
		{
			get
			{
				return GlobalConfig.ConfigurationAndLicenceOK && ErrorPageOK;
			}
		}

		bool fErrorPageOK;
		bool ErrorPageOK
		{
			get
			{
				// Test error page first time or on subsequent failed requests
				if (!fErrorPageOK)
				{
					if (ErrorPage != null)
					{
						FileInfo errorPageInfo = new FileInfo(Server.MapPath(ErrorPage));
						fErrorPageOK = errorPageInfo.Exists;
					}
				}
				return fErrorPageOK;
			}
		}

		void RenderSeriousErrorPage(string title, string errorMessage)
		{
			using (var writer = new HtmlTextWriter(HttpContext.Current.Response.Output))
			{
				writer.RenderBeginTag(HtmlTextWriterTag.Html);

				writer.RenderBeginTag(HtmlTextWriterTag.Head);

				writer.RenderBeginTag(HtmlTextWriterTag.Title);
				writer.Write(title);
				writer.RenderEndTag();
				writer.RenderEndTag();

				writer.RenderBeginTag(HtmlTextWriterTag.Body);

				writer.RenderBeginTag(HtmlTextWriterTag.H1);
				writer.WriteLine(title);
				writer.RenderEndTag();

				writer.RenderBeginTag(HtmlTextWriterTag.H3);
				writer.WriteLine(errorMessage);
				writer.RenderEndTag();

				writer.RenderEndTag();

				writer.RenderEndTag();
				writer.Flush();
				writer.Close();
			}
			HttpContext.Current.Response.End();
		}

		[SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		internal void RefreshRegistryItemCache()
		{
			if (DateTime.UtcNow.Subtract(lastRegistryItemCacheRefresh) > RegistryRefreshInterval)
			{
				lastRegistryItemCacheRefresh = DateTime.UtcNow;
				RegistryItemDictionary.Instance.PurgeAllIfUpdatedByUser();
			}
		}

		protected virtual TimeSpan RegistryRefreshInterval
		{
			get { return TimeSpan.FromMinutes(1); }
		}

		[SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		DateTime lastRegistryItemCacheRefresh = DateTime.UtcNow;

		#endregion Helper Methods
	}
}
