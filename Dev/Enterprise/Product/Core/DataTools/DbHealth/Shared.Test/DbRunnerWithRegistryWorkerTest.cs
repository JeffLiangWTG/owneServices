using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.DbHealth.Shared.Test
{
	using System.Collections.Generic;
	using System.Globalization;
	using CargoWise.Common;

	public interface ICheckerWithRegistryWorkerForTest
	{
		string RegistryDb { get; set; }
		string RegistryTableView { get; set; }
		int RegistryStep { get; set; }
		IDbRegistryWorker RegistryWorker_Exposed { get; }
		int ForceStepTimeOut { get; set; }
		void Run(DbConnection connection, IEnumerable<string> dbNames);
		bool IsStuckExceptionThrown { get; set; }
	}

	public static class DbWorker
	{
		/// <summary>
		/// Create test database with some tables, views and indexed views
		/// </summary>
		public static void CreateTestDb(string testDbName, bool createTables = true)
		{
			DropTestDbIfExists(testDbName);

			using (var conn = Db.NewAdminConnection())
			{
				conn.CreateDatabase(testDbName);
			}

			if (createTables)
			{
				using (((ICurrentDbControl)Db.Connection).UseDatabase(testDbName))
				{
					string sqlText = @"
					CREATE TABLE dbo.Table1(
						id int NOT NULL PRIMARY KEY, 
						a int NOT NULL, 
						b float(53) NOT NULL
					)";
					Db.Connection.ExecuteNonQuery(sqlText);

					sqlText = @"
					CREATE TABLE dbo.Table2(
						id2 int NOT NULL PRIMARY KEY, 
						a2 int NOT NULL, 
						b2 float(53) NOT NULL
					)";
					Db.Connection.ExecuteNonQuery(sqlText);

					sqlText = @"
					CREATE VIEW dbo.View1 WITH SCHEMABINDING AS
						SELECT a, sum(b) as b, COUNT_BIG(*) AS c FROM dbo.Table1 group by a";
					Db.Connection.ExecuteNonQuery(sqlText);

					sqlText = @"
					CREATE VIEW dbo.View2 WITH SCHEMABINDING AS
						SELECT a, sum(b) as b, COUNT_BIG(*) AS c FROM dbo.Table1 group by a";
					Db.Connection.ExecuteNonQuery(sqlText);

					sqlText = "CREATE UNIQUE CLUSTERED INDEX idx ON dbo.View1(a)";
					Db.Connection.ExecuteNonQuery(sqlText);

					sqlText = @"
					INSERT dbo.Table1(id, a, b) VALUES
						(1, 1, 1.0e1),
						(2, 1, 1.0e2),
						(3, 2, 1.0e0),
						(4, 2, 5.0e-17),
						(5, 2, 5.0e-17),
						(6, 2, 5.0e-17);";
					Db.Connection.ExecuteNonQuery(sqlText);

					sqlText = @"
					INSERT dbo.Table2(id2, a2, b2) VALUES
						(1, 1, 1.0e1),
						(2, 1, 1.0e2),
						(3, 2, 1.0e0),
						(4, 2, 5.0e-17),
						(5, 2, 5.0e-17),
						(6, 2, 5.0e-17);";
					Db.Connection.ExecuteNonQuery(sqlText);
				}
			}
		}

		public static void DropTestDbIfExists(string testDbName)
		{
			var sql = string.Format(CultureInfo.InvariantCulture,
				"if (EXISTS (SELECT NULL FROM sys.databases WHERE name = N{0})) DROP DATABASE {1};"
				, testDbName.QuoteName('\'') // 0
				, testDbName.QuoteName()     // 1
				);
			using (var adminConnection = Db.NewAdminConnection())
			{
				adminConnection.ExecuteNonQuery(sql);
			}
		}

		public static int GetNumberOfTables(string dbName)
		{
			string formatText = "SELECT count(*) FROM [{0}].sys.objects WHERE type in ('S', 'U')";
			return (int)Db.Connection.ExecuteScalar(string.Format(CultureInfo.InvariantCulture, formatText, dbName));
		}
	}
}

namespace Enterprise.DbHealth.Shared.Test
{
	public abstract class DbRunnerWithRegistryWorkerTest : TestCase
	{
		public void TestCheckWithTimeouts()
		{
			TestCheckWithTimeoutsCore();
			Assert(true);
		}

		protected virtual void TestCheckWithTimeoutsCore()
		{
			ICheckerWithRegistryWorkerForTest testChecker = GetTestObject();
			string testDbName = "DbForConsistencyTest_BFAD575548084627B6F71EB5690620CD";
			string testDbName2 = testDbName + "2";

			try
			{
				using (var adminConnection = Db.NewAdminConnection())
				using (((ICurrentDbControl)adminConnection).UseDatabase(Db.Connection.CurrentDatabase))
				{
					DbWorker.CreateTestDb(testDbName);
					DbWorker.CreateTestDb(testDbName2);

					testChecker.ForceStepTimeOut = 2;
					testChecker.Run(adminConnection, new string[] { testDbName, testDbName2 });

					AssertRegValues(testDbName, "[dbo].[Table1]", 2, testChecker);

					testChecker.RegistryDb = testDbName2;
					testChecker.RegistryTableView = "[dbo].[Table2]";
					testChecker.RegistryStep = 2;
					testChecker.Run(adminConnection, new string[] { testDbName, testDbName2 });

					AssertRegValues(testDbName2, "[dbo].[Table2]", 2, testChecker);
					Assert(testChecker.IsStuckExceptionThrown);

					testChecker.ForceStepTimeOut = 0;
					testChecker.Run(adminConnection, new string[] { testDbName, testDbName2 });

					AssertRegValues("", "", 0, testChecker);
				}
			}
			finally
			{
				DbWorker.DropTestDbIfExists(testDbName);
				DbWorker.DropTestDbIfExists(testDbName2);
			}
		}

		void AssertRegValues(string db, string tab, int step, ICheckerWithRegistryWorkerForTest testChecker)
		{
			AssertEquals(db, testChecker.RegistryDb);
			AssertEquals(tab, testChecker.RegistryTableView);
			if (testChecker.RegistryWorker_Exposed.SteppingApplied)
			{
				AssertEquals(step, testChecker.RegistryStep);
			}
		}

		ICheckerWithRegistryWorkerForTest testObject;
		protected ICheckerWithRegistryWorkerForTest TestObject
		{
			get
			{
				if (testObject == null)
				{
					testObject = GetTestObject();
				}
				return testObject;
			}
		}

		protected abstract ICheckerWithRegistryWorkerForTest GetTestObject();

		protected override void SetUp()
		{
			base.SetUp();
			TestObject.ForceStepTimeOut = 0;
		}

		protected override void TearDown()
		{
			DbCommitTracker.Ignore("DbForConsistencyTest");
			base.TearDown();
		}
	}
}
