using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.DataProtection;
using CargoWise.DataProtection.Administration.SqlServer;
using CargoWise.DataProtection.TestFramework;
using Enterprise.AlwaysOn.Setup;
using NUnit.Framework;
using static System.FormattableString;
using DbSecurity = Enterprise.AlwaysOn.Setup.DbSecurity;

namespace Enterprise.AlwaysOn.Testing
{
	class DbSecurityTest : AlwaysOnTestFixture
	{
		public const string AlwaysOnTestLoginNameSuffix = "AlwaysOnTestlogin";

		class ChangeEndPointsAuthorizationToLogin : AlwaysOnTestFixture
		{
			public void TestKeepingGrantedPermission()
			{
				var sqlContext = Program.SqlContextManager.GetSqlExecutionContext(Db.ServerName);
				// Arrange
				var permissionBeforeAction = ExecuteQueryList(sqlContext, testEndPointOwnerLogin);

				// Act
				DbSecurity.ChangeEndPointsAuthorizationToLogin(sqlContext, testEndPointOwnerLogin, DbSecurity.SysAdminUserLogin);

				// Assert
				var permissionAfterAction = ExecuteQueryList(sqlContext, DbSecurity.SysAdminUserLogin);
				CombineAssertions(() =>
				{
					permissionBeforeAction.ForEach(x =>
					{
						if (!string.IsNullOrEmpty(x.grantee))
						{
							Assert($"EndPoint owner should be {testEndPointOwnerLogin} before action", x.endpointOwner == testEndPointOwnerLogin);
							Assert($"Permission '{x.state} {x.permission} {x.grantee} {x.endPoint}' should be kept. EndPointOwner and Grantor should be [{DbSecurity.SysAdminUserLogin}]",
								permissionAfterAction.Any(
									y => y.grantee == x.grantee
									&& y.permission == x.permission
									&& y.state == x.state
									&& y.endPoint == x.endPoint
									&& y.endpointOwner == DbSecurity.SysAdminUserLogin
									&& y.grantor == DbSecurity.SysAdminUserLogin));
						}
					});
				});
			}

			List<(string permission, string state, string grantee, string grantor, string endpointOwner, string endPoint)> ExecuteQueryList(ISqlExecutionContext sqlContext, string endpointOwner)
			{
				var queryPermission = $@"
SELECT
	EndPointName = QUOTENAME(ep.name)
  , EndPointOwner = SUSER_NAME(ep.principal_id)
  , Grantor = ISNULL(SUSER_NAME(sp.grantor_principal_id), '')
  , Grantee = ISNULL(SUSER_NAME(sp.grantee_principal_id), '')
  , StateDesc = ISNULL(sp.state_desc, '')
  , PermissionName = ISNULL(sp.permission_name, '')
FROM
	sys.endpoints                    AS ep
	LEFT JOIN sys.server_permissions AS sp ON sp.major_id = ep.endpoint_id
		AND sp.class = 105
WHERE 1=1
	AND ep.principal_id = SUSER_ID(@owner)
ORDER BY
	EndPointName
";
				var result = new List<(string permission, string state, string grantee, string grantor, string endpointOwner, string endPoint)>();
				using var reader = sqlContext.ExecuteReader(
					queryPermission, CommandType.Text,
					cmd => cmd.AddParameter("@owner", DbType.String, endpointOwner)
					);
				{
					while (reader.Read())
					{
						result.Add((
						(string)reader["PermissionName"],
						(string)reader["StateDesc"],
						(string)reader["Grantee"],
						(string)reader["Grantor"],
						(string)reader["EndPointOwner"],
						(string)reader["EndPointName"]));
					}
				}
				return result;
			}

			protected override void SetUp()
			{
				base.SetUp();
				using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
				{
					CreateLogin(connection, testEndPointOwnerLogin);
					foreach (var loginInfo in testLogins)
					{
						CreateLogin(connection, loginInfo.login);
					}

					CreateEndPoints(connection);

					foreach (var endpoint in testEndPoints)
					{
						foreach (var loginInfo in testLogins)
						{
							CreatePermissionOnEndPointToObject(connection, endpoint, loginInfo.permissionState, loginInfo.login);
						}
					}
				}
			}

