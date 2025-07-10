using CargoWise.Common;
using CargoWise.Data.Testing;
using NUnit.Framework;

namespace CargoWise.Data.Test
{
	sealed class UnrestrictedWriterConnectionTest : TestCase
	{
		public void TestConstructor()
		{
			// Arrange
			// Act
			using (var connection = UnrestrictedWriterConnection.New("whatever"))
			{
				// Assert
				AssertEquals(Db.ServerName, connection.ServerName);
				AssertEquals(Db.DatabaseName, connection.CurrentDatabase);
				AssertEquals(Db.DatabaseName, ((ICurrentDbControl)connection).InitialDatabase);
				AssertEquals(10, connection.ExecuteScalar<int>("SELECT Id FROM dbo.NewTable"));
			}
		}

		public void TestExtraConstructor()
		{
			// Arrange
			// Act
			using (var connection = UnrestrictedWriterConnection.New(Db.ServerName, Db.DatabaseName, "whatever"))
			{
				// Assert
				AssertEquals(Db.ServerName, connection.ServerName);
				AssertEquals(Db.DatabaseName, connection.CurrentDatabase);
				AssertEquals(Db.DatabaseName, ((ICurrentDbControl)connection).InitialDatabase);
				AssertEquals(10, connection.ExecuteScalar<int>("SELECT Id FROM dbo.NewTable"));
			}
		}

		[UseSnapshotProtection]
		public void TestWriterPermission()
		{
			// Arrange
			// Act
			using (var connection = UnrestrictedWriterConnection.New())
			{
				// Assert
				AssertNoExceptionThrown(() => connection.ExecuteNonQuery("UPDATE dbo.NewTable SET Id = 1000 WHERE Id = 10"));
			}
		}

		public void TestDatabaseUpgradeInProgressExceptionCanBeStillThrown()
		{
			// Arrange
			using (new DisposableAction(
				() => DbLockout.AcquireLockout(adminConnection, LockoutReason.Upgrade),
				() => DbLockout.ResetLockout(adminConnection)))
			using (var connection = UnrestrictedWriterConnection.New())
			{
				// Act
				// Assert
				AssertExceptionThrown<DatabaseUpgradeInProgressException>(() => connection.EnsureIsOpen());
			}
		}

		AdminConnection adminConnection;
		protected override void SetUp()
		{
			base.SetUp();

			adminConnection = Db.NewAdminConnection();
			adminConnection.IgnoreCommitTracker = true;
			adminConnection.ExecuteNonQuery(@"
CREATE TABLE dbo.NewTable (
	Id INT PRIMARY KEY CLUSTERED
);
INSERT INTO dbo.NewTable VALUES (10);
");
		}

		protected override void TearDown()
		{
			adminConnection.ExecuteNonQuery(@"
DROP TABLE IF EXISTS dbo.NewTable;
");
			adminConnection.Dispose();

			base.TearDown();
		}
	}
}
