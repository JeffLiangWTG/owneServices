using System;
using System.Data;
using System.Globalization;
using CargoWise.Common;
using CargoWise.DataProtection;

namespace CargoWise.Data
{
	/// <summary>
	/// Logic for putting the main CW database in a locked-out state, where only the locking connection should be doing anything.
	/// Used for upgrades and purges.
	/// A lockout consists of:
	/// - setting a db extended property
	/// Older lockouts that were taken outside the upgrade transaction also included:
	/// - recording the SPID and login time of the connection doing the lockout, to detect if it dies.
	/// Even older, original lockout included:
	/// - Disabling all staff and application logins, except for the AdminConnection login (OdysseyAdmin).
	///	  This is being phased out due to performance problems.
	/// </summary>
	public static class DbLockout
	{
		/// <summary>
		/// Name of the db extended property to contain the lockout reason.
		/// </summary>
		internal const string DbIsLockedOutForProperty = "DbIsLockedOutFor";

		public const int LockRequestTimeoutPeriodExceededSqlError = 1222;

		/// <summary>
		/// Aquire a lockout inside a transaction, by setting a db extended property.
		/// The SPID and connection time are not recorded since detecting a dead connection is not needed.
		/// The lockout db extended property has to be queried with the NOLOCK hint.
		///
		/// Make sure to call ResetTransactionLockout before the transaction is committed.
		/// 
		/// If the transaction is rolled back then the lockout is removed. No chance of an abandoned lockout.
		/// </summary>
		/// <exception cref="InvalidOperationException">thrown if the connection is not in a transaction</exception>
		/// <returns>true if lockout obtained, false if another connection already had the lockout</returns>
		public static bool AcquireTransactionLockout(AdminConnection connection, LockoutReason lockoutReason = LockoutReason.Upgrade)
		{
			if (!connection.IsInTransaction)
			{
				throw new InvalidOperationException("connection is not in a transaction");
			}

			try
			{
				using (connection.TemporarySetLockTimeout(0))
				{
					DataUtils.AddDbExtendedProperty(connection, DbIsLockedOutForProperty, lockoutReason.ToString());
					return true;
				}
			}
			catch (SqlException sqlEx) when (sqlEx.Number == LockRequestTimeoutPeriodExceededSqlError)
			{
				return false;
			}
		}

		/// <summary>
		/// Reset the lockout from AquireTransactionLockout, by dropping the property.
		/// Should be called just before the transaction is committed.
		/// Does nothing and silently returns if the property does not exist.
		/// </summary>
		/// <exception cref="InvalidOperationException">thrown if the connection is not in a transaction</exception>
		public static void ResetTransactionLockout(AdminConnection connection)
		{
			if (!connection.IsInTransaction)
			{
				throw new InvalidOperationException("connection is not in a transaction");
			}

			DataUtils.DropDbExtendedProperty(connection, DbIsLockedOutForProperty);
		}

		/// <summary>
		/// Acquire legacy lockout.
		/// Sets lockout reason extended property and lockout SPID registry items.
		/// Optionally disables logins for upgrading from older software.
		/// </summary>
		/// <returns>ValidLockout if lockout registry already matched current SPID, or AquiredLockout otherwise</returns>
		public static DbLockoutState AcquireLockout(AdminConnection connection, LockoutReason lockoutReason = LockoutReason.Upgrade, bool disableLogins = false)
		{
			if (lockoutReason == LockoutReason.None)
			{
				throw new ArgumentException("A reason for the lock Db must be specified."); // Developer error message
			}

			DataUtils.AddDbExtendedProperty(connection, DbIsLockedOutForProperty, lockoutReason.ToString());

			DbLockoutState state = DbLockoutState.ValidLockout;
			connection.RunTransactioned(delegate
			{
				state = CheckLockoutState(connection);
				if (state != DbLockoutState.ValidLockout)
				{
					if (disableLogins)
					{
						((IDbLockout)connection).DisableApplicationDbLogins();
					}
					SetLockoutRegistryItems(connection);
					state = DbLockoutState.AquiredLockout;
			}
			});
			if (disableLogins && state == DbLockoutState.AquiredLockout)
			{
				DisableStaffAssignedDbLogin(connection);
			}
			return state;
		}

