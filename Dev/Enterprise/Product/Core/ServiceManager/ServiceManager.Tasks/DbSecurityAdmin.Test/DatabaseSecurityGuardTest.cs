using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.SqlServer;
using CargoWise.Data.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ServiceManager.Tasks.DbSecurityAdmin
{
	[UseSnapshotProtection]
	sealed class DatabaseSecurityGuardTest : TestCase
	{
		public void TestCheckAndLockDownOpenDatabaseSecurity()
		{
			// Arrange
			var dbSecurityGuard = new DatabaseSecurityGuard();

			using (var testAdminConnection = Db.NewAdminConnection())
			using (testAdminConnection.BeginTransactionWithManager())
			{
				var dbNames = testAdminConnection.GetDatabases(DatabaseType.Operational);

				var logger = new TestServiceLogger();
				dbSecurityGuard.CheckAndLockDownOpenDatabaseSecurity(testAdminConnection, logger);

				var testSecurity = new DbSecurityLockDown();

				// Act
				var result = new
				{
					dbSecurityLog = testSecurity.LockdownDatabaseLevelSecurity(testAdminConnection),
					serverSecurityLog = testSecurity.LockdownServerLevelSecurity(testAdminConnection),
				};

				// Assert
				CombineAssertions(() =>
				{
					AssertNullOrEmpty("DB security already locked down by dbSecurityGuard.Check => Log count:", result.dbSecurityLog);
					AssertNullOrEmpty("Server security already locked down by dbSecurityGuard.Check => Log count:", result.serverSecurityLog);
				});
			}
		}

		#region MSDB

		public void TestCheckAndLockDownMsdbAccess_Hosted()
		{
			// Arrange
			EnvProxy.SetHostedLocationForTest("SYD");

			using (var connection = Db.NewAdminConnection())
			{
				try
				{
					LoginSyncServiceTask.Testing.Helper.EnsureMsdbDeniedRoleAndUsersDropped(connection);
					Assert($"Before action, we expect the {DbRoleTypes.Constants.CwMsdbAccessDeniedRole} should be nonexistent",
						!SqlSecurityUtils.DbRole.Exists(connection, Db.SqlMsdb, DbRoleTypes.Constants.CwMsdbAccessDeniedRole));

					var logger = new TestServiceLogger();
					var guard = new DatabaseSecurityGuard();
					// Act
					guard.CheckAndLockDownMsdbAccess(connection, logger);

					// Assert
					Assert($"The {DbRoleTypes.Constants.CwMsdbAccessDeniedRole} should be created",
						SqlSecurityUtils.DbRole.Exists(connection, Db.SqlMsdb, DbRoleTypes.Constants.CwMsdbAccessDeniedRole));
					var fullLog = logger.ToString();
					AssertContains("Should check MSDB access", "Check and lock down MSDB access", fullLog, ignoreCase: true);
					AssertContains("Modifications were made to security", fullLog, ignoreCase: true);
				}
				finally
				{
					LoginSyncServiceTask.Testing.Helper.EnsureMsdbDeniedRoleAndUsersDropped(connection);
				}
			}
		}

		public void TestCheckAndLockDownMsdbAccess_NotHosted()
		{
			EnvProxy.SetHostedLocationForTest("NCW");

			using (var connection = Db.NewAdminConnection())
			{
				try
				{
					var logger = new TestServiceLogger();
					var guard = new DatabaseSecurityGuard();
					guard.CheckAndLockDownMsdbAccess(connection, logger);

					AssertEquals("Should skip checking MSDB access", string.Empty, logger.ToString());
				}
				finally
				{
					LoginSyncServiceTask.Testing.Helper.EnsureMsdbDeniedRoleAndUsersDropped(connection);
				}
			}
		}

		public void TestCheckAndLockDownMsdbAccess()
		{
			EnvProxy.SetHostedLocationForTest("SYD");

			var dbName = Db.SqlMsdb;
			var roleName = DbRoleTypes.Constants.CwMsdbAccessDeniedRole;
			var logger = new TestServiceLogger();

			using (var connection = Db.NewAdminConnection())
			{
				try
				{
					new DatabaseSecurityGuard().CheckAndLockDownMsdbAccess(connection, logger);

					SqlSecurityUtils.AssertDbRoleExists(connection, dbName, roleName, expected: true);

					foreach (var login in connection.Logins.Select(dbLogin => dbLogin.LoginName))
					{
						SqlSecurityUtils.AssertDbUserExists(connection, dbName, login, expected: true);
						SqlSecurityUtils.AssertDbRoleContainsDbUser(connection, dbName, roleName, login, expected: true);
						SqlSecurityUtils.AssertLoginSidMatchesDbUserSid(connection, dbName, login, expected: true);
					}
				}
				finally
				{
					LoginSyncServiceTask.Testing.Helper.EnsureMsdbDeniedRoleAndUsersDropped(connection);
				}
			}
		}

		public void TestCheckAndLockDownMsdbAccess_LogCorrectServerNames()
		{
			// Arrange
			var logger = new TestServiceLogger();
			EnvProxy.SetHostedLocationForTest("SYD");

			using (ArrangeAlwaysOn())
			using (var connection = Db.NewAdminConnection())
			{
				var databaseSecurityGuardMock = new Mock<DatabaseSecurityGuard>();
				databaseSecurityGuardMock
					.Protected()
					.Setup<bool>("LockDownMsdbAccessOnServer",
						ItExpr.IsAny<AdminConnection>(),
						ItExpr.IsAny<string>(),
						ItExpr.IsAny<IEnumerable<string>>())
					.Returns(false);

				// Act
				databaseSecurityGuardMock.Object.CheckAndLockDownMsdbAccess(connection, logger);

				// Assert
				var actualLogs = logger.ToString().SplitByLine().Where(l => l.StartsWith("Information|Checking database"));
				AssertSequencesEqual(expected: new[]
				{
					$"Information|Checking database [{Db.SqlMsdb}] on the server [{connection.ServerName}]",
					$"Information|Checking database [{Db.SqlMsdb}] on the server [testReplica1]",
					$"Information|Checking database [{Db.SqlMsdb}] on the server [testReplica2]"
				}, actualLogs);
			}

			IDisposable ArrangeAlwaysOn()
			{
				var isDbPartOfAlwaysOnForTest = AlwaysOn.IsDbPartOfAlwaysOn_ForTest.Value;
				var replicaNamesForTest = AlwaysOn.ReplicaNames_ForTest.Value;

				AlwaysOn.IsDbPartOfAlwaysOn_ForTest.Value = true;
				AlwaysOn.ReplicaNames_ForTest.Value = new List<AlwaysOnReplicaInfo>
				{
					new AlwaysOnReplicaInfo { ReplicaServerName = "testReplica1", AvailabilityMode = 1 },
					new AlwaysOnReplicaInfo { ReplicaServerName = "testReplica2", AvailabilityMode = 1 }
				};

				return new DisposableAction(() =>
				{
					AlwaysOn.IsDbPartOfAlwaysOn_ForTest.Value = isDbPartOfAlwaysOnForTest;
					AlwaysOn.ReplicaNames_ForTest.Value = replicaNamesForTest;
				});
			}
		}

		#endregion // MSDB
	}
}
