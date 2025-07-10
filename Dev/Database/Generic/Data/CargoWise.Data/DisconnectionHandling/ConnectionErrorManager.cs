using System;
using System.Data;
using CargoWise.Common;

namespace CargoWise.Data
{
	public class ConnectionErrorManager
	{
		internal ConnectionErrorManager(IDbReconnectionHandling connectionToHandle)
		{
			Argument.NotNull(connectionToHandle, nameof(connectionToHandle));
			Argument.NotNullOrEmpty(connectionToHandle.ServerName, nameof(connectionToHandle.ServerName));

			this.connectionToHandle = connectionToHandle;
		}

		internal virtual bool HandleDisconnectionAndSecurityErrors(Exception ex)
		{
			Argument.NotNull(ex, nameof(ex));

			bool result =
				ReconnectionOccured(ex)
				|| ReconnectIfApplicable(ex)
				|| HandleSecurityErrors(ex)
			;

			return result && (connectionToHandle.AppTransactionCount == 0);
		}

		#region Reconnect

		bool ReconnectionOccured(Exception ex)
		{
			Argument.NotNull(ex, nameof(ex));

			var sqlException = ex as SqlException;

			if (sqlException != null)
			{
				var errorHandler = new DbErrorMatch(sqlException);
				return (errorHandler.ExceptionType == DbErrorType.ConnectionRecovered);
			}

			return false;
		}

		internal bool ReconnectIfApplicable(Exception ex)
		{
			Argument.NotNull(ex, nameof(ex));

			return ReconnectIfApplicableCore(ex);
		}

		/// <summary>
		/// Made virtual for testing purposes.
		/// </summary>
		protected virtual bool ReconnectIfApplicableCore(Exception e)
		{
			Argument.NotNull(e, nameof(e));

			bool shouldHandleDisconnectionError =
				(!connectionToHandle.IsConnecting)
				&& (connectionToHandle.AppTransactionCount == 0)
				&& (IsInvalidOperationError(e) || IsGeneralNetworkError(e));

			if (shouldHandleDisconnectionError)
			{
				return TryCloseAndReopenConnection();
			}

			return false;
		}

		bool TryCloseAndReopenConnection()
		{
			var result = true;

			try
			{
				connectionToHandle.CloseAndReopenConnection();
			}
			catch (CloseReopenWhileConnectingException)
			{
				result = false;
			}
			return result;
		}

		bool IsInvalidOperationError(Exception e)
		{
			Argument.NotNull(e, nameof(e));

			return (
				(e is InvalidOperationException) &&
				(connectionToHandle.State != ConnectionState.Open)
				);
		}

		bool IsGeneralNetworkError(Exception e)
		{
			bool result = false;

			var sqlException = e as SqlException;
			if (sqlException != null)
			{
				var errorType = new DbErrorMatch(sqlException).ExceptionType;

				if (DbErrorMatch.IsNetworkError(errorType))
				{
					result = true;
				}
			}

			return result;
		}

		#endregion

		#region Security Errors

		bool HandleSecurityErrors(Exception ex)
		{
			Argument.NotNull(ex, nameof(ex));

			var sqlException = ex as SqlException;

			if (sqlException != null)
			{
				var errorMatch = new DbErrorMatch(sqlException);

				return (
					HandlePermissionDeniedToDatabaseOrObjectError(errorMatch)
					|| HandleCannotExecuteAsDatabasePrincipalError(errorMatch)
				);
			}

			return false;
		}

		bool HandleCannotExecuteAsDatabasePrincipalError(DbErrorMatch errorMatch)
		{
			Argument.NotNull(errorMatch, nameof(errorMatch));

			if (errorMatch.ExceptionType == DbErrorType.CannotExecuteAsDatabasePrincipal)
			{
				string loginName = errorMatch.GetPrincipalFromError();

				if (!String.IsNullOrWhiteSpace(loginName))
				{
					return RepairPermissionOnPrimaryServer(
						(lr) => lr.EnsureLoginCorrectlyMappedToAllDatabases(loginName));
				}
			}

			return false;
		}

