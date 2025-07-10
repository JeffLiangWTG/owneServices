using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.ProfitLoss;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.ProfitLoss.Testing
{
	[TestedType(typeof(RetainedEarnings))]
	class RetainedEarningsTest : DbCreateScriptTest
	{
		public void TestNteGLAccountIsNotIncluded()
		{
			var parentGLPnl = "2201.02.01";
			var parentGLNte = "2202.02.01";

			var testDBHelper = new TestDbHelper(TestConnection);
			var parentAccountPK1 = TestHelper.InsertGLHeader(parentGLPnl, "Test Account 1", "P&L", "");
			var parentAccountPK2 = TestHelper.InsertGLHeader(parentGLNte, "Test Account 2", "NTE", "KWH");

			TestHelper.CreateGLAggregate(testDBHelper, "", 1, 90, 201203, parentAccountPK1, testDBHelper.DefaultDepartmentPK, testDBHelper.DefaultBranchPK, TestDbHelper.DefaultCompanyPK);
			TestHelper.CreateGLAggregate(testDBHelper, "", 1, 100, 201201, parentAccountPK1, testDBHelper.DefaultDepartmentPK, testDBHelper.DefaultBranchPK, TestDbHelper.DefaultCompanyPK);
			TestHelper.CreateGLAggregate(testDBHelper, "", 1, 120, 201201, parentAccountPK2, testDBHelper.DefaultDepartmentPK, testDBHelper.DefaultBranchPK, TestDbHelper.DefaultCompanyPK);

			var result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT dbo.RetainedEarnings({201201}, {201202}, '{TestDbHelper.DefaultCompanyPK}')");
			AssertEquals("Result should have 1 rows", 1, result.Rows.Count);
			var amountAggregated = result.Rows[0][0];
			AssertEquals("Should only include non-Note journal within given accounting period", 100m, amountAggregated);
		}
	}
}

