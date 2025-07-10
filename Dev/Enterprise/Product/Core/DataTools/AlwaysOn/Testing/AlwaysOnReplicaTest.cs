using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using CargoWise.ActiveDirectory.TestFramework;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.DataProtection.Administration.SqlServer;
using CargoWise.DataProtection.TestFramework;
using Enterprise.AlwaysOn.Setup;
using Moq;
using Moq.Protected;
using static System.FormattableString;

namespace Enterprise.AlwaysOn.Testing
{
	class AlwaysOnReplicaTest : AlwaysOnTestFixture
	{
		readonly SqlServerInfo defaultServerInfo = new SqlServerInfo("SomeNode\\SomeInstance", "SomeNode", "SomeInstance", "SomeDomain.Net", "SomeNode.SomeDomain.Net", default);

		public void TestAttributes()
		{
			var groupId = Guid.NewGuid();
			var alwaysOnDb = AlwaysOnDatabaseFactory.New("SomeDb", "SomeGroup", groupId);
			var replicaOptions = ReplicaOptionsTest.NewReplicaOptionsForTesting(AvailabilityMode.ASYNCHRONOUS_COMMIT, FailoverMode.MANUAL);
			IAvailabilityGroup group = new AvailabilityGroup(defaultServerInfo, alwaysOnDb);
			var replicaId = Guid.NewGuid();
			var replica = AlwaysOnReplicaFactory.New(ReplicaRole.Primary, SyncronisationHealth.HEALTHY, JoinState.JOINED_STANDALONE, replicaOptions, replicaId, group, defaultServerInfo, "TCP://10.10.10.100:5024");

			AssertEquals("Role", ReplicaRole.Primary, replica.Role);
			AssertEquals("Health", SyncronisationHealth.HEALTHY, replica.Health);
			AssertEquals("Join State", JoinState.JOINED_STANDALONE, replica.JoinState);
			AssertEquals("CommitMode", AvailabilityMode.ASYNCHRONOUS_COMMIT, replica.Options.CommitMode);
			AssertEquals("Failover", FailoverMode.MANUAL, replica.Options.Failover);
			AssertEquals("SecondaryRole", AllowConnection.ALL, replica.Options.SecondaryAllowConnection);
			AssertEquals("SecondaryReadOnlyRoutingUrl", "", replica.Options.SecondaryReadOnlyRoutingUrl);
			AssertEquals("ServerAddressFromEndpointUrl", "10.10.10.100", replica.ServerAddressFromEndpointUrl);
			AssertEquals("GroupId", groupId, replica.ParentGroup.GroupId);
			AssertEquals("ReplicaId", replicaId, replica.ReplicaId);

			AssertEquals("IsLoaded?", true, replica.IsLoaded);
			AssertEquals("HasErrors?", false, replica.HasErrors);
			AssertEquals("LastErrorMessage", null, replica.LastErrorMessage);
		}

		public void TestGetGroupDatabaseLogins()
		{
			var serverInfo = new SqlServerInfo(Db.ServerName);
			var windowsUsername = TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000;
			var result = CreatePrincipalForWindows(windowsUsername);
			try
			{
				IReplicaOptions replicaOptions = ReplicaOptions.NewFromRawData(0, 1, 1, "TestUrl");
				IAvailabilityGroup group = new AvailabilityGroupForTesting();
				var replica = AlwaysOnReplicaFactory.New(0, 0, 0, replicaOptions, Guid.NewGuid(), group, serverInfo, "");
				var dbLogins = replica.GetGroupDatabaseLogins();

				var rwCredentials = DataProtectionTestBed.Current.GetExpectedCredentials(TestEnvironmentWellKnownSecret.OdysseyDatCargoWiseWriterLogin);
				var appLoginName = rwCredentials.UserName;
				var appDbLoginSelection = dbLogins.Where(l => l.LoginName == appLoginName);
				AssertEquals("Application login found", 1, appDbLoginSelection.Count());

				var appDbLogin = appDbLoginSelection.First();
				AssertEquals("LoginName", appLoginName, appDbLogin.LoginName);
				AssertEquals("Type", DbLoginInfo.LoginType.SQL, appDbLogin.Type);

				var loginSid = GetSid(windowsUsername, isLogin: true);

				var windowsLoginExist = dbLogins.Any(x => x.LoginSid.Equals(loginSid, StringComparison.OrdinalIgnoreCase) && x.Type == DbLoginInfo.LoginType.WINDOWS);
				if (!windowsLoginExist)
				{
					var msgLogins = string.Join(Environment.NewLine, dbLogins.OrderBy(l => l.LoginName).Select(l => $"[{l.LoginName}] ({l.Type})"));
					var dbUserSid = GetSid(windowsUsername, isLogin: false);

					var message = string.Join(Environment.NewLine, new[]
					{
						$"Cannot find windows login [{windowsUsername}] in all db Logins by comparing Sid:",
						msgLogins,
						"",
						$"Login__SID: {loginSid}",
						$"DbUser_SID: {dbUserSid}",
					});

					Fail(message);
				}
			}
			finally
			{
				using (var connection = Db.NewAdminConnection(Db.DatabaseName))
				{
					if (result.loginCreated)
					{
						connection.ExecuteNonQuery(Invariant($"DROP Login [{windowsUsername}]"));
					}

					if (result.userCreated)
					{
						connection.ExecuteNonQuery(Invariant($"DROP User [{windowsUsername}]"));
					}
				}
			}
		}

