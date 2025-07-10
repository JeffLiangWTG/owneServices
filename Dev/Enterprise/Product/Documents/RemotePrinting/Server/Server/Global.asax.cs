using System;
using System.Web;
using System.Web.Http;
using CargoWise.Data;
using CargoWiseOne.WebInfrastructure;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Web.GUI;

namespace Enterprise.RemotePrinting.Server
{
	public class Global : ZGlobal
	{
		public override string DefaultPage
		{
			get { return ApplicationRoot + "default.htm"; }
		}

		public string SupportPage
		{
			get { return ApplicationRoot + "Support/Diagnostics.aspx"; }
		}

		public override string BaseStyleSheet
		{
			get
			{
				return ApplicationRoot + "Styles/BaseStyle.css";
			}
		}

		protected override bool IsApplicationUserInteractive => true;

		protected override void Application_Start(object sender, EventArgs e)
		{
			base.Application_Start(sender, e);
			DbConnection.ApplicationName = "CargoWiseOneWebPrint";
			GlobalConfiguration.Configure(WebApiConfig.Register);
		}

		protected override bool ExceptionShouldBeHandled(Exception unhandledException)
		{
			if (WebUpgradeManager.IsUpgrateRunning())
			{
				return true;
			}

			return ExceptionHandlingExtension.ShouldReportError(unhandledException) && base.ExceptionShouldBeHandled(unhandledException);
		}

		protected override bool ConfigurationOK => true;

		public override void ReportError(string pageTitle, string message)
		{
			if (HttpContext.Current.Request.Headers["SOAPAction"] != null)
			{
				var statusCode = 500;
				var lastError = Server.GetLastError();
				if (lastError != null)
				{
					var unhandledException = lastError.GetBaseException();
					statusCode = ExceptionHandlingExtension.DetermineStatusCode(unhandledException);
					message = $"{message}\r\n{unhandledException}";
				}
				ExceptionHandlingExtension.ReportErrorToClient(statusCode, message);
			}
			else
			{
				base.ReportError(pageTitle, message);
			}
		}

		#region WebPrintEnvironmentProvider

		protected override EnvProvider WebEnvProvider => lazyEnvProvider.Value;

		readonly Lazy<WebPrintEnvironmentProvider> lazyEnvProvider = new Lazy<WebPrintEnvironmentProvider>(() => new WebPrintEnvironmentProvider());

		#endregion
	}
}
