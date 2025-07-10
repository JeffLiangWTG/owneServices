using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.IO;
using Enterprise.DataTools.DbBackupAndRestore.Business.Common;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	sealed class DbMaintenanceManagerTest : TestCase
	{
		public void TestDropDatabases_MainDbOnly()
		{
			var maintenanceManager = new DbMaintenanceManager();

			using (var adminConn = Db.NewAdminConnection())
			{
				try
				{
					CreateTestDatabases(adminConn, testDbName, createDependentDbs: true);

					using (var tempDir = new TempDirectory())
					{
						maintenanceManager.DropDatabases(Db.ServerName, testDbName, false, false, false);
					}
					AssertEquals("Cannot drop database", false, CheckDatabaseExist(adminConn, testDbName));
				}
				finally
				{
					DropTestDatabase(adminConn, testDbName, deleteDependentDbs: true);
				}
			}
		}

		public void TestDropDatabasses_MainConnectionLogin()
		{
			var maintenanceManager = new DbMaintenanceManager();
			var testCw1MainLogins = ListOfCw1MainConnectionLogins(testDbName);
			AssertEquals("PRE: The number of main connection login should be same", 5, testCw1MainLogins.Count);

			using (var adminConn = Db.NewAdminConnection())
			{
				try
				{
					CreateTestDatabases(adminConn, testDbName, createDependentDbs: true);
					CreateLogins(adminConn, testDbName, testCw1MainLogins);

					AssertLoginsExist(adminConn, testCw1MainLogins, true);

					maintenanceManager.DropDatabases(Db.ServerName, testDbName, false, false, false);
					AssertLoginsExist(adminConn, testCw1MainLogins, false);
				}
				finally
				{
					DropTestDatabase(adminConn, testDbName, deleteDependentDbs: true);
					DropLogins(adminConn, testCw1MainLogins);
				}
			}
		}

		public void TestDropDatabasesAlsoDropsRelatedServerPrincipals()
		{
			var maintenanceManager = new DbMaintenanceManager();

			var nonCw1ManagedLogins = new List<(string username, bool isWindowsUser)> { ("SAND\\ADTest_Admin", true) };

			var testCw1SqlLogins = ListOfCw1SqlLogins(testDbName);
			testCw1SqlLogins.Add(("SAND\\ADTest_User", true));

			var anotherCw1SystemTestDb = "DbDropManagerTestAnotherDb";
			var anotherCw1SystemLogins = ListOfCw1SqlLogins(anotherCw1SystemTestDb);
			anotherCw1SystemLogins.Add(("SAND\\ADTest_User_No_OU", true));

			using (var adminConn = Db.NewAdminConnection())
			{
				try
				{
					CreateLogins(adminConn, Db.SqlMasterDb, nonCw1ManagedLogins);

					CreateTestDatabases(adminConn, testDbName, createDependentDbs: true);
					CreateLogins(adminConn, testDbName, testCw1SqlLogins);

					CreateTestDatabases(adminConn, anotherCw1SystemTestDb, createDependentDbs: true);
					CreateLogins(adminConn, anotherCw1SystemTestDb, anotherCw1SystemLogins);

					AssertLoginsExist(adminConn, nonCw1ManagedLogins, true);
					AssertLoginsExist(adminConn, anotherCw1SystemLogins, true);
					AssertLoginsExist(adminConn, testCw1SqlLogins, true);

					maintenanceManager.DropDatabases(Db.ServerName, testDbName, false, false, false);
					AssertLoginsExist(adminConn, nonCw1ManagedLogins, true);
					AssertLoginsExist(adminConn, anotherCw1SystemLogins, true);
					AssertLoginsExist(adminConn, testCw1SqlLogins, false);
				}
				finally
				{
					DropTestDatabase(adminConn, testDbName, deleteDependentDbs: true);
					DropTestDatabase(adminConn, anotherCw1SystemTestDb, deleteDependentDbs: true);
					DropLogins(adminConn, nonCw1ManagedLogins);
					DropLogins(adminConn, anotherCw1SystemLogins);
					DropLogins(adminConn, testCw1SqlLogins);
				}
			}
		}

		List<(string username, bool isWindowsUser)> ListOfCw1SqlLogins(string mainDbName)
		{
			var login = ListOfCw1MainConnectionLogins(mainDbName);
			login.Add((DbUserRepository.GetStaffDbLoginFullPrefix(mainDbName) + "test", false));
			login.Add((DbUserRepository.GetStaffDbLoginFullPrefix(mainDbName) + "test2", false));
			return login;
		}

		List<(string username, bool isWindowsUser)> ListOfCw1SqlLoginsWithUniCode(string mainDbName)
		{
			var login = ListOfCw1MainConnectionLogins(mainDbName);
			login.Add((DbUserRepository.GetStaffDbLoginFullPrefix(mainDbName) + "test", false));
			login.Add((DbUserRepository.GetStaffDbLoginFullPrefix(mainDbName) + "王万祥", false));
			return login;
		}

		List<(string username, bool isWindowsUser)> ListOfCw1MainConnectionLogins(string mainDbName)
		{
			return Db.GetAllLoginNames(mainDbName).Select(loginName => (loginName, false)).ToList();
		}

		void CreateLogins(AdminConnection adminConn, string defaultDbName, List<(string username, bool isWindowsUser)> logins)
		{
			DropLogins(adminConn, logins);

			foreach (var login in logins)
			{
				var sqlText = login.isWindowsUser
					? $"CREATE LOGIN [{login.username}] From Windows WITH DEFAULT_DATABASE = [{defaultDbName}]"
					: $"CREATE LOGIN [{login.username}] WITH PASSWORD = 'Temp1234ABCD'";
				adminConn.ExecuteNonQuery(sqlText);
			}
		}

		void DropLogins(AdminConnection adminConn, List<(string username, bool isWindowsUser)> logins)
		{
			foreach (var login in logins)
			{
				var sqlText = $"IF EXISTS (SELECT null FROM master.sys.server_principals WHERE name = N'{login.username}') DROP LOGIN [{login.username}]";
				adminConn.ExecuteNonQuery(sqlText);
			}
		}

		void AssertLoginsExist(AdminConnection adminConn, List<(string username, bool isWindowsUser)> logins, bool isLoginExpected)
		{
			foreach (var login in logins)
			{
				var sqlText = $"SELECT COUNT(*) FROM master.sys.server_principals WHERE name = N'{login.username}' and type = '{(login.isWindowsUser ? 'U' : 'S')}'";
				AssertEquals($"Login [{login.username}] exist?", isLoginExpected, (int)adminConn.ExecuteScalar(sqlText) == 1);
			}
		}

		public void TestDropDatabases_IncludeOperationalDbs()
		{
			var maintenanceManager = new DbMaintenanceManager();

			using (var adminConn = Db.NewAdminConnection())
			{
				DropTestDatabase(adminConn, testDbName, deleteDependentDbs: true);

				try
				{
					using (var tempDir = new TempDirectory())
					{
						CreateTestDatabases(adminConn, testDbName, createDependentDbs: true);

						System.Threading.Thread.Sleep(1001);
						maintenanceManager.DropDatabases(Db.ServerName, testDbName, true, false, false);

						AssertEquals("Cannot drop database " + testDbName, false, CheckDatabaseExist(adminConn, testDbName));
						AssertEquals("Cannot drop database " + testDbName + "_RefDb_Ent_YY", true, CheckDatabaseExist(adminConn, testDbName + "_RefDb_Ent_YY"));
						AssertEquals("Cannot drop database " + testDbName + "_RefDb_Ent_ZZ", true, CheckDatabaseExist(adminConn, testDbName + "_RefDb_Ent_ZZ"));
						AssertEquals("Cannot drop database " + testDbName + "_SD001", false, CheckDatabaseExist(adminConn, testDbName + "_SD001"));
						AssertEquals("Cannot drop database " + testDbName + "_SD254", false, CheckDatabaseExist(adminConn, testDbName + "_SD254"));
					}
				}
				finally
				{
					DropTestDatabase(adminConn, testDbName, deleteDependentDbs: true);
				}
			}
		}

		public void TestDropDatabases_IncludeRefDbs()
		{
			var maintenanceManager = new DbMaintenanceManager();

			using (var adminConn = Db.NewAdminConnection())
			{
				try
				{
					using (var tempDir = new TempDirectory())
					{
						CreateTestDatabases(adminConn, testDbName, createDependentDbs: true);

						System.Threading.Thread.Sleep(1001);
						maintenanceManager.DropDatabases(Db.ServerName, testDbName, false, true, false);

						AssertEquals("Cannot drop database " + testDbName, false, CheckDatabaseExist(adminConn, testDbName));
						AssertEquals("Cannot drop database " + testDbName + "_RefDb_Ent_YY", false, CheckDatabaseExist(adminConn, testDbName + "_RefDb_Ent_YY"));
						AssertEquals("Cannot drop database " + testDbName + "_RefDb_Ent_ZZ", false, CheckDatabaseExist(adminConn, testDbName + "_RefDb_Ent_ZZ"));
						AssertEquals("Cannot drop database " + testDbName + "_SD001", true, CheckDatabaseExist(adminConn, testDbName + "_SD001"));
						AssertEquals("Cannot drop database " + testDbName + "_SD254", true, CheckDatabaseExist(adminConn, testDbName + "_SD254"));
					}
				}
				finally
				{
					DropTestDatabase(adminConn, testDbName, deleteDependentDbs: true);
				}
			}
		}

		public void TestDropDatabases_IncludeBiDbs()
		{
			var maintenanceManager = new DbMaintenanceManager();

			using (var adminConn = Db.NewAdminConnection())
			{
				try
				{
					using (var tempDir = new TempDirectory())
					{
						CreateTestDatabases(adminConn, testDbName, createDependentDbs: true);
						InsertBiServerRegistry(adminConn, testDbName);

						System.Threading.Thread.Sleep(1001);
						maintenanceManager.DropDatabases(Db.ServerName, testDbName, false, false, true);

						AssertEquals("Cannot drop database " + testDbName, false, CheckDatabaseExist(adminConn, testDbName));
						AssertEquals("Cannot drop database " + testDbName + "_Audit", false, CheckDatabaseExist(adminConn, testDbName + "_Audit"));
						AssertEquals("Cannot drop database " + testDbName + "_EDW", false, CheckDatabaseExist(adminConn, testDbName + "_EDW"));
					}
				}
				finally
				{
					DropTestDatabase(adminConn, testDbName, deleteDependentDbs: true);
				}
			}
		}

		public void TestDropDatabases_IncludeBiDbs_NoBiServers()
		{
			var maintenanceManager = new DbMaintenanceManager();

			using (var adminConn = Db.NewAdminConnection())
			{
				try
				{
					using (var tempDir = new TempDirectory())
					{
						CreateTestDatabases(adminConn, testDbName, createDependentDbs: true);

						System.Threading.Thread.Sleep(1001);
						maintenanceManager.DropDatabases(Db.ServerName, testDbName, false, false, true);

						AssertEquals("Cannot drop database " + testDbName, false, CheckDatabaseExist(adminConn, testDbName));
						AssertEquals("Cannot drop database " + testDbName + "_Audit", false, CheckDatabaseExist(adminConn, testDbName + "_Audit"));
						AssertEquals("Cannot drop database " + testDbName + "_EDW", false, CheckDatabaseExist(adminConn, testDbName + "_EDW"));
					}
				}
				finally
				{
					DropTestDatabase(adminConn, testDbName, deleteDependentDbs: true);
				}
			}
		}

		public void TestGetServerDbList()
		{
			var maintenanceManager = new DbMaintenanceManager();
			var dbList = maintenanceManager.GetServerDbList("", true);

			AssertEquals("No databases should be returned if no server name is specified", true, dbList.Length == 0);
		}

		public void TestGetServerDbListShowWarningWhenServerIsUnreachable()
		{
			// Arrange
			const string serverName = "NotReachableServer";
			const string expectedInfo = "Failed to connect to Sql Server. It might be triggered by incorrect Server value. If problem persists, please contact your administrator.";

			var outputErrorMessage = string.Empty;
			var maintenanceManager = new DbMaintenanceManager();
			maintenanceManager.OnTaskFailed += (errorMessage) => outputErrorMessage = errorMessage;

			// Act
			maintenanceManager.GetServerDbList(serverName, true);

			// Assert
			AssertContains(expectedInfo, outputErrorMessage);
		}

		public void TestDropDatabases_IncludeOperationalAndRefDbs()
		{
			var maintenanceManager = new DbMaintenanceManager();

			using (var adminConn = Db.NewAdminConnection())
			{
				DropTestDatabase(adminConn, testDbName, deleteDependentDbs: true);

				try
				{
					using (var tempDir = new TempDirectory())
					{
						CreateTestDatabases(adminConn, testDbName, createDependentDbs: true);

						System.Threading.Thread.Sleep(1001);
						maintenanceManager.DropDatabases(Db.ServerName, testDbName, true, true, false);

						AssertEquals("Cannot drop database " + testDbName, false, CheckDatabaseExist(adminConn, testDbName));
						AssertEquals("Cannot drop database " + testDbName + "_RefDb_Ent_YY", false, CheckDatabaseExist(adminConn, testDbName + "_RefDb_Ent_YY"));
						AssertEquals("Cannot drop database " + testDbName + "_RefDb_Ent_ZZ", false, CheckDatabaseExist(adminConn, testDbName + "_RefDb_Ent_ZZ"));
						AssertEquals("Cannot drop database " + testDbName + "_SD001", false, CheckDatabaseExist(adminConn, testDbName + "_SD001"));
						AssertEquals("Cannot drop database " + testDbName + "_SD254", false, CheckDatabaseExist(adminConn, testDbName + "_SD254"));
					}
				}
				finally
				{
					DropTestDatabase(adminConn, testDbName, deleteDependentDbs: true);
				}
			}
		}

		[UseSnapshotProtection()]
		public void TestDropDatabases_WithUnicodeStaffLogin()
		{
			var maintenanceManager = new DbMaintenanceManager();
			var testCw1SqlLogins = ListOfCw1SqlLoginsWithUniCode(testDbName);

			using (var adminConn = Db.NewAdminConnection())
			{
				try
				{
					// Arrange
					CreateTestDatabases(adminConn, testDbName, createDependentDbs: true);
					CreateLogins(adminConn, testDbName, testCw1SqlLogins);

					AssertLoginsExist(adminConn, testCw1SqlLogins, true);

					// Act
					maintenanceManager.DropDatabases(Db.ServerName, testDbName, false, false, false);

					// Assert
					AssertLoginsExist(adminConn, testCw1SqlLogins, false);
				}
				finally
				{
					DropTestDatabase(adminConn, testDbName, deleteDependentDbs: true);
				}
			}
		}

		[UseSnapshotProtection()]
		public void TestStaffLoginsWithUnicodeCharactersAreDroppedWhenDropDatabases()
		{
			var maintenanceManager = new DbMaintenanceManager();
			var testCw1SqlLogins = ListOfCw1SqlLoginsWithUniCode(testDbName);
			testCw1SqlLogins.Add((DbUserRepository.GetStaffDbLoginFullPrefix(testDbName) + "冯嘉莉", false));
			testCw1SqlLogins.Add((DbUserRepository.GetStaffDbLoginFullPrefix(testDbName) + "叶晓文", false));
			testCw1SqlLogins.Add((DbUserRepository.GetStaffDbLoginFullPrefix(testDbName) + "吴淑玲", false));

			using (var adminConn = Db.NewAdminConnection())
			{
				try
				{
					// Arrange
					CreateTestDatabases(adminConn, testDbName, createDependentDbs: true);
					CreateLogins(adminConn, testDbName, testCw1SqlLogins);

					AssertLoginsExist(adminConn, testCw1SqlLogins, true);

					// Act
					maintenanceManager.DropDatabases(Db.ServerName, testDbName, false, false, false);

					// Assert
					AssertEquals("Cannot drop database " + testDbName, false, CheckDatabaseExist(adminConn, testDbName));
					AssertLoginsExist(adminConn, testCw1SqlLogins, false);
				}
				finally
				{
					DropTestDatabase(adminConn, testDbName, deleteDependentDbs: true);
				}
			}
		}

		public void TestDropDatabaseInNorecoveryState_MainDbOnly()
		{
			var maintenanceManager = new DbMaintenanceManager();

			using (var adminConn = Db.NewAdminConnection())
			{
				try
				{
					RestoreTestDatabaseWithNorecovery();

					using (var tempDir = new TempDirectory())
					{
						maintenanceManager.DropDatabases(Db.ServerName, testDbName, false, false, false);
					}

					AssertEquals(testDbName + " database should have been dropped", false, adminConn.DatabaseExists(testDbName));
					AssertEquals(testDbName + "_RefDb_Ent_YY database should exist", true, adminConn.DatabaseExists(testDbName + "_RefDb_Ent_YY"));
					AssertEquals(testDbName + "_RefDb_Ent_ZZ database should exist", true, adminConn.DatabaseExists(testDbName + "_RefDb_Ent_ZZ"));
					AssertEquals(testDbName + "_SD001 database should exist", true, adminConn.DatabaseExists(testDbName + "_SD001"));
					AssertEquals(testDbName + "_SD254 database should exist", true, adminConn.DatabaseExists(testDbName + "_SD254"));
					AssertEquals(testDbName + "_UserRepository database should exist", true, adminConn.DatabaseExists(testDbName + "_UserRepository"));
				}
				finally
				{
					DropTestDatabase(adminConn, testDbName, deleteDependentDbs: true);
				}
			}
		}

		public void TestDropDatabaseInNorecoveryState_IncludeOperationalAndRefDbs()
		{
			var maintenanceManager = new DbMaintenanceManager();

			using (var adminConn = Db.NewAdminConnection())
			{
				try
				{
					RestoreTestDatabaseWithNorecovery();

					using (var tempDir = new TempDirectory())
					{
						maintenanceManager.DropDatabases(Db.ServerName, testDbName, true, true, false);
					}

					AssertEquals(testDbName + " database should have been dropped", false, adminConn.DatabaseExists(testDbName));
					AssertEquals(testDbName + "_RefDb_Ent_YY database should have been dropped", false, adminConn.DatabaseExists(testDbName + "_RefDb_Ent_YY"));
					AssertEquals(testDbName + "_RefDb_Ent_ZZ database should have been dropped", false, adminConn.DatabaseExists(testDbName + "_RefDb_Ent_ZZ"));
					AssertEquals(testDbName + "_SD001 database should have been dropped", false, adminConn.DatabaseExists(testDbName + "_SD001"));
					AssertEquals(testDbName + "_SD254 database should have been dropped", false, adminConn.DatabaseExists(testDbName + "_SD254"));
					AssertEquals(testDbName + "_UserRepository database should have been dropped", false, adminConn.DatabaseExists(testDbName + "_UserRepository"));
				}
				finally
				{
					DropTestDatabase(adminConn, testDbName, deleteDependentDbs: true);
				}
			}
		}

		public void TestDropDatabaseInNorecoveryState_eDocsOnly()
		{
			var maintenanceManager = new DbMaintenanceManager();

			using (var adminConn = Db.NewAdminConnection())
			{
				try
				{
					RestoreTestDatabaseWithNorecovery();

					using (var tempDir = new TempDirectory())
					{
						maintenanceManager.DropDatabases(Db.ServerName, testDbName, true, false, false);
					}

					AssertEquals(testDbName + " database should have been dropped", false, adminConn.DatabaseExists(testDbName));
					AssertEquals(testDbName + "_RefDb_Ent_YY database should exit", true, adminConn.DatabaseExists(testDbName + "_RefDb_Ent_YY"));
					AssertEquals(testDbName + "_RefDb_Ent_ZZ database should exit", true, adminConn.DatabaseExists(testDbName + "_RefDb_Ent_ZZ"));
					AssertEquals(testDbName + "_SD001 database should have been dropped", false, adminConn.DatabaseExists(testDbName + "_SD001"));
					AssertEquals(testDbName + "_SD254 database should have been dropped", false, adminConn.DatabaseExists(testDbName + "_SD254"));
					AssertEquals(testDbName + "_UserRepository database should have been dropped", false, adminConn.DatabaseExists(testDbName + "_UserRepository"));
				}
				finally
				{
					DropTestDatabase(adminConn, testDbName, deleteDependentDbs: true);
				}
			}
		}

		public void TestDropDatabaseInNorecoveryState_RefDbsOnly()
		{
			var maintenanceManager = new DbMaintenanceManager();

			using (var adminConn = Db.NewAdminConnection())
			{
				try
				{
					RestoreTestDatabaseWithNorecovery();

					using (var tempDir = new TempDirectory())
					{
						maintenanceManager.DropDatabases(Db.ServerName, testDbName, false, true, false);
					}

					AssertEquals(testDbName + " database should have been dropped", false, adminConn.DatabaseExists(testDbName));
					AssertEquals(testDbName + "_RefDb_Ent_YY database should have been dropped", false, adminConn.DatabaseExists(testDbName + "_RefDb_Ent_YY"));
					AssertEquals(testDbName + "_RefDb_Ent_ZZ database should have been dropped", false, adminConn.DatabaseExists(testDbName + "_RefDb_Ent_ZZ"));
					AssertEquals(testDbName + "_SD001 database should exist", true, adminConn.DatabaseExists(testDbName + "_SD001"));
					AssertEquals(testDbName + "_SD254 database should exist", true, adminConn.DatabaseExists(testDbName + "_SD254"));
					AssertEquals(testDbName + "_UserRepository database should exist", true, adminConn.DatabaseExists(testDbName + "_UserRepository"));
				}
				finally
				{
					DropTestDatabase(adminConn, testDbName, deleteDependentDbs: true);
				}
			}
		}

		public void TestDropDatabases_DeleteBackupHistory()
		{
			var maintenanceManager = new DbMaintenanceManager();

			using (var adminConn = Db.NewAdminConnection())
			{
				try
				{
					AdoTestUtils.CreateDbIfNotExists(adminConn, testDbName);

					using (var tempDir = new TempDirectory())
					{
						var backupManager = new DbBackupManager();
						backupManager.BackupDatabases(Db.ServerName, testDbName, tempDir, null, null, includeOperationalDbs: false, includeRefFilesDbs: false, includeBiDbs: false);
						AssertEquals("Cannot create DB backup history", true, CheckDbBackupHistoryExists(adminConn, testDbName));

						maintenanceManager.DropDatabases(Db.ServerName, testDbName, false, false, false);
					}

					AssertEquals("Cannot drop database", false, CheckDatabaseExist(adminConn, testDbName));
					AssertEquals("Cannot delete DB backup history", false, CheckDbBackupHistoryExists(adminConn, testDbName));
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(adminConn, testDbName);
				}
			}
		}

		public void TestDropDatabases_InAvailabilityGroup()
		{
			var alwaysOnHelperMock = new Mock<IAlwaysOnHelper>();
			alwaysOnHelperMock
				.Setup(a => a.IsDbPartOfAlwaysOn(It.IsAny<AdminConnection>(), It.IsAny<string>()))
				.Returns(true);
			alwaysOnHelperMock
				.Setup(a => a.GetSecondaryReplicaNamesList(It.IsAny<AdminConnection>(), It.IsAny<string>()))
				.Returns(() => new List<string> { Db.ServerName });
			alwaysOnHelperMock
				.Setup(a => a.RemoveDatabaseFromAvailabilityGroup(It.IsAny<AdminConnection>(), It.IsAny<string>(), It.IsAny<string>()))
				.Returns(true);

			var databaseWaiterMock = new Mock<IDatabaseWaiter>();

			var maintenanceManagerMock = new Mock<DbMaintenanceManager>();
			maintenanceManagerMock
				.Protected()
				.Setup(
					"CheckAndDropDatabases",
					ItExpr.IsAny<string>(),
					ItExpr.IsAny<string>(),
					ItExpr.IsAny<bool>(),
					ItExpr.IsAny<bool>(),
					ItExpr.IsAny<bool>()
				).CallBase();
			maintenanceManagerMock
				.Protected()
				.Setup(
					"DropDatabase",
					ItExpr.IsAny<AdminConnection>(),
					ItExpr.IsAny<string>(),
					ItExpr.IsAny<string>()
				);

			maintenanceManagerMock.Object.AlwaysOnHelper = alwaysOnHelperMock.Object;
			maintenanceManagerMock.Object.DatabaseWaiter = databaseWaiterMock.Object;

			using (var adminConn = Db.NewAdminConnection())
			{
				try
				{
					CreateTestDatabases(adminConn, testDbName, true);

					AssertNoExceptionThrown("Database is removed from secondary replicas",
						() => maintenanceManagerMock.Object.DropDatabases(
							Db.ServerName,
							testDbName,
							true,
							true,
							true)
					);

					maintenanceManagerMock.Protected()
					.Verify("DropDatabase",
						Times.Exactly(16),
						ItExpr.IsAny<AdminConnection>(),
						ItExpr.IsAny<string>(),
						ItExpr.IsAny<string>()
					);
					alwaysOnHelperMock.Verify(m => m.RemoveDatabaseFromAvailabilityGroup(It.IsAny<AdminConnection>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(8));
				}
				finally
				{
					DropTestDatabase(adminConn, testDbName, deleteDependentDbs: true);
				}
			}
		}

		public void TestDropDatabases_RestoringState()
		{
			var maintenanceManager = new DbMaintenanceManager();

			using (var adminConn = Db.NewAdminConnection())
			using (var tempDir = new TempDirectory())
			{
				try
				{
					AdoTestUtils.CreateDbIfNotExists(adminConn, testDbName, DbRecoveryModel.Full);
					adminConn.ExecuteNonQuery(string.Format(
@"
BACKUP DATABASE [{0}] TO  DISK = N'{1}\{0}.bak'
BACKUP LOG [{0}] TO DISK = '{1}\{0}_Log.ldf' WITH NORECOVERY",
						testDbName, tempDir.DirectoryName));

					maintenanceManager.DropDatabases(Db.ServerName, testDbName, false, false, includeBiDbs: true);
					AssertEquals("Database in recovery state should be dropped if including BI databases", false, CheckDatabaseExist(adminConn, testDbName));

					maintenanceManager.DropDatabases(Db.ServerName, testDbName, false, false, includeBiDbs: false);
					AssertEquals("Cannot drop database", false, CheckDatabaseExist(adminConn, testDbName));
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(adminConn, testDbName);
				}
			}
		}

		public void TestDropDatabases_WithoutSDBinaryColumn()
		{
			var maintenanceManager = new DbMaintenanceManager();

			using (var adminConn = Db.NewAdminConnection())
			{
				using (var tempDir = new TempDirectory())
				using (AdoTestUtils.CreateDbDropExistingDisposable(adminConn, testDbName))
				using (AdoTestUtils.CreateDbDropExistingDisposable(adminConn, testDbName + "_Audit"))
				{
					CreateTestDatabases(adminConn, testDbName, createDependentDbs: false);
					CreateTestDatabases(adminConn, testDbName + "_Audit", createDependentDbs: true);
					CreateStmDataWithoutSDBinaryValue(adminConn, testDbName + "_Audit");

					maintenanceManager.DropDatabases(Db.ServerName, testDbName + "_Audit", false, false, false);

					AssertEquals("Cannot drop database " + testDbName + "_Audit", false, CheckDatabaseExist(adminConn, testDbName + "_Audit"));
				}
			}
		}

		bool CheckDbBackupHistoryExists(AdminConnection connection, string databaseName)
		{
			var result = false;

			var sqlText = string.Format(
			@"IF EXISTS (SELECT null FROM msdb.dbo.backupset WHERE database_name = '{0}')
			BEGIN
				SELECT 1
				RETURN
			END
			SELECT 0",
			databaseName);

			using (var cmd = connection.Command(sqlText, 3600))
			{
				result = (Convert.ToInt32(cmd.ExecuteScalar()) == 1);
			}

			return result;
		}

		void RestoreTestDatabaseWithNorecovery()
		{
			var resManager = new DbRestoreManager(new DbHeaderOnlyReader());

			using (var tempDir = new TempDirectory())
			{
				using (var connection = Db.NewAdminConnection())
				{
					var mainDbFullBackupPath = PrepareBackupFilesForTest(connection, tempDir);
					var dbFiles = resManager.GetBackupDbFileInfoCollection(Db.ServerName, mainDbFullBackupPath, null, null, null, null);

					var mainServerConfig = new DbServerConfiguration(Db.ServerName, mainDbFullBackupPath);
					var auditServerConfig = AuditDbServerConfiguration.Empty;
					var edwServerConfig = EdwDbServerConfiguration.Empty;
					var dbRestoreSettings = new DbRestoreSettings(testDbName, dbFiles, DbRestoreOption.RestoreWithNoRecovery);

					resManager.RestoreDatabases(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);

					Assert(testDbName + " database should exist", connection.DatabaseExists(testDbName));
					Assert(testDbName + "_RefDb_Ent_YY database should exist", connection.DatabaseExists(testDbName + "_RefDb_Ent_YY"));
					Assert(testDbName + "_RefDb_Ent_ZZ database should exist", connection.DatabaseExists(testDbName + "_RefDb_Ent_ZZ"));
					Assert(testDbName + "_SD001 database should exist", connection.DatabaseExists(testDbName + "_SD001"));
					Assert(testDbName + "_SD254 database should exist", connection.DatabaseExists(testDbName + "_SD254"));
					Assert(testDbName + "_UserRepository database should exist", connection.DatabaseExists(testDbName + "_UserRepository"));
				}
			}
		}

		string PrepareBackupFilesForTest(AdminConnection connection, string backupDirectory)
		{
			CreateTestDatabases(connection, testDbName, true);

			var bckManager = new DbBackupManager();
			bckManager.BackupDatabases(Db.ServerName, testDbName, backupDirectory, null, null, includeOperationalDbs: true, includeRefFilesDbs: true, includeBiDbs: false);

			DropTestDatabase(connection, testDbName, deleteDependentDbs: true);

			var mainDbBackupFile = Directory.GetFiles(backupDirectory, "*" + testDbName + ".bak");

			return mainDbBackupFile.Length == 1 ? mainDbBackupFile[0] : string.Empty;
		}

		bool CheckDatabaseExist(AdminConnection conn, string dbName)
		{
			var isExist = false;
			var sqlText = string.Format(
				"SELECT name FROM sys.databases WHERE name = '{0}'",
				dbName);

			using (var cmd = conn.Command(sqlText))
			{
				isExist = cmd.ExecuteScalar() != null;
			}

			return isExist;
		}

		void CreateTestDatabases(AdminConnection conn, string mainDbName, bool createDependentDbs)
		{
			AdoTestUtils.CreateDbIfNotExists(conn, mainDbName);

			if (createDependentDbs)
			{
				AdoTestUtils.CreateDbIfNotExists(conn, mainDbName + "_RefDb_Ent_YY");
				AdoTestUtils.CreateDbIfNotExists(conn, mainDbName + "_RefDb_Ent_ZZ");
				AdoTestUtils.CreateDbIfNotExists(conn, mainDbName + "_SD001");
				AdoTestUtils.CreateDbIfNotExists(conn, mainDbName + "_SD254");
				AdoTestUtils.CreateDbIfNotExists(conn, mainDbName + "_UserRepository");
				AdoTestUtils.CreateDbIfNotExists(conn, mainDbName + "_Audit");
				AdoTestUtils.CreateDbIfNotExists(conn, mainDbName + "_EDW");
			}
		}

		void InsertBiServerRegistry(AdminConnection conn, string dbName)
		{
			var sqlText = string.Format(@"
CREATE TABLE dbo.StmData
( 
		SD_PK UNIQUEIDENTIFIER NOT NULL DEFAULT newid(),
		SD_Name VARCHAR(300) NOT NULL,
		SD_BinaryValue VARBINARY(max) NULL
)
;

INSERT INTO dbo.StmData(SD_PK, SD_Name, SD_BinaryValue) SELECT newid(), 'BiDataWarehouseServer', CONVERT(VARBINARY(MAX), N'{0}')
INSERT INTO dbo.StmData(SD_PK, SD_Name, SD_BinaryValue) SELECT newid(), 'BiAuditServer', CONVERT(VARBINARY(MAX), N'{0}')",
				Db.ServerName);
			using (((ICurrentDbControl)conn).UseDatabase(dbName))
			{
				conn.ExecuteNonQuery(sqlText);
			}
		}

		void CreateStmDataWithoutSDBinaryValue(AdminConnection conn, string dbName)
		{
			var sqlText = string.Format(@"
CREATE TABLE StmData
( 
		SD_PK UNIQUEIDENTIFIER NOT NULL DEFAULT newid(),
		SD_Name VARCHAR(300) NOT NULL
);");

			using (((ICurrentDbControl)conn).UseDatabase(dbName))
			{
				conn.ExecuteNonQuery(sqlText);
			}
		}

		void DropTestDatabase(AdminConnection conn, string dbName, bool deleteDependentDbs)
		{
			AdoTestUtils.DropDbIfExists(conn, dbName);

			if (deleteDependentDbs)
			{
				AdoTestUtils.DropDbIfExists(conn, dbName + "_RefDb_Ent_YY");
				AdoTestUtils.DropDbIfExists(conn, dbName + "_RefDb_Ent_ZZ");
				AdoTestUtils.DropDbIfExists(conn, dbName + "_SD001");
				AdoTestUtils.DropDbIfExists(conn, dbName + "_SD254");
				AdoTestUtils.DropDbIfExists(conn, dbName + "_UserRepository");
				AdoTestUtils.DropDbIfExists(conn, dbName + "_Audit");
				AdoTestUtils.DropDbIfExists(conn, dbName + "_EDW");
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			RefDbTableNameResolver.ResetSingleRefDatabaseName("CW-RefDatabase-ForTest");
		}

		protected override void TearDown()
		{
			RefDbTableNameResolver.ResetSingleRefDatabaseName();

			base.TearDown();
		}

		readonly string testDbName = "DbDropManagerTestDb1";
	}
}
