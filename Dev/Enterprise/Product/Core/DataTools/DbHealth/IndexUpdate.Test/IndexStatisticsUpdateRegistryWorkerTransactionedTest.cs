using System;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.DbHealth.IndexUpdate.Testing
{
	sealed class IndexStatisticsUpdateRegistryWorkerTransactionedTest : TransactionedTestCase
	{
		public void TestUsingCorrectExtendedProperties()
		{
			DataUtils.DropDbExtendedProperty(Db.Connection, IndexStatisticsUpdateRegistryWorker.StatisticsLastRunningDbPropertyName);
			DataUtils.DropDbExtendedProperty(Db.Connection, IndexStatisticsUpdateRegistryWorker.StatisticsLastRunningTablePropertyName);
			DataUtils.DropDbExtendedProperty(Db.Connection, IndexStatisticsUpdateRegistryWorker.StatisticsLastRunningElapsedTimePropertyName);

			var worker = new IndexStatisticsUpdateRegistryWorker();
			AssertEquals("PRECONDITION: Database", String.Empty, worker.LastCheckStepDatabaseExposed);
			AssertEquals("PRECONDITION: TableOrView", String.Empty, worker.LastCheckStepTableOrViewExposed);
			AssertEquals("PRECONDITION: ElapsedTime", String.Empty, worker.LastCheckStepElapsedTimeExposed);

			worker.LastCheckStepDatabaseExposed = "testDb";
			worker.LastCheckStepTableOrViewExposed = "testObject";
			worker.LastCheckStepElapsedTimeExposed = "01:02:03.444";

			AssertEquals("New Database", "testDb", worker.LastCheckStepDatabaseExposed);
			AssertEquals("New TableOrView", "testObject", worker.LastCheckStepTableOrViewExposed);
			AssertEquals("New ElapsedTime", "01:02:03.444", worker.LastCheckStepElapsedTimeExposed);

			AssertEquals("New Database", "testDb", DataUtils.LoadDbExtendedProperty(TestConnection, IndexStatisticsUpdateRegistryWorker.StatisticsLastRunningDbPropertyName));
			AssertEquals("New TableOrView", "testObject", DataUtils.LoadDbExtendedProperty(TestConnection, IndexStatisticsUpdateRegistryWorker.StatisticsLastRunningTablePropertyName));
			AssertEquals("New ElapsedTime", "01:02:03.444", DataUtils.LoadDbExtendedProperty(TestConnection, IndexStatisticsUpdateRegistryWorker.StatisticsLastRunningElapsedTimePropertyName));
		}
	}
}
