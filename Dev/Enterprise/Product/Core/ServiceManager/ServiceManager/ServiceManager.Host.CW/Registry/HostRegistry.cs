using Enterprise.Environment;
using ServiceManager.Host.Abstractions;

namespace ServiceManager.Host.CW;

class HostRegistry : IHostRegistry
{
	public HostRegistry(HostRegistrySettings settings)
	{
		this.settings = settings;
	}

	public void Initialize()
	{
		ReadStaticSettings();

		InitializeEnvironmentTime();

		ReadDynamicSettings();

		static void InitializeEnvironmentTime()
		{
			var time = Env.Time;
			time.SetCacheToNeverExpire();
			_ = time.CurrentUtcDateTime;
		}
	}

	public void Refresh()
	{
		ReadDynamicSettings();

		Env.Time.RefreshCacheNow();
	}

	void ReadStaticSettings()
	{
		var registry = new DirectHostRegistrySettings();

		settings.ProductivityWiseModeEnabled = registry.ProductivityWiseModeEnabled;
		settings.ThrottlingPeriodForDispatchingLoopWithBacklogWaitingForMaxSecondary = registry.ThrottlingPeriodForDispatchingLoopWithBacklogWaitingForMaxSecondary;
		settings.ThrottlingPeriodForDispatchingLoopWithAcceptableBacklog = registry.ThrottlingPeriodForDispatchingLoopWithAcceptableBacklog;
		settings.ThrottlingPeriodForDispatchingLoopWithExceedingBacklog = registry.ThrottlingPeriodForDispatchingLoopWithExceedingBacklog;
		settings.ThrottlingTargetTimeToClearQueueBacklog = registry.ThrottlingTargetTimeToClearQueueBacklog;
		settings.ServiceTaskMemoryConstraint = registry.ServiceTaskMemoryConstraint;
		settings.ServiceTaskMaxWaitForResourceAvailability = registry.ServiceTaskMaxWaitForResourceAvailability;
		settings.ServiceTaskMaximumCpuLoadBeforeThrottlingTasks = registry.ServiceTaskMaximumCpuLoadBeforeThrottlingTasks;
		settings.ServiceTaskMaximumDiskQueueLengthBeforeThrottlingTasks = registry.ServiceTaskMaximumDiskQueueLengthBeforeThrottlingTasks;
		settings.ServiceTaskRequirementCheckTimeLimit = registry.ServiceTaskRequirementCheckTimeLimit;
		settings.SwitchToNewServiceTasksModule = registry.SwitchToNewServiceTasksModule;
	}

	void ReadDynamicSettings()
	{
		var registry = new DirectHostRegistrySettings();

		settings.ProcessControllerVerboseLogging = registry.ProcessControllerVerboseLogging;
		settings.ProcessControllerNLogInternalLoggingEnabled = registry.ProcessControllerNLogInternalLoggingEnabled;

		settings.ForcefullyDisabledTasks = registry.ForcefullyDisabledTasks;

		settings.FileSystemLoggingEnabled = registry.FileSystemLoggingEnabled;
		settings.ShowQueryStackTraceInProcessControllerEnabled = registry.ShowQueryStackTraceInProcessControllerEnabled;

		settings.KafkaLoggingEnabled = registry.KafkaLoggingEnabled;
		if (settings.KafkaLoggingEnabled)
		{
			settings.KafkaTopic = registry.KafkaTopic;
			settings.KafkaBrokers = registry.KafkaBrokers;
			settings.ProcessControllerKafkaSecurity = registry.ProcessControllerKafkaSecurity;
		}

		settings.SyslogLoggingEnabled = registry.SyslogLoggingEnabled;
		if (settings.SyslogLoggingEnabled)
		{
			settings.ProcessControllerSyslogProtocolVersion = registry.ProcessControllerSyslogProtocolVersion;
			settings.ProcessControllerSyslogNetworkProtocol = registry.ProcessControllerSyslogNetworkProtocol;
			settings.ProcessControllerSyslogServerPort = registry.ProcessControllerSyslogServerPort;
			settings.ProcessControllerSyslogServerHostname = registry.ProcessControllerSyslogServerHostname;
			settings.ProcessControllerSyslogSslEnable = registry.ProcessControllerSyslogSslEnable;
		}

		settings.ElasticSearchLoggingEnabled = registry.ElasticSearchLoggingEnabled;
		if (settings.ElasticSearchLoggingEnabled)
		{
			settings.ElasticsearchServiceUri = registry.ElasticsearchServiceUri;
			settings.ElasticsearchServerUserName = registry.ElasticsearchServerUserName;
			settings.ElasticsearchServerPassword = registry.ElasticsearchServerPassword;
			settings.ElasticsearchIndex = registry.ElasticsearchIndex;
		}

		settings.CombinedFileSystemLoggingEnabled = registry.CombinedFileSystemLoggingEnabled;
		if (settings.CombinedFileSystemLoggingEnabled)
		{
			settings.ProcessControllerCombinedFileRetentionPeriod = registry.ProcessControllerCombinedFileRetentionPeriod;
		}

		settings.ProcessControllerQueueMonitoringEnabled = registry.ProcessControllerQueueMonitoringEnabled;
		if (settings.ProcessControllerQueueMonitoringEnabled)
		{
			settings.ProcessControllerQueueMonitoringFrequency = registry.ProcessControllerQueueMonitoringFrequency;
			settings.ProcessControllerQueueMonitoringRetentionPeriodInDays = registry.ProcessControllerQueueMonitoringRetentionPeriodInDays;
		}

		settings.ServiceTaskHostTerminatorFrequency = registry.ServiceTaskHostTerminatorFrequency;
		settings.ServiceTaskUnloadTimeoutInSeconds = registry.ServiceTaskUnloadTimeoutInSeconds;

		settings.BusyRunnerWaitTimeInSeconds = registry.BusyRunnerWaitTimeInSeconds;
		settings.SecondaryProcessSpinUpDelayInSeconds = registry.SecondaryProcessSpinUpDelayInSeconds;
		settings.ServiceTaskRunnerConnectionPoolingEnabled = registry.ServiceTaskRunnerConnectionPoolingEnabled;
		settings.ServiceTaskProcessingMaximumBatchSize = registry.ServiceTaskProcessingMaximumBatchSize;
		settings.ServiceTaskProcessingBatchSizeScalingFactor = registry.ServiceTaskProcessingBatchSizeScalingFactor;
		settings.ServiceTaskProcessingBatchDelay = registry.ServiceTaskProcessingBatchDelay;
		settings.RunnerProcessPriorityValue = registry.RunnerProcessPriorityValue;

		settings.ServiceTaskBusinessObjectBindingEnabled = registry.ServiceTaskBusinessObjectBindingEnabled;
		settings.ServiceTaskConfigurationPollingInterval = registry.ServiceTaskConfigurationPollingInterval;

		settings.ServiceTaskHttpProcessorMaxThreads = registry.ServiceTaskHttpProcessorMaxThreads;

		settings.SwitchRunnerToNetCore = registry.SwitchRunnerToNetCore;

		settings.ServiceTaskRunnerSpecificGroup = registry.ServiceTaskRunnerSpecificGroup;
	}

	readonly HostRegistrySettings settings;
}
