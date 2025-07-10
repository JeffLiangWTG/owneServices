using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.ToBeClassified;
using Enterprise.Build.Database.Script.Public.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.ToBeClassified
{
	[TestedType(typeof(vw_Report_GLChartOfAccountsMapping))]
	class vw_Report_GLChartOfAccountsMappingTest : DbCreateScriptTest
	{
		public void TestContainUnits()
		{
			var testDBHelper = new TestDbHelper(Db.Connection);
			var accountPK = TestHelper.InsertGLHeader("2201.02.01", "Test Account 1", "P&L", "");
			var accountPK1 = TestHelper.InsertGLHeader("2202.02.01", "Test Account 2", "NTE", "");
			var accountPK2 = TestHelper.InsertGLHeader("2203.02.01", "Test Account 3", "P&L", "");
			var accountPK3 = TestHelper.InsertGLHeader("2204.02.01", "Test Account 4", "NTE", "KWH");

			TestHelper.CreateGLAggregate(testDBHelper, "", 5, 100, 201201, accountPK, testDBHelper.DefaultDepartmentPK, testDBHelper.DefaultBranchPK, TestDbHelper.DefaultCompanyPK);
			TestHelper.CreateGLAggregate(testDBHelper, "", 6, 100, 201201, accountPK1, testDBHelper.DefaultDepartmentPK, testDBHelper.DefaultBranchPK, TestDbHelper.DefaultCompanyPK);

			TestHelper.InsertGLMappingAccount("", "2201.02.01", accountPK2, "Test", "COA", "");
			TestHelper.InsertGLMappingAccount("", "2202.02.01", accountPK3, "Test", "COA", "NTE");

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM dbo.vw_Report_GLChartOfAccountsMapping");
			AssertEquals("Result should have 2 rows", 2, result.Rows.Count);
			var rowselected = result.Select("LocalAccountNumber = '2201.02.01'");
			AssertEquals(1, rowselected.Length);
			AssertEquals("2203.02.01", rowselected[0]["GLHeaderAccountNumber"]);
			AssertEquals("", rowselected[0]["Units"]);
			rowselected = result.Select("LocalAccountNumber = '2202.02.01'");
			AssertEquals(1, rowselected.Length);
			AssertEquals("2204.02.01", rowselected[0]["GLHeaderAccountNumber"]);
			AssertEquals("KWH", rowselected[0]["Units"]);
		}
	}
}

