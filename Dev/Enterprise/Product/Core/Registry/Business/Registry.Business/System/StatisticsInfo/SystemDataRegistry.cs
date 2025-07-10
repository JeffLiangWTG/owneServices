using System;
using CargoWise.Common;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	partial class SystemDataRegistry
	{
		public CodePairRegistryItem StatisticsCollectionEnabled =>
			GetItem("StatisticsCollection",
				() => new CodePairRegistryItem(
					"StatisticsCollection",
					Categories.System_Statistics,
					ResString.GetMultilingualString("6D364ACE-51EC-43BA-A7F5-AFAEFC9CBEA1", "Collection Enabled"),
					ResString.GetMultilingualString("F573F041-4D5F-4F34-AA4E-390F36C556D8", "Specifies whether or not {0} should collate resource efficiency statistics.", Constants.ProductName),
					StatisticsEnabledListProvider,
					RegistryStorageFlags.All,
					RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
					(NoResString)"Disabled"));

		string ISystemDataRegistry.StatisticsCollectionEnabled
		{
			get
			{
				var company = Env.CurrentCompanyPK;
				var branch = Env.CurrentBranchPK;
				var department = Env.CurrentDepartmentPK;

				return StatisticsCollectionEnabled.GetFallBackValueAtAllLevels(company, branch, department);
			}

			set
			{
				StatisticsCollectionEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
			}
		}

		ICodeDescriptionPairListProvider StatisticsEnabledListProvider =>
			new CodeDescriptionPairListProvider(() => new CodeDescriptionPairList
			{
				new CodeDescriptionPair(nameof(EnabledState.Disabled), ResString.GetMultilingualString("648F9043-73A5-4FE2-9AC7-8410545671E9", "Disabled")),
				new CodeDescriptionPair(nameof(EnabledState.Simple), ResString.GetMultilingualString("648F9043-73A5-4FE2-9AC7-8410545671E8", "Simple")),
				new CodeDescriptionPair(nameof(EnabledState.Detailed), ResString.GetMultilingualString("648F9043-73A5-4FE2-9AC7-8410545671E7", "Detailed")),
			});

		public IntRegistryItem StatisticsMinimumRetentionRawDataDays =>
			GetItem("StatisticsMinimumRetentionRawDataDays",
				() => new IntRegistryItem(
					"StatisticsMinimumRetentionRawDataDays",
					Categories.System_Statistics,
					ResString.GetMultilingualString("A7C1D70E-BF54-40B9-89D1-4EF2E18E7D8B", "Minimum retention of raw usage data"),
					ResString.GetMultilingualString("8F50FBE0-8491-4E5D-ADA5-DD657E59AEBD", "The minimum number of days raw usage data will be stored (prior to any form of aggregation)"),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
					30,
					1,
					60));

		public IntRegistryItem StatisticsRemoveModuleIdAfterDays =>
			GetItem("StatisticsRemoveModuleIdAfterDays",
				() => new IntRegistryItem(
					"StatisticsRemoveModuleIdAfterDays",
					Categories.System_Statistics,
					ResString.GetMultilingualString("D3BD65FD-68D1-46DA-A248-60434BAC3EA2", "Minimum retention of Module Identifier information"),
					ResString.GetMultilingualString("C845844C-2541-4AAC-A490-CB900A888D85", "The minimum number of days Module Identifier information will be stored (prior to any form of aggregation)"),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
					60,
					1,
					365));

		public IntRegistryItem StatisticsRemoveBranchAfterDays =>
			GetItem("StatisticsRemoveBranchAfterDays",
				() => new IntRegistryItem(
					"StatisticsRemoveBranchAfterDays",
					Categories.System_Statistics,
					ResString.GetMultilingualString("359bcabd-e51e-4f90-9422-0a822bbaff24", "Minimum retention of Branch information"),
					ResString.GetMultilingualString("ec8efac4-b64c-439d-8018-d2e234a23159", "The minimum number of days Branch information will be stored (prior to any form of aggregation)"),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
					60,
					1,
					365));

		public IntRegistryItem StatisticsRemoveStaffAfterDays =>
			GetItem("StatisticsRemoveStaffAfterDays",
				() => new IntRegistryItem(
					"StatisticsRemoveStaffAfterDays",
					Categories.System_Statistics,
					ResString.GetMultilingualString("32d4671e-fd92-4281-abde-6da2401b001f", "Minimum retention of Staff information"),
					ResString.GetMultilingualString("9b9ed8c4-f1f1-4a34-920e-4dc59002d38b", "The minimum number of days Staff information will be stored (prior to any form of aggregation)"),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
					60,
					1,
					365));

		public StatisticsFoldupInfoRegistryItem StatisticsAggregationParameters =>
			GetItem("FoldingParameters",
				() => new StatisticsFoldupInfoRegistryItem(
					"FoldingParameters",
					Categories.System_Statistics,
					ResString.GetMultilingualString("00F94ECD-EC3B-46A2-9CC2-1D1F8625708F", "Data Aggregation"),
					ResString.GetMultilingualString("7784DF05-DE28-4449-B2A1-2F26B3469876", "How long to wait before data is aggregated, and the timespan to aggregate by"),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
					StatisticsFoldupInfoCollection.GetDefaultCollection()));

		public IntRegistryItem StatisticsTopRowCountForDelete =>
			GetItem("StatisticsTopRowCountForDelete",
				() => new IntRegistryItem(
					"StatisticsTopRowCountForDelete",
					Categories.System_Statistics,
					ResString.GetMultilingualString("769E9407-FE57-460D-B1CB-22E1728A11DA", "Top row count for Removing old data"),
					ResString.GetMultilingualString("EA70787E-8B58-4B16-834E-4F00BE1D8616", "Number of rows to be performed in one batch during removing old data"),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
					1000,
					1,
					1000000));

		public IntRegistryItem StatisticsTopRowCountForSummarise =>
			GetItem("StatisticsTopRowCountForSummarise",
				() => new IntRegistryItem(
					"StatisticsTopRowCountForSummarise",
					Categories.System_Statistics,
					ResString.GetMultilingualString("C10ADB95-42FF-4F8E-9604-121D54B964C1", "Top row count for Summarizing usage statistics"),
					ResString.GetMultilingualString("F7FBD06B-6ED5-4247-9AA0-8AEC279700E8", "Number of rows to be performed in one batch during summarizing usage statistics"),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
					1000,
					1,
					1000000));
	}
}
