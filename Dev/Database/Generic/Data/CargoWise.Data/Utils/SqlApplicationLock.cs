using System;
using System.Data;
using System.Globalization;
using CargoWise.Common;

namespace CargoWise.Data.Utils
{
	#region SuppressResourceStringsCheckRegion

	public class SqlApplicationLock : Disposable, ISqlApplicationLock
	{
		#region Factory Method

		internal static SqlApplicationLock Acquire(DbConnection connection, string dbName, string key, TimeSpan sqlLockTimeout, SqlApplicationLockMode lockMode = DefaultLockMode)
		{
			if (!(!string.IsNullOrWhiteSpace(key) && key.Length <= 255))
			{
				throw new ArgumentException("key must be non null and not empty and 255 characters or less", nameof(key));
			}

			Argument.NotNullOrEmpty(dbName, nameof(dbName));
			Argument.NotNull(connection, nameof(connection));

			int appLockResult;

			var cmdText = string.Format(CultureInfo.InvariantCulture, "[{0}]..sp_getapplock", dbName);

			using (var cmd = connection.Command(cmdText, cmdTimeoutInSeconds: (int)sqlLockTimeout.TotalSeconds + 60))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@Resource", SqlDbType.NVarChar, 255, key);
				cmd.AddParameter("@LockMode", SqlDbType.VarChar, 32, lockMode.ToString());
				cmd.AddParameter("@LockOwner", SqlDbType.VarChar, 32, LockOwner);
				cmd.AddParameter("@LockTimeout", SqlDbType.Int, sqlLockTimeout.TotalMilliseconds);

				appLockResult = cmd.ExecuteProcedureWithReturnValue();
			}
			return (appLockResult >= 0)
				? new SqlApplicationLock(key, connection, dbName, lockMode: lockMode)
				: null;
		}

		#endregion

		internal SqlApplicationLock(string key, DbConnection connection, string lockDatabase, ISqlApplicationLockStrategy strategy = null, SqlApplicationLockMode lockMode = DefaultLockMode)
			: base()
		{
			if (string.IsNullOrWhiteSpace(lockDatabase))
			{
				throw new ArgumentException("lockDatabase cannot be null or blank", nameof(lockDatabase));
			}

			if (!(!string.IsNullOrWhiteSpace(key) && key.Length <= 255))
			{
				throw new ArgumentException("key must be non null and not empty and 255 characters or less", nameof(key));
			}

			Argument.NotNull(connection, nameof(connection));

			acquiredAt = DateTime.UtcNow;
			WasAcquiredOnLastCheck = true;

			this.connection = connection;
			this.lockDatabase = lockDatabase;
			this.key = key;
			this.strategy = strategy ?? new DefaultSqlApplicationLockStrategy();
			this.lockMode = lockMode;
		}

		public DateTime AcquiredAt { get { return acquiredAt; } }
		public string Key { get { return key; } }

		readonly DateTime acquiredAt;
		readonly string key;
		readonly string lockDatabase;
		readonly SqlApplicationLockMode lockMode;
		readonly DbConnection connection;
		readonly ISqlApplicationLockStrategy strategy;

		const SqlApplicationLockMode DefaultLockMode = SqlApplicationLockMode.Exclusive;
		internal const string LockOwner = "Session";

		#region Is Holding Lock

		internal bool WasAcquiredOnLastCheck { get; private set; }

		public bool IsHoldingLock()
		{
			if (WasAcquiredOnLastCheck)
			{
				return strategy.IsHoldingLock(connection, lockDatabase, key, LockOwner, lockMode);
			}

			return WasAcquiredOnLastCheck;
		}

		#endregion

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture,
				"SqlApplicationLock{{Key: '{0}', IsDisposed: {1}, WasAcquiredOnLastCheck: {2}}}",
				Key, IsDisposed, WasAcquiredOnLastCheck);
		}

		protected override void Dispose(bool isDisposing)
		{
			// WasAcquiredOnLastCheck is used here to avoid checking lock twice
			// If UndisposedLocksPresentAfterReconnect this means that reconnection has happened and we don't need to release the lock on the database side
			if (isDisposing && WasAcquiredOnLastCheck && !connection.DatabaseUpgradedExceptionHasBeenThrown && !connection.UndisposedLocksPresentAfterReconnect)
			{
				try
				{
					strategy.Cleanup(connection, lockDatabase, key, LockOwner);
				}
				catch (DatabaseUpgradeException)
				{
					throw;
				}
				// Ignore lost lock exception caused by a disconnection while releasing the lock
				catch (SqlLockLostException) { }
				// Ignore SQL errors when disposing as a shortcircuit to avoid double check
				catch (SqlException) { }
			}
		}
	}

	#endregion

	#region Strategies

	public interface ISqlApplicationLockStrategy
	{
		bool IsHoldingLock(DbConnection connection, string lockDatabase, string key, string lockOwner, SqlApplicationLockMode lockMode);

		void Cleanup(DbConnection connection, string lockDatabase, string key, string lockOwner);
	}

	public class DefaultSqlApplicationLockStrategy : ISqlApplicationLockStrategy
	{
		public void Cleanup(DbConnection connection, string lockDatabase, string key, string lockOwner)
		{
			if (connection.State != ConnectionState.Open)
			{
				return;
			}

			var cmdText = string.Format(CultureInfo.InvariantCulture, "[{0}]..sp_releaseapplock", lockDatabase); // test only

			using (var cmd = connection.Command(cmdText))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@Resource", SqlDbType.NVarChar, 255, key);
				cmd.AddParameter("@LockOwner", SqlDbType.VarChar, 32, lockOwner);

				cmd.ExecuteProcedureWithReturnValue();
			}
		}

		public bool IsHoldingLock(DbConnection connection, string lockDatabase, string key, string lockOwner, SqlApplicationLockMode lockMode)
		{
			try
			{
				if (connection.State == ConnectionState.Open)
				{
					var cmdText = string.Format(CultureInfo.InvariantCulture, @"
							EXEC [{0}]..sp_executesql
								N'SELECT APPLOCK_MODE(@role, @key, @owner)',
								N'@role nvarchar(128), @key nvarchar(255), @owner varchar(32)',
								@role = @DbPrincipal, @key = @LockKey, @owner = @LockOwner",  // test only
						lockDatabase);

					using (var command = connection.Command(cmdText))
					{
						command.AddParameter("@DbPrincipal", SqlDbType.NVarChar, 128, "public"); // test only
						command.AddParameter("@LockKey", SqlDbType.NVarChar, 255, key);
						command.AddParameter("@LockOwner", SqlDbType.VarChar, 32, lockOwner);
						var result = command.ExecuteScalar();
						var gotResult = Enum.TryParse<SqlApplicationLockMode>(result?.ToString() ?? "", true, out var resultEnum);
						return gotResult && (int)resultEnum >= (int)lockMode;
					}
				}
				else
				{
					return false;
				}
			}
			catch (SqlException)
			{
				// If checking lock throws an exception, it means the lock is dead
				return false;
			}
		}
	}

	#endregion
}
