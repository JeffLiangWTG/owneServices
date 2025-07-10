using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.ServiceTasks.JobCostingReport;
using Moq;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Accounting.ServiceTasks.Testing
{
	class JCDDBObjectFunctionsTest : TestCaseWithFactory
	{
		public void TestDoesAllJCDDBObjectExist()
		{
			AssertEquals("Initial State", false, DBObjectChecker.DoesAllJCDDBObjectExist());

			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);

			var startAction = new JCDStartActionStrategy(TestConnection, new TestServiceLogger());
			startAction.Process();
			AssertEquals("Initial State", true, DBObjectChecker.DoesAllJCDDBObjectExist());
		}

		public void TestDBObjectInfoIsCached()
		{
			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);
			var jcdDBObjectChecker = new JCDDBObjectsChecker(TestConnection);
			// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
			using (TestConnection.TrackExecutedCommands())
			{
				var exist = jcdDBObjectChecker.DoesAllPermanentJCDDBObjectExist();
				Assert(!exist);
				Assert("Query is issued", TestConnection.ExecutedCommandsAndQueryPlans?.Any(t => t.Item1.Contains("--This is the query for loading JCD DB Object Info")) ?? false);
			}

			var startAction = new JCDStartActionStrategy(TestConnection, new TestServiceLogger());
			startAction.Process();

			using (TestConnection.TrackExecutedCommands())
			{
				var exist = jcdDBObjectChecker.DoesAllPermanentJCDDBObjectExist();
				Assert("Although DB Objects have been created, expected value is false, as cached value is used.", !exist);
				Assert("Query should not be issued", !TestConnection.ExecutedCommandsAndQueryPlans?.Any(t => t.Item1.Contains("--This is the query for loading JCD DB Object Info")) ?? false);
			}
		}

		public void TestDoesAllPermanentJCDDBObjectExist()
		{
			AssertEquals("Initial State", false, DBObjectChecker.DoesAllPermanentJCDDBObjectExist());

			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);

			var startAction = new JCDStartActionStrategy(TestConnection, new TestServiceLogger());
			startAction.Process();
			AssertEquals("After Creating all JCD DB Object", true, DBObjectChecker.DoesAllPermanentJCDDBObjectExist());

			var sync = new JCDDependentObjectSynchronizer(TestConnection, new TestServiceLogger(), JCDTempFunctionList.GetVersionManager());
			sync.DropObjects(0);
			JCDTableAndPartitionCreator.DeleteTempTableAndIndex(TestConnection, (s) => { });
			AssertEquals("After deleting all temporary Tables, functions and Procedures Object", true, DBObjectChecker.DoesAllPermanentJCDDBObjectExist());

			sync = new JCDDependentObjectSynchronizer(TestConnection, new TestServiceLogger(), JCDDependentObjectList.GetVersionManager());
			sync.DropObjects(0);
			AssertEquals("After deleting all functions and procedures", false, DBObjectChecker.DoesAllPermanentJCDDBObjectExist());
		}

		public void TestDoesAnyTempJCDDBObjectExist()
		{
			AssertEquals("Initial State", false, DBObjectChecker.DoesAnyTempJCDDBObjectExist());

			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);

			var startAction = new JCDStartActionStrategy(TestConnection, new TestServiceLogger());
			startAction.Process();
			AssertEquals("After Creating all JCD DB Object", true, DBObjectChecker.DoesAnyTempJCDDBObjectExist());

			var sync = new JCDDependentObjectSynchronizer(TestConnection, new TestServiceLogger(), JCDTempFunctionList.GetVersionManager());
			sync.DropObjects(0);
			AssertEquals("After deleting all temporary functions and Procedures Object. But temporary tables and Index still exist", true, DBObjectChecker.DoesAnyTempJCDDBObjectExist());

			JCDTableAndPartitionCreator.DeleteTempTableAndIndex(TestConnection, (s) => { });
			AssertEquals("After deleting all temporary tables and Index", false, DBObjectChecker.DoesAnyTempJCDDBObjectExist());
		}

		public void TestDoesAnyJCDDBObjectExist()
		{
			AssertEquals("Initial State", false, DBObjectChecker.DoesAnyTempJCDDBObjectExist());

			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);

			var startAction = new JCDStartActionStrategy(TestConnection, new TestServiceLogger());
			startAction.Process();
			AssertEquals("After Creating all JCD DB Object", true, DBObjectChecker.DoesAnyJCDDBObjectExist());

			var sync = new JCDDependentObjectSynchronizer(TestConnection, new TestServiceLogger(), JCDTempFunctionList.GetVersionManager());
			sync.DropObjects(0);
			AssertEquals("After deleting all temporary functions and Procedures Object. But temporary tables and Index still exist", true, DBObjectChecker.DoesAnyJCDDBObjectExist());

			JCDTableAndPartitionCreator.DeleteTempTableAndIndex(TestConnection, (s) => { });
			AssertEquals("After deleting all temporary tables and Index", true, DBObjectChecker.DoesAnyJCDDBObjectExist());

			sync = new JCDDependentObjectSynchronizer(TestConnection, new TestServiceLogger(), JCDDependentObjectList.GetVersionManager());
			sync.DropObjects(0);
			AssertEquals("After deleting functions and Procedures", true, DBObjectChecker.DoesAnyJCDDBObjectExist());

			JCDTableAndPartitionCreator.DeleteMainReportTableAndPartition(TestConnection, (s) => { });
			AssertEquals("After deleting all DB Objects", false, DBObjectChecker.DoesAnyJCDDBObjectExist());
		}

		public void TestDoesAllTempJCDTableExist()
		{
			AssertEquals("Initial State", false, DBObjectChecker.DoesAllTempJCDTableExist());

			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);

			var startAction = new JCDStartActionStrategy(TestConnection, new TestServiceLogger());
			startAction.Process();
			AssertEquals("After Creating all JCD DB Object", true, DBObjectChecker.DoesAllTempJCDTableExist());

			var sync = new JCDDependentObjectSynchronizer(TestConnection, new TestServiceLogger(), JCDTempFunctionList.GetVersionManager());
			sync.DropObjects(0);
			AssertEquals("After deleting all temporary functions and Procedures Object. But temporary tables and Index still exist", true, DBObjectChecker.DoesAllTempJCDTableExist());

			JCDTableAndPartitionCreator.DeleteTempTableAndIndex(TestConnection, (s) => { });
			AssertEquals("After deleting all temporary tables and Index", false, DBObjectChecker.DoesAllTempJCDTableExist());
		}

		public void TestDoesAllPermanentJCDTableAndPartitionExist()
		{
			AssertEquals("Initial State", false, DBObjectChecker.DoesAllPermanentJCDTableAndPartitionExist());

			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);

			var startAction = new JCDStartActionStrategy(TestConnection, new TestServiceLogger());
			startAction.Process();
			AssertEquals("After Creating all JCD DB Object", true, DBObjectChecker.DoesAllPermanentJCDTableAndPartitionExist());

			var sync = new JCDDependentObjectSynchronizer(TestConnection, new TestServiceLogger(), JCDTempFunctionList.GetVersionManager());
			sync.DropObjects(0);
			AssertEquals("After deleting all temporary functions and Procedures Object. But temporary tables and Index still exist", true, DBObjectChecker.DoesAllPermanentJCDTableAndPartitionExist());

			JCDTableAndPartitionCreator.DeleteTempTableAndIndex(TestConnection, (s) => { });
			AssertEquals("After deleting all temporary tables and Index", true, DBObjectChecker.DoesAllPermanentJCDTableAndPartitionExist());

			sync = new JCDDependentObjectSynchronizer(TestConnection, new TestServiceLogger(), JCDDependentObjectList.GetVersionManager());
			sync.DropObjects(0);
			AssertEquals("After deleting functions and Procedures", true, DBObjectChecker.DoesAllPermanentJCDTableAndPartitionExist());

			JCDTableAndPartitionCreator.DeleteMainReportTableAndPartition(TestConnection, (s) => { });
			AssertEquals("After deleting all tables and partition", false, DBObjectChecker.DoesAllPermanentJCDTableAndPartitionExist());
		}

		public void TestIsThereAnyUnprocessedOldALRecord()
		{
			AssertEquals("Initial State", false, DBObjectChecker.IsThereAnyUnprocessedOldALRecord());

			CreateTranasctionLine();

			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);

			var startAction = new JCDStartActionStrategy(TestConnection, new TestServiceLogger());
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				startAction.Process();
			}
			AssertEquals("After Creating all JCD DB Object and populating temp tables", true, DBObjectChecker.IsThereAnyUnprocessedOldALRecord());

			TestConnection.ExecuteNonQuery("DELETE FROM RptDtUnprocessedAccTransactionLines");

			AssertEquals("After deleting record from temp tables", false, DBObjectChecker.IsThereAnyUnprocessedOldALRecord());
		}

		public void TestIsThereAnyUnprocessedOldALRecordWhenOnlyOneTransactionLineInDB()
		{
			AssertEquals("Initial State", false, DBObjectChecker.IsThereAnyUnprocessedOldALRecord());

			ObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", ObjectCreator.AUD, 1m, 100m, 0m, 100m, 0m);
			Factory.Save();

			var sql = @"SELECT COUNT(1) FROM dbo.AccTransactionLines";
			using (var command = Db.Connection.Command(sql))
			{
				AssertEquals("Precondition - only 1 transaction line in database.", 1, command.ExecuteScalar());
			}

			AssertEquals("Precondition - JCDQueueHighWaterMark is 1.", 1, AccountingConfigurationRegistry.Instance.JCDQueueHighWaterMark.Value);

			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);

			var startAction = new JCDStartActionStrategy(TestConnection, new TestServiceLogger());
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				AssertNoExceptionThrown("Shouldn't throw RegistryValidationException", startAction.Process);
			}
			AssertEquals("After Creating all JCD DB Object and populating temp tables", true, DBObjectChecker.IsThereAnyUnprocessedOldALRecord());
		}

		public void TestIsThereAnyALRecord()
		{
			AssertEquals("Initial State", false, DBObjectChecker.IsThereAnyALRecord());

			CreateTranasctionLine();

			AssertEquals("Initial State", true, DBObjectChecker.IsThereAnyALRecord());
		}

		public void TestIsQueueEmpty()
		{
			CreateTranasctionLine();

			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);

			var startAction = new JCDStartActionStrategy(TestConnection, new TestServiceLogger());
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				startAction.Process();
			}

			AssertEquals("Initial State", true, DBObjectChecker.IsQueueEmpty());

			var processAction = new JCDDefaultActionStrategy(TestConnection, new TestServiceLogger());
			processAction.Process();

			AssertEquals("After POR task", false, DBObjectChecker.IsQueueEmpty());
		}

		public void TestIsReportTableEmpty()
		{
			CreateTranasctionLine();

			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);

			var startAction = new JCDStartActionStrategy(TestConnection, new TestServiceLogger());
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				startAction.Process();
			}

			AssertEquals("Initial State", true, DBObjectChecker.IsReportTableEmpty());

			var processAction = new JCDDefaultActionStrategy(TestConnection, new TestServiceLogger());
			processAction.Process();

			AssertEquals("After POR task", true, DBObjectChecker.IsReportTableEmpty());

			processAction = new JCDDefaultActionStrategy(TestConnection, new TestServiceLogger());
			processAction.Process();

			AssertEquals("After CUP task", false, DBObjectChecker.IsReportTableEmpty());
		}

		public void TestBehaviorWithTombStones()
		{
			var dbObjectChecker = new JCDDBObjectsChecker(TestConnection);
			CombineAssertions("Precondition: nothing exists", () =>
			{
				AssertEquals(nameof(dbObjectChecker.DoesAllPermanentJCDDBObjectExist), false, dbObjectChecker.DoesAllPermanentJCDDBObjectExist());
				AssertEquals(nameof(dbObjectChecker.DoesAllJCDDBObjectExist), false, dbObjectChecker.DoesAllJCDDBObjectExist());
				AssertEquals(nameof(dbObjectChecker.DoesAllTempJCDTableExist), false, dbObjectChecker.DoesAllTempJCDTableExist());
				AssertEquals(nameof(dbObjectChecker.DoesAllPermanentJCDTableAndPartitionExist), false, dbObjectChecker.DoesAllPermanentJCDTableAndPartitionExist());
			});

			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);

			var startAction = new JCDStartActionStrategy(TestConnection, new TestServiceLogger());
			startAction.Process();
			dbObjectChecker = new JCDDBObjectsChecker(TestConnection);
			CombineAssertions("After running Start Action Strategy, all DB objects should exist", () =>
			{
				AssertEquals(nameof(dbObjectChecker.DoesAllPermanentJCDDBObjectExist), true, dbObjectChecker.DoesAllPermanentJCDDBObjectExist());
				AssertEquals(nameof(dbObjectChecker.DoesAllJCDDBObjectExist), true, dbObjectChecker.DoesAllJCDDBObjectExist());
				AssertEquals(nameof(dbObjectChecker.DoesAllTempJCDTableExist), true, dbObjectChecker.DoesAllTempJCDTableExist());
				AssertEquals(nameof(dbObjectChecker.DoesAllPermanentJCDTableAndPartitionExist), true, dbObjectChecker.DoesAllPermanentJCDTableAndPartitionExist());
			});

			// Create a fake tomb-stoned object.
			TestConnection.ExecuteNonQuery($"CREATE VIEW dbo.{JCDDBObjectInfoList.Instance["RptDt_ViewJobCostingDataAmountByJob"].Name} AS SELECT 1 Name");

			dbObjectChecker = new JCDDBObjectsChecker(TestConnection);
			CombineAssertions("Even if tomb-stoned objects exist, all methods should return true", () =>
			{
				AssertEquals(nameof(dbObjectChecker.DoesAllPermanentJCDDBObjectExist), true, dbObjectChecker.DoesAllPermanentJCDDBObjectExist());
				AssertEquals(nameof(dbObjectChecker.DoesAllJCDDBObjectExist), true, dbObjectChecker.DoesAllJCDDBObjectExist());
				AssertEquals(nameof(dbObjectChecker.DoesAllTempJCDTableExist), true, dbObjectChecker.DoesAllTempJCDTableExist());
				AssertEquals(nameof(dbObjectChecker.DoesAllPermanentJCDTableAndPartitionExist), true, dbObjectChecker.DoesAllPermanentJCDTableAndPartitionExist());
			});

			TestConnection.ExecuteNonQuery($"DROP VIEW dbo.{JCDDBObjectInfoList.Instance["RptDt_ViewJobCostingDataAmountByJob"].Name}");
			var result = TestConnection.ExecuteScalar($"SELECT [name] FROM sys.objects where [name] = '{JCDDBObjectInfoList.Instance["RptDt_ViewJobCostingDataAmountByJob"].Name}'");
			AssertNull("Tomb-stoned object has been deleted.", result);

			dbObjectChecker = new JCDDBObjectsChecker(TestConnection);
			CombineAssertions("Even after tomb-stoned objects is removed, all methods should return true", () =>
			{
				AssertEquals(nameof(dbObjectChecker.DoesAllPermanentJCDDBObjectExist), true, dbObjectChecker.DoesAllPermanentJCDDBObjectExist());
				AssertEquals(nameof(dbObjectChecker.DoesAllJCDDBObjectExist), true, dbObjectChecker.DoesAllJCDDBObjectExist());
				AssertEquals(nameof(dbObjectChecker.DoesAllTempJCDTableExist), true, dbObjectChecker.DoesAllTempJCDTableExist());
				AssertEquals(nameof(dbObjectChecker.DoesAllPermanentJCDTableAndPartitionExist), true, dbObjectChecker.DoesAllPermanentJCDTableAndPartitionExist());
			});

			// Note: nothing in JCDTempFunctionList has been tomb-stoned (yet), so it is untested.
		}

		void CreateTranasctionLine()
		{
			var jobs = Helper.CreateJobsWithPeriod(false);

			var apLine1 = Helper.CreateCSTLine(jobs[0], 1, 113, reverse: true);
			var apLine2 = Helper.CreateCSTLine(jobs[1], 2, 123, reverse: true);

			var arLine1 = Helper.CreateREVLine(jobs[0], 1, 112, reverse: true);
			var arLine2 = Helper.CreateREVLine(jobs[1], 2, 122, reverse: true);

			var wipacr1 = Helper.CreateACRWIPLine(jobs[0], 1, 111, 110, reverseWIP: false, reverseACR: false);
			var wipacr2 = Helper.CreateACRWIPLine(jobs[1], 2, 121, 120, reverseWIP: false, reverseACR: false);
		}

		JCDDBObjectsChecker DBObjectChecker => new JCDDBObjectsChecker(TestConnection);

		protected JobCostingReportDataTestHelper Helper
		{
			get { return helper ?? (helper = new JobCostingReportDataTestHelper(ObjectCreator)); }
		}

		JobCostingReportDataTestHelper helper;

		protected TestObjectCreator ObjectCreator
		{
			get { return objectCreator ?? (objectCreator = new TestObjectCreator(Factory)); }
		}

		TestObjectCreator objectCreator;
	}
}
