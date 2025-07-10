using System;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting
{
	[TestedType(typeof(Report_RevenueAndCostSuspenseControlOutstandingTransactions))]
	class Report_RevenueAndCostSuspenseControlOutstandingTransactionsTest : DbCreateScriptTest
	{
		public void TestJobInactive()
		{
			var today = DateTime.Today;
			var helper = new TestDbHelper(TestConnection);
			var companyPK = TestDbHelper.DefaultCompanyPK;
			var branchPK = helper.DefaultBranchPK;
			var departmentPK = helper.DefaultDepartmentPK;
			var orgPK = helper.InsertOrgHeader("TSTORG1", "Test Org 1");
			var chargeCodePK = helper.InsertChargeCode(companyPK, "FRT1");
			var glHeaderPK = helper.GLAccountPK1;

			var shipment1Number = "S001001";
			var shipment1PK = helper.InsertShipment(shipment1Number, today);

			var shipment2Number = "S001002";
			var shipment2PK = helper.InsertShipment(shipment2Number, today);

			var jobStatus = "WRK";
			var job1PK = helper.InsertJob(shipment1Number, companyPK, branchPK, departmentPK, "JS", shipment1PK, jobStatus, today);
			helper.InsertJobChargeRevRecognition("IMM", today, job1PK);
			var job2PK = helper.InsertJob(shipment2Number, companyPK, branchPK, departmentPK, "JS", shipment2PK, jobStatus, today);
			helper.InsertJobChargeRevRecognition("IMM", today, job2PK);

			//lines for job1
			var header2PK = helper.InsertTransactionHeader("AR", "INV", "1002", 200M, today, branchPK, departmentPK, job: job1PK, companyPK: companyPK, org: orgPK, exchangeRate: 1M);
			helper.InsertTransactionLine(header2PK, job1PK, chargeCodePK, glHeaderPK, branchPK, departmentPK, orgPK, 200M, "REV", today, reverseDate: null, companyPK: companyPK); //REV without reversed date

			var header4PK = helper.InsertTransactionHeader("AP", "INV", "1004", 400M, today, branchPK, departmentPK, job: job1PK, companyPK: companyPK, org: orgPK, exchangeRate: 1M);
			helper.InsertTransactionLine(header4PK, job1PK, chargeCodePK, glHeaderPK, branchPK, departmentPK, orgPK, 400M, "CST", today, reverseDate: null, companyPK: companyPK); //CST without reversed date

			//lines for job2
			var header10PK = helper.InsertTransactionHeader("AR", "INV", "1010", 200M, today, branchPK, departmentPK, job: job1PK, companyPK: companyPK, org: orgPK, exchangeRate: 1M);
			helper.InsertTransactionLine(header10PK, job2PK, chargeCodePK, glHeaderPK, branchPK, departmentPK, orgPK, 200M, "REV", today, reverseDate: null, companyPK: companyPK); //REV without reversed date

			var header12PK = helper.InsertTransactionHeader("AP", "INV", "1012", 400M, today, branchPK, departmentPK, job: job1PK, companyPK: companyPK, org: orgPK, exchangeRate: 1M);
			helper.InsertTransactionLine(header12PK, job2PK, chargeCodePK, glHeaderPK, branchPK, departmentPK, orgPK, 400M, "CST", today, reverseDate: null, companyPK: companyPK); //CST without reversed date

			helper.RunSQL(new { JH_PK = job2PK }, @"
UPDATE dbo.JobHeader
SET
	JH_IsActive = 0,
	JH_SystemLastEditTimeUtc = GETUTCDATE(),
	JH_SystemLastEditUser = '~BP'
WHERE
	JH_PK = @JH_PK");

			var totalRows = 0;
			TestConnection.ExecuteReader($"SELECT * FROM Report_RevenueAndCostSuspenseControlOutstandingTransactions('{companyPK}', '{today.AddDays(1).ToString("yyyyMMdd")}', 'ALL', '', '', '', '')",
				_ => totalRows++);
			AssertEquals("Should have data with inactive job", 4, totalRows);
		}
	}
}

