using System;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.DataProtection;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	sealed class ConnectionErrorManagerTest : TestCase
	{
		public void TestReconnectIfApplicable()
		{
			using (DbConnection testConnection = Db.NewExtraConnectionToMainDb())
			{
				string connProcId = testConnection.ExecuteScalar("SELECT @@spid").ToString();

				using (DbConnection anotherTestConnection = Db.NewAdminConnection())
				{
					anotherTestConnection.ExecuteNonQuery("KILL " + connProcId);
				}

				AssertEquals("Connection State should still be open from the APPLICATION view", ConnectionState.Open, testConnection.State);
				int numOfTables = (int)testConnection.ExecuteScalar("SELECT count(*) FROM sys.objects");
				Assert("Connection should have been automatically reconnected and the command rerun", numOfTables > 0);
			}
		}

		[UseSnapshotProtection]
		public void TestCanRepair_ReaderUserDoesNotExist_LoginDoes()
		{
			var dbName = RefDbTableNameResolver.GetSharedAvailabilityGroupRefDbPrefix(nameof(TestCanRepair_ReaderUserDoesNotExist_LoginDoes), RefDbTypeEnum.Enterprise, "ZZ");
			var tableName = nameof(TestCanRepair_ReaderUserDoesNotExist_LoginDoes);
			var synonym = $"RefDb{tableName}";
			var loginName = GetDefaultReaderLoginName();
			AssertContains("PRE: Expecting some kind of reader login", "Read", loginName);

			using (var connection = Db.NewAdminConnection())
			using (AdoTestUtils.CreateDbDropExistingDisposable(dbName, Db.DatabaseName))
			{
				var readerLogin = connection.Logins.Single(l => l.LoginName == loginName);
				using (((ICurrentDbControl)connection).UseDatabase(dbName))
				{
					AddDummyTable(connection, tableName);

					readerLogin.EnsureLoginHasRightsToCurrentDatabase();
					readerLogin.DropUserFromDatabase(dbName);
				}

				using (CreateSynonym(connection, synonym, dbName, tableName))
				{
					Assert("PRE: The login exists, even though the user has been deleted", LoginExists(connection, readerLogin));
					AssertNotNull(Db.Connection.ExecuteScalar($"SELECT COUNT(*) FROM {synonym} {DbCommand.ExecuteAsReaderFlagComments}"));
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

		string GetDefaultReaderLoginName()
			=> Db.Connection.ExecuteScalar<string>($"SELECT CURRENT_USER {DbCommand.ExecuteAsReaderFlagComments}");

		bool LoginExists(AdminConnection connection, DatabaseLogin login)
			=> connection.Exists($"FROM master.sys.sql_logins WHERE name='{login.LoginName}'");

		public void TestReconnectIfApplicableCore()
		{
			ConnectionErrorManager testConnection = new ConnectionErrorManager(new DbReconnectionHandlingMock());
			bool successful = testConnection.ReconnectIfApplicable(new InvalidOperationException());
			Assert(!successful);
		}

		public void TestReconnectIfApplicableReconnectsIfInATransactionButCommandIsNotReissued()
		{
			using (DbConnection testConnection = Db.NewExtraConnectionToMainDb())
			{
				string connProcId = testConnection.ExecuteScalar("SELECT @@spid").ToString();
				testConnection.BeginTransaction();

				using (DbConnection anotherTestConnection = Db.NewAdminConnection())
				{
					anotherTestConnection.ExecuteNonQuery("KILL " + connProcId);
				}

				AssertEquals("Connection transaction count from the APPLICATION perspective", 1, testConnection.AppTransactionCount);
				AssertEquals("Connection State should still be open from the APPLICATION perspective", ConnectionState.Open, testConnection.State);

				try
				{
					testConnection.ExecuteNonQuery("--");
					Fail("Should throw exception");
				}
				catch (SqlException e)
				{
					AssertEquals("Wrong Exception caught - " + e.Message, DbErrorType.GeneralNetworkError, new DbErrorMatch(e).ExceptionType);
					AssertEquals("Connection State should be CLOSED from the APPLICATION perspective", ConnectionState.Closed, testConnection.State);
				}
			}
		}

		public void TestCallingConnectionErrorManagerManually_RestrictedWriterLogin()
		{
			using (var testConnection = new ConnectionThatDoesNotHandleDisconnectionsForTest())
			{
				CallConnectionErrorManager(testConnection);
			}
		}

		public void TestCallingConnectionErrorManagerManually_IntegratedSecurity()
		{
			using (var testConnection = new IntegratedSecurityConnectionThatDoesNotHandleDisconnectionsForTest(Db.ServerName, Db.DatabaseName))
			{
				CallConnectionErrorManager(testConnection);
			}
		}

		static void CallConnectionErrorManager(DbConnection testConnection)
		{
			string connProcId = testConnection.ExecuteScalar("SELECT @@spid").ToString();

			using (DbConnection anotherTestConnection = Db.NewAdminConnection())
			{
				anotherTestConnection.ExecuteNonQuery("KILL " + connProcId);
			}

			try
			{
				testConnection.ExecuteNonQuery("--");
				Fail("Should throw exception");
			}
			catch (SqlException e)
			{
				AssertEquals("Wrong Exception caught - " + e.Message, DbErrorType.GeneralNetworkError, new DbErrorMatch(e).ExceptionType);
				AssertEquals("Connection State from the APPLICATION view should be CLOSED", ConnectionState.Closed, testConnection.State);

				var testConnectionErrorManager = new ConnectionErrorManager(testConnection);
				testConnectionErrorManager.ReconnectIfApplicable(e);
			}

			AssertEquals("Connection State from the APPLICATION view should be OPEN again", ConnectionState.Open, testConnection.State);
			int numOfTables = (int)testConnection.ExecuteScalar("SELECT count(*) FROM sys.objects");
			Assert("Connection should have been automatically reconnected and the command rerun", numOfTables > 0);
		}

		public void TestHandleDisconnectionAndSecurityErrors()
		{
			AssertHandleDisconnectionAndSecurityErrors(Db.DatabaseName + "_SD000");
		}

		[UseSnapshotProtection(new[] { DatabaseType.BI })]
		public void TestHandleDisconnectionAndSecurityErrorsInBiDatabase()
		{
			AssertHandleDisconnectionAndSecurityErrors(Db.AuditDatabaseName);
			AssertHandleDisconnectionAndSecurityErrors(Db.EdwDatabaseName);
		}

		void AssertHandleDisconnectionAndSecurityErrors(string testDbName)
		{
			string testSql = String.Format(
				CultureInfo.InvariantCulture,
				"SELECT count(*) FROM [{0}].sys.database_principals WHERE name = '{1}'",
				testDbName,
				RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName));

			string dropUserSql = String.Format(
				CultureInfo.InvariantCulture,
				"IF (({0}) = 1) EXEC [{1}]..sp_executesql N'DROP USER [{2}]'",
				testSql,
				testDbName,
				RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName)
			);

			using (var testAdminConn = Db.NewAdminConnection())
			{
				bool dbExistedBefore = testAdminConn.DatabaseExists(testDbName);

				try
				{
					string sqlText = (dbExistedBefore)
						? dropUserSql
						: String.Format(CultureInfo.InvariantCulture, "CREATE DATABASE [{0}]", testDbName);
					testAdminConn.ExecuteNonQuery(sqlText);

					//
					// Test without transaction

					AssertEquals(
						"Not in a transaction => Should handle exception, fix user mapping, and re-run command. AppLogin user exists?",
						true, (int)Db.Connection.ExecuteScalar(testSql) == 1);

					//
					// Test in transaction

					testAdminConn.ExecuteNonQuery(dropUserSql);

					Db.Connection.BeginTransaction();

					try
					{
						Db.Connection.ExecuteNonQuery(testSql);
						Fail("Should throw exception because it's in a transaction context");
					}
					catch (SqlException ex)
					{
						AssertEquals(
							"Caught Exception: " + ex.Message + " / Number: " + ex.Number.ToString(),
							DbErrorType.LoginIsNotAbleToAccessDatabaseUnderCurrentSecurityContext,
							new DbErrorMatch(ex).ExceptionType);
					}
					finally
					{
						Db.Connection.RollbackTransaction();
					}

					AssertEquals("User mapping should be fixed. Login user exists?", true, (int)Db.Connection.ExecuteScalar(testSql) == 1);
				}
				finally
				{
					if (!dbExistedBefore || testDbName.EndsWith("SD000", StringComparison.OrdinalIgnoreCase))
					{
						AdoTestUtils.DropDbIfExists(testAdminConn, testDbName);
					}
				}
			}
		}

		public void TestHandleDisconnectionAndSecurityErrorsFixesPermissionDeniedOnObject()
		{
			AssertHandleDisconnectionAndSecurityErrorsFixesPermissionDeniedOnObject(Db.DatabaseName + "_SD000");
		}

		[UseSnapshotProtection(new[] { DatabaseType.BI })]
		public void TestHandleDisconnectionAndSecurityErrorsFixesPermissionDeniedOnObjectInBiDatabase()
		{
			AssertHandleDisconnectionAndSecurityErrorsFixesPermissionDeniedOnObject(Db.AuditDatabaseName);
			AssertHandleDisconnectionAndSecurityErrorsFixesPermissionDeniedOnObject(Db.EdwDatabaseName);
		}

		void AssertHandleDisconnectionAndSecurityErrorsFixesPermissionDeniedOnObject(string testDbName)
		{
			Guid testGuid = Guid.NewGuid();

			string dropUserSql = String.Format(
				CultureInfo.InvariantCulture,
				"IF EXISTS(SELECT 1 FROM [{0}].sys.database_principals WHERE name = '{1}') EXEC [{0}]..sp_executesql N'DROP USER [{1}]'",
				testDbName,
				RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName)
			);

			string dropTestTableSql = String.Format(CultureInfo.InvariantCulture,
				"IF EXISTS(SELECT 1 FROM [{0}].sys.tables WHERE name = '~T1') EXEC [{0}]..sp_executesql N'DROP TABLE dbo.[~T1]';",
				testDbName
			);

			using (var testAdminConn = Db.NewAdminConnection())
			{
				bool dbExistedBefore = testAdminConn.DatabaseExists(testDbName);

				try
				{
					string sqlText = (dbExistedBefore)
						? dropTestTableSql
						: String.Format(CultureInfo.InvariantCulture, "CREATE DATABASE [{0}]", testDbName);
					testAdminConn.ExecuteNonQuery(sqlText);
					testAdminConn.ExecuteNonQuery(dropUserSql);

					using (((ICurrentDbControl)testAdminConn).UseDatabase(testDbName))
					{
						testAdminConn.ExecuteNonQuery("CREATE TABLE [~T1] (Col1 uniqueidentifier)");

						sqlText = String.Format(CultureInfo.InvariantCulture, "INSERT [~T1] VALUES ('{0}')", testGuid.ToString());
						testAdminConn.ExecuteNonQuery(sqlText);

						sqlText = String.Format(CultureInfo.InvariantCulture, "CREATE USER [{0}] FOR LOGIN [{0}]", RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName));
						testAdminConn.ExecuteNonQuery(sqlText);
					}

					string getTestGuidSql = String.Format(CultureInfo.InvariantCulture, "SELECT Col1 FROM [{0}]..[~T1]", testDbName);

					//
					// Test without transaction

					AssertEquals(
						"(not in a transaction => should handle exception, fix user mapping, and re-run command) TestGuid =",
						testGuid, (Guid)Db.Connection.ExecuteScalar(getTestGuidSql));
					DatabaseLoginTest.AssertUserIsMemberOfRole(testAdminConn, testDbName, RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName), "cwRestrictedWriterRole");

					//
					// Test in transaction

					string revokeDbOwnerSql = String.Format(
						CultureInfo.InvariantCulture,
						"EXEC [{0}]..sp_executesql N'ALTER ROLE [cwRestrictedWriterRole] DROP MEMBER [{1}]'",
						testDbName,
						RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName));
					testAdminConn.ExecuteNonQuery(revokeDbOwnerSql);
					DatabaseLoginTest.AssertUserIsMemberOfRole(testAdminConn, testDbName, RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName), "cwRestrictedWriterRole", expected: false);

					Db.Connection.BeginTransaction();

					try
					{
						Db.Connection.ExecuteNonQuery(getTestGuidSql);
						Fail("Should throw exception because it's in a transaction context");
					}
					catch (SqlException ex)
					{
						AssertEquals(
							"Caught Exception: " + ex.Message + " / Number: " + ex.Number.ToString(),
							DbErrorType.PermissionDeniedOnObject,
							new DbErrorMatch(ex).ExceptionType);
					}
					finally
					{
						Db.Connection.RollbackTransaction();
					}

					DatabaseLoginTest.AssertUserIsMemberOfRole(testAdminConn, testDbName, RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName), "cwRestrictedWriterRole");
				}
				finally
				{
					if (dbExistedBefore && !testDbName.EndsWith("SD000", StringComparison.OrdinalIgnoreCase))
					{
						testAdminConn.ExecuteNonQuery(dropTestTableSql);
					}
					else
					{
						AdoTestUtils.DropDbIfExists(testAdminConn, testDbName);
					}
				}
			}
		}

		public void TestHandleDisconnectionAndSecurityErrorsFixesPermissionDeniedToDatabase()
		{
			AssertHandleDisconnectionAndSecurityErrorsFixesPermissionDeniedToDatabase(Db.DatabaseName + "_SD000");
		}

		[UseSnapshotProtection(new[] { DatabaseType.BI })]
		public void TestHandleDisconnectionAndSecurityErrorsFixesPermissionDeniedInBiDatabase()
		{
			AssertHandleDisconnectionAndSecurityErrorsFixesPermissionDeniedToDatabase(Db.AuditDatabaseName);
			AssertHandleDisconnectionAndSecurityErrorsFixesPermissionDeniedToDatabase(Db.EdwDatabaseName);
		}

		void AssertHandleDisconnectionAndSecurityErrorsFixesPermissionDeniedToDatabase(string testDbName)
		{
			Guid testGuid = Guid.NewGuid();

			string dropUserSql = String.Format(CultureInfo.InvariantCulture,
				"IF EXISTS(SELECT 1 FROM [{0}].sys.database_principals WHERE name = '{1}') EXEC [{0}]..sp_executesql N'DROP USER [{1}]';",
				testDbName,
				RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName)
			);

			string dropTestTableSql = String.Format(CultureInfo.InvariantCulture,
				"IF EXISTS(SELECT 1 FROM [{0}].sys.tables WHERE name = '~T1') EXEC [{0}]..sp_executesql N'DROP TABLE dbo.[~T1]';",
				testDbName
			);

			using (var testAdminConn = Db.NewAdminConnection())
			{
				bool dbExistedBefore = testAdminConn.DatabaseExists(testDbName);

				try
				{
					string sqlText = (dbExistedBefore)
						? dropTestTableSql
						: String.Format(CultureInfo.InvariantCulture, "CREATE DATABASE [{0}]", testDbName);
					testAdminConn.ExecuteNonQuery(sqlText);
					testAdminConn.ExecuteNonQuery(dropUserSql);

					using (((ICurrentDbControl)testAdminConn).UseDatabase(testDbName))
					{
						sqlText = String.Format(
							CultureInfo.InvariantCulture,
							"CREATE USER [{0}] FOR LOGIN [{0}]",
							RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName));
						testAdminConn.ExecuteNonQuery(sqlText);
					}

					DatabaseLoginTest.AssertUserIsMemberOfRole(testAdminConn, testDbName, RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName), "cwRestrictedWriterRole", expected: false);

					string createTableSql = String.Format(
						CultureInfo.InvariantCulture,
						"EXEC [{0}]..sp_executesql N'CREATE TABLE dbo.[~T1] (Col1 bit)'",
						testDbName);
					Db.Connection.ExecuteNonQuery(createTableSql);

					AssertEquals("Table [~T1] exists?", true, DataUtils.ObjectExists(testAdminConn, testDbName + ".dbo.[~T1]"));
					DatabaseLoginTest.AssertUserIsMemberOfRole(testAdminConn, testDbName, RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName), "cwRestrictedWriterRole", expected: true);
				}
				finally
				{
					if (dbExistedBefore && !testDbName.EndsWith("SD000", StringComparison.OrdinalIgnoreCase))
					{
						testAdminConn.ExecuteNonQuery(dropTestTableSql);
					}
					else
					{
						AdoTestUtils.DropDbIfExists(testAdminConn, testDbName);
					}
				}
			}
		}

		public void TestHandleDisconnectionAndSecurityErrorsFixesCannotExecuteAsDatabasePrincipalError()
		{
			string testDb = Db.DatabaseName + "_SD000";
			Guid testGuid = Guid.NewGuid();

			using (var testAdminConn = Db.NewAdminConnection())
			{
				try
				{
					AdoTestUtils.DropDbIfExists(testAdminConn, testDb);
					testAdminConn.ExecuteNonQuery(String.Format(CultureInfo.InvariantCulture, "CREATE DATABASE [{0}]", testDb));

					using (((ICurrentDbControl)testAdminConn).UseDatabase(testDb))
					{
						((IDbLoginRepair)testAdminConn).EnsureWriterDbLoginHasRightsToCurrentDatabase();
					}

					DatabaseLoginTest.AssertUserIsMemberOfRole(testAdminConn, testDb, RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName), "cwRestrictedWriterRole", expected: true);
					DatabaseLoginTest.AssertLoginIsMappedToDatabase(testAdminConn, testDb, ((IDbLoginRepair)testAdminConn).ReaderDbLoginName, expected: false);

					using (var testWritterConn = Db.NewExtraConnectionToMainDb())
					using (((ICurrentDbControl)testWritterConn).UseDatabase(testDb))
					{
						AssertEquals("User context (before execute as)",
							RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName),
							testWritterConn.ExecuteScalar("SELECT SUSER_NAME()").ToString());

						try
						{
							string executeAsReaderSql = String.Format(
								CultureInfo.InvariantCulture,
								"EXECUTE AS USER = '{0}'",
								((IDbLoginRepair)testAdminConn).ReaderDbLoginName);
							testWritterConn.ExecuteNonQuery(executeAsReaderSql);

							AssertEquals("User context (after execute as)",
								((IDbLoginRepair)testAdminConn).ReaderDbLoginName,
								testWritterConn.ExecuteScalar("SELECT SUSER_NAME()").ToString());
						}
						finally
						{
							testWritterConn.ExecuteNonQuery("REVERT");
						}
					}

					DatabaseLoginTest.AssertLoginIsMappedToDatabase(testAdminConn, testDb, ((IDbLoginRepair)testAdminConn).ReaderDbLoginName, expected: true);
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(testAdminConn, testDb);
				}
			}
		}

		public void TestReconnectIfApplicableDoesNotReconnectIfStillInProcessOfConnecting()
		{
			using (var testConn = new ConnectionWithConnectionErrorForTesting())
			{
				try
				{
					testConn.EnsureIsOpen();
					Fail("Should throw an exception!");
				}
				catch (SqlException ex)
				{
					AssertEquals("Is the exception a GeneralNetworkError?\r\n" + ex.Message, true, (new DbErrorMatch(ex).ExceptionType == DbErrorType.GeneralNetworkError));
				}
			}
		}

		class ConnectionWithConnectionErrorForTesting : DbConnection<RestrictedWriterLoginCredentials>
		{
			public ConnectionWithConnectionErrorForTesting() : base()
			{
			}

			public override string UserLogin => RestrictedWriterLoginCredentials.UserNameFor(fInitialDatabaseName);

			protected override void RunTasksAfterOpenConnection()
			{
				base.RunTasksAfterOpenConnection();
				AdoTestUtils.KillConnection(this);
				this.EnsureIsOpen();
			}
		}

		class DbReconnectionHandlingMock : IDbReconnectionHandling
		{
			public void CloseAndReopenConnection()
			{
				throw new CloseReopenWhileConnectingException();
			}

			public ConnectionState State
			{
				get { return ConnectionState.Closed; }
			}

			public int AppTransactionCount
			{
				get { return 0; }
			}

			public bool IsConnecting
			{
				get { return false; }
			}

			public string ImpersonatedLogin
			{
				get { throw new NotImplementedException(); }
			}

			public string LoginName
			{
				get { throw new NotImplementedException(); }
			}

			public bool HasDbSchemaOrScriptOrTransformationVersionChanged()
			{
				throw new NotImplementedException();
			}

			public IDisposable UseDatabase(string database)
			{
				throw new NotImplementedException();
			}

			public string ServerName
			{
				get { return "testserver"; }
			}

			public string InitialDatabase
			{
				get { return "database"; }
			}
		}
	}
}
