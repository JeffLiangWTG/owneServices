using System;
using CargoWise.Application;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Testing
{
	sealed partial class SystemDataRegistryTest : RegistryItemSetTestCaseWithFactory<SystemDataRegistry>
	{
		public void TestEnableStatisticsFromCompanyLevel()
		{
			SystemDataRegistry.Instance.StatisticsCollectionEnabled.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "Detailed");

			AssertEquals("Detailed", ObjectFactory.Get<ISystemDataRegistry>().StatisticsCollectionEnabled);
		}

		public void TestStatisticsCollectionEnabled()
		{
			TestGenericRegistryItem(ItemSet.StatisticsCollectionEnabled,
				"StatisticsCollection",
				"System/Statistics",
				"Collection Enabled",
				$"Specifies whether or not {Core.Constants.ProductName} should collate resource efficiency statistics.",
				RegistryStorageFlags.All,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
				"Disabled");
		}

		public void TestStatisticsMinimumRetentionRawDataDays()
		{
			TestRegistryItem(ItemSet.StatisticsMinimumRetentionRawDataDays,
				"StatisticsMinimumRetentionRawDataDays",
				"System/Statistics",
				"Minimum retention of raw usage data",
				"The minimum number of days raw usage data will be stored (prior to any form of aggregation)",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
				30,
				1,
				60);
		}

		public void TestStatisticsRemoveModuleIdAfterDays()
		{
			TestRegistryItem(ItemSet.StatisticsRemoveModuleIdAfterDays,
				"StatisticsRemoveModuleIdAfterDays",
				"System/Statistics",
				"Minimum retention of Module Identifier information",
				"The minimum number of days Module Identifier information will be stored (prior to any form of aggregation)",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
				60,
				1,
				365);
		}

		public void TestStatisticsRemoveBranchAfterDays()
		{
			TestRegistryItem(ItemSet.StatisticsRemoveBranchAfterDays,
				"StatisticsRemoveBranchAfterDays",
				"System/Statistics",
				"Minimum retention of Branch information",
				"The minimum number of days Branch information will be stored (prior to any form of aggregation)",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
				60,
				1,
				365);
		}

		public void TestStatisticsRemoveStaffAfterDays()
		{
			TestRegistryItem(ItemSet.StatisticsRemoveStaffAfterDays,
				"StatisticsRemoveStaffAfterDays",
				"System/Statistics",
				"Minimum retention of Staff information",
				"The minimum number of days Staff information will be stored (prior to any form of aggregation)",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
				60,
				1,
				365);
		}

		public void TestStatisticsAggregationParameters()
		{
			TestGenericRegistryItem(ItemSet.StatisticsAggregationParameters,
				"FoldingParameters",
				"System/Statistics",
				"Data Aggregation",
				"How long to wait before data is aggregated, and the timespan to aggregate by",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController);
		}

		public void TestStatisticsTopRowCountForDelete()
		{
			TestRegistryItem(ItemSet.StatisticsTopRowCountForDelete,
				"StatisticsTopRowCountForDelete",
				"System/Statistics",
				"Top row count for Removing old data",
				"Number of rows to be performed in one batch during removing old data",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
				1000,
				1,
				1000000);
		}

		public void TestStatisticsTopRowCountForSummarise()
		{
			TestRegistryItem(ItemSet.StatisticsTopRowCountForSummarise,
				"StatisticsTopRowCountForSummarise",
				"System/Statistics",
				"Top row count for Summarizing usage statistics",
				"Number of rows to be performed in one batch during summarizing usage statistics",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
				1000,
				1,
				1000000);
		}
	}
}
