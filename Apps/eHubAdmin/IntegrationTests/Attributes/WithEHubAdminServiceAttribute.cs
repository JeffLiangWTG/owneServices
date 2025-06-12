using System;
using System.Configuration;
using System.Xml.Linq;
using System.Xml.XPath;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using WTG.DevTools.TestFramework;

namespace eServices.eHubAdmin.IntegrationTests.Attributes
{
	public class WithEHubAdminServiceAttribute : WithWebApplicationAttribute
	{
		const string applicationName = "eHubAdmin";

		public WithEHubAdminServiceAttribute() : base()
		{
		}

		protected override void InitializeWebConfig(XDocument doc) { }

		bool IsLocalDeployment => ServerName == "localhost";
		protected override string RelativeBuildPath => "eHubAdmin\\IntegrationTest";
		protected override string ApplicationName => applicationName;
		Exception SetupException { get; set; }
		public override string ServerName => GetConfigOrDefault("EHubAdminServerName", base.ServerName);
		public override string SiteName => GetConfigOrDefault("EHubAdminSiteName", base.SiteName);
		protected override bool EnableWindowsAuthentication => true;

		public static WithEHubAdminServiceAttribute Current => (WithEHubAdminServiceAttribute)TestContext.CurrentContext.Test.Properties.Get(applicationName);

		public override void BeforeTest(ITest test)
		{
			try
			{
				// If we're testing against a remote deployment then we don't want to run the base method
				// which deploys the service locally
				if (IsLocalDeployment)
				{
					base.BeforeTest(test);
				}

				test.Properties.Set(ApplicationName, this);
				SetupException = null;
			}
			catch (Exception ex)
			{
				SetupException = ex;
			}
		}

		public override void AfterTest(ITest test)
		{
			try
			{
				if (IsLocalDeployment)
				{
					base.AfterTest(test);
				}
			}
			catch when (SetupException != null)
			{
			}
		}

		static string GetConfigOrDefault(string key, string defaultValue)
		{
			var value = ConfigurationManager.AppSettings[key];

			if (string.IsNullOrWhiteSpace(value))
			{
				value = defaultValue;
			}

			return value;
		}

		public Uri GetHttpEndPoint(string route)
		{
			string EndpointPath = IsLocalDeployment ? $"/{route}" : $"/{ApplicationName}/{route}";
			return GetHttpUri(EndpointPath);
		}
	}
}
