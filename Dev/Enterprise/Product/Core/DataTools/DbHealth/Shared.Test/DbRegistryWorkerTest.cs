using System.Linq;
using NUnit.Framework;

namespace Enterprise.DbHealth.Shared.Test
{
	public class DbRegistryWorkerTest : TestCase
	{
		public void TestGetDbListFromStartDb()
		{
			var allDbs = new string[] { "DBA", "DBF", "DBM", "DBX" };

			var worker = GetRegistryWorker() as DbRegistryWorkerForTest;
			worker.RegStep = 2;
			worker.RegTab = "Table";

			var expected = new string[] { "DBA", "DBF", "DBM", "DBX" };
			AssertArrayEqualsByElements("Start = ''", expected, worker.GetDbListFromStartDb(allDbs).ToArray());
			AssertEquals("Start step", 0, worker.StartStep);
			AssertEquals("Start table", "", worker.StartTabOrView);

			worker.RegDb = "DBX";
			worker.LoadStartPoint();
			expected = new string[] { "DBX" };
			AssertArrayEqualsByElements("Start = 'DBX'", expected, worker.GetDbListFromStartDb(allDbs).ToArray());
			AssertEquals("Start step", 2, worker.StartStep);
			AssertEquals("Start table", "Table", worker.StartTabOrView);

			worker.RegDb = "DBA";
			worker.LoadStartPoint();
			expected = new string[] { "DBA", "DBF", "DBM", "DBX" };
			AssertArrayEqualsByElements("Start = 'DBA'", expected, worker.GetDbListFromStartDb(allDbs).ToArray());
			AssertEquals("Start step", 2, worker.StartStep);
			AssertEquals("Start table", "Table", worker.StartTabOrView);

			worker.RegDb = "DBH";
			worker.LoadStartPoint();
			expected = new string[] { "DBA", "DBF", "DBM", "DBX" };
			AssertArrayEqualsByElements("Start = 'DBH'", expected, worker.GetDbListFromStartDb(allDbs).ToArray());
			AssertEquals("Start step", 0, worker.StartStep);
			AssertEquals("Start table", "", worker.StartTabOrView);
		}

		public void TestIsStuck()
		{
			DbRegistryWorkerForTest worker = GetRegistryWorker() as DbRegistryWorkerForTest;

			worker.StartDB = "";
			worker.StartTabOrView = "";
			worker.StartStep = 0;

			worker.CurrentDatabase = "";
			worker.CurrentTableOrView = "";
			worker.CurrentStep = 0;

			Assert(!worker.IsStuckAtInitialPoint);

			worker.StartDB = "db";
			worker.StartTabOrView = "";
			worker.StartStep = 0;

			worker.CurrentDatabase = "";
			worker.CurrentTableOrView = "";
			worker.CurrentStep = 0;

			Assert(!worker.IsStuckAtInitialPoint);

			worker.StartDB = "db";
			worker.StartTabOrView = "tab";
			worker.StartStep = 5;

			worker.CurrentDatabase = "";
			worker.CurrentTableOrView = "";
			worker.CurrentStep = 0;

			Assert(!worker.IsStuckAtInitialPoint);

			worker.StartDB = "";
			worker.StartTabOrView = "";
			worker.StartStep = 0;

			worker.CurrentDatabase = "db";
			worker.CurrentTableOrView = "";
			worker.CurrentStep = 0;

			Assert(!worker.IsStuckAtInitialPoint);

			worker.StartDB = "";
			worker.StartTabOrView = "";
			worker.StartStep = 0;

			worker.CurrentDatabase = "db";
			worker.CurrentTableOrView = "tab";
			worker.CurrentStep = 3;

			Assert(!worker.IsStuckAtInitialPoint);

			worker.StartDB = "db1";
			worker.StartTabOrView = "tab1";
			worker.StartStep = 1;

			worker.CurrentDatabase = "db2";
			worker.CurrentTableOrView = "tab1";
			worker.CurrentStep = 1;

			Assert(!worker.IsStuckAtInitialPoint);

			worker.StartDB = "db1";
			worker.StartTabOrView = "tab1";
			worker.StartStep = 1;

			worker.CurrentDatabase = "db1";
			worker.CurrentTableOrView = "tab2";
			worker.CurrentStep = 1;

			Assert(!worker.IsStuckAtInitialPoint);

			worker.StartDB = "db1";
			worker.StartTabOrView = "tab1";
			worker.StartStep = 0;

			worker.CurrentDatabase = "db1";
			worker.CurrentTableOrView = "tab1";
			worker.CurrentStep = 1;

			Assert(!worker.IsStuckAtInitialPoint);

			worker.StartDB = "db1";
			worker.StartTabOrView = "tab1";
			worker.StartStep = 0;

			worker.CurrentDatabase = "db1";
			worker.CurrentTableOrView = "tab1";
			worker.CurrentStep = 0;

			Assert(worker.IsStuckAtInitialPoint);

			worker.StartDB = "";
			worker.StartTabOrView = "tab1";
			worker.StartStep = 0;

			worker.CurrentDatabase = "";
			worker.CurrentTableOrView = "tab1";
			worker.CurrentStep = 0;

			Assert(worker.IsStuckAtInitialPoint);
		}

		DbRegistryWorker GetRegistryWorker()
		{
			return new DbRegistryWorkerForTest();
		}
	}
}
