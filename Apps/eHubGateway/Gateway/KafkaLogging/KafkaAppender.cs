using System;
using System.Configuration;
using Common.Logging;
using Confluent.Kafka;
using log4net.Appender;
using log4net.Core;

namespace CargoWise.eHub.Gateway
{
	public class KafkaAppender : AppenderSkeleton
	{
		public IProducer<Null, LogEvent> producer { get; set; }
		public KafkaOptions kafkaOptions { get; set; }
		private static readonly ILog logger = LogManager.GetLogger("KafkaLog");

		public override void ActivateOptions()
		{
			try
			{
				producer = new ProducerBuilder<Null, LogEvent>(GetProducerConfig(kafkaOptions))
								.SetErrorHandler(
									(p, e) => logger.ErrorFormat("Error: {0} - {1}", e.Code, e.Reason))
								.SetKeySerializer(Serializers.Null)
								.SetValueSerializer(new KafkaEventSerializer())
								.Build();
			}
			catch (Exception ex)
			{
				logger.Error($"Error while building kafka producer: {ex}");
			}
		}

		protected override void Append(LoggingEvent loggingEvent)
		{
			var message = GetMessage(loggingEvent);
			producer.Produce(kafkaOptions.Topic, message);
		}

		internal static Message<Null, LogEvent> GetMessage(LoggingEvent loggingEvent)
		{
			var logEvent = new LogEvent
			{
				ApplicationName = loggingEvent.Domain,
				LoggerName = loggingEvent.LoggerName,
				Level = loggingEvent.Level.Name,
				Message = loggingEvent.RenderedMessage
			};

			logEvent.ExceptionID = loggingEvent.LookupProperty(nameof(logEvent.ExceptionID))?.ToString();

			return new Message<Null, LogEvent> { Key = null, Value = logEvent };
		}

		internal static ProducerConfig GetProducerConfig(KafkaOptions kafkaOptions)
		{
			var sensitiveAppSettings = (System.Collections.Specialized.NameValueCollection)ConfigurationManager.GetSection("sensitiveAppSettings");
			var config = new ProducerConfig
			{
				BootstrapServers = kafkaOptions.Brokers,
				SaslUsername = kafkaOptions.SaslUsername,
				SaslPassword = sensitiveAppSettings["GatewayKafkaSaslPassword"],
				SecurityProtocol = kafkaOptions.SecurityProtocol,
				SaslMechanism = kafkaOptions.SaslMechanism,
				EnableSslCertificateVerification = true,
				Acks = kafkaOptions.Acks
			};

			return config;
		}

		protected override void OnClose()
		{
			producer?.Flush();
			producer?.Dispose();
			producer = null;
		}
	}
}
