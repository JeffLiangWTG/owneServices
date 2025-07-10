using System;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.DbHealth.Check
{
	sealed class DbConsistencyRegistryWorkerTransactionedTest : TransactionedTestCase
	{
		public void TestUsingCorrectExtendedProperties()
		{
			DataUtils.DropDbExtendedProperty(Db.Connection, DbConsistencyRegistryWorker.ConsistencyCheckerLastCheckStepPropertyName);
			DataUtils.DropDbExtendedProperty(Db.Connection, DbConsistencyRegistryWorker.ConsistencyCheckerLastRunningDbPropertyName);
			DataUtils.DropDbExtendedProperty(Db.Connection, DbConsistencyRegistryWorker.ConsistencyCheckerLastRunningTablePropertyName);
			DataUtils.DropDbExtendedProperty(Db.Connection, DbConsistencyRegistryWorker.ConsistencyCheckerLastRunningElapsedTimePropertyName);

			var worker = new DbConsistencyRegistryWorker(Db.Connection, Db.DatabaseName);
			AssertEquals("PRECONDITION: Step", 0, worker.LastCheckStepExposed);
			AssertEquals("PRECONDITION: Database", String.Empty, worker.LastCheckStepDatabaseExposed);
			AssertEquals("PRECONDITION: TableOrView", String.Empty, worker.LastCheckStepTableOrViewExposed);
			AssertEquals("PRECONDITION: ElapsedTime", String.Empty, worker.LastCheckStepElapsedTimeExposed);

			worker.LastCheckStepExposed = 1;
			worker.LastCheckStepDatabaseExposed = "testDb";
			worker.LastCheckStepTableOrViewExposed = "testObject";
			worker.LastCheckStepElapsedTimeExposed = "01:02:03.444";

			AssertEquals("New Step", 1, worker.LastCheckStepExposed);
			AssertEquals("New Database", "testDb", worker.LastCheckStepDatabaseExposed);
			AssertEquals("New TableOrView", "testObject", worker.LastCheckStepTableOrViewExposed);
			AssertEquals("New ElapsedTime", "01:02:03.444", worker.LastCheckStepElapsedTimeExposed);

			AssertEquals("New Step", "1", DataUtils.LoadDbExtendedProperty(TestConnection, DbConsistencyRegistryWorker.ConsistencyCheckerLastCheckStepPropertyName));
			AssertEquals("New Database", "testDb", DataUtils.LoadDbExtendedProperty(TestConnection, DbConsistencyRegistryWorker.ConsistencyCheckerLastRunningDbPropertyName));
			AssertEquals("New TableOrView", "testObject", DataUtils.LoadDbExtendedProperty(TestConnection, DbConsistencyRegistryWorker.ConsistencyCheckerLastRunningTablePropertyName));
			AssertEquals("New ElapsedTime", "01:02:03.444", DataUtils.LoadDbExtendedProperty(TestConnection, DbConsistencyRegistryWorker.ConsistencyCheckerLastRunningElapsedTimePropertyName));
		}
	}
}
