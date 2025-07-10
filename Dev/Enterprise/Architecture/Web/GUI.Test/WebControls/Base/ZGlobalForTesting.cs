using System;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Web.GUI.Testing
{
	public class ZGlobalForTesting : ZGlobal
	{
		public ZGlobalForTesting()
		{
			HasApplicationStartedSuccessfully = true;
		}

		public override string DefaultPage
		{
			get { return "/DefaultPage.aspx"; }
		}

		public override string ApplicationRoot
		{
			get { return "/"; }
		}

		protected override Uri RequestUrl => RequestUrlOverride ?? base.RequestUrl;

		public Uri RequestUrlOverride { get; set; }

		protected override TimeSpan RegistryRefreshInterval
		{
			get { return TimeSpan.FromSeconds(1); }
		}

		#region Test Properties

		public void Application_BeginRequest_ForTesting(Object sender, EventArgs e) => Application_BeginRequest(sender, e);
		public void Session_Start_ForTesting(object sender, EventArgs e) => Session_Start(sender, e);
		public void Application_Error_ForTesting(object sender, EventArgs e) => Application_Error(sender, e);
		public void OnCustomSessionStartForTesting(Object sender, EventArgs e) => OnCustomSessionStart(sender, e);
		public bool ExceptionShouldBeHandledForTesting(Exception unhandledException) => ExceptionShouldBeHandled(unhandledException);
		public string ApplicationCookieNameForTesting => ApplicationCookieName;
		public BaseExceptionReporter WebExceptionReporterForTesting => WebExceptionReporter;
		public TimeSpan RegistryRefreshIntervalForTesting => RegistryRefreshInterval;
		public Uri RequestUrlForTesting => RequestUrl;

		#endregion
	}
}
