using System;
using CargoWise.Bi.Common;
using CargoWise.Data;
using CargoWise.DbUpgrader.Foundation;
using Enterprise.DbUpgrader.Resource.Version;

namespace Enterprise.DbUpgrader.Startup
{
	class BiAuditUpgradePreparation : BusinessIntelligenceUpgradePreparation
	{
		public BiAuditUpgradePreparation(AdminConnection mainDbConnection, AdminConnection biConnection, IVersionChangeInfo upgVersionInfo)
			: base(mainDbConnection, biConnection, upgVersionInfo)
		{
		}

		protected override string BiDatabaseName => mainDbName + Db.AuditDatabaseSuffix;
		protected override string BiDatabaseType => "Audit";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Decrutification", "WTG3012:AvoidBoolLiteralsInLargerBoolExpressions", Justification = "#if directive in expression")]
		protected override bool MustRecreateBiDatabase
		{
			get
			{
				return false
#if DEBUG
					|| (
						NUnit.Framework.TestingState.IsRunningOnDAT &&
						!NUnit.Framework.TestingState.IsRunningTests &&
						upgVersionInfo.DbReferenceVersion_Schema.CompareTo(new VersionLabel(5830, 0)) < 0 // Remove audit partitioning on tables for DAT systems
					)
#endif
					;
			}
		}

		public override void PerformNonTransactionalSettings(IUpgradeTaskWorkflowLogger logger)
		{
			base.PerformNonTransactionalSettings(logger);
			ResetAuditVersionIfRequired(logger);
			EnsureAllPendingCdcDataIsLoadedIntoAuditDbBeforeSchemaSynchronisation(logger);
			logger.ShowInfoMessage(".");
		}

		#region Before Schema Synchronisation

		void ResetAuditVersionIfRequired(IUpgradeTaskWorkflowLogger logger)
		{
			var sqlText = "IF EXISTS (select * from sys.tables where name like '%[_]CT' and schema_name(schema_id) = 'cdc') SELECT 1 ELSE SELECT 0";
			var cdcTableListExists = Convert.ToBoolean(mainDbConnection.ExecuteScalar(sqlText));

			if (!cdcTableListExists)
			{
				logger.ShowInfoMessage("Resetting Audit schema version");
				DataUtils.DropDbExtendedProperty(biConnection, BiConstants.MainDbSchemaVersionExtPtyName, BiDatabaseName);
			}
		}

		void EnsureAllPendingCdcDataIsLoadedIntoAuditDbBeforeSchemaSynchronisation(IUpgradeTaskWorkflowLogger logger)
		{
			var etlRunner = new BusinessIntelligenceUpgradeEtlRunner(mainDbConnection, biConnection);
			etlRunner.CapturePendingCdcChangesAndFlushToAuditDatabase(logger);
		}

		#endregion
	}
}
