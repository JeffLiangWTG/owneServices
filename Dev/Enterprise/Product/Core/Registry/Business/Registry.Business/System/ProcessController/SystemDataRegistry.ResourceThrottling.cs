using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	partial class SystemDataRegistry
	{
		#region SuppressResourceStringsCheckRegion

		public BooleanRegistryItem ServiceTaskResourceThrottlingEnabled =>
			GetItem("ServiceTaskResourceThrottlingEnabled", () =>
				new BooleanRegistryItem(
					"ServiceTaskResourceThrottlingEnabled",
					Categories.System_ProcessController_ResourceThrottling,
					ResString.GetMultilingualString("{F74442D6-587D-46F6-BEB2-A8110239B1B1}", "Enable Resource Throttling"),
					ResString.GetMultilingualString("{149688F4-0FCE-46DB-A5DB-2AA071FB6E36}", "Enable Resource throttling to ensure CPU and disk IO are maintained within acceptable limits."),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
					false)
			);

		public IntRegistryItem ServiceTaskMaximumCpuLoadBeforeThrottlingTasks =>
			GetItem("ServiceTaskMaximumCpuLoadBeforeThrottlingTasks", () =>
				new IntRegistryItem(
					"ServiceTaskMaximumCpuLoadBeforeThrottlingTasks",
					Categories.System_ProcessController_ResourceThrottling,
					ResString.GetMultilingualString("{C03061CB-78B3-479E-B24C-DB62331EA2FC}", "Maximum CPU before throttling"),
					ResString.GetMultilingualString("{E3351A47-8572-424F-B316-EBEC2FCDA149}", "Maximum CPU allowable before delay loading service tasks."),
					new NumericRegistryEditorInfo(0),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
					80,
					0,
					100)
			);

		public DecimalRegistryItem ServiceTaskMaximumDiskQueueLengthBeforeThrottlingTasks =>
			GetItem("ServiceTaskMaximumDiskQueueLengthBeforeThrottlingTasks", () =>
				new DecimalRegistryItem(
					"ServiceTaskMaximumDiskQueueLengthBeforeThrottlingTasks",
					Categories.System_ProcessController_ResourceThrottling,
					ResString.GetMultilingualString("{4B8B47DA-5CD2-47B4-A8E3-91A3E2284328}", "Maximum Disk Queue Length before throttling"),
					ResString.GetMultilingualString("{7B9A131F-B4EC-4DD6-B170-E495249A6A71}", "Maximum Average Disk Queue Length before delay loading service tasks."),
					new NumericRegistryEditorInfo(2),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
					1,
					0,
					100)
			);

		public IntRegistryItem ServiceTaskMaxWaitForResourceAvailabilityInSeconds =>
			GetItem("ServiceTaskMaxWaitForResourceAvailabilityInSeconds", () =>
				new IntRegistryItem(
					"ServiceTaskMaxWaitForResourceAvailabilityInSeconds",
					Categories.System_ProcessController_ResourceThrottling,
					ResString.GetMultilingualString("{580F3353-B9D7-49A0-A48D-DBC8B9207EDE}", "Maximum wait time to run task"),
					ResString.GetMultilingualString("{B9B994A2-E5A6-43C7-A91D-9E3B00E8DA7A}", "Maximum amount of time to wait for system resources availability before delaying the task start when host is under load (seconds)."),
					new NumericRegistryEditorInfo(0),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
					30,
					0,
					3600)
			);

		public TimeSpan ServiceTaskMaxWaitForResourceAvailability => TimeSpan.FromSeconds(ServiceTaskMaxWaitForResourceAvailabilityInSeconds.Value);

		#endregion SuppressResourceStringsCheckRegion
	}
}
