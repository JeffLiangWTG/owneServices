using System;
using CargoWise.Application;
using Enterprise.Registry.Business;
using Enterprise.ServiceManager.Shared.Interfaces;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using ServiceManager.Shared.Abstractions;

namespace ServiceManager.Shared.CW
{
	/// <summary>
	/// Implementation of <see cref="ISharedRegistrySettings"/> that goes directly to the registry.
	/// Used for anything outside of process controller.
	/// </summary>
	public class DirectSharedRegistrySettings : ISharedRegistrySettings, ILoggerRegistrySettings
	{
		public bool ProcessControllerNLogInternalLoggingEnabled { get => SystemDataRegistry.Instance.ProcessControllerNLogInternalLoggingEnabled.Value; }
		public VerboseLoggingCollection ProcessControllerVerboseLogging { get => SystemDataRegistry.Instance.ProcessControllerVerboseLogging.Value; }
	
		public bool FileSystemLoggingEnabled => HasLoggingMethod(LoggingMethods.FSL);

		static ICodeDescriptionPairListWithDefaultCodeAndExtraBool LoggingMethodList => SystemDataRegistry.Instance.LoggingMethods.Value;
		static bool HasLoggingMethod(string loggingMethod) => LoggingMethodList.FindElementByCode(loggingMethod).Bool2;
		public bool SwitchRunnerToNetCore { get => SystemDataRegistry.Instance.SwitchRunnerToNetCore.Value; }

		// Elastic logging
		public bool ElasticSearchLoggingEnabled { get => HasLoggingMethod(LoggingMethods.ELK); }
		public string ElasticsearchIndex { get => SystemDataRegistry.Instance.ElasticsearchIndex.Value; }
		public string ElasticsearchServerUserName { get => SystemDataRegistry.Instance.ElasticsearchServerUserName.Value; }
		public string ElasticsearchServerPassword { get => SystemDataRegistry.Instance.ElasticsearchServerPassword.Value; }
		public string ElasticsearchServiceUri { get => SystemDataRegistry.Instance.ElasticsearchServiceUri.Value; }

		// Kafka logging
		public bool KafkaLoggingEnabled { get => HasLoggingMethod(LoggingMethods.KAF); }
		public string KafkaTopic { get => SystemDataRegistry.Instance.KafkaTopic.Value; }
		public ReadOnlyCodeDescriptionPairList KafkaBrokers { get => SystemDataRegistry.Instance.KafkaBrokers.Value; }
		public KafkaSecurity ProcessControllerKafkaSecurity { get => SystemDataRegistry.Instance.ProcessControllerKafkaSecurity.Value; }

		// Syslog logging
		public bool SyslogLoggingEnabled { get => HasLoggingMethod(LoggingMethods.SYS); }
		public string ProcessControllerSyslogProtocolVersion { get => SystemDataRegistry.Instance.ProcessControllerSyslogProtocolVersion.Value; }
		public string ProcessControllerSyslogNetworkProtocol { get => SystemDataRegistry.Instance.ProcessControllerSyslogNetworkProtocol.Value; }
		public int ProcessControllerSyslogServerPort { get => SystemDataRegistry.Instance.ProcessControllerSyslogServerPort.Value; }
		public string ProcessControllerSyslogServerHostname { get  => SystemDataRegistry.Instance.ProcessControllerSyslogServerHostname.Value; }
		public bool ProcessControllerSyslogSslEnable { get => SystemDataRegistry.Instance.ProcessControllerSyslogSslEnable.Value; }

		// Combined file system logging
		public bool CombinedFileSystemLoggingEnabled { get => HasLoggingMethod(LoggingMethods.CFL); }
		public int ProcessControllerCombinedFileRetentionPeriod { get => SystemDataRegistry.Instance.ProcessControllerCombinedFileRetentionPeriod.Value; }

		public bool ServiceTaskBusinessObjectBindingEnabled { get => ObjectFactory.Get<ISystemDataRegistry>().ServiceTaskBusinessObjectBindingEnabled; }
		public bool ProductivityWiseModeEnabled { get => DataRegistry.Instance.ProductivityWiseModeEnabled; }
		public TimeSpan ServiceTaskRequirementCheckTimeLimit { get => SystemDataRegistry.Instance.ServiceTaskRequirementCheckTimeLimit; }

		// Queue Monitoring
		public bool ProcessControllerQueueMonitoringEnabled { get => SystemDataRegistry.Instance.ProcessControllerQueueMonitoringEnabled.Value; }
		public TimeSpan ProcessControllerQueueMonitoringFrequency { get => SystemDataRegistry.Instance.ProcessControllerQueueMonitoringFrequency; }
		public int ProcessControllerQueueMonitoringRetentionPeriodInDays { get => SystemDataRegistry.Instance.ProcessControllerQueueMonitoringRetentionPeriodInDays.Value; }
	}
}