		/// <summary>
		/// Reset the lockout taken by AcquireLockout if the given connection is the lockout owner,
		/// or if the lockout SPID is no longer connected.
		/// - Drops the db extended property and clears the lockout SPID registries.
		/// - Enables logins if they are disabled.
		/// </summary>
		/// <param name="connection"></param>
		/// <returns>ResetLockout if the lockout was reset,
		/// ValidLockout if another live connection has the lockout,
		/// NoLockout if there was no lockout.</returns>
		public static DbLockoutState ResetLockout(AdminConnection connection)
		{
			DataUtils.DropDbExtendedProperty(connection, DbIsLockedOutForProperty);

			var state = DbLockoutState.ValidLockout;
			try
			{
				connection.RunTransactioned(delegate
				{
					state = CheckLockoutState(connection);
					if (state == DbLockoutState.ValidLockout && IsLockoutOwner(connection))
					{
						state = DbLockoutState.InvalidLockout;
					}
					if (state == DbLockoutState.InvalidLockout)
					{
						((IDbLoginRepair)connection).EnableApplicationDbLogins();
						ClearLockoutRegistryItems(connection);
						state = DbLockoutState.ResetLockout;
					}
				});
				if (state == DbLockoutState.ResetLockout)
				{
					EnableStaffAssignedDbLogin(connection);
				}
			}
			catch (Exception ex) when (IsRegistryAccessFromAnotherDbException(connection, ex))
			{
				var currentDb = (ICurrentDbControl)connection;
				using (currentDb.UseDatabase(currentDb.InitialDatabase))
				{
					ResetLockout(connection);
				}
			}
			return state;
		}

		static bool IsRegistryAccessFromAnotherDbException(AdminConnection connection, Exception ex)
		{
			return
				!ex.IsCriticalException()
				&& (ex is SqlException || ex is RegistryAccessFromSystemDatabaseException)
				&& connection.CurrentDatabase != InitialDatabase(connection);
		}

		static string InitialDatabase(DbConnection connection)
			=> ((ICurrentDbControl)connection).InitialDatabase;

		/// <summary>
		/// Check for a lockout owned by any connection to the database.
		/// Lockouts are obtained by calling the AcquireLockout method.
		/// Returns:
		/// - ValidLockout if:
		///		- the lockout SPID setting in the db matches an active connection
		///	    - or the settings are not readable, because some other connection has those records locked.
		/// - InvalidLockout if
		///		- there are no lockout SPID settings, but the main user login is disabled
		///		- or there are lockout SPID settings, but there's no matching connection (e.g., upgrade process had died).
		///	- NoLockout if there are no lockout SPID settings and the main user login is enabled.
		/// Will not return the other states: AquiredLockout, ResetLockout
		///
		/// Requires an AdminConnection <see cref="LockoutConnectionIsValid" />
		/// </summary>
		public static DbLockoutState CheckLockoutState(AdminConnection connection)
		{
			DbLockoutState state = DbLockoutState.ValidLockout;
			using (connection.TemporarySetLockTimeout(3000))
			{
				try
				{
					if (DbRegistry.LockoutSpid.LoadValue(connection) < 0)
					{
						state = IsMainUserLoginDisabledOrDoesNotExist(connection) ? DbLockoutState.InvalidLockout : DbLockoutState.NoLockout;
					}
					else
					{
						state = LockoutConnectionIsValid(connection) ? DbLockoutState.ValidLockout : DbLockoutState.InvalidLockout;
					}
				}
				catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.LockTimeoutExpired)
				{
					state = DbLockoutState.ValidLockout;
				}
			}

			return state;
		}