		bool HandlePermissionDeniedToDatabaseOrObjectError(DbErrorMatch errorMatch)
		{
			Argument.NotNull(errorMatch, nameof(errorMatch));

			if (
				errorMatch.ExceptionType == DbErrorType.LoginIsNotAbleToAccessDatabaseUnderCurrentSecurityContext
				|| errorMatch.ExceptionType == DbErrorType.PermissionDeniedOnObject
				|| errorMatch.ExceptionType == DbErrorType.PermissionDeniedInDatabase)
			{
				var errorDb = errorMatch.GetDatabaseFromSecurityError();
				var loginToRepair = errorMatch.GetPrincipalFromError();
				var permissionToRepair = errorMatch.GetPermissionFromError();
				var schemaToRepair = errorMatch.GetSchemaFromError();

				loginToRepair = loginToRepair ?? connectionToHandle.ImpersonatedLogin ?? connectionToHandle.LoginName;

				if (!ShouldAttemptRepair(loginToRepair, permissionToRepair, schemaToRepair))
				{
					return true;
				}

				if (String.IsNullOrWhiteSpace(errorDb))
				{
					return RepairPermissionOnPrimaryServer(
						(lr) => lr.EnsureLoginCorrectlyMappedToAllDatabases(loginToRepair));
				}
				else
				{
					return RepairPermissionOnPrimaryServer(
						(lr) => lr.HandlePermissionDeniedError(loginToRepair, errorDb),
						errorDb);
				}
			}

			return false;
		}

		bool ShouldAttemptRepair(string loginToRepair, string permissionToRepair, string schemaToRepair)
		{
			if (string.IsNullOrWhiteSpace(loginToRepair))
			{
				return true;
			}

			if (!DatabaseLogin.IsApplicationLogin(loginToRepair))
			{
				return false;
			}

			if (string.IsNullOrWhiteSpace(permissionToRepair) || string.IsNullOrWhiteSpace(schemaToRepair))
			{
				return true;
			}

			using (var adminConnection = Db.NewAdminConnection(connectionToHandle.ServerName, connectionToHandle.InitialDatabase))
			{
				return DatabaseLogin.IsSchemaPermissionExpected(adminConnection, connectionToHandle.ServerName, connectionToHandle.InitialDatabase, loginToRepair, permissionToRepair, schemaToRepair);
			}
		}

		bool RepairPermissionOnPrimaryServer(Func<IDbLoginRepair, bool> repairFunction, string errorDb = null)
		{
			Argument.NotNull(repairFunction, nameof(repairFunction));

			using (var connection = Db.NewAdminConnection(connectionToHandle.ServerName, connectionToHandle.InitialDatabase))
			{
				if (connection.IsSecondaryReplicaDatabase(errorDb ?? connectionToHandle.InitialDatabase))
				{
					using (var primaryDbConnection = Db.NewAdminConnection(connectionToHandle.InitialDatabase))
					{
						return RepairPermissionHandlingLockTimeoutError(primaryDbConnection, repairFunction);
					}
				}
				else
				{
					return RepairPermissionHandlingLockTimeoutError(connection, repairFunction);
				}
			}
		}

		bool RepairPermissionHandlingLockTimeoutError(AdminConnection repairConnection, Func<IDbLoginRepair, bool> repairFunction)
		{
			Argument.NotNull(repairConnection, nameof(repairConnection));
			Argument.NotNull(repairFunction, nameof(repairFunction));

#if DEBUG
			repairConnection.ExecuteNonQuery("SET LOCK_TIMEOUT 0");
#endif

			try
			{
				bool permissionFixed = repairFunction(repairConnection);
				return permissionFixed;
			}
			catch (SqlException ex)
			{
				if (new DbErrorMatch(ex).ExceptionType != DbErrorType.LockTimeoutExpired)
				{
					throw;
				}
			}

			return false;
		}

		#endregion

		protected readonly IDbReconnectionHandling connectionToHandle;
	}
}
