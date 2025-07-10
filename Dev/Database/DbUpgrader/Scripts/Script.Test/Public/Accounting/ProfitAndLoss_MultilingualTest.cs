using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(ProfitAndLoss_Multilingual))]
	class ProfitAndLoss_MultilingualTest : DbCreateScriptTest
	{
		public void TestContainUnits()
		{
			var testDBHelper = new TestDbHelper(TestConnection);
			var accountPK = TestHelper.InsertGLHeader("2201.02.01", "Test Account 1", "P&L", "");
			var accountPK1 = TestHelper.InsertGLHeader("2202.02.01", "Test Account 2", "P&L", "");
			var accountPK2 = TestHelper.InsertGLHeader("2203.02.01", "Test Account 3", "P&L", "");
			var accountPK3 = TestHelper.InsertGLHeader("2204.02.01", "Test Account 4", "P&L", "KWH");

			TestHelper.CreateGLAggregate(testDBHelper, "", 5, 100, 201101, accountPK2, testDBHelper.DefaultDepartmentPK, testDBHelper.DefaultBranchPK, TestDbHelper.DefaultCompanyPK);
			TestHelper.CreateGLAggregate(testDBHelper, "", 6, 100, 201101, accountPK3, testDBHelper.DefaultDepartmentPK, testDBHelper.DefaultBranchPK, TestDbHelper.DefaultCompanyPK);
			TestHelper.CreateGLAggregate(testDBHelper, "", 7, 100, 201201, accountPK2, testDBHelper.DefaultDepartmentPK, testDBHelper.DefaultBranchPK, TestDbHelper.DefaultCompanyPK);
			TestHelper.CreateGLAggregate(testDBHelper, "", 8, 100, 201201, accountPK3, testDBHelper.DefaultDepartmentPK, testDBHelper.DefaultBranchPK, TestDbHelper.DefaultCompanyPK);

			TestHelper.InsertGLMappingAccount("", "2201.02.01", accountPK2, "Test", "COA", "");
			TestHelper.InsertGLMappingAccount("", "2202.02.01", accountPK3, "Test", "COA", "");

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM ProfitAndLoss_Multilingual(201201, '00000000', '99999999', '{TestDbHelper.DefaultCompanyPK}', '', '', '', '', 'Y')");
			AssertEquals("Result should have 2 rows", 2, result.Rows.Count);

			DataRow rowSelected = (from DataRow row in result.Rows where row["AccountNumber"].ToString().StartsWith("2201") select row).FirstOrDefault();
			AssertResult("", -700, -700, -500);
			rowSelected = (from DataRow row in result.Rows where row["AccountNumber"].ToString().StartsWith("2202") select row).FirstOrDefault();
			AssertResult("KWH", -800, -800, -600);

			void AssertResult(string units, decimal currentPeriod, decimal yearToPeriod, decimal lastYearTotal)
			{
				AssertEquals("Units field", units, rowSelected["Units"].ToString());
				AssertEquals("CurrentPeriod field", currentPeriod, rowSelected["CurrentPeriod"]);
				AssertEquals("YearToPeriod field", yearToPeriod, rowSelected["YearToPeriod"]);
				AssertEquals("LastYearTotal field", lastYearTotal, rowSelected["LastYearTotal"]);
			}
		}

		public void TestHandleNoteJournal()
		{
			var testDBHelper = new TestDbHelper(Db.Connection);
			var accountPK = TestHelper.InsertGLHeader("2201.02.01", "Test Account 1", "NTE", "KWH");
			var accountPK1 = TestHelper.InsertGLHeader("2202.02.01", "Test Account 2", "NTE", "KWH");

			TestHelper.CreateGLAggregate(testDBHelper, "", 5, 100, 201101, accountPK, testDBHelper.DefaultDepartmentPK, testDBHelper.DefaultBranchPK, TestDbHelper.DefaultCompanyPK);
			TestHelper.CreateGLAggregate(testDBHelper, "", 6, 100, 201101, accountPK1, testDBHelper.DefaultDepartmentPK, testDBHelper.DefaultBranchPK, TestDbHelper.DefaultCompanyPK);
			TestHelper.CreateGLAggregate(testDBHelper, "", 7, 100, 201201, accountPK, testDBHelper.DefaultDepartmentPK, testDBHelper.DefaultBranchPK, TestDbHelper.DefaultCompanyPK);
			TestHelper.CreateGLAggregate(testDBHelper, "", 8, 100, 201201, accountPK1, testDBHelper.DefaultDepartmentPK, testDBHelper.DefaultBranchPK, TestDbHelper.DefaultCompanyPK);

			TestHelper.InsertGLMappingAccount("ZH-CN", "22010201", accountPK, "Test", "COA", "NTE", countryOfCompliance: "CN");
			TestHelper.InsertGLMappingAccount("ZH-CN", "22020201", accountPK1, "Test", "COA", "NTE", countryOfCompliance: "CN");

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM ProfitAndLoss_Multilingual(201201, '00000000', '99999999', '{TestDbHelper.DefaultCompanyPK}', '', '', 'ZH-CN', 'CN', 'Y')");
			AssertEquals("Result should have 2 rows", 2, result.Rows.Count);

			DataRow rowSelected = (from DataRow row in result.Rows where row["AccountNumber"].ToString().StartsWith("2201") select row).FirstOrDefault();
			AssertResult("NTE", 700, 700, 500);
			rowSelected = (from DataRow row in result.Rows where row["AccountNumber"].ToString().StartsWith("2202") select row).FirstOrDefault();
			AssertResult("NTE", 800, 800, 600);

			void AssertResult(string accountType, decimal currentPeriod, decimal yearToPeriod, decimal lastYearTotal)
			{
				AssertEquals("AccountType field", accountType, rowSelected["AccountType"].ToString());
				AssertEquals("CurrentPeriod field", currentPeriod, rowSelected["CurrentPeriod"]);
				AssertEquals("YearToPeriod field", yearToPeriod, rowSelected["YearToPeriod"]);
				AssertEquals("LastYearTotal field", lastYearTotal, rowSelected["LastYearTotal"]);
			}
		}
	}
}

