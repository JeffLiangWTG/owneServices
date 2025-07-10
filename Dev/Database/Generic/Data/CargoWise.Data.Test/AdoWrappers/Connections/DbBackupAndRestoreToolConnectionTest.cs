using System;
using CargoWise.Common;
using CargoWise.DataProtection;
using Moq;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	sealed class DbBackupAndRestoreToolConnectionTest : TestCase
	{
		public void TestOdysseyAdminLoginIsUsed()
		{
			// Arrange
			using (var connection = new DbBackupAndRestoreToolConnection(Db.ServerName, Db.DatabaseName, Mock.Of<IErrorReporter>()))
			{
				// Act
				var loginName = connection.ExecuteScalar<string>("SELECT SYSTEM_USER");

				// Assert
				AssertEquals(OdysseyAdminCredentials.AdminUserName, loginName);
			}
		}

		public void TestConnectionCanWork()
		{
			// Arrange
			using (var connection = new DbBackupAndRestoreToolConnection(Db.ServerName, Db.DatabaseName, Mock.Of<IErrorReporter>()))
			{
				connection.BeginTransaction();
				connection.ExecuteScalar($@"
CREATE TABLE dbo.TestTable1 (
	Id INT
)

INSERT INTO dbo.TestTable1 VALUES (10)
");
				// Act
				var actualResult = connection.ExecuteScalar<int>($"SELECT Id FROM dbo.TestTable1");

				// Assert
				AssertEquals(10, actualResult);

				// Cleanup
				connection.RollbackTransaction();
			}
		}

		sealed class ConnectionErrorHandlerTest : TestCase
		{
			public void TestInstance()
			{
				// Arrange
				using (var connection = new DbBackupAndRestoreToolConnection(Db.ServerName, Db.DatabaseName, Mock.Of<IErrorReporter>()))
				{
					// Act
					// Assert
					AssertEquals("NoActionConnectionErrorManager", connection.ConnectionErrorHandler.GetType().Name);
				}
			}

			public void TestHandleDisconnectionAndSecurityErrors()
			{
				// Arrange
				using (var connection = new DbBackupAndRestoreToolConnection(Db.ServerName, Db.DatabaseName, Mock.Of<IErrorReporter>()))
				{
					// Act
					var actualResult = connection.ConnectionErrorHandler.HandleDisconnectionAndSecurityErrors(Mock.Of<Exception>());

					// Assert
					AssertEquals(false, actualResult);
				}
			}

			public void TestReconnectIfApplicable()
			{
				// Arrange
				using (var connection = new DbBackupAndRestoreToolConnection(Db.ServerName, Db.DatabaseName, Mock.Of<IErrorReporter>()))
				{
					// Act
					var actualResult = connection.ConnectionErrorHandler.ReconnectIfApplicable(Mock.Of<Exception>());

					// Assert
					AssertEquals(false, actualResult);
				}
			}
		}
	}
}
