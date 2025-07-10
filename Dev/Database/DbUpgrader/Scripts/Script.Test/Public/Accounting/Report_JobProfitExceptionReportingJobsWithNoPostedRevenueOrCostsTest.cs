using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(Report_JobProfitExceptionReportingJobsWithNoPostedRevenueOrCosts))]
	class Report_JobProfitExceptionReportingJobsWithNoPostedRevenueOrCostsTest : DbCreateScriptTest
	{
		[TestDate(2020, 10, 01)]
		public void TestPerformance_Report_JobProfitExceptionReportingJobsWithNoPostedRevenueOrCosts()
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

			var shipment3Number = "S001003";
			var shipment3PK = helper.InsertShipment(shipment3Number, today);

			var shipment4Number = "S001004";
			var shipment4PK = helper.InsertShipment(shipment4Number, today);

			var jobStatus = "WRK";
			var job1PK = helper.InsertJob(shipment1Number, companyPK, branchPK, departmentPK, "JS", shipment1PK, jobStatus, today);
			helper.InsertJobChargeRevRecognition("IMM", today, job1PK);
			var job2PK = helper.InsertJob(shipment2Number, companyPK, branchPK, departmentPK, "JS", shipment2PK, jobStatus, today);
			helper.InsertJobChargeRevRecognition("IMM", today, job2PK);
			var job3PK = helper.InsertJob(shipment3Number, companyPK, branchPK, departmentPK, "JS", shipment3PK, jobStatus, today);
			helper.InsertJobChargeRevRecognition("IMM", today, job3PK);
			var job4PK = helper.InsertJob(shipment4Number, companyPK, branchPK, departmentPK, "JS", shipment4PK, jobStatus, today);
			helper.InsertJobChargeRevRecognition("IMM", today, job4PK);

			//lines for job1
			var header1PK = helper.InsertTransactionHeader("AR", "INV", "1001", 100M, today, branchPK, departmentPK, job: job1PK, companyPK: companyPK, org: orgPK, exchangeRate: 1M);
			helper.InsertTransactionLine(header1PK, job1PK, chargeCodePK, glHeaderPK, branchPK, departmentPK, orgPK, 100M, "REV", today, reverseDate: today, companyPK: companyPK); //REV with reversed date

			var header2PK = helper.InsertTransactionHeader("AR", "INV", "1002", 200M, today, branchPK, departmentPK, job: job1PK, companyPK: companyPK, org: orgPK, exchangeRate: 1M);
			helper.InsertTransactionLine(header2PK, job1PK, chargeCodePK, glHeaderPK, branchPK, departmentPK, orgPK, 200M, "REV", today, reverseDate: null, companyPK: companyPK); //REV without reversed date

			var header3PK = helper.InsertTransactionHeader("AP", "INV", "1003", 300M, today, branchPK, departmentPK, job: job1PK, companyPK: companyPK, org: orgPK, exchangeRate: 1M);
			helper.InsertTransactionLine(header3PK, job1PK, chargeCodePK, glHeaderPK, branchPK, departmentPK, orgPK, 300M, "CST", today, reverseDate: today, companyPK: companyPK); //CST with reversed date

			var header4PK = helper.InsertTransactionHeader("AP", "INV", "1004", 400M, today, branchPK, departmentPK, job: job1PK, companyPK: companyPK, org: orgPK, exchangeRate: 1M);
			helper.InsertTransactionLine(header4PK, job1PK, chargeCodePK, glHeaderPK, branchPK, departmentPK, orgPK, 400M, "CST", today, reverseDate: null, companyPK: companyPK); //CST without reversed date

			for (int i = 0; i < 500; i++)
			{
				helper.InsertTransactionLine(null, job1PK, null, glHeaderPK, branchPK, departmentPK, orgPK, 500M, "WIP", today, reverseDate: today, companyPK: companyPK); //WIP with reversed date
				helper.InsertTransactionLine(null, job1PK, null, glHeaderPK, branchPK, departmentPK, orgPK, 700M, "ACR", today, reverseDate: today, companyPK: companyPK); //ACR with reversed date
			}

			for (int i = 0; i < 500; i++)
			{
				helper.InsertTransactionLine(null, job1PK, null, glHeaderPK, branchPK, departmentPK, orgPK, 600M, "WIP", today, reverseDate: null, companyPK: companyPK); //WIP without reversed date
				helper.InsertTransactionLine(null, job1PK, null, glHeaderPK, branchPK, departmentPK, orgPK, 800M, "ACR", today, reverseDate: null, companyPK: companyPK); //ACR without reversed date
			}

			//lines for job2
			var header9PK = helper.InsertTransactionHeader("AR", "INV", "1009", 100M, today, branchPK, departmentPK, job: job1PK, companyPK: companyPK, org: orgPK, exchangeRate: 1M);
			helper.InsertTransactionLine(header9PK, job2PK, chargeCodePK, glHeaderPK, branchPK, departmentPK, orgPK, 100M, "REV", today, reverseDate: today, companyPK: companyPK); //REV with reversed date

			var header10PK = helper.InsertTransactionHeader("AR", "INV", "1010", 200M, today, branchPK, departmentPK, job: job1PK, companyPK: companyPK, org: orgPK, exchangeRate: 1M);
			helper.InsertTransactionLine(header10PK, job2PK, chargeCodePK, glHeaderPK, branchPK, departmentPK, orgPK, 200M, "REV", today, reverseDate: null, companyPK: companyPK); //REV without reversed date

			var header11PK = helper.InsertTransactionHeader("AP", "INV", "1011", 300M, today, branchPK, departmentPK, job: job1PK, companyPK: companyPK, org: orgPK, exchangeRate: 1M);
			helper.InsertTransactionLine(header11PK, job2PK, chargeCodePK, glHeaderPK, branchPK, departmentPK, orgPK, 300M, "CST", today, reverseDate: today, companyPK: companyPK); //CST with reversed date

			var header12PK = helper.InsertTransactionHeader("AP", "INV", "1012", 400M, today, branchPK, departmentPK, job: job1PK, companyPK: companyPK, org: orgPK, exchangeRate: 1M);
			helper.InsertTransactionLine(header12PK, job2PK, chargeCodePK, glHeaderPK, branchPK, departmentPK, orgPK, 400M, "CST", today, reverseDate: null, companyPK: companyPK); //CST without reversed date

			//lines for job3
			for (int i = 0; i < 500; i++)
			{
				helper.InsertTransactionLine(null, job3PK, null, glHeaderPK, branchPK, departmentPK, orgPK, 500M, "WIP", today, reverseDate: today, companyPK: companyPK); //WIP with reversed date
				helper.InsertTransactionLine(null, job3PK, null, glHeaderPK, branchPK, departmentPK, orgPK, 700M, "ACR", today, reverseDate: today, companyPK: companyPK); //ACR with reversed date
			}

			for (int i = 0; i < 500; i++)
			{
				helper.InsertTransactionLine(null, job3PK, null, glHeaderPK, branchPK, departmentPK, orgPK, 600M, "WIP", today, reverseDate: null, companyPK: companyPK); //WIP without reversed date
				helper.InsertTransactionLine(null, job3PK, null, glHeaderPK, branchPK, departmentPK, orgPK, 800M, "ACR", today, reverseDate: null, companyPK: companyPK); //ACR without reversed date
			}

			//lines for job4
			for (int i = 0; i < 500; i++)
			{
				helper.InsertTransactionLine(null, job4PK, null, glHeaderPK, branchPK, departmentPK, orgPK, 500M, "WIP", today, reverseDate: today, companyPK: companyPK); //WIP with reversed date
				helper.InsertTransactionLine(null, job4PK, null, glHeaderPK, branchPK, departmentPK, orgPK, 700M, "ACR", today, reverseDate: today, companyPK: companyPK); //ACR with reversed date
			}

			for (int i = 0; i < 500; i++)
			{
				helper.InsertTransactionLine(null, job4PK, null, glHeaderPK, branchPK, departmentPK, orgPK, 600M, "WIP", today, reverseDate: null, companyPK: companyPK); //WIP without reversed date
				helper.InsertTransactionLine(null, job4PK, null, glHeaderPK, branchPK, departmentPK, orgPK, 800M, "ACR", today, reverseDate: null, companyPK: companyPK); //ACR without reversed date
			}

			TestConnection.ExecuteNonQuery($"UPDATE STATISTICS AccTransactionLines WITH FULLSCAN");
			// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
			using (TestConnection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				var totalRows = 0;
				TestConnection.ExecuteReader($"select * from Report_JobProfitExceptionReportingJobsWithNoPostedRevenueOrCosts('{companyPK}', 'S', 'WRK', 'REV', '{today.AddDays(-7):yyyy-MM-dd}', '{today.AddDays(7):yyyy-MM-dd}', '') order by JH_JobNum",
					_ => totalRows++);
				AssertEquals(2, totalRows);

				var queryPlan = TestConnection.ExecutedCommandsAndQueryPlans.First(t => t.Item1.Contains("Report_JobProfitExceptionReportingJobsWithNoPostedRevenueOrCosts"));
				var queryPlanAnalyzer = new QueryPlanalyzer(queryPlan.Item2.First());
				Assert("There should be no table scan on AccTransactionLines", !queryPlanAnalyzer.TableScans.Any(x => x.TableName.Equals("AccTransactionLines")));
			}
		}

		[TestDate(2020, 10, 01)]
		public void TestActiveStatusFiltering()
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

			for (int i = 0; i < 10; i++)
			{
				helper.InsertTransactionLine(null, job1PK, null, glHeaderPK, branchPK, departmentPK, orgPK, 600M, "WIP", today, reverseDate: null, companyPK: companyPK); //WIP without reversed date
				helper.InsertTransactionLine(null, job1PK, null, glHeaderPK, branchPK, departmentPK, orgPK, 800M, "ACR", today, reverseDate: null, companyPK: companyPK); //ACR without reversed date
			}

			for (int i = 0; i < 10; i++)
			{
				helper.InsertTransactionLine(null, job2PK, null, glHeaderPK, branchPK, departmentPK, orgPK, 600M, "WIP", today, reverseDate: null, companyPK: companyPK); //WIP without reversed date
				helper.InsertTransactionLine(null, job2PK, null, glHeaderPK, branchPK, departmentPK, orgPK, 800M, "ACR", today, reverseDate: null, companyPK: companyPK); //ACR without reversed date
			}

			helper.RunSQL(new { JH_PK = job2PK }, @"
UPDATE dbo.JobHeader
SET
	JH_IsActive = 0,
	JH_SystemLastEditTimeUtc = GETUTCDATE(),
	JH_SystemLastEditUser = '~BP'
WHERE
	JH_PK = @JH_PK");
			
			var sql = $"select * from Report_JobProfitExceptionReportingJobsWithNoPostedRevenueOrCosts('{companyPK}', 'S', '', 'REV', '{today.AddDays(-7):yyyy-MM-dd}', '{today.AddDays(7):yyyy-MM-dd}', '') order by JH_JobNum";
			var rows = DataUtils.GetDataTableFromQuery(TestConnection, sql).AsEnumerable();

			AssertEquals("Should have data with all job", 2, rows.Count());

			sql = $"select * from Report_JobProfitExceptionReportingJobsWithNoPostedRevenueOrCosts('{companyPK}', 'S', '', 'REV', '{today.AddDays(-7):yyyy-MM-dd}', '{today.AddDays(7):yyyy-MM-dd}', 'Inactive') order by JH_JobNum";
			rows = DataUtils.GetDataTableFromQuery(TestConnection, sql).AsEnumerable();

			AssertEquals("Should have data only with inactive job", 1, rows.Count());

			var jobNum = (string)rows.FirstOrDefault()["JH_JobNum"];
			var isActive = (bool)rows.FirstOrDefault()["JH_IsActive"];

			AssertEquals(shipment2Number, jobNum);
			Assert(!isActive);

			sql = $"select * from Report_JobProfitExceptionReportingJobsWithNoPostedRevenueOrCosts('{companyPK}', 'S', '', 'REV', '{today.AddDays(-7):yyyy-MM-dd}', '{today.AddDays(7):yyyy-MM-dd}', 'Active') order by JH_JobNum";

			rows = DataUtils.GetDataTableFromQuery(TestConnection, sql).AsEnumerable();

			AssertEquals("Should have data only with active job", 1, rows.Count());

			jobNum = (string)rows.FirstOrDefault()["JH_JobNum"];
			isActive = (bool)rows.FirstOrDefault()["JH_IsActive"];
			AssertEquals(shipment1Number, jobNum);
			Assert(isActive);
		}
	}
}

