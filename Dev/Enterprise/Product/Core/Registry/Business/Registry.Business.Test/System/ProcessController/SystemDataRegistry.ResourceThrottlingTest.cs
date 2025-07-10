using System;
using Enterprise.Integration;

namespace Enterprise.Registry.Business.Testing
{
	sealed partial class SystemDataRegistryTest
	{
		public void TestServiceTaskResourceThrottlingEnabled()
		{
			TestRegistryItem(
				ItemSet.ServiceTaskResourceThrottlingEnabled,
				"ServiceTaskResourceThrottlingEnabled",
				SystemDataRegistry.Categories.System_ProcessController_ResourceThrottling,
				"Enable Resource Throttling",
				"Enable Resource throttling to ensure CPU and disk IO are maintained within acceptable limits.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
				false);
		}

		public void TestServiceTaskMaximumCpuLoadBeforeThrottlingTasks()
		{
			TestRegistryItem(
				ItemSet.ServiceTaskMaximumCpuLoadBeforeThrottlingTasks,
				"ServiceTaskMaximumCpuLoadBeforeThrottlingTasks",
				SystemDataRegistry.Categories.System_ProcessController_ResourceThrottling,
				"Maximum CPU before throttling",
				"Maximum CPU allowable before delay loading service tasks.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
				80,
				0,
				100);
		}

		public void TestServiceTaskMaximumDiskQueueLengthBeforeThrottlingTasks()
		{
			TestRegistryItem(
				ItemSet.ServiceTaskMaximumDiskQueueLengthBeforeThrottlingTasks,
				"ServiceTaskMaximumDiskQueueLengthBeforeThrottlingTasks",
				SystemDataRegistry.Categories.System_ProcessController_ResourceThrottling,
				"Maximum Disk Queue Length before throttling",
				"Maximum Average Disk Queue Length before delay loading service tasks.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
				1,
				0,
				100);
		}

		public void TestServiceTaskMaxWaitForResourceAvailabilityInSeconds()
		{
			TestRegistryItem(
				ItemSet.ServiceTaskMaxWaitForResourceAvailabilityInSeconds,
				"ServiceTaskMaxWaitForResourceAvailabilityInSeconds",
				SystemDataRegistry.Categories.System_ProcessController_ResourceThrottling,
				"Maximum wait time to run task",
				"Maximum amount of time to wait for system resources availability before delaying the task start when host is under load (seconds).",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
				30,
				0,
				3600);
		}

		public void TestServiceTaskMaxWaitForResourceAvailability()
		{
			CombineAssertions(() =>
			{
				Test(1, TimeSpan.FromSeconds(1));
				Test(10, TimeSpan.FromSeconds(10));
				Test(100, TimeSpan.FromSeconds(100));
			});

			void Test(int value, TimeSpan expected)
			{
				using (SystemDataRegistry.Instance.ServiceTaskMaxWaitForResourceAvailabilityInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, value))
				{
					AssertEquals(expected, SystemDataRegistry.Instance.ServiceTaskMaxWaitForResourceAvailability);
				}
			}
		}
	}
}
