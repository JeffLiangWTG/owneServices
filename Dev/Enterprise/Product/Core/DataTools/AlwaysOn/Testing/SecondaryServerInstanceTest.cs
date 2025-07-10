using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.IO;
using Enterprise.AlwaysOn.Setup;
using Moq;
using static System.FormattableString;

namespace Enterprise.AlwaysOn.Testing
{
	class SecondaryServerInstanceTest : AlwaysOnTestFixture
	{
		/// <summary>
		/// Ensure error number constants are valid
		/// </summary>
		public void TestSqlServerErrorNumbersMatch()
		{
			using (var connection = Db.NewExtraConnectionWithMainDbCredentials(Db.ServerName, Db.SqlMasterDb))
			{
				// NonExistentSecondaryDbErrorNumber = 911;
				DbManagerTest.AssertErrorMessageMatches(connection, 911, "Database '%.*ls' does not exist. Make sure that the name is entered correctly.");

				// CannotOpenBackupDeviceErrorNumber = 3201;
				DbManagerTest.AssertErrorMessageMatches(connection, 3201, "Cannot open backup device '%ls'. Operating system error %ls.");

				// SecondaryDbHasInsufficientTransactionLogToBeJoinedErrorNumber = 1478;
				DbManagerTest.AssertErrorMessageMatches(connection, 1478, "The mirror database, \"%.*ls\", has insufficient transaction log data to preserve the log backup chain of the principal database.  This may happen if a log backup from the principal database has not been taken or has not been restored on the mirror database.");

				// ConnectionToPrimaryReplicaIsNotActiveErrorNumber = 35250;
				DbManagerTest.AssertErrorMessageMatches(connection, 35250, "The connection to the primary replica is not active.  The command cannot be processed.");

				// DatabaseDoesNotBelongToGroupDbErrorNumber = 41132;
				DbManagerTest.AssertErrorMessageMatches(connection, 41132, "Cannot join database '%.*ls' to availability group '%.*ls'.  The specified database does not belong to the availability group.  Verify the names of the database and the availability group, and retry the command specifying the correct names.");

				// DatabaseAlreadyJoinedToGroupDbErrorNumber = 41145;
				DbManagerTest.AssertErrorMessageMatches(connection, 41145, "Cannot join database '%.*ls' to availability group '%.*ls'.  The database has already joined the availability group.  This is an informational message.  No user action is required.");
			}
		}

		public void TestGetPreJoinDatabaseStatuses()
		{
			var serverInfo = new SqlServerInfo(Db.ServerName, default);
			var primaryServerMock = new Mock<IPrimaryServerInstance>();
			primaryServerMock.Setup(x => x.ServerInfo).Returns(serverInfo);

			var secondaryServer = DbServerInstanceFactory.ConnectAndValidateSecondaryServer(serverInfo, primaryServerMock.Object);

			var mainDb = Db.DatabaseName;
			var edocDb = Db.DatabaseName + "_SD001";
			var refDb = Db.DatabaseName + "_" + RefDbTableNameResolver.RefDbAffix + "_Ent_AU";
			var userDb = Db.DatabaseName + DbUserRepository.RepositoryDbSuffix;
			var nonExistingDb = "Non-Existing-Db@F27EE076-28A0-4A03-B1C6-0B5A1CD3D161";

			var groupDbList = new Dictionary<string, DbFileAndTransactionLogInfo>();
			groupDbList.Add(mainDb, null);
			groupDbList.Add(edocDb, null);
			groupDbList.Add(refDb, null);
			groupDbList.Add(userDb, null);
			groupDbList.Add(nonExistingDb, null);

			var dbPreJoinStatusList = secondaryServer.GetPreJoinDatabaseStatuses(groupDbList, Guid.NewGuid());

			// Main DB
			var mainDbStatus = dbPreJoinStatusList.First(dbst => dbst.Name == mainDb);
			AssertDbPreJoinStatus(mainDbStatus, mainDb, expectedDbExists: true);
			// eDocs DB
			var edocDbStatus = dbPreJoinStatusList.First(dbst => dbst.Name == edocDb);
			AssertDbPreJoinStatus(edocDbStatus, edocDb, Db.Connection.DatabaseExists(edocDb));
			// Reference DB
			var refDbStatus = dbPreJoinStatusList.First(dbst => dbst.Name == refDb);
			AssertDbPreJoinStatus(refDbStatus, refDb, true);
			// User Repository DB
			var userDbStatus = dbPreJoinStatusList.First(dbst => dbst.Name == userDb);
			AssertDbPreJoinStatus(userDbStatus, userDb, Db.Connection.DatabaseExists(userDb));
			// Non-existing DB
			var nonExistingDbStatus = dbPreJoinStatusList.First(dbst => dbst.Name == nonExistingDb);
			AssertDbPreJoinStatus(nonExistingDbStatus, nonExistingDb, expectedDbExists: false);
		}

