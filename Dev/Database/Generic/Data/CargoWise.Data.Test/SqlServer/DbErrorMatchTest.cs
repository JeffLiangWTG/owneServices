using System;
using System.Data;
using System.Globalization;
using System.IO;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	sealed class DbErrorMatchTest : TransactionedTestCase
	{
		public void TestTransportLevelError()
		{
			var dbErrorMatch = new DbErrorMatch(SqlExceptionBuilder.CreateSqlException(10054,
				"A transport-level error has occurred when sending the request to the server. (provider: TCP Provider, error: 0 - An existing connection was forcibly closed by the remote host.)"
				));
			AssertEquals(DbErrorType.GeneralNetworkError, dbErrorMatch.ExceptionType);
			Assert(dbErrorMatch.IsInfrastructureDbError);
		}

		public void TestIsDbLoginError()
		{
			var dbLoginErrorTypes = new[]
			{
				DbErrorType.LoginDisabled,
				DbErrorType.CannotOpenDbRequestedInLogin,
				DbErrorType.LoginFailedBecauseItIsLockedOut,
				DbErrorType.SqlServerAuthenticationModeIsOff,
			};

			foreach (var dbLoginErrorType in dbLoginErrorTypes)
			{
				Assert(DbErrorMatch.IsDbLoginError(dbLoginErrorType));
			}
		}

		/// <summary>
		/// Cannot alter the role 'cwReaderRole', because it does not exist or you do not have permission.
		/// </summary>
		public void TestDatabasePrincipalDoesNotExist()
		{
			ExecuteAndAssert("DROP ROLE [A role which does not exist]", DbErrorType.DatabasePrincipalDoesNotExist);
			ExecuteAndAssert("ALTER ROLE [A role which does not exist] ADD MEMBER [any]", DbErrorType.DatabasePrincipalDoesNotExist);
		}

		public void TestDatabasePrincipalAlreadyExists()
		{
			ExecuteAndAssert("CREATE ROLE [cwReaderRole]", DbErrorType.DatabasePrincipalAlreadyExists);
		}

		public void TestGetIndexNameIfUniqueIndexViolation_ValidException()
		{
			string sqlText = "INSERT dbo.GlbGroup (GG_PK, GG_Code, GG_Desc) VALUES (NEWID(), 'ALL', 'ANYTHING')";
			DbErrorMatch testHandler = ExecuteAndAssert(sqlText, DbErrorType.CannotInsertDuplicateUniqueIndexKey);
			AssertEquals("NR_UX__GG_Code", testHandler.GetIndexNameIfUniqueIndexViolation());
		}

		public void TestBadSecurityContextCanDetermineUser()
		{
			var error = SqlExceptionBuilder.CreateSqlError(916, byte.MaxValue, byte.MinValue, TestConnection.ServerName, @"The server principal ""Odyssey_CargoWiseReaderLogin"" is not able to access the database ""CW-RefDb-Ent-ZZ-000201"" under the current security context.", "", 0);
			var errors = SqlExceptionBuilder.CreateSqlErrorCollection(error);
			var exception = SqlExceptionBuilder.CreateSqlException(errors);
			var match = new DbErrorMatch(exception);

			AssertEquals("Odyssey_CargoWiseReaderLogin", match.GetPrincipalFromError());
		}

		public void TestGetIndexNameIfUniqueIndexViolation_InvalidException()
		{
			string sqlText =
				"INSERT dbo.RefCountryStates (RW_PK, RW_Code, RW_Description, RW_RN_NKCountryCode) VALUES " +
				" (NEWID(), '***', 'TestInvalidFKReference', '')";

			DbErrorMatch testHandler = ExecuteAndAssert(sqlText, DbErrorType.InsertConflictedWithCheckConstraint);
			AssertEquals("", testHandler.GetIndexNameIfUniqueIndexViolation());
		}

		public void TestLogonFailedForUntrustedDomain()
		{
			var error = SqlExceptionBuilder.CreateSqlError(18452, byte.MaxValue, byte.MinValue, TestConnection.ServerName, "Login failed. The login is from an untrusted domain and cannot be used with Windows authentication.", "", 0);
			var errors = SqlExceptionBuilder.CreateSqlErrorCollection(error);
			var exception = SqlExceptionBuilder.CreateSqlException(errors);

			var errorHandler = new DbErrorHandler(exception, TestConnection);
			AssertEquals("LoginFailedForUntrustedDomain", DbErrorType.LoginFailedForUntrustedDomain, errorHandler.ExceptionType);

			var errorMatch = new DbErrorMatch(exception);
			string friendlyMessage = errorMatch.GetUserFriendlyMessage(TestConnection);
			AssertEquals("Try again later.\r\n" + exception.Message, friendlyMessage);
		}

		public void TestQueryProcessorCouldNotProduceQueryPlanBecauseOfHints()
		{
			var error = SqlExceptionBuilder.CreateSqlError(8622, byte.MaxValue, byte.MinValue, Db.Connection.ServerName, "Query processor could not produce a query plan because of the hints defined in this query. Resubmit the query without specifying any hints and without using SET FORCEPLAN.", "", 0);
			var errors = SqlExceptionBuilder.CreateSqlErrorCollection(error);
			var exception = SqlExceptionBuilder.CreateSqlException(errors);

			var errorHandler = new DbErrorHandler(exception, Db.Connection);
			Assert("Did not correctly match the 8622 error.", errorHandler.ExceptionType == DbErrorType.QueryProcessorCouldNotProduceQueryPlanBecauseOfHints);
		}

		public void TestRanOutOfResourcesBecauseOfQueryPlan()
		{
			// Doing a mock-y test because the real thing would be far too slow.
			var error = SqlExceptionBuilder.CreateSqlError(8623, byte.MaxValue, byte.MinValue, Db.Connection.ServerName, "Ran out of resources... you've executed a terrible query.", "", 0);
			var errors = SqlExceptionBuilder.CreateSqlErrorCollection(error);
			var exception = SqlExceptionBuilder.CreateSqlException(errors);

			var errorHandler = new DbErrorHandler(exception, Db.Connection);
			Assert("Did not correctly match the 8623 error.", errorHandler.ExceptionType == DbErrorType.RanOutOfInternalResources);
		}

		public void TestGhostRecordsBeingDeleted()
		{
			var error = SqlExceptionBuilder.CreateSqlError(3948, 1, 1, Db.Connection.ServerName, "Transaction Terminated", "", 1);
			var errors = SqlExceptionBuilder.CreateSqlErrorCollection(error);
			var exception = SqlExceptionBuilder.CreateSqlException(errors);

			var errorHandler = new DbErrorHandler(exception, Db.Connection);
			Assert("Did not correctly match the  3948 error.", errorHandler.ExceptionType == DbErrorType.GhostRecordsBeingDeleted);
		}

		public void TestMediumOnDeviceHasNotExpiredAndCannotBeOverwritten()
		{
			var error = SqlExceptionBuilder.CreateSqlError(4030, 1, 1, Db.Connection.ServerName, "Log backup with retaindays", "", 1);
			var errors = SqlExceptionBuilder.CreateSqlErrorCollection(error);
			var exception = SqlExceptionBuilder.CreateSqlException(errors);

			var errorHandler = new DbErrorHandler(exception, Db.Connection);
			Assert("Did not correctly match the  4030 error.", errorHandler.ExceptionType == DbErrorType.MediumOnDeviceHasNotExpiredAndCannotBeOverwritten);
		}

		public void TestCannotInitOleDbDataSourceObjForLinkedServer()
		{
			var error = SqlExceptionBuilder.CreateSqlError(7303, 1, 1, Db.Connection.ServerName, "Cannot initialize the data source object of OLE DB provider", "", 1);
			var errors = SqlExceptionBuilder.CreateSqlErrorCollection(error);
			var exception = SqlExceptionBuilder.CreateSqlException(errors);

			var errorHandler = new DbErrorHandler(exception, Db.Connection);
			Assert("Did not correctly match the 7303 error.", errorHandler.ExceptionType == DbErrorType.CannotInitOleDbDataSourceObjForLinkedServer);
		}

		public void TestErrorOnUserDefinedRoutineOrAggregate()
		{
			var error = SqlExceptionBuilder.CreateSqlError(6522, 1, 1, Db.Connection.ServerName, "A.NET Framework error occurred during execution of user - defined routine or aggregate", "", 1);
			var errors = SqlExceptionBuilder.CreateSqlErrorCollection(error);
			var exception = SqlExceptionBuilder.CreateSqlException(errors);

			var errorHandler = new DbErrorHandler(exception, Db.Connection);
			Assert("Did not correctly match the 6522 error.", errorHandler.ExceptionType == DbErrorType.ErrorOnUserDefinedRoutineOrAggregate);
		}

		public void TestTCPProviderConnectionAttemptFailed()
		{
			var error = SqlExceptionBuilder.CreateSqlError(10060, 1, 1, Db.Connection.ServerName, "TCP Provider: A connection attempt failed because the connected party did not properly respond after a period of time, or established connection failed because connected host has failed to respond", "", 1);
			var errors = SqlExceptionBuilder.CreateSqlErrorCollection(error);
			var exception = SqlExceptionBuilder.CreateSqlException(errors);

			var errorHandler = new DbErrorHandler(exception, Db.Connection);
			Assert("Did not correctly match the 10060 error.", errorHandler.ExceptionType == DbErrorType.TCPProviderConnectionAttemptFailed);
		}

		public void TestSynonymRefersToAnInvalidObject()
		{
			string sqlText = @"CREATE SYNONYM BadSynonym FOR [SomeNonExistingTable-333B4005-81FF-4DF0-9031-7B597450D63A];
							SELECT * FROM BadSynonym;";

			var expectedFriendlyMessage = @"Synonym 'BadSynonym' refers to an invalid object.
Please contact your system administrator in order to fix this issue by going to Help > Database Administration > Recreate Database Synonyms.";

			ExecuteAndAssertGetUserFriendlyMessage(sqlText, DbErrorType.SynonymRefersToAnInvalidObject, expectedFriendlyMessage);
		}

		#region FK

		public void TestInsertConflictedWithForeignKey()
		{
			string sqlText =
				"INSERT dbo.RefCountryStates (RW_PK, RW_Code, RW_Description, RW_RN_NKCountryCode) VALUES " +
				"(NEWID(), '***', 'TestInvalidFKReference', '')";

			DbErrorMatch testHandler = ExecuteAndAssert(sqlText, DbErrorType.InsertConflictedWithCheckConstraint);
			string testUserFriendlyMessage = testHandler.GetUserFriendlyMessage(TestConnection);
			AssertEquals("Cannot import to table 'RefCountryStates' because constraint 'Constraint_RN_NKCountryCode' failed on column 'RN_NKCountryCode' - (len([RN_NKCountryCode])=(2)).", testUserFriendlyMessage);
		}

		public void TestUpdateConflictedWithForeignKey()
		{
			string sqlText =
				"UPDATE dbo.RefCountryStates" +
				" SET RW_RN_NKCountryCode = ''" +
				" WHERE RW_PK = (SELECT TOP 1 RW_PK FROM dbo.RefCountryStates)";

			DbErrorMatch testHandler = ExecuteAndAssert(sqlText, DbErrorType.UpdateConflictedWithCheckConstraint);
			string testUserFriendlyMessage = testHandler.GetUserFriendlyMessage(TestConnection);
			AssertEquals("Cannot import to table 'RefCountryStates' because constraint 'Constraint_RN_NKCountryCode' failed on column 'RN_NKCountryCode' - (len([RN_NKCountryCode])=(2)).", testUserFriendlyMessage);
		}

		public void TestDeleteConflictedWithForeignKey()
		{
			string sqlText = "DELETE dbo.GlbCompany WHERE GC_Code = @Code";
			DbErrorMatch testHandler = ExecuteAndAssert(sqlText, DbErrorType.DeleteConflictedWithForeignKey,
														cmd => cmd.AddParameterBasedOnDbColumn("@Code", "DEM", GlbCompanySchema.GC_Code));
			string testUserFriendlyMessage = testHandler.GetUserFriendlyMessage(TestConnection);
			string glbCompanyResourceDesc = DbErrorMatch.GetTableDescriptiveName("GlbCompany");
			Assert("User-friendly message [" + testUserFriendlyMessage + "] should not be empty and should contain [" + glbCompanyResourceDesc + "].", testUserFriendlyMessage.IndexOf(glbCompanyResourceDesc) >= 0);
		}

		[ExpectNoExceptions]
		public void TestGetUserFriendlyMessageReturnOriginalMessage_WhenTheForeignKeyNotFound()
		{
			string errMessage = $@"The DELETE statement conflicted with the REFERENCE constraint ""AccChargeCode_AC_GC_FK2_GlbCompany_RRR_120N"". The conflict occurred in database ""{TestConnection.CurrentDatabase}"", table ""dbo.AccChargeCode"", column 'AC_GC'.
The statement has been terminated.";
			string sqlText = "DELETE dbo.GlbCompany WHERE GC_Code = @Code";
			DbErrorMatch testHandler = ExecuteAndAssert(sqlText, DbErrorType.DeleteConflictedWithForeignKey,
														cmd => cmd.AddParameterBasedOnDbColumn("@Code", "DEM", GlbCompanySchema.GC_Code));
			string dropSql = "alter table dbo.AccChargeCode drop constraint AccChargeCode_AC_GC_FK2_GlbCompany_RRR_120N";
			TestConnection.ExecuteNonQuery(dropSql);
			string testUserFriendlyMessage = testHandler.GetUserFriendlyMessage(TestConnection);
			AssertEquals("Original exception message return.", errMessage, testUserFriendlyMessage);
		}

		#endregion

		#region Check Constraint

		public void TestInsertConflictedWithCheckConstraint()
		{
			Guid orgPk = GetPkFromRandomRowInTable("OrgHeader", "OH_PK");

			string sqlText =
				"INSERT dbo.OrgAddress (OA_PK, OA_Address1, OA_OH) " +
				"VALUES (NEWID(), '', '" + orgPk + "')";

			DbErrorMatch testHandler = ExecuteAndAssert(sqlText, DbErrorType.InsertConflictedWithCheckConstraint);
			string testUserFriendlyMessage = testHandler.GetUserFriendlyMessage(TestConnection);
			AssertEquals("Cannot import to table 'OrgAddress' because constraint 'Constraint_Address1' failed on column 'Address1' - (len([ADDRESS1])>(0)).", testUserFriendlyMessage);
		}

		public void TestUpdateConflictedWithCheckConstraint()
		{
			string sqlText =
				"UPDATE dbo.OrgAddress" +
				" SET OA_Address1 = ''" +
				" WHERE OA_PK = (SELECT TOP 1 OA_PK FROM dbo.OrgAddress)";

			DbErrorMatch testHandler = ExecuteAndAssert(sqlText, DbErrorType.UpdateConflictedWithCheckConstraint);
			string testUserFriendlyMessage = testHandler.GetUserFriendlyMessage(TestConnection);
			AssertEquals("Cannot import to table 'OrgAddress' because constraint 'Constraint_Address1' failed on column 'Address1' - (len([ADDRESS1])>(0)).", testUserFriendlyMessage);
		}

		#endregion

		#region Duplicate Key

		public void TestCannotInsertDuplicateUniqueIndexKey()
		{
			string sqlText = "INSERT dbo.GlbGroup (GG_PK, GG_Code, GG_Desc) VALUES (NEWID(), 'ALL', 'ANYTHING')";
			DbErrorMatch testHandler = ExecuteAndAssert(sqlText, DbErrorType.CannotInsertDuplicateUniqueIndexKey);
			string testUserFriendlyMessage = testHandler.GetUserFriendlyMessage(TestConnection);
			AssertEquals("User-friendly message is correct", "The value of Group Code must be unique on Group. The duplicate value(s) are: (ALL).", testUserFriendlyMessage);
		}

		public void TestCannotInsertDuplicateConstraintKey()
		{
			var groupPK = GetPkFromRandomRowInTable("GlbGroup", "GG_PK");
			var sqlText = "INSERT dbo.GlbGroup (GG_PK, GG_Code, GG_Desc) VALUES ('" + groupPK + "','NEW', 'NEW')";

			var testHandler = ExecuteAndAssert(sqlText, DbErrorType.CannotInsertDuplicateConstraintKey);
			var testMessage = testHandler.GetUserFriendlyMessage(TestConnection);
			var expectedMessage =
@"Another record already exists in database with the same primary key. Please cancel current edit and enter data again if you cannot find that record.
The value of GlbGroup|GG_PK must be unique on Group. The duplicate value(s) are: (" + groupPK + ").";

			AssertEquals("User-friendly message is correct", expectedMessage, testMessage);
		}

		public void TestCannotInsertDuplicateUniqueIndex_WithEnumerator()
		{
			var orgPK = GetPkFromRandomRowInTable("OrgHeader", "OH_PK");
			var pk1 = Guid.NewGuid();
			var pk2 = Guid.NewGuid();

			var sql1 = $"insert into dbo.OrgContact (OC_PK, OC_OH, OC_ContactName, OC_Email) values ('{pk1}', '{orgPK}', 'Tony Stark (5)', 'tony1@marvel.com')";
			var sql2 = $"insert into dbo.OrgContact (OC_PK, OC_OH, OC_ContactName, OC_Email) values ('{pk2}', '{orgPK}', 'Tony Stark (5)', 'tony2@marvel.com')";

			TestConnection.Command(sql1).ExecuteNonQuery();
			var testHandler = ExecuteAndAssert(sql2, DbErrorType.CannotInsertDuplicateUniqueIndexKey);
			var testMessage = testHandler.GetUserFriendlyMessage(TestConnection);
			var expectedMessage = $"The value of Contact Name + Organization must be unique on Contact. The duplicate value(s) are: (Tony Stark (5), {orgPK}).";

			AssertEquals("User-friendly message is correct", expectedMessage, testMessage);
		}

		#endregion

		public void TestObjectAlreadyExists()
		{
			string sqlText = "CREATE TABLE dbo.GlbGroup (Col1 int null)";
			ExecuteAndAssert(sqlText, DbErrorType.ObjectAlreadyExists);
		}

		/// <summary>
		///	Note:
		///		There are two messages in sysmessages with identical text: Invalid column name '%.*ls'.
		///		Number 207 seems to be the only one that we want at this stage.
		/// </summary>
		public void TestInvalidColumnName()
		{
			string sqlText = "SELECT [A field name which does not exist] FROM sys.objects";
			ExecuteAndAssert(sqlText, DbErrorType.InvalidColumnName);
		}

		public void TestInvalidObjectName()
		{
			string sqlText = "SELECT * FROM [A table name which does not exist]";
			ExecuteAndAssertGetUserFriendlyMessage(sqlText, DbErrorType.InvalidObjectName, string.Empty);

			sqlText = @"SELECT * FROM RefDatabase_NotExists;";
			string expectedFriendlyMessage = @"Invalid object name 'RefDatabase_NotExists'.
Please contact your system administrator in order to fix this issue by going to Help > Database Administration > Recreate Database Synonyms.";
			ExecuteAndAssertGetUserFriendlyMessage(sqlText, DbErrorType.InvalidObjectName, expectedFriendlyMessage);

			sqlText = @"SELECT * FROM RefDbOthXX_NotExists;";
			expectedFriendlyMessage = @"Invalid object name 'RefDbOthXX_NotExists'.
Please contact your system administrator in order to fix this issue by going to Help > Database Administration > Recreate Database Synonyms.";
			ExecuteAndAssertGetUserFriendlyMessage(sqlText, DbErrorType.InvalidObjectName, expectedFriendlyMessage);
		}

		public void TestRollBackTranHasNoCorrespondingBeginTran()
		{
			try
			{
				AnotherDbConnection.ExecuteNonQuery("ROLLBACK TRANSACTION");
				Fail();
			}
			catch (SqlException e)
			{
				AssertEquals("Wrong Exception caught - " + e.Message, DbErrorType.RollBackTranHasNoCorrespondingBeginTran, new DbErrorMatch(e).ExceptionType);
			}
		}

		public void TestCannotAlterDbWhileInUse()
		{
			using (DbCommand cmd = AnotherDbConnection.Command(""))
			{
				try
				{
					cmd.CommandText = "ALTER DATABASE " + Db.DatabaseName + " SET SINGLE_USER WITH NO_WAIT";
					cmd.ExecuteNonQuery();
					Fail("Should throw exception");
				}
				catch (SqlException e)
				{
					AssertEquals("Wrong Exception caught - " + e.Message, DbErrorType.CannotAlterDbWhileInUse, new DbErrorMatch(e).ExceptionType);
				}
				finally
				{
					cmd.CommandText = "ALTER DATABASE " + Db.DatabaseName + " SET MULTI_USER";
					cmd.ExecuteNonQuery();
				}
			}
		}

		public void TestSqlNativeError121DoesNotOverlapWithConnectionProtocolError121()
		{
			try
			{
				TestConnection.ExecuteNonQuery("INSERT dbo.RefCountry (RN_PK) SELECT newid(), 'q'");
				Fail("Should throw exception");
			}
			catch (SqlException e)
			{
				AssertEquals("Error Number", 121, e.Number);
				AssertEquals("Error Line Number", 1, e.LineNumber);
				AssertEquals("Should not be one of handled errors - " + e.Message, DbErrorType.NotHandled, new DbErrorMatch(e).ExceptionType);
			}
		}

		public void TestOnlyUserProcessesCanBeKilled()
		{
			using (var anotherConnection = Db.NewAdminConnection())
			using (var cmd = anotherConnection.Command("SELECT TOP(1) session_id FROM sys.dm_exec_sessions WHERE is_user_process = 0"))
			{
				var processToKill = Convert.ToInt32(cmd.ExecuteScalar());

				try
				{
					cmd.CommandText = "KILL " + processToKill.ToString();
					cmd.ExecuteNonQuery();
					Fail("Should throw exception");
				}
				catch (SqlException e)
				{
					AssertEquals("Wrong Exception caught - " + e.Message, DbErrorType.OnlyUserProcessesCanBeKilled, new DbErrorMatch(e).ExceptionType);
				}
			}
		}

		/// <summary>
		/// Opens a new connection to the database, explicitly kills it and tries to execute another
		/// command. This will cause the General Network Error.
		/// </summary>
		public void TestGeneralNetworkError()
		{
			using (ConnectionThatDoesNotHandleDisconnectionsForTest connBeingKilled = new ConnectionThatDoesNotHandleDisconnectionsForTest())
			{
				try
				{
					AdoTestUtils.KillConnection(connBeingKilled);
					connBeingKilled.ExecuteNonQuery("--");
					Fail("Should throw exception");
				}
				catch (SqlException e)
				{
					AssertEquals("Wrong Exception caught - " + e.Message, DbErrorType.GeneralNetworkError, new DbErrorMatch(e).ExceptionType);
				}
			}
		}

		/// <summary>
		/// Attempt to open a connection to an invalid database server to cause
		/// a "SQL Server does not exist or access denied" exception.
		/// The Connection Timeout is 1sec, but it's still regarded as as slow test.
		/// </summary>
		[SnailTest()]
		public void TestServerDoesNotExist()
		{
			try
			{
				ConnectionWithTinyTimeoutForTest connToInvalidServer = new ConnectionWithTinyTimeoutForTest("InvalidServerName", Db.DatabaseName);
				connToInvalidServer.EnsureIsOpen();
				Fail("Should throw exception");
			}
			catch (SqlException e)
			{
				AssertEquals("Wrong Exception caught - " + e.Message + " Exception Number " + e.Number, DbErrorType.ServerDoesNotExist, new DbErrorMatch(e).ExceptionType);
			}
		}

		[SnailTest()]
		public void TestServerIsBeingUpgraded()
		{
			try
			{
				ServerInScriptModeForTest connToServer = new ServerInScriptModeForTest();
				connToServer.Connect();
				Fail("Should throw exception");
			}
			catch (SqlException e)
			{
				AssertEquals("Wrong Exception caught - " + e.Message + " Exception Number " + e.Number, DbErrorType.ServerIsInScriptMode, new DbErrorMatch(e).ExceptionType);
			}
		}

		/// <summary>
		/// Uses a select command with a tiny timeout in another connection to catch a Timeout Expired exception
		/// while the main conection has a lock on the selected row.
		/// </summary>
		[SnailTest()]
		public void TestTimeoutExpired()
		{
			string sqlText = "UPDATE dbo.RefCountry SET RN_IsActive = 0 WHERE RN_Code = @Code";
			TestConnection.ExecuteNonQuery(sqlText, cmd => cmd.AddParameter("@Code", SqlDbType.Char, 2, "AU"));

			try
			{
				var command = AnotherDbConnection.Command("SELECT RN_IsActive FROM dbo.RefCountry WITH (UPDLOCK) WHERE RN_Code = @Code", 1);
				command.AddParameter("@Code", SqlDbType.Char, 2, "AU");
				command.ExecuteNonQuery();

				Fail("Should throw exception");
			}
			catch (SqlException e)
			{
				AssertEquals("Wrong Exception caught - " + e.Message, DbErrorType.TimeoutExpired, new DbErrorMatch(e).ExceptionType);
			}
		}

		/// <summary>
		/// Runs Uses a select command with a lock timeout in another connection to catch a Timeout Expired exception
		/// while the main conection has a lock on the selected row.
		/// </summary>
		public void TestLockTimeoutExpired()
		{
			var sql = "UPDATE dbo.RefCountry SET RN_IsActive = 0 WHERE RN_Code = @Code";
			TestConnection.ExecuteNonQuery(sql, cmd => cmd.AddParameter("@Code", SqlDbType.Char, 2, "AU"));

			try
			{
				sql = @"SELECT RN_IsActive FROM dbo.RefCountry WITH (UPDLOCK) WHERE RN_Code = @Code;";
				using (var conn = Db.NewExtraConnectionToMainDb())
				using (var temp = conn.TemporarySetLockTimeout(0))
				using (var cmd = conn.Command(sql))
				{
					cmd.AddParameter("@Code", SqlDbType.Char, 2, "AU");
					cmd.ExecuteNonQuery();
				}

				Fail("Should throw exception");
			}
			catch (SqlException e)
			{
				AssertEquals("Wrong Exception caught - " + e.Message, DbErrorType.LockTimeoutExpired, new DbErrorMatch(e).ExceptionType);
			}
		}

		/// <summary>
		/// Runs Uses a DBCC CHECKTABLE command with a lock timeout in another connection to catch a Lock Timeout Expired exception
		/// while the main conection has a lock on the selected row.
		/// </summary>
		public void TestLockTimeoutExpired_DBCC()
		{
			var sql = "UPDATE dbo.RefCountry SET RN_IsActive = 0 WHERE RN_Code = @Code";
			TestConnection.ExecuteNonQuery(sql, cmd => cmd.AddParameter("@Code", SqlDbType.Char, 2, "AU"));

			try
			{
				sql = @"DBCC CHECKTABLE(N'dbo.RefCountry') WITH TABLOCK;";
				using (var conn = Db.NewAdminConnection())
				using (var temp = conn.TemporarySetLockTimeout(0))
				using (var cmd = conn.Command(sql))
				{
					cmd.ExecuteNonQuery();
				}

				Fail("Should throw exception");
			}
			catch (SqlException e)
			{
				AssertEquals("Wrong Exception caught - " + e.Message, DbErrorType.LockTimeoutExpired, new DbErrorMatch(e).ExceptionType);
			}
		}

		public void TestCannotOpenBackupDeviceOrInvalidDeviceName()
		{
			// Cannot find device
			try
			{
				string sqlText = "BACKUP DATABASE " + Db.DatabaseName + @" TO DISK='InvalidDrive:\InvalidPath\InvalidFileName.bkp'";

				using (var adminConnection = Db.NewAdminConnection())
				{
					adminConnection.ExecuteNonQuery(sqlText);
				}

				Fail("Should throw exception");
			}
			catch (SqlException e)
			{
				AssertEquals("Wrong Exception caught - " + e.Message, DbErrorType.CannotOpenBackupDeviceOrInvalidDeviceName, new DbErrorMatch(e).ExceptionType);
			}

			// Invalid device name
			try
			{
				string sqlText = "BACKUP DATABASE " + Db.DatabaseName + @" TO DISK='\\\InvalidUncPath\AnyFileName.bkp'";

				using (var adminConnection = Db.NewAdminConnection())
				{
					adminConnection.ExecuteNonQuery(sqlText);
				}

				Fail("Should throw exception");
			}
			catch (SqlException e)
			{
				AssertEquals("Wrong Exception caught - " + e.Message, DbErrorType.CannotOpenBackupDeviceOrInvalidDeviceName, new DbErrorMatch(e).ExceptionType);
			}
		}

		public void TestCannotCreateFileBecauseItAlreadyExists()
		{
			const string dbName = "TestCannotCreateFileBecauseItAlreadyExists";
			const string logFileName = dbName + "_log.ldf";
			const string dbFileName = dbName + ".mdf";

			var logFilePath = Path.Combine(TempForTest.TempPath, logFileName + "_.ldf");
			var dbFilePath = Path.Combine(TempForTest.TempPath, dbFileName + ".mdf");

			var query = string.Format("create database {0} on (name='{0}_Data', filename='{1}') log on (name='{0}_Log', filename='{2}');", dbName, dbFilePath, logFilePath);

			using (var adminConnection = Db.NewAdminConnection())
			{
				try
				{
					File.Create(logFilePath).Dispose();

					adminConnection.ExecuteNonQuery(query);

					Fail("Should throw exception");
				}
				catch (SqlException e)
				{
					var testHandler = new DbErrorMatch(e);
					AssertEquals("Wrong Exception caught - " + e.Message, DbErrorType.CannotCreateFileBecauseItAlreadyExists, testHandler.ExceptionType);
					AssertEquals("File full name should be in the message\r\n" + e.Message, logFilePath.ToLower(), testHandler.GetFileFullNameOnCannotCreateFileError().ToLower());
				}
				finally
				{
					// If DB was created (it shouldn't have been), drop it
					var sqlText = string.Format("IF EXISTS (SELECT name FROM sys.databases WHERE name = '{0}') DROP DATABASE {0}", dbName);
					adminConnection.ExecuteNonQuery(sqlText);

					// Drop test file
					File.Delete(logFilePath);
				}
			}
		}

		public void TestGeneralUserException()
		{
			try
			{
				TestConnection.ExecuteNonQuery("RAISERROR('blah', 16, 1)");
				Fail("Should throw exception");
			}
			catch (SqlException ex)
			{
				DbErrorMatch match = new DbErrorMatch(ex);
				AssertEquals("Wrong Exception caught - " + ex.Message, DbErrorType.GeneralUserException, match.ExceptionType);
			}
		}

		[ExpectNoExceptions]
		public void TestCannotDropLoginMappedToDbUser()
		{
			using (var login = new DbLoginForTest("TestCannotDropLoginMappedToDbUser", ""))
			using (DbConnection adminConnection = Db.NewAdminConnection())
			{
				string sqlText = "EXEC master..sp_droplogin @userLogin";
				using (DbCommand cmd = adminConnection.Command(sqlText))
				{
					cmd.AddParameter("@userLogin", SqlDbType.VarChar, 128, login.LoginName);
					cmd.ExecuteNonQuery();
				}
			}
		}

		[ExpectNoExceptions]
		public void TestCouldNotFindDatabaseId()
		{
			string sqlText = @"
					SELECT
						UdfView.GS_Code
					FROM
						dbo.GlbStaff
						LEFT JOIN (SELECT GS_PK, GS_Code
											FROM dbo.GlbStaff
											INNER JOIN dbo.GlbCompany
											ON GS_LastPasswordChangeDate != dbo.GetPeriodFromDate(GS_Birthdate, GC_PK)
						) AS UdfView ON GlbStaff.GS_PK = UdfView.GS_PK";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		public void TestLoginFailedForUser()
		{
			try
			{
				using (var connWithWrongPwd = new ExtraConnectionWithTinyTimeoutForTest(Db.ServerName, Db.DatabaseName, Db.SysAdminUserLogin, "~This*Cannot*Be*The*Right*Pwd~"))
				{
					connWithWrongPwd.EnsureIsOpen();
				}
				Fail("Should throw exception");
			}
			catch (SqlException e)
			{
				DbErrorType errorType = new DbErrorMatch(e).ExceptionType;
				AssertEquals("Wrong Exception caught - " + e.Message + " Exception Number " + e.Number, DbErrorType.LoginFailedForUser, errorType);
				Assert("Should not be upgrade lockout error", !DbErrorMatch.IsDbLoginError(errorType));
			}
		}

		/// <summary>
		/// The server principal "XXX" is not able to access the database "Odyssey_SD001" under the current security context.
		/// </summary>
		public void TestLoginIsNotAbleToAccessDatabaseUnderCurrentSecurityContext()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				// Not meant to have access to model anyway.
				// Otherwise it would be automatically granted rights to every new database, even if belonging to a different system.
				((IDbLoginRepair)adminConnection).DropDbLoginUsersFromDatabase("model", msg => { });
			}

			try
			{
				TestConnection.ExecuteNonQuery("SELECT top 0 * FROM model.sys.tables");
				Fail("Should throw exception");
			}
			catch (SqlException ex)
			{
				var errorMatch = new DbErrorMatch(ex);
				AssertEquals(
					"Wrong Exception caught - " + ex.Message,
					DbErrorType.LoginIsNotAbleToAccessDatabaseUnderCurrentSecurityContext,
					errorMatch.ExceptionType);

				string errorDbName = errorMatch.GetDatabaseFromSecurityError();
				AssertEquals("Database from error message", "model", errorDbName);
			}
		}

		/// <summary>
		/// CREATE TABLE permission denied in database 'XXX'.
		/// </summary>
		public void TestPermissionDeniedInDatabase()
		{
			string sqlText = string.Format(
				CultureInfo.InvariantCulture,
				"EXEC [{0}]..sp_executesql N'CREATE TABLE [*Test*Table*] (Col1 bit)'",
				Db.SqlMasterDb
			);
			var errorMatch = ExecuteAndAssert(sqlText, DbErrorType.PermissionDeniedInDatabase);
			string errorDbName = errorMatch.GetDatabaseFromSecurityError();
			AssertEquals("Database from error message", Db.SqlMasterDb, errorDbName);
		}

		/// <summary>
		/// Cannot execute as the server principal because the principal "XXX" does not exist, this type of principal cannot be impersonated, or you do not have permission.
		/// </summary>
		public void TestCannotExecuteAsTheServerPrincipal()
		{
			string sqlText = "EXECUTE AS LOGIN = '~non!existin@login#'";
			var errorMatch = ExecuteAndAssert(sqlText, DbErrorType.CannotExecuteAsServerPrincipal);
			string errorPrincipal = errorMatch.GetPrincipalFromError();
			AssertEquals("Login from error message", "~non!existin@login#", errorPrincipal);
		}

		/// <summary>
		/// Cannot execute as the database principal because the principal "XXX" does not exist, this type of principal cannot be impersonated, or you do not have permission.
		/// </summary>
		public void TestCannotExecuteAsTheDatabasePrincipal()
		{
			string sqlText = "EXECUTE AS USER = '~non!existin@user#'";
			var errorMatch = ExecuteAndAssert(sqlText, DbErrorType.CannotExecuteAsDatabasePrincipal);
			string errorPrincipal = errorMatch.GetPrincipalFromError();
			AssertEquals("User from error message", "~non!existin@user#", errorPrincipal);
		}

		/// <summary>
		/// Cannot insert the value NULL into column 'Col1'; column does not allow nulls.
		/// </summary>
		public void TestCannotInsertNullIntoNonNullableColumn()
		{
			string sqlText = @"
				CREATE TABLE TestAlterColumnD2E643AD002545178FD9A088671DDAED (Col1 int NULL)
				INSERT TestAlterColumnD2E643AD002545178FD9A088671DDAED VALUES (null)
				ALTER TABLE TestAlterColumnD2E643AD002545178FD9A088671DDAED ALTER COLUMN Col1 int NOT NULL";

			var result = ExecuteAndAssert(sqlText, DbErrorType.CannotInsertNullIntoNonNullableColumn);
			AssertEquals("Friendly error message", string.Format(CultureInfo.InvariantCulture,
				"Cannot insert the value NULL into column 'Col1', table '{0}.dbo.TestAlterColumnD2E643AD002545178FD9A088671DDAED'; column does not allow nulls. UPDATE fails.", TestConnection.CurrentDatabase), result.GetUserFriendlyMessage(TestConnection));
		}

		/// <summary>
		/// string or binary data would be truncated.
		/// </summary>
		public void TestStringOrBinaryDataWouldBeTruncated()
		{
			string sqlText = @"
				CREATE TABLE TestAlterColumnD2E643AD002545178FD9A088671DDAED (Col1 char(5))
				INSERT TestAlterColumnD2E643AD002545178FD9A088671DDAED VALUES ('12345')
				ALTER TABLE TestAlterColumnD2E643AD002545178FD9A088671DDAED ALTER COLUMN Col1 char(2)";

			ExecuteAndAssert(sqlText, DbErrorType.StringOrBinaryDataWouldBeTruncated);
		}

		/// <summary>
		/// Operand type clash: uniqueidentifier is incompatible with int.
		/// </summary>
		public void TestIncompatibleDataTypeConversionClash_AlterColumn()
		{
			string sqlText = @"
				CREATE TABLE TestAlterColumnD2E643AD002545178FD9A088671DDAED (Col1 uniqueidentifier)
				ALTER TABLE TestAlterColumnD2E643AD002545178FD9A088671DDAED ALTER COLUMN Col1 int";
			ExecuteAndAssert(sqlText, DbErrorType.IncompatibleDataTypeConversionClash);
		}

		/// <summary>
		/// Operand type clash: Cannot convert from numeric to decimal
		/// </summary>
		public void TestCannotConvertDatatype()
		{
			var exception = SqlExceptionBuilder.CreateSqlException(8114, "Error converting data type numeric to decimal.");
			var errorHandler = new DbErrorHandler(exception, Db.Connection);
			Assert("Did not correctly match the 8114 error.", errorHandler.ExceptionType == DbErrorType.CannotConvertDataType);
		}

		/// <summary>
		/// Operand type clash: uniqueidentifier is incompatible with int.
		/// </summary>
		public void TestIncompatibleDataTypeConversionClash_IncompatibleDefaultValue()
		{
			string sqlText = "CREATE TABLE TestTable5A0E57D2713C4EC7B0074216FE5F8E10 (Col1 int DEFAULT newid())";
			ExecuteAndAssert(sqlText, DbErrorType.IncompatibleDataTypeConversionClash);
		}

		/// <summary>
		/// Disallowed implicit conversion from data type char to data type money.
		/// </summary>
		public void TestDisallowedImplicitDataTypeConversionError()
		{
			string sqlText = @"
				CREATE TABLE TestAlterColumnD2E643AD002545178FD9A088671DDAED (Col1 char(5))
				ALTER TABLE TestAlterColumnD2E643AD002545178FD9A088671DDAED ALTER COLUMN Col1 binary(100)";

			DbErrorMatch testHandler = ExecuteAndAssert(sqlText, DbErrorType.DisallowedImplicitDataTypeConversionError);
		}

		/// <summary>
		/// Cannot alter column 'Col1' to be data type TimeStamp.
		/// The purpose of this Test is to test a specific DbErrorType.
		/// </summary>
		public void TestCannotAlterColumnToSpecificDataType()
		{
			string sqlText = @"
				CREATE TABLE TestAlterColumnD2E643AD002545178FD9A088671DDAED(Col1 varchar(max))
				ALTER TABLE TestAlterColumnD2E643AD002545178FD9A088671DDAED ALTER COLUMN Col1 TimeStamp";

			ExecuteAndAssert(sqlText, DbErrorType.CannotAlterColumnToSpecificDataType);
		}

		/// <summary>
		/// Msg 4928, Level 16, State 1, Procedure sys.sp_rename, Line 690
		/// Cannot alter column 'colb' because it is 'COMPUTED'.
		/// </summary>
		public void TestCannotAlterColumn()
		{
			// Arrange
			TestConnection.ExecuteNonQuery("DROP TABLE IF EXISTS dbo._tab;");
			TestConnection.ExecuteNonQuery("CREATE TABLE dbo._tab (ColA int, colb AS ColA);");

			// Act
			// Assert
			ExecuteAndAssert("EXEC sys.sp_rename N'dbo._tab.colb', N'ColB', 'COLUMN';", DbErrorType.CannotAlterColumn);
		}

		/// <summary>
		/// The object 'PkTestAlterColumnD2E643AD002545178FD9A088671DDAED' is dependent on column 'Col1'.
		/// </summary>
		public void TestCannotDropColumnWithDependentObjects()
		{
			string sqlText = @"
				CREATE TABLE TestAlterColumnD2E643AD002545178FD9A088671DDAED (Col1 char(5) NOT NULL, Col2 int)
				ALTER TABLE TestAlterColumnD2E643AD002545178FD9A088671DDAED ADD CONSTRAINT PkTestAlterColumnD2E643AD002545178FD9A088671DDAED PRIMARY KEY (Col1)
				ALTER TABLE TestAlterColumnD2E643AD002545178FD9A088671DDAED DROP COLUMN Col1";

			DbErrorMatch testHandler = ExecuteAndAssert(sqlText, DbErrorType.ObjectDependencyError);
		}

		/// <summary>
		/// ALTER TABLE DROP COLUMN failed because 'Col1' is the only data column in table 'TestAlterColumnD2E643AD002545178FD9A088671DDAED'.
		/// </summary>
		public void TestCannotDropLastColumnInATable()
		{
			string sqlText = @"
				CREATE TABLE TestAlterColumnD2E643AD002545178FD9A088671DDAED (Col1 int)
				ALTER TABLE TestAlterColumnD2E643AD002545178FD9A088671DDAED DROP COLUMN Col1";

			DbErrorMatch testHandler = ExecuteAndAssert(sqlText, DbErrorType.CannotDropLastColumnInATable);
		}

		/// <summary>
		/// ALTER FUNCTION failed because function types are incompatible.
		/// </summary>
		public void TestCannotAlterObjectsOfIncompatibleTypes()
		{
			string sqlText = @"CREATE FUNCTION TestFunction800F07D889AD49F0B104AC1F5A093AC8() RETURNS TABLE AS RETURN SELECT 'A' AS A";
			TestConnection.ExecuteNonQuery(sqlText);

			sqlText = @"ALTER FUNCTION TestFunction800F07D889AD49F0B104AC1F5A093AC8() RETURNS @Result TABLE (A VARCHAR(1)) BEGIN INSERT @Result SELECT 'A' AS A RETURN END";
			DbErrorMatch testHandler = ExecuteAndAssert(sqlText, DbErrorType.CannotAlterObjectOfIncompatibleType);
		}

		#region TestLoginFailed

		public void TestLoginFailed()
		{
			string fakeLoginName = "Muhaha6D5FF39152C443FB8DD169E98C4B1445";
			using (var connection = Db.NewExtraConnection(Db.ServerName, Db.DatabaseName, fakeLoginName, ""))
			{
				try
				{
					connection.ExecuteNonQuery("select top 1 * from dbo.orgheader");

					Fail("Should throw exception");
				}
				catch (SqlException e)
				{
					DbErrorMatch testHandler = new DbErrorMatch(e);
					AssertEquals("Wrong Exception caught - " + e.Message, DbErrorType.LoginFailedForUser, testHandler.ExceptionType);
					AssertEquals(
						"Exception message",
						string.Format(
							"Database login failed - please check the server error log.\r\nIf the problem persists then please contact your system administrator.\r\n\r\nMessage: Login failed for user '{0}'.\r\nServer Name: {1}\r\nDatabase: {2}",
							fakeLoginName, Db.ServerName, Db.DatabaseName),
						testHandler.GetUserFriendlyMessage(TestConnection));
				}
			}
		}

		#endregion

		public void TestFailedToInitClrDueToMemPressure()
		{
			var error = SqlExceptionBuilder.CreateSqlError(6513, byte.MaxValue, byte.MinValue, Db.Connection.ServerName, "Failed to initialize the Common Language Runtime (CLR) v2.0.50727 due to memory pressure.", "", 0);
			var errors = SqlExceptionBuilder.CreateSqlErrorCollection(error);
			var exception = SqlExceptionBuilder.CreateSqlException(errors);

			DbErrorHandler errorHandler = new DbErrorHandler(exception, Db.Connection);
			Assert(errorHandler.ExceptionType == DbErrorType.FailedToInitClrDueToMemPressure);

			var friendlyMessage = errorHandler.GetDBErrorUserFriendlyMessage();
			Assert(friendlyMessage.Contains("Failed to initialize the Common Language Runtime (CLR) v2.0.50727 due to memory pressure."));
			Assert(friendlyMessage.Contains("You need to restart SQL Server to fix this issue."));
		}

		public void TestSystemAttentionFromPooledConnectionStateResetTimeout()
		{
			var error = SqlExceptionBuilder.CreateSqlError(3617, byte.MaxValue, byte.MinValue, Db.Connection.ServerName, string.Empty, string.Empty, 0);
			var errors = SqlExceptionBuilder.CreateSqlErrorCollection(error);
			var exception = SqlExceptionBuilder.CreateSqlException(errors);

			DbErrorHandler errorHandler = new DbErrorHandler(exception, Db.Connection);
			AssertEquals(DbErrorType.TimeoutExpired, errorHandler.ExceptionType);

			var friendlyMessage = errorHandler.GetDBErrorUserFriendlyMessage();
			Assert(friendlyMessage.Contains("Server is taking too long to respond. Please try again."));
		}

		public void TestCannotRetrievePeerToPeerDbInfo()
		{
			var error = SqlExceptionBuilder.CreateSqlError(18847, 1, 1, Db.Connection.ServerName, "Cannot retrieve the peer-to-peer database information. Contact Customer Support Services.", "", 1);
			var exception = SqlExceptionBuilder.CreateSqlException(error);

			var errorHandler = new DbErrorHandler(exception, Db.Connection);
			Assert("Did not correctly match the  18847 error.", errorHandler.ExceptionType == DbErrorType.CannotRetrievePeerToPeerDbInfo);
		}

		public void TestStatementHasBeenTerminated()
		{
			var error = SqlExceptionBuilder.CreateSqlError(3621, 1, 1, Db.Connection.ServerName, "The statement has been terminated.", "", 1);
			var exception = SqlExceptionBuilder.CreateSqlException(error);

			var errorHandler = new DbErrorHandler(exception, Db.Connection);
			Assert("Did not correctly match the 3621 error.", errorHandler.ExceptionType == DbErrorType.StatementHasBeenTerminated);
		}

		public void TestDatabaseCannotBeRevertedFromSnapshot()
		{
			using (var conn = Db.NewAdminConnection())
			{
				var snapshotName1 = "asdf1-SS";
				var snapshotName2 = "asdf2-SS";

				try
				{
					SnapshotCreator.CreateSnapshot(conn, () => Db.Connection.CloseConnection(), Db.DatabaseName, snapshotName1, filename_ext: "_1");
					var secondSnapshot = SnapshotCreator.CreateSnapshot(conn, () => Db.Connection.CloseConnection(), Db.DatabaseName, snapshotName2, filename_ext: "_2");
					ExecuteAndAssert(secondSnapshot.Dispose, DbErrorType.DatabaseCannotBeRevertedFromSnapshot);
				}
				finally
				{
					conn.ExecuteNonQuery($"DROP DATABASE [{snapshotName1}]");
					// The logic in snapshot creator will still drop the snapshot even if the restore fails beacuse it is done in a single batch and xact_abort is off. So no need to drop snapshotName2
				}
			}
		}

		public void TestGetUserFriendlyMessageHandlesCheckConstraintViolationOnTempTable()
		{
			//Arrange
			var tempTableName = $"#TestTable_{Guid.NewGuid().ToString("N")}";
			var constraintName = $"CHK_{Guid.NewGuid().ToString("N")}_Securable_Values";

			using (var command = Db.Connection.Command(
				$@"
                CREATE TABLE dbo.{tempTableName} (
                    ID INT PRIMARY KEY,
                    Value VARCHAR(50),
                    CONSTRAINT {constraintName} CHECK (Value = 'Valid')
                );"))
			{
				command.ExecuteNonQuery();
			}
			// Act
			var caughtException = AssertExceptionThrown<SqlException>(() =>
			{
				using (var command = Db.Connection.Command($"INSERT INTO {tempTableName} (ID, Value) VALUES (1, 'Invalid');"))
				{
					command.ExecuteNonQuery();
				}
			});

			// Assert
			AssertNotNull(caughtException);

			string friendlyError = null;
			var handler = new DbErrorMatch(caughtException);

			AssertNoExceptionThrown(() =>
			{
				friendlyError = handler.GetUserFriendlyMessage(Db.Connection);
			});

			AssertEquals($"Cannot import to table '{tempTableName.Replace("_", "")}' because constraint '{constraintName.Replace("_", "")}' failed on column 'Value' - ([Value]='Valid').", friendlyError);
		}

		#region Implementation

		DbConnection AnotherDbConnection;

		protected override void SetUp()
		{
			base.SetUp();
			AnotherDbConnection = Db.NewExtraConnectionToMainDb();
		}

		protected override void TearDown()
		{
			((IDisposable)AnotherDbConnection).Dispose();
			base.TearDown();
		}

		DbErrorMatch ExecuteAndAssert(string command, DbErrorType expectedErrorType, Action<DbCommand> paramsAction = null)
		{
			if (paramsAction == null)
			{
				return ExecuteAndAssert(() => TestConnection.ExecuteNonQuery(command), expectedErrorType);
			}
			return ExecuteAndAssert(() => TestConnection.ExecuteNonQuery(command, paramsAction), expectedErrorType);
		}

		DbErrorMatch ExecuteAndAssert(Action action, DbErrorType expectedErrorType)
		{
			DbErrorMatch result = null;

			try
			{
				action();
				Fail("Should throw exception");
			}
			catch (SqlException e)
			{
				result = new DbErrorMatch(e);
				AssertEquals("Wrong Exception caught - " + e.Message, expectedErrorType, result.ExceptionType);
			}

			AssertNotNull(result);
			return result;
		}

		void ExecuteAndAssertGetUserFriendlyMessage(string command, DbErrorType expectedErrorType, string expectedFriendlyMessage)
		{
			var handler = ExecuteAndAssert(command, expectedErrorType);
			var userFriendlyMessage = handler.GetUserFriendlyMessage(TestConnection);
			AssertEquals(expectedFriendlyMessage, userFriendlyMessage);
		}

		Guid GetPkFromRandomRowInTable(string tableName, string pkColumn)
		{
			string sqlText = "SELECT TOP 1 " + pkColumn + " FROM " + tableName;
			Guid result = (Guid)TestConnection.ExecuteScalar(sqlText);
			return result;
		}

		#endregion
	}
}
