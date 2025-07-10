using CargoWise.Data;
using Enterprise.Integration;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.DbHealth.Check
{
	sealed class DbCheckRunnerTest : TestCase
	{
		public void TestRun()
		{
			var testDbCheck = new DbCheckRunnerForTesting();
			var testLogger = new TestServiceLogger();
			testDbCheck.PerformChecks_Exposed(Db.ServerName, Db.DatabaseName, testLogger);
			AssertEquals("Information|Test checker", testLogger[0]);
			AssertEquals("Information|DbHealthCheck is completed", testLogger[1]);
			AssertEquals(1, testDbCheck.TestChecker.CheckCount);
		}

		class DbCheckRunnerForTesting : DbCheckRunner
		{
			public DbHealthWarningList PerformChecks_Exposed(string dbServer, string mainDbName, ILogger logger)
			{
				return this.PerformChecks(dbServer, mainDbName, logger, new IChecker[] { TestChecker });
			}

			public readonly DummyChecker TestChecker = new DummyChecker();
		}
	}
}
