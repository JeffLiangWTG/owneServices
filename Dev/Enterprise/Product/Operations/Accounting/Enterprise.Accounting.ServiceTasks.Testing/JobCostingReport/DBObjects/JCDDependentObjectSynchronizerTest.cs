using System;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.ServiceTasks.JobCostingReport;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.ServiceTasks.Testing
{
	public class JCDDependentObjectSynchronizerTest : TestCaseWithFactory
	{
		public void TestVersionIsUpdatedCorrectlyAfterSynchronizingObjects()
		{
			using (AccountingConfigurationRegistry.Instance.JobCostingReportRelatedDBObjectVersion.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				AssertEquals(0, AccountingConfigurationRegistry.Instance.JobCostingReportRelatedDBObjectVersion.Value);

				var versionManager = JCDDependentObjectList.GetVersionManager();
				var synchronizer = new DBObjectSynchronizer_ForTest(TestConnection, new LoggerForTest(), versionManager);
				synchronizer.Synchronize();
				AssertEquals(JCDDependentObjectList.LATEST_VERSION, AccountingConfigurationRegistry.Instance.JobCostingReportRelatedDBObjectVersion.Value);

				synchronizer = new DBObjectSynchronizer_ForTest(TestConnection, new LoggerForTest(), versionManager);
				AssertEquals(JCDDependentObjectList.LATEST_VERSION, AccountingConfigurationRegistry.Instance.JobCostingReportRelatedDBObjectVersion.Value);
			}
		}

		public void TestVersionIsUpdatedCorrectlyAfterDroppingObjects()
		{
			var versionManager = JCDDependentObjectList.GetVersionManager();
			var synchronizer = new DBObjectSynchronizer_ForTest(TestConnection, new LoggerForTest(), versionManager);
			synchronizer.Synchronize();
			AssertEquals(JCDDependentObjectList.LATEST_VERSION, AccountingConfigurationRegistry.Instance.JobCostingReportRelatedDBObjectVersion.Value);

			synchronizer.DropObjects(0);

			AssertEquals(0, AccountingConfigurationRegistry.Instance.JobCostingReportRelatedDBObjectVersion.Value);
		}

		public void TestVersionIsRollbackedCorrectlyIfAnyErrorOccursDuringSynchronizingObjects()
		{
			var synchronizer = new DBObjectSynchronizer_ForTest(TestConnection, new LoggerForTest(), JCDDependentObjectList.GetVersionManager());
			using (synchronizer.SuspendDBObjectCreation.GetSuspender())
			{
				try
				{
					synchronizer.Synchronize(); // Just for test only so that synchronizer attempts to update
				}
				catch
				{
					AssertEquals(0, AccountingConfigurationRegistry.Instance.JobCostingReportRelatedDBObjectVersion.Value);
				}
			}
		}

		public void TestVersionIsRollbackedCorrectlyIfAnyErrorOccursDuringDroppingObjects()
		{
			//Create JCD related DB Objects and update Version No to Latest.
			new JCDDependentObjectSynchronizer(TestConnection, new LoggerForTest(), JCDDependentObjectList.GetVersionManager()).Synchronize();

			//Now try to drop all JCD related DB Objects and test whether Version No remains unchanged if any error occurs during deletion process
			var synchronizer = new DBObjectSynchronizer_ForTest(TestConnection, new LoggerForTest(), JCDDependentObjectList.GetVersionManager());
			using (synchronizer.SuspendDBObjectCreation.GetSuspender())
			{
				try
				{
					synchronizer.DropObjects(0); // Just for test only so that synchronizer attempts to drop object
				}
				catch
				{
					AssertEquals(JCDDependentObjectList.LATEST_VERSION, AccountingConfigurationRegistry.Instance.JobCostingReportRelatedDBObjectVersion.Value);
				}
			}
		}

		protected override void SetUp()
		{
			JCDTableAndPartitionCreator.DeleteTempTableAndIndex(TestConnection, (s) => { });

			JCDTableAndPartitionCreator.DeleteMainReportTableAndPartition(TestConnection, (s) => { });

			JCDTableAndPartitionCreator.Create(TestConnection, 0, (s) => { });

			AccountingConfigurationRegistry.Instance.JobCostingReportRelatedDBObjectVersion.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
		}
	}

	public class DBObjectSynchronizer_ForTest : JCDDependentObjectSynchronizer
	{
		public DBObjectSynchronizer_ForTest(DbConnection connection, ILogger logger, JCDDBObjectVersionManager manager) : base(connection, logger, manager)
		{
		}

		public void CreateDBObjects(params JCDDependentDBObject[] dbObject)
		{
			foreach (var item in dbObject)
			{
				RunScriptToCreateDBObject(item);
			}
		}

		protected override void RunScriptToCreateDBObject(JCDDependentDBObject dbObject)
		{
			if (SuspendDBObjectCreation.IsSuspended)
			{
				throw new Exception("Forced Error");
			}
			else
			{
				base.RunScriptToCreateDBObject(dbObject);
			}
		}

		protected override void RunScriptToDropDBObject(JCDDependentDBObject dbObject)
		{
			if (SuspendDBObjectCreation.IsSuspended)
			{
				throw new Exception("Forced Error");
			}
			else
			{
				base.RunScriptToDropDBObject(dbObject);
			}
		}

		public FunctionalitySuspender SuspendDBObjectCreation
		{
			get { return suspendDBObjectCreation ?? (suspendDBObjectCreation = new FunctionalitySuspender()); }
		}
		FunctionalitySuspender suspendDBObjectCreation;
	}
}
