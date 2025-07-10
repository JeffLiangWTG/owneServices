using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(csfn_FreighStaffAssignmentsAndOrgAttributions))]
	class csfn_FreighStaffAssignmentsAndOrgAttributionsTest : DbCreateScriptTest
	{
		public void TestJobInactive()
		{
			var date = new DateTime(2019, 08, 15);
			PrepareTestData(date, false);

			var result = GetResults(TestDbHelper.DefaultCompanyPK, date, date);
			AssertEquals("Should have data with inactive job", 2, result.Rows.Count);
		}

		public void TestNullDate()
		{
			var date = new DateTime(2019, 08, 15);
			PrepareTestData(date);

			var fromDateResult = GetResults(TestDbHelper.DefaultCompanyPK, date, DateTime.MaxValue);
			AssertEquals("Should not have data with toDate is null", 0, fromDateResult.Rows.Count);

			var toDateResult = GetResults(TestDbHelper.DefaultCompanyPK, DateTime.MinValue, date);
			AssertEquals("Should have data with fromDate is null", 2, toDateResult.Rows.Count);
		}

		public void PrepareTestData(DateTime date, bool isActive = true)
		{
			var helper = new TestDbHelper(TestConnection);
			var companyPK = TestDbHelper.DefaultCompanyPK;
			var branchPK = helper.InsertBranch("ZZB", companyPK);
			var departmentPK = helper.InsertDepartment("ZZD");
			var shipmentPK = helper.InsertShipment("S99999999", date);
			var shipment2PK = helper.InsertShipment("00002024", date);
			InsertJob(helper, "S99999999", TestDbHelper.DefaultCompanyPK, branchPK, departmentPK, "JS", shipmentPK, "WRK", date);
			InsertJob(helper, "00002024", TestDbHelper.DefaultCompanyPK, branchPK, departmentPK, "TH", shipment2PK, "WRK", date, isActive);
		}

		void InsertJob(TestDbHelper helper, string jobNumber, Guid companyPK, Guid branchPK, Guid departmentPK,
			string parentTableCode, Guid parentId, string status, DateTime createdTime, bool isActive = true)
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
				JH_A_JOP = createdTime,
				JH_IsActive = isActive
			});
		}

		DataTable GetResults(Guid companyPK, DateTime fromDate, DateTime toDate)
		{
			var fromDateStr = fromDate == DateTime.MinValue ? "null" : $"'{fromDate.ToString("yyyyMMdd")}'";
			var toDateStr = toDate == DateTime.MaxValue ? "null" : $"'{toDate.AddDays(1).ToString("yyyyMMdd")}'";
			var sql = $"SELECT * FROM csfn_FreighStaffAssignmentsAndOrgAttributions('{companyPK}', {fromDateStr}, {toDateStr}, 'I', '', 'AU', 'B', '', '', '', 'S', null, null, null, null)";
			return DataUtils.GetDataTableFromQuery(TestConnection, sql);
		}
	}
}
