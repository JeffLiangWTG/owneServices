using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.ComplianceReport;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.ComplianceReport
{
	[TestedType(typeof(QueueForComplianceReportLIQ))]
	class QueueForComplianceReportLIQTest : DbCreateScriptTest
	{
		public void TestSampleCall()
		{
			var helper = new TestDbHelper(TestConnection);

			var glAccountPK = helper.InsertGLAccount("1234.56.03", "TestGLAccount");
			var glCompanyPK = TestDataCreator.CreateCompany("TC1", "IT", "EUR");
			var chargeCodePK = helper.InsertChargeCode(TestDbHelper.DefaultCompanyPK, "CC1");
			var creditorOrgPK = helper.InsertOrgHeader("ZC1", "Creditor 1");
			var branchPK = helper.InsertBranch("ZZB", TestDbHelper.DefaultCompanyPK);
			var departmentPK = helper.InsertDepartment("ZZD");

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.AccTransactionLines");
			AssertEquals("Result should have rows", 0, result.Rows.Count);

			InsertTransactions(helper, branchPK, departmentPK, glAccountPK, glCompanyPK, chargeCodePK, creditorOrgPK, "S00000001", "2021-11-15", "AR", 1);
			InsertTransactions(helper, branchPK, departmentPK, glAccountPK, glCompanyPK, chargeCodePK, creditorOrgPK, "S00000002", "2021-11-15", "AP", 4);
			InsertTransactions(helper, branchPK, departmentPK, glAccountPK, glCompanyPK, chargeCodePK, creditorOrgPK, "S00000003", "2022-01-15", "AR", 7);
			InsertTransactions(helper, branchPK, departmentPK, glAccountPK, glCompanyPK, chargeCodePK, creditorOrgPK, "S00000004", "2022-01-15", "AP", 10);

			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.AccTransactionLines");
			AssertEquals("Result should have rows", 20, result.Rows.Count);

			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.AccTransactionComplianceReportQueue");
			AssertEquals("Result should have rows", 0, result.Rows.Count);

			result = DataUtils.GetDataTableFromQuery(TestConnection, "EXEC QueueForComplianceReportLIQ ");
			AssertEquals("Result should not have rows", 0, result.Rows.Count);

			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.AccTransactionComplianceReportQueue");
			AssertEquals("Result should have rows", 10, result.Rows.Count);

			InsertTransactions(helper, branchPK, departmentPK, glAccountPK, glCompanyPK, chargeCodePK, creditorOrgPK, "S00000005", "2022-01-25", "AR", 13);

			var glAccountPKnotIT = helper.InsertGLAccount("1234.56.13", "TestGLAccount 2");
			var glCompanyPKnotIT = TestDataCreator.CreateCompany("TC2", "US", "USD");
			var chargeCodePKnotIT = helper.InsertChargeCode(TestDbHelper.OtherCompanyPK, "CC2");
			var creditorOrgPKnotIT = helper.InsertOrgHeader("ZC2", "Creditor 2");
			var branchPKnotIT = helper.InsertBranch("ZSB", TestDbHelper.OtherCompanyPK);
			var departmentPKnotIT = helper.InsertDepartment("ZSD");

			InsertTransactions(helper, branchPKnotIT, departmentPKnotIT, glAccountPKnotIT, glCompanyPKnotIT, chargeCodePKnotIT, creditorOrgPKnotIT, "S00000006", "2022-01-20", "AR", 16);

			result = DataUtils.GetDataTableFromQuery(TestConnection, "EXEC QueueForComplianceReportLIQ ");
			AssertEquals("Result should not have rows", 0, result.Rows.Count);

			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.AccTransactionComplianceReportQueue");
			AssertEquals("Result should have rows", 15, result.Rows.Count);
		}

		public static void InsertTransactions(TestDbHelper helper, Guid branchPK, Guid departmentPK, Guid glAccountPK, Guid glCompanyPK, Guid chargeCodePK, Guid creditorOrgPK, string jobNumber, string date, string ledger, int firstTransactionNumber)
		{
			var postDate = helper.ToDate(date);
			var shipmentPK = helper.InsertShipment(jobNumber, postDate);
			var shipmentJobPK = helper.InsertJob(jobNumber, TestDbHelper.DefaultCompanyPK, branchPK, departmentPK, "JS", shipmentPK, "WRK", postDate);

			var apInvPK = helper.InsertTransactionHeader(ledger, "INV", firstTransactionNumber.ToString(), 110, postDate, branchPK, departmentPK, glAccountPK: glAccountPK, companyPK: glCompanyPK);
			helper.InsertTransactionLine(apInvPK, shipmentJobPK, chargeCodePK, glAccountPK, branchPK, departmentPK, creditorOrgPK, 50, "CST", postDate, null, 5, 0.5m, companyPK: glCompanyPK);
			helper.InsertTransactionLine(apInvPK, shipmentJobPK, chargeCodePK, glAccountPK, branchPK, departmentPK, creditorOrgPK, 60, "CST", postDate, null, 6, 0.5m, companyPK: glCompanyPK);

			var apCrdPK = helper.InsertTransactionHeader(ledger, "CRD", (firstTransactionNumber + 1).ToString(), 220, postDate, branchPK, departmentPK, glAccountPK: glAccountPK, companyPK: glCompanyPK);
			helper.InsertTransactionLine(apCrdPK, shipmentJobPK, chargeCodePK, glAccountPK, branchPK, departmentPK, creditorOrgPK, 100, "CST", postDate, null, 10, 0.5m, companyPK: glCompanyPK);
			helper.InsertTransactionLine(apCrdPK, shipmentJobPK, chargeCodePK, glAccountPK, branchPK, departmentPK, creditorOrgPK, 120, "CST", postDate, null, 12, 0.5m, companyPK: glCompanyPK);

			var apAdjPK = helper.InsertTransactionHeader(ledger, "ADJ", (firstTransactionNumber + 2).ToString(), 330, postDate, branchPK, departmentPK, glAccountPK: glAccountPK, companyPK: glCompanyPK);
			helper.InsertTransactionLine(apAdjPK, shipmentJobPK, chargeCodePK, glAccountPK, branchPK, departmentPK, creditorOrgPK, 150, "CST", postDate, null, 15, 0.5m, companyPK: glCompanyPK);
		}
	}
}