			protected override void TearDown()
			{
				using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
				{
					DropEndpoints(connection);
					DropLogin(connection, testEndPointOwnerLogin);
					foreach (var loginInfo in testLogins)
					{
						DropLogin(connection, loginInfo.login);
					}
				}
				base.TearDown();
			}

			void CreateEndPoints(AdminConnection connection)
			{
				const int randomPortNumber = 55555;
				for (int i = 0; i < testEndPoints.Count; i++)
				{
					connection.ExecuteNonQuery($@"
CREATE ENDPOINT {testEndPoints[i]} AUTHORIZATION {testEndPointOwnerLogin.QuoteName()}
STATE = STARTED
AS TCP (
    LISTENER_PORT = {randomPortNumber + i}
)
FOR TSQL ();

/*
 Below is given by SQL Server engine:
 Creation of a TSQL endpoint will result in the revocation of any 'Public' connect permissions on the 'TSQL Default TCP' endpoint.
 If 'Public' access is desired on this endpoint, reapply this permission using 'GRANT CONNECT ON ENDPOINT::[TSQL Default TCP] to [public]'.
*/
GRANT CONNECT ON ENDPOINT::[TSQL Default TCP] TO public;
");
				}
			}

			void DropEndpoints(AdminConnection connection)
			{
				foreach (var endPointName in testEndPoints)
				{
					connection.ExecuteNonQuery($@"
IF Exists(SELECT null FROM sys.endpoints WHERE name = '{endPointName}')
BEGIN
	DROP ENDPOINT {endPointName}
END
");
				}
			}

			void CreatePermissionOnEndPointToObject(AdminConnection connection, string endPointName, string permissionState, string grantee)
			{
				var createPermissionSql = $@"
{permissionState} ALTER ON ENDPOINT::{endPointName.QuoteName()} TO {grantee.QuoteName()};
{permissionState} CONNECT ON ENDPOINT::{endPointName.QuoteName()} TO {grantee.QuoteName()};
{permissionState} CONTROL ON ENDPOINT::{endPointName.QuoteName()} TO {grantee.QuoteName()};
{permissionState} TAKE OWNERSHIP ON ENDPOINT::{endPointName.QuoteName()} TO {grantee.QuoteName()};
{permissionState} VIEW DEFINITION ON ENDPOINT::{endPointName.QuoteName()} TO {grantee.QuoteName()}; 
";
				connection.ExecuteNonQuery(createPermissionSql);
			}

			readonly IReadOnlyList<string> testEndPoints = new List<string>() { "TestEndpoint1", "TestEndpoint2" };
			readonly IReadOnlyList<(string login, string permissionState)> testLogins = new List<(string, string)>()
			{
				("TestGrantedPermissionLogin", "Grant"),
				("TestDenyPermissionLogin", "DENY")
			};
			const string testEndPointOwnerLogin = "TestChangeEndPointsAuthorization";
		}

		public void TestChangeAvailabilityGroupsAuthorizationToLogin()
		{
			AssertNoExceptionThrown(() =>
			{
				var sqlContext = Program.SqlContextManager.GetSqlExecutionContext(Db.ServerName);
				DbSecurity.ChangeAvailabilityGroupsAuthorizationToLogin(sqlContext, OdysseyAdminCredentials.AdminUserName, DbSecurity.SysAdminUserLogin);
			});
		}

		internal string GenerateNewLoginSidForTest(ISqlExecutionContext sqlContext)
		{
			var testEnvironmentOA = DataProtectionTestBed.Current.GetExpectedCredentials(TestEnvironmentWellKnownSecret.TestServerOdysseyAdmin);

			var alwaysOnTestLogin = Invariant($"{testEnvironmentOA.UserName}{AlwaysOnTestLoginNameSuffix}");
			DbSecurity.AlterAuthorizationsAndDropLoginIfExists(sqlContext, alwaysOnTestLogin, SysAdminCredentials.SysAdminUserName);

			var sql = Invariant($@"
		DECLARE @loginSid varchar(128)

		IF EXISTS(SELECT null FROM sys.server_principals WHERE[name] = '{alwaysOnTestLogin.QuoteEscapedName('\'')}') DROP LOGIN [alwaysOnTestLogin]
		;

		CREATE LOGIN [{alwaysOnTestLogin}] WITH PASSWORD = '{testEnvironmentOA.Password}'
		;

		SELECT
			@loginSid = sys.fn_varbintohexsubstring(1, sid, 1, 0)
		FROM
			sys.sql_logins
		WHERE
			name = '{alwaysOnTestLogin.QuoteEscapedName('\'')}'
		;

