using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	[UseSnapshotProtection]
	sealed class DbErrorMatchSysMessagesTest : TestCase
	{
		public void TestGetErrorType_SqlExceptionDdlDisconnection()
		{
			var ex = GetSqlException(1219, "Your session has been disconnected because of a high priority DDL operation.");
			var handler = new DbErrorMatch(ex);
			AssertEquals(DbErrorType.SqlExceptionDdlDisconnection, handler.ExceptionType);
			AssertEquals(true, DbErrorMatch.SqlExceptionDdlDisconnection(handler.ExceptionType));
		}

		public void TestGetErrorType_ErrorLoadingUntrustedAssembly()
		{
			var ex = GetSqlException(10314, "ErrorLoadingUntrustedAssembly");
			var handler = new DbErrorMatch(ex);
			AssertEquals("Incorrect SQL Exception type", handler.ExceptionType, DbErrorType.ErrorLoadingUntrustedAssembly);
		}

		public void TestModuleBeingExecutedIsNotTrusted()
		{
			var ex = GetSqlException(15562, "ModuleBeingExecutedIsNotTrusted");
			var handler = new DbErrorMatch(ex);
			AssertEquals("Incorrect SQL Exception type", handler.ExceptionType, DbErrorType.ModuleBeingExecutedIsNotTrusted);
		}

		public void TestCannotOperateOnDatabaseWhenAleadyConfiguredForMirroringOrJoinedAvailabilityGroup()
		{
			var ex = GetSqlException(3104, "CannotOperateOnDatabaseWhenAleadyConfiguredForMirroringOrJoinedAvailabilityGroup");
			var handler = new DbErrorMatch(ex);
			AssertEquals("Incorrect SQL Exception type", handler.ExceptionType, DbErrorType.CannotOperateOnDatabaseWhenAlreadyConfiguredForMirroringOrJoinedToAvailabilityGroup);
		}

		public void TestDatabaseAlreadyJoinedToAvailabilityGroup()
		{
			var ex = GetSqlException(35280, "DatabaseAlreadyAddedToAvailabilityGroup");
			var handler = new DbErrorMatch(ex);
			AssertEquals("Incorrect SQL Exception type", handler.ExceptionType, DbErrorType.DatabaseAlreadyJoinedToAvailabilityGroup);

			ex = GetSqlException(41145, "DatabaseAlreadyJoinedToAvailabilityGroup");
			handler = new DbErrorMatch(ex);
			AssertEquals("Incorrect SQL Exception type", handler.ExceptionType, DbErrorType.DatabaseAlreadyJoinedToAvailabilityGroup);
		}

		public void TestGetErrorType_UnableToAccessResolvingReplicaDb()
		{
			AssertErrorMessageMatches(976, "The target database", "is participating in an availability group and is currently not accessible for queries. Either data movement is suspended or the availability replica is not enabled for read access. To allow read-only access to this and other");

			var ex = GetSqlException(976, "UnableToAccessDb_NotAccessibleForQueries");
			var handler = new DbErrorMatch(ex);
			AssertEquals("Incorrect SQL Exception type", handler.ExceptionType, DbErrorType.AlwaysOnAccessError);
			string friendlyError = handler.GetUserFriendlyMessage(TestConnection);
			Assert(string.Format("Friendly error message [{0}] not as expected", friendlyError), friendlyError.StartsWith("Please contact your systems administrator to check the database for errors"));

			AssertErrorMessageMatches(983, "Unable to access availability database", "because the database replica is not in the PRIMARY or SECONDARY role");
			ex = GetSqlException(983, "UnableToAccessResolvingReplicaDb");
			handler = new DbErrorMatch(ex);
			AssertEquals("Incorrect SQL Exception type", handler.ExceptionType, DbErrorType.AlwaysOnAccessError);
			friendlyError = handler.GetUserFriendlyMessage(TestConnection);
			Assert(string.Format("Friendly error message [{0}] not as expected", friendlyError), friendlyError.StartsWith("Please contact your systems administrator to check the database for errors"));

			AssertErrorMessageMatches(988, "Unable to access database", "because it lacks a quorum of nodes for high availability. Try the operation again later.");
			ex = GetSqlException(988, "UnableToAccessDb_LackOfQuorum");
			handler = new DbErrorMatch(ex);
			AssertEquals("Incorrect SQL Exception type", handler.ExceptionType, DbErrorType.AlwaysOnAccessError);
			friendlyError = handler.GetUserFriendlyMessage(TestConnection);
			Assert(string.Format("Friendly error message [{0}] not as expected", friendlyError), friendlyError.StartsWith("Please contact your systems administrator to check the database for errors"));
		}

		public void TestGetErrorType_RemoteHardenOfTransactionFailure()
		{
			var ex = GetSqlException(3303, "RemoteHardenOfTransactionFailure");
			var handler = new DbErrorMatch(ex);
			string friendlyError = handler.GetUserFriendlyMessage(TestConnection);

			AssertStartsWith(
				"Friendly error message not as expected",
				"The application has encountered a remote harden of transaction failure. Please retry your past action.",
				friendlyError);
		}

		public void TestGetErrorType_DatabaseIsBeingRecovered()
		{
			SqlException ex = GetSqlException(922, "DatabaseIsBeingRecovered");
			DbErrorMatch handler = new DbErrorMatch(ex);
			AssertEquals("Incorrect SQL Exception type", handler.ExceptionType, DbErrorType.DatabaseIsBeingRecovered);
			AssertNotEquals("", handler.GetUserFriendlyMessage(TestConnection));
		}

		public void TestGetErrorType_ConnectionRecovered()
		{
			SqlException ex = GetSqlException(4083, "ConnectionRecovered");
			DbErrorMatch handler = new DbErrorMatch(ex);
			AssertEquals("Incorrect SQL Exception type", handler.ExceptionType, DbErrorType.ConnectionRecovered);
		}

		public void TestGetErrorType_InsufficientSystemMemoryToRunQuery()
		{
			SqlException ex = GetSqlException(701, "InsufficientSystemMemoryToRunQuery");
			DbErrorMatch handler = new DbErrorMatch(ex);
			AssertEquals("Incorrect SQL Exception type", handler.ExceptionType, DbErrorType.InsufficientSystemMemoryToRunQuery);
			AssertNotEquals("", handler.GetUserFriendlyMessage(TestConnection));
		}

		public void TestGetErrorType_InsufficientSystemMemoryAccessingCriticalResource()
		{
			SqlException ex = GetSqlException(6533, "InsufficientSystemMemoryAccessingCriticalResource");
			DbErrorMatch handler = new DbErrorMatch(ex);
			AssertEquals("Incorrect SQL Exception type", handler.ExceptionType, DbErrorType.InsufficientSystemMemoryAccessingCriticalResource);
			AssertNotEquals("", handler.GetUserFriendlyMessage(TestConnection));
		}

		public void TestGetErrorType_CannotDropLoginCurrentlyLoggedIn()
		{
			SqlException ex = GetSqlException(15434, "CannotDropLoginCurrentlyLoggedIn");
			DbErrorMatch handler = new DbErrorMatch(ex);
			AssertEquals("Incorrect SQL Exception type", handler.ExceptionType, DbErrorType.CannotDropLoginCurrentlyLoggedIn);
			AssertNotEquals("", handler.GetUserFriendlyMessage(TestConnection));
		}

		public void TestGetErrorType_DatabaseCannotBeAutostartedDuringServerShutdownOrStartup()
		{
			SqlException ex = GetSqlException(904, "DatabaseCannotBeAutostartedDuringServerShutdownOrStartup");
			DbErrorMatch handler = new DbErrorMatch(ex);
			AssertEquals("Incorrect SQL Exception type", handler.ExceptionType, DbErrorType.DatabaseCannotBeAutostartedDuringServerShutdownOrStartup);
			AssertNotEquals("", handler.GetUserFriendlyMessage(TestConnection));
		}

		public void TestGetErrorType_CannotContinueScanWithNoLockDueToDataMovement()
		{
			SqlException ex = GetCannotContinueScanWithNoLockDueToDataMovementException(601);
			DbErrorMatch handler = new DbErrorMatch(ex);
			AssertEquals("Incorrect SQL Exception type", handler.ExceptionType, DbErrorType.CannotContinueScanWithNoLockDueToDataMovement);
		}

		public void TestGetErrorType_Other()
		{
			SqlException ex = GetCannotContinueScanWithNoLockDueToDataMovementException(17);
			DbErrorMatch handler = new DbErrorMatch(ex);
			AssertEquals("Incorrect SQL Exception type", handler.ExceptionType, DbErrorType.ServerDoesNotExist);
		}

		public void TestGetErrorType_LogUnavailable()
		{
			var ex = GetSqlException(9001, "Something");
			var matcher = new DbErrorMatch(ex);
			AssertEquals(DbErrorType.LogUnavailable, matcher.ExceptionType);
		}

		public void TestGetErrorType_DatabaseOffline()
		{
			var ex = GetSqlException(942, "Something");
			var matcher = new DbErrorMatch(ex);
			AssertEquals(DbErrorType.DatabaseOffline, matcher.ExceptionType);
			AssertNotEquals("", matcher.GetUserFriendlyMessage(TestConnection));

			ex = GetSqlException(952, "Something else");
			matcher = new DbErrorMatch(ex);
			AssertEquals(DbErrorType.DatabaseOffline, matcher.ExceptionType);
		}

		public void TestGetErrorType_CannotOpenDatabaseMarkedAsSuspect()
		{
			var ex = GetSqlException(926, "CannotOpenDatabaseMarkedAsSuspect");
			var matcher = new DbErrorMatch(ex);
			AssertEquals(DbErrorType.CannotOpenDatabaseMarkedAsSuspect, matcher.ExceptionType);
			AssertNotEquals("", matcher.GetUserFriendlyMessage(TestConnection));
		}

		public void TestGetErrorType_DatabaseIsInTheMiddleOfRestore()
		{
			var ex = GetSqlException(927, "blah");
			var matcher = new DbErrorMatch(ex);
			AssertEquals(DbErrorType.DatabaseIsInTheMiddleOfRestore, matcher.ExceptionType);
			AssertNotEquals("", matcher.GetUserFriendlyMessage(TestConnection));
		}

		public void TestGetErrorType_CouldNotStartThreadResourcesForParallelQueryExecution()
		{
			var ex = GetSqlException(8642, "blah");
			var matcher = new DbErrorMatch(ex);
			AssertEquals(DbErrorType.CouldNotStartThreadResourcesForParallelQueryExecution, matcher.ExceptionType);
			AssertNotEquals("", matcher.GetUserFriendlyMessage(TestConnection));
		}

		public void TestGetErrorType_CannotRenameTableBecauseItIsPublishedForReplication()
		{
			var ex = GetSqlException(15051, "Cannot rename the table because it is published for replication.");
			var matcher = new DbErrorMatch(ex);
			AssertEquals(DbErrorType.CannotRenameTableBecauseItIsPublishedForReplication, matcher.ExceptionType);
			AssertErrorMessageMatches(15051, "Cannot rename the table because it is published for replication.");
		}

		public void TestGetErrorType_InvalidParameterPassedToOpenRowset()
		{
			var ex = GetSqlException(9005, "blah");
			var matcher = new DbErrorMatch(ex);
			AssertEquals(DbErrorType.InvalidParameterPassedToOpenRowset, matcher.ExceptionType);
			AssertEquals("", matcher.GetUserFriendlyMessage(TestConnection));
		}

		public void TestGetErrorType_InsufficientMemoryInBufferPool()
		{
			// Arrange
			var ex = GetSqlException(802, "There is insufficient memory available in the buffer pool.");

			// Act
			var handler = new DbErrorMatch(ex);

			// Assert
			AssertEquals("Incorrect SQL Exception type", handler.ExceptionType, DbErrorType.InsufficientMemoryInBufferPool);
			AssertEquals("SQL Server has experienced an issue because there is not enough memory in the Buffer Pool. Please try again or contact your systems administrator if the issue persists.\r\nMessage: " + ex.Message, handler.GetUserFriendlyMessage(TestConnection));
		}

		public void TestErrorsWhichCannotBeReproducedInUnitTests()
		{
			AssertErrorMessageMatches(1468,
				"The operation cannot be performed on database ",
				"because it is involved in a database mirroring session or an availability group. Some operations are not allowed on a database that is participating in a database mirroring session or in an availability group.");
			AssertErrorMessageMatches(6005, "SHUTDOWN is in progress");
			AssertErrorMessageMatches(6263, @"Execution of user code in the .NET Framework is disabled. Enable ""clr enabled"" configuration option.");
			AssertErrorMessageMatches(7139,
				"Length of LOB data",
				"to be replicated exceeds configured maximum");
			AssertErrorMessageMatches(8630,
							"Internal Query Processor Error",
							"The query processor encountered an unexpected error during execution");
			AssertErrorMessageMatches(15281,
				"SQL Server blocked access to",
				"because this component is turned off as part of the security configuration for this server",
				"sp_configure");
			AssertErrorMessageMatches(18401,
				"Login failed for user",
				"Reason: Server is in script upgrade mode. Only administrator can connect at this time.");
		}

		public void TestAnotherConnectionIsRunningSpReplcmdsForCdc()
		{
			AssertErrorMessageMatches(22903, "Another connection", "is already running 'sp_replcmds' for Change Data Capture in the current database.");
		}

		public void TestRequestToStopJobRefused()
		{
			var ex = GetSqlException(22022, "Request to stop job refused because the job is not currently running");
			var matcher = new DbErrorMatch(ex);
			AssertEquals(DbErrorType.RequestToStopCdcJobRefused, matcher.ExceptionType);
		}

		public void TestCouldNotUpdateCdcChangeTables()
		{
			var ex = GetSqlException(22851, "Could not update cdc.change_tables to indicate a change in the low water mark for database ediprod.");
			var matcher = new DbErrorMatch(ex);
			AssertEquals(DbErrorType.CouldNotUpdateCdcChangeTables, matcher.ExceptionType);
		}

		public void TestSsisFailedToDeployProject()
		{
			AssertErrorMessageMatches(27203, "Failed to deploy project. For more information, query the operation_messages view for the operation identifier");
		}

		public void TestCdcFailedDueToTransactionalReplication()
		{
			AssertErrorMessageMatches(22911, "The capture job cannot be used by Change Data Capture to extract changes from the log when transactional replication is also enabled on the same database. When Change Data Capture and transactional replication are both enabled on a database, use the logreader agent to extract the log changes.");
		}

		public void TestDataTypeIsUnknown()
		{
			var ex = GetSqlException(8009, "blah");
			var matcher = new DbErrorMatch(ex);
			AssertEquals(DbErrorType.DataTypeIsUnknown, matcher.ExceptionType);
			AssertEquals("You may be experiencing network problems. Please close down the application and try again.\n If the problem persists then please contact your network administrator.", matcher.GetUserFriendlyMessage(TestConnection));
			Assert(DbErrorMatch.IsNetworkError(DbErrorType.DataTypeIsUnknown));
		}

		public void TestCouldNotExecuteOnServerNotConfiguredForRemoteAccess()
		{
			var ex = GetSqlException(7201, "Could not execute procedure on remote server 'blah' because SQL Server is not configured for remote access.");
			AssertEquals(DbErrorType.CouldNotExecuteOnServerNotConfiguredForRemoteAccess, new DbErrorMatch(ex).ExceptionType);

			AssertErrorMessageMatches(ex.Number,
				"Could not execute procedure on remote server",
				"because SQL Server is not configured for remote access");
		}

		public void TestCouldNotFindServerLoopbackInSysServers()
		{
			var ex = GetSqlException(7202, "Could not find server 'LOOPBACK' in sys.servers. Verify that the correct server name was specified. If necessary, execute the stored procedure sp_addlinkedserver to add the server to sys.servers");
			AssertEquals(DbErrorType.LoopbackLinkedServerDoesNotExist, new DbErrorMatch(ex).ExceptionType);
			AssertErrorMessageMatches(ex.Number, "Could not find server '%.*ls' in sys.servers.");
		}

		public void TestCouldNotFindServerNONLOOPBACKInSysServers()
		{
			var ex = GetSqlException(7202, "Could not find server 'Whatever' in sys.servers. Verify that the correct server name was specified. If necessary, execute the stored procedure sp_addlinkedserver to add the server to sys.servers");
			AssertNotEquals(DbErrorType.LoopbackLinkedServerDoesNotExist, new DbErrorMatch(ex).ExceptionType);
		}

		public void TestIncomingRemoteConnectionFailed()
		{
			var ex = GetSqlException(121, "A connection was successfully established with the server, but then an error occurred during the login process. (provider: TCP Provider, error: 0 - The semaphore timeout period has expired.)");
			AssertEquals(DbErrorType.IncomingRemoteConnectionFailed, new DbErrorMatch(ex).ExceptionType);
		}

		public void TestSqlServerAuthenticationModeIsOff()
		{
			var ex = GetSqlException(233, "A connection was successfully established with the server, but then an error occurred during the login process. (provider: Named Pipes Provider, error: 0 - No process is on the other end of the pipe.)");
			AssertEquals(DbErrorType.SqlServerAuthenticationModeIsOff, new DbErrorMatch(ex).ExceptionType);
		}

		public void TestGeneralDbLoginError()
		{
			var ex = GetSqlException(233, "A connection was successfully established with the server, but then an error occurred during the login process. (provider: Shared Memory Provider, error: 0 - No process is on the other end of the pipe.)");
			AssertEquals(DbErrorType.SqlServerAuthenticationModeIsOff, new DbErrorMatch(ex).ExceptionType);
		}

		public void TestInvalidLoginError()
		{
			var ex = GetSqlException(15007, "'blah' is not a valid login or you do not have permission.");
			AssertEquals(DbErrorType.InvalidLoginOrNoPermission, new DbErrorMatch(ex).ExceptionType);
		}

		public void TestIsCannotExecuteAsDatabasePrincipalError()
		{
			Assert(DbErrorMatch.IsCannotExecuteAsDatabasePrincipalError(DbErrorType.CannotExecuteAsDatabasePrincipal));
			Assert(!DbErrorMatch.IsCannotExecuteAsDatabasePrincipalError(DbErrorType.AllAttemptsFailed));
		}

		public void TestBackupAndFileManipulationOperationsMustBeSerialized()
		{
			var ex = GetSqlException(3023, nameof(TestBackupAndFileManipulationOperationsMustBeSerialized));
			var errorMatch = new DbErrorMatch(ex);

			// Act
			var errorMessage = errorMatch.GetUserFriendlyMessage(TestConnection);

			// Assert
			Assert(errorMessage.StartsWith("Database backup or file manipulation operation failed, possibly caused by other concurrent running backup service tasks or database shrink operations."));
		}

		public void TestSqlErrorNumber3023()
		{
			var dbErrorMatch = new DbErrorMatch(SqlExceptionBuilder.CreateSqlException(3023, "DbErrorType.BackupAndFileManipulationOperationsMustBeSerialized"));
			Assert(dbErrorMatch.ExceptionType == DbErrorType.BackupAndFileManipulationOperationsMustBeSerialized);
		}

		public void TestSqlError3140CouldNotAdjustTheSpaceAllocationForFile()
		{
			var dbErrorMatch = new DbErrorMatch(SqlExceptionBuilder.CreateSqlException(3140, "Could not adjust the space allocation for file"));
			AssertEquals(DbErrorType.CouldNotAdjustTheSpaceAllocationForFile, dbErrorMatch.ExceptionType);
		}

		public void TestSqlError7202LinkedServerNotExisted()
		{
			var dbErrorMatch = new DbErrorMatch(SqlExceptionBuilder.CreateSqlException(7202, "Could not find server '%.*ls' in sys.servers. Verify that the correct server name was specified. If necessary, execute the stored procedure sp_addlinkedserver to add the server to sys.servers."));
			AssertEquals(DbErrorType.CouldNotFindLinkedServer, dbErrorMatch.ExceptionType);
		}

		public void TestSqlError7416NoLoginMappingExistedOnRemoteServer()
		{
			var dbErrorMatch = new DbErrorMatch(SqlExceptionBuilder.CreateSqlException(7416, "Access to the remote server is denied because no login-mapping exists."));
			AssertEquals(DbErrorType.NoLoginMappingExistsOnRemoteServer, dbErrorMatch.ExceptionType);
		}

		public void TestSqlError7411ServerIsNotConfigured()
		{
			var dbErrorMatch = new DbErrorMatch(SqlExceptionBuilder.CreateSqlException(7411, "Server '%.*ls' is not configured for %ls."));
			AssertEquals(DbErrorType.ServerIsNotConfigured, dbErrorMatch.ExceptionType);
		}

		#region Common

		void AssertErrorMessageMatches(int errorNumber, params string[] expectedMessageParts)
		{
			AssertEquals("Was at least one expected message part passed in?", true, expectedMessageParts.Length > 0);

			string sqlText = "SELECT text FROM sys.messages WHERE language_id = 1033 AND message_id = " + errorNumber.ToString();
			string actualMessage = TestConnection.ExecuteScalar(sqlText).ToString(); // DbCommand in Data solution
			string lowerCaseMessage = actualMessage.ToLower();

			foreach (string expectedPart in expectedMessageParts)
			{
				AssertEquals(
					string.Format("Error message\r\n\t[{0}]\r\ncontains part\r\n\t[{1}]\r\n?", actualMessage, expectedPart),
					true,
					lowerCaseMessage.Contains(expectedPart.ToLower()));
			}
		}

		SqlException GetSqlException(int errorCode, string errorMessage)
		{
			return AdoTestUtils.GetSqlException(errorCode, errorMessage, TestConnection) as SqlException;
		}

		SqlException GetCannotContinueScanWithNoLockDueToDataMovementException(int errorCode)
		{
			return GetSqlException(errorCode, "CannotContinueScanWithNoLockDueToDataMovement");
		}

		DbConnection TestConnection
		{
			get
			{
				return Db.Connection;
			}
		}

		#endregion
	}
}
