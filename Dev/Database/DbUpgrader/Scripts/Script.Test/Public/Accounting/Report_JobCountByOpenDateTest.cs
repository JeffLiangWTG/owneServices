using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(Report_JobCountByOpenDate))]
	class Report_JobCountByOpenDateTest : DbCreateScriptTest
	{
		public void TestResultsDoesNotIncludeOneOffQuoteJobs()
		{
			var date = new DateTime(2019, 08, 15);
			var helper = new TestDbHelper(TestConnection);
			var companyPK = TestDbHelper.DefaultCompanyPK;
			var branchPK = helper.InsertBranch("ZZB", companyPK);
			var departmentPK = helper.InsertDepartment("ZZD");
			var shipmentPK = helper.InsertShipment("S99999999", date);
			InsertJob(helper, "S99999999", TestDbHelper.DefaultCompanyPK, branchPK, departmentPK, "JS", shipmentPK, "WRK", date);
			AssertJobCount(companyPK, date, 1);
			InsertJob(helper, "00001571", TestDbHelper.DefaultCompanyPK, branchPK, departmentPK, "TH", Guid.NewGuid(), "WRK", date);
			AssertJobCount(companyPK, date, 1);
		}

		public void TestJobInactive()
		{
			var date = new DateTime(2019, 08, 15);
			var helper = new TestDbHelper(TestConnection);
			var companyPK = TestDbHelper.DefaultCompanyPK;
			var branchPK = helper.InsertBranch("ZZB", companyPK);
			var departmentPK = helper.InsertDepartment("ZZD");
			var shipmentPK = helper.InsertShipment("S99999999", date);
			var shipment2PK = helper.InsertShipment("00001571", date);
			InsertJob(helper, "S99999999", TestDbHelper.DefaultCompanyPK, branchPK, departmentPK, "JS", shipmentPK, "WRK", date);
			InsertJob(helper, "00001571", TestDbHelper.DefaultCompanyPK, branchPK, departmentPK, "JS", shipment2PK, "WRK", date);
			helper.RunSQL(new { JH_JobNum = "00001571" }, @"
UPDATE dbo.JobHeader
SET
	JH_IsActive = 0,
	JH_SystemLastEditTimeUtc = GETUTCDATE(),
	JH_SystemLastEditUser = '~BP'
WHERE
	JH_JobNum = @JH_JobNum");

			var result = GetResults(companyPK, date);

			AssertEquals("Should have data with inactive job", 1, result.Rows.Count);
			AssertEquals("Should have data with inactive job", 2, result.Rows[0]["TotalJobs"]);
		}

		public void TestResultsHasCorrectData_PivotBranch()
		{
			var date = new DateTime(2019, 08, 15);
			var companyPK = TestDbHelper.DefaultCompanyPK;
			setupTestData(companyPK, date);

			AssertJobData(companyPK, date, 3, "Branch", new (string, int)[] { ("ZZB", 1), ("ZZC", 2) });
		}

		public void TestResultsHasCorrectData_PivotDepartment()
		{
			var date = new DateTime(2019, 08, 15);
			var companyPK = TestDbHelper.DefaultCompanyPK;
			setupTestData(companyPK, date);

			AssertJobData(companyPK, date, 3, "Department", new (string, int)[] { ("ZZD", 2), ("ZZE", 1) });
		}

		void setupTestData(Guid companyPK, DateTime date)
		{
			var helper = new TestDbHelper(TestConnection);
			var branch1PK = helper.InsertBranch("ZZB", companyPK);
			var branch2PK = helper.InsertBranch("ZZC", companyPK);
			var department1PK = helper.InsertDepartment("ZZD");
			var department2PK = helper.InsertDepartment("ZZE");
			var shipmentPK = helper.InsertShipment("S99999999", date);
			var shipment2PK = helper.InsertShipment("00001571", date);
			var shipment3PK = helper.InsertShipment("00001572", date);
			InsertJob(helper, "S99999999", companyPK, branch1PK, department1PK, "JS", shipmentPK, "WRK", date);
			InsertJob(helper, "00001571", companyPK, branch2PK, department1PK, "JS", shipment2PK, "WRK", date);
			InsertJob(helper, "00001572", companyPK, branch2PK, department2PK, "JS", shipment3PK, "WRK", date);
		}

		void InsertJob(TestDbHelper helper, string jobNumber, Guid companyPK, Guid branchPK, Guid departmentPK,
			string parentTableCode, Guid parentId, string status, DateTime createdTime)
		{
			helper.Insert("JobHeader", new
			{
				JH_PK = Guid.NewGuid(),
				JH_JobNum = jobNumber,
				JH_GB = branchPK,
				JH_GC = companyPK,
				JH_GE = departmentPK,
				JH_ParentID = parentId,
				JH_Status = status,
				JH_ParentTableCode = parentTableCode,
				JH_SystemCreateTimeUtc = createdTime,
				JH_A_JOP = createdTime
			});
		}
		DataTable GetResults(Guid companyPK, DateTime date, string pivotBy = "Branch")
		{
			var sql = $"EXEC Report_JobCountByOpenDate '{pivotBy}', '', '{companyPK}', '{date.ToString("yyyyMMdd")}', '{date.AddDays(1).ToString("yyyyMMdd")}', '', ''";
			return DataUtils.GetDataTableFromQuery(TestConnection, sql);
		}

		void AssertJobCount(Guid companyPK, DateTime date, int jobsCount)
		{
			var result = GetResults(companyPK, date);

			AssertEquals(1, result.Rows.Count);
			AssertEquals(jobsCount, result.Rows[0]["TotalJobs"]);
		}

		void AssertJobData(Guid companyPK, DateTime date, int jobsCount, string pivotBy, (string pivotCode, int jobs)[] pivotColumns)
		{
			var result = GetResults(companyPK, date, pivotBy);

			AssertEquals(1, result.Rows.Count);
			AssertEquals(3 + (2 * (pivotColumns.Length)), result.Columns.Count);

			var row = result.Rows[0];
			AssertEquals(date, row["JobDate"]);
			AssertEquals(jobsCount, row["TotalJobs"]);
			AssertEquals(pivotColumns.Length, row["ColumnCount"]);

			var i = 1;
			foreach (var (pivotCode, jobs) in pivotColumns)
			{
				AssertEquals(pivotCode, row[$"Col{i}Name"]);
				AssertEquals(jobs, row[$"Col{i}"]);
				i++;
			}
		}
	}
}

