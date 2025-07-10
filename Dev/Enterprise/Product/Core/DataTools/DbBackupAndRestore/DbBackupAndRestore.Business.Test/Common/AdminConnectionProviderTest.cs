using System;
using System.Data;
using CargoWise.Common;
using CargoWise.Data;
using Moq;
using NUnit.Framework;

namespace Enterprise.DataTools.DbBackupAndRestore.Business.Testing.Common;

class AdminConnectionProviderTest : TestCase
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1065:Encrypt SQL Connection Rule", Justification = "Testing")]
	public void TestApplicationName()
	{
		Test(x => x.GetMainDbConnection());
		Test(x => x.GetAuditDbConnection());
		Test(x => x.GetEdwDbConnection());

		void Test(Func<AdminConnectionProvider, DbConnection> connectionFunc)
		{
			// Arrange
			var serverName = Db.ServerName;
			using var adminConnectionProvider = new AdminConnectionProvider(serverName, serverName, serverName, Mock.Of<IErrorReporter>());
			{
				// Act
				var connection = connectionFunc(adminConnectionProvider);

				// Assert
				AssertApplicationName(connection);
			}
		}

		void AssertApplicationName(DbConnection connection)
		{
			var internalConnection = ((IDbConnectionInternals)connection).InternalDbConnection;
			var builder = new SqlConnectionStringBuilder(internalConnection.ConnectionString);
			AssertEquals("Db Backup And Restore Tool", builder.ApplicationName);
		}
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1065:Encrypt SQL Connection Rule", Justification = "Testing")]
	public void TestConnectionType()
	{
		Test(x => x.GetMainDbConnection());
		Test(x => x.GetAuditDbConnection());
		Test(x => x.GetEdwDbConnection());

		void Test(Func<AdminConnectionProvider, DbConnection> connectionFunc)
		{
			// Arrange
			var serverName = Db.ServerName;
			using var adminConnectionProvider = new AdminConnectionProvider(serverName, serverName, serverName, Mock.Of<IErrorReporter>());
			{
				// Act
				var connection = connectionFunc(adminConnectionProvider);

				// Assert
				AssertType<DbBackupAndRestoreToolConnection>(connection);
			}
		}
	}

	public void TestRetrievesSameConnection()
	{
		// Arrange
		var serverName = Db.ServerName;
		using var adminConnectionProvider = new AdminConnectionProvider(serverName, serverName, serverName, Mock.Of<IErrorReporter>());

		// Act
		var mainDbConnection = adminConnectionProvider.GetMainDbConnection();
		var auditDbConnection = adminConnectionProvider.GetAuditDbConnection();
		var edwDbConnection = adminConnectionProvider.GetEdwDbConnection();

		// Assert
		Assert(ReferenceEquals(mainDbConnection, auditDbConnection));
		Assert(ReferenceEquals(edwDbConnection, auditDbConnection));
	}

	public void TestDisposesOfAllConnection()
	{
		// Arrange
		var serverName = Db.ServerName;
		DbConnection connection;
		using (var adminConnectionProvider = new AdminConnectionProvider(serverName, serverName, serverName, Mock.Of<IErrorReporter>()))
		{
			connection = adminConnectionProvider.GetMainDbConnection();

			// Just run a command to open the connection
			_ = connection.ExecuteNonQuery("SELECT 1");

			// Act: Dispose adminConnectionProvider
		}

		// Assert
		AssertEquals(nameof(ConnectionState), ConnectionState.Closed, connection.State);
	}

	public void TestRetrievesConnectionWithType()
	{
		// Arrange
		using var adminConnectionProvider = new AdminConnectionProvider("server1", "server2", "server3", Mock.Of<IErrorReporter>());

		var mainDbConnection = adminConnectionProvider.GetMainDbConnection();
		var auditDbConnection = adminConnectionProvider.GetAuditDbConnection();
		var edwDbConnection = adminConnectionProvider.GetEdwDbConnection();

		void Test(string dbType, DbConnection expected)
		{
			// Act
			var connection = adminConnectionProvider.GetConnection(dbType);

			// Assert
			Assert(dbType, ReferenceEquals(connection, expected));
		}

		Test(DbFileInfo.DbTypeAuditDB, auditDbConnection);
		Test(DbFileInfo.DbTypeEdwDB, edwDbConnection);
		Test(DbFileInfo.DbTypeMain, mainDbConnection);
		Test("anything-else", mainDbConnection);
	}
}

