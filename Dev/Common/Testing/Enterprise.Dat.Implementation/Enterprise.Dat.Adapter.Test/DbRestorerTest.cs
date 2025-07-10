using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.DataProtection;
using CargoWise.DataProtection.TestFramework;
using CargoWise.IO;
using Dat.Integration;
using Enterprise.Dat.Implementation;
using Moq;
using NUnit.Framework;

namespace Enterprise.Dat.Adapter.Testing
{
	[SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "This assembly has no reference to enterprise libraries")]
	sealed class DbRestorerTest : TestCase
	{
		public void TestAllLoginsHaveUnderscoreAfterDatabaseName()
		{
			// Arrange
			using (var adminConnection = Db.NewAdminConnection())
			{
				adminConnection.EnsureIsOpen();

				// Act
				var result = adminConnection
					.Logins
					.Select(login => login.LoginName)
					.Where(login => !login.StartsWith($"{adminConnection.CurrentDatabase}_", StringComparison.OrdinalIgnoreCase));

				// Assert
				AssertContainsExactElementsInAnyOrder(Enumerable.Empty<string>(), result);
			}
		}

		public void TestRestoreCurrentDbsDropsExtraLogins()
		{
			// Arrange
			var databaseName = $"Odyssey{nameof(TestRestoreCurrentDbsDropsExtraLogins)}";
			var testLoginName = $"{databaseName}_TestLogin";
			using (var sqlConnection = LocalDBConnection.GetConnection())
			using (new DisposableAction(() => Cleanup(databaseName)))
			{
				sqlConnection.Open();
				var serverName = new SqlConnectionStringBuilder(sqlConnection.ConnectionString).DataSource;

				using (var adminConnection = Db.NewAdminConnection())
				{
					adminConnection.CreateDatabase(databaseName);
				}

				using (var cmd = sqlConnection.CreateCommand())
				{
					cmd.CommandText = $"CREATE LOGIN [{testLoginName}] WITH PASSWORD = '1StrongSecret0^0ButNotReally!!'";
					cmd.ExecuteNonQuery();
				}

				DropDatabases(databaseName);

				var dbRestorer = new DbRestorer(sqlConnection, Mock.Of<ITaskLogger>(), databaseName, CreateDataProtectionTestBed());

				// Act
				dbRestorer.RestoreCurrentDbs();

				// Assert
				using (var cmd = sqlConnection.CreateCommand())
				{
					cmd.CommandText = $"IF exists(SELECT null FROM sys.server_principals WHERE name = '{testLoginName}') SELECT 1 ELSE SELECT 0";
					AssertEquals(0, cmd.ExecuteScalar());
				}
			}
		}

		public void TestConnectionAfterRestoreCanLoginToDatabaseWithRestrictedWriter()
		{
			// Arrange
			var databaseName = $"Odyssey{nameof(TestConnectionAfterRestoreCanLoginToDatabaseWithRestrictedWriter)}";
			var testLoginName = RestrictedWriterLoginCredentials.UserNameFor(databaseName);

			string serverName;

			using (new DisposableAction(() => Cleanup(databaseName)))
			{
				using (var sqlConnection = LocalDBConnection.GetConnection())
				{
					sqlConnection.Open();
					serverName = new SqlConnectionStringBuilder(sqlConnection.ConnectionString).DataSource;
					var taskLogger = Mock.Of<ITaskLogger>();

					using (var cmd = sqlConnection.CreateCommand())
					{
						cmd.CommandText =
							$"CREATE LOGIN [{testLoginName}] WITH PASSWORD = '1StrongSecret0^0ButNotReally!!';" +
							$"ALTER LOGIN [{testLoginName}] DISABLE;";
						cmd.ExecuteNonQuery();
					}

					new DbRestorer(sqlConnection, taskLogger, databaseName, CreateDataProtectionTestBed()).RestoreCurrentDbs();
				}

				// Assert
				using (var connection = Db.NewExtraRestrictedWriterConnection(serverName, databaseName))
				{
					AssertNoExceptionThrown(() => connection.EnsureIsOpen());
				}
			}
		}

		public void TestCycleIdInfoIsCreatedAfterRestore()
		{
			// Arrange
			var databaseName = $"Odyssey{nameof(TestCycleIdInfoIsCreatedAfterRestore)}";
			using (var sqlConnection = LocalDBConnection.GetConnection())
			using (new DisposableAction(() => Cleanup(databaseName)))
			{
				sqlConnection.Open();
				var serverName = new SqlConnectionStringBuilder(sqlConnection.ConnectionString).DataSource;

				var dbRestorer = new DbRestorer(sqlConnection, Mock.Of<ITaskLogger>(), databaseName, CreateDataProtectionTestBed());

				// Act
				dbRestorer.RestoreCurrentDbs();

				// Assert
				var cycleIdFilePath = GetCycleIdFilePath(databaseName);
				AssertEquals(true, File.Exists(cycleIdFilePath));
				AssertEquals(GetCycleIdInfoFromDb(serverName, databaseName), File.ReadAllText(cycleIdFilePath));
			}
		}

		public void TestDifferentDbHasDifferentCycleIdInfo()
		{
			// Arrange
			var databaseName1 = $"Odyssey{nameof(TestDifferentDbHasDifferentCycleIdInfo)}1";
			var databaseName2 = $"Odyssey{nameof(TestDifferentDbHasDifferentCycleIdInfo)}2";

			// Act
			var cycleIdInfo1 = GetCycleIdInfo(databaseName1);
			var cycleIdInfo2 = GetCycleIdInfo(databaseName2);

			// Assert
			AssertNotEquals(cycleIdInfo1.infoFilePath, cycleIdInfo2.infoFilePath);
			AssertNotEquals(cycleIdInfo1.infoInFile, cycleIdInfo2.infoInFile);
			AssertNotEquals(cycleIdInfo1.infoInDb, cycleIdInfo2.infoInDb);

			(string infoFilePath, string infoInFile, string infoInDb) GetCycleIdInfo(string databaseName)
			{
				using (var sqlConnection = LocalDBConnection.GetConnection())
				using (new DisposableAction(() => Cleanup(databaseName)))
				{
					sqlConnection.Open();
					var serverName = new SqlConnectionStringBuilder(sqlConnection.ConnectionString).DataSource;

					var dbRestorer = new DbRestorer(sqlConnection, Mock.Of<ITaskLogger>(), databaseName, CreateDataProtectionTestBed());

					dbRestorer.RestoreCurrentDbs();

					var filePath = GetCycleIdFilePath(databaseName);
					return (filePath, File.ReadAllText(filePath), GetCycleIdInfoFromDb(serverName, databaseName));
				}
			}
		}

		static string GetCycleIdFilePath(string databaseName)
		{
			return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), $"DatDbCycleId-{databaseName}");
		}

		static string GetCycleIdInfoFromDb(string serverName, string databaseName)
		{
			using (var adminConnection = Db.NewAdminConnection(serverName, databaseName))
			{
				return adminConnection.ExecuteScalar<string>($"SELECT CONCAT(CONVERT(char(32), value), '_', Trim(CONVERT(char(20), SERVERPROPERTY('ProductVersion')))) FROM [{databaseName}].sys.extended_properties WHERE name = 'DatDbCycleId'");
			}
		}

		static void DropDatabases(string mainDatabaseName)
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				var databases = new List<string>();
				adminConnection.ExecuteReader(
					"SELECT name FROM sys.databases WHERE name LIKE @databaseName",
					command =>
					{
						command.AddParameter("@databaseName", SqlDbType.NVarChar, 128, $"{mainDatabaseName}%");
					},
					record => databases.Add(record["name"].ToString()));

				foreach (var databaseName in databases)
				{
					var snapshotName = $"DATREF-{databaseName}";
					DropDatabase(adminConnection, snapshotName);
					DropDatabase(adminConnection, databaseName);
				}
			}

			void DropDatabase(AdminConnection adminConnection, string databaseName)
			{
				DbConnectionKiller.KillOtherConnections(adminConnection, databaseName);
				adminConnection.ExecuteNonQuery($"IF EXISTS (SELECT null FROM sys.databases WHERE name = '{databaseName}') DROP DATABASE [{databaseName}]");
			}
		}

		[DeveloperOnlyTest]
		public void TestDropDatabase_DatabaseIsNotOnline_Success()
		{
			// Arrange

			var databaseName = nameof(TestDropDatabase_DatabaseIsNotOnline_Success);
			using var sqlServerService = new ServiceController("MSSQLSERVER");
			using (var tempDir = new TempDirectory())
			using (var connection = LocalDBConnection.GetConnection())
			{
				connection.Open();
				using var cmd = connection.CreateCommand();
				cmd.CommandText = $@"
					CREATE DATABASE [{databaseName}]
					ON PRIMARY (NAME = N'{databaseName}_Data', FILENAME = N'{tempDir.DirectoryName}\{databaseName}_Data.mdf')
					LOG ON 	( NAME = N'{databaseName}_Log', FILENAME = N'{tempDir.DirectoryName}\{databaseName}_Log.ldf')";
				cmd.ExecuteNonQuery();

				sqlServerService.Stop();
				SpinWait.SpinUntil(() => sqlServerService.Status == ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));

				// When we exit the tempDir scope while the sql server service is stopped, the database files will be deleted but the database will remain attached to the sql server with 'PENDING RECOVERY' status. 
			}
			sqlServerService.Start();
			SpinWait.SpinUntil(() => sqlServerService.Status == ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
			using (var connection = LocalDBConnection.GetConnection())
			{
				connection.Open();
				var dbRestorer = new DbRestorer(connection, Mock.Of<ITaskLogger>(), databaseName, CreateDataProtectionTestBed());

				// Act

				AssertNoExceptionThrown(() => dbRestorer.DropDatabase(databaseName));

				// Assert

				using var cmd = connection.CreateCommand();
				cmd.CommandText = $"SELECT COUNT(*) FROM sys.databases WHERE name = '{databaseName}'";
				var count = (int)cmd.ExecuteScalar();
				AssertEquals(0, count);
			}
		}

		static void Cleanup(string databaseName)
		{
			DropDatabases(databaseName);
			DropLogins();
			DeleteCycleIdFile();

			void DeleteCycleIdFile()
			{
				var cycleIdFilePath = GetCycleIdFilePath(databaseName);
				if (File.Exists(cycleIdFilePath))
				{
					File.Delete(cycleIdFilePath);
				}
			}

			void DropLogins()
			{
				using (var adminConnection = Db.NewAdminConnection())
				{
					adminConnection.ExecuteNonQuery(
						$@"
DECLARE @stmt nvarchar(max) = N''

SELECT @stmt = STRING_AGG(CONVERT(nvarchar(max), 'DROP LOGIN [' + name + ']'), CHAR(13) + CHAR(10))
FROM sys.server_principals
WHERE name like @loginName

EXEC (@stmt)
",
						command => command.AddParameter("@loginName", SqlDbType.NVarChar, 128, $"{databaseName}[_]%"));
				}
			}
		}

		static DataProtectionTestBed CreateDataProtectionTestBed()
		{
			var dataProtectionTestBedMock = new Mock<DataProtectionTestBed>("blahblah");
			dataProtectionTestBedMock.Setup(x => x.CleanUpAllProtectedDataRepositories());
			dataProtectionTestBedMock.Setup(x => x.SetupAndActivateTestEnvironmentEnterpriseSecrets(It.IsAny<string>()));

			return dataProtectionTestBedMock.Object;
		}
	}
}
