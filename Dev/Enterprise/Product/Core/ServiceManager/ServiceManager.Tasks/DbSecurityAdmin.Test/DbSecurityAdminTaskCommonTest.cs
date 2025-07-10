using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.DataProtection;
using CargoWise.DataProtection.TestFramework;
using Enterprise.Scheduler.Business;
using Enterprise.ServiceManager.Tasks.DbSecurityAdmin;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ServiceManager.Tasks.LoginSyncServiceTask.Testing
{
	sealed class DbSecurityAdminTaskCommonTest : TestCase
	{
		public void TestSqlExceptionsAreLoggedAndFaultyDbSkipped()
		{
			var faultyOfflineDb = string.Empty;
			var databasesCreatedForTest = new List<string>();
			using var adminConnection = Db.NewAdminConnection();

			try
			{
				var storageDocs = new[] { 1, 2 }.Select(i =>
				{
					var dbName = $"{Db.DatabaseName}{Db.SDDatabaseAffix}{i:D3}";

					if (!Db.Connection.DatabaseExists(dbName))
					{
						AdoTestUtils.CreateDbIfNotExists(adminConnection, dbName);
						databasesCreatedForTest.Add(dbName);
					}

					return dbName;
				}).ToList();

				var dbNames = adminConnection.GetDatabases(DatabaseType.All & ~DatabaseType.EDW & ~DatabaseType.Audit);
				DataUtils.SetServerConfigOption(adminConnection, "clr enabled", "0");
				DataUtils.SetDbPropertyImmediately(adminConnection, Db.DatabaseName, "is_trustworthy_on = 1", "TRUSTWORTHY OFF");
				foreach (var dbName in dbNames)
				{
					DataUtils.AlterDbAuthorisation(adminConnection, dbName, "sa");
				}

				// Arrange
				faultyOfflineDb = storageDocs[0];
				_ = adminConnection.ExecuteNonQuery($"ALTER DATABASE {faultyOfflineDb.QuoteName()} SET OFFLINE WITH ROLLBACK IMMEDIATE");

				var dsaServiceTask = new DbSecurityAdminTaskForTest();
				dsaServiceTask.ServiceLogger = new TestServiceLogger();

				// Act
				// Assert
				AssertNoExceptionThrown(() => dsaServiceTask.EnsureDatabaseSettings_Exposed(adminConnection));
				AssertContains(
					"EnsureDatabaseSettings errors on one database are logged and ignored",
					$"Database '{faultyOfflineDb}' cannot be opened because it is offline.",
					dsaServiceTask.ServiceLogger.ToString(),
					ignoreCase: true);

				CombineAssertions(() =>
				{
					AssertEquals("clr enabled for main server", true, Convert.ToBoolean(Db.Connection.ExecuteScalar("SELECT value_in_use FROM sys.configurations WHERE name = 'clr enabled'")));
					AssertEquals("is_trustworthy_on for main db", true, Convert.ToBoolean(GetDbPropertyValue(Db.Connection, "is_trustworthy_on", Db.DatabaseName)));

					foreach (var dbName in dbNames.Except(faultyOfflineDb))
					{
						var authResult = Convert.ToString(Db.Connection.ExecuteScalar($"SELECT TOP 1 suser_sname(owner_sid) FROM sys.databases WHERE name = '{dbName}'"));
						AssertEquals($"AUTHORIZATION for db: {dbName} has been set to: {OdysseyAdminCredentials.AdminUserName}", OdysseyAdminCredentials.AdminUserName, authResult);
					}
				});
			}
			finally
			{
				DataUtils.SetServerConfigOption(adminConnection, "clr enabled", "1");
				DataUtils.SetDbPropertyImmediately(adminConnection, Db.DatabaseName, "is_trustworthy_on = 0", "TRUSTWORTHY ON");

				if (!string.IsNullOrEmpty(faultyOfflineDb))
				{
					_ = adminConnection.ExecuteNonQuery($"ALTER DATABASE {faultyOfflineDb.QuoteName()} SET ONLINE");
				}

				databasesCreatedForTest.ForEach(x => AdoTestUtils.DropDbIfExists(adminConnection, x));
			}
		}

		public void TestAlwaysRunAtStartupIsTrue()
		{
			var serviceAttribute = AssemblyMetaDataReader.GetAttributes<HostedServiceAttribute>().Single(a => a.Code == ScheduleTypeConstants.DbSecurityAdminCode);
			AssertEquals(true, serviceAttribute.AlwaysRunAtStartup);
		}

		void SetupEnsureDbConnectionTest(AdminConnection connection, IEnumerable<string> dbNames)
		{
			DataUtils.SetServerConfigOption(connection, "clr enabled", "0");
			DataUtils.SetDbPropertyImmediately(connection, Db.DatabaseName, "is_trustworthy_on = 1", "TRUSTWORTHY OFF");
			DataUtils.SetDbPropertyImmediately(connection, Db.AuditDatabaseName, "is_trustworthy_on = 1", "TRUSTWORTHY OFF");
			DataUtils.SetDbPropertyImmediately(connection, Db.EdwDatabaseName, "is_trustworthy_on = 1", "TRUSTWORTHY OFF");

			foreach (var dbName in dbNames)
			{
				DataUtils.AlterDbAuthorisation(connection, dbName, "sa");
			}

			AssertDatabaseSettings(
				dbNames,
				mainDbEnsureSettings: false,
				biDbEnsureSettings: false,
				authorisationExpected: "sa"
			);
		}

		void AssertDatabaseSettings(IEnumerable<string> dbNames,
			bool mainDbEnsureSettings, bool biDbEnsureSettings, bool biDbsExist = true,
			string authorisationExpected = "sa")
		{
			CombineAssertions(() =>
			{
				AssertEquals("clr enabled for main server", mainDbEnsureSettings, Convert.ToBoolean(Db.Connection.ExecuteScalar("select value_in_use from sys.configurations where name = 'clr enabled'")));
				AssertEquals("is_trustworthy_on for main db", mainDbEnsureSettings, Convert.ToBoolean(GetDbPropertyValue(Db.Connection, "is_trustworthy_on", Db.DatabaseName)));
				AssertEquals("is_trustworthy_on for audit db", biDbEnsureSettings, Convert.ToBoolean(GetDbPropertyValue(Db.Connection, "is_trustworthy_on", Db.AuditDatabaseName)));
				AssertEquals("is_trustworthy_on for edw db", biDbEnsureSettings, Convert.ToBoolean(GetDbPropertyValue(Db.Connection, "is_trustworthy_on", Db.EdwDatabaseName)));

				foreach (var dbName in dbNames)
				{
					var authResult = Convert.ToString(Db.Connection.ExecuteScalar("select TOP 1 suser_sname(owner_sid) from sys.databases where name = '" + dbName + "'"));
					if (IsBiDb(dbName))
					{
						if (biDbsExist)
						{
							AssertEquals($"AUTHORIZATION for DB {dbName} should not change in EnsureDatabaseSettings", "sa", authResult);
						}
						else
						{
							AssertEquals($"AUTHORIZATION for DB {dbName} should be empty if the DB doesn't exist", string.Empty, authResult);
						}
					}
					else
					{
						AssertEquals($"AUTHORIZATION for DB {dbName}", authorisationExpected, authResult);
					}
				}
			});
		}

		bool IsBiDb(string dbName)
		{
			return dbName == Db.AuditDatabaseName || dbName == Db.EdwDatabaseName;
		}

		[UseSnapshotProtection]
		public void TestEnsureDatabaseSettings()
		{
			using (var connection = Db.NewAdminConnection())
			using (SnapshotCreator.CreateSnapshot(connection, () => Db.Connection.CloseConnection(), Db.AuditDatabaseName, Db.AuditDatabaseName + "-SS"))
			using (SnapshotCreator.CreateSnapshot(connection, () => Db.Connection.CloseConnection(), Db.EdwDatabaseName, Db.EdwDatabaseName + "-SS"))
			{
				try
				{
					var dbNames = connection.GetDatabases(DatabaseType.All);
					SetupEnsureDbConnectionTest(connection, dbNames);
					var taskForTest = new DbSecurityAdminTaskForTest();
					taskForTest.EnsureDatabaseSettings_Exposed(connection);

					var expectedAdminLogin = DataProtectionTestBed.Current.GetExpectedCredentials(TestEnvironmentWellKnownSecret.TestServerOdysseyAdmin);

					AssertDatabaseSettings(
						dbNames,
						mainDbEnsureSettings: true,
						biDbEnsureSettings: true,
						authorisationExpected: expectedAdminLogin.UserName
					);
				}
				finally
				{
					DataUtils.SetServerConfigOption(connection, "clr enabled", "1");

					DataUtils.SetDbPropertyImmediately(connection, Db.DatabaseName, "is_trustworthy_on = 0", "TRUSTWORTHY ON");
					DataUtils.SetDbPropertyImmediately(connection, Db.AuditDatabaseName, "is_trustworthy_on = 0", "TRUSTWORTHY ON");
					DataUtils.SetDbPropertyImmediately(connection, Db.EdwDatabaseName, "is_trustworthy_on = 0", "TRUSTWORTHY ON");
				}
			}
		}

		[UseSnapshotProtection]
		public void TestEnsureDatabaseSettings_BiServerNotSetInRegistry()
		{
			using (var connection = Db.NewAdminConnection())
			using (SnapshotCreator.CreateSnapshot(connection, () => Db.Connection.CloseConnection(), Db.AuditDatabaseName, Db.AuditDatabaseName + "-SS"))
			using (SnapshotCreator.CreateSnapshot(connection, () => Db.Connection.CloseConnection(), Db.EdwDatabaseName, Db.EdwDatabaseName + "-SS"))
			{
				try
				{
					var dbNames = connection.GetDatabases(DatabaseType.All);
					SetupEnsureDbConnectionTest(connection, dbNames);
					var taskForTest = new DbSecurityAdminTaskForTest();

					// When BI DBs don't exist, the result of this method will be string.Empty
					taskForTest.SetBiServerEmptyForTest = true;
					taskForTest.EnsureDatabaseSettings_Exposed(connection);

					var expectedAdminLogin = DataProtectionTestBed.Current.GetExpectedCredentials(TestEnvironmentWellKnownSecret.TestServerOdysseyAdmin);

					AssertDatabaseSettings(
						dbNames,
						mainDbEnsureSettings: true,
						biDbEnsureSettings: false, // BI DBs remain not trustworthy because the servername is not set in registry
						authorisationExpected: expectedAdminLogin.UserName
					);
				}
				finally
				{
					DataUtils.SetServerConfigOption(connection, "clr enabled", "1");

					DataUtils.SetDbPropertyImmediately(connection, Db.DatabaseName, "is_trustworthy_on = 0", "TRUSTWORTHY ON");
					DataUtils.SetDbPropertyImmediately(connection, Db.AuditDatabaseName, "is_trustworthy_on = 0", "TRUSTWORTHY ON");
					DataUtils.SetDbPropertyImmediately(connection, Db.EdwDatabaseName, "is_trustworthy_on = 0", "TRUSTWORTHY ON");
				}
			}
		}

		[UseSnapshotProtection]
		public void TestEnsureDatabaseSettings_BiDbDoesNotExist()
		{
			using (var connection = Db.NewAdminConnection())
			using (SnapshotCreator.CreateSnapshot(connection, () => Db.Connection.CloseConnection(), Db.AuditDatabaseName, Db.AuditDatabaseName + "-SS"))
			using (SnapshotCreator.CreateSnapshot(connection, () => Db.Connection.CloseConnection(), Db.EdwDatabaseName, Db.EdwDatabaseName + "-SS"))
			{
				try
				{
					var dbNames = connection.GetDatabases(DatabaseType.All);
					SetupEnsureDbConnectionTest(connection, dbNames);
					var taskForTest = new DbSecurityAdminTaskForTest();
					RenameDbIfExists(connection, Db.AuditDatabaseName, "bingbong_Audit");
					RenameDbIfExists(connection, Db.EdwDatabaseName, "bingbong_EDW");
					taskForTest.EnsureDatabaseSettings_Exposed(connection);

					var expectedAdminLogin = DataProtectionTestBed.Current.GetExpectedCredentials(TestEnvironmentWellKnownSecret.TestServerOdysseyAdmin);

					AssertDatabaseSettings(
						dbNames,
						mainDbEnsureSettings: true,
						biDbEnsureSettings: false, // BI DBs remain false because they are not altered since they are not able to be connected to
						biDbsExist: false,
						authorisationExpected: expectedAdminLogin.UserName
					);
				}
				finally
				{
					DataUtils.SetServerConfigOption(connection, "clr enabled", "1");

					DataUtils.SetDbPropertyImmediately(connection, Db.DatabaseName, "is_trustworthy_on = 0", "TRUSTWORTHY ON");
					RenameDbIfExists(connection, "bingbong_Audit", Db.AuditDatabaseName);
					RenameDbIfExists(connection, "bingbong_EDW", Db.EdwDatabaseName);
				}
			}
		}

		void RenameDbIfExists(AdminConnection connection, string dbName, string newName)
		{
			var sqlText = @$"
				IF EXISTS (SELECT 1 FROM sys.databases WHERE name = '{dbName}')
				BEGIN
					ALTER DATABASE {dbName} MODIFY NAME = {newName};
				END";
			connection.ExecuteNonQuery(sqlText);
		}

		[UseSnapshotProtection]
		public void TestCheckAndLockDownMsdbAccess()
		{
			EnvProxy.SetHostedLocationForTest("SYD");

			try
			{
				var adminTask = new DbSecurityAdminTask();
				adminTask.ServiceLogger = new TestServiceLogger();

				adminTask.RunTask(CancellationToken.None);

				AssertContains("Should check MSDB access", "Check and lock down MSDB access", adminTask.ServiceLogger.ToString(), ignoreCase: true);
			}
			finally
			{
				using (var connection = Db.NewAdminConnection())
				{
					Helper.EnsureMsdbDeniedRoleAndUsersDropped(connection);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestAccessToMsdb_Hosted()
		{
			EnvProxy.SetHostedLocationForTest("SYD");

			try
			{
				var adminTask = new DbSecurityAdminTask();
				adminTask.ServiceLogger = new TestServiceLogger();

				adminTask.RunTask(CancellationToken.None);

				using (var connection = Db.NewAdminConnection())
				{
					foreach (var login in connection.Logins.Select(dbLogin => dbLogin.LoginName))
					{
						var ex = AssertExceptionThrown<SqlException>(() =>
						{
							AccessMsdb(connection, login);
						});
						AssertEquals("Access denied exception must be thrown", DbErrorType.PermissionDeniedOnObject, new DbErrorMatch(ex).ExceptionType);
					}
				}
			}
			finally
			{
				using (var connection = Db.NewAdminConnection())
				{
					Helper.EnsureMsdbDeniedRoleAndUsersDropped(connection);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestAccessToMsdb_NotHosted()
		{
			// Arrange
			EnvProxy.SetHostedLocationForTest("NCW");
			try
			{
				using (var connection = Db.NewAdminConnection())
				{
					Helper.EnsureMsdbDeniedRoleAndUsersDropped(connection);
					Assert($"Before action, we expect the {DbRoleTypes.Constants.CwMsdbAccessDeniedRole} should be nonexistent",
					!SqlSecurityUtils.DbRole.Exists(connection, Db.SqlMsdb, DbRoleTypes.Constants.CwMsdbAccessDeniedRole));
				}

				var adminTask = new DbSecurityAdminTask();
				adminTask.ServiceLogger = new TestServiceLogger();

				// Act
				adminTask.RunTask(CancellationToken.None);

				// Assert
				using (var connection = Db.NewAdminConnection())
				{
					Assert($"The {DbRoleTypes.Constants.CwMsdbAccessDeniedRole} should not be created",
						!SqlSecurityUtils.DbRole.Exists(connection, Db.SqlMsdb, DbRoleTypes.Constants.CwMsdbAccessDeniedRole));
					foreach (var login in connection.Logins.Select(dbLogin => dbLogin.LoginName))
					{
						AssertNoExceptionThrown(() =>
						{
							AccessMsdb(connection, login);
						});
					}
				}
			}
			finally
			{
				using (var connection = Db.NewAdminConnection())
				{
					Helper.EnsureMsdbDeniedRoleAndUsersDropped(connection);
				}
			}
		}

		#region Implementation

		object GetDbPropertyValue(DbConnection connection, string ptyName, string dbName)
		{
			var sqlText = String.Format("SELECT [{0}] FROM sys.databases WHERE name = '{1}'", ptyName, dbName);
			return connection.ExecuteScalar(sqlText);
		}

		void AccessMsdb(AdminConnection connection, string login)
		{
			connection.ExecuteNonQuery($@"
EXECUTE AS LOGIN = {login.QuoteName('\'')}

BEGIN TRY
	SELECT RowExists = IIF(EXISTS(SELECT NULL FROM msdb.dbo.backupset), 1, 0)
END TRY BEGIN CATCH
	REVERT
	;THROW
END CATCH

REVERT

");
		}

		class DbSecurityAdminTaskForTest : DbSecurityAdminTask
		{
			public void EnsureDatabaseSettings_Exposed(AdminConnection connection)
			{
				base.EnsureDatabaseSettings(connection, CancellationToken.None);
			}

			public bool SetBiServerEmptyForTest;
			public override string GetBiServerName(AdminConnection connection, string dbName)
			{
				if (SetBiServerEmptyForTest)
				{
					return string.Empty;
				}
				else
				{
					return base.GetBiServerName(connection, dbName);
				}
			}
		}

		#endregion // Implementation
	}
}
