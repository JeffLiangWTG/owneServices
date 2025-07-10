using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.ComplianceReport;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.ComplianceReport
{
	[TestedType(typeof(QueueForComplianceReport_JCJNL))]
	class QueueForComplianceReport_JCJNLTest : DbCreateScriptTest
	{
		public void TestSampleCall()
		{
			var helper = new TestDbHelper(TestConnection);

			var branchPK = helper.InsertBranch("ZZB", TestDbHelper.DefaultCompanyPK);
			var departmentPK = helper.InsertDepartment("ZZD");
			InsertTransactions(helper, branchPK, departmentPK);

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("EXEC QueueForComplianceReport_JCJNL 'LIB', '{0}', NULL, 'Nov 9 2015', 'Nov 10 2015'", TestDbHelper.DefaultCompanyPK));
			AssertEquals("Result should not have rows", 0, result.Rows.Count);

			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.AccTransactionComplianceReportQueue");
			AssertEquals("Result should have rows", 4, result.Rows.Count);

			var expectedCodes = new string[] {
				"*JC*JNL*ARSusp*-",
				"*JC*JNL*CFX*",
				"*JC*JNL**Rev-",
				"*JC*JNL*ARSusp*Rev" };

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT ACQ_ReportSubCode FROM dbo.AccTransactionComplianceReportQueue  WHERE ACQ_Date = 'Nov 9 2015' AND ACQ_ReportType = 'LIB' AND ACQ_ParentTableCode = 'AL' AND  ACQ_GC_Company = '{0}' AND ACQ_GB_Branch = '{1}'",
				TestDbHelper.DefaultCompanyPK, branchPK));
			AssertEquals("Result should have rows", 4, result.Rows.Count);
			AssertContainsExactElementsInAnyOrder("ReportSubCodes", expectedCodes, result.Rows.Cast<DataRow>().Select(x => x[0].ToString().TrimEnd(' ')).ToArray());
		}

		public static void InsertTransactions(TestDbHelper helper, Guid branchPK, Guid departmentPK)
		{
			var glAccountPK = helper.InsertGLAccount("1234.56.04", "TestGLAccount 4");
			var chargeCodePK = helper.InsertChargeCode(TestDbHelper.DefaultCompanyPK, "CC2");
			var creditorOrgPK = helper.InsertOrgHeader("ZC2", "Creditor 2");
			var shipmentPK = helper.InsertShipment("S00000002", helper.ToDate("2015-11-09"));
			var shipmentJobPK = helper.InsertJob("S00000002", TestDbHelper.DefaultCompanyPK, branchPK, departmentPK, "JS", shipmentPK, "WRK", helper.ToDate("2015-11-09"));

			var journalPK = helper.InsertTransactionHeader("JC", "JNL", "001", 0, helper.ToDate("2015-11-09"), branchPK, departmentPK);
			helper.InsertTransactionLine(journalPK, shipmentJobPK, chargeCodePK, glAccountPK, branchPK, departmentPK, creditorOrgPK, 100, "REV", helper.ToDate("2015-11-09"), helper.ToDate("2015-11-09"));
		}
	}
}