		IF EXISTS(SELECT null FROM sys.server_principals WHERE[name] = '{alwaysOnTestLogin.QuoteEscapedName('\'')}') DROP LOGIN [{alwaysOnTestLogin}]
		;
		SELECT @loginSid
		;
		");
			return (string)sqlContext.ExecuteScalar(sql);
		}

		[UseSnapshotProtection]
		[DeveloperOnlyTest]
		public void TestPropagateOdysseyAdminLogin()
		{
			string tempSid;
			var sqlContext = Program.SqlContextManager.GetSqlExecutionContext(Db.ServerName);

			tempSid = GenerateNewLoginSidForTest(sqlContext);

			Assert(!string.IsNullOrEmpty(tempSid));
			AssertPropagateOdysseyAdminLogin(tempSid);
			AssertPropagateOdysseyAdminLogin(DbSecurity.OdysseyAdminLoginSid);
		}

		class ChangeDatabaseOwnerToLogin : AlwaysOnTestFixture
		{
			public void TestChangeDatabaseOwnerToLoginWillChangeOwner()
			{
				// Arrange
				var database = testDatabase;

				using (var connection = Db.NewAdminConnection(testDatabase))
				using (connection.TemporarySetLockTimeout(DbConnection.LockTimeout.Infinite))
				{
					connection.ExecuteNonQuery($@"
ALTER AUTHORIZATION ON DATABASE::{database.QuoteName()} TO {testOwnerLogin1.QuoteName()};
");

					AssertDatabaseOwnedByLogin(connection, database, testOwnerLogin1);

					var sqlContext = new SqlExecutionContext(((IDbConnectionInternals)connection).ADOConnection, null);

					// Act
					DbSecurity.ChangeDatabaseOwnerToLogin(sqlContext, database, testOwnerLogin2, 30);

					// Assert
					AssertDatabaseOwnedByLogin(connection, database, testOwnerLogin2);
				}
			}

			//			public void TestChangeDatabaseOwnerToLoginWillChangeOwner()
			//			{
			//				// Arrange
			//				var database = testDatabase;

			//				var sqlContext = Program.SqlContextManager.GetSqlExecutionContext(Db.ServerName);
			//				var initialLockTimeout = 0;
			//				try
			//				{
			//					initialLockTimeout = (int)sqlContext.ExecuteScalar($"select @@LOCK_TIMEOUT; SET LOCK_TIMEOUT 999999");

			//					sqlContext.ExecuteNonQuery($@"
			//ALTER AUTHORIZATION ON DATABASE::{database.QuoteName()} TO {testOwnerLogin1.QuoteName()};
			//");

			//					AssertDatabaseOwnedByLogin(sqlContext, database, testOwnerLogin1);

			//					var sqlConnection = ((IDbConnectionInternals)connection).ADOConnection;

			//					// Act
			//					DbSecurity.ChangeDatabaseOwnerToLogin(sqlContext, database, testOwnerLogin2);

			//					// Assert
			//					AssertDatabaseOwnedByLogin(connection, database, testOwnerLogin2);
			//				}
			//				finally
			//				{
			//					sqlContext.ExecuteScalar($"SET LOCK_TIMEOUT {initialLockTimeout}");
			//				}
			//			}

			[SnailTest]
			public void TestChangeDatabaseOwnerToLoginWillNotTimeoutBefore5minWhenOwnerIsChangingInSuspendedTransaction()
			{
				TestChangeDatabaseOwnerToLoginWillNotTimeout(createUser: false, alterAuthorization: true);
			}

			[SnailTest]
			public void TestChangeDatabaseOwnerToLoginWillNotTimeoutBefore5minWhenThereIsLockOnDatabase()
			{
				TestChangeDatabaseOwnerToLoginWillNotTimeout(createUser: true, alterAuthorization: false);
			}

			[SnailTest]
			public void TestChangeDatabaseOwnerToLoginWillTimeOutWhenOwnerIsChaningInSuspendedTransaction()
			{
				// Arrange
				var database = testDatabase;
				Task blockerTask = null;

				using (var connection = Db.NewAdminConnection(database))
				using (connection.TemporarySetLockTimeout(DbConnection.LockTimeout.Infinite))
				{
					try
					{
						connection.ExecuteNonQuery($@"
ALTER AUTHORIZATION ON DATABASE::{database.QuoteName()} TO {testOwnerLogin1};
");
						blockerTask = CreateBlockerTaskToChangeOwnershipAndWaitForItToStart(
							connection,
							testOwnerLogin2, //owner is changing from testOwnerLogin1 to testOwnerLogin2
							createUser: false,
							alterAuthorization: true);

						var sqlContext = Program.SqlContextManager.GetSqlExecutionContext(Db.ServerName);

						// Act
						// Assert
						AssertExceptionThrown(
							"Should not wait for more than 5 mins, but should time out after five minutes",
							typeof(SqlException),
							"Execution Timeout Expired.",
							() =>
							{
								DbSecurity.ChangeDatabaseOwnerToLogin(sqlContext, database, testOwnerLogin2, 10);
							},
							assertStartsWith: true);

						AssertExceptionThrown(
							"Should not wait for more than 5 mins, but should time out after five minutes",
							typeof(SqlException),
							"Execution Timeout Expired.",
							() =>
							{
								DbSecurity.ChangeDatabaseOwnerToLogin(sqlContext, database, testOwnerLogin1, 10);
							},
							assertStartsWith: true);
					}
					finally
					{
						blockerTaskUnblockSignal.Set();
						Task.WaitAll(new[] { blockerTask }, TimeSpan.FromMinutes(10));
					}
				}
			}

			[SnailTest]
			public void TestChangeDatabaseOwnerToLoginWillTimeOutWhenOwnerIsNotCorrectAndThereIsALockOnDatabase()
			{
				// Arrange
				var database = testDatabase;
				Task blockerTask = null;
				//var timeToBlockAfterDbOwnerIsSet = TimeSpan.FromSeconds(400);

				using (var connection = Db.NewAdminConnection(database))
				using (connection.TemporarySetLockTimeout(DbConnection.LockTimeout.Infinite))
				{
					try
					{
						connection.ExecuteNonQuery($@"
ALTER AUTHORIZATION ON DATABASE::{database.QuoteName()} TO {testOwnerLogin1};
");
						blockerTask = CreateBlockerTaskToChangeOwnershipAndWaitForItToStart(
							connection,
							testOwnerLogin2,
							createUser: true, // this will place a lock on a database
							alterAuthorization: false); //there is NO attempt to change owner to the same or different one

						var sqlContext = Program.SqlContextManager.GetSqlExecutionContext(Db.ServerName);

						// Act
						// Assert
						AssertExceptionThrown(
							"Cannot change ownership as there is a lock on database. Should not wait for more than 5 mins, but should time out after five minutes",
							typeof(SqlException),
							"Execution Timeout Expired.",
							() =>
							{
								DbSecurity.ChangeDatabaseOwnerToLogin(sqlContext, database, testOwnerLogin2, 10); //attepmt to change owner
							},
							assertStartsWith: true);
					}
					finally
					{
						blockerTaskUnblockSignal.Set();
						Task.WaitAll(new[] { blockerTask }, TimeSpan.FromMinutes(10));
					}
				}
			}

			[SnailTest]
			public void TestChangeDatabaseOwnerToLoginWillNotThrowTimeOutExceptionIfOwnerIsCorrectAndIsNotChangingInSuspendedTransaction()
			{
				// Arrange
				var database = testDatabase;
				Task blockerTask = null;
				//var timeToBlockAfterDbOwnerIsSet = TimeSpan.FromSeconds(400);

				using (var connection = Db.NewAdminConnection(database))
				using (connection.TemporarySetLockTimeout(DbConnection.LockTimeout.Infinite))
				{
					try
					{
						connection.ExecuteNonQuery($@"
ALTER AUTHORIZATION ON DATABASE::{database.QuoteName()} TO {testOwnerLogin1.QuoteName()};
");

						AssertDatabaseOwnedByLogin(connection, database, testOwnerLogin1);

						blockerTask = CreateBlockerTaskToChangeOwnershipAndWaitForItToStart(
							connection,
							testOwnerLogin1, //setting the same owner hence it should still be able to read from sys.databases
							createUser: false,
							alterAuthorization: true);

						var sqlContext = new SqlExecutionContext(((IDbConnectionInternals)connection).ADOConnection, null);

						// Act
						// Assert
						AssertNoExceptionThrown(
							"No exception should be throw as database is already owned by correct principal.",
							() =>
							{
								DbSecurity.ChangeDatabaseOwnerToLogin(sqlContext, database, testOwnerLogin1, 10); //owner is not changing
							});
					}
					finally
					{
						blockerTaskUnblockSignal.Set();
						Task.WaitAll(new[] { blockerTask }, (int)TimeSpan.FromMinutes(10).TotalMilliseconds);
					}
				}
			}

			#region Setup and helper methods

			const string testOwnerLogin1 = "_Tst_LoginToTestChangeDatabaseOwner1";
			const string testOwnerLogin2 = "_Tst_LoginToTestChangeDatabaseOwner2";
			const string testOwnerLogin3 = "_Tst_LoginToTestChangeDatabaseOwner2";

			const string testDatabase = "_Tst_DatabaseToTestChangeDatabaseOwner";

			protected override void SetUp()
			{
				base.SetUp();

				using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
				{
					connection.CreateDatabase(testDatabase);
					CreateLogin(connection, testOwnerLogin1);
					CreateLogin(connection, testOwnerLogin2);
					CreateLogin(connection, testOwnerLogin3);
				}
			}

			protected override void TearDown()
			{
				using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
				{
					connection.ExecuteNonQuery($"DROP DATABASE IF EXISTS {testDatabase.QuoteName()};");

					DropLogin(connection, testOwnerLogin1);
					DropLogin(connection, testOwnerLogin2);
					DropLogin(connection, testOwnerLogin3);
				}

				base.TearDown();
			}

			void TestChangeDatabaseOwnerToLoginWillNotTimeout(bool createUser, bool alterAuthorization)
			{
				// Arrange
				var database = testDatabase;
				Task blockerTask = null;
				//var timeToBlockAfterDbOwnerIsSet = TimeSpan.FromSeconds(200);

				using (var connection = Db.NewAdminConnection(database))
				using (connection.TemporarySetLockTimeout(DbConnection.LockTimeout.Infinite))
				{
					try
					{
						connection.ExecuteNonQuery($@"
ALTER AUTHORIZATION ON DATABASE::{database.QuoteName()} TO {testOwnerLogin1.QuoteName()};
");
						AssertDatabaseOwnedByLogin(connection, database, testOwnerLogin1);
						blockerTask = CreateBlockerTaskToChangeOwnershipAndWaitForItToStart(
							connection,
							testOwnerLogin2, // will attempt to change the owner if alterAuthorization is true
							createUser,
							alterAuthorization);

						var sqlContext = new SqlExecutionContext(((IDbConnectionInternals)connection).ADOConnection, null);

						// Act
						// Assert
						Task.Delay(10).ContinueWith((t) => blockerTaskUnblockSignal.Set());
						AssertNoExceptionThrown(() =>
						{
							DbSecurity.ChangeDatabaseOwnerToLogin(sqlContext, database, testOwnerLogin3, 300);
						});

						AssertDatabaseOwnedByLogin(connection, database, testOwnerLogin3);
					}
					finally
					{
						blockerTaskUnblockSignal.Set();
						Task.WaitAll(new[] { blockerTask }, TimeSpan.FromMinutes(10));
					}
				}
			}

			readonly AutoResetEvent blockerTaskUnblockSignal = new AutoResetEvent(false);
			Task CreateBlockerTaskToChangeOwnershipAndWaitForItToStart(AdminConnection connection, string ownerLogin, bool createUser, bool alterAuthorization)
			{
				var blockerTaskblockedSignal = new ManualResetEventSlim(false);
				var blockerConnectionSPID = -1;
				var database = connection.CurrentDatabase;
				var blockerTask = Task.Run(() =>
				{
					using (var blockerConnection = Db.NewAdminConnection(database))
					using (blockerConnection.BeginTransactionWithManager())
					using (blockerConnection.TemporarySetDefaultCommandTimeOut(int.MaxValue))
					{
						blockerConnectionSPID = blockerConnection.ExecuteScalar<Int16>("SELECT @@SPID");
						blockerConnection.ExecuteNonQuery(
							$@"
{(alterAuthorization ? $"ALTER AUTHORIZATION ON DATABASE::{testDatabase.QuoteName()} TO {ownerLogin.QuoteName()};" : string.Empty)}
{(createUser ? "CREATE USER Test WITHOUT LOGIN;" : string.Empty)}");
						blockerTaskblockedSignal.Set();
						blockerTaskUnblockSignal.WaitOne();
					}
				});

				blockerTaskblockedSignal.Wait();
				return blockerTask;
			}

			static void AssertDatabaseOwnedByLogin(AdminConnection connection, string databaseName, string ownerName)
			{
				Assert(
	$"Database '{databaseName}' should be owned by '{ownerName}'.",
	connection.Exists(
	$@"
FROM sys.databases AS dbs
	JOIN sys.server_principals AS sp ON dbs.owner_sid = sp.sid
WHERE dbs.name = @dbName AND sp.name = @adminLoginName
",
	cmd =>
	{
		cmd.AddParameter("@adminLoginName", SqlDbType.NVarChar, 128, ownerName);
		cmd.AddParameter("@dbName", SqlDbType.NVarChar, 128, databaseName);
	}));
			}

			#endregion Setup and Helper methods
		}

		#region Helper methods

		static void CreateLogin(AdminConnection connection, string login)
		{
			connection.ExecuteNonQuery(
				$@"
IF NOT EXISTS(SELECT null FROM sys.server_principals WHERE name = @loginName)
	CREATE LOGIN {login.QuoteName()} WITH PASSWORD = '1234', CHECK_POLICY = off
",
				cmd => cmd.AddParameter("@loginName", SqlDbType.NVarChar, 128, login));
		}

		static void DropLogin(AdminConnection connection, string login)
		{
			connection.ExecuteNonQuery($@"
IF Exists(SELECT null FROM sys.server_principals WHERE name = '{login}')
BEGIN
	DROP LOGIN {login.QuoteName()}
END
");
		}

		void AssertPropagateOdysseyAdminLogin(string expectedSid)
		{
			var masterOdysseyAdminLogin = new DbLoginInfo(OdysseyAdminCredentials.AdminUserName, "S", "master", expectedSid, DbSecurity.OdysseyAdminPwdHash, true);

			using (var connection = Db.NewAdminConnection())
			{
				var sqlContext = Program.SqlContextManager.GetSqlExecutionContext(Db.ServerName);
				var serverInfo = new SqlServerInfo(Db.ServerName);
				serverInfo.LoadDetailsFromServer(sqlContext);

				DbSecurity.PropagateOdysseyAdminLogin(serverInfo, masterOdysseyAdminLogin);
				sqlContext = Program.SqlContextManager.GetSqlExecutionContext(Db.ServerName);

				var loginSid = DbSecurity.QueryLoginSid(sqlContext, OdysseyAdminCredentials.AdminUserName);
				Assert(string.Equals(expectedSid, loginSid, StringComparison.OrdinalIgnoreCase));
			}
		}

		#endregion
	}
}
