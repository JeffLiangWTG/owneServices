using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.ComplianceReport;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.ComplianceReport
{
	[TestedType(typeof(QueueForComplianceReport_DRCDPY))]
	class QueueForComplianceReport_DRCDPYTest : DbCreateScriptTest
	{
		public void TestSampleCall()
		{
			var helper = new TestDbHelper(TestConnection);

			var branchPK = helper.InsertBranch("ZZB", TestDbHelper.DefaultCompanyPK);
			var departmentPK = helper.InsertDepartment("ZZD");
			InsertTransactions(helper, branchPK, departmentPK);

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("EXEC QueueForComplianceReport_DRCDPY 'LIB', '{0}', NULL, 'Nov 9 2015', 'Nov 10 2015'", TestDbHelper.DefaultCompanyPK));
			AssertEquals("Result should not have rows", 0, result.Rows.Count);

			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.AccTransactionComplianceReportQueue");
			AssertEquals("Result should have rows", 7, result.Rows.Count);

			var expectedCodes = new string[] {
				"*CB*DPY**-",
				"*CB*DRC**-",
				"*CB*DPY*GSTIn*-",
				"*CB*DRC*GSTOut*-",
				"*CB*DPY**GSTNotRec-" };

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT ACQ_ReportSubCode FROM dbo.AccTransactionComplianceReportQueue  WHERE ACQ_Date = 'Nov 9 2015' AND ACQ_ReportType = 'LIB' AND ACQ_ParentTableCode = 'AL' AND  ACQ_GC_Company = '{0}' AND ACQ_GB_Branch = '{1}'",
				TestDbHelper.DefaultCompanyPK, branchPK));
			AssertEquals("Result should have AL based rows", 5, result.Rows.Count);
			AssertContainsExactElementsInAnyOrder("ReportSubCodes", expectedCodes, result.Rows.Cast<DataRow>().Select(x => x[0].ToString().TrimEnd(' ')).ToArray());

			expectedCodes = new string[] {
				"*CB*DPY*Bank*Total",
				"*CB*DRC*Bank*Total" };

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT ACQ_ReportSubCode FROM dbo.AccTransactionComplianceReportQueue  WHERE ACQ_Date = 'Nov 9 2015' AND ACQ_ReportType = 'LIB' AND ACQ_ParentTableCode = 'AH' AND  ACQ_GC_Company = '{0}' AND ACQ_GB_Branch = '{1}'",
				TestDbHelper.DefaultCompanyPK, branchPK));
			AssertEquals("Result should have AH based rows", 2, result.Rows.Count);
			AssertContainsExactElementsInAnyOrder("ReportSubCodes", expectedCodes, result.Rows.Cast<DataRow>().Select(x => x[0].ToString().TrimEnd(' ')).ToArray());
		}

		public static void InsertTransactions(TestDbHelper helper, Guid branchPK, Guid departmentPK)
		{
			var glAccountPK = helper.InsertGLAccount("1234.56.02", "TestGLAccount 2");
			var bankAccountPK = helper.InsertBankAccount("ZBA", glAccountPK);

			var drcPK = helper.InsertTransactionHeader("CB", "DRC", "001", 110, helper.ToDate("2015-11-09"), branchPK, departmentPK, bankAccountPK);
			// AccTransactionLines is not valid with empty AL_LineType.
			helper.InsertTransactionLine(drcPK, null, null, glAccountPK, branchPK, departmentPK, null, 100, "WIP", helper.ToDate("2015-11-09"), null, 10);
			var dpyPK = helper.InsertTransactionHeader("CB", "DPY", "002", 220, helper.ToDate("2015-11-09"), branchPK, departmentPK, bankAccountPK);
			// AccTransactionLines is not valid with empty AL_LineType.
			helper.InsertTransactionLine(dpyPK, null, null, glAccountPK, branchPK, departmentPK, null, 200, "WIP", helper.ToDate("2015-11-09"), null, 20, 0.5m);
		}
	}
}

