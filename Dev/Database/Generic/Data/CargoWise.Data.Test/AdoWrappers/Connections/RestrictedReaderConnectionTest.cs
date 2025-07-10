using System;
using CargoWise.Common;
using CargoWise.DataProtection;
using NUnit.Framework;

namespace CargoWise.Data.Test
{
	sealed class RestrictedReaderConnectionTest : TestCase
	{
		public void TestConstructor()
		{
			// Arrange
			// Act
			using (var connection = RestrictedReaderConnection.New("whatever"))
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
			using (var connection = RestrictedReaderConnection.New(Db.ServerName, Db.DatabaseName, "whatever"))
			{
				// Assert
				AssertEquals(Db.ServerName, connection.ServerName);
				AssertEquals(Db.DatabaseName, connection.CurrentDatabase);
				AssertEquals(Db.DatabaseName, ((ICurrentDbControl)connection).InitialDatabase);
				AssertEquals(10, connection.ExecuteScalar<int>("SELECT Id FROM dbo.NewTable"));
			}
		}

		public void TestOnlyReaderPermission()
		{
			// Arrange
			// Act
			using (var connection = RestrictedReaderConnection.New())
			{
				// Assert
				var sqlException = AssertExceptionThrown<SqlException>(() => connection.ExecuteNonQuery("UPDATE dbo.NewTable SET Id = 1000 WHERE Id = 10"));
				AssertEquals(DbErrorType.PermissionDeniedOnObject, new DbErrorMatch(sqlException).ExceptionType);
			}
		}

		public void TestDatabaseUpgradeInProgressExceptionCanBeStillThrown()
		{
			// Arrange
			using (new DisposableAction(
				() => DbLockout.AcquireLockout(adminConnection, LockoutReason.Upgrade),
				() => DbLockout.ResetLockout(adminConnection)))
			using (var connection = RestrictedReaderConnection.New())
			{
				// Act
				// Assert
				AssertExceptionThrown<DatabaseUpgradeInProgressException>(() => connection.EnsureIsOpen());
			}
		}

		public void TestSqlExceptionCanBeHandled_LoginFailedForUser()
		{
			// Arrange
			using (DropRestrictedReaderLoginDisposable())
			using (var connection = RestrictedReaderConnection.New())
			{
				// Act
				// Assert
				AssertEquals(10, connection.ExecuteScalar<int>("SELECT Id FROM dbo.NewTable"));
			}
		}

		IDisposable DropRestrictedReaderLoginDisposable()
		{
			adminConnection.ExecuteNonQuery($"DROP LOGIN {RestrictedReaderLoginCredentials.UserNameFor(Db.DatabaseName)}");
			return new DisposableAction(() => Db.FixReaderLogin(Db.ServerName));
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