		/// <summary>
		/// Get the lockout reason if there is valid active lockout.
		/// Internally uses a new admin connection.
		/// </summary>
		public static LockoutReason GetLockoutReason()
		{
			try
			{
				using (var adminConnection = Db.NewAdminConnection())
				{
					adminConnection.IsUpgradeCheckDisabled = true;
					var lockout = TryLoadLockoutReasonFromMainDb(adminConnection);
					if (!lockout.ok)
					{
						return LockoutReason.None;
					}

					if (lockout.isLocked || lockout.reason == LockoutReason.None)
					{
						return lockout.reason;
					}

					// Old style lockout - the property was set outside a transaction.
					// Verify the locking connection still exists.
					var dbLockoutState = CheckLockoutState(adminConnection);
					if (dbLockoutState != DbLockoutState.NoLockout)
					{
						return lockout.reason;
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
			}

			return LockoutReason.None;
		}

		/// <summary>
		/// Load the lockout db extended property from the Db.DatabaseName database and determine if it is locked in a transaction.
		/// Uses NOLOCK hint to read uncommitted records.
		/// Newer lockouts will be in a transaction, older ones are not.
		/// </summary>
		/// <returns>"ok" true if no error, false otherwise</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:DoNotUseDbConnectionMethods", Justification = "Necessary")]
		internal static (bool ok, LockoutReason reason, bool isLocked) TryLoadLockoutReasonFromMainDb(DbConnection connection)
		{
			var sql =
$@"SET NOCOUNT ON;
SET LOCK_TIMEOUT 0;

DECLARE @reason sql_variant = (SELECT value FROM {Db.DatabaseName.QuoteName()}.sys.extended_properties WITH(NOLOCK) WHERE class = 0 AND name = @PropertyName);
DECLARE @isLocked bit = 0;

IF @reason is not null
BEGIN
	-- determine if the property has a lock
	BEGIN TRY
		SET @reason = (SELECT value FROM {Db.DatabaseName.QuoteName()}.sys.extended_properties WHERE class = 0 AND name = @PropertyName);
	END TRY
	BEGIN CATCH
		IF (ERROR_NUMBER() != 1222)
			THROW;
		SET @isLocked = 1;
	END CATCH;	
END

SELECT Reason = @reason, IsLocked = @isLocked;";

			try
			{
				using (var cmd = connection.Command(sql))
				{
					cmd.AddParameter("@PropertyName", System.Data.SqlDbType.NVarChar, 128, DbIsLockedOutForProperty);
					using (var reader = cmd.ExecuteReader())
					{
						reader.Read();
						var reasonObject = reader.GetValue(0);
						var isLocked = reader.GetBoolean(1);
						var reasonText = (reasonObject == null || reasonObject == DBNull.Value) ? null : reasonObject.ToString();
						var reason = LockoutReason.None;
						var ok = reasonText == null || Enum.TryParse(reasonText, out reason);
						return (ok, reason, isLocked);
					}
				}
			}
			catch (SqlException ex) when (!ex.IsCriticalException())
			{
				return (false, LockoutReason.None, false);
			}
		}

		static bool TryGetDbLockoutReasonByDbExtendedProperty(DbConnection connection, out LockoutReason dbIsLockedOutReason)
		{
			dbIsLockedOutReason = LockoutReason.None;

			try
			{
				var stringValue = DataUtils.LoadDbExtendedPropertyOnMainDb(connection, DbIsLockedOutForProperty, withNoLock: true);

				if (stringValue != null && Enum.TryParse<LockoutReason>(stringValue, out var result))
				{
					dbIsLockedOutReason = result;
					return true;
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
			}

			return false;
		}

		/// <summary>
		/// Check if the main user login is disabled.
		/// </summary>
		static bool IsMainUserLoginDisabledOrDoesNotExist(AdminConnection adminConnection)
		{
			var sqlText = @"
IF exists(
	SELECT null FROM sys.server_principals l INNER JOIN sys.server_permissions p ON l.principal_id = p.grantee_principal_id
	WHERE l.name = @name AND l.is_disabled = 0 AND p.type = 'COSQ') SELECT 0
ELSE SELECT 1;
";
			var result = adminConnection.ExecuteScalar<int>(sqlText,
				cmd => cmd.AddParameter("@name", SqlDbType.NVarChar, 128, RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName))
			);
			return result == 1;
		}

		/// <summary>
		/// Checks if an exception is caused by a disabled login.
		/// Software that doesn't require logins to be disabled and is not actually doing the database upgrade from such software should not get this error.
		/// </summary>
		internal static bool IsUpgradeLockoutError(System.Data.Common.DbException sqlException)
		{
			return
				DbErrorMatch.IsDbLoginError(new DbErrorMatch(sqlException).ExceptionType)
				&& GetLockoutReason() != LockoutReason.None;
		}

		internal static void SetLockoutRegistryItems(DbConnection connection)
		{
			DbRegistry.LockoutSpid.SaveValue(connection.SPID, connection);
			DbRegistry.LockoutLoginTime.SaveValue(connection.LoginTime, connection);
		}

		internal static void ClearLockoutRegistryItems(DbConnection connection)
		{
			DbRegistry.LockoutSpid.SaveValue(-1, connection);
			DbRegistry.LockoutLoginTime.SaveValue(DateTime.MinValue, connection);
		}

		internal static bool IsLockoutOwner(DbConnection connection)
		{
			return DbRegistry.LockoutSpid.LoadValue(connection) == connection.SPID
				&& DbRegistry.LockoutLoginTime.LoadValue(connection) == connection.LoginTime;
		}

		/// <summary>
		/// Checks if the lockout SPID settings in the DB are valid, i.e., there is an active connection that matches and so has the lockout.
		/// Requires an AdminConnection since the Writer login cannot see other connections.
		/// For an upgrade lockout, if the connection is not the given connection then we expect the settings are unreadable due to SCH-M locks owned by the other connection.
		/// In that case this method will eventually throw a Lock Timeout, or a SQL Timeout exception, whichever times-out first.
		/// Note, there is a tiny window during an upgrade between saving the settings and obtaining the SCH-M locks when they would be readable.
		/// For a purge lockout, SCH-M locks are not taken and settings should always be readable.
		/// </summary>
		internal static bool LockoutConnectionIsValid(AdminConnection connection)
		{
			var sql = @"
SELECT CONVERT(bit, CASE WHEN EXISTS(
SELECT
	NULL
FROM
	sys.dm_exec_sessions AS sp
	JOIN dbo.StmData     AS SpidData  ON sp.session_id = CONVERT(int, CONVERT(nvarchar(max), SpidData.SD_BinaryValue)) AND SpidData.SD_Name = 'LockoutSpid'
	JOIN dbo.StmData     AS LoginData ON sp.login_time BETWEEN dateadd(ms, -5, convert(datetime2, convert(nvarchar, LoginData.SD_BinaryValue))) AND dateadd(ms, 5, convert(datetime2, convert(nvarchar, LoginData.SD_BinaryValue))) AND LoginData.SD_Name = 'LockoutLoginTime'
) THEN 1 ELSE 0 END)
";

			return Convert.ToBoolean(connection.ExecuteScalar(sql), CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Check if a lockout (Upgrade or Purge) has been acquired using only the given connection.
		/// Only reads the lockout db extended property and doesn't verify if the lockout is valid, i.e. has an active connection.
		/// </summary>
		public static bool HasLockout(DbConnection connection)
		{
			return TryGetDbLockoutReasonByDbExtendedProperty(connection, out var reason)
				&& reason != LockoutReason.None;
		}

		#region Enable/Disable Staff DB Login - only done for legacy reasons

		static void DisableStaffAssignedDbLogin(AdminConnection connection)
		{
			var dbName = InitialDatabase(connection);
			var prefix = DataUtils.ReplaceSqlLikeWildcard(DbUserRepository.GetStaffDbLoginFullPrefix(dbName));

			var sql = string.Format(CultureInfo.InvariantCulture, @"-- DisableStaffAssignedDbLogin
DECLARE
	@stmt nvarchar(max);

SELECT
	@stmt = ISNULL(@stmt, N'')
		+ N'ALTER LOGIN ' + QUOTENAME(name) + N' DISABLE;'
FROM
	sys.server_principals
WHERE
	is_disabled = 0
	AND name LIKE N'{0}%';

if (@stmt is NOT NULL) EXEC sys.sp_executesql @stmt;"
				, prefix
				);
			connection.ExecuteNonQuery(sql);
		}

		static void EnableStaffAssignedDbLogin(AdminConnection connection)
		{
			var dbName = InitialDatabase(connection);
			var sql = string.Format(CultureInfo.InvariantCulture,
				"SELECT name FROM sys.server_principals WHERE is_disabled = 1 AND name LIKE N'{0}%';"
				, DataUtils.ReplaceSqlLikeWildcard(DbUserRepository.GetStaffDbLoginFullPrefix(dbName))
				);
			var clientDbLoginTable = DataUtils.GetDataTableFromQuery(connection, sql);
			var tryCatchWrapper = new DbTryCatchWrapper();

			foreach (DataRow clientDbLoginRow in clientDbLoginTable.Rows)
			{
				var login = clientDbLoginRow[0];

				string loginName = login.ToString();
				connection.ExecuteNonQuery(tryCatchWrapper.GetEnableDbLoginCommandSafe(loginName));
			}
		}

		#endregion
	}
}
