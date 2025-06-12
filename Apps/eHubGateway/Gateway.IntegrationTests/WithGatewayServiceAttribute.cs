using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Web.Services.Description;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.eServices.Authentication.IntegrationTests;
using Microsoft.Web.Administration;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using WTG.DevTools.TestFramework;

namespace CargoWise.eHub.Gateway.IntegrationTests
{
	public class WithGatewayServiceAttribute : WithWebApplicationAttribute
	{
		const string applicationName = "GatewayService";
		const string billingServiceName = "BillingServiceController";
		const string sendBillingToKafkaPropName = "SendBillingToKafka";
		private const int MaxRetries = 10;

		public WithGatewayServiceAttribute() : base(new WithAuthenticationServiceAttribute())
		{
			IncludeTlsBinding = true;
		}

		protected WithGatewayServiceAttribute(bool withoutDependentActions)
			: base()
		{
			IncludeTlsBinding = true;
		}

		public static WithGatewayServiceAttribute Current => (WithGatewayServiceAttribute)TestContext.CurrentContext.Test.Properties.Get(applicationName);
		public static TestBillingServiceController BillingServiceController => (TestBillingServiceController)TestContext.CurrentContext.Test.Properties.Get(billingServiceName);

		protected override string RelativeBuildPath => applicationName;

		protected override string ApplicationName => applicationName;

		Exception SetupException { get; set; }

		public string BaseUrl => GetUrl("eHubGateway/");
		public string HealthCheckUrl => GetUrl("wtg/status");

		protected override void InitializeWebConfig(XDocument doc)
		{
			doc.XPathSelectElement("//common/logging//arg[@key='configFile']")
				.SetAttributeValue("value", @"~\nologging.log4net.test.config");
			doc.XPathSelectElement("//appSettings/add[@key='CargoWise.eServices.Authentication.ServiceClient.Endpoint']")
				.SetAttributeValue("value", WithAuthenticationServiceAttribute.Current.GetUrl("Authentication/"));

			doc.XPathSelectElement("//system.serviceModel//endpoint[@name='billing_service']")
				.SetAttributeValue("address", $"{billingBaseUri}BillingService");
			var sendBillingToKafka = TestContext.CurrentContext.Test.Properties.ContainsKey(sendBillingToKafkaPropName) ? TestContext.CurrentContext.Test.Properties.Get(sendBillingToKafkaPropName) : "false";
			doc.XPathSelectElement("//appSettings//add[@key='SendBillingToKafka']")
				.SetAttributeValue("value", sendBillingToKafka);

			var bindingSecurity = doc.XPathSelectElement("//basicHttpBinding/binding[@name='secure']/security");
			bindingSecurity.RemoveAll();
			bindingSecurity.SetAttributeValue("mode", "None");

			var kafkaBootstrapServer = doc.XPathSelectElement("//billingKafkaClientSettings/add[@key='bootstrap.servers']");
			kafkaBootstrapServer.SetAttributeValue("value", BillingServiceController.KafkaBrokers);
			foreach (var element in doc.XPathSelectElements("//billingKafkaClientSettings/add"))
			{
				if (element.Attribute("key")?.Value != "bootstrap.servers")
				{
					element.Remove();
				}
			}
		}


		public override void BeforeTest(ITest test)
		{
			Stopwatch baseSetupWatch  = Stopwatch.StartNew();
			var databaseDeployTime = DatabaseSetupTiming.DeployTime;
			TimeSpan? baseBeforeTestTime = null;
			TimeSpan? startServiceTime = null;
			try
			{
				var sendBillingToKafka = test.Properties.ContainsKey(sendBillingToKafkaPropName) && bool.TryParse(test.Properties.Get(sendBillingToKafkaPropName)?.ToString(), out var result) && result;
				var testBillingServiceController = new TestBillingServiceController(billingBaseUri, sendBillingToKafka);
				test.Properties.Set(billingServiceName, testBillingServiceController);
				base.BeforeTest(test);

				baseSetupWatch.Stop();
				baseBeforeTestTime = baseSetupWatch.Elapsed;
				ConfigurationManager.AppSettings["CargoWise.eHub.Gateway.HealthCheck.Http.Endpoint"] =
					Current.HealthCheckUrl;
				ConfigurationManager.AppSettings["CargoWise.eHub.Gateway.Wsdl.Http.Endpoint"] = GetUrl("wsdl.aspx");

				test.Properties.Set(ApplicationName, this);
				SetupException = null;
				var startServiceWatch = Stopwatch.StartNew();
				testBillingServiceController.StartBillingService();
				startServiceWatch.Stop();
				startServiceTime = startServiceWatch.Elapsed;
				throw new Exception("This is a test exception to ensure the setup works correctly. Remove this line in production code.");
			}
			catch (Exception ex)
			{
				SetupException = ex;

				var errorDetails = $@"
Setup failed for: {billingBaseUri}
Exception: {ex}
DatabaseDeployTime: {databaseDeployTime?.TotalSeconds:F1} sec
BaseBeforeTestTime: {baseBeforeTestTime?.TotalSeconds:F1} sec
StartBillingServiceTime: {startServiceTime?.TotalSeconds:F1} sec
";
				throw new Exception(errorDetails);
			}
		}

		public override void AfterTest(ITest test)
		{
			try
			{
				base.AfterTest(test);
				BillingServiceController?.Stop();
			}
			catch when (SetupException != null)
			{
			}
		}



		public object GetRemoteSettings(string key)
		{
			using (var manager = GetServerManager())
			{
				return GetConfigurationElement(manager, key)?.GetAttributeValue("value");
			}
		}

		public void SetRemoteSettings(string key, string value)
		{
			using (var manager = GetServerManager())
			{
				GetConfigurationElement(manager, key)?.SetAttributeValue("value", value);
				manager.CommitChanges();
			}
		}

		public Uri HttpEndpoint => GetHttpUri(EndpointPath);

		public Uri HttpsEndpoint => GetHttpsUri(EndpointPath);

		string EndpointPath => "eHubStreamedService.svc";

		Microsoft.Web.Administration.ConfigurationElement GetConfigurationElement(ServerManager manager, string key)
		{
			var config = manager.GetWebConfiguration(SiteName);
			var section = config.GetSection("appSettings");
			var settings = section.GetCollection();
			return settings.FirstOrDefault(x => x.GetAttributeValue("key").ToString() == key);
		}

		public ApplicationPool GetApplicationPool()
		{
			using (var serverManager = GetServerManager())
			{
				var gatewayApplication = serverManager.Sites[SiteName].Applications[0];
				return serverManager.ApplicationPools[gatewayApplication.ApplicationPoolName];
			}
		}

		public ServerManager GetServerManager() => ServerManager.OpenRemote(ServerName);
		readonly Uri billingBaseUri = new Uri($"http://localhost:{new Random().Next(49152, ushort.MaxValue)}");
	}
}
