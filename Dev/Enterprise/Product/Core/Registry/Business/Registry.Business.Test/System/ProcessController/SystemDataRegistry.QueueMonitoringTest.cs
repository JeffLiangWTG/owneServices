using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Core.Constants;

namespace Enterprise.Registry.Business.Testing
{
	sealed partial class SystemDataRegistryTest
	{
		public void TestProcessControllerQueueMonitoringEnabled_Hosted()
		{
			EnvProxy.SetHostedLocationForTest("SYD");
			EnvProxy.SetIsInternalSystemForTest(false);
			AssertEquals(true, EnvProxy.IsHostedWithCargowise);
			TestGenericRegistryItem(
				ItemSet.ProcessControllerQueueMonitoringEnabled,
				"ProcessControllerQueueMonitoringEnable",
				SystemDataRegistry.Categories.System_ProcessController_QueueMonitoring,
				"Enable Queue Monitoring",
				"Activate Queue logging.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
				true);
		}

		public void TestProcessControllerQueueMonitoringEnabled_InternalSystem()
		{
			EnvProxy.SetHostedLocationForTest(LicenceConstants.NotHostedWithCargoWise);
			EnvProxy.SetIsInternalSystemForTest(true);
			AssertEquals(false, EnvProxy.IsHostedWithCargowise);
			TestGenericRegistryItem(
				ItemSet.ProcessControllerQueueMonitoringEnabled,
				"ProcessControllerQueueMonitoringEnable",
				SystemDataRegistry.Categories.System_ProcessController_QueueMonitoring,
				"Enable Queue Monitoring",
				"Activate Queue logging.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
				true);
		}

		public void TestProcessControllerQueueMonitoringEnabled_NotHostedAndNotInternal()
		{
			EnvProxy.SetHostedLocationForTest(LicenceConstants.NotHostedWithCargoWise);
			EnvProxy.SetIsInternalSystemForTest(false);
			AssertEquals(false, EnvProxy.IsHostedWithCargowise);
			TestGenericRegistryItem(
				ItemSet.ProcessControllerQueueMonitoringEnabled,
				"ProcessControllerQueueMonitoringEnable",
				SystemDataRegistry.Categories.System_ProcessController_QueueMonitoring,
				"Enable Queue Monitoring",
				"Activate Queue logging.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
				false);
		}

		public void TestProcessControllerQueueMonitoringEnabled_NotHostedAndUndefinedInternal()
		{
			EnvProxy.SetHostedLocationForTest(LicenceConstants.NotHostedWithCargoWise);
			EnvProxy.SetIsInternalSystemForTest(null);
			AssertEquals(false, EnvProxy.IsHostedWithCargowise);
			TestGenericRegistryItem(
				ItemSet.ProcessControllerQueueMonitoringEnabled,
				"ProcessControllerQueueMonitoringEnable",
				SystemDataRegistry.Categories.System_ProcessController_QueueMonitoring,
				"Enable Queue Monitoring",
				"Activate Queue logging.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
				false);
		}

		public void TestProcessControllerQueueMonitoringFrequencyInMinutes()
		{
			TestRegistryItem(
				ItemSet.ProcessControllerQueueMonitoringFrequencyInMinutes,
				"ProcessControllerQueueMonitoringFrequencyInMinutes",
				SystemDataRegistry.Categories.System_ProcessController_QueueMonitoring,
				"Frequency",
				"Sets how often (in minutes) a task's queue status should be logged.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
				15,
				1,
				1440);
		}

		public void TestProcessControllerQueueMonitoringRetentionPeriodInDays()
		{
			TestRegistryItem(
				ItemSet.ProcessControllerQueueMonitoringRetentionPeriodInDays,
				"ProcessControllerQueueMonitoringRetentionPeriodInDays",
				SystemDataRegistry.Categories.System_ProcessController_QueueMonitoring,
				"Retention Period",
				"Number of days to keep queue log files, older files are deleted from the file system.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
				4,
				1,
				60);
		}
	}
}
