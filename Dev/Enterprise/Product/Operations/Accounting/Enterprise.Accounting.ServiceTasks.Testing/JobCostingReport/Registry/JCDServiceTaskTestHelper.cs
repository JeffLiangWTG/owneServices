using System;
using CargoWise.Data;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Accounting.ServiceTasks.Testing
{
	public class JCDServiceTaskTestHelper : IJCDServiceTaskTestHelper
	{
		public void InitialiseAndRunJCDServiceTask(DbConnection connection)
		{
			var logger = new TestServiceLogger();
			AccountingConfigurationRegistry.Instance.JCDQueueHighWaterMark.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);

			using (var task = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(connection) { ServiceLogger = logger })
			{
				if (AccountingConfigurationRegistry.Instance.JCDServiceTaskController.Value == JCDActionList.Codes.NotInitialized)
				{
					AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);

					task.RunTask();
				}
				else if (AccountingConfigurationRegistry.Instance.JCDServiceTaskController.Value == JCDActionList.Codes.CompletedOldTransactionLinesHaveBeenProcessed)
				{
					AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.ReInitializeJobCostingDataQueueServiceTask);
					task.RunTask();
				}

				task.RunTask();
				task.RunTask();
			}
		}

		public void RunJCDServiceTaskForTableCreation(DbConnection connection)
		{
			if (AccountingConfigurationRegistry.Instance.JCDServiceTaskController.Value == JCDActionList.Codes.NotInitialized)
			{
				AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);
				var logger = new TestServiceLogger();

				using (var task = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(connection) { ServiceLogger = logger })
				{
					task.RunTask();
				}
			}

			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.CompletedOldTransactionLinesHaveBeenProcessed);
		}
	}
}
