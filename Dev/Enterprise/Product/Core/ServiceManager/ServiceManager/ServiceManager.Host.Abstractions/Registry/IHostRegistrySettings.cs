using System;
using System.Collections.Generic;
using System.Diagnostics;
using ServiceManager.Shared.Abstractions;

namespace ServiceManager.Host.Abstractions
{
	/// <summary>
	/// All registry settings in the database that are used in this Host project and its dependencies.
	/// Use this instead of the registry directly.
	/// <see cref="Host.Core.HostRegistry"/>
	/// </summary>
	public interface IHostRegistrySettings : ISharedRegistrySettings
	{
		int ServiceTaskHostTerminatorFrequency { get; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		int ServiceTaskUnloadTimeoutInSeconds { get; }
		bool ServiceTaskRunnerConnectionPoolingEnabled { get; }
		int ServiceTaskProcessingMaximumBatchSize { get; }
		decimal ServiceTaskProcessingBatchSizeScalingFactor { get; }
		TimeSpan ServiceTaskProcessingBatchDelay { get; }

		bool ShowQueryStackTraceInProcessControllerEnabled { get; }
		ProcessPriorityClass RunnerProcessPriorityValue { get; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		int BusyRunnerWaitTimeInSeconds { get; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		int SecondaryProcessSpinUpDelayInSeconds { get; }

		TimeSpan ThrottlingPeriodForDispatchingLoopWithBacklogWaitingForMaxSecondary { get; }
		TimeSpan ThrottlingPeriodForDispatchingLoopWithAcceptableBacklog { get; }
		TimeSpan ThrottlingPeriodForDispatchingLoopWithExceedingBacklog { get; }
		TimeSpan ThrottlingTargetTimeToClearQueueBacklog { get; }

		int ServiceTaskMemoryConstraint { get; }
		string ForcefullyDisabledTasks { get; }

		TimeSpan ServiceTaskMaxWaitForResourceAvailability { get; }
		int ServiceTaskMaximumCpuLoadBeforeThrottlingTasks { get; }
		decimal ServiceTaskMaximumDiskQueueLengthBeforeThrottlingTasks { get; }

		TimeSpan ServiceTaskConfigurationPollingInterval { get; }

		int ServiceTaskHttpProcessorMaxThreads { get; }
		IReadOnlyDictionary<string, string> ServiceTaskRunnerSpecificGroup { get; }

		bool SwitchToNewServiceTasksModule { get; }
	}
}
