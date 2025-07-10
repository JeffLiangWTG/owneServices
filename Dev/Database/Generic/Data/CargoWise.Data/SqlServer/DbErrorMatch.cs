using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Database.Abstractions;
using CargoWise.Integration;
using CargoWise.Schema;

using Microsoft.Extensions.DependencyInjection;

namespace CargoWise.Data
{
	public class DbErrorMatch : IDbErrorMatch
	{
		public DbErrorMatch(System.Data.Common.DbException exRaw)
		{
			var ex = new SqlExceptionWrapper(exRaw);

			sqlServerExceptionMessage = ex.Message;

			if (ex.Errors != null && ex.Errors.Count != 0)
			{
				sqlServerExceptionNumber = ex.Number;
				sqlServerExceptionLineNumber = ex.LineNumber;
				sqlServerExceptionServer = ex.Server;
			}

			sqlServerExceptionErrors = ex.Errors;
		}

		readonly int sqlServerExceptionNumber;
		readonly int sqlServerExceptionLineNumber;
		readonly string sqlServerExceptionServer;
		readonly string sqlServerExceptionMessage;
		readonly IEnumerable<SqlErrorWrapper> sqlServerExceptionErrors;

		DbErrorType exceptionType = DbErrorType.NotInitialised;

		public bool IsInfrastructureDbError => InfrastructureDbErrors.Contains(ExceptionType);

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		static readonly DbErrorType[] InfrastructureDbErrors =
		{
			DbErrorType.CannotOpenDbRequestedInLogin,
			DbErrorType.DatabaseCannotBeAutostartedDuringServerShutdownOrStartup,
			DbErrorType.DatabaseIsBeingRecovered,
			DbErrorType.DatabaseIsInTheMiddleOfRestore,
			DbErrorType.DatabaseOffline,
			DbErrorType.CannotOpenDatabaseMarkedAsSuspect,
			DbErrorType.DbFilegroupIsFull,
			DbErrorType.DeviceActivationError,
			DbErrorType.GeneralNetworkError,
			DbErrorType.InaccessibleFiles,
			DbErrorType.InsufficientSystemMemoryToRunQuery,
			DbErrorType.LoginFailedForUser,
			DbErrorType.LogonFailedForLogin,
			DbErrorType.LoginFailedBecauseItIsLockedOut,
			DbErrorType.LogIsFull,
			DbErrorType.LogUnavailable,
			DbErrorType.ServerDoesNotExist,
			DbErrorType.ClrDbOptionDisabled,
			DbErrorType.CmdShellDbOptionDisabled,
			DbErrorType.LoginDisabled,
			DbErrorType.AlwaysOnAccessError,
			DbErrorType.LargeObjectSizeExceedsReplicationMaximum,
			DbErrorType.AllAttemptsFailed,
			DbErrorType.SqlServerCannotObtainALockResource,
			DbErrorType.SqlExceptionDdlDisconnection,
			DbErrorType.InsufficientSystemMemoryAccessingCriticalResource,
			DbErrorType.OwnerSIDDiffersFromMaster,
			DbErrorType.ServerIsInScriptMode,
			DbErrorType.CannotCreateFileBecauseItAlreadyExists,
			DbErrorType.ModifyFileEncounteredOperatingSystemError,
			DbErrorType.DiskFullIoError,
			DbErrorType.TempdbIsOutOfSpace,
			DbErrorType.ModuleBeingExecutedIsNotTrusted,
			DbErrorType.SqlServerAuthenticationModeIsOff,
			DbErrorType.IncomingRemoteConnectionFailed,
			DbErrorType.BufferLatchTimeout,
			DbErrorType.SevereError,
			DbErrorType.InsufficientMemoryInBufferPool,
			DbErrorType.FatalError,
			DbErrorType.TCPProviderConnectionAttemptFailed,
			DbErrorType.TlsCertificateError,
			DbErrorType.ExecuteQueryInResourcePoolTimeOut,
			DbErrorType.LoopbackLinkedServerDoesNotExist,
		};

		public static bool IsDbLoginError(DbErrorType error) =>
			error == DbErrorType.LoginDisabled
			|| error == DbErrorType.CannotOpenDbRequestedInLogin
			|| error == DbErrorType.LoginFailedBecauseItIsLockedOut
			|| error == DbErrorType.SqlServerAuthenticationModeIsOff;

		#region SqlExceptionDdlDisconnection

		public static bool SqlExceptionDdlDisconnection(DbErrorType error)
		{
			return error == DbErrorType.SqlExceptionDdlDisconnection;
		}

		public static DbErrorType GetExceptionType(SqlException ex)
		{
			return new DbErrorMatch(ex).ExceptionType;
		}

		#endregion

		#region IDbErrorMatch Members

		public DbErrorType ExceptionType
		{
			get
			{
				if (exceptionType == DbErrorType.NotInitialised)
				{
					exceptionType = GetErrorType();
				}
				return exceptionType;
			}
		}

		public string GetUserFriendlyMessage(DbConnection connection)
		{
			return DoGetUserFriendlyMessage(connection);
		}

		public string GetIndexNameIfUniqueIndexViolation()
		{
			return DoGetIndexNameIfUniqueIndexViolation();
		}

		#endregion

		#region Error Type

