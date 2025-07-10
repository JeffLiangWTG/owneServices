using System;

namespace ServiceManager.Shared.Abstractions;

/// <summary>
/// All registry settings in the database that are used in this Shared project.
/// Use this instead of the registry directly.
/// <see cref="SharedRegistry"/>
/// </summary>
public interface ISharedRegistrySettings
{
	bool ServiceTaskBusinessObjectBindingEnabled { get; }
	bool ProductivityWiseModeEnabled { get; }
	TimeSpan ServiceTaskRequirementCheckTimeLimit { get; }
	// Queue Monitoring
	bool ProcessControllerQueueMonitoringEnabled { get; }
	TimeSpan ProcessControllerQueueMonitoringFrequency { get; }
	int ProcessControllerQueueMonitoringRetentionPeriodInDays { get; }
	bool SwitchRunnerToNetCore { get; }
}