		void AssertDbPreJoinStatus(IDatabasePreJoinStatus dbPreJoinStatus, string dbName, bool expectedDbExists)
		{
			AssertEquals("Name", dbName, dbPreJoinStatus.Name);
			AssertEquals("SecondaryExists?", expectedDbExists, dbPreJoinStatus.SecondaryExists);

			// Values which are not set (irrelevant for this test)
			AssertEquals("LastPrimaryBackupLsn", -1m, dbPreJoinStatus.LastPrimaryBackupLsn);
			AssertEquals("SecondaryRedoLsn", -1m, dbPreJoinStatus.SecondaryRedoLsn);
			AssertEquals("IsSecondaryRestoring?", false, dbPreJoinStatus.IsSecondaryRestoring);
			AssertEquals("DbFilesMatch?", false, dbPreJoinStatus.DbFilesMatch);
			AssertEquals("JoinLevel", PreJoinLevel.Cannot_be_joined, dbPreJoinStatus.JoinLevel);
		}

		public void TestEnsureAlwaysOnEndpoint()
		{
			ISecondaryServerInstance secondaryServer = new SecondaryServerInstanceForTesting();
			using (var adminConnection = Db.NewAdminConnection(Db.SqlMasterDb))
			{
				((SecondaryServerInstanceForTesting)secondaryServer).SetEndpointPort(adminConnection);
				var endPointPort = (secondaryServer.AlwaysOnEndpointPort <= 0) ? 5022 : secondaryServer.AlwaysOnEndpointPort;
				RevokeEndpointPermission(adminConnection);

				try
				{
					secondaryServer.EnsureAlwaysOnEndpoint(endPointPort, SecondaryServerInstanceForTesting.MockSqlServiceAccount);
					AssertEndpointCreatedAndPermissionGranted(adminConnection, endPointPort);
				}
				finally
				{
					RevokeEndpointPermission(adminConnection);
				}
			}
		}

		public void TestEnsureDatabaseLogins()
		{
			ISecondaryServerInstance secondaryServer = new SecondaryServerInstanceForTesting();

			var dbLogin1 = new DbLoginInfo("TestLogin_E65445FE-FA12-497A-9945-B78CE54506FE", "S", null, "0xA9CFCA2B1ED14F4B8420E826D3B9ADE4", "0x02005C0C1ED19D13DB24CB33BEF16C77667AC362DFC0B80AB74D4F9A1CAE20B4A9D0B609452AB34E346E53B614F2FC9741D344C0B5D2536D69C5E1009862B8353C2EF6A612EB");
			var dbLogin2 = new DbLoginInfo("TestLogin_F64C8DAE-2397-4122-8EBF-5CDC1F7514FC", "S", Db.DatabaseName, "0x51F8B03634ED2446B7878F31DC7A74C6", "0x02005C0C1ED19D13DB24CB33BEF16C77667AC362DFC0B80AB74D4F9A1CAE20B4A9D0B609452AB34E346E53B614F2FC9741D344C0B5D2536D69C5E1009862B8353C2EF6A612EB");
			var primaryDbLogins = new List<DbLoginInfo>() { dbLogin1, dbLogin2 };

			using (var adminConnection = Db.NewAdminConnection(Db.SqlMasterDb))
			{
				DropTestLogins(adminConnection, primaryDbLogins.Select(l => l.LoginName).ToArray());
				AssertLoginExists(adminConnection, dbLogin1, expectedToExist: false);
				AssertLoginExists(adminConnection, dbLogin2, expectedToExist: false);

				try
				{
					secondaryServer.EnsureDatabaseLogins(primaryDbLogins);
					AssertLoginExists(adminConnection, dbLogin1, expectedToExist: true);
					AssertLoginExists(adminConnection, dbLogin2, expectedToExist: true);
				}
				finally
				{
					DropTestLogins(adminConnection, primaryDbLogins.Select(l => l.LoginName).ToArray());
				}
			}
		}

