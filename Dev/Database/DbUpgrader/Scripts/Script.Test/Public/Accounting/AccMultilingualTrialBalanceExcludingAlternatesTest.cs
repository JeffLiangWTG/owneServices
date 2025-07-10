using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(AccMultilingualTrialBalanceExcludingAlternates))]
	class AccMultilingualTrialBalanceExcludingAlternatesTest : DbCreateScriptTest
	{
		public void TestHandleNoteJournal()
		{
			var testDBHelper = new TestDbHelper(Db.Connection);
			var accountPK = TestHelper.InsertGLHeader("8000.22.00", "Test Account 1", "NTE", "");
			TestHelper.CreateGLAggregate(testDBHelper, "", 5, 100, 202011, accountPK, testDBHelper.DefaultDepartmentPK, testDBHelper.DefaultBranchPK, TestDbHelper.DefaultCompanyPK);
			TestHelper.CreateGLAggregate(testDBHelper, "", 6, 100, 201911, accountPK, testDBHelper.DefaultDepartmentPK, testDBHelper.DefaultBranchPK, TestDbHelper.DefaultCompanyPK);

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM AccMultilingualTrialBalanceExcludingAlternates(202011, '{TestDbHelper.DefaultCompanyPK}')");

			DataRow rowSelected = (from DataRow row in result.Rows where row["AG_PK"].Equals(accountPK) select row).FirstOrDefault();
			AssertEquals("CurrentAmount field", 500M, rowSelected["CurrentAmount"]);
			AssertEquals("ClosingAmount field", 1100M, rowSelected["ClosingAmount"]);
		}
	}
}