		(bool userCreated, bool loginCreated) CreatePrincipalForWindows(string username)
		{
			var userCreated = false;
			var loginCreated = false;
			using (var connection = Db.NewAdminConnection(Db.DatabaseName))
			{
				if (!connection.Exists(Invariant($"from sys.server_principals Where name = '{username}'")))
				{
					connection.ExecuteNonQuery(Invariant($"Create Login [{username}] From Windows"));
					loginCreated = true;
				}

				if (!connection.Exists(Invariant($"from sys.database_principals Where name = '{username}'")))
				{
					connection.ExecuteNonQuery(Invariant($"Create user [{username}] for login [{username}]"));
					userCreated = true;
				}
			}

			return (userCreated, loginCreated);
		}

		string GetSid(string principal, bool isLogin)
		{
			if (isLogin)
			{
				using (var connection = Db.NewAdminConnection())
				{
					return connection.ExecuteScalar<string>($"SELECT sid = ISNULL(sys.fn_varbintohexsubstring(1, MIN(sid), 1, 0), '0x') FROM sys.server_principals WHERE name = N{principal.QuoteName('\'')}");
				}
			}

			return Db.Connection.ExecuteScalar<string>($"SELECT sid = ISNULL(sys.fn_varbintohexsubstring(1, MIN(sid), 1, 0), '0x') FROM sys.database_principals WHERE name = N{principal.QuoteName('\'')}");
		}

		public void TestPerfomPlannedManualFailover()
		{
			AssertPerfomPlannedManualFailover(AvailabilityMode.SYNCHRONOUS_COMMIT, AvailabilityMode.SYNCHRONOUS_COMMIT, SyncronisationHealth.HEALTHY, null);
			AssertPerfomPlannedManualFailover(AvailabilityMode.SYNCHRONOUS_COMMIT, AvailabilityMode.SYNCHRONOUS_COMMIT, SyncronisationHealth.PARTIALLY_HEALTHY, "A planned manual failover is supported only if the secondary replica is in a healthy state.");
			AssertPerfomPlannedManualFailover(AvailabilityMode.SYNCHRONOUS_COMMIT, AvailabilityMode.SYNCHRONOUS_COMMIT, SyncronisationHealth.NOT_HEALTHY, "A planned manual failover is supported only if the secondary replica is in a healthy state.");

			AssertPerfomPlannedManualFailover(AvailabilityMode.SYNCHRONOUS_COMMIT, AvailabilityMode.ASYNCHRONOUS_COMMIT, SyncronisationHealth.NOT_HEALTHY, "A planned manual failover is supported only if both the primary replica and secondary replica are configured for synchronous-commit mode.");
			AssertPerfomPlannedManualFailover(AvailabilityMode.SYNCHRONOUS_COMMIT, AvailabilityMode.ASYNCHRONOUS_COMMIT, SyncronisationHealth.PARTIALLY_HEALTHY, "A planned manual failover is supported only if both the primary replica and secondary replica are configured for synchronous-commit mode.");
			AssertPerfomPlannedManualFailover(AvailabilityMode.SYNCHRONOUS_COMMIT, AvailabilityMode.ASYNCHRONOUS_COMMIT, SyncronisationHealth.HEALTHY, "A planned manual failover is supported only if both the primary replica and secondary replica are configured for synchronous-commit mode.");
			AssertPerfomPlannedManualFailover(AvailabilityMode.ASYNCHRONOUS_COMMIT, AvailabilityMode.SYNCHRONOUS_COMMIT, SyncronisationHealth.NOT_HEALTHY, "A planned manual failover is supported only if both the primary replica and secondary replica are configured for synchronous-commit mode.");
			AssertPerfomPlannedManualFailover(AvailabilityMode.ASYNCHRONOUS_COMMIT, AvailabilityMode.SYNCHRONOUS_COMMIT, SyncronisationHealth.PARTIALLY_HEALTHY, "A planned manual failover is supported only if both the primary replica and secondary replica are configured for synchronous-commit mode.");
			AssertPerfomPlannedManualFailover(AvailabilityMode.ASYNCHRONOUS_COMMIT, AvailabilityMode.SYNCHRONOUS_COMMIT, SyncronisationHealth.HEALTHY, "A planned manual failover is supported only if both the primary replica and secondary replica are configured for synchronous-commit mode.");
			AssertPerfomPlannedManualFailover(AvailabilityMode.ASYNCHRONOUS_COMMIT, AvailabilityMode.ASYNCHRONOUS_COMMIT, SyncronisationHealth.HEALTHY, "A planned manual failover is supported only if both the primary replica and secondary replica are configured for synchronous-commit mode.");
		}

		public void TestCheckOdysseyAdminIsDbOwnerDoesNotThrow()
		{
			// Arrange
			var replicaOption = ReplicaOptionsTest.NewReplicaOptionsForTesting(AvailabilityMode.ASYNCHRONOUS_COMMIT, FailoverMode.MANUAL);
			var replica = new AlwaysOnReplica(ReplicaRole.Primary, SyncronisationHealth.HEALTHY, JoinState.JOINED_STANDALONE, replicaOption, Guid.NewGuid(), Mock.Of<IAvailabilityGroup>(), defaultServerInfo, "TCP://10.10.10.100:5024");
			Assert(!((IValidationStatus)replica).HasErrors);

			// Act
			// Assert
			AssertNoExceptionThrown("CheckOdysseyAdminIsDbOwner should throw no exceptions even when replica is not properly setup.", () => replica.CheckOdysseyAdminIsDbOwner(new List<string> { "RandomName" }));
			Assert("Validation status should indicate an error if CheckOdysseyAdminIsDbOwner failed.", ((IValidationStatus)replica).HasErrors);
		}

