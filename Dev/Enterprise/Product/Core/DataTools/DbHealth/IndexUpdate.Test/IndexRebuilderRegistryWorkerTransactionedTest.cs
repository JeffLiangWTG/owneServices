using System;
using CargoWise.Database.ExtendedProperties;
using NUnit.Framework;

namespace Enterprise.DbHealth.IndexUpdate.Testing
{
	sealed class IndexRebuilderRegistryWorkerTransactionedTest : TransactionedTestCase
	{
		public void TestUsingCorrectExtendedProperties()
		{
			var db_Main = TestConnection.CurrentDatabase;
			var db_SD001 = db_Main + "_SD001";

			ExtProperty.Database.Delete(TestConnection, IndexRebuilderRegistryWorker.LastRunningDbPropertyName);
			ExtProperty.Database.Delete(TestConnection, IndexRebuilderRegistryWorker.LastRunningIndexPropertyName);
			ExtProperty.Database.Delete(TestConnection, IndexRebuilderRegistryWorker.LastRunningElapsedTimePropertyName);

			var worker = new IndexRebuilderRegistryWorker();
			AssertEquals("PRECONDITION: Database", db_Main, worker.LastCheckStepDatabaseExposed);
			AssertEquals("PRECONDITION: Index", String.Empty, worker.LastCheckStepTableOrViewExposed);
			AssertEquals("PRECONDITION: ElapsedTime", String.Empty, worker.LastCheckStepElapsedTimeExposed);

			worker.LastCheckStepDatabaseExposed = db_SD001;
			worker.LastCheckStepTableOrViewExposed = "testIndex";
			worker.LastCheckStepElapsedTimeExposed = "01:02:03.444";

			AssertEquals("New Database", db_SD001, worker.LastCheckStepDatabaseExposed);
			AssertEquals("New Index", "testIndex", worker.LastCheckStepTableOrViewExposed);
			AssertEquals("New ElapsedTime", "01:02:03.444", worker.LastCheckStepElapsedTimeExposed);

			AssertEquals("New Database", "_SD001", ExtProperty.Database.Select(TestConnection, IndexRebuilderRegistryWorker.LastRunningDbPropertyName));
			AssertEquals("New Index", "testIndex", ExtProperty.Database.Select(TestConnection, IndexRebuilderRegistryWorker.LastRunningIndexPropertyName));
			AssertEquals("New ElapsedTime", "01:02:03.444", ExtProperty.Database.Select(TestConnection, IndexRebuilderRegistryWorker.LastRunningElapsedTimePropertyName));
		}
	}
}
