using System;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.Accounting;
using Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Functions.Accounting.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.ReportFunctions.Accounting.Testing
{
	[TestedType(typeof(csfn_AllJobProfitDetailCoreWithTaxExpense))]
	class csfn_AllJobProfitDetailCoreWithTaxExpenseTest : BiCreateScriptTest
	{
		public void TestJobInactive()
		{
			var companyPK = TestHelper.InsertCompany("CNY", "DCN");
			InitActiveAndInactiveJobs(companyPK);
			var result = GetResultSetAll(companyPK);
			AssertEquals("Total counts", 2, result.Rows.Count);
			AssertEquals(1, result.Select("JH_JobNum = 'S0001'").Length);
			AssertEquals(1, result.Select("JH_JobNum = 'S0002'").Length);
		}

		public void TestActiveStatusFiltering()
		{
			var companyPK = TestHelper.InsertCompany("CNY", "DCN");
			InitActiveAndInactiveJobs(companyPK);

			var dtWithAllLines = GetResultSetAll(companyPK, activeStatus: "All");
			var dtWithActiveLines = GetResultSetAll(companyPK, activeStatus: "Active");
			var dtWithInactiveLines = GetResultSetAll(companyPK, activeStatus: "Inactive");

			AssertActiveStatusCount(dtWithAllLines, "All", 2, 1, 1);
			AssertActiveStatusCount(dtWithActiveLines, "Active", 1, 1, 0);
			AssertActiveStatusCount(dtWithInactiveLines, "Inactive", 1, 0, 1);
		}

		void AssertActiveStatusCount(DataTable dt, string activeStatus, int allLinesCount, int activeLinesCount, int inactiveLinesCount)
		{
			AssertEquals($"Should have {allLinesCount} lines when active status filter is '{activeStatus}'", allLinesCount, dt.Rows.Count);
			AssertEquals($"Should have {activeLinesCount} lines of active job when active status filter is '{activeStatus}'", activeLinesCount, dt.Select("JH_IsActive = 1").Length);
			AssertEquals($"Should have {inactiveLinesCount} lines of inactive job when active status filter is '{activeStatus}'", inactiveLinesCount, dt.Select("JH_IsActive = 0").Length);
		}

		void InitActiveAndInactiveJobs(Guid companyPK)
		{
			TestHelper.CreateJobHeader("S0001", companyPK);
			TestHelper.CreateJobHeader("S0002", companyPK, isActive: 0);
			TestHelper.InsertGLTransactionLineExtended("WIP", companyPK, "2023-09-08", "2023-09-08");
			TestHelper.InsertGLTransactionLineExtended("WIP", companyPK, "2023-09-08", "", jobHeaderKey: 2);
		}

		public void TestReversedWIPandACR()
		{
			var companyPK = PrepareDataForReversedWIPACR();

			var result = GetResultSetAll(companyPK);
			AssertEquals("Total counts", 4, result.Rows.Count);
			AssertEquals(3, result.Select("JH_JobNum = 'S0001'").Length);
			AssertEquals(1, result.Select("JH_JobNum = 'S0002'").Length);

			result = GetResultSetAll(companyPK, "Y");
			AssertEquals("Total counts", 2, result.Rows.Count);
			AssertEquals(1, result.Select("JH_JobNum = 'S0002' and WIPAmount = 1").Length);
			AssertEquals(1, result.Select("JH_JobNum = 'S0001' and WIPAmount = 0").Length);
		}

		public void TestNotIncludeReversedWIPACR()
		{
			var companyPK = PrepareDataForReversedWIPACR();

			var result = GetResultSetAll(companyPK, notIncludeReversedWIPACR: "N");
			AssertEquals("Total counts", 4, result.Rows.Count);
			var date = new DateTime(2023, 9, 7);
			AssertEquals(1, result.Select("AL_LineType = 'ACR' and WIPAmount = 0").Length);
			AssertEquals(1, result.Select("AL_LineType = 'WIP' and WIPAmount = 1").Length);
			AssertEquals(1, result.Select("AL_LineType = 'REV' and WIPAmount = -1").Length);
			AssertEquals(1, result.Select("AL_LineType = 'WIP' and WIPAmount = 0").Length);

			result = GetResultSetAll(companyPK, notIncludeReversedWIPACR: "Y");
			AssertEquals("Total counts", 2, result.Rows.Count);
			AssertEquals(1, result.Select("AL_LineType = 'WIP' and WIPAmount = 1").Length);
			AssertEquals(1, result.Select("AL_LineType = 'REV' and WIPAmount = -1").Length);
		}

		Guid PrepareDataForReversedWIPACR()
		{
			var companyPK = TestHelper.InsertCompany("CNY", "DCN");
			TestHelper.CreateJobHeader("S0001", companyPK);
			TestHelper.CreateJobHeader("S0002", companyPK);
			TestHelper.InsertGLTransactionLineExtended("WIP", companyPK, "2023-09-08", "2023-09-08");
			TestHelper.InsertGLTransactionLineExtended("REV", companyPK, "2023-09-08", "2023-09-08");
			TestHelper.InsertGLTransactionLineExtended("ACR", companyPK, "2023-09-08", "2023-09-08", wipAmount: 0, acrAmount: 1);
			TestHelper.InsertGLTransactionLineExtended("WIP", companyPK, "2023-09-08", "", jobHeaderKey: 2);

			return companyPK;
		}

		protected override string ScriptDbName { get { return Db.EdwDatabaseName; } }

		DataTable GetResultSetAll(Guid companyPK, string outstandingWIP = "", string notIncludeReversedWIPACR = "", string activeStatus = "")
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, "SELECT * FROM [{0}].[dbo].[csfn_AllJobProfitDetailCoreWithTaxExpense]('{1}','1900-01-01 00:00','2060-01-1 00:00:00','','{2}','','{3}','','','',NULL,NULL,'','','','','','{4}')", ScriptDbName, companyPK, outstandingWIP, notIncludeReversedWIPACR, activeStatus);
			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		AccountingFunctionTestingHelper TestHelper => fTestHelper ?? (fTestHelper = new AccountingFunctionTestingHelper(TestConnection, ScriptDbName));
		AccountingFunctionTestingHelper fTestHelper;
	}
}
