using System;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Shared
{
	public sealed class UpgCommandRunnerTest : TransactionedTestCase
	{
		public void TestRunCommandOnGivenDb()
		{
			using (((ICurrentDbControl)TestConnection).UseDatabase(Db.SqlMasterDb))
			{
				string nonexistingView = "CommandRunnerTest_NonexistingView";

				AssertObjectExists(Db.DatabaseName, nonexistingView, false);
				AssertObjectExists(Db.SqlMasterDb, nonexistingView, false);

				UpgCommandRunner.RunCommandOnGivenDb(
					TestConnection, Db.DatabaseName,
					String.Format("CREATE VIEW {0} AS SELECT GETDATE() ColumnNow", nonexistingView));

				AssertObjectExists(Db.DatabaseName, nonexistingView, true);
				AssertObjectExists(Db.SqlMasterDb, nonexistingView, false);
			}
		}

		void AssertObjectExists(string dbName, string objectName, bool expectedResult)
		{
			string sqlText = String.Format("SELECT count(*) FROM {0}.sys.objects WHERE name = '{1}'", dbName, objectName);
			int count = (int)TestConnection.ExecuteScalar(sqlText);
			AssertEquals(string.Format("[{0}] should {1}exist on {2}", objectName, expectedResult ? "" : "NOT ", dbName), expectedResult, count == 1);
		}
	}
}
