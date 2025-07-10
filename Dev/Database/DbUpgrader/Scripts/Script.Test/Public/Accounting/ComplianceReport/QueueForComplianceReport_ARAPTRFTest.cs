using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.ComplianceReport;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.ComplianceReport
{
	[TestedType(typeof(QueueForComplianceReport_ARAPTRF))]
	class QueueForComplianceReport_ARAPTRFTest : DbCreateScriptTest
	{
		public void TestSampleCall()
		{
			var helper = new TestDbHelper(TestConnection);

			var branchPK = helper.InsertBranch("ZZB", TestDbHelper.DefaultCompanyPK);
			var departmentPK = helper.InsertDepartment("ZZD");
			InsertTransactions(helper, branchPK, departmentPK);

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("EXEC QueueForComplianceReport_ARAPTRF 'LIB', '{0}', NULL, 'Nov 9 2015', 'Nov 10 2015'", TestDbHelper.DefaultCompanyPK));
			AssertEquals("Result should not have rows", 0, result.Rows.Count);

			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.AccTransactionComplianceReportQueue");
			AssertEquals("Result should have rows", 4, result.Rows.Count);

			var expectedCodes = new string[] {
				"*AR*TRF*ARCtrl*",
				"*AP*TRF*APCtrl*" };

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT ACQ_ReportSubCode FROM dbo.AccTransactionComplianceReportQueue  WHERE ACQ_Date = 'Nov 9 2015' AND ACQ_ReportType = 'LIB' AND ACQ_ParentTableCode = 'AH' AND  ACQ_GC_Company = '{0}' AND ACQ_GB_Branch = '{1}'",
				TestDbHelper.DefaultCompanyPK, branchPK));
			AssertEquals("Result should have rows", 4, result.Rows.Count);
			AssertContainsExactElementsInAnyOrder("ReportSubCodes", expectedCodes, result.Rows.Cast<DataRow>().Select(x => x[0].ToString().TrimEnd(' ')).Distinct().ToArray());
		}

		public static void InsertTransactions(TestDbHelper helper, Guid branchPK, Guid departmentPK)
		{
			helper.InsertTransactionHeader("AR", "TRF", "001", 100, helper.ToDate("2015-11-09"), branchPK, departmentPK, null, null, 1);
			helper.InsertTransactionHeader("AR", "TRF", "001", -100, helper.ToDate("2015-11-09"), branchPK, departmentPK, null, null, 2);
			helper.InsertTransactionHeader("AP", "TRF", "002", 200, helper.ToDate("2015-11-09"), branchPK, departmentPK, null, null, 1);
			helper.InsertTransactionHeader("AP", "TRF", "002", -200, helper.ToDate("2015-11-09"), branchPK, departmentPK, null, null, 2);
		}
	}
}

