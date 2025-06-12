using System;
using System.Data;
using System.Xml;
using CargoWise.eHub.MessageEvent.KafkaProducerService.KafkaProducer;
using CargoWise.eHub.MessageEvent.KafkaProducerService.Logging;
using CargoWise.eHub.MessageEvent.KafkaProducerService.Options;
using CargoWise.eHub.MessageEvent.KafkaProducerService.ServiceBroker;
using Confluent.Kafka;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CargoWise.eHub.MessageEvent.KafkaProducerService.DI
{
	static class ConfigureAppServices
	{
		public static IServiceCollection AddAppServices(this IServiceCollection services)
		{
			services.AddTransient<ISerializer<XmlDocument>, MessageEventJsonSerializer>();
			services.AddTransient<IMessageEventServiceBroker, MessageEventServiceBroker>();

			services.AddTransient(provider =>
			{
				var options = provider.GetRequiredService<IOptions<KafkaOptions>>().Value;

				if (string.IsNullOrEmpty(options.BootstrapServers))
				{
					throw new ApplicationException($"Application configuration missing Kafka {nameof(options.BootstrapServers)}");
				}

				if (string.IsNullOrEmpty(options.SaslUsername))
				{
					throw new ApplicationException($"Application configuration missing Kafka {nameof(options.SaslUsername)}");
				}

				if (string.IsNullOrEmpty(options.SaslPassword))
				{
					throw new ApplicationException($"Application configuration missing Kafka {nameof(options.SaslPassword)}");
				}

				if (string.IsNullOrEmpty(options.TransactionalId))
				{
					throw new ApplicationException($"Application configuration missing Kafka {nameof(options.TransactionalId)}");
				}

				var logger = provider.GetRequiredService<ILogger<Program>>();

				logger.eHubLog(LogLevel.Trace, "", "", typeof(ConfigureAppServices)?.FullName ?? "Unknown Source",
					"KafkaProducer config, EnableIdempotence: {enable}, BootstrapServers: {servers}, TransactionId: {transId}", options.EnableIdempotence, options.BootstrapServers, options.TransactionalId);

				return new ProducerConfig
				{
					EnableIdempotence = options.EnableIdempotence,
					BootstrapServers = options.BootstrapServers,
					SecurityProtocol = SecurityProtocol.SaslSsl,
					SaslMechanism = SaslMechanism.Plain,
					SaslUsername = options.SaslUsername,
					SaslPassword = options.SaslPassword,
					TransactionalId = options.TransactionalId,
				};
			});

			services.AddTransient<IDbConnection>(provider =>
			{
				var config = provider.GetRequiredService<IConfiguration>();
				var builder = new SqlConnectionStringBuilder(config.GetConnectionString("eHubTransactions"));
				var options = provider.GetRequiredService<IOptions<ServiceBrokerOptions>>().Value;

				if (!builder.IntegratedSecurity)
				{
					if (string.IsNullOrEmpty(options.DbPassword))
					{
						throw new ApplicationException($"Application configuration missing Database {nameof(options.DbPassword)}");
					}
					builder.Password = options.DbPassword;
				}

				return new SqlConnection(builder.ConnectionString);
			});

			services.AddTransient(provider =>
			{
				var valueSerializer = provider.GetRequiredService<ISerializer<XmlDocument>>();
				var config = provider.GetRequiredService<ProducerConfig>();
				var logger = provider.GetRequiredService<ILogger<Program>>();

				var producer = new ProducerBuilder<string, XmlDocument>(config)
					.SetValueSerializer(valueSerializer)
					.SetLogHandler((p, message) =>
						logger.eHubLog(LogLevel.Trace, "", "", typeof(ConfigureAppServices)?.FullName ?? "Unknown Source",
							"{facility}-{name}-{level}-{message}", message.Facility, message.Name, message.Level, message.Message))
					.SetErrorHandler((p, error) =>
						logger.eHubLog(LogLevel.Error, "", "", typeof(ConfigureAppServices)?.FullName ?? "Unknown Source",
							"{code}-{reason}", error.Code, error.Reason))
					.Build();

				var kafkaOptions = provider.GetRequiredService<IOptions<KafkaOptions>>().Value;
				var timeSpan = TimeSpan.FromSeconds(kafkaOptions.TransactionTimeoutSecond);

				logger.eHubLog(LogLevel.Trace, "", "", typeof(ConfigureAppServices)?.FullName ?? "Unknown Source",
					"InitTransactions for producer started");

				producer.InitTransactions(timeSpan);
				logger.eHubLog(LogLevel.Trace, "", "", typeof(ConfigureAppServices)?.FullName ?? "Unknown Source",
					"InitTransactions for producer finished");

				return producer;
			});

			return services;
		}
	}
}
