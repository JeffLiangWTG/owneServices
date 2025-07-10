using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.ComplianceReport;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.ComplianceReport
{
	[TestedType(typeof(QueueForComplianceReport_GJLRJL))]
	class QueueForComplianceReport_GJLRJLTest : DbCreateScriptTest
	{
		public void TestSampleCall()
		{
			var helper = new TestDbHelper(TestConnection);

			var branchPK = helper.InsertBranch("ZZB", TestDbHelper.DefaultCompanyPK);
			var departmentPK = helper.InsertDepartment("ZZD");
			InsertTransactions(helper, branchPK, departmentPK);

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("EXEC QueueForComplianceReport_GJLRJL 'LIB', '{0}', NULL, 'Oct 31 2015', 'Nov 10 2015', 'OPN', 'CLS'", TestDbHelper.DefaultCompanyPK));
			AssertEquals("Result should not have rows", 0, result.Rows.Count);

			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.AccTransactionComplianceReportQueue");
			AssertEquals("Result should have rows", 10, result.Rows.Count);

			var expectedCodes = new string[] {
				"*GL*GJL**",
				"*GL*RJL**",
				"*GL*RJL**Rev-" };

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(
				@"SELECT ACQ_ReportSubCode FROM dbo.AccTransactionComplianceReportQueue
				WHERE ACQ_Date >= 'Oct 31 2015' AND ACQ_Date <= 'Nov 9 2015'
				AND ACQ_ReportType = 'LIB' AND ACQ_ParentTableCode = 'AL' 
				AND  ACQ_GC_Company = '{0}' AND ACQ_GB_Branch = '{1}'",
				TestDbHelper.DefaultCompanyPK, branchPK));
			AssertEquals("Result should have rows", 10, result.Rows.Count);
			AssertContainsExactElementsInAnyOrder("ReportSubCodes", expectedCodes, result.Rows.Cast<DataRow>().Select(x => x["ACQ_ReportSubCode"].ToString().TrimEnd(' ')).Distinct().ToArray());
		}

		public static void InsertTransactions(TestDbHelper helper, Guid branchPK, Guid departmentPK)
		{
			var glAccount1PK = helper.InsertGLAccount("1234.56.07", "TestGLAccount 7");
			var glAccount2PK = helper.InsertGLAccount("1234.56.08", "TestGLAccount 8");

			var gjlPK = helper.InsertTransactionHeader("GL", "GJL", "001", 0, helper.ToDate("2015-11-09"), branchPK, departmentPK);
			helper.InsertTransactionLine(gjlPK, null, null, glAccount1PK, branchPK, departmentPK, null, 100, "GJL", helper.ToDate("2015-11-09"), null);
			helper.InsertTransactionLine(gjlPK, null, null, glAccount2PK, branchPK, departmentPK, null, -100, "GJL", helper.ToDate("2015-11-09"), null);

			var rjlPK = helper.InsertTransactionHeader("GL", "RJL", "002", 0, helper.ToDate("2015-10-31 23:59:00"), branchPK, departmentPK, null, helper.ToDate("2015-11-01"));
			helper.InsertTransactionLine(rjlPK, null, null, glAccount1PK, branchPK, departmentPK, null, 200, "RJL", helper.ToDate("2015-10-31 23:59:00"), helper.ToDate("2015-11-01"));
			helper.InsertTransactionLine(rjlPK, null, null, glAccount2PK, branchPK, departmentPK, null, -200, "RJL", helper.ToDate("2015-10-31 23:59:00"), helper.ToDate("2015-11-01"));

			//presentation journal
			var openingPK = helper.InsertTransactionHeader("GL", "GJL", "003", 0, helper.ToDate("2015-11-01"), branchPK, departmentPK, null, null, 1, "OPN");
			helper.InsertTransactionLine(openingPK, null, null, glAccount1PK, branchPK, departmentPK, null, 100, "GJL", helper.ToDate("2015-11-01"), null);
			helper.InsertTransactionLine(openingPK, null, null, glAccount2PK, branchPK, departmentPK, null, -100, "GJL", helper.ToDate("2015-11-01"), null);

			var closingPK = helper.InsertTransactionHeader("GL", "GJL", "004", 0, helper.ToDate("2015-11-09"), branchPK, departmentPK, null, null, 1, "CLS");
			helper.InsertTransactionLine(closingPK, null, null, glAccount1PK, branchPK, departmentPK, null, 1000, "GJL", helper.ToDate("2015-11-09"), null);
			helper.InsertTransactionLine(closingPK, null, null, glAccount2PK, branchPK, departmentPK, null, -1000, "GJL", helper.ToDate("2015-11-09"), null);
		}
	}
}

