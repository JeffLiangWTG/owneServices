using System;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	sealed class ExtraConnectionTest : TestCase
	{
		public void TestExtraConnectionWithDifferentLogin_WithSelectPermissionDenied_ThrowsSqlException()
		{
			using (var login = new DbLoginForTest("TestExtraConnectionWithDifferentLogin", ""))
			{
				using (Db.DisableSchemaVersionCheck()) // Login doesn't have permissions to check schema version
				using (var testExtraConnection = Db.NewExtraConnection(Db.ServerName, Db.DatabaseName, login.LoginName, login.Password))
				{
					try
					{
						int testCount = Convert.ToInt32(testExtraConnection.ExecuteScalar("SELECT count(*) from sys.objects"));
						Assert("Should be able query sys.objects", testCount > 0);

						testExtraConnection.ExecuteNonQuery("SELECT * FROM dbo.StmEvent");
						Fail("Should throw exception");
					}
					catch (SqlException e)
					{
						Assert("Wrong Exception caught - " + e.Message,
							e.Message.StartsWith("SELECT permission denied on object 'StmEvent'")
							|| e.Message.StartsWith("The SELECT permission was denied on the object 'StmEvent'"));
					}
				}
			}
		}

		public void TestExtraConnectionWithIntegratedSecurity_WithSelectPermissionAllowed()
		{
			using (var testExtraConnection = Db.NewExtraConnectionIntegratedSecurityEnabled(Db.ServerName, Db.DatabaseName))
			{
				var domain = testExtraConnection.ExecuteScalar("SELECT nt_domain FROM sys.dm_exec_sessions WHERE session_id = @@SPID");
				Assert(!string.IsNullOrEmpty((string)domain));
			}
		}

		public void TestConstructorDoesNotRelyOnPds()
		{
			// Arrange
			using var login = new DbLoginForTest("TestSqlDataProviderFactoryCreatedDoesNotRelyOnPds", "abcd");

			var serverName = Db.ServerName;
			var databaseName = Db.DatabaseName;
			using var clearServerDetails = Db.ClearServerDetailsTemporarily();

			// Act
			// Assert
			AssertNoExceptionThrown(() => Db.NewExtraConnection(serverName, databaseName, login.LoginName, login.Password).Dispose());
		}

		public void TestConnectionCanWork()
		{
			// Arrange
			using var login = new DbLoginForTest("TestSqlDataProviderFactoryCreatedDoesNotRelyOnPds", "abcd");
			login.DropDbLogin();
			login.CreateSysAdminLogin(); // We need to access StmData in DbConnection, use sysadmin login for convenience

			using var extraConnection = Db.NewExtraConnection(Db.ServerName, Db.DatabaseName, login.LoginName, login.Password);

			// Act
			var result = extraConnection.ExecuteScalar<int>("SELECT 1");

			// Assert
			AssertEquals(1, result);
		}
	}
}
