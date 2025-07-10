using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(Report_JobwithExcessProfitSummary))]
	class Report_JobwithExcessProfitSummaryTest : DbCreateScriptTest
	{
		public void TestJobInactive()
		{
			var today = DateTime.Today;
			var helper = new TestDbHelper(TestConnection);
			var companyPK = TestDbHelper.DefaultCompanyPK;
			var branchPK = helper.DefaultBranchPK;
			var departmentPK = helper.DefaultDepartmentPK;
			var orgPK = helper.InsertOrgHeader("TSTORG1", "Test Org 1");
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

			helper.InsertTransactionLine(null, job1PK, null, glHeaderPK, branchPK, departmentPK, orgPK, 100M, "WIP", today, reverseDate: null, companyPK: companyPK);
			helper.InsertTransactionLine(null, job2PK, null, glHeaderPK, branchPK, departmentPK, orgPK, 100M, "WIP", today, reverseDate: null, companyPK: companyPK);

			helper.RunSQL(new { JH_PK = job2PK }, @"
UPDATE dbo.JobHeader
SET
	JH_IsActive = 0,
	JH_SystemLastEditTimeUtc = GETUTCDATE(),
	JH_SystemLastEditUser = '~BP'
WHERE
	JH_PK = @JH_PK");

			var result = GetResults(companyPK, today);
			AssertEquals("Should have data with inactive job", 2, result.Rows.Count);
		}

		DataTable GetResults(Guid companyPK, DateTime date, string pivotBy = "Branch")
		{
			var sql = $"SELECT * FROM Report_JobwithExcessProfitSummary('{companyPK}', '{date.ToString("yyyyMMdd")}', '{date.AddDays(1).ToString("yyyyMMdd")}', '', '', '', null, null, '', '', -200, '{date.AddDays(-1).ToString("yyyyMMdd")}', '{date.AddDays(1).ToString("yyyyMMdd")}')";
			return DataUtils.GetDataTableFromQuery(TestConnection, sql);
		}
	}
}
