using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Data;
using CargoWise.DataProtection.Administration.SqlServer;
using Enterprise.AlwaysOn.Setup;
using static System.FormattableString;

namespace Enterprise.AlwaysOn.Testing
{
	class PrimaryServerInstanceTest : AlwaysOnTestFixture
	{
		public void TestAttributes()
		{
			var serverInfo = new SqlServerInfo(Db.ServerName);
			var dbServer = DbServerInstanceFactory.ConnectAndValidatePrimaryServer(serverInfo);

			Assert("ServerInfo should be loaded from the server", serverInfo.DetailsLoaded);

			AssertEquals("IsLoaded?", false, dbServer.IsLoaded);
			AssertEquals("HasErrors?", true, dbServer.HasErrors);
			AssertEquals("LastErrorMessage", $"Server={serverInfo.ServerAlias} - This database server instance is not enabled for AlwaysOn High Availability. Please refer to release notes for instructions on how to enable it.", dbServer.LastErrorMessage);

			AssertNull("SqlServiceAccount", dbServer.SqlServiceAccount);
			AssertNull("EligibleTopLevelDatabases", dbServer.EligibleTopLevelDatabases);
			AssertEquals("AlwaysOnEndpointPort", 0, dbServer.AlwaysOnEndpointPort);

			AssertNull("FailoverCluster", dbServer.FailoverCluster);
			Assert("Test env is not FCI", !dbServer.IsFailoverClusterInstance);
		}

		public void TestEligibleTopLevelDatabases_DbNameWithDot() => AssertEligibleTopLevelDatabasesCore(".OdysseyB0BEBD5F71684B76ABBD70FFA43E550A", isValidName: false);
		public void TestEligibleTopLevelDatabases_DbNameWithComma() => AssertEligibleTopLevelDatabasesCore("Odyssey,B0BEBD5F71684B76ABBD70FFA43E550A", isValidName: false);
		public void TestEligibleTopLevelDatabases_DbNameWithUnderscore() => AssertEligibleTopLevelDatabasesCore("Odyssey_B0BEBD5F71684B76ABBD70FFA43E550A", isValidName: false);
		public void TestEligibleTopLevelDatabases_DbNameWithDash() => AssertEligibleTopLevelDatabasesCore("Odyssey-B0BEBD5F71684B76ABBD70FFA43E550A", isValidName: false);
		public void TestEligibleTopLevelDatabases_DbNameWithQuote() => AssertEligibleTopLevelDatabasesCore("Odyssey'B0BEBD5F71684B76ABBD70FFA43E550A", isValidName: false);
		public void TestEligibleTopLevelDatabases_DbNameAlphaNumericOne() => AssertEligibleTopLevelDatabasesCore("OdysseyENTTSTB0BEBD5F71684B76ABBD70FFA43E550A", isValidName: true);
		public void TestEligibleTopLevelDatabases_DbNameAlphaNumericTwo() => AssertEligibleTopLevelDatabasesCore("OdysseyTestB0BEBD5F71684B76ABBD70FFA43E550A", isValidName: true);
		public void TestEligibleTopLevelDatabases_DbNameAlphaNumericThree() => AssertEligibleTopLevelDatabasesCore("Odyssey123B0BEBD5F71684B76ABBD70FFA43E550A", isValidName: true);

