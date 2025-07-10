using System;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	internal class SnapshotLeakListenerTest : TestCase
	{
		public void TestRestoreDatabaseSnapshotMainDb() => CheckRestoresLeakedSnapshot("");
		public void TestRestoreDatabaseSnapshotAuditDb() => CheckRestoresLeakedSnapshot(Db.AuditDatabaseSuffix);
		public void TestRestoreDatabaseSnapshotEdwDb() => CheckRestoresLeakedSnapshot(Db.EdwDatabaseSuffix);

		void CheckRestoresLeakedSnapshot(string dbSuffix, bool shouldAppearAsIfOnDat = false)
		{
			var dbName = Db.DatabaseName + dbSuffix;
			using (RunWithDatState(shouldAppearAsIfOnDat))
			using (var connection = Db.NewAdminConnection(dbName))
			{
				SnapshotCreator.CreateSnapshot(connection, () => Db.Connection.CloseConnection(), dbName, UseSnapshotProtectionAttribute.GetSnapshotName(dbName));
				connection.ExecuteNonQuery("CREATE TABLE MyTableForTesting ( id int );");

				var listener = new SnapshotLeakListener();
				listener.StartAllTests(DateTime.Now);

				var count = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM sys.tables WHERE name = 'MyTableForTesting'");
				AssertEquals("Should clean up the database using the existing snapshot", 0, count);
			}
		}

		IDisposable RunWithDatState(bool setupRunOnDat)
		{
			var isActuallyOnDat = TestingState.IsRunningOnDAT;
			TestingState.IsRunningOnDAT = setupRunOnDat;
			return new DisposableAction(() => TestingState.IsRunningOnDAT = isActuallyOnDat);
		}

		public void TestDoesNotRunInDat()
		{
			try
			{
				CheckRestoresLeakedSnapshot("", shouldAppearAsIfOnDat: true);
				Fail("Should throw an expcetion because the check for a leaked snapshot should not run when running on dat");
			}
			catch (AssertionFailedError)
			{
				Assert("Should not clean up using existing snapshot when running in DAT", true);
			}
			finally
			{
				using (var conn = Db.NewAdminConnection())
				{
					SnapshotCreator.RestoreFromSnapshot(conn, () => Db.Connection.CloseConnection(), Db.DatabaseName, UseSnapshotProtectionAttribute.GetSnapshotName(Db.DatabaseName));
				}
			}
		}

		public void TestMessageWhenCannotRestore()
		{
			using (var connection = Db.NewAdminConnection())
			using (RunWithDatState(false))
			{
				SnapshotCreator.CreateSnapshot(connection, () => Db.Connection.CloseConnection(), snapshotName: UseSnapshotProtectionAttribute.GetSnapshotName(Db.DatabaseName), filename_ext: "");
				var snapshot2 = SnapshotCreator.CreateSnapshot(connection, () => Db.Connection.CloseConnection(), snapshotName: UseSnapshotProtectionAttribute.GetSnapshotName(Db.DatabaseName + "2"), filename_ext: "_2");

				var listener = new SnapshotLeakListener();
				listener.StartAllTests(DateTime.Now);
				try
				{
					listener.AfterEachTest(DateTime.Now);
					Fail("Should throw exception");
				}
				catch (AssertionFailedError ex)
				{
					var expectedMsg = "A database snapshot used for a snapshot protected test could not be restored because there are multiple snapshots on the same database, this may occur as a result of having additional snapshots on the database that is used for testing. Please drop the additional snapshot to allow snapshot protected tests to function properly.";
					AssertEquals("Message should provide details about problem", expectedMsg, Unescape(ex.Message));
				}
				RestoreSafe(snapshot2);
			}
		}

		string Unescape(string escaped)
		{
			// Assertions convert spaces to non-breaking spaces in an attempt to preserve
			return escaped.Replace("&nbsp;", " ");
		}

		void RestoreSafe(IDisposable snapshot)
		{
			try
			{
				snapshot.Dispose();
			}
			catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.DatabaseCannotBeRevertedFromSnapshot)
			{
				// This exception is expected
			}
		}
	}
}