		public void TestCheckDatabaseIsOnlineWithWait()
		{
			var group = new AvailabilityGroupForTesting();
			var replicaOption = ReplicaOptionsTest.NewReplicaOptionsForTesting(AvailabilityMode.ASYNCHRONOUS_COMMIT, FailoverMode.MANUAL);
			var timer = new Stopwatch();

			var serverInfo = new SqlServerInfo("ReplicaServer", "SomeNode", "SomeInstance", "SomeDomain", "SomeNode.SomeDomain", default);
			var replica = new AlwaysOnReplicaForTesting(ReplicaRole.Secondary, SyncronisationHealth.HEALTHY, JoinState.JOINED_STANDALONE, replicaOption, Guid.NewGuid(), group, serverInfo, "TCP://10.10.10.100:5024");
			replica.SleepTimeInSeconds = TimeSpan.FromSeconds(2);

			var sqlContext = Program.SqlContextManager.GetSqlExecutionContext(Db.ServerName, Db.SqlMasterDb);
			timer.Start();
			replica.SleepTimeInSeconds = TimeSpan.FromSeconds(2);
			replica.CheckDatabaseIsOnlineWithWait_Exposed(sqlContext);
			AssertEquals(Invariant($"Check should take at least {replica.SleepTimeInSeconds.TotalSeconds} seconds, no errors expected"), true, timer.Elapsed.TotalSeconds >= replica.SleepTimeInSeconds.TotalSeconds);

			var sqlContext2 = new SqlExecutionContextForTest(null, sqlContext);
			sqlContext2.overrideExecuteScalar = true;
			sqlContext2.scalarValue = true;

			replica = new AlwaysOnReplicaForTesting(ReplicaRole.Primary, SyncronisationHealth.HEALTHY, JoinState.JOINED_STANDALONE, replicaOption, Guid.NewGuid(), group, serverInfo, "TCP://10.10.10.100:5024");
			replica.SleepTimeInSeconds = TimeSpan.FromSeconds(2);
			timer.Restart();
			replica.CheckDatabaseIsOnlineWithWait_Exposed(sqlContext2);
			AssertEquals("Should be a quick check", true, timer.Elapsed.TotalSeconds < 1);

			sqlContext2.scalarValue = false;
			replica = new AlwaysOnReplicaForTesting(ReplicaRole.Resolving, SyncronisationHealth.HEALTHY, JoinState.JOINED_STANDALONE, replicaOption, Guid.NewGuid(), group, serverInfo, "TCP://10.10.10.100:5024");
			timer.Restart();
			var nrLoops = 2;
			replica.SetWaitLoop(nrLoops);
			replica.SleepTimeInSeconds = TimeSpan.FromSeconds(2);
			replica.CheckDatabaseIsOnlineWithWait_Exposed(sqlContext2);
			AssertEquals(Invariant($"Check should take at least {nrLoops * replica.SleepTimeInSeconds.TotalSeconds} seconds"), true, timer.Elapsed.TotalSeconds > nrLoops * replica.SleepTimeInSeconds.TotalSeconds);

			replica = new AlwaysOnReplicaForTesting(ReplicaRole.Secondary, SyncronisationHealth.HEALTHY, JoinState.JOINED_STANDALONE, replicaOption, Guid.NewGuid(), group, serverInfo, "TCP://10.10.10.100:5024");
			timer.Restart();
			nrLoops = 1;
			replica.SetWaitLoop(nrLoops);
			replica.SleepTimeInSeconds = TimeSpan.FromSeconds(2);
			replica.CheckDatabaseIsOnlineWithWait_Exposed(sqlContext2);
			AssertEquals(Invariant($"Check should take at least {nrLoops * replica.SleepTimeInSeconds.TotalSeconds} seconds"), true, timer.Elapsed.TotalSeconds > nrLoops * replica.SleepTimeInSeconds.TotalSeconds);
		}

		public void TestGetCheckDatabaseIsOnlineScriptReturnsCorrectValue()
		{
			var dbName = "aDbNameA7E605F6AE50";
			try
			{
				var serverInfo = new SqlServerInfo("ReplicaServer", "SomeNode", "SomeInstance", "SomeDomain", "SomeNode.SomeDomain", default);
				var replicaId = Guid.NewGuid();
				CreateTestViewsAndTables(dbName, replicaId);
				var group = new AvailabilityGroupForTesting();
				var replicaOption = ReplicaOptionsTest.NewReplicaOptionsForTesting(AvailabilityMode.ASYNCHRONOUS_COMMIT, FailoverMode.MANUAL);
				var replica = new AlwaysOnReplicaForTesting(ReplicaRole.Secondary, SyncronisationHealth.HEALTHY, JoinState.JOINED_STANDALONE, replicaOption, replicaId, group, serverInfo, "TCP://10.10.10.100:5024");

				AssertQueryResult(dbName: dbName, replica: replica, dbState: 0, isLocal: 1, role: 1, operational_state: 2, expected: true);
				AssertQueryResult(dbName: dbName, replica: replica, dbState: 1, isLocal: 1, role: 1, operational_state: 2, expected: false);
				AssertQueryResult(dbName: dbName, replica: replica, dbState: 0, isLocal: 0, role: 1, operational_state: 2, expected: false);
				AssertQueryResult(dbName: dbName, replica: replica, dbState: 0, isLocal: 1, role: 0, operational_state: 2, expected: false);
				AssertQueryResult(dbName: dbName, replica: replica, dbState: 0, isLocal: 1, role: 1, operational_state: 0, expected: false);
				AssertQueryResult(dbName: dbName, replica: replica, dbState: 0, isLocal: 0, role: 0, operational_state: 2, expected: false);
				AssertQueryResult(dbName: dbName, replica: replica, dbState: 1, isLocal: 0, role: 0, operational_state: 2, expected: false);

				using (var admConnection = Db.NewAdminConnection(Db.SqlMasterDb))
				{
					admConnection.ExecuteNonQuery("Update master.dbo.tbl_A7E605F6AE50_databases Set replica_id = NEWID()");
				}
				AssertQueryResult(dbName: dbName, replica: replica, dbState: 0, isLocal: 1, role: 1, operational_state: 2, expected: false);
			}
			finally
			{
				DropTestViewsAndTables(dbName);
			}
		}

