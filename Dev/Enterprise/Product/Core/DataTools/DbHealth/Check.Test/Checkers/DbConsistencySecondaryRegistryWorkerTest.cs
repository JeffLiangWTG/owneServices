using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.DbHealth.Check.Testing
{
	sealed class DbConsistencySecondaryRegistryWorkerTest : TestCase
	{
		public void TestSkipStepping()
		{
			var worker = new DbConsistencySecondaryRegistryWorker(Db.Connection, Db.DatabaseName);

			AssertEquals("PRECONDITION: Step", 0, worker.LastCheckStepExposed);
			AssertEquals("PRECONDITION: Database", string.Empty, worker.LastCheckStepDatabaseExposed);
			AssertEquals("PRECONDITION: TableOrView", string.Empty, worker.LastCheckStepTableOrViewExposed);
			AssertEquals("PRECONDITION: ElapsedTime", string.Empty, worker.LastCheckStepElapsedTimeExposed);

			worker.LastCheckStepExposed = 1;
			worker.LastCheckStepDatabaseExposed = "testDb";
			worker.LastCheckStepTableOrViewExposed = "testObject";
			worker.LastCheckStepElapsedTimeExposed = "01:02:03.444";

			AssertEquals("Step", 0, worker.LastCheckStepExposed);
			AssertEquals("Database", string.Empty, worker.LastCheckStepDatabaseExposed);
			AssertEquals("TableOrView", string.Empty, worker.LastCheckStepTableOrViewExposed);
			AssertEquals("ElapsedTime", string.Empty, worker.LastCheckStepElapsedTimeExposed);
		}
	}
}
