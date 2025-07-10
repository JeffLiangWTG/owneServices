using System;
using CargoWise.Data;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.ServiceTasks.JobCostingReport;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.ServiceTasks
{
	public class JCDStartActionStrategy : JCDActionStrategy
	{
		public JCDStartActionStrategy(DbConnection connection, ILogger logger) : base(connection, logger)
		{
		}

		protected override bool CanPerform()
		{
			return (AccountingConfigurationRegistry.Instance.JCDServiceTaskController.Value == JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask)
					|| (AccountingConfigurationRegistry.Instance.JCDServiceTaskController.Value == JCDActionList.Codes.CompletedAllDatabaseObjectsForJobCostingDataQueueHaveBeenCreated);
		}

		protected override void SynchronizeDBObjectsCore()
		{
			if (AccountingConfigurationRegistry.Instance.JCDServiceTaskController.Value == JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask)
			{
				var (_, startingPeriod) = PeriodCompanyPartitionUpdater.GetStartPeriod(connection);
				JCDTableAndPartitionCreator.Create(connection, startingPeriod, logText => logger.Log(LogType.Debug, logText));
				UpdateJCDDBObjectVesionNoInRegistryAfterTableAndPartitionCreation(startingPeriod);

				//Reloading latest DB Object Info after DB object synchronization
				ResetDBObjectChecker();
			}

			//ask DBSynchronizer to create DBObjects
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
			string msg = (NoResString)"Job Costing Data Queue service task is already initialized. So cannot be initialized again. If you want to start from fresh, please choose 'RST' action instead.";
			msg += FormattableString.Invariant($"\r\n Job Costing Data Queue Service Task Controller Registry Value: {AccountingConfigurationRegistry.Instance.JCDServiceTaskController.Value}");
			msg += FormattableString.Invariant($"\r\n Missing DB Object Names: {DBObjectChecker.GetMissingDBObjectNames()}");
			return msg;
		}

		protected override void UpdateControllerRegistryStatusAfterDBSync()
		{
			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.CompletedAllDatabaseObjectsForJobCostingDataQueueHaveBeenCreated);
		}

		protected override bool IsNudgingRequired
		{
			get { return base.IsNudgingRequired || DBObjectChecker.IsThereAnyUnprocessedOldALRecord(); }
		}
	}
}
