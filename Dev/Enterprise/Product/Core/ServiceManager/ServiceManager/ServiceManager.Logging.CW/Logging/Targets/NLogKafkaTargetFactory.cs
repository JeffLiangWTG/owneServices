using Enterprise.Registry.Business;
using Enterprise.ServiceManager.Shared.Interfaces;
using NLog;
using NLog.Layouts;
using NLog.Targets;
using NLog.Targets.Wrappers;
using ServiceManager.Shared.CW;
using WTG.Logging.NLog.Kafka;

namespace ServiceManager.Logging.CW
{
	class NLogKafkaTargetFactory : INLogTargetFactory
	{
		public Target? GetOrCreateTarget()
		{
			var registry = (ILoggerRegistrySettings)SharedRegistry.Instance;
			if (!registry.KafkaLoggingEnabled)
			{
				return null;
			}

			const string targetName = "kafka";

			var existingBufferingTarget = LogManager.Configuration.FindTargetByName<BufferingTargetWrapper>(targetName);
			if (existingBufferingTarget != null)
			{
				return existingBufferingTarget;
			}

			var topic = registry.KafkaTopic;
			var addresses = registry.KafkaBrokers.GetAllCodes();
			var security = registry.ProcessControllerKafkaSecurity;
			var layout = new JsonLayout
			{
				MaxRecursionLimit = 3,
				IncludeEventProperties = true,
			};
			layout.Attributes.Add(new JsonAttribute("message", "${message}"));
			layout.Attributes.Add(new JsonAttribute("eventTime", "${date:universalTime=true:format=yyyy-MM-dd\\THH\\:mm\\:ss.fffK}"));

			switch (security.SecurityProtocol.ToString())
			{
				case KafkaSecurityProtocolOptions.PLAINTEXT:
					return KafkaLoggingTarget.GetPlaintextTarget(
						targetName,
						topic,
						addresses,
						layout);

				case KafkaSecurityProtocolOptions.SSL:
					return KafkaLoggingTarget.GetSslTarget(
						targetName,
						topic,
						addresses,
						layout,
						security.SslCaLocation);

				case KafkaSecurityProtocolOptions.SASL_PLAINTEXT:
					return KafkaLoggingTarget.GetSaslPlaintextTarget(
						targetName,
						topic,
						addresses,
						security.SaslUsername,
						security.SaslPassword,
						layout);

				case KafkaSecurityProtocolOptions.SASL_SSL:
					return KafkaLoggingTarget.GetSaslSslTarget(
						targetName,
						topic,
						addresses,
						security.SaslUsername,
						security.SaslPassword,
						layout,
						security.SslCaLocation);

				default:
					return null;
			}
		}
	}
}