		void AssertQueryResult(string dbName, AlwaysOnReplicaForTesting replica, int dbState, int isLocal, int role, int operational_state, bool expected)
		{
			var sqlScript = Invariant($@"
		Update master.dbo.tbl_A7E605F6AE50_databases Set state = {dbState}
		Update master.dbo.tbl_A7E605F6AE50_dm_hadr_availability_replica_states Set is_local = {isLocal}, role = {role}, operational_state = {operational_state}
		");
			using (var admConnection = Db.NewAdminConnection(Db.SqlMasterDb))
			{
				admConnection.ExecuteNonQuery(sqlScript);
				sqlScript = replica.GetCheckDatabaseIsOnlineScriptForTest(dbName);
				AssertEquals(expected, (bool)admConnection.ExecuteScalar(sqlScript));
			}
		}

		void DropTestViewsAndTables(string dbName)
		{
			var sqlScript = @"
		If EXISTS(Select * from sys.views Where name = 'vw_A7E605F6AE50_databases') Drop View vw_A7E605F6AE50_databases
		If EXISTS(Select * from sys.tables Where name = 'tbl_A7E605F6AE50_databases') Drop Table tbl_A7E605F6AE50_databases

		If EXISTS(Select * from sys.views Where name = 'vw_A7E605F6AE50_dm_hadr_availability_replica_states') Drop View vw_A7E605F6AE50_dm_hadr_availability_replica_states
		If EXISTS(Select * from sys.tables Where name = 'tbl_A7E605F6AE50_dm_hadr_availability_replica_states') Drop Table tbl_A7E605F6AE50_dm_hadr_availability_replica_states
		";
			using (var admConnection = Db.NewAdminConnection(Db.SqlMasterDb))
			{
				admConnection.ExecuteNonQuery(sqlScript);
				AdoTestUtils.DropDbIfExists(admConnection, dbName);
			}
		}

		void CreateTestViewsAndTables(string dbName, Guid replicaId)
		{
			DropTestViewsAndTables(dbName);
			using (var admConnection = Db.NewAdminConnection(Db.SqlMasterDb))
			{
				AdoTestUtils.CreateDbDropExisting(dbName);
				var sqlScript = Invariant($@"
		Select database_id, state, '{replicaId}' replica_id Into tbl_A7E605F6AE50_databases From sys.databases Where name = '{dbName}'

		Select replica_id, 1 is_local, 1 role, 2 operational_state Into tbl_A7E605F6AE50_dm_hadr_availability_replica_states From tbl_A7E605F6AE50_databases
		");
				admConnection.ExecuteNonQuery(sqlScript);

				sqlScript = "Create View vw_A7E605F6AE50_databases As Select * From tbl_A7E605F6AE50_databases";
				admConnection.ExecuteNonQuery(sqlScript);

				sqlScript = "Create View vw_A7E605F6AE50_dm_hadr_availability_replica_states as Select * From tbl_A7E605F6AE50_dm_hadr_availability_replica_states";
				admConnection.ExecuteNonQuery(sqlScript);
			}
		}

		void AssertPerfomPlannedManualFailover(AvailabilityMode primaryAvailabilityMode, AvailabilityMode replicaAvailabilityMode, SyncronisationHealth syncHealth, string expectedErrorMessage)
		{
			var group = new AvailabilityGroupForTesting();
			var primaryOption = ReplicaOptionsTest.NewReplicaOptionsForTesting(primaryAvailabilityMode, FailoverMode.MANUAL);
			var primaryServerInfo = new SqlServerInfo("ReplicaServer", "SomeNode", "SomeInstance", "SomeDomain", "SomeNode.SomeDomain", default);
			var primary = new AlwaysOnReplicaForTesting(ReplicaRole.Primary, syncHealth, JoinState.JOINED_STANDALONE, primaryOption, Guid.NewGuid(), group, primaryServerInfo, "TCP://10.10.10.100:5024");
			group.PrimaryReplica = primary;

			var replicaServerInfo = new SqlServerInfo("ReplicaServer", "SomeNode", "SomeInstance", "SomeDomain", "SomeNode.SomeDomain", default);
			var replicaOptions = ReplicaOptionsTest.NewReplicaOptionsForTesting(replicaAvailabilityMode, FailoverMode.MANUAL);
			var replica = new AlwaysOnReplicaForTesting(ReplicaRole.Primary, syncHealth, JoinState.JOINED_STANDALONE, replicaOptions, Guid.NewGuid(), group, replicaServerInfo, "TCP://10.10.10.100:5024");

			((IAlwaysOnReplica)replica).PerformPlannedManualFailover();

			if (string.IsNullOrEmpty(expectedErrorMessage))
			{
				AssertEquals("ReplicaServer", replica.ReplicaServer);
				AssertEquals("Two commands were executed", 3, replica.SqlCommandString.Count);
				AssertEquals(Invariant($"ALTER AVAILABILITY GROUP [{@group.GroupName}] FAILOVER"), replica.SqlCommandString[0]);
				var mainDbName = group.MainDatabaseName;
				AssertEquals("Waiting for DB to come online command", Invariant($@"
Declare @isDbOnline bit = 0
If EXISTS(
			Select null
			From
				sys.databases dbs INNER JOIN
				sys.dm_hadr_availability_replica_states replicas on dbs.replica_id = replicas.replica_id
			Where 1=1
				AND dbs.database_id = DB_ID('{mainDbName}')
				AND dbs.state = 0  
				AND replicas.is_local = 1
				AND replicas.role = 1
				AND replicas.operational_state = 2
		)
	Set @isDbOnline = 1
Select @isDbOnline
")
, replica.SqlCommandString[1]);
				AssertEquals("Executed Command", Invariant($@"
DECLARE @SqlCmd nvarchar(1000) = (
	SELECT TOP (1)
		'EXEC [{mainDbName}]..sp_executesql N''
			IF exists(SELECT null FROM sys.database_principals WHERE name = ''''OdysseyAdmin'''') DROP USER [OdysseyAdmin];
			ALTER AUTHORIZATION ON DATABASE::[{mainDbName}] TO [OdysseyAdmin];
		'';'
	FROM
		(SELECT [sid] FROM sys.server_principals WHERE name = 'OdysseyAdmin') AdminLogin
		LEFT JOIN (SELECT [sid] FROM [{mainDbName}].sys.database_principals WHERE name = 'dbo') DboUser
			ON DboUser.sid = AdminLogin.sid
		LEFT JOIN (SELECT owner_sid FROM sys.databases WHERE name = '{mainDbName}') UserDatabase
			ON UserDatabase.owner_sid = AdminLogin.sid
	WHERE
		(DboUser.sid is null OR UserDatabase.owner_sid is null)
);
IF (@SqlCmd is not null) EXEC (@SqlCmd);
"), replica.SqlCommandString[2]);
			}
			else
			{
				AssertEquals(expectedErrorMessage, ((IValidationStatus)replica).LastErrorMessage);
			}
		}

		public void TestSuspendResumeOnSecondaryReplica()
		{
			IReplicaOptions replicaOptions = ReplicaOptions.NewFromRawData(0, 1, 1, "TestUrl");
			IAvailabilityGroup group = new AvailabilityGroupForTesting();
			var replica = new AlwaysOnReplicaForTesting(ReplicaRole.Primary, SyncronisationHealth.HEALTHY, JoinState.JOINED_STANDALONE, replicaOptions, Guid.NewGuid(), group, defaultServerInfo, "TCP://10.10.10.100:5024");

			((IAlwaysOnReplica)replica).ResumeReplication(Db.DatabaseName);
			AssertEquals(expected: Invariant($@"
If EXISTS (Select null From sys.databases Where name = '{Db.DatabaseName}' and replica_id IS NOT NULL)
ALTER DATABASE [{Db.DatabaseName}] SET HADR RESUME
"), replica.SqlCommandString[0]);

			((IAlwaysOnReplica)replica).SuspendReplication(Db.DatabaseName);
			AssertEquals(expected: Invariant($@"
If EXISTS (Select null From sys.databases Where name = '{Db.DatabaseName}' and replica_id IS NOT NULL)
ALTER DATABASE [{Db.DatabaseName}] SET HADR SUSPEND
"), replica.SqlCommandString[1]);
		}

		public void TestIsPrimary()
		{
			var serverInfo = new SqlServerInfo(Db.ServerName, "actualserver", "SomeInstance", "SomeDomain", Db.ServerName, default);

			var replicaOptionsMock = new Mock<IReplicaOptions>();
			var groupMock = new Mock<IAvailabilityGroup>();
			var primaryReplica = AlwaysOnReplicaFactory.New(ReplicaRole.Primary, 0, 0, replicaOptionsMock.Object, Guid.NewGuid(), groupMock.Object, serverInfo, "");
			var secondaryReplica = AlwaysOnReplicaFactory.New(ReplicaRole.Secondary, 0, 0, replicaOptionsMock.Object, Guid.NewGuid(), groupMock.Object, serverInfo, "");

			Assert(primaryReplica.IsPrimary);
			Assert(!secondaryReplica.IsPrimary);
		}

		public void TestRefreshHealthState_ExceptionThrown()
		{
			IReplicaOptions replicaOptions = ReplicaOptions.NewFromRawData(0, 1, 1, "TestUrl");
			IAvailabilityGroup group = new AvailabilityGroupForTesting();
			var replica = new AlwaysOnReplicaForTesting(ReplicaRole.Primary, SyncronisationHealth.HEALTHY, JoinState.JOINED_STANDALONE, replicaOptions, Guid.NewGuid(), group, defaultServerInfo, "TCP://10.10.10.100:5024");

			var readermock = new Mock<IDataReader>();
			readermock.Setup(r => r.Read()).Throws<Exception>();
			replica.DataReaderOverride = readermock.Object;

			((IAlwaysOnReplica)replica).RefreshHealthState();

			AssertEquals(SyncronisationHealth.NOT_HEALTHY, ((IAlwaysOnReplica)replica).Health);
		}

		public void TestRefreshHealthState_ExceptionThrownOnConnection()
		{
			IReplicaOptions replicaOptions = ReplicaOptions.NewFromRawData(0, 1, 1, "TestUrl");
			IAvailabilityGroup group = new AvailabilityGroupForTesting();
			var replica = new AlwaysOnReplicaForTesting(ReplicaRole.Primary, SyncronisationHealth.HEALTHY, JoinState.JOINED_STANDALONE, replicaOptions, Guid.NewGuid(), group, defaultServerInfo, "TCP://10.10.10.100:5024");

			replica.GetSqlExecutionContextForReplicaServerAction = () => throw new Exception();

			((IAlwaysOnReplica)replica).RefreshHealthState();

			AssertEquals(SyncronisationHealth.NOT_HEALTHY, ((IAlwaysOnReplica)replica).Health);
		}

		public void TestCopyDatabaseLoginsFromPrimaryReplicaShowsExceptionFromGetGroupDatabaseLoginsWithoutErrorHandling()
		{
			// Arrange
			const string expectedMessage = "I'm an expected exception during the database access";
			var alwaysOnPrimaryReplicaMock = new Mock<AlwaysOnReplica>(ReplicaRole.Secondary, SyncronisationHealth.HEALTHY, JoinState.NOT_JOINED, Mock.Of<IReplicaOptions>(), Guid.Empty, Mock.Of<IAvailabilityGroup>(), defaultServerInfo, string.Empty);
			alwaysOnPrimaryReplicaMock
				.Protected()
				.Setup<ISqlExecutionContext>("GetSqlExecutionContextForReplicaServer")
				.Throws(new AlwaysOnException(expectedMessage));
			var availabilityGroupMock = Mock.Of<IAvailabilityGroup>(group => group.PrimaryReplica == alwaysOnPrimaryReplicaMock.Object);

			var alwaysOnSecondaryReplica = new AlwaysOnReplica(ReplicaRole.Secondary, SyncronisationHealth.HEALTHY, JoinState.NOT_JOINED, Mock.Of<IReplicaOptions>(), Guid.Empty, availabilityGroupMock, defaultServerInfo, string.Empty);

			// Act
			((IAlwaysOnReplica)alwaysOnSecondaryReplica).CopyDatabaseLoginsFromPrimaryReplica();

			// Assert
			AssertEquals(expectedMessage, ((IValidationStatus)alwaysOnSecondaryReplica).LastErrorMessage);
		}

		public void TestCopyDatabaseLoginsFromPrimaryReplicaShowsExceptionFromGetOdysseyAdminLoginIfExistsWithoutErrorHandlingFromPrimary()
		{
			var serverInfo = new SqlServerInfo(Db.ServerName);
			// Arrange
			const string expectedMessage = "I'm an expected exception during the database access";
			var alwaysOnPrimaryReplicaMock = new Mock<AlwaysOnReplica>(ReplicaRole.Secondary, SyncronisationHealth.HEALTHY, JoinState.NOT_JOINED, Mock.Of<IReplicaOptions>(), Guid.Empty, Mock.Of<IAvailabilityGroup>(), serverInfo, string.Empty);
			alwaysOnPrimaryReplicaMock
				.Protected()
				.SetupSequence<ISqlExecutionContext>("GetSqlExecutionContextForReplicaServer")
				.Returns(CreateSqlContextForTest)
				.Throws(new AlwaysOnException(expectedMessage));
			var availabilityGroupMock = Mock.Of<IAvailabilityGroup>(group => group.PrimaryReplica == alwaysOnPrimaryReplicaMock.Object);

			var alwaysOnSecondaryReplica = new AlwaysOnReplica(ReplicaRole.Secondary, SyncronisationHealth.HEALTHY, JoinState.NOT_JOINED, Mock.Of<IReplicaOptions>(), Guid.Empty, availabilityGroupMock, serverInfo, string.Empty);

			// Act
			((IAlwaysOnReplica)alwaysOnSecondaryReplica).CopyDatabaseLoginsFromPrimaryReplica();

			// Assert
			AssertEquals(expectedMessage, ((IValidationStatus)alwaysOnSecondaryReplica).LastErrorMessage);
		}

		public void TestCopyDatabaseLoginsFromPrimaryReplicaShowsExceptionFromGetOdysseyAdminLoginIfExistsWithoutErrorHandlingFromSecondary()
		{
			// Arrange
			const string expectedMessage = "I'm an expected exception during the database access";
			var alwaysOnPrimaryReplicaMock = new Mock<AlwaysOnReplica>(ReplicaRole.Secondary, SyncronisationHealth.HEALTHY, JoinState.NOT_JOINED, Mock.Of<IReplicaOptions>(), Guid.Empty, Mock.Of<IAvailabilityGroup>(), defaultServerInfo, string.Empty);
			alwaysOnPrimaryReplicaMock
				.Protected()
				.Setup<ISqlExecutionContext>("GetSqlExecutionContextForReplicaServer")
				.Returns(CreateSqlContextForTest);
			var availabilityGroupMock = Mock.Of<IAvailabilityGroup>(group => group.PrimaryReplica == alwaysOnPrimaryReplicaMock.Object);

			var alwaysOnSecondaryReplica = new Mock<AlwaysOnReplica>(ReplicaRole.Secondary, SyncronisationHealth.HEALTHY, JoinState.NOT_JOINED, Mock.Of<IReplicaOptions>(), Guid.Empty, availabilityGroupMock, defaultServerInfo, string.Empty)
			{
				CallBase = true,
			};
			alwaysOnSecondaryReplica
				.Protected()
				.SetupSequence<ISqlExecutionContext>("GetSqlExecutionContextForReplicaServer")
				.Returns(CreateSqlContextForTest);
			alwaysOnSecondaryReplica
				.As<IAlwaysOnReplica>()
				.SetupSequence(replica => replica.GetOdysseyAdminLoginIfExistsWithoutErrorHandling())
				.Throws(new AlwaysOnException(expectedMessage));

			// Act
			((IAlwaysOnReplica)alwaysOnSecondaryReplica.Object).CopyDatabaseLoginsFromPrimaryReplica();

			// Assert
			AssertEquals(expectedMessage, ((IValidationStatus)alwaysOnSecondaryReplica.Object).LastErrorMessage);
		}

		public void TestCopyDatabaseLoginsFromPrimaryReplicaShowsExceptionFromGetOdysseyAdminLoginIfExistsWithoutErrorHandlingFromSecondarySecondTime()
		{
			var serverInfo = new SqlServerInfo(Db.ServerName);

			// Arrange
			const string expectedMessage = "I'm an expected exception during the database access";
			Db.Connection.EnsureIsOpen();
			var alwaysOnPrimaryReplicaMock = new Mock<AlwaysOnReplica>(ReplicaRole.Secondary, SyncronisationHealth.HEALTHY, JoinState.NOT_JOINED, Mock.Of<IReplicaOptions>(), Guid.Empty, Mock.Of<IAvailabilityGroup>(), defaultServerInfo, string.Empty);
			alwaysOnPrimaryReplicaMock
				.Protected()
				.Setup<ISqlExecutionContext>("GetSqlExecutionContextForReplicaServer")
				.Returns(CreateSqlContextForTest);
			var availabilityGroupMock = Mock.Of<IAvailabilityGroup>(group => group.PrimaryReplica == alwaysOnPrimaryReplicaMock.Object);

			var alwaysOnSecondaryReplica = new Mock<AlwaysOnReplica>(ReplicaRole.Secondary, SyncronisationHealth.HEALTHY, JoinState.NOT_JOINED, Mock.Of<IReplicaOptions>(), Guid.Empty, availabilityGroupMock, serverInfo, string.Empty)
			{
				CallBase = true,
			};
			alwaysOnSecondaryReplica
				.As<IAlwaysOnReplica>()
				.SetupSequence(replica => replica.GetOdysseyAdminLoginIfExistsWithoutErrorHandling())
				.Returns(new DbLoginInfo("b", "S", "C", "d", "e"))
				.Throws(new AlwaysOnException(expectedMessage));
			alwaysOnSecondaryReplica
				.Protected()
				.Setup<ISqlExecutionContext>("GetSqlExecutionContextForReplicaServer")
				.Returns(CreateSqlContextForTest);

			// Act
			((IAlwaysOnReplica)alwaysOnSecondaryReplica.Object).CopyDatabaseLoginsFromPrimaryReplica();

			// Assert
			AssertEquals(expectedMessage, ((IValidationStatus)alwaysOnSecondaryReplica.Object).LastErrorMessage);
		}

		static ISqlExecutionContext CreateSqlContextForTest()
		{
			return Program.SqlContextManager.GetSqlExecutionContext(Db.ServerName, Db.SqlMasterDb);
		}

		public void TestGetGroupDatabaseLoginsCatchesException()
		{
			// Arrange
			const string expectedMessage = "I'm an expected exception during the database access";
			var alwaysOnPrimaryReplicaMock = new Mock<AlwaysOnReplica>(ReplicaRole.Secondary, SyncronisationHealth.HEALTHY, JoinState.NOT_JOINED, Mock.Of<IReplicaOptions>(), Guid.Empty, Mock.Of<IAvailabilityGroup>(), defaultServerInfo, string.Empty)
			{
				CallBase = true,
			};
			alwaysOnPrimaryReplicaMock
				.Protected()
				.Setup<ISqlExecutionContext>("GetSqlExecutionContextForReplicaServer")
				.Throws(new AlwaysOnException(expectedMessage));

			// Act
			((IAlwaysOnReplica)alwaysOnPrimaryReplicaMock.Object).GetGroupDatabaseLogins();

			// Assert
			AssertEquals(expectedMessage, ((IValidationStatus)alwaysOnPrimaryReplicaMock.Object).LastErrorMessage);
		}

		public void TestGetOdysseyAdminLoginIfExists()
		{
			// Arrange
			var serverInfo = new SqlServerInfo(Db.ServerName);
			var replicaOption = ReplicaOptionsTest.NewReplicaOptionsForTesting(AvailabilityMode.ASYNCHRONOUS_COMMIT, FailoverMode.MANUAL);
			var replica = new AlwaysOnReplica(ReplicaRole.Primary, SyncronisationHealth.HEALTHY, JoinState.JOINED_STANDALONE, replicaOption, Guid.NewGuid(), Mock.Of<IAvailabilityGroup>(), serverInfo, "TCP://10.10.10.100:5024");

			// Act
			var result = replica.GetOdysseyAdminLoginIfExists();

			// Assert
			using var connection = Db.NewAdminConnection();
			var expectedSid = connection.ExecuteScalar<string>("SELECT sys.fn_varbintohexsubstring(1, original_security_id, 1, 0) FROM sys.dm_exec_sessions WHERE session_id = @@SPID");
			AssertEquals(expectedSid, result.LoginSid);
			AssertEquals(true, result.IsOdysseyAdminLogin);
			AssertEquals(DataProtectionTestBed.Current.GetExpectedCredentials(TestEnvironmentWellKnownSecret.TestServerOdysseyAdmin).UserName, result.LoginName);
			AssertEquals(DbLoginInfo.LoginType.SQL, result.Type);
		}

		public void TestGetOdysseyAdminLoginIfExistsCatchesException()
		{
			// Arrange
			const string expectedMessage = "I'm an expected exception during the database access";
			var alwaysOnPrimaryReplicaMock = new Mock<AlwaysOnReplica>(ReplicaRole.Secondary, SyncronisationHealth.HEALTHY, JoinState.NOT_JOINED, Mock.Of<IReplicaOptions>(), Guid.Empty, Mock.Of<IAvailabilityGroup>(), defaultServerInfo, string.Empty)
			{
				CallBase = true,
			};
			alwaysOnPrimaryReplicaMock
				.Protected()
				.Setup<ISqlExecutionContext>("GetSqlExecutionContextForReplicaServer")
				.Throws(new AlwaysOnException(expectedMessage));

			// Act
			((IAlwaysOnReplica)alwaysOnPrimaryReplicaMock.Object).GetOdysseyAdminLoginIfExists();

			// Assert
			AssertEquals(expectedMessage, ((IValidationStatus)alwaysOnPrimaryReplicaMock.Object).LastErrorMessage);
		}

		public void TestGetMainDb_ReturnsMainDb()
		{
			// Arrange
			var localHost = new SqlServerInfo(".", 1345);
			var alwaysOnDatabaseMock = new Mock<IAlwaysOnDatabase>();
			alwaysOnDatabaseMock.Setup(x => x.GroupId).Returns(Guid.NewGuid());
			alwaysOnDatabaseMock.Setup(x => x.GroupName).Returns("group1");
			alwaysOnDatabaseMock.Setup(x => x.Name).Returns(Db.DatabaseName);

			var availabilityGroupMock = new Mock<AvailabilityGroup>(localHost, alwaysOnDatabaseMock.Object);
			var availabilityGroup = (IAvailabilityGroup)availabilityGroupMock.Object;

			availabilityGroupMock.Setup(x => x.GetGroupDatabases(It.IsAny<ISqlExecutionContext>())).Returns(new List<string>
					{
						"CW-RefDatabase",
						Db.DatabaseName,
						Db.DatabaseName + "Other",
						Db.DatabaseName + "_SD001",
						Db.DatabaseName + "_Audit",
						Db.DatabaseName + "_EDW",
						Db.DatabaseName + "_RefDb_Cmr_AU",
						Db.DatabaseName + "_RefDb_Ent_AU",
						Db.DatabaseName + "_RefDb_Ent_CA",
						Db.DatabaseName + "_RefDb_Ent_US",
						Db.DatabaseName + "_RefDb_Ent_ZZ",
						Db.DatabaseName + "_RefDb_Trf_AU",
						Db.DatabaseName + "_RefDb_Trf_CA",
						Db.DatabaseName + "_RefDb_Trf_GB",
						Db.DatabaseName + "_RefDb_Trf_NZ",
						Db.DatabaseName + "_SD001",
						Db.DatabaseName + "_UserRepository",
					});

			// Act
			availabilityGroupMock.Object.Load(localHost);
			var firstDbContainsNotUnderScore = availabilityGroup.Databases.FirstOrDefault(x => !x.Contains('_'));
			var mainDb = availabilityGroup.MainDatabaseName;

			// Assert
			AssertNotEquals(Db.DatabaseName, firstDbContainsNotUnderScore);
			AssertEquals(Db.DatabaseName, mainDb);
		}
	}

	class AlwaysOnReplicaForTesting : AlwaysOnReplica
	{
		public List<string> SqlCommandString { get; private set; }

		readonly bool isDbOnlineAndRoleIsPrimary;
		internal TimeSpan SleepTimeInSeconds
		{
			get
			{
				return base.sleepTimeSpan;
			}
			set
			{
				base.sleepTimeSpan = value;
			}
		}

		public AlwaysOnReplicaForTesting(ReplicaRole roleCode, SyncronisationHealth healthCode, JoinState joinStateCode, IReplicaOptions options, Guid replicaId, IAvailabilityGroup parentGroup, SqlServerInfo serverInfo, string endpointUrl)
			: base(roleCode, healthCode, joinStateCode, options, replicaId, parentGroup, serverInfo, endpointUrl)
		{
			SqlCommandString = new List<string>();
			isDbOnlineAndRoleIsPrimary = roleCode == ReplicaRole.Primary;
		}

		protected override ISqlExecutionContext GetSqlExecutionContextForReplicaServer()
		{
			GetSqlExecutionContextForReplicaServerAction?.Invoke();
			ReplicaServer = ServerInfo.ServerAlias;
			var contextWrapper = new SqlExecutionContextForTest(this.SqlCommandString, null);
			contextWrapper.overrideExecuteScalar = true;
			contextWrapper.scalarValue = isDbOnlineAndRoleIsPrimary;
			return contextWrapper;
		}
		public string ReplicaServer { get; private set; }

		public Action GetSqlExecutionContextForReplicaServerAction { get; set; }

		internal bool CheckDatabaseIsOnlineWithWait_Exposed(ISqlExecutionContext sqlContext)
		{
			return CheckDatabaseIsOnlineWithWait(sqlContext);
		}

		internal void SetWaitLoop(int numberOfLoops)
		{
			waitingLoops = numberOfLoops;
		}

		internal string GetCheckDatabaseIsOnlineScriptForTest(string dbName)
		{
			return base.GetCheckDatabaseIsOnlineScript(dbName).Replace("sys.", "master.dbo.vw_A7E605F6AE50_");
		}

		public IDataReader DataReaderOverride { get; set; }
	}

	class AvailabilityGroupForTesting : IAvailabilityGroup
	{
		public Guid GroupId
		{
			get { return Guid.Empty; }
		}

		public string GroupName
		{
			get { return "AvailabilityGroupForTesting"; }
		}

		public IEnumerable<IAlwaysOnReplica> Replicas
		{
			get { return null; }
		}

		public IAlwaysOnReplica PrimaryReplica { get; set; }

		public IEnumerable<string> Databases
		{
			get
			{
				yield return Db.DatabaseName + "_SD001";
				yield return Db.DatabaseName;
			}
		}

		public string MainDatabaseName => Db.DatabaseName;

		public IAlwaysOnReplica AddSecondaryReplica(ISecondaryServerInstance secondaryServer, IReplicaOptions replicaOptions)
		{
			return null;
		}

		public void RemoveSecondaryReplica(IAlwaysOnReplica secondaryReplica)
		{
		}

		public void ChangeReplicaSettings(IAlwaysOnReplica replica, IReplicaOptions newSettings)
		{
		}

		public void AddDatabases(IEnumerable<string> databasesToAdd)
		{
		}

		public bool IsLoaded
		{
			get { return false; }
		}

		public bool HasErrors
		{
			get { return false; }
		}

		public string LastErrorMessage
		{
			get { return null; }
		}

		public string LastWarningMessage { get; }

		public bool HasWarnings
		{
			get { return !string.IsNullOrWhiteSpace(LastWarningMessage); }
		}
	}
}
