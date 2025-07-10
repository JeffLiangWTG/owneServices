using System;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.ServiceTasks.JobCostingReport;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Accounting.ServiceTasks.Testing
{
	public class JCDTempFunctionListTest : TestCaseWithFactory
	{
		public void TestTempFunctionsAndProceduresAreCreated()
		{
			Helper.DropJobCostingDataTables();

			using (AccountingConfigurationRegistry.Instance.JobCostingQueueDBObjectVersion.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, -1))
			{
				var logger = new TestServiceLogger();

				var versionManager = JCDTempFunctionList.GetVersionManager();
				var synchronizer = new DBObjectSynchronizer_ForTest(TestConnection, logger, versionManager);
				synchronizer.Synchronize();

				AssertEquals("Log Count", 5, logger.Count);
				AssertEquals("Debug|Dropping PROCEDURE RptDtTransformAccTransactionLineToJobCostingQueueRecord", logger[0]);
				AssertEquals("Debug|Dropping TRIGGER RptDt_TG_AccTransactionLines_InsertToReversedLinesTable", logger[1]);
				AssertEquals("Debug|Creating TRIGGER RptDt_TG_AccTransactionLines_InsertToReversedLinesTable", logger[2]);
				AssertEquals("Debug|Creating PROCEDURE RptDtTransformAccTransactionLineToJobCostingQueueRecord", logger[3]);
				AssertEquals("Information|Temporary DB Objects are up-to-date.", logger[4]);

				//Assert SP is created
				var spExists = Helper.Connection.ExecuteScalar<int>("SELECT ISNULL(OBJECT_ID('RptDtTransformAccTransactionLineToJobCostingQueueRecord'), -1)") > 0;
				AssertEquals("RptDtTransformAccTransactionLineToJobCostingQueueRecord should Exist", true, spExists);

				//Assert Trigger is created
				var triggerExists = Helper.Connection.ExecuteScalar<int>("SELECT ISNULL(OBJECT_ID('RptDt_TG_AccTransactionLines_InsertToReversedLinesTable'), -1)") > 0;
				AssertEquals("RptDt_TG_AccTransactionLines_InsertToReversedLinesTable should Exist", true, triggerExists);

				AssertEquals(JCDTempFunctionList.LATEST_VERSION, AccountingConfigurationRegistry.Instance.JobCostingQueueDBObjectVersion.Value);
			}
		}

		public void TestAllObjectNameStartsWithRptDt()
		{
			var regEx = new Regex("(PROCEDURE|FUNCTION|TRIGGER) RptDt[_|A-Z|a-z|0-9]*");
			var versionManager = JCDTempFunctionList.GetVersionManager();
			versionManager.DBObjects.ToList().ForEach((obj) =>
			{
				Assert("Name matches with Script", obj.CreateSQLText.StartsWith(string.Format("CREATE {0}", obj.Name)));
				Assert("Name Starts with RptDt", regEx.IsMatch(obj.Name));
			});
		}

		protected override void SetUp()
		{
			JCDTableAndPartitionCreator.DeleteTempTableAndIndex(TestConnection, (s) => { });

			JCDTableAndPartitionCreator.DeleteMainReportTableAndPartition(TestConnection, (s) => { });

			JCDTableAndPartitionCreator.Create(TestConnection, 0, (s) => { });
		}

		JobCostingReportDataTestHelper Helper
		{
			get { return helper ?? (helper = new JobCostingReportDataTestHelper(ObjectCreator)); }
		}
		JobCostingReportDataTestHelper helper;

		TestObjectCreator ObjectCreator
		{
			get { return objectCreator ?? (objectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator objectCreator;
	}
}
