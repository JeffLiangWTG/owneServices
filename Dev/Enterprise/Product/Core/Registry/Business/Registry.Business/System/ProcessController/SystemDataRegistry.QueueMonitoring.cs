using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	partial class SystemDataRegistry
	{
		public BooleanRegistryItem ProcessControllerQueueMonitoringEnabled =>
			GetItem("ProcessControllerQueueMonitoringEnable",
				() => new BooleanRegistryItem(
					"ProcessControllerQueueMonitoringEnable",
					Categories.System_ProcessController_QueueMonitoring,
					ResString.GetMultilingualString("{D547C9E2-37E3-4E26-BACF-E19AAE78B408}", "Enable Queue Monitoring"),
					ResString.GetMultilingualString("{692B51D2-B748-46CE-9382-08C266D6F7EA}", "Activate Queue logging."),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
					EnvProxy.IsHostedWithCargowise || (EnvProxy.IsInternalSystem ?? false)));

		public IntRegistryItem ProcessControllerQueueMonitoringFrequencyInMinutes =>
			GetItem("ProcessControllerQueueMonitoringFrequencyInMinutes",
				() => new IntRegistryItem(
					"ProcessControllerQueueMonitoringFrequencyInMinutes",
					Categories.System_ProcessController_QueueMonitoring,
					ResString.GetMultilingualString("{EA46394D-798D-48A3-AF45-F888F291A930}", "Frequency"),
					ResString.GetMultilingualString("{C43881C7-C874-4C6C-AA2E-9BF1DE494D54}", "Sets how often (in minutes) a task's queue status should be logged."),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
					15,
					1,
					1440));

		public TimeSpan ProcessControllerQueueMonitoringFrequency => TimeSpan.FromMinutes(ProcessControllerQueueMonitoringFrequencyInMinutes.Value);

		public IntRegistryItem ProcessControllerQueueMonitoringRetentionPeriodInDays =>
			GetItem("ProcessControllerQueueMonitoringRetentionPeriodInDays",
				() => new IntRegistryItem(
					"ProcessControllerQueueMonitoringRetentionPeriodInDays",
					Categories.System_ProcessController_QueueMonitoring,
					ResString.GetMultilingualString("{7038EABB-D58E-472B-BD58-6F1D1873A887}", "Retention Period"),
					ResString.GetMultilingualString("{05B68C1C-2239-4260-B6B6-29F03EFE082F}", "Number of days to keep queue log files, older files are deleted from the file system."),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
					4,
					1,
					60));
	}
}
