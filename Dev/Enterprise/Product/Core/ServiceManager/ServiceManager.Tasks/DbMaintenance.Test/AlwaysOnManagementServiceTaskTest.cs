using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Bi.Common;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.SqlServer;
using CargoWise.Data.Testing;
using Enterprise.Integration;
using Enterprise.SqlSecurity;
using Enterprise.ZArchitecture.AlwaysOnHelper;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ServiceManager.Tasks.DbMaintenance.Testing
{
	[TestedType(typeof(AlwaysOnManagementServiceTask))]
	sealed class AlwaysOnManagementServiceTaskTest : ServiceTaskTestCase<AlwaysOnManagementServiceTask>
	{
		protected override void SetUpCore()
		{
			base.SetUpCore();

			initialValues.ServerName = Db.ServerName;
			initialValues.DbName = Db.DatabaseName;

			serverNameWithoutPort = EnsureWithoutPort();
			serverNameWithPort = EnsureWithPort();

			string EnsureWithoutPort()
			{
				AssertEquals(
					$"This test setup requires Db.ServerName to not include a port. Db.ServerName: {Db.ServerName}",
					Db.ServerName.IndexOf(','),
					-1);
				return Db.ServerName;
			}

			string EnsureWithPort()
			{
				Db.ClearServerDetails();
				var databaseName = initialValues.DbName;
				var comma = initialValues.ServerName.IndexOf(',');
				var serverName = comma != -1 ? initialValues.ServerName : initialValues.ServerName + ",1433";

				Db.InitializeDatabaseDetails(serverName, databaseName);
				BiServers.ClearBiServersCache();

				AssertLessThan("There shouldn't be more than one comma in the servername",
								initialValues.ServerName.Count((c) => c == ','),
								2);

				using (var mainDbConnection = Db.NewAdminConnection())
				{
					AssertEquals("Audit server should include port number", Db.ServerName,
								BiServers.LoadAuditServerUsingCacheIfPossible(mainDbConnection)
								);
					AssertEquals("EDW server should include port number", Db.ServerName,
								BiServers.LoadDataWarehouseServerUsingCacheIfPossible(mainDbConnection)
								);
				}

				return DataUtils.GetDbSeverFullDomainNameIncludingSqlPort(initialValues.ServerName);
			}
		}

		protected override void TearDownCore()
		{
			Db.ClearServerDetails();
			BiServers.ClearBiServersCache();

			Db.InitializeDatabaseDetails(initialValues.ServerName, initialValues.DbName);

			base.TearDownCore();
		}

		public void TestServiceTaskCanRunInAnyBranch()
		{
			Assert(GetHostedServiceAttributes().All(x => x.CanRunInAnyBranch));
		}

		[UseSnapshotProtection]
		public void TestAlwaysOnManagementTasksWhenModernSecurityIsOff()
		{
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;

			var auCmrDb = Db.DatabaseName + "_RefDb_Cmr_AU";
			var usRefDb = Db.DatabaseName + "_RefDb_Ent_US";

			using (var connection = Db.NewAdminConnection())
			{
				connection.BeginTransaction();

				try
				{
					var loginRepairConnection = (IDbLoginRepair)connection;

					DatabaseLoginTest.AssertLoginIsMappedToDatabase(connection, auCmrDb, loginRepairConnection.RestrictedWriterDbLoginName, expected: true);
					DatabaseLoginTest.AssertLoginIsMappedToDatabase(connection, auCmrDb, loginRepairConnection.ReaderDbLoginName, expected: true);
					DatabaseLoginTest.AssertLoginIsMappedToDatabase(connection, usRefDb, loginRepairConnection.RestrictedWriterDbLoginName, expected: true);
					DatabaseLoginTest.AssertLoginIsMappedToDatabase(connection, usRefDb, loginRepairConnection.ReaderDbLoginName, expected: true);

					loginRepairConnection.DropDbLoginUsersFromDatabase(auCmrDb, msg => { });
					loginRepairConnection.DropDbLoginUsersFromDatabase(usRefDb, msg => { });
					DatabaseLoginTest.AssertLoginIsMappedToDatabase(connection, auCmrDb, loginRepairConnection.RestrictedWriterDbLoginName, expected: false);
					DatabaseLoginTest.AssertLoginIsMappedToDatabase(connection, auCmrDb, loginRepairConnection.ReaderDbLoginName, expected: false);
					DatabaseLoginTest.AssertLoginIsMappedToDatabase(connection, usRefDb, loginRepairConnection.RestrictedWriterDbLoginName, expected: false);
					DatabaseLoginTest.AssertLoginIsMappedToDatabase(connection, usRefDb, loginRepairConnection.ReaderDbLoginName, expected: false);

					var logger = new TestServiceLogger();
					var taskForTest = new AlwaysOnManagementServiceTaskForTest();
					taskForTest.ServiceLogger = logger;

					taskForTest.EnsureApplicationDbLoginRights_Exposed(connection);

					var actualLogs = logger.ToString().Trim();
					AssertContains("Log", "Information|Ensure login and database mappings", actualLogs);
					AssertContains("RestrictedReaderLogin", $"Information|Login type [RestrictedReader] was checked on server.", actualLogs);
					AssertContains("RestrictedWriterLogin", $"Information|Login type [RestrictedWriter] was checked on server.", actualLogs);
					AssertContains("UnrestrictedWriterLogin", $"Information|Login type [UnrestrictedWriter] was checked on server.", actualLogs);
					AssertContains("CargoWiseReaderLogin", $"Information|Login type [Reader] was checked on server.", actualLogs);
					AssertContains("CargoWiseWriterLogin", $"Information|Login type [Writer] was checked on server.", actualLogs);

					DatabaseLoginTest.AssertLoginIsMappedToDatabase(connection, auCmrDb, loginRepairConnection.RestrictedWriterDbLoginName, expected: true);
					DatabaseLoginTest.AssertLoginIsMappedToDatabase(connection, auCmrDb, loginRepairConnection.ReaderDbLoginName, expected: true);
					DatabaseLoginTest.AssertLoginIsMappedToDatabase(connection, usRefDb, loginRepairConnection.RestrictedWriterDbLoginName, expected: true);
					DatabaseLoginTest.AssertLoginIsMappedToDatabase(connection, usRefDb, loginRepairConnection.ReaderDbLoginName, expected: true);
				}
				finally
				{
					connection.RollbackTransaction();
				}
			}
		}

		[UseSnapshotProtection]
		public void TestAlwaysOnManagementTasksForBiWhenModernSecurityIsOff()
		{
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;

			var auditDb = Db.DatabaseName + "_Audit";
			var edwDb = Db.DatabaseName + "_EDW";

			using (var connection = Db.NewAdminConnection())
			{
				connection.BeginTransaction();

				try
				{
					var loginRepairConnection = (IDbLoginRepair)connection;

					DatabaseLoginTest.AssertLoginIsMappedToDatabase(connection, auditDb, loginRepairConnection.RestrictedWriterDbLoginName, expected: true);
					DatabaseLoginTest.AssertLoginIsMappedToDatabase(connection, auditDb, loginRepairConnection.ReaderDbLoginName, expected: true);
					DatabaseLoginTest.AssertLoginIsMappedToDatabase(connection, edwDb, loginRepairConnection.RestrictedWriterDbLoginName, expected: true);
					DatabaseLoginTest.AssertLoginIsMappedToDatabase(connection, edwDb, loginRepairConnection.ReaderDbLoginName, expected: true);

					loginRepairConnection.DropDbLoginUsersFromDatabase(auditDb, msg => { });
					loginRepairConnection.DropDbLoginUsersFromDatabase(edwDb, msg => { });
					DatabaseLoginTest.AssertLoginIsMappedToDatabase(connection, auditDb, loginRepairConnection.RestrictedWriterDbLoginName, expected: false);
					DatabaseLoginTest.AssertLoginIsMappedToDatabase(connection, auditDb, loginRepairConnection.ReaderDbLoginName, expected: false);
					DatabaseLoginTest.AssertLoginIsMappedToDatabase(connection, edwDb, loginRepairConnection.RestrictedWriterDbLoginName, expected: false);
					DatabaseLoginTest.AssertLoginIsMappedToDatabase(connection, edwDb, loginRepairConnection.ReaderDbLoginName, expected: false);

					var logger = new TestServiceLogger();
					var taskForTest = new AlwaysOnManagementServiceTaskForTest();
					taskForTest.ServiceLogger = logger;

					taskForTest.EnsureApplicationDbLoginRights_Exposed(connection);

					var actualLogs = logger.ToString().Trim();
					AssertContains("Log", "Information|Ensure login and database mappings", actualLogs);
					AssertContains("RestrictedReaderLogin", $"Information|Login type [RestrictedReader] was checked on server.", actualLogs);
					AssertContains("RestrictedWriterLogin", $"Information|Login type [RestrictedWriter] was checked on server.", actualLogs);
					AssertContains("UnrestrictedWriterLogin", $"Information|Login type [UnrestrictedWriter] was checked on server.", actualLogs);
					AssertContains("CargoWiseReaderLogin", $"Information|Login type [Reader] was checked on server.", actualLogs);
					AssertContains("CargoWiseWriterLogin", $"Information|Login type [Writer] was checked on server.", actualLogs);

					DatabaseLoginTest.AssertLoginIsMappedToDatabase(connection, auditDb, loginRepairConnection.RestrictedWriterDbLoginName, expected: true);
					DatabaseLoginTest.AssertLoginIsMappedToDatabase(connection, auditDb, loginRepairConnection.ReaderDbLoginName, expected: true);
					DatabaseLoginTest.AssertLoginIsMappedToDatabase(connection, edwDb, loginRepairConnection.RestrictedWriterDbLoginName, expected: true);
					DatabaseLoginTest.AssertLoginIsMappedToDatabase(connection, edwDb, loginRepairConnection.ReaderDbLoginName, expected: true);
				}
				finally
				{
					connection.RollbackTransaction();
				}
			}
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection]
		public void TestAlwaysOnManagementTasksCallsSqlSecurityManagerPropagateWhenMainDbIsPartOfAONAndModernSecurityIsOn()
		{
			// Arrange
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;

			AlwaysOn.IsDbPartOfAlwaysOn_ForTest.Value = true;

			var testReplicasNames =
				new List<AlwaysOnReplicaInfo>()
				{
					new AlwaysOnReplicaInfo { ReplicaServerName = Db.ServerName, AvailabilityMode = 1 },
					new AlwaysOnReplicaInfo { ReplicaServerName = "testReplica11", AvailabilityMode = 1 },
					new AlwaysOnReplicaInfo { ReplicaServerName = "testReplica12", AvailabilityMode = 1 },
				};

			AlwaysOn.ReplicaNames_ForTest.Value = testReplicasNames;

			EnvProxy.Instance.Registry.AlwaysOnReplicaCachedInfos = testReplicasNames.ToArray();

			var secondaryReplicaNames = AlwaysOnHelper.GetAlwaysOnSecondaryReplicaNames(Db.DatabaseName, useCache: true);

			var sqlSecurityManagerMock = new Mock<ISqlSecurityManager>();
			var aomTask = new AlwaysOnManagementServiceTaskWithProvidedSqlSecurityManager(sqlSecurityManagerMock.Object);
			aomTask.ServiceLogger = Mock.Of<ILogger>();

			// Act
			aomTask.RunTask(CancellationToken.None);

			// Assert
			sqlSecurityManagerMock.Verify(
				sqlSecurityManager
				=> sqlSecurityManager
				.Propagate(It.IsAny<AdminConnection>(), secondaryReplicaNames, It.IsAny<CancellationToken>()), Times.Once());
		}

		public void TestAlwaysOnManagementTasksGetSqlSecurityManagerReturnsNonNull()
		{
			// Arrange
			var aomTask = new AlwaysOnManagementServiceTask();
			aomTask.ServiceLogger = Mock.Of<ILogger>();

			// Act
			//Assert
			AssertNotNull(aomTask.GetSqlSecurityManager());
		}

		public void TestGetDatabasesNotJoinedToReplica()
		{
			var aomSrv = new AlwaysOnManagementServiceTask();
			var operationlDatabases = Db.Connection.GetDatabases(DatabaseType.Operational);
			var dbs = aomSrv.GetDatabasesNotJoinedToReplica(Db.Connection, Guid.Empty, operationlDatabases);

			AssertEquals(operationlDatabases.ToList().Count, dbs.Count);
			AssertArrayEqualsByElements(operationlDatabases.ToArray(), dbs.ToArray());
		}

		public void TestCheckReplica()
		{
			var aomTask = new AlwaysOnManagementServiceTaskForTest();
			var logger = new TestServiceLogger();

			var databases = Db.Connection.GetDatabases(DatabaseType.Operational);
			aomTask.UnhealthyDatabaseList.Add((databases.First(), serverNameWithoutPort));
			aomTask.RepairUnhealthyReplicationTest("agName", databases, logger);
			AssertEquals("Number of executed commands", logger.Count, 1);

			AssertEquals($"Information|Joining database [{databases.First()}] to availability group [agName] on secondary replica [{serverNameWithPort}]\r\n", logger.ToString());
		}

		public void TestCheckSecondaryReplicaButItIsBadState()
		{
			const string brokenServer = "BrokenServer";
			const string exceptionMessage = "A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was not found or was not accessible. Verify that the instance name is correct and that SQL Server is configured to allow remote connections.";

			foreach (var sqlErrorNumber in new[] { 1225, -1 })
			{
				ExecuteTest(sqlErrorNumber);
			}

			void ExecuteTest(int errorNumber)
			{
				//Arrange
				var logger = new TestServiceLogger();

				var aomTask = new AlwaysOnManagementServiceTaskForTest();
				aomTask.UnhealthyDatabaseList.Add(("NonexistentDatabase", brokenServer));

				AlwaysOn.IsDbPartOfAlwaysOn_ForTest = new LazyOverridable<bool?>(() =>
				{
					var error = SqlExceptionBuilder.CreateSqlError(errorNumber, 0, 20, "", exceptionMessage, "", 0);
					var errorCollection = SqlExceptionBuilder.CreateSqlErrorCollection(error);
					var exception = SqlExceptionBuilder.CreateSqlException(errorCollection);
					throw exception;
				});

				using (new DisposableAction(() => AlwaysOn.IsDbPartOfAlwaysOn_ForTest = new LazyOverridable<bool?>(() => null)))
				{
					//Act
					aomTask.RepairUnhealthyReplicationTest("agName", new List<string>(), logger);
				}

				//Assert
				AssertEquals("Number of executed commands", logger.Count, 1);
				AssertEquals(@$"Warning|Failed to connect to secondary replica [{DataUtils.GetDbSeverFullDomainNameIncludingSqlPort(brokenServer)}]. Please double check the replica's availability.
Error Number: {errorNumber}, Message:{exceptionMessage}
", logger.ToString());
			}
		}

		public void TestDatabaseJoinedButReplicationIsNotHealthy_ResumeSuccessfulOnline()
		{
			AlwaysOn.IsDbPartOfAlwaysOn_ForTest.Value = true;
			var aomTask = new AlwaysOnManagementServiceTaskForTest();
			var databases = Db.Connection.GetDatabases(DatabaseType.Operational);
			var logger = new TestServiceLogger();
			aomTask.UnhealthyDatabaseList.Add((databases.First(), serverNameWithoutPort));
			aomTask.ShouldExecuteSql = sql =>
			{
				if (sql.EndsWith("SET HADR SUSPEND;") || sql.EndsWith("SET HADR RESUME;"))
				{
					aomTask.SetDatabaseStateForTesting("ONLINE");
					return false;
				}
				return true;
			};

			aomTask.SetDatabaseStateForTesting("ONLINE");
			aomTask.SetDatabaseStateForTesting("Some unhealthy Status");
			aomTask.RepairUnhealthyReplicationTest("agName", databases, logger);

			AssertEquals($@"Information|Database [{databases.First()}] replication appears to be unhealthy on [{serverNameWithPort}].
Information|Suspending database [{databases.First()}] on [{serverNameWithPort}].
Information|Resuming database [{databases.First()}].
", logger.ToString());
		}

		public void TestDatabaseJoinedButReplicationIsNotHealthy_ResumeSuccessfulRestoring()
		{
			AlwaysOn.IsDbPartOfAlwaysOn_ForTest.Value = true;
			var aomTask = new AlwaysOnManagementServiceTaskForTest();
			var databases = Db.Connection.GetDatabases(DatabaseType.Operational);
			var logger = new TestServiceLogger();
			aomTask.UnhealthyDatabaseList.Add((databases.First(), serverNameWithoutPort));
			aomTask.ShouldExecuteSql = sql =>
			{
				if (sql.EndsWith("SET HADR SUSPEND;") || sql.EndsWith("SET HADR RESUME;"))
				{
					aomTask.SetDatabaseStateForTesting("RESTORING");
					return false;
				}
				return true;
			};

			aomTask.SetDatabaseStateForTesting("ONLINE");
			aomTask.SetDatabaseStateForTesting("Some unhealthy Status");
			aomTask.RepairUnhealthyReplicationTest("agName", databases, logger);

			AssertEquals($@"Information|Database [{databases.First()}] replication appears to be unhealthy on [{serverNameWithPort}].
Information|Suspending database [{databases.First()}] on [{serverNameWithPort}].
Information|Resuming database [{databases.First()}].
", logger.ToString());
		}

		public void TestDatabaseJoinedButReplicationIsNotHealthy_ResumeSuccessfulException()
		{
			// Arrange
			AlwaysOn.IsDbPartOfAlwaysOn_ForTest.Value = true;
			var aomTask = new AlwaysOnManagementServiceTaskForTest();
			var databases = Db.Connection.GetDatabases(DatabaseType.Operational);
			var logger = new TestServiceLogger();
			aomTask.UnhealthyDatabaseList.Add((databases.First(), Db.ServerName));
			aomTask.ShouldExecuteSql = sql =>
			{
				if (sql.EndsWith("SET HADR SUSPEND;"))
				{
					throw SqlExceptionBuilder.CreateSqlException(123, $"Database '{databases.First()}' cannot be opened. It has been marked SUSPECT by recovery.");
				}

				if (sql.EndsWith("SET HADR RESUME;"))
				{
					return false;
				}
				return true;
			};

			aomTask.SetDatabaseStateForTesting("ONLINE");
			aomTask.SetDatabaseStateForTesting("Some unhealthy Status");

			var expectedCommandList = new List<string>()
			{
				$"ALTER DATABASE [{databases.First()}] SET HADR SUSPEND;",
				$"ALTER DATABASE [{databases.First()}] SET HADR RESUME;"
			};

			// Act
			aomTask.RepairUnhealthyReplicationTest("agName", databases, logger);

			// Assert
			AssertContainsExactElementsInExactOrder("Commands should be same.", expectedCommandList, aomTask.commandList);
		}
		public void TestDatabaseJoinedButDatabaseStateIsRecoveryPending()
		{
			AlwaysOn.IsDbPartOfAlwaysOn_ForTest.Value = true;
			var aomTask = new AlwaysOnManagementServiceTaskForTest();
			var databases = Db.Connection.GetDatabases(DatabaseType.Operational);
			var logger = new TestServiceLogger();
			aomTask.UnhealthyDatabaseList.Add((databases.First(), serverNameWithoutPort));
			aomTask.ShouldExecuteSql = sql =>
			{
				if (sql.Contains("ALTER AVAILABILITY") || sql.Contains("HADR"))
				{
					aomTask.SetDatabaseStateForTesting("RESTORING");
					return false;
				}
				return true;
			};
			aomTask.SetDatabaseStateForTesting("RECOVERY_PENDING");
			aomTask.SetDatabaseStateForTesting("ONLINE");
			aomTask.RepairUnhealthyReplicationTest("agName", databases, logger);

			AssertEquals($@"Warning|Database [{databases.First()}] state is RECOVERY_PENDING on secondary replica [{serverNameWithPort}].
Information|Removing database [{databases.First()}] from availability group [agName] on secondary [{serverNameWithPort}].
Information|Joining database [{databases.First()}] to availability group [agName] on secondary replica [{serverNameWithPort}]
", logger.ToString());
		}

		[ExpectNoExceptions]
		public void TestDoesNotFixPrimaryWhenAlwaysOnResourceLockOccupied()
		{
			// Arrange
			var storageDb = $"{Db.DatabaseName}_SD001";
			var exclusiveResource = "AlwaysOnExclusiveLockKey";
			AlwaysOn.OverridableRemoveDatabaseFromAlwaysOnSetupForTest.Value = dbName => true;
			AlwaysOn.AlwaysOnDatabases_ForTest.Value = new List<string>() { Db.DatabaseName };
			AlwaysOn.ReplicaNames_ForTest.Value = new List<AlwaysOnReplicaInfo>
			{
				new AlwaysOnReplicaInfo { ReplicaServerName = Db.ServerName, AvailabilityMode = 1 },
			};
			var fakeGroupInfo = new AvailabilityGroupInfo("TestGroup", Guid.Parse("00000000-0000-0000-0000-000000000001"), Guid.Parse("00000000-0000-0000-0000-000000000002"), new List<string>());

			var logMock = new Mock<ILogger>();
			var aomTask = new AlwaysOnManagementServiceTask(logMock.Object);
			using (new DisposableAction(AlwaysOn.OverridableRemoveDatabaseFromAlwaysOnSetupForTest.ResetValue))
			using (new DisposableAction(AlwaysOn.AlwaysOnDatabases_ForTest.ResetValue))
			using (new DisposableAction(AlwaysOn.ReplicaNames_ForTest.ResetValue))
			using (AvailabilityGroupInfo.SetupGroupInfoForTest(fakeGroupInfo))
			using (AdoTestUtils.CreateDbDropExistingDisposable(storageDb))
			using (var extraConnection = Db.NewAdminConnection())
			{
				extraConnection.RunLocked(
					exclusiveResource,
					_ =>
					{
						// Act
						aomTask.RunTask();
					},
					max_tries: 1,
					dbName: Db.DatabaseName);

				// Assert
				logMock.Verify(x => x.Log(LogType.Warning, "Failed to get AppLock. Please try again after other process completes its task."), Times.Once);
			}
		}
		public void TestDatabaseJoinedButDatabaseStateIsRecoveryPendingForBoth()
		{
			AlwaysOn.IsDbPartOfAlwaysOn_ForTest.Value = true;
			var aomTask = new AlwaysOnManagementServiceTaskForTest();
			var databases = Db.Connection.GetDatabases(DatabaseType.Operational);
			aomTask.UnhealthyDatabaseList.Add((databases.First(), serverNameWithoutPort));
			var logger = new TestServiceLogger();
			aomTask.ShouldExecuteSql = sql =>
			{
				if (sql.Contains("REMOVE REPLICA"))
				{
					aomTask.SetDatabaseStateForTesting("RESTORING");
					return false;
				}
				return true;
			};
			aomTask.SetDatabaseStateForTesting("RECOVERY_PENDING");
			aomTask.SetDatabaseStateForTesting("RECOVERY_PENDING");
			aomTask.RepairUnhealthyReplicationTest("agName", databases, logger);

			AssertEquals($@"Error|Database [{databases.First()}] state is RECOVERY_PENDING on the primary replica.
", logger.ToString());
		}

		public void TestGetDatabases()
		{
			var aomSrv = new AlwaysOnManagementServiceTaskForTest();

			var operationlDatabases = Db.Connection.GetDatabases(DatabaseType.Operational | DatabaseType.UserRepository | DatabaseType.BI).ToList();

			var allDatabases = Db.Connection.GetDatabases(DatabaseType.All & ~DatabaseType.SingleSharedRef).ToList();

			AssertGreaterThan(allDatabases.Count, operationlDatabases.Count);

			try
			{
				var actual = Enumerable.Empty<string>();

				DataUtils.IsWiseTechGlobalDatabaseServerForTest = false;
				AlwaysOn.IsDbPartOfAlwaysOn_ForTest.Value = false;
				actual = aomSrv.GetDatabases_Exposed(Db.Connection);
				AssertArrayEqualsByElements(operationlDatabases.ToArray(), actual.ToArray());

				DataUtils.IsWiseTechGlobalDatabaseServerForTest = false;
				AlwaysOn.IsDbPartOfAlwaysOn_ForTest.Value = true;
				actual = aomSrv.GetDatabases_Exposed(Db.Connection);
				AssertArrayEqualsByElements(operationlDatabases.ToArray(), actual.ToArray());

				DataUtils.IsWiseTechGlobalDatabaseServerForTest = true;
				AlwaysOn.IsDbPartOfAlwaysOn_ForTest.Value = false;
				actual = aomSrv.GetDatabases_Exposed(Db.Connection);
				AssertArrayEqualsByElements(operationlDatabases.ToArray(), actual.ToArray());

				DataUtils.IsWiseTechGlobalDatabaseServerForTest = true;
				AlwaysOn.IsDbPartOfAlwaysOn_ForTest.Value = true;
				actual = aomSrv.GetDatabases_Exposed(Db.Connection);
				AssertArrayEqualsByElements(allDatabases.ToArray(), actual.ToArray());
			}
			finally
			{
				DataUtils.IsWiseTechGlobalDatabaseServerForTest = false;
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		(string ServerName, string DbName) initialValues;
		string serverNameWithPort;
		string serverNameWithoutPort;

		class AlwaysOnManagementServiceTaskWithProvidedSqlSecurityManager : AlwaysOnManagementServiceTask
		{
			readonly ISqlSecurityManager securityManager;

			public AlwaysOnManagementServiceTaskWithProvidedSqlSecurityManager(ISqlSecurityManager sqlSecurityManager)
			{
				this.securityManager = sqlSecurityManager;
			}

			internal override ISqlSecurityManager GetSqlSecurityManager()
			{
				return securityManager;
			}
		}
	}
}
