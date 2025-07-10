using CargoWise.Data;
using Enterprise.DbHealth.Shared.Test;

namespace Enterprise.DbHealth.Check
{
	sealed class DbConsistencyCheckerRegistryTest : DbRunnerWithRegistryWorkerTest
	{
		protected override ICheckerWithRegistryWorkerForTest GetTestObject()
		{
			return new DbConsistencyCheckerForTest();
		}

		public void TestRun()
		{
			var testChecker = (DbConsistencyCheckerForTest)GetTestObject();
			var warningList = new DbHealthWarningList();
			var logger = testChecker.Logger;

			var testDbName = "DbForConsistencyTest-BFAD575548084627B6F71EB5690620CD";
			testChecker.DatabasesToCheckOverride = new string[] { testDbName };

			try
			{
				using (var adminConnection = Db.NewAdminConnection())
				using (((ICurrentDbControl)adminConnection).UseDatabase(Db.Connection.CurrentDatabase))
				{
					DbWorker.CreateTestDb(testDbName);

					(testChecker as IChecker).Check(adminConnection, warningList, logger);
					AssertEquals("No warnings", 0, warningList.Count);
				}
			}
			finally
			{
				DbWorker.DropTestDbIfExists(testDbName);
			}

			(testChecker as IChecker).Check(Db.Connection, warningList, logger);
			AssertEquals("Warnings", 1, warningList.Count);
		}
	}
}