		[UseSnapshotProtection]
		public void TestRestoreSequenceBackups()
		{
			// Arrange
			var mainServerInfo = new SqlServerInfo(Db.ServerName, default);
			var primaryServerMock = Mock.Of<IPrimaryServerInstance>(
				x => x.ServerInfo == new SqlServerInfo(Db.ServerName, default));
			var secondaryServer = new SecondaryServerInstance(mainServerInfo, primaryServerMock);
			var testDbName = "TestRestoreSequenceBackupsDB";

			using (var tempDir = new TempDirectory())
			using (var connection = Db.NewAdminConnection())
			{
				var firstLogLsn = (decimal)connection.ExecuteScalar($@"
IF NOT EXISTS (SELECT null FROM sys.databases WHERE name = '{testDbName}') CREATE DATABASE [{testDbName}]

ALTER DATABASE [{testDbName}] SET RECOVERY FULL;

BACKUP DATABASE [{testDbName}] TO  DISK = N'{tempDir.DirectoryName}\{testDbName}.bak' WITH NOFORMAT, NOINIT, SKIP, NOREWIND, NOUNLOAD,  STATS = 10;

SELECT
	top(1) first_lsn
FROM
	msdb..backupset
WHERE
	database_name = '{testDbName}'
ORDER BY
	last_lsn DESC;
");

				var expectedResult = CreateLogBackup(connection, tempDir.DirectoryName);

				// Act
				var realResult = secondaryServer.GetLogBackupSequenceToRestore(testDbName, firstLogLsn);

				//Assert
				AssertSequencesEqual("Sequence should be the same", expectedResult, realResult);
			}

			List<string> CreateLogBackup(DbConnection connection, string dirPath)
			{
				var result = new List<string>();
				for (var i = 0; i < 101; i++)
				{
					connection.ExecuteNonQuery($@"
BACKUP LOG [{testDbName}] TO  DISK = N'{dirPath}\{testDbName}{i}.trn' WITH NOFORMAT, NOINIT, SKIP, NOREWIND, NOUNLOAD,  STATS = 10
");
					result.Add($@"{dirPath}\{testDbName}{i}.trn");
				}

				return result;
			}
		}

		void AssertEndpointCreatedAndPermissionGranted(AdminConnection adminConnection, int endPointPort)
		{
			int endpointId;

			using (var reader = adminConnection.Command("SELECT name, port, endpoint_id FROM sys.tcp_endpoints WHERE [type] = 4").ExecuteReader())
			{
				AssertEquals("Was mirroring type endpoint created or was alredy there?", true, reader.Read());
				AssertEquals("Endpoint name", "hadr_endpoint", reader[0].ToString().ToLower());
				AssertEquals("Endpoint port", endPointPort, Convert.ToInt32(reader[1]));
				endpointId = Convert.ToInt32(reader[2]);
			}

			var sqlText = Invariant($@"
SELECT count(*)
FROM sys.server_permissions p
INNER JOIN sys.server_principals l ON l.principal_id = p.grantee_principal_id
WHERE l.name = '{SecondaryServerInstanceForTesting.MockSqlServiceAccount}'
AND p.major_id = {endpointId.ToString()}
AND p.class_desc = 'ENDPOINT'
AND p.[type] = 'CO'
AND p.state = 'G'
");

			AssertEquals("Was connect permission on endpoint granted?", true, Convert.ToInt32(adminConnection.ExecuteScalar(sqlText)) == 1);
		}

		void AssertLoginExists(AdminConnection adminConnection, DbLoginInfo login, bool expectedToExist)
		{
			var sqlText = Invariant($@"
SELECT count(*)
	FROM sys.server_principals
	WHERE name = '{login.LoginName}'
	AND default_database_name = '{(string.IsNullOrWhiteSpace(login.DefaultDatabase) ? DbManager.MasterDbName : login.DefaultDatabase)}'
	AND {((login.Type == DbLoginInfo.LoginType.SQL)
? $"[type] = 'S' AND LOGINPROPERTY(name, 'PasswordHash') = {login.PwdHash} AND [sid] = {login.LoginSid}"
: "[type] in ('U','G') AND LOGINPROPERTY(name, 'PasswordHash') is null")}");

			var loginCount = Convert.ToInt32(adminConnection.ExecuteScalar(sqlText));
			AssertEquals("Login count", (expectedToExist ? 1 : 0), loginCount);
		}

		void DropTestLogins(AdminConnection adminConnection, params string[] loginsToDelete)
		{
			foreach (var login in loginsToDelete)
			{
				AdoTestUtils.DropDbLoginIfExists(adminConnection, login);
			}
		}

		void RevokeEndpointPermission(AdminConnection adminConnection)
		{
			var sqlText = Invariant($@"
IF exists(SELECT null FROM sys.server_principals WHERE name = '{SecondaryServerInstanceForTesting.MockSqlServiceAccount}') AND exists(SELECT null FROM sys.tcp_endpoints WHERE name = 'Hadr_endpoint')
REVOKE CONNECT ON ENDPOINT::[Hadr_endpoint] TO [{SecondaryServerInstanceForTesting.MockSqlServiceAccount}];
");
			adminConnection.ExecuteNonQuery(sqlText);
		}

		class SecondaryServerInstanceForTesting : SecondaryServerInstance
		{
			public SecondaryServerInstanceForTesting()
				: base(MainDbServerInfo, PrimaryServerMock.Object)
			{
				PrimaryServerMock.Setup(x => x.ServerInfo).Returns(MainDbServerInfo);
			}

			public void SetEndpointPort(AdminConnection connection)
			{
				var getDbMirroringEndpointSql = "SELECT ep.port FROM sys.tcp_endpoints ep WHERE ep.[type] = 4";
				var objResult = connection.ExecuteScalar(getDbMirroringEndpointSql);
				alwaysOnEndpointPort = objResult == null ? 0 : Convert.ToInt32(objResult);
			}

			static readonly Mock<IPrimaryServerInstance> PrimaryServerMock = new Mock<IPrimaryServerInstance>();
			static readonly SqlServerInfo MainDbServerInfo = new SqlServerInfo(Db.ServerName, 1433);
			public static readonly string MockSqlServiceAccount = Environment.UserDomainName + "\\" + Environment.UserName;
		}
	}
}
