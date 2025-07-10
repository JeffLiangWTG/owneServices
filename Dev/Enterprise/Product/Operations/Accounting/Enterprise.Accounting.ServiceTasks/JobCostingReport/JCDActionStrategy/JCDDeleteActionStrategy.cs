using System;
using CargoWise.Data;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.ServiceTasks
{
	public class JCDDeleteActionStrategy : JCDActionStrategy
	{
		public JCDDeleteActionStrategy(DbConnection connection, ILogger logger) : base(connection, logger)
		{
		}

		protected override bool CanPerform()
		{
			return DBObjectChecker.DoesAnyJCDDBObjectExist();
		}

		protected override void SynchronizeDBObjectsCore()
		{
			DeleteAllDBObjects();
		}

		protected override void SynchronizePartitionKeysWithAccountingPeriods()
		{
			// Nothing to do, as we are removing Partitions in SynchronizeDBObjectsCore.
		}

		protected override void ProcessData()
		{
			//Nothing To do
		}

		protected override string GetCannotPerformMessage()
		{
			return (NoResString)"Job Costing Data Queue service task is already initialized.";
		}

		protected override void UpdateControllerRegistryStatusAfterDBSync()
		{
			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.NotInitialized);
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
				UpdateControllerRegistryStatusAfterDBSync();
			}
			else
			{
				base.LogErrorIfCannotPerform();
			}
		}
	}
}
