using System;
using System.Data;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	sealed class DbCommandAdoRollbackScenariosTest : TestCase
	{
		public void TestAdoTransactionScenario001_SneakyBeginTran()
		{
			using (var conn = Db.NewExtraConnectionToMainDb())
			{
				conn.ExecuteNonQuery("BEGIN TRANSACTION");

				try
				{
					conn.CommitTransaction();
				}
				catch (Exception ex)
				{
					AssertEquals("Cannot commit => no corresponding transaction", ex.Message);
				}

				// Calling RollbackTransaction should not do anything: no rollback, no exception.
				conn.RollbackTransaction();
			}
		}

		public void TestAdoTransactionScenario002_SneakyBeginAndRollback()
		{
			using (var conn = Db.NewExtraConnectionToMainDb())
			{
				conn.ExecuteNonQuery("BEGIN TRANSACTION");
				conn.ExecuteNonQuery("CREATE TABLE T1 (Col1 int, Col2 char(10))");
				conn.ExecuteNonQuery("ROLLBACK TRANSACTION");
				int x = (int)conn.ExecuteScalar("SELECT count(*) FROM sys.tables WHERE name = 'T1'");
				AssertEquals(0, x);
			}
		}

		public void TestAdoTransactionScenario003_SneakyRollback()
		{
			AssertAdoRollbackScenario(c => c.ExecuteNonQuery("ROLLBACK TRANSACTION"), null);
		}

		public void TestAdoTransactionScenario003_SneakyRollback_WithNestedTransaction()
		{
			AssertAdoRollbackScenario(
				c =>
				{
					c.BeginTransaction();
					c.ExecuteNonQuery("ROLLBACK");
					AssertEquals("Connection is in transaction?", false, c.IsInTransaction);

					try
					{
						c.ExecuteScalar("SELECT count(*) FROM sys.tables WHERE name = 'T1'");
						Fail("Exception expected");
					}
					catch (Exception ex)
					{
						AssertEquals("Transaction has been rolled back in the server (application transaction count pending reset).", ex.Message);
					}

					c.RollbackTransaction();
				},
				null
			);
		}

		[UseSnapshotProtection]
		public void TestAdoTransactionScenario004_TriggerRollback()
		{
			try
			{
				Db.Connection.ExecuteNonQuery("CREATE TABLE T004 (Col1 int, Col2 char(10))");
				Db.Connection.ExecuteNonQuery("CREATE TRIGGER TG4 ON T004 AFTER INSERT AS ROLLBACK;");

				AssertAdoRollbackScenario(
					c => c.ExecuteNonQuery("INSERT T004 VALUES (1, 'um'), (2, 'dois')"),
					exceptionMessage => AssertEquals("The transaction ended in the trigger. The batch has been aborted.", exceptionMessage));
			}
			finally
			{
				Db.Connection.ExecuteNonQuery("DROP TABLE T004");
			}
		}

		[UseSnapshotProtection]
		public void TestAdoTransactionScenario005_ProcRollback()
		{
			try
			{
				Db.Connection.ExecuteNonQuery("CREATE PROCEDURE P005 AS ROLLBACK;");

				AssertAdoRollbackScenario(
					c => c.ExecuteNonQuery("EXEC P005"),
					exceptionMessage => AssertEquals("Transaction count after EXECUTE indicates a mismatching number of BEGIN and COMMIT statements. Previous count = 1, current count = 0.", exceptionMessage));
			}
			finally
			{
				Db.Connection.ExecuteNonQuery("DROP PROCEDURE P005");
			}
		}

		[UseSnapshotProtection]
		public void TestAdoTransactionScenario006_KillSpid()
		{
			AssertAdoRollbackScenario(
				c =>
				{
					AdoTestUtils.KillConnection(c);
					AssertEquals("Connection is in transaction?", true, c.IsInTransaction);
					c.EnsureIsOpen();
				},
				exceptionMessage => AssertStartsWith("exception message", "A transport-level error has occurred when receiving results from the server. (provider: ", exceptionMessage));
		}

		[UseSnapshotProtection]
		public void TestAdoTransactionScenario006_KillSpid_WithNestedTransaction()
		{
			AssertAdoRollbackScenario(
				c =>
				{
					c.BeginTransaction();

					AdoTestUtils.KillConnection(c);
					AssertEquals("Connection is in transaction?", true, c.IsInTransaction);

					try
					{
						c.EnsureIsOpen();
						Fail("Exception expected");
					}
					catch (Exception ex)
					{
						AssertStartsWith("exception message", "A transport-level error has occurred when receiving results from the server. (provider: ", ex.Message);
					}

					AssertEquals("Connection is in transaction?", false, c.IsInTransaction);

					try
					{
						c.ExecuteScalar("SELECT count(*) FROM sys.tables WHERE name = 'T1'");
						Fail("Exception expected");
					}
					catch (Exception ex)
					{
						AssertEquals("Transaction has been rolled back in the server (application transaction count pending reset).", ex.Message);
					}

					c.RollbackTransaction();
				},
				null
			);
		}

		public void TestAdoTransactionScenario006_KillSpid_WithNoTransaction()
		{
			using (var conn = Db.NewExtraConnectionToMainDb())
			{
				AssertEquals("Connection state", ConnectionState.Closed, conn.State);
				conn.EnsureIsOpen();
				AssertEquals("Connection state", ConnectionState.Open, conn.State);
				AdoTestUtils.KillConnection(conn);
				conn.EnsureIsOpen();
				AssertEquals("Connection state", ConnectionState.Open, conn.State);
			}
		}

		public void TestAdoTransactionScenario007_ErrorSeverityRollback()
		{
			AssertAdoRollbackScenario(
				c => c.ExecuteNonQuery("CREATE TABLE T1 (Col1 int)"),
				exceptionMessage => AssertEquals("There is already an object named 'T1' in the database.", exceptionMessage));
		}

		public void TestAdoTransactionScenario007_ErrorSeverityRollback_WithNestedTransaction()
		{
			AssertAdoRollbackScenario(
				c =>
				{
					c.BeginTransaction();

					try
					{
						c.ExecuteScalar("CREATE TABLE T1 (Col1 int)");
						Fail("Exception expected");
					}
					catch (Exception ex)
					{
						AssertEquals("There is already an object named 'T1' in the database.", ex.Message);
					}

					AssertEquals("Connection is in transaction?", false, c.IsInTransaction);

					try
					{
						c.ExecuteScalar("SELECT count(*) FROM sys.tables WHERE name = 'T1'");
						Fail("Exception expected");
					}
					catch (Exception ex)
					{
						AssertEquals("Transaction has been rolled back in the server (application transaction count pending reset).", ex.Message);
					}

					c.RollbackTransaction();
				},
				null
			);
		}

		void AssertAdoRollbackScenario(Action<DbConnection> cmdToCauseRollback, Action<string> exceptionMessageAssertion)
		{
			using (Db.DisposableActionForDbConnection())
			using (var conn = Db.NewExtraConnectionToMainDb())
			{
				conn.BeginTransaction();

				conn.ExecuteNonQuery("CREATE TABLE T1 (Col1 int, Col2 char(10))");

				AssertEquals("Connection is in transaction?", true, conn.IsInTransaction);

				try
				{
					cmdToCauseRollback(conn);
					if (exceptionMessageAssertion != null)
					{
						Fail("Expected exception not thrown");
					}
				}
				catch (Exception ex)
				{
					exceptionMessageAssertion(ex.Message);
				}

				AssertEquals("Connection is in transaction?", false, conn.IsInTransaction);

				try
				{
					conn.ExecuteScalar("SELECT count(*) FROM sys.tables WHERE name = 'T1'");
					Fail("Exception expected");
				}
				catch (Exception ex)
				{
					AssertEquals("Transaction has been rolled back in the server (application transaction count pending reset).", ex.Message);
				}

				conn.RollbackTransaction();
				int x = (int)conn.ExecuteScalar("SELECT count(*) FROM sys.tables WHERE name = 'T1'");
				AssertEquals(0, x);
			}
		}
	}
}
