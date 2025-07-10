using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using Enterprise.Registry.Business;
using ServiceManager.Host.Abstractions;
using ServiceManager.Shared.CW;

namespace ServiceManager.Host.CW;

class DirectHostRegistrySettings : DirectSharedRegistrySettings, IHostRegistrySettings
{
	public int ServiceTaskHostTerminatorFrequency => SystemDataRegistry.Instance.ServiceTaskHostTerminatorFrequency.Value;

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
	public int ServiceTaskUnloadTimeoutInSeconds => SystemDataRegistry.Instance.ServiceTaskUnloadTimeoutInSeconds.Value;
	public bool ServiceTaskRunnerConnectionPoolingEnabled => SystemDataRegistry.Instance.ServiceTaskRunnerConnectionPoolingEnabled.Value;
	public int ServiceTaskProcessingMaximumBatchSize => SystemDataRegistry.Instance.ServiceTaskProcessingMaximumBatchSize.Value;
	public decimal ServiceTaskProcessingBatchSizeScalingFactor => SystemDataRegistry.Instance.ServiceTaskProcessingBatchSizeScalingFactor.Value;
	public TimeSpan ServiceTaskProcessingBatchDelay => TimeSpan.FromMilliseconds(SystemDataRegistry.Instance.ServiceTaskProcessingBatchDelayMilliseconds.Value);
	public bool ShowQueryStackTraceInProcessControllerEnabled => SystemDataRegistry.Instance.ShowQueryStackTraceInProcessControllerEnabled.Value;
	public ProcessPriorityClass RunnerProcessPriorityValue => SystemDataRegistry.Instance.RunnerProcessPriorityValue;

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
	public int BusyRunnerWaitTimeInSeconds => SystemDataRegistry.Instance.BusyRunnerWaitTimeInSeconds.Value;

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
	public int SecondaryProcessSpinUpDelayInSeconds => SystemDataRegistry.Instance.SecondaryProcessSpinUpDelayInSeconds.Value;

	public TimeSpan ThrottlingPeriodForDispatchingLoopWithBacklogWaitingForMaxSecondary => SystemDataRegistry.Instance.ThrottlingPeriodForDispatchingLoopWithBacklogWaitingForMaxSecondary;
	public TimeSpan ThrottlingPeriodForDispatchingLoopWithAcceptableBacklog => SystemDataRegistry.Instance.ThrottlingPeriodForDispatchingLoopWithAcceptableBacklog;
	public TimeSpan ThrottlingPeriodForDispatchingLoopWithExceedingBacklog => SystemDataRegistry.Instance.ThrottlingPeriodForDispatchingLoopWithExceedingBacklog;
	public TimeSpan ThrottlingTargetTimeToClearQueueBacklog => SystemDataRegistry.Instance.ThrottlingTargetTimeToClearQueueBacklog;

	public int ServiceTaskMemoryConstraint => SystemDataRegistry.Instance.ServiceTaskMemoryConstraint.Value;
	public string ForcefullyDisabledTasks => SystemDataRegistry.Instance.ForcefullyDisabledTasks.Value;

	public TimeSpan ServiceTaskMaxWaitForResourceAvailability => SystemDataRegistry.Instance.ServiceTaskMaxWaitForResourceAvailability;
	public int ServiceTaskMaximumCpuLoadBeforeThrottlingTasks => SystemDataRegistry.Instance.ServiceTaskMaximumCpuLoadBeforeThrottlingTasks.Value;
	public decimal ServiceTaskMaximumDiskQueueLengthBeforeThrottlingTasks => SystemDataRegistry.Instance.ServiceTaskMaximumDiskQueueLengthBeforeThrottlingTasks.Value;

	public TimeSpan ServiceTaskConfigurationPollingInterval => SystemDataRegistry.Instance.ServiceTaskConfigurationPollingInterval;

	public int ServiceTaskHttpProcessorMaxThreads => SystemDataRegistry.Instance.ServiceTaskHttpProcessorMaxThreads.Value;

	public IReadOnlyDictionary<string, string> ServiceTaskRunnerSpecificGroup => DeserializeTaskSpecificGroup(SystemDataRegistry.Instance.EnableServiceTaskRunnerSpecificGrouping.Value, SystemDataRegistry.Instance.ServiceTaskRunnerSpecificGrouping.Value);

	static IReadOnlyDictionary<string, string> DeserializeTaskSpecificGroup(bool enabled, string taskSpecificGroupString)
	{
		if (!enabled)
		{
			return new Dictionary<string, string>();
		}

		try
		{
			var taskToGroupDictionary = JsonSerializer.Deserialize<Dictionary<string, IEnumerable<string>>>(taskSpecificGroupString);
			return taskToGroupDictionary
				.SelectMany(kvp => kvp.Value.Select(task => new KeyValuePair<string, string>(task, kvp.Key)))
				.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
		}
		catch (ArgumentException e)
		{
			return new Dictionary<string, string>() { { "ERROR", e.Message } };
		}
		catch (JsonException e)
		{
			return new Dictionary<string, string>() { { "ERROR", e.Message } };
		}
	}

	public bool SwitchToNewServiceTasksModule => SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.Value;
}
