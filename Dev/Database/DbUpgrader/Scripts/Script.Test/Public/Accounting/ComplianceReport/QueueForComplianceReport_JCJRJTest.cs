using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.ComplianceReport;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.ComplianceReport
{
	[TestedType(typeof(QueueForComplianceReport_JCJRJ))]
	class QueueForComplianceReport_JCJRJTest : DbCreateScriptTest
	{
		public void TestSampleCall()
		{
			var helper = new TestDbHelper(TestConnection);

			var branchPK = helper.InsertBranch("ZZB", TestDbHelper.DefaultCompanyPK);
			var departmentPK = helper.InsertDepartment("ZZD");
			var linePKs = InsertTransactions(helper, branchPK, departmentPK);

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("EXEC QueueForComplianceReport_JCJRJ 'LIB', '{0}', NULL, 'Nov 9 2015', 'Nov 10 2015'", TestDbHelper.DefaultCompanyPK));
			AssertEquals("Result should not have rows", 0, result.Rows.Count);

			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.AccTransactionComplianceReportQueue");
			AssertEquals("Result should have rows", 8, result.Rows.Count);

			Func<DataTable, string[]> getReportSubCodes = table => table.Rows.Cast<DataRow>().Select(x => x[0].ToString().TrimEnd(' ')).ToArray();

			var expectedCodes = new string[] {
				"*JC*JRJ*ARSusp*-",
				"*JC*JRJ*JRJCtrl*",
				"*JC*JRJ**Rev-",
				"*JC*JRJ*ARSusp*Rev" };

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT ACQ_ReportSubCode FROM dbo.AccTransactionComplianceReportQueue  WHERE ACQ_Date = 'Nov 9 2015' AND ACQ_ReportType = 'LIB' AND ACQ_ParentTableCode = 'AL' AND  ACQ_GC_Company = '{0}' AND ACQ_GB_Branch = '{1}' AND ACQ_ParentID = '{2}'",
				TestDbHelper.DefaultCompanyPK, branchPK, linePKs[0]));
			AssertEquals("Result should have rows for line 1", 4, result.Rows.Count);
			AssertContainsExactElementsInAnyOrder("ReportSubCodes for line 1", expectedCodes, getReportSubCodes(result));

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT ACQ_ReportSubCode FROM dbo.AccTransactionComplianceReportQueue  WHERE ACQ_Date = 'Nov 9 2015' AND ACQ_ReportType = 'LIB' AND ACQ_ParentTableCode = 'AL' AND  ACQ_GC_Company = '{0}' AND ACQ_GB_Branch = '{1}' AND ACQ_ParentID = '{2}'",
				TestDbHelper.DefaultCompanyPK, branchPK, linePKs[1]));
			AssertEquals("Result should have rows for line 2", 4, result.Rows.Count);
			AssertContainsExactElementsInAnyOrder("ReportSubCodes for line 2", expectedCodes, getReportSubCodes(result));
		}

		public static Guid[] InsertTransactions(TestDbHelper helper, Guid branchPK, Guid departmentPK)
		{
			var glAccountPK = helper.InsertGLAccount("1234.56.05", "TestGLAccount 5");
			var chargeCodePK = helper.InsertChargeCode(TestDbHelper.DefaultCompanyPK, "CC3");
			var creditorOrgPK = helper.InsertOrgHeader("ZC3", "Creditor 3");
			var debtorOrgPK = helper.InsertOrgHeader("ZD3", "Debtor 3");
			var shipmentPK = helper.InsertShipment("S00000003", helper.ToDate("2015-11-09"));
			var shipmentJobPK = helper.InsertJob("S00000003", TestDbHelper.DefaultCompanyPK, branchPK, departmentPK, "JS", shipmentPK, "WRK", helper.ToDate("2015-11-09"));

			var journalPK = helper.InsertTransactionHeader("JC", "JRJ", "001", 0, helper.ToDate("2015-11-09"), branchPK, departmentPK);
			var line1PK = helper.InsertTransactionLine(journalPK, shipmentJobPK, chargeCodePK, glAccountPK, branchPK, departmentPK, debtorOrgPK, -100, "REV", helper.ToDate("2015-11-09"), helper.ToDate("2015-11-09"));
			var line2PK = helper.InsertTransactionLine(journalPK, shipmentJobPK, chargeCodePK, glAccountPK, branchPK, departmentPK, creditorOrgPK, 100, "REV", helper.ToDate("2015-11-09"), helper.ToDate("2015-11-09"));
			return new Guid[] { line1PK, line2PK };
		}
	}
}

