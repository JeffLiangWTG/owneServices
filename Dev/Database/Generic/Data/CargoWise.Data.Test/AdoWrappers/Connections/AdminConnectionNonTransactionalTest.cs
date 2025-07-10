using System;
using System.Globalization;
using System.Text;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data.SqlServer.Testing;
using CargoWise.DataProtection.TestFramework;

using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	public class AdminConnectionNonTransactionalTest : TestCase
	{
		public void TestEnsureDbLoginsCorrectlyMappedToAllDatabases()
		{
			string testEdocsDb = Db.DatabaseName + "_SD001";

			using (var testAdminConn = Db.NewAdminConnection())
			{
				AdoTestUtils.DropDbIfExists(testAdminConn, testEdocsDb);

				try
				{
					string sqlText = string.Format("CREATE DATABASE [{0}]", testEdocsDb);
					testAdminConn.ExecuteNonQuery(sqlText);

					//
					// Test with an unmatching SID user

					var builder1 = new StringBuilder();
					((IDbLoginRepair)testAdminConn).DropDbLoginUsersFromDatabase(testEdocsDb, msg => builder1.AppendLine(msg));
					sqlText = string.Format(
						CultureInfo.InvariantCulture,
						"EXEC [{0}]..sp_executesql N'CREATE USER {1} WITHOUT LOGIN; " +
						"CREATE USER {2} WITHOUT LOGIN; " +
						"CREATE USER {3} WITHOUT LOGIN; " +
						"CREATE USER {4} WITHOUT LOGIN;'",
						testEdocsDb,
						((IDbLoginRepair)testAdminConn).ReaderDbLoginName,
						((IDbLoginRepair)testAdminConn).RestrictedReaderDbLoginName,
						((IDbLoginRepair)testAdminConn).RestrictedWriterDbLoginName,
						((IDbLoginRepair)testAdminConn).UnrestrictedWriterDbLoginName);
					testAdminConn.ExecuteNonQuery(sqlText);

					DbSecurityTest.AssertLogEntries(builder1.ToString(),
						string.Format($"User [{Db.DatabaseName}_CargoWiseReaderLogin] is dropped from database [{testEdocsDb}]."),
						string.Format($"User [{Db.DatabaseName}_CargoWiseWriterLogin] is dropped from database [{testEdocsDb}]."),
						string.Format($"User [{Db.DatabaseName}_RestrictedReaderLogin] is dropped from database [{testEdocsDb}]."),
						string.Format($"User [{Db.DatabaseName}_RestrictedWriterLogin] is dropped from database [{testEdocsDb}]."),
						string.Format($"User [{Db.DatabaseName}_UnrestrictedWriterLogin] is dropped from database [{testEdocsDb}]."));

					AssertAllLoginsAreMappedToDatabase(testAdminConn, testEdocsDb, expected: false);

					var builder2 = new StringBuilder();
					((IDbLoginRepair)testAdminConn).EnsureDbLoginsCorrectlyMappedToAllDatabases(msg => builder2.AppendLine(msg));

					DbSecurityTest.AssertLogEntries(builder2.ToString(),
						string.Format($"Db Role(s) [cwReaderRole, db_datareader] created and permissions granted on database [{testEdocsDb}]."),
						string.Format($"Db User [{Db.DatabaseName}_CargoWiseReaderLogin] recreated on database [{testEdocsDb}]."),
						string.Format($"Db Role(s) [cwRestrictedReaderRole] created and permissions granted on database [{testEdocsDb}]"),
						string.Format($"Db User [{Db.DatabaseName}_RestrictedReaderLogin] recreated on database [{testEdocsDb}]."),
						string.Format($"Db Role(s) [cwRestrictedWriterRole] created and permissions granted on database [{testEdocsDb}]."),
						string.Format($"Db User [{Db.DatabaseName}_RestrictedWriterLogin] recreated on database [{testEdocsDb}]."),
						string.Format($"Db Role(s) [cwUnrestrictedWriterRole] created and permissions granted on database [{testEdocsDb}]."),
						string.Format($"Db User [{Db.DatabaseName}_UnrestrictedWriterLogin] recreated on database [{testEdocsDb}]."));

					AssertAllLoginsAreMappedToDatabase(testAdminConn, testEdocsDb);

					//
					// Test with no mapped user at all

					var builder3 = new StringBuilder();
					((IDbLoginRepair)testAdminConn).DropDbLoginUsersFromDatabase(testEdocsDb, msg => builder3.AppendLine(msg));

					DbSecurityTest.AssertLogEntries(builder3.ToString(),
						string.Format($"User [{Db.DatabaseName}_CargoWiseReaderLogin] is dropped from database [{testEdocsDb}]."),
						string.Format($"User [{Db.DatabaseName}_CargoWiseWriterLogin] is dropped from database [{testEdocsDb}]."),
						string.Format($"User [{Db.DatabaseName}_RestrictedReaderLogin] is dropped from database [{testEdocsDb}]."),
						string.Format($"User [{Db.DatabaseName}_RestrictedWriterLogin] is dropped from database [{testEdocsDb}]."),
						string.Format($"User [{Db.DatabaseName}_UnrestrictedWriterLogin] is dropped from database [{testEdocsDb}]."));

					AssertAllLoginsAreMappedToDatabase(testAdminConn, testEdocsDb, expected: false);

					var builder4 = new StringBuilder();
					((IDbLoginRepair)testAdminConn).EnsureDbLoginsCorrectlyMappedToAllDatabases(msg => builder4.AppendLine(msg));

					DbSecurityTest.AssertLogEntries(builder2.ToString(),
						string.Format($"Db Role(s) [cwReaderRole, db_datareader] created and permissions granted on database [{testEdocsDb}]."),
						string.Format($"Db User [{Db.DatabaseName}_CargoWiseReaderLogin] recreated on database [{testEdocsDb}]."),
						string.Format($"Db Role(s) [cwRestrictedReaderRole] created and permissions granted on database [{testEdocsDb}]"),
						string.Format($"Db User [{Db.DatabaseName}_RestrictedReaderLogin] recreated on database [{testEdocsDb}]."),
						string.Format($"Db Role(s) [cwRestrictedWriterRole] created and permissions granted on database [{testEdocsDb}]."),
						string.Format($"Db User [{Db.DatabaseName}_RestrictedWriterLogin] recreated on database [{testEdocsDb}]."),
						string.Format($"Db Role(s) [cwUnrestrictedWriterRole] created and permissions granted on database [{testEdocsDb}]."),
						string.Format($"Db User [{Db.DatabaseName}_UnrestrictedWriterLogin] recreated on database [{testEdocsDb}]."));

					AssertAllLoginsAreMappedToDatabase(testAdminConn, testEdocsDb);
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(testAdminConn, testEdocsDb);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestEnsureDbLoginsCorrectlyMappedToAllDatabases_FixLogins()
		{
			var testDbName = RefDbTableNameResolver.GetSharedAvailabilityGroupRefDbPrefix(nameof(TestEnsureDbLoginsCorrectlyMappedToAllDatabases_FixLogins), RefDbTypeEnum.Enterprise, "AA");
			var tableName = nameof(TestEnsureDbLoginsCorrectlyMappedToAllDatabases_FixLogins);
			var synonym = $"RefDb{tableName}";

			using (var testAdminConn = Db.NewAdminConnection())
			using (AdoTestUtils.CreateDbDropExistingDisposable(testDbName, Db.DatabaseName))
			{
				using (((ICurrentDbControl)testAdminConn).UseDatabase(testDbName))
				{
					AddDummyTable(testAdminConn, tableName);
				}

				using (CreateSynonym(testAdminConn, synonym, testDbName, tableName))
				{
					foreach (var login in testAdminConn.Logins)
					{
						if (!(login is RestrictedWriterDatabaseLogin))
						{
							testAdminConn.ExecuteNonQuery($@"IF EXISTS(SELECT name FROM sys.server_principals WHERE name = '{login.LoginName}') DROP LOGIN[{login.LoginName}]");
						}
					}

					((IDbLoginRepair)testAdminConn).DropDbLoginUsersFromDatabase(testDbName, msg => { });

					var sb = new StringBuilder();
					((IDbLoginRepair)testAdminConn).EnsureDbLoginsCorrectlyMappedToAllDatabases(msg => sb.AppendLine(msg));

					DbSecurityTest.AssertLogEntries(sb.ToString(),
						string.Format($"Login type [RestrictedReader] was checked on server."),
						string.Format($"Db Role(s) [cwReaderRole, db_datareader] created and permissions granted on database [{testDbName}]."),
						string.Format($"Db User [{Db.DatabaseName}_CargoWiseReaderLogin] recreated on database [{testDbName}]."),

						string.Format($"Login type [RestrictedWriter] was checked on server."),
						string.Format($"Db Role(s) [cwUnrestrictedWriterRole] created and permissions granted on database [{testDbName}]."),
						string.Format($"Db User [{Db.DatabaseName}_CargoWiseWriterLogin] recreated on database [{testDbName}]."),

						string.Format($"Login type [UnrestrictedWriter] was checked on server."),
						string.Format($"Db Role(s) [cwRestrictedReaderRole] created and permissions granted on database [{testDbName}]"),
						string.Format($"Db User [{Db.DatabaseName}_RestrictedReaderLogin] recreated on database [{testDbName}]."),

						string.Format($"Login type [Reader] was checked on server."),
						string.Format($"Db Role(s) [cwRestrictedWriterRole] created and permissions granted on database [{testDbName}]."),
						string.Format($"Db User [{Db.DatabaseName}_RestrictedWriterLogin] recreated on database [{testDbName}]."),

						string.Format($"Login type [Writer] was checked on server."),
						string.Format($"Db Role(s) [cwUnrestrictedWriterRole] created and permissions granted on database [{testDbName}]."),
						string.Format($"Db User [{Db.DatabaseName}_UnrestrictedWriterLogin] recreated on database [{testDbName}]."));

					AssertAllLoginsAreMappedToDatabase(testAdminConn, testDbName);
				}
			}
		}

		IDisposable CreateSynonym(AdminConnection connection, string synonymName, string dbName, string tableName)
		{
			if (connection.Exists($"FROM sys.synonyms WHERE name='{synonymName}'"))
			{
				DropSynonym();
			}

			connection.ExecuteNonQuery($"CREATE SYNONYM [{synonymName}] FOR [{dbName}]..[{tableName}]");
			return new DisposableAction(DropSynonym);

			void DropSynonym()
				=> connection.ExecuteNonQuery($"DROP SYNONYM [{synonymName}];");
		}

		void AddDummyTable(AdminConnection connection, string tablename)
			=> connection.ExecuteNonQuery($"CREATE TABLE {tablename} (pk uniqueidentifier not null primary key, c char);");

		public void TestEnsureDbLoginsHaveRightsToCurrentDatabase()
		{
			var testEdocsDb = Db.DatabaseName + "_SDasdfasdf";

			using (var testAdminConn = Db.NewAdminConnection())
			{
				AdoTestUtils.DropDbIfExists(testAdminConn, testEdocsDb);

				try
				{
					var sqlText = string.Format("CREATE DATABASE [{0}]", testEdocsDb);
					testAdminConn.ExecuteNonQuery(sqlText);

					using (var testAdminConn2 = Db.NewAdminConnection(testEdocsDb))
					{
						sqlText = string.Format(
							CultureInfo.InvariantCulture,
							"EXEC [{0}]..sp_executesql N'CREATE USER {1} WITHOUT LOGIN; " +
							"CREATE USER {2} WITHOUT LOGIN; " +
							"CREATE USER {3} WITHOUT LOGIN; " +
							"CREATE USER {4} WITHOUT LOGIN;'",
							testEdocsDb,
							((IDbLoginRepair)testAdminConn2).ReaderDbLoginName,
							((IDbLoginRepair)testAdminConn2).RestrictedReaderDbLoginName,
							((IDbLoginRepair)testAdminConn2).RestrictedWriterDbLoginName,
							((IDbLoginRepair)testAdminConn2).UnrestrictedWriterDbLoginName);
						testAdminConn2.ExecuteNonQuery(sqlText);

						((IDbLoginRepair)testAdminConn2).EnsureDbLoginsHaveRightsToCurrentDatabase();

						DatabaseLoginTest.AssertLoginIsEnabled(testAdminConn2, ((IDbLoginRepair)testAdminConn2).ReaderDbLoginName, expected: true);
						DatabaseLoginTest.AssertLoginIsEnabled(testAdminConn2, ((IDbLoginRepair)testAdminConn2).RestrictedReaderDbLoginName, expected: true);
						DatabaseLoginTest.AssertLoginIsEnabled(testAdminConn2, ((IDbLoginRepair)testAdminConn2).RestrictedWriterDbLoginName, expected: true);
						DatabaseLoginTest.AssertLoginIsEnabled(testAdminConn2, ((IDbLoginRepair)testAdminConn2).UnrestrictedWriterDbLoginName, expected: true);

						DatabaseLoginTest.AssertUserIsMemberOfRole(testAdminConn2, testAdminConn2.CurrentDatabase, ((IDbLoginRepair)testAdminConn2).ReaderDbLoginName, "cwReaderRole");
						DatabaseLoginTest.AssertUserIsMemberOfRole(testAdminConn2, testAdminConn2.CurrentDatabase, ((IDbLoginRepair)testAdminConn2).ReaderDbLoginName, "db_datareader");
						DatabaseLoginTest.AssertUserIsMemberOfRole(testAdminConn2, testAdminConn2.CurrentDatabase, ((IDbLoginRepair)testAdminConn2).RestrictedReaderDbLoginName, "cwRestrictedReaderRole");
						DatabaseLoginTest.AssertUserIsMemberOfRole(testAdminConn2, testAdminConn2.CurrentDatabase, ((IDbLoginRepair)testAdminConn2).RestrictedWriterDbLoginName, "cwRestrictedWriterRole");
						DatabaseLoginTest.AssertUserIsMemberOfRole(testAdminConn2, testAdminConn2.CurrentDatabase, ((IDbLoginRepair)testAdminConn2).UnrestrictedWriterDbLoginName, "cwUnrestrictedWriterRole");
					}
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(testAdminConn, testEdocsDb);
				}
			}
		}

		public static void AssertAllLoginsAreMappedToDatabase(AdminConnection connection, string dbName, bool expected = true)
		{
			DatabaseLoginTest.AssertLoginIsMappedToDatabase(connection, dbName, ((IDbLoginRepair)connection).ReaderDbLoginName, expected);
			DatabaseLoginTest.AssertLoginIsMappedToDatabase(connection, dbName, ((IDbLoginRepair)connection).RestrictedReaderDbLoginName, expected);
			DatabaseLoginTest.AssertLoginIsMappedToDatabase(connection, dbName, ((IDbLoginRepair)connection).RestrictedWriterDbLoginName, expected);
			DatabaseLoginTest.AssertLoginIsMappedToDatabase(connection, dbName, ((IDbLoginRepair)connection).UnrestrictedWriterDbLoginName, expected);
		}

		#region ServerSID

		public void TestServerSID()
		{
			string sqlMask = "SELECT name FROM sys.server_principals WHERE convert(uniqueidentifier, sid) = '{0}'";
			string actualLoginName;
			using (var adminConnection = Db.NewAdminConnection())
			{
				actualLoginName = adminConnection.ExecuteScalar(string.Format(sqlMask, AdminConnection.ServerSid.ToString())).ToString();
			}
			var expectedLoginName = DataProtectionTestBed.Current.GetExpectedCredentials(TestEnvironmentWellKnownSecret.TestServerOdysseyAdmin).UserName;
			AssertEquals("Server SID caclculated from user ...", expectedLoginName, actualLoginName);
		}

		[ExpectNoExceptions]
		public void TestServerSID_ThreadSafe()
		{
			using var cancellationTokenSource = new CancellationTokenSource();
			var action = new ServerSidAbortSafeLoopRunner().RunInLoop;
			ThreadSafeAccessTestCase.RunTestOnMultipleThreads(action, 10, ThreadSafeAccessTestCase.EndThreadTestAction.Abort, 100);
		}

		class ServerSidAbortSafeLoopRunner : ThreadSafeAccessTestCase.AbortSafeLoopRunner
		{
			protected override void Execute(CancellationToken token)
			{
			}
		}

		#endregion

		public void TestGetSqlLoginHashedPassword()
		{
			var username = "SqlLoginDD649F13FF784D019BB95E7B599D895E";
			var password = "AComplexPasswor#$%^152";

			using (var connection = Db.NewAdminConnection())
			{
				try
				{
					AdoTestUtils.DropDbLoginIfExists(connection, username);

					connection.ExecuteNonQuery($"Create LOGIN [{username}] WITH PASSWORD = '{password}', DEFAULT_DATABASE=[master], DEFAULT_LANGUAGE=[us_english], CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF");
					var vl = connection.GetSqlLoginHashedPassword(username);

					AdoTestUtils.DropDbLoginIfExists(connection, username);
					connection.ExecuteNonQuery($"Create LOGIN [{username}] WITH PASSWORD = {vl} HASHED, DEFAULT_DATABASE=[master], DEFAULT_LANGUAGE=[us_english], CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF");

					Assert((bool)connection.ExecuteScalar($"Select CAST(PWDCOMPARE('{password}', password_hash) as bit) PassCompare From sys.sql_logins Where name = '{username}'"));
				}
				finally
				{
					AdoTestUtils.DropDbLoginIfExists(connection, username);
				}
			}
		}

		public void TestNewAdminConnection_ServerNameIsProvided_CanCreateWithoutDBInitialization()
		{
			var serverName = Db.ServerName;
			var databaseName = Db.DatabaseName;
			using (Db.ClearServerDetailsTemporarily())
			{
				using var connection = Db.NewAdminConnection(serverName, databaseName);
				var result = connection.ExecuteScalar("Select 'OK'") as string;
				AssertEquals("OK", result);
			}
		}
	}
}
