using System;
using System.Collections.Generic;
using System.Diagnostics;
using Enterprise.Registry.Business;
using Enterprise.ServiceManager.Shared.Interfaces;
using Enterprise.ZArchitecture.Core;
using ServiceManager.Host.Abstractions;

namespace ServiceManager.Host.CW;

class HostRegistrySettings : IHostRegistrySettings, ILoggerRegistrySettings
{
	public int ServiceTaskHostTerminatorFrequency { get; internal set; }

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
	public int ServiceTaskUnloadTimeoutInSeconds { get; internal set; }
	public bool ServiceTaskRunnerConnectionPoolingEnabled { get; internal set; }

	public bool ShowQueryStackTraceInProcessControllerEnabled { get; internal set; }
	public ProcessPriorityClass RunnerProcessPriorityValue { get; internal set; }

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
	public int BusyRunnerWaitTimeInSeconds { get; internal set; }

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
	public int SecondaryProcessSpinUpDelayInSeconds { get; internal set; }

	public bool ProcessControllerNLogInternalLoggingEnabled { get; internal set; }
	public VerboseLoggingCollection ProcessControllerVerboseLogging { get; internal set; }

	public bool FileSystemLoggingEnabled { get; internal set; }

	public bool ProductivityWiseModeEnabled { get; internal set; }

	// Elastic logging
	public bool ElasticSearchLoggingEnabled { get; internal set; }
	public string ElasticsearchServiceUri { get; internal set; }
	public string ElasticsearchServerPassword { get; internal set; }
	public string ElasticsearchServerUserName { get; internal set; }
	public string ElasticsearchIndex { get; internal set; }

	// Kafka logging
	public bool KafkaLoggingEnabled { get; internal set; }
	public string KafkaTopic { get; internal set; }
	public ReadOnlyCodeDescriptionPairList KafkaBrokers { get; internal set; }
	public KafkaSecurity ProcessControllerKafkaSecurity { get; internal set; }

	// Syslog logging
	public bool SyslogLoggingEnabled { get; internal set; }
	public string ProcessControllerSyslogProtocolVersion { get; internal set; }
	public string ProcessControllerSyslogNetworkProtocol { get; internal set; }
	public int ProcessControllerSyslogServerPort { get; internal set; }
	public string ProcessControllerSyslogServerHostname { get; internal set; }
	public bool ProcessControllerSyslogSslEnable { get; internal set; }

	// Combined file system logging
	public bool CombinedFileSystemLoggingEnabled { get; internal set; }
	public int ProcessControllerCombinedFileRetentionPeriod { get; internal set; }

	// Queue Monitoring
	public bool ProcessControllerQueueMonitoringEnabled { get; internal set; }
	public TimeSpan ProcessControllerQueueMonitoringFrequency { get; internal set; }
	public int ProcessControllerQueueMonitoringRetentionPeriodInDays { get; internal set; }

	public TimeSpan ThrottlingPeriodForDispatchingLoopWithBacklogWaitingForMaxSecondary { get; internal set; }
	public TimeSpan ThrottlingPeriodForDispatchingLoopWithAcceptableBacklog { get; internal set; }
	public TimeSpan ThrottlingPeriodForDispatchingLoopWithExceedingBacklog { get; internal set; }
	public TimeSpan ThrottlingTargetTimeToClearQueueBacklog { get; internal set; }

	public int ServiceTaskMemoryConstraint { get; internal set; }
	public string ForcefullyDisabledTasks { get; internal set; }

	public TimeSpan ServiceTaskMaxWaitForResourceAvailability { get; internal set; }
	public int ServiceTaskMaximumCpuLoadBeforeThrottlingTasks { get; internal set; }
	public decimal ServiceTaskMaximumDiskQueueLengthBeforeThrottlingTasks { get; internal set; }

	public bool ServiceTaskBusinessObjectBindingEnabled { get; internal set; }

	public TimeSpan ServiceTaskConfigurationPollingInterval { get; internal set; }

	public TimeSpan ServiceTaskRequirementCheckTimeLimit { get; internal set; }

	public int ServiceTaskProcessingMaximumBatchSize { get; internal set; }
	public decimal ServiceTaskProcessingBatchSizeScalingFactor { get; internal set; }

	public TimeSpan ServiceTaskProcessingBatchDelay { get; internal set; }

	public int ServiceTaskHttpProcessorMaxThreads { get; internal set; }

	// Experimental
	public bool SwitchRunnerToNetCore { get; internal set; }

	public IReadOnlyDictionary<string, string> ServiceTaskRunnerSpecificGroup { get; internal set; }

	public bool SwitchToNewServiceTasksModule { get; internal set; }
}