		/// <summary>
		/// Asserts top level databases have no special characters in their name
		/// </summary>
		void AssertEligibleTopLevelDatabasesCore(string databaseName, bool isValidName)
		{
			//Arrange
			var alphaNumericRegex = new Regex(@"^(?!.*['.,_-]).*$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
			AssertEquals("Is database name alphanumeric?", isValidName, alphaNumericRegex.IsMatch(databaseName));

			IEnumerable<IAlwaysOnDatabase> eligibleTopLevelDatabases;
			using var adminConnection = Db.NewAdminConnection();
			using var tempDatabase = AdoTestUtils.CreateDbDropExistingDisposable(adminConnection, databaseName, Db.DatabaseName);

			adminConnection.ExecuteNonQuery(Invariant($"ALTER DATABASE [{databaseName}] SET RECOVERY FULL;"));

			//Act
			var testSqlServer = new SqlServerInfo("My test\\SQL Server");
			var dbServer = new PrimaryServerInstanceForTesting(testSqlServer);
			var sqlContext = Program.SqlContextManager.GetSqlExecutionContext(Db.ServerName);
			eligibleTopLevelDatabases = dbServer.GetEligiblePrimaryTopLevelDatabases_Exposed(sqlContext);

			//Assert
			var dbNames = eligibleTopLevelDatabases.Select(x => x.Name).ToHashSet();
			AssertEquals($@"Should {(isValidName ? "" : "not ")}include db Name: {databaseName}, as the name is {(isValidName ? "valid" : "invalid")}", isValidName, dbNames.Contains(databaseName));
		}

		public void TestAlwaysOnRequiresWindowsServerVersion()
		{
			var serverInfo = new SqlServerInfo(Db.ServerName);

			var dbServer = new PrimaryServerInstanceForTesting(serverInfo);

			var info = GetCurrentInfo(serverInfo);
			dbServer.Load();
			var expected = $"Server={serverInfo.ServerAlias} - This database server instance is not enabled for AlwaysOn High Availability. Please refer to release notes for instructions on how to enable it.";
			var actual = ((IValidationStatus)dbServer).LastErrorMessage;
			info += GetCurrentInfo(serverInfo);
			AssertEquals(info, expected, actual);

			dbServer.WindowsServerVersionForTest = "6.2";
			dbServer.Load();
			expected = $"Server={serverInfo.ServerAlias} - AlwaysOn requires Windows Server 2012 R2 or later version.";
			actual = ((IValidationStatus)dbServer).LastErrorMessage;
			info += GetCurrentInfo(serverInfo);
			AssertEquals(info, expected, actual);
		}

		string GetCurrentInfo(SqlServerInfo serverInfo)
		{
			var info = "ServerNameWithoutInstance: " + Db.Connection.ServerNameWithoutInstance + Environment.NewLine;
			info += "ServerInstanceName: " + Db.Connection.ServerInstanceName + Environment.NewLine;
			info += "serverInfo.FullInstanceAlias: " + serverInfo.ServerAlias + Environment.NewLine;
			if (serverInfo.DetailsLoaded)
			{
				info += "serverInfo.ServerDomain: " + serverInfo.ServerDomain + Environment.NewLine;
			}
			info += Environment.NewLine;
			return info;
		}

		/// <summary>
		/// Asserts enterprise database affixes used in PrimaryServerInstance are
		/// in sync with the database list (CargoWise.Data.DbConnection)
		/// 
		/// -- Note 1: Predicate in GetDatabaseListEnforcingAlwaysOnRequirements
		/// db.[name] = '{0}'
		/// OR db.[name] like '{0}[_]SD[0-9][0-9][0-9]'
		/// OR db.[name] like '{0}[_]RefDb[_]___[_]__'
		/// OR db.[name] like '{0}[_]UserRepository'
		/// 
		/// -- Note 2: Does not call GetDatabaseListEnforcingAlwaysOnRequirements as it changes database settings.
		/// </summary>
		public void TestGetDatabaseListEnforcingAlwaysOnRequirements()
		{
			AssertEquals("StorageDocDbSuffixSqlPattern", "[_]SD[0-9][0-9][0-9]", Db.StorageDocDbSuffixSqlPattern);
			AssertEquals("RefDbAffix", "RefDb", RefDbTableNameResolver.RefDbAffix);
			AssertEquals("RepositoryDbSuffix", "_UserRepository", DbUserRepository.RepositoryDbSuffix);

			var dbList = Db.Connection.GetDatabases(DatabaseType.All & ~DatabaseType.SingleSharedRef);

			AssertEquals("dbList.First", Db.DatabaseName, dbList.First());

			// ^Odyssey_SD\d{3}$
			var edocsRegex = new Regex("^" + Db.DatabaseName + @"_SD[0-9]{3}$", RegexOptions.IgnoreCase);
			// ^Odyssey_RefDb_\w{3}_\w{2}$
			var refdbRegex = new Regex("^" + Db.DatabaseName + "_" + RefDbTableNameResolver.RefDbAffix + @"_\w{3}_\w{2}$", RegexOptions.IgnoreCase);
			// ^Odyssey_UserRepository$
			var repositoryRegex = new Regex("^" + Db.DatabaseName + DbUserRepository.RepositoryDbSuffix + "$", RegexOptions.IgnoreCase);
			// ^Odyssey_Audit$
			var auditRegex = new Regex("^" + Db.DatabaseName + @"_Audit$", RegexOptions.IgnoreCase);
			// ^Odyssey_EDW
			var edwRegex = new Regex("^" + Db.DatabaseName + @"_EDW$", RegexOptions.IgnoreCase);

			foreach (var dbName in dbList.Skip(1))
			{
				if (
					!(edocsRegex.IsMatch(dbName)
					|| refdbRegex.IsMatch(dbName)
					|| repositoryRegex.IsMatch(dbName)
					|| auditRegex.IsMatch(dbName)
					|| edwRegex.IsMatch(dbName))
					)
				{
					Fail("[" + dbName + "] does not match any of the dependent database naming conventions.");
				}
			}
		}

		public void TestGetDatabaseListEnforcingAlwaysOnRequirements_Messaging()
		{
			const string testDbName = "TestGetAlwaysOnSetOfDatabasesGFJHK89723JHG3452AKL1245GTAE3";

			using (AdminConnection conn = Db.NewAdminConnection())
			{
				try
				{
					AdoTestUtils.DropDbIfExists(conn, testDbName);
					AdoTestUtils.CreateDbIfNotExists(conn, testDbName);

					var sqlConn = (conn as IDbConnectionInternals).ADOConnection;

					var testSqlServer = new SqlServerInfo("My test\\SQL Server");
					PrimaryServerInstanceForTesting ps = new PrimaryServerInstanceForTesting(testSqlServer);
					Dictionary<string, string> dbInfo = null;

					// Change RECOVERY, AUTO_CLOSE settings
					conn.ExecuteNonQuery(Invariant($"ALTER DATABASE [{testDbName}] SET RECOVERY SIMPLE;"));
					conn.ExecuteNonQuery(Invariant($"ALTER DATABASE [{testDbName}] SET AUTO_CLOSE ON;"));

					// Recovery model has been changed => requires full backup
					AssertExceptionThrown(typeof(AlwaysOnException)
						, Invariant($"The RECOVERY model for the following databases has been changed to FULL according to the system requirements. Please ensure all databases have a recent full backup.\r\n\r\n{testDbName}")
						, () => ps.GetDatabaseListEnforcingAlwaysOnRequirements_Exposed(new SqlExecutionContext(sqlConn, null), testDbName));

					dbInfo = GetDbInfo(conn, testDbName);
					AssertEquals(dbInfo["recovery_model_desc"], "FULL");
					AssertEquals(dbInfo["is_auto_close_on"], "False");
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(conn, testDbName);
				}
			}
		}

		public void TestGetAlwaysOnSetOfDatabases()
		{
			const string testDbName = "TestGetAlwaysOnSetOfDatabasesGFJHK89723JHG3452AKL1245GTAE3";

			using (AdminConnection conn = Db.NewAdminConnection())
			{
				try
				{
					AdoTestUtils.DropDbIfExists(conn, testDbName);
					AdoTestUtils.CreateDbIfNotExists(conn, testDbName);

					var sqlConn = (conn as IDbConnectionInternals).ADOConnection;

					var testSqlServer = new SqlServerInfo("My test\\SQL Server");
					PrimaryServerInstanceForTesting ps = new PrimaryServerInstanceForTesting(testSqlServer);
					Dictionary<string, string> dbInfo = null;

					// Change RECOVERY, AUTO_CLOSE settings
					conn.ExecuteNonQuery(Invariant($"ALTER DATABASE [{testDbName}] SET RECOVERY SIMPLE;"));
					conn.ExecuteNonQuery(Invariant($"ALTER DATABASE [{testDbName}] SET AUTO_CLOSE ON;"));

					dbInfo = GetDbInfo(conn, testDbName);
					AssertEquals(dbInfo["recovery_model_desc"], "SIMPLE");
					AssertEquals(dbInfo["is_auto_close_on"], "True");

					// Get databases should fix settings
					var databases = ps.GetAlwaysOnSetOfDatabases_Exposed(new SqlExecutionContext(sqlConn, null), testDbName, true).ToList();
					AssertContainsExactElementsInAnyOrder(new string[] { testDbName }, databases.Select(d => d.Name));
					AssertEquals("Recovery model has been changed", true, databases[0].RecoveryModelChanged);

					dbInfo = GetDbInfo(conn, testDbName);
					AssertEquals(dbInfo["recovery_model_desc"], "FULL");
					AssertEquals(dbInfo["is_auto_close_on"], "False");
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(conn, testDbName);
				}
			}
		}

		public void TestSuspendResumeRepolicationOnPrimary()
		{
			var sqlCommandStringList = new List<string>();
			var sqlContext = new SqlExecutionContextForTest(sqlCommandStringList, null);

			const string dbName1 = "TheDatabase1";
			const string dbName2 = "TheDatabase3";
			var testSqlServer = new SqlServerInfo(Db.ServerName);
			var dbServer = new PrimaryServerInstanceForTesting(testSqlServer);

			var contextMock = new SqlExecutionContextForTest(sqlCommandStringList, null);
			dbServer.GetSqlExecutionContextForServerOverride = contextMock;

			dbServer.ResumeReplication(dbName1);
			dbServer.SuspendReplication(dbName2);
			dbServer.SuspendReplication(dbName1);
			dbServer.ResumeReplication(dbName2);
			AssertEquals("Number of executed commands", 4, sqlCommandStringList.Count);
			AssertEquals(Invariant($"ALTER DATABASE [{dbName1}] SET HADR RESUME"), sqlCommandStringList[0]);
			AssertEquals(Invariant($"ALTER DATABASE [{dbName2}] SET HADR SUSPEND"), sqlCommandStringList[1]);
			AssertEquals(Invariant($"ALTER DATABASE [{dbName1}] SET HADR SUSPEND"), sqlCommandStringList[2]);
			AssertEquals(Invariant($"ALTER DATABASE [{dbName2}] SET HADR RESUME"), sqlCommandStringList[3]);
		}

		Dictionary<string, string> GetDbInfo(AdminConnection connection, string dbName)
		{
			Dictionary<string, string> dbInfo = new Dictionary<string, string>();

			string getDbListSql = Invariant($@"
SELECT
	db.recovery_model_desc,
	db.is_auto_close_on
FROM
	sys.databases db where name='{dbName}'
");

			using (var cmd = connection.Command(getDbListSql))
			{
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						dbInfo["recovery_model_desc"] = reader["recovery_model_desc"].ToString();
						dbInfo["is_auto_close_on"] = reader["is_auto_close_on"].ToString();

						return dbInfo;
					}
				}
			}

			return dbInfo;
		}

