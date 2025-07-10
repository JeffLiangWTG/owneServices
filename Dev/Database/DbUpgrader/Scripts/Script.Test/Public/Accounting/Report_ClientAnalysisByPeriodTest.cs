using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(Report_ClientAnalysisByPeriod))]
	class Report_ClientAnalysisByPeriodTest : DbCreateScriptTest
	{
		public void TestJobInactive()
		{
			var date = new DateTime(2019, 08, 15);
			var helper = new TestDbHelper(TestConnection);
			var companyPK = TestDbHelper.DefaultCompanyPK;
			var branchPK = helper.InsertBranch("ZZB", companyPK);
			var departmentPK = helper.InsertDepartment("ZZD");
			var shipmentPK = helper.InsertShipment("S99999999", date);
			var shipment2PK = helper.InsertShipment("00002024", date);
			helper.InsertAccPeriod(2019, 07, companyPK);
			helper.InsertAccPeriod(2019, 08, companyPK);
			helper.InsertAccPeriod(2019, 09, companyPK);
			InsertJob(helper, "S99999999", TestDbHelper.DefaultCompanyPK, branchPK, departmentPK, "JS", shipmentPK, "WRK", date);
			InsertJob(helper, "00002024", TestDbHelper.DefaultCompanyPK, branchPK, departmentPK, "JK", shipment2PK, "WRK", date, false);

			var result = GetResults(companyPK, date);
			AssertEquals("Should have data with inactive job", 2, result.Rows[0]["Total"]);
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

		DataTable GetResults(Guid companyPK, DateTime date)
		{
			var sql = $"EXEC Report_ClientAnalysisByPeriod '{companyPK}', '{date.AddMonths(2).ToString("yyyyMM")}', 'I', 'C', null, null, null, null, null, null, null";
			return DataUtils.GetDataTableFromQuery(TestConnection, sql);
		}
	}
}

