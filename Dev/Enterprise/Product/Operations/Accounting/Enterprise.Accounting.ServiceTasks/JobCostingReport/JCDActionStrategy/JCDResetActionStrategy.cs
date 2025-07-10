using System;
using CargoWise.Data;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.ServiceTasks.JobCostingReport;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.ServiceTasks
{
	public class JCDResetActionStrategy : JCDActionStrategy
	{
		public JCDResetActionStrategy(DbConnection connection, ILogger logger) : base(connection, logger)
		{
		}

		protected override bool CanPerform()
		{
			return DBObjectChecker.DoesAnyJCDDBObjectExist();
		}

		protected override void SynchronizeDBObjectsCore()
		{
			//Delete Existing DB Objects.
			DeleteAllDBObjects();

			var (_, startingPeriod) = PeriodCompanyPartitionUpdater.GetStartPeriod(connection);

			//Create tables, index and partitions
			JCDTableAndPartitionCreator.Create(connection, startingPeriod, s => logger.Log(LogType.Debug, s));

			//Set version number of DBObjectVersion in the registry to 0, as only tables, index and partitions are created.
			UpdateJCDDBObjectVesionNoInRegistryAfterTableAndPartitionCreation(startingPeriod);

			//Reloading latest DB Object Info after DB object synchronization
			ResetDBObjectChecker();

			//ask DBSynchronizer to create Functions, Procedures and Triggers
			base.SynchronizeDBObjectsCore();
		}

		protected override void ProcessData()
		{
			PopulateTempTables();
		}

		protected override bool IsThereAnyNewParitionToCreate()
		{
			return true;
		}

		protected override string GetCannotPerformMessage()
		{
			return (NoResString)"Job Costing Data Queue service task is already initialized.";
		}

		protected override void UpdateControllerRegistryStatusAfterDBSync()
		{
			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.CompletedAllDatabaseObjectsForJobCostingDataQueueHaveBeenCreated);
		}

		protected override bool IsNudgingRequired
		{
			get { return base.IsNudgingRequired || DBObjectChecker.IsThereAnyUnprocessedOldALRecord(); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "SQL command")]
		protected override void LogErrorIfCannotPerform()
		{
			ResetDBObjectChecker();

			if (!DBObjectChecker.DoesAnyJCDDBObjectExist())
			{
				if (!DBObjectChecker.IsQueueEmpty())
				{
					connection.ExecuteNonQuery("TRUNCATE TABLE dbo.JobCostingDataQueue");
				}
				AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);
			}
			else
			{
				base.LogErrorIfCannotPerform();
			}
		}
	}
}