		class PrimaryServerInstanceForTesting : PrimaryServerInstance
		{
			public PrimaryServerInstanceForTesting(SqlServerInfo serverInfo = null)
				: base(serverInfo)
			{
			}

			public string WindowsServerVersionForTest;

			protected override string GetWindowsServerVersion(ISqlExecutionContext sqlContext)
			{
				return WindowsServerVersionForTest ?? base.GetWindowsServerVersion(sqlContext);
			}

			public IEnumerable<IAlwaysOnDatabase> GetEligiblePrimaryTopLevelDatabases_Exposed(ISqlExecutionContext sqlContext)
			{
				return base.GetEligiblePrimaryTopLevelDatabases(sqlContext);
			}

			public IEnumerable<DatabaseAlwaysOnStatus> GetAlwaysOnSetOfDatabases_Exposed(ISqlExecutionContext sqlContext, string mainDbName, bool alterDbSettingToMeetRequiremtns)
			{
				return base.GetAlwaysOnSetOfDatabases(sqlContext, mainDbName, alterDbSettingToMeetRequiremtns);
			}

			public IEnumerable<string> GetDatabaseListEnforcingAlwaysOnRequirements_Exposed(ISqlExecutionContext sqlContext, string mainDbName)
			{
				return GetDatabaseListEnforcingAlwaysOnRequirements(sqlContext, mainDbName);
			}

			public ISqlExecutionContext GetSqlExecutionContextForServerOverride { get; set; }
			protected override ISqlExecutionContext GetSqlExecutionContextForServer()
			{
				if (GetSqlExecutionContextForServerOverride is not null)
				{
					return GetSqlExecutionContextForServerOverride;
				}
				else
				{
					return base.GetSqlExecutionContextForServer();
				}
			}
		}
	}
}
