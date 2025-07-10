using System;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.ServiceTasks.JobCostingReport;
using Enterprise.Environment;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Accounting.ServiceTasks.Testing
{
	public class JCDDependentObjectListTest : TestCaseWithFactory
	{
		public void TestFunctionsAndProceduresAreCreated()
		{
			AccountingConfigurationRegistry.Instance.JobCostingReportRelatedDBObjectVersion.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, -1);

			var logger = new TestServiceLogger();
			var versionManager = JCDDependentObjectList.GetVersionManager();
			var synchronizer = new DBObjectSynchronizer_ForTest(TestConnection, logger, versionManager);
			synchronizer.Synchronize();

			// Note that tomb-stoned objects should be dropped, but not created.
			var expectedLogMessages = @"
Debug|Dropping FUNCTION RptDt_Report_GlobalForwardingAndCustomsSummary
Debug|Dropping FUNCTION RptDt_Report_GlobalJobProfitSummaryByJob
Debug|Dropping PROCEDURE RptDtPopulateJobCostingDataFromQueue
Debug|Dropping TRIGGER RptDt_TG_AccTransactionLines_InsertToJobCostingDataQueue
Debug|Dropping FUNCTION RptDt_GlobalJobProfitReportCore
Debug|Dropping FUNCTION RptDt_GetPeriodKeys
Debug|Dropping INDEX RptDt_NX_JCA_OH_JCA_GC_JCA_JH
Debug|Dropping INDEX RptDt_NX_JCA_JH_JCA_GC_JCA_OH
Debug|Dropping INSERT INTO RptDtJobCostingDataAmountByJob
Debug|Dropping UNIQUE CLUSTERED INDEX RptDt_UC_JCA_PostPeriod_JCA_GC_JCA_OH_JCA_JH
Debug|Dropping UNIQUE CLUSTERED INDEX RptDt_UC_JCA_PostPeriod_JCA_GC_JCA_OH_JCA_JH_JCA_RX_NKLocalCurrency
Debug|Dropping TABLE RptDtJobCostingDataAmountByJob
Debug|Dropping INDEX RptDt_NI_JCD_OH_JCD_GC_JCD_JH
Debug|Dropping INDEX RptDt_NI_JCD_JH_JCD_GC_JCD_OH
Debug|Dropping UNIQUE CLUSTERED INDEX RptDt_CI_JCD_PostPeriod_JCD_GC_JCD_OH_JCD_JH
Debug|Dropping VIEW RptDt_ViewJobCostingDataAmountByJob
Debug|Creating TABLE RptDtJobCostingDataAmountByJob
Debug|Creating UNIQUE CLUSTERED INDEX RptDt_UC_JCA_PostPeriod_JCA_GC_JCA_OH_JCA_JH_JCA_RX_NKLocalCurrency
Debug|Creating INSERT INTO RptDtJobCostingDataAmountByJob
Debug|Creating INDEX RptDt_NX_JCA_JH_JCA_GC_JCA_OH
Debug|Creating INDEX RptDt_NX_JCA_OH_JCA_GC_JCA_JH
Debug|Creating FUNCTION RptDt_GetPeriodKeys
Debug|Creating FUNCTION RptDt_GlobalJobProfitReportCore
Debug|Creating TRIGGER RptDt_TG_AccTransactionLines_InsertToJobCostingDataQueue
Debug|Creating PROCEDURE RptDtPopulateJobCostingDataFromQueue
Debug|Creating FUNCTION RptDt_Report_GlobalJobProfitSummaryByJob
Debug|Creating FUNCTION RptDt_Report_GlobalForwardingAndCustomsSummary
Information|Functions, Stored Procedures and Dependent Tables are up-to-date.
".Trim();
			AssertMultilineASCIIEquals("Log messages", expectedLogMessages, logger.ToString());

			//Assert RptDtJobCostingDataAmountByJob is created
			var exists = Helper.Connection.ExecuteScalar<int>("SELECT ISNULL(OBJECT_ID('RptDtJobCostingDataAmountByJob'), -1)") > 0;
			AssertEquals("RptDtJobCostingDataAmountByJob should Exist", true, exists);

			//Assert RptDt_UC_JCA_PostPeriod_JCA_GC_JCA_OH_JCA_JH_JCA_RX_NKLocalCurrency is created
			exists = Helper.Connection.ExecuteScalar<int>("SELECT ISNULL(IndexProperty(OBJECT_ID('dbo.RptDtJobCostingDataAmountByJob'), 'RptDt_UC_JCA_PostPeriod_JCA_GC_JCA_OH_JCA_JH_JCA_RX_NKLocalCurrency', 'IndexID'), -1)") > 0;
			AssertEquals("RptDt_UC_JCA_PostPeriod_JCA_GC_JCA_OH_JCA_JH_JCA_RX_NKLocalCurrency should Exist", true, exists);

			//Assert RptDt_NX_JCA_JH_JCA_GC_JCA_OH is created
			exists = Helper.Connection.ExecuteScalar<int>("SELECT ISNULL(IndexProperty(OBJECT_ID('dbo.RptDtJobCostingDataAmountByJob'), 'RptDt_NX_JCA_JH_JCA_GC_JCA_OH', 'IndexID'), -1)") > 0;
			AssertEquals("RptDt_NX_JCA_JH_JCA_GC_JCA_OH should Exist", true, exists);

			//Assert RptDt_NX_JCA_OH_JCA_GC_JCA_JH is created
			exists = Helper.Connection.ExecuteScalar<int>("SELECT ISNULL(IndexProperty(OBJECT_ID('dbo.RptDtJobCostingDataAmountByJob'), 'RptDt_NX_JCA_OH_JCA_GC_JCA_JH', 'IndexID'), -1)") > 0;
			AssertEquals("RptDt_NX_JCA_OH_JCA_GC_JCA_JH should Exist", true, exists);

			//Assert RptDt_TG_AccTransactionLines_InsertToJobCostingDataQueue is created
			exists = Helper.Connection.ExecuteScalar<int>("SELECT ISNULL(OBJECT_ID('RptDt_TG_AccTransactionLines_InsertToJobCostingDataQueue'), -1)") > 0;
			AssertEquals("RptDt_TG_AccTransactionLines_InsertToJobCostingDataQueue should Exist", true, exists);

			//Assert RptDtPopulateJobCostingDataFromQueue is created
			exists = Helper.Connection.ExecuteScalar<int>("SELECT ISNULL(OBJECT_ID('RptDtPopulateJobCostingDataFromQueue'), -1)") > 0;
			AssertEquals("RptDtPopulateJobCostingDataFromQueue should Exist", true, exists);

			//Assert RptDt_Report_GlobalJobProfitSummaryByJob is created
			exists = Helper.Connection.ExecuteScalar<int>("SELECT ISNULL(OBJECT_ID('RptDt_Report_GlobalJobProfitSummaryByJob'), -1)") > 0;
			AssertEquals("RptDt_Report_GlobalJobProfitSummaryByJob should Exist", true, exists);

			//Assert RptDt_Report_GlobalForwardingAndCustomsSummary is created
			exists = Helper.Connection.ExecuteScalar<int>("SELECT ISNULL(OBJECT_ID('RptDt_Report_GlobalForwardingAndCustomsSummary'), -1)") > 0;
			AssertEquals("RptDt_Report_GlobalForwardingAndCustomsSummary should Exist", true, exists);

			//Assert RptDt_GetPeriodKeys is created
			exists = Helper.Connection.ExecuteScalar<int>("SELECT ISNULL(OBJECT_ID('RptDt_GetPeriodKeys'), -1)") > 0;
			AssertEquals("RptDt_GetPeriodKeys should Exist", true, exists);

			AssertEquals(JCDDependentObjectList.LATEST_VERSION, AccountingConfigurationRegistry.Instance.JobCostingReportRelatedDBObjectVersion.Value);
		}

		public void TestTombStonedObjectsAreDropped()
		{
			var logger = new TestServiceLogger();
			bool exists;

			using (AccountingConfigurationRegistry.Instance.JobCostingReportRelatedDBObjectVersion.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, -1))
			{
				var versionManager = JCDDependentObjectList.GetVersionManager();
				var synchronizer = new DBObjectSynchronizer_ForTest(TestConnection, logger, versionManager);
				synchronizer.Synchronize();

				exists = Helper.Connection.ExecuteScalar<int>("SELECT ISNULL(OBJECT_ID('dbo.RptDt_ViewJobCostingDataAmountByJob'), -1)") > 0;
				AssertEquals("RptDt_ViewJobCostingDataAmountByJob should not exist, as it has been tomb-stoned", false, exists);
			}

			TestConnection.ExecuteNonQuery($"CREATE VIEW dbo.{JCDDBObjectInfoList.Instance["RptDt_ViewJobCostingDataAmountByJob"].Name} AS SELECT 1 Name");
			exists = Helper.Connection.ExecuteScalar<int>("SELECT ISNULL(OBJECT_ID('dbo.RptDt_ViewJobCostingDataAmountByJob'), -1)") > 0;
			AssertEquals("Precondition: RptDt_ViewJobCostingDataAmountByJob should exist, as it is manually created", true, exists);

			using (AccountingConfigurationRegistry.Instance.JobCostingReportRelatedDBObjectVersion.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, -1))
			{
				var versionManager = JCDDependentObjectList.GetVersionManager();
				var synchronizer = new DBObjectSynchronizer_ForTest(TestConnection, logger, versionManager);
				synchronizer.Synchronize();

				exists = Helper.Connection.ExecuteScalar<int>("SELECT ISNULL(OBJECT_ID('dbo.RptDt_ViewJobCostingDataAmountByJob'), -1)") > 0;
				AssertEquals("RptDt_ViewJobCostingDataAmountByJob should not exist, as it has been dropped during upgrade", false, exists);
			}
		}

		public void TestAllCreatableObjectNameStartsWithRptDt()
		{
			var regEx = new Regex("(PROCEDURE|FUNCTION|TRIGGER|VIEW|INDEX|TABLE) RptDt[_|A-Z|a-z|0-9]*");
			var versionManager = JCDDependentObjectList.GetVersionManager();
			foreach (var obj in versionManager.DBObjects.Where(x => x.ShouldCreateOnSynchronize && x.DbObjectInfo.ObjectCategory == JCDDBObjectInfo.DbObjectCategory.Definition))
			{
				Assert($"Name matches with Script '{obj.CreateSQLText}' 'CREATE {obj.Name}'", obj.CreateSQLText.Trim().StartsWith($"CREATE {obj.Name}"));
				Assert("Name Starts with 'RptDt', so that it doesn't get removed by the regular DB upgrade process", regEx.IsMatch(obj.Name));
			}
		}

		public void TestInsertObjectsSetRemovePartitionSqlCommand()
		{
			var partitionInfo = new PartitionInfo(
					200101,
					"200101" + Env.CurrentCompanyPK + "AMKEY",
					new DateTime(2001, 01, 01),
					new DateTime(2001, 01, 31),
					Env.CurrentCompanyPK,
					"200101" + Env.CurrentCompanyPK + "PERIODKEY"
				);

			var insertObjects = JCDDependentObjectList.GetVersionManager().DBObjects.Where(x => x.DbObjectInfo.Type == JCDDBObjectInfo.INSERT);
			foreach (var obj in insertObjects)
			{
				var objAsInterface = (IDeleteDataWhenRemovePartition)obj;
				using (var cmd = TestConnection.Command("THIS MUST BE OVERRIDDEN IN THE OBJECT"))
				{
					objAsInterface.SetCommandForRemovePartition(cmd, partitionInfo);
					AssertNotEquals($"SQL objects defined as type 'INSERT' should also return SQL to remove data when a partition is dropped. Please implement {nameof(IDeleteDataWhenRemovePartition)}.{nameof(IDeleteDataWhenRemovePartition.SetCommandForRemovePartition)}() to return an appropriate DELETE statement.", "THIS MUST BE OVERRIDDEN IN THE OBJECT", cmd.CommandText);
				}
			}

			var allOtherObjects = JCDDependentObjectList.GetVersionManager().DBObjects.Where(x => x.DbObjectInfo.Type != JCDDBObjectInfo.INSERT);
			foreach (var obj in allOtherObjects)
			{
				var objAsInterface = obj as IDeleteDataWhenRemovePartition;
				AssertNull($"All non-INSERT SQL objects should make no changes when a partition is dropped. Do not implement {nameof(IDeleteDataWhenRemovePartition)}.", objAsInterface);
			}
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
