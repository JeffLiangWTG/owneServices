using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.DbUpgrader.Foundation;

namespace Enterprise.DbUpgrader.Startup.Testing
{
	sealed class UpgradePreparationForTest : UpgradePreparation
	{
		public UpgradePreparationForTest(AdminConnection connection, bool shouldEnableCdc_Override)
			: base(connection)
		{
			SetFileGrowthSize = true;
			ShouldEnableCdc_Override = shouldEnableCdc_Override;
		}

		public UpgradePreparationForTest(AdminConnection connection)
			: base(connection)
		{
			SetFileGrowthSize = true;
		}

		public bool? BiDisableChangeDataCapture_OverrideValueForTest;

		public override bool IsCdcDisabledOnRegistry()
		{
			if (BiDisableChangeDataCapture_OverrideValueForTest.HasValue)
			{
				return BiDisableChangeDataCapture_OverrideValueForTest.Value;
			}
			else
			{
				return base.IsCdcDisabledOnRegistry();
			}
		}

		public void PerformNonTransactionalDatabaseSettings_Exposed(IUpgradeTaskWorkflowLogger logger, IEnumerable<string> allDatabases)
		{
			ApplyServerConfigurationSettings();
			SetAutoStatsOnModelDatabase();
			AlterStorageDocDatabaseAutoCreateStatistics();
			TurnOffMainDbStatistics();
			EnsureMainDatabaseSettings(logger);
			AdjustAllDatabasesRecoveryModels(logger);
			EnsureCargoWiseDatabaseSettings(allDatabases);
			EnableCdc(logger);
			ManageFilegroups();
		}

		protected override int? InitialFileSizeInGb => null;

		protected override int? CdcFilegroupGrowthSize => SetFileGrowthSize ? 2 : null;

		protected override string CdcFilegroupGrowthSizeUnit => "MB";

		protected override int? ReportFilegroupGrowthSize => SetFileGrowthSize ? 2 : null;

		protected override string ReportFilegroupGrowthSizeUnit => "MB";

		public bool SetFileGrowthSize { get; set; }

		public bool? ShouldEnableCdc_Override;
		public override bool ShouldEnableCdc()
		{
			if (ShouldEnableCdc_Override.HasValue)
			{
				return ShouldEnableCdc_Override.Value;
			}
			else
			{
				return base.ShouldEnableCdc();
			}
		}
	}
}