		DbErrorType GetErrorType()
		{
			var result = DbErrorType.NotHandled;

			try
			{
				result = GetServerErrorType();

				if (result == DbErrorType.NotHandled)
				{
					result = GetNonServerErrorType();
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				// Avoid hiding original exception
			}

			return result;
		}

		[SuppressMessage("Microsoft.Contracts", "TestAlwaysEvaluatingToAConstant")]
		DbErrorType GetServerErrorType()
		{
			DbErrorType result;
			var message = sqlServerExceptionMessage;

			switch (sqlServerExceptionNumber)
			{
				case 21:  //Warning: Fatal error %d occurred at %S_DATE. Note the error and time, and contact your system administrator.
					result = DbErrorType.FatalError;
					break;

				case 201:  //Procedure or function '%s' expects parameter '%s', which was not supplied.
					result = DbErrorType.ExpectedParameterNotSupplied;
					break;

				case 206:  // Operand type clash: %ls is incompatible with %ls
					result = DbErrorType.IncompatibleDataTypeConversionClash;
					break;

				case 207:  // Invalid column name '%.*ls'.
					result = DbErrorType.InvalidColumnName;
					break;

				case 208:  // Invalid object name '%.*ls'.
					result = DbErrorType.InvalidObjectName;
					break;

				case 220:  // Arithmetic overflow error for data type %ls, value = %ld.
				case 232:  // Arithmetic overflow error for type %ls, value = %f.
					result = DbErrorType.ArithmeticOverflowForNumericType;
					break;

				case 229:  // The %ls permission was denied on the object '%.*ls', database '%.*ls', schema '%.*ls'.
					result = DbErrorType.PermissionDeniedOnObject;
					break;

				case 235:  // Cannot convert a char value to money. The char value has incorrect syntax.
					result = DbErrorType.CannotConvertNonnumericCharValueToMoney;
					break;

				case 242:  // The conversion of a %ls data type to a %ls data type resulted in an out-of-range value.
					result = DbErrorType.DataTypeConversionOverflow;
					break;

				case 257:  // Implicit conversion from data type %ls to %ls is not allowed.
				case 260:  // Disallowed implicit conversion from data type %ls to data type %ls, table '%.*ls', column '%.*ls'.
					result = DbErrorType.DisallowedImplicitDataTypeConversionError;
					break;

				case 262:  // %ls permission denied in database '%.*ls'.
					result = DbErrorType.PermissionDeniedInDatabase;
					break;

				case 515:  // Cannot insert the value NULL into column '%.*ls', table '%.*ls'; column does not allow nulls. %ls fails.
					result = DbErrorType.CannotInsertNullIntoNonNullableColumn;
					break;

				case 529: // Explicit conversion from data type %s to %s is not allowed.
					result = DbErrorType.ExplicitConversionNotAllowed;
					break;

				case 547:  // The %ls statement conflicted with the %ls constraint "%.*ls". The conflict occurred in database "%.*ls", table "%.*ls"%ls%.*ls%ls.
					result = GetConstraintViolationType(message);
					break;

				case 596: // Cannot continue the execution because the session is in the kill state.\r\nA severe error occurred on the current command.  The results, if any, should be discarded.
					result = DbErrorType.SevereError;
					break;

				case 601:  // Could not continue scan with NOLOCK due to data movement.
					result = DbErrorType.CannotContinueScanWithNoLockDueToDataMovement;
					break;

				case 615:  // Could not find database ID %d, name '%.*ls'. The database may be offline. Wait a few minutes and try again.
					result = DbErrorType.CouldNotFindDatabaseTableId;
					break;

				case 701:  // There is insufficient system memory in resource pool '%ls' to run this query.
					result = DbErrorType.InsufficientSystemMemoryToRunQuery;
					break;

				case 802: // There is insufficient memory available in the buffer pool.
					result = DbErrorType.InsufficientMemoryInBufferPool;
					break;

				case 845: // Time-out occurred while waiting for buffer latch type %d for page %S_PGID, database ID %d.
					result = DbErrorType.BufferLatchTimeout;
					break;

				case 904:  // Database %ld cannot be autostarted during server shutdown or startup.
					result = DbErrorType.DatabaseCannotBeAutostartedDuringServerShutdownOrStartup;
					break;

				case 911:  // Database '%.*ls' does not exist. Make sure that the name is entered correctly.
					result = DbErrorType.CouldNotLocateDbInSysdatabases;
					break;

				case 913:  // Could not find database ID %d. Database may not be activated yet or may be in transition.
					result = DbErrorType.CouldNotFindDatabaseId;
					break;

				case 916:  // The server principal "%.*ls" is not able to access the database "%.*ls" under the current security context.
					result = DbErrorType.LoginIsNotAbleToAccessDatabaseUnderCurrentSecurityContext;
					break;

				case 922:  // Database '%.*ls' is being recovered. Waiting until recovery is finished.
					result = DbErrorType.DatabaseIsBeingRecovered;
					break;

				case 924:  // Database '%.*ls' is already open and can only have one user at a time.
					result = DbErrorType.DatabaseInSingleUserModeAndAlreadyOpen;
					break;

				case 926:  // Database '%.*ls' cannot be opened. It has been marked SUSPECT by recovery. See the SQL Server errorlog for more information.
					result = DbErrorType.CannotOpenDatabaseMarkedAsSuspect;
					break;

				case 927:  // Database '%.*ls' cannot be opened. It is in the middle of a restore.
					result = DbErrorType.DatabaseIsInTheMiddleOfRestore;
					break;

				case 942: // Database '%.*ls' cannot be opened because it is offline.
				case 952: // Database '%.*ls' is in transition. Try the statement later.
					result = DbErrorType.DatabaseOffline;
					break;

				case 945:  // Database '%.*ls' cannot be opened due to inaccessible files or insufficient memory or disk space.
					result = DbErrorType.InaccessibleFiles;
					break;

				case 976: // The target database, '%.*ls', is participating in an availability group and is currently not accessible for queries. Either data movement is suspended or the availability replica is not enabled for read access. To allow read-only access to this and other
				case 983: // Unable to access database '%.*ls' because its replica role is RESOLVING which does not allow connections. Try the operation again later.
				case 988: // Unable to access database '%.*ls' because it lacks a quorum of nodes for high availability. Try the operation again later.
					result = DbErrorType.AlwaysOnAccessError;
					break;

				case 1101: // Could not allocate a new page for database '%.*ls' because of insufficient disk space in filegroup '%.*ls'.
				case 1105: // Could not allocate space for object '%.*ls'%.*ls in database '%.*ls' because the '%.*ls' filegroup is full.
					result = DbErrorType.DbFilegroupIsFull;
					break;

				case 1204: // The instance of the SQL Server Database Engine cannot obtain a LOCK resource at this time. Rerun your statement when there are fewer active users. Ask the database administrator to check the lock and memory configuration for this instance, or to check for long-running transactions.
					result = DbErrorType.SqlServerCannotObtainALockResource;
					break;

				case 1205: // Transaction (Process ID %d) was deadlocked on %.*ls resources with another process and has been chosen as the deadlock victim.
					result = DbErrorType.DeadlockError;
					break;

				case 1219: //Session has been disconnected because of a high priority DDL operation
					result = DbErrorType.SqlExceptionDdlDisconnection;
					break;

				case 1222: // Lock request time out period exceeded.
				case 5245: // DBCC CHECKTABLE(...) WITH TABLOCK lock request time out period exceeded.
					result = DbErrorType.LockTimeoutExpired;
					break;

				case 1223: // Cannot release the application lock (Database Principal: '%.*ls', Resource: '%.*ls') because it is not currently held.
					result = DbErrorType.CannotReleaseAppLockBecauseItIsNotCurrentlyHeld;
					break;

				case 1468: //The operation cannot be performed on database '%.*ls' because it is involved in a database mirroring session or an availability group.
					result = DbErrorType.AlwaysOnAccessError;
					break;

				case 1801: // Database '%.*ls' already exists. Choose a different database name.
					result = DbErrorType.DatabaseAlreadyExists;
					break;

				case 1802: // CREATE DATABASE failed. Some file names listed could not be created. Check related errors.
					result = GetCannotCreateDbFileErrorType(sqlServerExceptionErrors);
					break;

				case 1803: // The CREATE DATABASE statement failed. The primary file must be at least %d MB to accommodate a copy of the model database.
					result = DbErrorType.CreateDbFailedSizeCannotAccommodateCopyOfModelDb;
					break;

				case 1807: // Could not obtain exclusive lock on database '%.*ls'. Retry the operation later.
					result = DbErrorType.CouldNotObtainExclusiveLock;
					break;

				case 1912: // Could not proceed with index DDL operation on %S_MSG '%.*ls' because it conflicts with another concurrent operation that is already in progress on the object. The concurrent operation could be an online index operation on the same object or another concurrent operation that moves index pages like DBCC SHRINKFILE.
					result = DbErrorType.IndexOperationAlreadyInProgress;
					break;

				case 2010: // Cannot perform alter on '%.*ls' because it is an incompatible object type.
					result = DbErrorType.CannotAlterObjectOfIncompatibleType;
					break;

				case 2601: // Cannot insert duplicate key row in object '%.*ls' with unique index '%.*ls'. The duplicate key value is %ls.
					result = DbErrorType.CannotInsertDuplicateUniqueIndexKey;
					break;

				case 2627: // Violation of %ls constraint '%.*ls'. Cannot insert duplicate key in object '%.*ls'. The duplicate key value is %ls.
					result = DbErrorType.CannotInsertDuplicateConstraintKey;
					break;

				case 2714: // There is already an object named '%.*ls' in the database.
					result = DbErrorType.ObjectAlreadyExists;
					break;

				case 351:  //Column, parameter, or variable %.*ls. : Cannot find data type %.*ls.
				case 2715: //Column, parameter, or variable #%d: Cannot find data type %.*ls.
					result = DbErrorType.CannotFindDataType;
					break;

				case 2725: // An online operation cannot be performed for %S_MSG '%.*ls' because...
					result = DbErrorType.OnlineOperationCannotBePerformed;
					break;

				case 2801: // The definition of object '%s' has changed since it was compiled.
					result = DbErrorType.DefinitionOfObjectHasChangedSinceCompilation;
					break;

				case 2812: // Could not find stored procedure '%.*ls'.
					result = DbErrorType.CouldNotFindStoredProcedure;
					break;

				case 3038: // The file name "%ls" is invalid as a backup device name. Reissue the BACKUP statement with a valid file name.
					result = DbErrorType.CannotOpenBackupDeviceOrInvalidDeviceName;
					break;

				case 3137: // Database cannot be reverted. Either the primary or the snapshot names are improperly specified, all other snapshots have not been dropped, or there are missing files.
					result = DbErrorType.DatabaseCannotBeRevertedFromSnapshot;
					break;

				case 3140: // Could not adjust the space allocation for file '%ls'.
					result = DbErrorType.CouldNotAdjustTheSpaceAllocationForFile;
					break;

				case 3156: // File '%ls' cannot be restored to '%ls'. Use WITH MOVE to identify a valid location for the file.
					result = DbErrorType.FileCannotBeRestoredToPath;
					break;

				case 3201: // Cannot open backup device '%ls'. Operating system error %ls.
					result = DbErrorType.CannotOpenBackupDeviceOrInvalidDeviceName;
					break;

				case 3202: // Write on "%ls" failed: %ls
				case 3271: // A nonrecoverable I/O error occurred on file "%ls:" %ls.
					result = GetIoExceptionType(message);
					break;

				case 3303: // Remote harden of transaction '%s' (ID %d) started at %s in database '%.*ls' at LSN (%s) failed.
					result = DbErrorType.RemoteHardenOfTransactionFailure;
					break;

				case 3621: //The statement has been terminated.
					result = DbErrorType.StatementHasBeenTerminated;
					break;

				case 3702: // Cannot drop database '%.*ls' because it is currently in use.
					result = DbErrorType.CannotDropDatabaseInUse;
					break;

				case 3729: // Cannot drop schema '%s' because it is being referenced by object '%s'.
					result = DbErrorType.CannotDropSchemaReferencedByObject;
					break;

				case 3764: // Cannot alter the procedure '%s' because it is being used for Change Data Capture.
					result = DbErrorType.CannotAllowAlterCDCMetaObjects;
					break;

				case 3903: // The ROLLBACK TRANSACTION request has no corresponding BEGIN TRANSACTION.
					result = DbErrorType.RollBackTranHasNoCorrespondingBeginTran;
					break;

				case 3906: // Failed to update database "%.*ls" because the database is read-only.
					result = DbErrorType.CouldNotBeginTransactionAsDbIsReadOnly;
					break;

				case 3908: // Could not run BEGIN TRANSACTION in database '%.*ls' because the database is in emergency mode or is damaged and must be restarted.
					result = DbErrorType.DatabaseInEmergencyModeOrDamaged;
					break;

				case 3948: //The transaction was terminated because of the availability replica config/state change or because ghost records are being deleted on the primary and the secondary availability replica that might be needed by queries running under snapshot isolation. Retry the transaction.
					result = DbErrorType.GhostRecordsBeingDeleted;
					break;

				case 3958: // Transaction aborted when accessing versioned row in table '%ls' in database '%ls'. Requested versioned row was not found. Your tempdb is probably out of space. Please refer to BOL on how to configure tempdb for versioning.
					result = DbErrorType.TempdbIsOutOfSpace;
					break;

				case 3961: // Snapshot isolation transaction failed in database '%.*ls' because the object accessed by the statement has been modified by a DDL statement in another concurrent transaction since the start of this transaction.
					result = DbErrorType.MetadataHasBeenChanged;
					break;

				case 4030: // The medium on device <filepath> expires on <datetime> and cannot be overwritten
					result = DbErrorType.MediumOnDeviceHasNotExpiredAndCannotBeOverwritten;
					break;

				case 4060: // Cannot open database "%.*ls" requested by the login. The login failed.
					result = DbErrorType.CannotOpenDbRequestedInLogin;
					break;

				case 4083: // The connection was recovered and rowcount in the first query is not available. Please execute another query to get a valid rowcount.
					result = DbErrorType.ConnectionRecovered;
					break;

				case 4214: // BACKUP LOG cannot be performed because there is no current database backup.
					result = DbErrorType.CannotBackupLogWithNoCurrentDbBackup;
					break;

				case 4326: // The log in this backup set terminates at LSN <LSN number here>, which is too early to apply to the database
					result = DbErrorType.LogBackupIsTooEarlyToApplyToTheDatabase;
					break;

				case 4923: // ALTER TABLE DROP COLUMN failed because '%.*ls' is the only data column in table '%.*ls'. A table must have at least one data column.
					result = DbErrorType.CannotDropLastColumnInATable;
					break;

				case 4927: // Cannot alter column '%.*ls' to be data type %.*ls.
					result = DbErrorType.CannotAlterColumnToSpecificDataType;
					break;

				case 4928: // Cannot alter column '%.*ls' because it is '%ls'.
					if (sqlServerExceptionMessage.Contains("because it is 'REPLICATED'"))
					{
						result = DbErrorType.CannotAlterColumnBecauseItIsReplicated;
					}
					else
					{
						result = DbErrorType.CannotAlterColumn;
					}
					break;

				case 5011: // User does not have permission to alter database '%.*ls', the database does not exist, or the database is not in a state that allows access checks.
					result = DbErrorType.DatabaseDoesNotExist;
					break;

				case 5030: // The database could not be exclusively locked to perform the operation.
					result = DbErrorType.CouldNotObtainExclusiveLock;
					break;

				case 5064: // Changes to the state or options of database '%.*ls' cannot be made at this time. The database is in single-user mode, and a user is currently connected to it.
					result = DbErrorType.CannotAlterDbStateWhileInSingleUserMode;
					break;

				case 3023: // Backup, file manipulation operations (such as ALTER DATABASE ADD FILE) and encryption changes on a database must be serialized
					result = DbErrorType.BackupAndFileManipulationOperationsMustBeSerialized;
					break;

				case 5052: // ALTER DATABASE is not permitted while a database is in the Restoring state.
				case 5061: // ALTER DATABASE failed because a lock could not be placed on database '%.*ls'.
				case 5069: // ALTER DATABASE statement failed.
				case 5070: // Database state cannot be changed while other users are using the database '%.*ls'
					result = DbErrorType.CannotAlterDbWhileInUse;
					break;

				case 5074: // The %S_MSG '%.*ls' is dependent on %S_MSG '%.*ls'.
					result = DbErrorType.ObjectDependencyError;
					break;

				case 5105: // A file activation error occurred. The physical file name '%.*ls' may be incorrect.
					result = DbErrorType.DeviceActivationError;
					break;

				case 5149: // MODIFY FILE encountered operating system error %ls while attempting to expand the physical file '%ls'.
					result = DbErrorType.ModifyFileEncounteredOperatingSystemError;
					break;

				case 5170:  // Cannot create file '%ls' because it already exists.
					result = DbErrorType.CannotCreateFileBecauseItAlreadyExists;
					break;

				case 5313: // Synonym '%.*ls' refers to an invalid object.
					result = DbErrorType.SynonymRefersToAnInvalidObject;
					break;

				case 6005:  // SHUTDOWN is in progress.
					result = DbErrorType.GeneralNetworkError;
					break;

				case 6106: // Process ID %d is not an active process ID.
					result = DbErrorType.NotAnActiveProcessId;
					break;

				case 6107: // Only user processes can be killed.
					result = DbErrorType.OnlyUserProcessesCanBeKilled;
					break;

				case 6263: // Execution of user code in the .NET Framework is disabled. Enable "clr enabled" configuration option.
					result = DbErrorType.ClrDbOptionDisabled;
					break;

				case 6513: // Failed to initialize the Common Language Runtime (CLR) v2.0.50727 due to memory pressure.
					result = DbErrorType.FailedToInitClrDueToMemPressure;
					break;

				case 6522: // A .NET Framework error occurred during execution of user-defined routine or aggregate
					result = DbErrorType.ErrorOnUserDefinedRoutineOrAggregate;
					break;

				case 6533: // Out of memory happened while accessing a critical resource. The application domain in which the thread was running has been unloaded.
					result = DbErrorType.InsufficientSystemMemoryAccessingCriticalResource;
					break;

				case 7139: // Length of LOB data (%I64d) to be replicated exceeds configured maximum %ld.
					result = DbErrorType.LargeObjectSizeExceedsReplicationMaximum;
					break;

				case 7201: // Could not execute procedure on remote server '%.*ls' because SQL Server is not configured for remote access. Ask your system administrator to reconfigure SQL Server to allow remote access.
					result = DbErrorType.CouldNotExecuteOnServerNotConfiguredForRemoteAccess;
					break;

				case 7202: // Could not find server '%.*ls' in sys.servers. Verify that the correct server name was specified.
					var match = Regex.Match(sqlServerExceptionMessage, @"Could not find server '([\S\s]*)'");
					if (match.Success && match.Groups[1].Value.Equals("loopback", StringComparison.InvariantCultureIgnoreCase))
					{
						result = DbErrorType.LoopbackLinkedServerDoesNotExist;
					}
					else
					{
						result = DbErrorType.CouldNotFindLinkedServer;
					}
					break;

				case 7302: // Cannot create an instance of OLE DB provider "%s" for linked server "%s".
					result = DbErrorType.CannotCreateOleDbProviderForLinkedServer;
					break;

				case 7303: // Cannot initialize the data source object of OLE DB provider "%ls" for linked server "%ls".
					result = DbErrorType.CannotInitOleDbDataSourceObjForLinkedServer;
					break;

				case 7403: // The OLE DB provider "%s" has not been registered.
					result = DbErrorType.OleDbProviderNotRegistered;
					break;

				case 7411: // Server '%.*ls' is not configured for %ls.
					result = DbErrorType.ServerIsNotConfigured;
					break;

				case 7416: // Access to the remote server is denied because no login-mapping exists.
					result = DbErrorType.NoLoginMappingExistsOnRemoteServer;
					break;

				case 8009: //The incoming tabular data stream (TDS) remote procedure call (RPC) protocol stream is incorrect. Parameter %d ("%.*ls"): Data type 0x%02X is unknown.
					result = DbErrorType.DataTypeIsUnknown;
					break;

				case 8114: // Error converting data type %ls to %ls.
					result = DbErrorType.CannotConvertDataType;
					break;

				case 8115: // Arithmetic overflow error converting %ls to data type %ls.
					result = DbErrorType.ArithmeticOverflowConvertingToDataType;
					break;

				case 8144: // Procedure or function %s has too many arguments specified.
					result = DbErrorType.IncorrectNumberOfParametersForProcedure;
					break;

				case 8145: // %s is not a parameter for procedure %s.
					result = DbErrorType.IncorrectParameterForProcedure;
					break;

				case 8152: // string or binary data would be truncated.
				case 2628: // String or binary data would be truncated in table '%.*ls', column '%.*ls'. Truncated value: '%.*ls'.
					result = DbErrorType.StringOrBinaryDataWouldBeTruncated;
					break;

				case 8169: // Conversion failed when converting from a character string to uniqueidentifier.
					result = DbErrorType.FailedToConvertCharToUniqueidentifier;
					break;

				case 8618: // Query processor could not produce query plan because a worktable is required, and its minimum size exceeds the maximum allowable.
					result = DbErrorType.QueryProcessorCouldNotProduceQueryPlanBecauseMinimumWorktableWasTooLong;
					break;

				case 8622: // Query processor could not produce a query plan because of the hints defined in this query. Resubmit the query without specifying any hints and without using SET FORCEPLAN.
					result = DbErrorType.QueryProcessorCouldNotProduceQueryPlanBecauseOfHints;
					break;

				case 8623: // The query processor ran out of internal resources and could not produce a query plan.
					result = DbErrorType.RanOutOfInternalResources;
					break;

				case 8630: // Internal Query Processor Error: The query processor encountered an unexpected error during execution
					result = DbErrorType.InternalQueryProcessorError;
					break;

				case 8642: // The query processor could not start the necessary thread resources for parallel query execution.
					result = DbErrorType.CouldNotStartThreadResourcesForParallelQueryExecution;
					break;

				case 8645: // A timeout occurred while waiting for memory resources to execute the query in resource pool.
					result = DbErrorType.ExecuteQueryInResourcePoolTimeOut;
					break;

				case 9001: // The log for database '%.*ls' is not available.
					result = DbErrorType.LogUnavailable;
					break;

				case 9002: // The transaction log for database '%ls' is full due to '%ls'.
					result = DbErrorType.LogIsFull;
					break;

				case 9005: // Invalid parameter passed to OpenRowset(DBLog, ...).
					result = DbErrorType.InvalidParameterPassedToOpenRowset;
					break;

				case 10060: // TCP Provider: A connection attempt failed because the connected party did not properly respond after a period of time, or established connection failed because connected host has failed to respond.
					result = DbErrorType.TCPProviderConnectionAttemptFailed;
					break;

				case 10314: // An error occurred in the Microsoft .NET Framework while trying to load assembly id %d...
					result = DbErrorType.ErrorLoadingUntrustedAssembly;
					break;

				case 15007: // '%s' is not a valid login or you do not have permission
					result = DbErrorType.InvalidLoginOrNoPermission;
					break;

				case 15023: // User, group, or role '%s' already exists in the current database.
					result = DbErrorType.DatabasePrincipalAlreadyExists;
					break;

				case 15025: // The server principal '%s' already exists.
					result = DbErrorType.ServerPrincipalAlreadyExists;
					break;

				case 15048: // Valid values of the database compatibility level are 90, 100, or 110.
					result = DbErrorType.DatabaseCompatibilityError;
					break;

				case 15051: // Cannot rename the table because it is published for replication.
					result = DbErrorType.CannotRenameTableBecauseItIsPublishedForReplication;
					break;

				case 15151: // Cannot %S_MSG the %S_MSG '%.*ls', because it does not exist or you do not have permission.
					if (sqlServerExceptionMessage.StartsWith("Cannot drop the login"))   // Exception message
					{
						result = DbErrorType.ServerPrincipalDoesNotExist;
					}
					else
					{
						result = DbErrorType.DatabasePrincipalDoesNotExist;
					}
					break;

				case 15174: // Login '%s' owns one or more database(s). Change the owner of the database(s) before dropping the login.
					result = DbErrorType.CannotDropLoginWhoOwnsDatabase;
					break;

				case 15175: // Login '%s' is aliased or mapped to a user in one or more database(s). Drop the user or alias before dropping the login.
					result = DbErrorType.CannotDropLoginMappedToDbUser;
					break;

				case 15281: // SQL Server blocked access to %S_MSG '%ls' of component '%.*ls' because this component is turned off as part of the security configuration for this server.
					result = DbErrorType.CmdShellDbOptionDisabled;
					break;

				case 15406: // Cannot execute as the server principal because the principal "%.*ls" does not exist, this type of principal cannot be impersonated, or you do not have permission.
					result = DbErrorType.CannotExecuteAsServerPrincipal;
					break;

				case 15434: // Could not drop login '%s' as the user is currently logged in.
					result = DbErrorType.CannotDropLoginCurrentlyLoggedIn;
					break;

				case 15517: // Cannot execute as the database principal because the principal "%.*ls" does not exist, this type of principal cannot be impersonated, or you do not have permission.
					result = DbErrorType.CannotExecuteAsDatabasePrincipal;
					break;

				case 15562: // The module being executed is not trusted. Either the owner of the database of the module needs to be granted authenticate permission, or the module needs to be digitally signed.\r\nThe statement has been terminated.
					result = DbErrorType.ModuleBeingExecutedIsNotTrusted;
					break;

				case 17892: // Logon failed for login '%.*ls' due to trigger execution.%.*ls
					result = DbErrorType.LogonFailedForLogin;
					break;

				case 18401: // Login failed for user '%.*ls'. Reason: Server is in script upgrade mode. Only administrator can connect at this time.%.*ls
					result = DbErrorType.ServerIsInScriptMode;
					break;

				case 18452: // Login failed. The login is from an untrusted domain and cannot be used with Windows Authentication.%.*ls
					result = DbErrorType.LoginFailedForUntrustedDomain;
					break;

				case 18456: // Login failed for user '%.*ls'.%.*ls%.*ls
					result = DbErrorType.LoginFailedForUser;
					break;

				case 18470: // Login failed for user '%.*ls'. Reason: The account is disabled.%.*ls
					result = DbErrorType.LoginDisabled;
					break;

				case 18486: // Login failed for user '%.*ls' because the account is currently locked out. The system administrator can unlock it. %.*ls
					result = DbErrorType.LoginFailedBecauseItIsLockedOut;
					break;

				case 18752: // Only one Log Reader Agent or log-related procedure can be connected to the database at a time.
					result = DbErrorType.OnlyOneLogReaderAgentCanConnectToDatabase;
					break;

				case 18847: // Cannot retrieve the peer-to-peer database information. Contact Customer Support Services.
					result = DbErrorType.CannotRetrievePeerToPeerDbInfo;
					break;

				case 18767: // The specified begin LSN {%08lx:%08lx:%04lx} for replication log scan occurs before replbeginlsn {%08lx:%08lx:%04lx}.
					result = DbErrorType.InvalidBeginLsnForCdcCommitRecord;
					break;

				case 22003: // xp_servicecontrol returned an error
					result = DbErrorType.ServiceControlError;
					break;

				case 22022: // Request to stop job cdc.%s refused because the job is not currently running
					result = DbErrorType.RequestToStopCdcJobRefused;
					break;

				case 22832: // Column name or number of supplied values does not match table definition.
				case 22930: // Columns specified in the captured column list could not be mapped to columns in source table '%.*ls'. Verify that the columns specified in the parameter @captured_column_list are delimited properly and match columns in the source table.
					result = DbErrorType.CdcDataOutdated;
					break;

				case 22851: // Could not update cdc.change_tables to indicate a change in the low water mark for database '%s'.
					result = DbErrorType.CouldNotUpdateCdcChangeTables;
					break;

				case 22901: // The database '%s' is not enabled for Change Data Capture. Ensure that the correct database context is set and retry the operation.
				case 22910: // The cleanup request for database '%s' failed.  The database is not enabled for Change Data Capture.
					result = DbErrorType.DatabaseNotEnabledForChangeDataCapture;
					break;

				case 22903: // Another connection is already running 'sp_replcmds' for Change Data Capture in the current database.
					result = DbErrorType.AnotherConnectionIsRunningSpReplcmdsForCdc;
					break;

				case 22911: // The capture job cannot be used by Change Data Capture to extract changes from the log when transactional replication is also enabled on the same database. When Change Data Capture and transactional replication are both enabled on a database, use the logreader agent to extract the log changes.
					result = DbErrorType.CdcFailedDueToTransactionalReplication;
					break;

				case 22939: // The parameter @supports_net_changes is set to 1, but the source table does not have a primary key defined and no alternate unique index has been specified.
					result = DbErrorType.CdcSourceTableHasNeitherPrimaryKeyNorUniqueIndex;
					break;

				case 27203: // Failed to deploy project. For more information, query the operation_messages view for the operation identifier '%I64d'.
					result = DbErrorType.SsisFailedToDeployProject;
					break;

				case 33009: // The database owner SID recorded in the master database differs from the database owner SID recorded in database '%.*ls'. You should correct this situation by resetting the owner of database '%.*ls' using the ALTER AUTHORIZATION statement.
					result = DbErrorType.OwnerSIDDiffersFromMaster;
					break;

				case 3104:  // RESTORE cannot operate on database '%ls' because it is configured for database mirroring or has joined an availability group. If you intend to restore the database, use ALTER DATABASE to remove mirroring or to remove the database from its availability group.
					result = DbErrorType.CannotOperateOnDatabaseWhenAlreadyConfiguredForMirroringOrJoinedToAvailabilityGroup;
					break;

				case 35280: // Database '%.*ls' cannot be added to availability group '%.*ls'. The database is already joined to the specified availability group. Verify that the database name is correct and that the database is not joined to an availability group, then retry the operation.
				case 41145: // Cannot join database '%.*ls' to availability group '%.*ls'. The database has already joined the availability group. This is an informational message. No user action is required.
					result = DbErrorType.DatabaseAlreadyJoinedToAvailabilityGroup;
					break;

				case 50000:
					result = GeneralUserException(message);
					break;

				case 58008: // insert trigger failure
					result = DbErrorType.RatingRateEntryOverlap;
					break;

				case 58009:
					result = DbErrorType.RatingRateLineInvalidChargeCode;
					break;

				case 58101:
					result = DbErrorType.CrmOpportunityScopeOverlap;
					break;

				case -2146893019:
				case -2146893022:
					result = DbErrorType.TlsCertificateError;
					break;

				default:
					result = DbErrorType.NotHandled;
					break;
			}

			return result;
		}

		static DbErrorType GeneralUserException(string message)
		{
			if (message.Contains("The module being executed is not trusted. Either the owner of the database of the module needs to be granted authenticate permission, or the module needs to be digitally signed."))  // This is an exception message thrown by the SQL Server
			{
				return DbErrorType.ModuleBeingExecutedIsNotTrusted;
			}

			return DbErrorType.GeneralUserException;
		}

		/// <summary>
		/// Errors not in sys.messages
		/// http://msdn.microsoft.com/en-us/library/ms365262(v=sql.105).aspx
		/// </summary>
		[SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity")]
		DbErrorType GetNonServerErrorType()
		{
			var result = DbErrorType.NotHandled;

			switch (sqlServerExceptionNumber)
			{
				case 3617:
				case -2:
					result = DbErrorType.TimeoutExpired;
					break;
				case 0:
					if (sqlServerExceptionMessage.Contains("The connection is broken and recovery is not possible")) // Exception message
					{
						result = DbErrorType.AllAttemptsFailed;
					}
					else
					{
						result = DbErrorType.SevereError;
					}
					break;

				case 6:
				case 17:
				case 53:
				case 11001: // switching to multisubnetfailover now uses TCP for all connections. No shared memory so this error occurs
					result = DbErrorType.ServerDoesNotExist;
					break;

				case -1:
					var linkedServerErrorMessageRegex = new Regex(@"provider\s+\"".+?\""\s+for\s+linked\s+server\s+\"".+?\""\s+returned\s+message", RegexOptions.IgnoreCase);
					if (linkedServerErrorMessageRegex.IsMatch(sqlServerExceptionMessage))
					{
						result = DbErrorType.LinkedServerReturnedErrorMessage;
					}
					else
					{
						result = DbErrorType.GeneralNetworkError;
					}
					break;

				case 59:
				case 64:
				case 1236:
				case 18461:
				case 10054:
				case 258:
					result = DbErrorType.GeneralNetworkError;
					break;

				case 823:
				case 824:
					result = DbErrorType.HardwareFault;
					break;

				case 121 when (sqlServerExceptionLineNumber == 0 && loginErrorRegex.IsMatch(sqlServerExceptionMessage)):
					result = DbErrorType.IncomingRemoteConnectionFailed;
					break;

				// 233 is an general error number, just found that it only happens when to connect to Local DB server and fail to call SqlConnection.Open() for the first time. the real error number maybe 18456 or 18470 in error log .
				case 233 when (sqlServerExceptionLineNumber == 0 && loginErrorRegex.IsMatch(sqlServerExceptionMessage)):
					result = DbErrorType.SqlServerAuthenticationModeIsOff;
					break;
				// ADO.NET handshake errors => LineNumber = 0
				// A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was not found or was not accessible. Verify that the instance name is correct and that SQL Server is configured to allow remote connections. (provider: Named Pipes Provider, error: 40 - Could not open a connection to SQL Server)
				// A transport-level error has occurred when sending the request to the server. (provider: Named Pipes Provider, error: 0 - An unexpected network error occurred.)
				// A transport-level error has occurred when sending the request to the server. (provider: Named Pipes Provider, error: 0 - The pipe is being closed.)
				// A transport-level error has occurred when sending the request to the server. (provider: Shared Memory Provider, error: 0 - No process is on the other end of the pipe.)
				// A transport-level error has occurred when sending the request to the server. (provider: TCP Provider, error: 0 - An established connection was aborted by the software in your host machine.)
				// A connection was successfully established with the server, but then an error occurred during the pre-login handshake. (provider: TCP Provider, error: 0 - The specified network name is no longer available.)
				default:
					if (sqlServerExceptionLineNumber == 0 && networkErrorRegex.IsMatch(sqlServerExceptionMessage))
					{
						result = DbErrorType.GeneralNetworkError;
					}
					break;
			}

			return result;
		}

		public static bool IsNetworkError(DbErrorType type)
		{
			return (type == DbErrorType.ServerDoesNotExist || type == DbErrorType.GeneralNetworkError || type == DbErrorType.DataTypeIsUnknown);
		}

		static readonly Regex networkErrorRegex = new Regex(@"(network|transport|handshake).*error", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		static readonly Regex loginErrorRegex = new Regex(@"(A connection was successfully established with the server, but then an error occurred during the login process).*error", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		/// <summary>
		/// Matches error 3202 and 3271 messages with OS error 112
		/// </summary>
		DbErrorType GetIoExceptionType(string exceptionMessage)
		{
			Argument.NotNull(exceptionMessage, nameof(exceptionMessage)); // Suggested By ReviewBot

			Regex statusRegex = new Regex(@"\b112\b", RegexOptions.IgnoreCase);

			if (statusRegex.IsMatch(exceptionMessage))
			{
				return DbErrorType.DiskFullIoError;
			}

			return DbErrorType.IoGenericError;
		}

		/// <summary>
		/// 1802 - Cannot Create DB File Error
		/// </summary>
		static DbErrorType GetCannotCreateDbFileErrorType(IEnumerable<SqlErrorWrapper> errors)
		{
			if (errors == null)
			{
				throw new ArgumentNullException(nameof(errors));
			}

			var result = DbErrorType.NotHandled;

			var firstTwoErrors = errors.Take(2).ToList();

			if (firstTwoErrors.Count == 2)
			{
				var error1 = firstTwoErrors[0];
				var error2 = firstTwoErrors[1];

				if (error2.Number == 1802)
				{
					if (error1.Number == 5170)
					{
						result = DbErrorType.CannotCreateFileBecauseItAlreadyExists;
					}
					else if (error1.Number == 5149)
					{
						result = DbErrorType.ModifyFileEncounteredOperatingSystemError;
					}
				}
			}

			return result;
		}

		#region Constraint Violation Exception

		DbErrorType GetConstraintViolationType(string exceptionMessage)
		{
			Argument.NotNull(exceptionMessage, nameof(exceptionMessage));

			var type = GetOperationAndConstraintTypeIfConstraintViolation(exceptionMessage);

			switch (type.OperationType)
			{
				case "INSERT":
					return (type.ConstraintType == "CHECK") ? DbErrorType.InsertConflictedWithCheckConstraint : DbErrorType.InsertConflictedWithForeignKey;

				case "UPDATE":
					return (type.ConstraintType == "CHECK") ? DbErrorType.UpdateConflictedWithCheckConstraint : DbErrorType.UpdateConflictedWithForeignKey;

				case "DELETE":
					return DbErrorType.DeleteConflictedWithForeignKey;
			}

			return DbErrorType.StatementConflictedWithConstraintGenericError;
		}

		ConstraintInfo GetOperationAndConstraintTypeIfConstraintViolation(string exceptionMessage)
		{
			Argument.NotNull(exceptionMessage, nameof(exceptionMessage)); // Suggested By ReviewBot

			string violationPattern = @"^(The\s+)?(?<SqlCmd>INSERT|UPDATE|DELETE)\b.+?\b(?<ConstraintType>FOREIGN KEY|REFERENCE|CHECK)\b[^'""]+['""]";      // Developer Message
			Regex regex = new Regex(violationPattern, RegexOptions.IgnoreCase);
			Match match = regex.Match(exceptionMessage);

			if (match.Success)
			{
				var sqlCmdGroup = match.Groups["SqlCmd"];
				var constraintTypeGroup = match.Groups["ConstraintType"];
				return new ConstraintInfo(sqlCmdGroup.ToString().ToUpper(), constraintTypeGroup.ToString().ToUpper());
			}

			return new ConstraintInfo("", "");
		}

		struct ConstraintInfo
		{
			public ConstraintInfo(string operationType, string constraintType)
			{
				this.OperationType = operationType;
				this.ConstraintType = constraintType;
			}

			public readonly string OperationType;
			public readonly string ConstraintType;
		}

		#endregion

		#region User Mapping Error Information

		/// <summary>
		/// The server principal "xx" is not able to access the database "yy" under the current security context.
		/// The SELECT permission was denied on the object 'XXX', database 'YYY', schema 'ZZZ'.
		/// CREATE TABLE permission denied in database 'XXX'.
		/// </summary>
		internal string GetDatabaseFromSecurityError()
		{
			const string dbGroupPattern = @"database\s*['""](?<DB>[^'""]+)['""]"; // DB Error Regex Pattern
			const string permissionDeniedPattern = @"^(?:\w|\s)+permission(\w|\s)+denied(\w|\s)+"; // DB Error Regex Pattern

			string regexPattern = null;

			switch (ExceptionType)
			{
				case DbErrorType.LoginIsNotAbleToAccessDatabaseUnderCurrentSecurityContext:
					regexPattern = @"^(?:\w|\s)+server\s+principal\s*['""][^'""]+['""]\s*(?:\w|\s)+" + dbGroupPattern + @"\s*(?:\w|\s)+security\s+context"; // DB Error Regex Pattern
					break;
				case DbErrorType.PermissionDeniedOnObject:
					regexPattern = permissionDeniedPattern + @"object\s*['""][^'""]+['""]\s*,\s*" + dbGroupPattern; // DB Error Regex Pattern
					break;
				case DbErrorType.PermissionDeniedInDatabase:
					regexPattern = permissionDeniedPattern + dbGroupPattern; // DB Error Regex Pattern
					break;
				default:
					throw new InvalidOperationException("Must be denied permission/access exception type"); // Exception Message
			}

			var regex = new Regex(regexPattern, RegexOptions.IgnoreCase);
			var match = regex.Match(sqlServerExceptionMessage);

			if (match.Success)
			{
				var dbGroup = match.Groups["DB"];
				return dbGroup.Value;
			}

			return null;
		}

		internal string GetPrincipalFromError()
			=> FindPrincipalFromErrorMessageParsing();

		string FindPrincipalFromErrorMessageParsing()
		{
			string principalRegex;
			switch (ExceptionType)
			{
				case DbErrorType.CannotExecuteAsServerPrincipal:
				case DbErrorType.CannotExecuteAsDatabasePrincipal:
					principalRegex = @"^cannot\s+execute\s+as\s+the\s+(server|database)\s+principal\s+because\s+the\s+principal\s+['""](?<PRINCIPAL>[^'""]+)['""]"; // DB Error Regex Pattern
					break;

				case DbErrorType.LoginIsNotAbleToAccessDatabaseUnderCurrentSecurityContext:
					principalRegex = @"the\s+(server|database)\s+principal\s+""(?<PRINCIPAL>[^'""]+)""";
					break;

				default:
					return null;
			}

			var match = Regex.Match(sqlServerExceptionMessage, principalRegex, RegexOptions.IgnoreCase);
			return match.Success ?
				match.Groups["PRINCIPAL"].Value :
				null;
		}

		internal string GetSchemaFromError()
			=> FindSchemaFromErrorMessageParsing();

		string FindSchemaFromErrorMessageParsing()
		{
			string principalRegex;
			switch (ExceptionType)
			{
				case DbErrorType.PermissionDeniedOnObject:
					principalRegex = @", schema '(?<SCHEMA>[^'""]+)'.$"; // DB Error Regex Pattern
					break;

				default:
					return null;
			}

			var match = Regex.Match(sqlServerExceptionMessage, principalRegex, RegexOptions.IgnoreCase);
			return match.Success ?
				match.Groups["SCHEMA"].Value :
				null;
		}

		internal string GetPermissionFromError()
			=> FindPermissionFromErrorMessageParsing();

		string FindPermissionFromErrorMessageParsing()
		{
			string principalRegex;
			switch (ExceptionType)
			{
				case DbErrorType.PermissionDeniedInDatabase:
					principalRegex = @"^(?<PERMISSON>[^'""]+) permission denied in database"; // DB Error Regex Pattern
					break;

				case DbErrorType.PermissionDeniedOnObject:
					principalRegex = @"^The (?<PERMISSON>[^'""]+) permission was denied on the object"; // DB Error Regex Pattern
					break;

				default:
					return null;
			}

			var match = Regex.Match(sqlServerExceptionMessage, principalRegex, RegexOptions.IgnoreCase);
			return match.Success ?
				match.Groups["PERMISSON"].Value :
				null;
		}

		public static bool IsCannotExecuteAsDatabasePrincipalError(DbErrorType type)
		{
			return type == DbErrorType.CannotExecuteAsDatabasePrincipal;
		}

		public static bool IsDatabaseInSingleUserModeError(DbErrorType type)
		{
			return type == DbErrorType.DatabaseInSingleUserModeAndAlreadyOpen
				|| type == DbErrorType.CannotOpenDbRequestedInLogin;
		}

		#endregion // User Mapping Error Information

		#endregion

		#region Friendly Error Message

		string DoGetUserFriendlyMessage(DbConnection connection)
		{
			string result = "";

			switch (ExceptionType)
			{
				case DbErrorType.CannotInsertDuplicateUniqueIndexKey:
				case DbErrorType.CannotInsertDuplicateConstraintKey:
					result = GetDuplicateKeyErrorMessage(connection);
					break;

				case DbErrorType.InsertConflictedWithForeignKey:
				case DbErrorType.UpdateConflictedWithForeignKey:
				case DbErrorType.DeleteConflictedWithForeignKey:
					result = GetInsertUpdateOrDeleteConflictedWithFKErrorMessage(ExceptionType == DbErrorType.DeleteConflictedWithForeignKey, connection);
					break;

				case DbErrorType.InsertConflictedWithCheckConstraint:
				case DbErrorType.UpdateConflictedWithCheckConstraint:
					result = GetInsertOrUpdateConflictedWithCheckConstraintErrorMessage(connection);
					break;

				case DbErrorType.GeneralNetworkError:
				case DbErrorType.DataTypeIsUnknown:
					result = GetGeneralNetworkErrorMessage();
					break;

				case DbErrorType.LogIsFull:
				case DbErrorType.DbFilegroupIsFull:
					result = DbFileIsFullErrorMessage(connection);
					break;

				case DbErrorType.DeadlockError:
					result = DeadlockErrorMessage();
					break;

				case DbErrorType.TimeoutExpired:
					result = TimeoutExpiredErrorMessage();
					break;

				case DbErrorType.LockTimeoutExpired:
					result = LockTimeoutExpiredErrorMessage();
					break;

				case DbErrorType.CannotContinueScanWithNoLockDueToDataMovement:
					result = sqlServerExceptionMessage;
					break;

				case DbErrorType.LogUnavailable:
				case DbErrorType.InaccessibleFiles:
					result = GetInaccessibleFilesFriendlyMessage();
					break;

				case DbErrorType.DeviceActivationError:
					result = GetDeviceActivationErrorFriendlyMessage();
					break;

				case DbErrorType.DiskFullIoError:
					result = DiskFullBackupErrorFriendlyMessage;
					break;

				case DbErrorType.ServerIsInScriptMode:
					result = ServerIsInScriptModeFriendlyMessage(connection);
					break;

				case DbErrorType.CannotOpenDbRequestedInLogin:
				case DbErrorType.LoginFailedForUser:
				case DbErrorType.LoginFailedBecauseItIsLockedOut:
					result = GetLoginFailedForUser(connection);
					break;

				case DbErrorType.RemoteHardenOfTransactionFailure:
					result = RemoteHardenOfTransactionFailureMessage;
					break;

				case DbErrorType.DatabaseIsBeingRecovered:
				case DbErrorType.DatabaseCannotBeAutostartedDuringServerShutdownOrStartup:
				case DbErrorType.AllAttemptsFailed:
				case DbErrorType.LoginFailedForUntrustedDomain:
					result = TryAgainLaterMessage;
					break;

				case DbErrorType.DatabaseOffline:
				case DbErrorType.CannotOpenDatabaseMarkedAsSuspect:
				case DbErrorType.DatabaseIsInTheMiddleOfRestore:
					result = DatabaseOfflineMessage;
					break;

				case DbErrorType.InsufficientSystemMemoryToRunQuery:
					result = InsufficientSystemMemoryToRunQueryMessage;
					break;

				case DbErrorType.InsufficientSystemMemoryAccessingCriticalResource:
					result = InsufficientSystemMemoryAccessingCriticalResourceMessage;
					break;

				case DbErrorType.InsufficientMemoryInBufferPool:
					result = InsufficientMemoryInBufferPoolMessage;
					break;

				case DbErrorType.CannotDropLoginCurrentlyLoggedIn:
					result = CannotDropLoginCurrentlyLoggedInMessage;
					break;

				case DbErrorType.SevereError:
				case DbErrorType.AlwaysOnAccessError:
				case DbErrorType.InternalQueryProcessorError:
					result = ContactSystemAdminErrorMessage;
					break;

				case DbErrorType.ServerDoesNotExist:
					result = SqlServerOfflineMessage;
					break;

				case DbErrorType.LargeObjectSizeExceedsReplicationMaximum:
				case DbErrorType.ClrDbOptionDisabled:
				case DbErrorType.CmdShellDbOptionDisabled:
					result = DatabaseServerConfigurationOptionsIncorrect;
					break;

				case DbErrorType.LoginDisabled:
					result = LoginDisabledFriendlyMessage;
					break;

				case DbErrorType.SynonymRefersToAnInvalidObject:
					result = InvalidSynonymFriendlyMessage;
					break;

				case DbErrorType.FailedToInitClrDueToMemPressure:
					result = FailedToInitClrDueToMemPressureFriendlyMessage;
					break;

				case DbErrorType.CouldNotBeginTransactionAsDbIsReadOnly:
					result = sqlServerExceptionMessage;
					break;

				case DbErrorType.LoginIsNotAbleToAccessDatabaseUnderCurrentSecurityContext:
					result = GetLoginIsNotAbleToAccessDatabaseUnderCurrentSecurityContextErrorMessage(connection);
					break;

				case DbErrorType.HardwareFault:
					result = HardwareFaultMessage;
					break;

				case DbErrorType.CouldNotStartThreadResourcesForParallelQueryExecution:
					result = CouldNotStartThreadResourcesForParallelQueryExecutionMessage;
					break;

				case DbErrorType.IncomingRemoteConnectionFailed:
					result = IncomingRemoteConnectionFailedMessge;
					break;

				case DbErrorType.SqlServerAuthenticationModeIsOff:
					result = SqlServerAuthenticationModeIsOffMessgae;
					break;

				case DbErrorType.CannotInsertNullIntoNonNullableColumn:
					result = sqlServerExceptionMessage.SplitByLine().FirstOrDefault();
					break;

				case DbErrorType.FatalError:
					result = FatalErrorMessage;
					break;

				case DbErrorType.CannotInitOleDbDataSourceObjForLinkedServer:
					result = sqlServerExceptionMessage;
					break;

				case DbErrorType.BackupAndFileManipulationOperationsMustBeSerialized:
					result = BackupErrorMessage;
					break;

				case DbErrorType.InvalidObjectName:
					result = GetInvalidObjectNameFriendlyMessage();
					break;

				case DbErrorType.TCPProviderConnectionAttemptFailed:
					result = sqlServerExceptionMessage;
					break;

				case DbErrorType.TlsCertificateError:
					result = TlsCertificateErrorMessage;
					break;

				case DbErrorType.CouldNotAdjustTheSpaceAllocationForFile:
					result = sqlServerExceptionMessage;
					break;

				case DbErrorType.ExecuteQueryInResourcePoolTimeOut:
					result = sqlServerExceptionMessage;
					break;

				case DbErrorType.LoopbackLinkedServerDoesNotExist:
					result = ConfigureServerHasNotBeenRun;
					break;
			}

			return result;
		}

		string BackupErrorMessage
		{
			get
			{
				return "Database backup or file manipulation operation failed, possibly caused by other concurrent running backup service tasks or database shrink operations.\r\nMessage: " + sqlServerExceptionMessage; // Exception Message
			}
		}

		string ContactSystemAdminErrorMessage
		{
			get
			{
				return "Please contact your systems administrator to check the database for errors.\r\nMessage: " + sqlServerExceptionMessage; // Exception Message
			}
		}

		string SqlServerOfflineMessage
		{
			get
			{
				return "SQL Server appears to be offline. Please contact your systems administrator.\r\nMessage: " + sqlServerExceptionMessage;    // Exception Message
			}
		}

		string CannotDropLoginCurrentlyLoggedInMessage
		{
			get
			{
				return "Please log out for the login you are trying to drop.\r\nMessage: " + sqlServerExceptionMessage;    // Exception Message
			}
		}

		string InsufficientSystemMemoryToRunQueryMessage
		{
			get
			{
				return "The application has encountered a transient SQL issue. Try again or restart the application.\r\nMessage: " + sqlServerExceptionMessage;    // Exception Message
			}
		}

		string InsufficientSystemMemoryAccessingCriticalResourceMessage
		{
			get
			{
				return "SQL Server has experienced an issue because there is not enough memory. Please contact your systems administrator.\r\nMessage: " + sqlServerExceptionMessage;    // Exception Message
			}
		}

		string InsufficientMemoryInBufferPoolMessage
		{
			get
			{
				return "SQL Server has experienced an issue because there is not enough memory in the Buffer Pool. Please try again or contact your systems administrator if the issue persists.\r\nMessage: " + sqlServerExceptionMessage;    // Exception Message
			}
		}

		string RemoteHardenOfTransactionFailureMessage
		{
			get
			{
				return "The application has encountered a remote harden of transaction failure. Please retry your past action.\r\n" + sqlServerExceptionMessage; // Exception Message
			}
		}

		string TryAgainLaterMessage
		{
			get
			{
				return "Try again later.\r\n" + sqlServerExceptionMessage; // Exception Message
			}
		}

		string DatabaseOfflineMessage
		{
			get
			{
				return "Sql Server failed to connect to a database.\r\nPlease contact your system administrator.\r\n\r\nMessage: " + sqlServerExceptionMessage; // Exception Message
			}
		}

		string DiskFullBackupErrorFriendlyMessage
		{
			get
			{
				string result =
					"\r\nFailed to create backup file: DISK IS FULL.\r\n" +     // Exception Message
					"\r\nPlease free some space in the disk or change the database backup location.\r\n" +
					sqlServerExceptionMessage;
				return result;
			}
		}

		string DatabaseServerConfigurationOptionsIncorrect
		{
			get
			{
				return
					"A database server configuration option is incorrectly set.\r\n" +      // Exception Message
					"Message: " + sqlServerExceptionMessage + "\r\n\r\n" +
					"This may happen due to a recent server migration.\r\n" +
					"Please contact your system administrator in order to set the configuration options as described in the following note:\r\n\r\n" +
					"http://www.cargowise.com/Documents/UpdateNotes/ediEnterpriseUpdateNote20071102.pdf";
			}
		}

		string ConfigureServerHasNotBeenRun
		{
			get
			{
				return "The system cannot perform this operation as a crucial server configuration object is missing. The 'ConfigureServer' procedure has not been run on this server. Please contact your system administrator.";
			}
		}

		string GetDeviceActivationErrorFriendlyMessage()
		{
			return "Sql Server failed to activate a database correctly.\r\nPlease contact your system administrator\r\n\r\nMessage: " + sqlServerExceptionMessage; // Exception Message
		}

		string GetInaccessibleFilesFriendlyMessage()
		{
			return "Sql Server failed to access one or more database files correctly.\r\nPlease contact your system administrator\r\n\r\nMessage: " + sqlServerExceptionMessage;   // Exception Message
		}

		string DoGetIndexNameIfUniqueIndexViolation()
		{
			string result = "";

			if (ExceptionType == DbErrorType.CannotInsertDuplicateUniqueIndexKey)
			{
				var message = sqlServerExceptionMessage;
				result = MetaData.GetUniqueIndexNameFromErrorMessage(message);
			}

			return result;
		}

		string LoginDisabledFriendlyMessage
		{
			get
			{
				return "The application database login is disabled. Please check if an upgrade is in progress.\r\nMessage: " + sqlServerExceptionMessage;  // Exception Message
			}
		}

		string InvalidSynonymFriendlyMessage
		{
			get
			{
				return sqlServerExceptionMessage + "\r\nPlease contact your system administrator in order to fix this issue by going to Help > Database Administration > Recreate Database Synonyms.";   // Exception Message
			}
		}

		string FailedToInitClrDueToMemPressureFriendlyMessage
		{
			get
			{
				return sqlServerExceptionMessage + " Failed to initialize the Common Language Runtime (CLR) v2.0.50727 due to memory pressure. You need to restart SQL Server to fix this issue."; // Exception Message
			}
		}

		string HardwareFaultMessage
		{
			get
			{
				return string.Format(CultureInfo.CurrentCulture, "The following error occured due to a hardware fault. Please contact your administrator\r\n{0}", sqlServerExceptionMessage); // Exception Message
			}
		}

		string CouldNotStartThreadResourcesForParallelQueryExecutionMessage
		{
			get
			{
				return "The query processor could not start the necessary thread resources for parallel query execution."; // Exception Message
			}
		}

		string IncomingRemoteConnectionFailedMessge
		{
			get
			{
				//For details: https://social.msdn.microsoft.com/Forums/sqlserver/en-US/f260c37d-0ab5-47d9-983f-b072ac0b8961/a-connection-was-successfully-established-with-the-server-but-then-an-error-occurred-during-the?forum=sqldatabaseengine
				return "This is related to network configuration on your server, network and firewall."; // Exception Message
			}
		}

		string SqlServerAuthenticationModeIsOffMessgae
		{
			get
			{
				return "The \"SQL Server and Windows Authentication mode\" on your server might be disabled. Enable it to fix this issue."; // Exception Message
			}
		}

		string FatalErrorMessage => string.Format(CultureInfo.CurrentCulture, "{0}\r\n{1}", sqlServerExceptionMessage, "There are some errors on your server. Please contact your system administrator to check your server windows event logs."); // Exception Message

		string TlsCertificateErrorMessage => Res.GetString("F15D2F96-1C3A-4AD1-8D15-C700FD1D846C", "{0}\r\n\r\nTLS certificate not configured or the subject name of the TLS certificate assigned to your SQL Server Instance does not match your server name {1}. Please contact your system administrator.", sqlServerExceptionMessage, sqlServerExceptionServer);

		#region CannotCreateFileBecauseItAlreadyExists

		public string GetFileFullNameOnCannotCreateFileError()
		{
			if (ExceptionType != DbErrorType.CannotCreateFileBecauseItAlreadyExists)
			{
				throw new InvalidOperationException("This method can only be called for [CannotCreateFileBecauseItAlreadyExists] exceptions."); // Exception Message
			}

			string result = "";

			string pattern = "(?<=^Cannot create file ')([^']*)(?=' because it already exists)";    // Exception Message
			Regex fileRegex = new Regex(pattern, RegexOptions.IgnoreCase);
			Match fileMatch = fileRegex.Match(sqlServerExceptionMessage);

			if (fileMatch.Success)
			{
				result = fileMatch.Value;
			}

			return result;
		}

		#endregion

		#region Duplicate Key

		protected string GetDuplicateKeyErrorMessage(DbConnection connection)
		{
			string result = string.Empty;

			string uniqueIndexName = MetaData.GetUniqueIndexNameFromErrorMessage(sqlServerExceptionMessage);
			if (!string.IsNullOrWhiteSpace(uniqueIndexName))
			{
				if (connection != null)
				{
					var index = IndexLoader.LoadTop1(connection, null, null, uniqueIndexName);
					if (index != null
						&& !string.IsNullOrWhiteSpace(index.TableName)
						&& index.KeyColumns != null
						&& index.KeyColumns.Length > 0
						)
					{
						var keyColumns = new StringCollection();
						keyColumns.AddRange(index.KeyColumns.Select(col => col.Name).ToArray());
						result = GetFormattedDuplicateKeyErrorMessage(index.TableName, keyColumns);
					}
				}

				if (uniqueIndexName.StartsWith("PK_"))
				{
					result =
						Res.GetString("a8f928cd-cdd7-4f9b-90ba-aefe9e36ebeb",
							"Another record already exists in database with the same primary key. Please cancel current edit and enter data again if you cannot find that record.") +
						"\r\n" + result;
				}
			}

			if (string.IsNullOrEmpty(result))
			{
				result = "Another record already exists with the same key details as you have entered.";    // Exception Message
			}

			return result;
		}

		string GetFormattedDuplicateKeyErrorMessage(string indexTableName, StringCollection indexColumns)
		{
			Argument.NotNullOrEmpty(indexTableName, nameof(indexTableName));
			Argument.NotNull(indexColumns, nameof(indexColumns)); // Suggested By ReviewBot

			string fieldDescription = "";

			if (indexColumns.Count == 1)
			{
				var column = indexColumns[0];

				fieldDescription = GetColumnDescriptiveName(indexTableName, column);
			}
			else if (indexColumns.Count > 1)
			{
				StringBuilder fieldNames = new StringBuilder();
				foreach (string columnName in indexColumns)
				{
					fieldNames.Append(GetColumnDescriptiveName(indexTableName, columnName) + " + ");
				}

				fieldDescription = fieldNames.ToString(0, fieldNames.Length >= 3 ? fieldNames.Length - 3 : fieldNames.Length);
			}

			var duplicateValues = sqlServerExceptionErrors.Select(error =>
			{
				int locationleftbrace = error.Message.IndexOf('('); // Parsing Exception Message using brackets which do not change with languages

				if (locationleftbrace != -1)
				{
					int locationrightbrace = error.Message.LastIndexOf(')'); // Parsing Exception Message using brackets which do not change with languages
					return error.Message.Substring(locationleftbrace + 1, locationrightbrace - locationleftbrace - 1).Trim();
				}
				return null;
			}).Where(s => !string.IsNullOrEmpty(s));

			var valueList = string.Join(", ", duplicateValues);
			return Res.GetString("57b7b11a-9a8c-4d19-9c91-4c45281515df", "The value of {0} must be unique on {1}. The duplicate value(s) are: ({2}).", fieldDescription, GetTableDescriptiveName(indexTableName), valueList);
		}

		#endregion

		#region Check Constraint Violation

		string GetInsertOrUpdateConflictedWithCheckConstraintErrorMessage(DbConnection connection)
		{
			var tableName = MetaData.GetTableNameFromErrorMessage(sqlServerExceptionMessage);
			if (string.IsNullOrEmpty(tableName))
			{
				var tableNameForParsingError = $"Cannot get table name, not valid error message.{System.Environment.NewLine}{sqlServerExceptionMessage}";
				ErrorReporter.ReportDeveloperExceptionOnce("GetInsertOrUpdateConflictedWithCheckConstraintErrorMessage", tableNameForParsingError, new InvalidOperationException(tableNameForParsingError));
				return tableNameForParsingError;
			}

			var constraintName = MetaData.GetCheckConstraintNameFromErrorMessage(sqlServerExceptionMessage);
			var constraintDefinition = GetConstraintDefinition(connection, tableName, constraintName);

			var columnName = MetaData.GetColumnNameFromErrorMessage(sqlServerExceptionMessage);
			var columnError = !columnName.IsNullOrEmpty() ? string.Format(CultureInfo.InvariantCulture, @"on column '{0}' ", columnName) : string.Empty; // Exception Message

			var error = string.Format(CultureInfo.InvariantCulture, @"Cannot import to table '{0}' because constraint '{1}' failed {2}- {3}.", // Exception Message
										tableName, constraintName, columnError, constraintDefinition);

			var pattern = string.Format(CultureInfo.InvariantCulture, @"({0}_)(?!{0}_)", GlobalServiceProvider.Instance.GetRequiredService<IApplicationSchemaResolver>().GetColumnNamePrefix(tableName));
			return new Regex(pattern).Replace(error, string.Empty);
		}

		#endregion

		#region FK Violation

		protected string GetInsertUpdateOrDeleteConflictedWithFKErrorMessage(bool isDelete, DbConnection connection)
		{
			string result = "";

			if (connection != null)
			{
				string fkName = MetaData.GetFKNameFromErrorMessage(sqlServerExceptionMessage);
				string[] tableNames = MetaData.GetParentAndChildTablesFromFK(connection, fkName);

				if (tableNames == null || tableNames.Length == 0)
				{
					return sqlServerExceptionMessage;
				}

				if (string.IsNullOrEmpty(tableNames[0]) || string.IsNullOrEmpty(tableNames[1]))
				{
					throw new InvalidOperationException();
				}

				if (isDelete)
				{
					result = GetFormattedDeleteConflictedWithFKErrorMessage(tableNames[0], tableNames[1]);
				}
				else
				{
					result = GetFormattedInsertOrUpdateConflictedWithFKErrorMessage(tableNames[0], tableNames[1]);
				}
			}

			return result;
		}

		protected string GetFormattedInsertOrUpdateConflictedWithFKErrorMessage(string parentTableName, string childTableName)
		{
			Argument.NotNullOrEmpty(childTableName, nameof(childTableName));
			Argument.NotNullOrEmpty(parentTableName, nameof(parentTableName));

			var parentDescriptiveName = GetTableDescriptiveName(parentTableName);
			var childDescriptiveName = GetTableDescriptiveName(childTableName);

			StringBuilder error = new StringBuilder();
			error.Append("The " + childDescriptiveName); // Exception Message
			if (!childTableName.Equals(childDescriptiveName))
			{
				error.Append(" (" + childTableName + ")");
			}
			error.Append(" cannot be inserted/updated, requires a reference to a valid " + parentDescriptiveName); // Exception Message
			if (!parentTableName.Equals(parentDescriptiveName))
			{
				error.Append(" (" + parentTableName + ")");
			}
			error.Append(".");
			return error.ToString();
		}

		protected string GetFormattedDeleteConflictedWithFKErrorMessage(string parentTableName, string childTableName)
		{
			Argument.NotNullOrEmpty(childTableName, nameof(childTableName));
			Argument.NotNullOrEmpty(parentTableName, nameof(parentTableName));

			return Res.GetString("d520c605-96fb-4f35-8942-2aa51eb32345", "The {0} cannot be deleted, because there is at least one {1} referencing it.", GetTableDescriptiveName(parentTableName), GetTableDescriptiveName(childTableName));
		}

		#endregion

		#region General Network Error

		protected string GetGeneralNetworkErrorMessage()
		{
			return "You may be experiencing network problems. Please close down the application and try again.\n If the problem persists then please contact your network administrator.";  // Exception Messag
		}

		#endregion

		#region Database log full

		string DbFileIsFullErrorMessage(DbConnection connection)
		{
			var message = new StringBuilder("Database is unable to grow. Possible cause(s):");  // Exception Message

			bool isLogFileError = (ExceptionType == DbErrorType.LogIsFull);

			string diskSpaceReason = string.Format(" - There is no space in the server {0} disk", (isLogFileError) ? "log" : "data");   // Exception Message
			message.AppendLine(diskSpaceReason);

			if (connection == null || !IsDbFileUnlimitedAutogrowth(isLogFileError, connection))
			{
				string autogrowthReason = string.Format(" - {0} file autogrowth is disabled or incorrectly set", (isLogFileError) ? "Log" : "Data");    // Exception Message
				message.AppendLine(autogrowthReason);
			}

			if (!isLogFileError && (connection == null || HasReachedSqlExpressLimit(connection)))
			{
				string expressLimitReason = " - It has reached the maximum data size (Express Edition only)";   // Exception Message
				message.AppendLine(expressLimitReason);
			}

			message.AppendLine();
			message.Append("Please contact your system administrator.");    // Exception Message

			return message.ToString();
		}

		bool IsDbFileUnlimitedAutogrowth(bool isCheckingLogFile, DbConnection connection)
		{
			bool result = false;

			if (connection != null)
			{
				string sqlText = string.Format(@"
					SELECT count(*)
					FROM sys.database_files
					WHERE [type] = {0}
					AND (growth = 0 OR max_size > 0)",
					(isCheckingLogFile) ? "1" : "0");

				int count = Convert.ToInt32(connection.ExecuteScalar(sqlText));

				result = (count == 0);
			}

			return result;
		}

		string GetConstraintDefinition(DbConnection connection, string tableName, string constraintName)
		{
			var dbName = string.Empty;
			var tableNamePredicate = $"st.name = '{tableName}'";
			if (tableName.StartsWith("#"))
			{
				dbName = "tempdb.";
				tableNamePredicate = $"CHARINDEX('{tableName}', st.name) = 1";
			}

			if (connection != null)
			{
				string sqlQuery = string.Format(CultureInfo.InvariantCulture, @"
					SELECT chk.definition
					FROM {0}sys.check_constraints chk
					INNER JOIN {0}sys.tables st on chk.parent_object_id = st.object_id
					WHERE {1}
					AND chk.name = '{2}'",
					dbName, tableNamePredicate, constraintName);

				using (var cmd = connection.Command(sqlQuery))
				{
					var result = cmd.ExecuteScalar();
					return result != null ? result.ToString() : string.Empty;
				}
			}
			return string.Empty;
		}

		bool HasReachedSqlExpressLimit(DbConnection connection)
		{
			Argument.NotNull(connection, nameof(connection)); // Suggested By ReviewBot

			bool result = false;

			if (connection.ServerEdition == DbConnection.SqlServerEdition.Express)
			{
				string sqlText = string.Format(
					"SELECT sum(size)/128 AS DbSizeMb FROM [{0}].sys.database_files WHERE [type] = 0",
					connection.CurrentDatabase);

				int dbSizeMb = Convert.ToInt32(connection.ExecuteScalar(sqlText));
				result = (dbSizeMb >= 10000);
			}

			return result;
		}

		#endregion

		#region Deadlock and Timeout

		protected string DeadlockErrorMessage()
		{
			return "Server cancelled the operation due to deadlock with another operation. Please try again.";  // Exception Messag
		}

		protected string TimeoutExpiredErrorMessage()
		{
			return "Server is taking too long to respond. Please try again.";   // Exception Messag
		}

		protected string LockTimeoutExpiredErrorMessage()
		{
			return "Lock request time out period exceeded. Please try again.";  // Exception Messag
		}

		#endregion

		#region LoginFailedForUser

		protected string ServerIsInScriptModeFriendlyMessage(ICurrentDbControl connection)
		{
			if (connection == null)
			{
				throw new ArgumentNullException(nameof(connection));
			}

			return string.Format(CultureInfo.InvariantCulture,
				"{0}\r\n{1}\r\n\r\nMessage: {2}\r\nServer Name: {3}\r\nDatabase: {4}",  // Exception Message
				"Your SQL Server is being updated. Please try again in a few minutes.", // Exception Message
				"If the problem persists then please contact your system administrator.",   // Exception Message
				sqlServerExceptionMessage,
				connection.ServerName,
				connection.InitialDatabase);
		}

		protected string GetLoginFailedForUser(ICurrentDbControl connection)
		{
			return string.Format(CultureInfo.InvariantCulture,
				"{0}\r\n{1}\r\n\r\nMessage: {2}\r\nServer Name: {3}\r\nDatabase: {4}",// Exception Message
				"Database login failed - please check the server error log.", // Exception Message
				"If the problem persists then please contact your system administrator.",// Exception Message
				sqlServerExceptionMessage,
				connection != null ? connection.ServerName : string.Empty,
				connection != null ? connection.InitialDatabase : string.Empty);
		}

		protected string GetLoginIsNotAbleToAccessDatabaseUnderCurrentSecurityContextErrorMessage(ICurrentDbControl connection)
		{
			return string.Format(CultureInfo.InvariantCulture,
			"{0}\r\n{1}\r\n\r\nMessage: {2}\r\nServer Name: {3}\r\nDatabase: {4}",// Exception Message
			"The server is not able to access the database under the current security context - please check the server error log.", // Exception Message
			"If the problem persists then please contact your system administrator.",// Exception Message
			sqlServerExceptionMessage,
			connection != null ? connection.ServerName : string.Empty,
			connection != null ? connection.InitialDatabase : string.Empty);
		}

		#endregion

		public static string GetColumnDescriptiveName(string tableName, string columnName)
		{
			Argument.NotNull(tableName, nameof(tableName));
			Argument.NotNullOrEmpty(columnName, nameof(columnName));

			var resStrings = GlobalServiceProvider.Instance.GetRequiredService<IDataBoundResourceStrings>();
			return resStrings.GetStringForProperty(tableName, columnName);
		}

		public static string GetTableDescriptiveName(string tableName)
		{
			Argument.NotNullOrEmpty(tableName, nameof(tableName));

			var resStrings = GlobalServiceProvider.Instance.GetRequiredService<IDataBoundResourceStrings>();
			return resStrings.GetStringForTable(tableName);
		}

		string GetInvalidObjectNameFriendlyMessage()
		{
			var objectNameMatches = Regex.Matches(sqlServerExceptionMessage, "'([^']*)'");
			foreach (Match match in objectNameMatches)
			{
				var objectName = match.Value;

				if (objectName.StartsWith("'RefDatabase") || objectName.StartsWith("'RefDb"))
				{
					return InvalidSynonymFriendlyMessage;
				}
			}

			return string.Empty;
		}

		#endregion
	}
}
