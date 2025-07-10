#if NETFRAMEWORK
using System;
using System.Configuration;
using System.Web;
using System.Web.Http;
using CargoWise.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets.Wrappers;
using WTG.Logging.NLog.Kafka;

namespace CargoWise.ProductRegistration.Service
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1018:HttpApplicationRule", Justification = "Service not deployed with the application.")]
	public class WebApiApplication : HttpApplication
	{
		protected void Application_Start()
		{
			GlobalConfiguration.Configure(config =>
			{
				config.MapHttpAttributeRoutes();

				var serviceCollection = new ServiceCollection();
				serviceCollection.ConfigureProtectedDataFactoryServices();
				serviceCollection.ConfigureProtectedDataSqlExtensions(ApplicationType.Web);
				serviceCollection.AddTransient<IRegistrationRepository, RegistrationRepository>();
				serviceCollection.AddTransient<RegistrationController>();
				serviceCollection.AddTransient<ConnectionManager>();

				serviceCollection.Configure<DatabaseSettings>(settings =>
				{
					settings.PrimaryConnectionString = ConfigurationManager.ConnectionStrings["Primary"]?.ConnectionString;
					settings.SecondaryConnectionString = ConfigurationManager.ConnectionStrings["Secondary"]?.ConnectionString;
				});

				config.DependencyResolver = new ServiceProviderDependencyResolver(serviceCollection.BuildServiceProvider());
			});
			ConfigNLog();
		}

		void ConfigNLog()
		{
			var layout = new JsonLayout();
			layout.IncludeEventProperties = true;
			layout.Attributes.Add(new JsonAttribute("longdate", "${longdate}"));
			layout.Attributes.Add(new JsonAttribute("level", "${level}"));
			layout.Attributes.Add(new JsonAttribute("logger", "${logger}"));
			layout.Attributes.Add(new JsonAttribute("message", "${message}"));
			layout.Attributes.Add(new JsonAttribute("exception", "${exception:format=tostring}"));

			try
			{
				if (LogManager.Configuration == null)
				{
					LogManager.Configuration = new LoggingConfiguration();
				}
				var config = LogManager.Configuration;

				var topic = "topic-au2-prod-myaccount-logs-prod";
				var brokerAddresses = new string[] { "1.au2-prod-1.kafka.wtg.ws:9093", "2.au2-prod-1.kafka.wtg.ws:9093", "3.au2-prod-1.kafka.wtg.ws:9093", "4.au2-prod-1.kafka.wtg.ws:9093", "5.au2-prod-1.kafka.wtg.ws:9093" };
				var caFile = HttpContext.Current.Server.MapPath("~/Kafka.pem");
				var kafkaTarget = KafkaLoggingTarget.GetSslTarget("kafka", topic, brokerAddresses, layout, caFile);
				var asyncKafkaTarget = new AsyncTargetWrapper(kafkaTarget)
				{
					Name = kafkaTarget.Name,
					QueueLimit = 50,
					OverflowAction = AsyncTargetWrapperOverflowAction.Discard
				};
				config.AddRule(LogLevel.Info, LogLevel.Fatal, asyncKafkaTarget, "*");

				LogManager.Configuration.Reload();
				LogManager.ReconfigExistingLoggers();
			}
			catch (Exception)
			{
			}
		}
	}
}
#endif
