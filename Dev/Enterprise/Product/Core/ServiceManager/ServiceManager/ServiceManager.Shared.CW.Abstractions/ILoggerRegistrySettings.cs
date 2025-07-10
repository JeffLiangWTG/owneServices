using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ServiceManager.Shared.Interfaces
{
	public interface ILoggerRegistrySettings
	{
		VerboseLoggingCollection ProcessControllerVerboseLogging { get; }

		// Internal logging
		bool ProcessControllerNLogInternalLoggingEnabled { get; }

		// File system logging
		bool FileSystemLoggingEnabled { get; }

		// Elastic logging
		bool ElasticSearchLoggingEnabled { get; }
		string ElasticsearchIndex { get; }
		string ElasticsearchServerPassword { get; }
		string ElasticsearchServerUserName { get; }
		string ElasticsearchServiceUri { get; }

		// Kafka logging
		bool KafkaLoggingEnabled { get; }
		string KafkaTopic { get; }
		ReadOnlyCodeDescriptionPairList KafkaBrokers { get; }
		KafkaSecurity ProcessControllerKafkaSecurity { get; }

		// Syslog logging
		bool SyslogLoggingEnabled { get; }
		string ProcessControllerSyslogProtocolVersion { get; }
		string ProcessControllerSyslogNetworkProtocol { get; }
		int ProcessControllerSyslogServerPort { get; }
		string ProcessControllerSyslogServerHostname { get; }
		bool ProcessControllerSyslogSslEnable { get; }

		// Combined file system logging
		bool CombinedFileSystemLoggingEnabled { get; }
		int ProcessControllerCombinedFileRetentionPeriod { get; }
	}
}
