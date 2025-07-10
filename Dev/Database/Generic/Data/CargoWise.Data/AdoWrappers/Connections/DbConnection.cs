#pragma warning disable CW1106 // Do Not Leave In Debug Messages
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.Data.HttpClient;
using CargoWise.Data.Providers.Common;
using CargoWise.Data.SqlServer;
using CargoWise.Data.Utils;
using CargoWise.Database.Abstractions;
using CargoWise.Database.Shared;
using CargoWise.DataProtection;
using CargoWise.Integration;
using Enterprise.DbUpgrader.Resource.Version;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using WTG.StaticAnalysis.Annotation;
using SqlBulkCopyOptions = CargoWise.Data.Providers.Common.SqlBulkCopyOptions;

namespace CargoWise.Data
{
	/// <summary>
	/// Base Abstract Connection Wrapper
	/// From which derive the concrete wrappers:
	///   - MainConnection  (singleton main database connection)
	///   - ExtraConnection (sporadic connections to other servers)
	/// </summary>
	public abstract partial class DbConnection : IDisposable, IDbConnectionInternals, IDbReconnectionHandling, IDbConnected,
		ICurrentDbControl, IPhysicalRefDbLocation, IOpenConnectionErrorHandler, ITransactionStarter, IDbConnectionWithSettings
	{
		[ThreadSafe]
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		readonly static HashSet<WeakReference<DbConnection>> timeoutableConnections = new HashSet<WeakReference<DbConnection>>();
		public static IEnumerable<DbConnection> TimeoutableConnections
		{
			get
			{
				var result = new List<DbConnection>();
				lock (hashLock)
				{
					foreach (var reference in timeoutableConnections.ToArray())
					{
						if (reference.TryGetTarget(out var target))
						{
							result.Add(target);
						}
						else
						{
							timeoutableConnections.Remove(reference);
						}
					}
				}
				return result;
			}
		}
		[ThreadSafe]
		readonly static object hashLock = new object();
		[ThreadSafe]
#if DEBUG
		internal
#endif
		static bool timeoutInitialized;
		public static void InitializeTimeout()
		{
			timeoutInitialized = true;
		}

		#region Constructors

		protected DbConnection(IDataProviderFactory dataProviderFactory, string serverName, string databaseName, string applicationNameSuffix = null, IErrorReporter errorReporter = null)
		{
			Argument.NotNull(dataProviderFactory, nameof(dataProviderFactory));
			Argument.NotNullOrEmpty(serverName, nameof(serverName));
			Argument.NotNullOrEmpty(databaseName, nameof(databaseName));

			fServerName = serverName;
			fInitialDatabaseName = databaseName;
			this.errorReporter = errorReporter;
			SuffixedApplicationName = GetSuffixedApplicationName(applicationNameSuffix);

			DataProviderFactory = dataProviderFactory;

			ObjectID = Interlocked.Increment(ref nextObjectID);
			CheckServerNameIsNotLocalhost(serverName);
			threadSentry = ThreadSentryProvider.GetThreadSentry(true);
		}

		#endregion

		public IDataProviderFactory DataProviderFactory { get; init; }

		bool IsInTimeoutableConnections => reference != null;
		WeakReference<DbConnection> reference;
		Stopwatch lastActivity;

		public TimeSpan TimeSinceLastActive => lastActivity.Elapsed;

		public void CloseAndRemoveDueToTimeOut()
		{
			try
			{
				if (ThreadSentry.IsOwner)
				{
					this.CloseConnection();
				}
				else if (ThreadSentry.IsPostable)
				{
					ThreadSentry.Post((o) => { this.CloseConnection(); }, null, nameof(CloseAndRemoveDueToTimeOut));
					return; //timeoutableConnections will be handled in CloseConnection on the target thread.
				}
			}
			catch (SqlException) { } //maybe already killed or can't hit DB?
			lock (hashLock)
			{
				timeoutableConnections.Remove(reference);
			}
			reference = null;
		}

		[SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", Justification = "Calling getter")]
		public void RefreshLastActivity()
		{
			if (timeoutInitialized && DataProviderFactory.ConnectionShouldTimeOut)
			{
				LazyInitializer.EnsureInitialized(ref lastActivity).Restart();
				if (!IsInTimeoutableConnections)
				{
					reference = new WeakReference<DbConnection>(this);
					lock (hashLock)
					{
						timeoutableConnections.Add(reference);
					}
				}
			}
		}

		void DbConnection_Disposed(object sender, EventArgs e)
		{
			if (sender is System.Data.Common.DbConnection connection)
			{
				connection.StateChange -= StateChange;
				connection.Disposed -= DbConnection_Disposed;
			}
		}

		public IThreadSentry ThreadSentry
		{
			get
			{
				return threadSentry;
			}
		}

		static int nextObjectID;

		public string SuffixedApplicationName { get; }

		public static string ApplicationName { get; set; }

		public static string GetSuffixedApplicationName(string applicationNameSuffix)
		{
			var result = ApplicationName ?? DbConnectionConstants.ApplicationNames.CargoWiseOne;
			if (!string.IsNullOrEmpty(applicationNameSuffix))
			{
				result += applicationNameSuffix;
				if (result.Length > 128)
				{
					result = result.Substring(0, 128);
				}
			}
			return result;
		}

		protected abstract IDbConnection OpenNewDbConnection();

		#region Published

		internal Thread InstantiationThread
		{
			get { return instantiationThread; }
		}

		public readonly int ObjectID;
		public int ConnectionTimeout
		{
			get
			{
				return DbEnv.Instance.ConnectionTimeout;
			}
		}

		#region LockTimeout

		/// <summary>
		/// Contains constants that specify Lock time-out intervals in milliseconds.
		/// </summary>
		public static class LockTimeout
		{
			public const int Infinite = -1;
			public const int NoWait = 0;
			public const int SqlDefault = -1;

			public const int Default = 15000;

			/// <summary>
			/// Cached value from DbRegistry.LockTimeout, updated when main db connection opened.
			/// Will be the registry default value initially.
			/// </summary>
			internal static int CachedFromRegistry
			{
				get { return cachedFromRegistry; }
				set { cachedFromRegistry = value; }
			}

			[SuppressMessage("CargoWiseOne", "CW1021:Static Fields Are Thread Static Rule", Justification = "global cache")]
			static int cachedFromRegistry = Default;
		}

		public void SetLockTimeout(int lockTimeoutMs)
			=> ExecuteNonQuery($"SET LOCK_TIMEOUT {lockTimeoutMs};");

		int GetAndSetLockTimeout(int lockTimeoutMs)
		{
			return ExecuteScalar<int>($"select @@LOCK_TIMEOUT; SET LOCK_TIMEOUT {lockTimeoutMs}");
		}

		public IDisposable TemporarySetLockTimeout(TimeSpan lockTimeout)
		{
			return TemporarySetLockTimeout((int)lockTimeout.TotalMilliseconds);
		}

		public IDisposable TemporarySetLockTimeout(int lockTimeoutMs)
		{
			var originalLockTimeout = GetAndSetLockTimeout(lockTimeoutMs);
			return new DisposableAction(() =>
			{
				if (State == ConnectionState.Open)
				{
					SetLockTimeout(originalLockTimeout);
				}
			});
		}

		#endregion // LockTimeout

		public abstract string UserLogin { get; }

		public IDisposable TemporarySetDeadlockPriority(DeadlockPriority deadlockPriority)
		{
			return TemporarySetDeadlockPriority((int)deadlockPriority);
		}

		public IDisposable TemporarySetDeadlockPriority(int deadlockPriority)
		{
			if (!(deadlockPriority >= (int)Data.DeadlockPriority.Min && deadlockPriority <= (int)Data.DeadlockPriority.Max))
			{
				throw new ArgumentException("Deadlock priority must be between -10 and 10", nameof(deadlockPriority));
			}

			var originalDeadlockPriority = DeadlockPriority;
			SetDeadlockPriority(deadlockPriority);
			return new DisposableAction(() =>
			{
				if (State == ConnectionState.Open)
				{
					SetDeadlockPriority(originalDeadlockPriority);
				}
			});
		}

		#region Caching

		public IDisposable StartScalarCaching()
		{
			if (Cache == null)
			{
				Cache = new ScalarCache();
			}
			else
			{
				throw new InvalidOperationException("Only one scalar cache per connection is supported");
				// Exception message only seen by developers
			}

			return new DisposableAction(() => { Cache = null; });
		}

#if DEBUG
		public IDisposable StartScalarCaching_ForTest(ScalarCache cache)
		{
			Cache = cache;
			return new DisposableAction(() => { Cache = null; });
		}
#endif

		internal ScalarCache Cache { get; private set; }

		#endregion

		#region Connection

		#region SQL Server Version

		public enum SqlServerEdition
		{
			Other = 0, // (1 = deprecated, 5 = SQL Azure)
			StandardWorkgroup = 2, // Standard, Web, Business Intelligence, Small Business Server, Workgroup
			EnterpriseDeveloper = 3, // Enterprise (all types), Developer, Evaluation, Data Center
			Express = 4, // Express (all types), Windows Embedded SQL
		}

		public SqlServerVersionNumber ServerVersionNumber
		{
			get
			{
				if (serverVersionNumber == null)
				{
					serverVersionNumber = new SqlServerVersionNumber(ProductVersion);
				}

				return serverVersionNumber;
			}
		}

		SqlServerVersionNumber serverVersionNumber;

		public static string SelectServerEditionCommandText => "SELECT CONVERT(int, SERVERPROPERTY('EngineEdition'))";
		public static SqlServerEdition ConvertToSqlServerEdition(int edition)
		{
			switch (edition)
			{
				case (int)SqlServerEdition.StandardWorkgroup:
					return SqlServerEdition.StandardWorkgroup;

				case (int)SqlServerEdition.EnterpriseDeveloper:
					return SqlServerEdition.EnterpriseDeveloper;

				case (int)SqlServerEdition.Express:
					return SqlServerEdition.Express;

				default:
					return SqlServerEdition.Other;
			}
		}

		/// <summary>
		/// Returns the SQL Server edition.
		/// </summary>
		public SqlServerEdition ServerEdition
		{
			get
			{
				if (!fServerEdition.HasValue)
				{
					using (DbCommand command = Command(SelectServerEditionCommandText))
					{
						fServerEdition = ConvertToSqlServerEdition(Convert.ToInt32(command.ExecuteScalar()));
					}
				}

				return fServerEdition.Value;
			}
		}

		SqlServerEdition? fServerEdition;

		public string ServerEditionText
		{
			get
			{
				if (serverEditionText == null)
				{
					using (DbCommand command = Command("SELECT SERVERPROPERTY('Edition')"))
					{
						var commandResult = command.ExecuteScalar();

						if (commandResult != null)
						{
							serverEditionText = commandResult.ToString();
						}
						else
						{
							throw new InvalidOperationException("Unable to get server edition");
						}
					}
				}

				return serverEditionText;
			}
		}

		string serverEditionText;

		public static string SelectProductVersionCommandText => "SELECT SERVERPROPERTY('ProductVersion')";
		public static string CheckedProductVersionString(string commandResult)
		{
			if (commandResult != null)
			{
				return commandResult;
			}
			else
			{
				throw new InvalidOperationException("Unable to get product version");
			}
		}

		public string ProductVersion
		{
			get
			{
				if (productVersion == null)
				{
					using (DbCommand command = Command(SelectProductVersionCommandText))
					{
						productVersion = CheckedProductVersionString(command.ExecuteScalar()?.ToString());
					}
				}

				return productVersion;
			}
		}

		string productVersion;

		public static string SelectServerFullVersionTextCommandText => "SELECT @@VERSION";

		/// <summary>
		/// Returns the SQL Server full version text.
		/// </summary>
		public string ServerFullVersionText
		{
			get
			{
				if (fServerFullVersionText == null)
				{
					using (DbCommand command = Command(SelectServerFullVersionTextCommandText))
					{
						var commandResult = command.ExecuteScalar();
						fServerFullVersionText = commandResult.ToString();
					}
				}

				return fServerFullVersionText;
			}
		}

		string fServerFullVersionText;

		#endregion

		#region Snapshot Isolation

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public bool IsDbUsingSnapshotIsolation(string dbName)
		{
			Argument.NotNullOrEmpty(dbName, nameof(dbName)); // Suggested By ReviewBot

			if (!dbSnapshotEnabledDictionary.ContainsKey(dbName))
			{
				lock (dbSnapshotEnabledDictionary)
				{
					if (!dbSnapshotEnabledDictionary.ContainsKey(dbName))
					{
						try
						{
							dbSnapshotEnabledDictionary[dbName] = IsDatabaseUsingSnapshotIsolation(dbName);
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							// if it fails because the connection is closed or server became unavailable,
							// it does not create a dictionary entry,
							// but regards it as not using snapshot isolation for the time being
							return false;
						}
					}
				}
			}

			return dbSnapshotEnabledDictionary[dbName];
		}

		public bool IsCurrentDbUsingSnapshotIsolation
		{
			get
			{
				return IsDbUsingSnapshotIsolation(CurrentDatabase);
			}
		}

		bool IsDatabaseUsingSnapshotIsolation(string dbName)
		{
			const string sqlText = @"
				IF exists(
					SELECT name
					FROM sys.databases
					WHERE name = @dbName
					AND snapshot_isolation_state = 1
					AND is_read_committed_snapshot_on = 1
				)
					SELECT 1
				ELSE
					SELECT 0"; // Sql queries do not need translating
			using (UseMasterDb())
			using (var command = Command(sqlText))
			{
				command.AddParameter("@dbName", SqlDbType.NVarChar, 128, dbName);
				return Convert.ToBoolean(command.ExecuteScalar());
			}
		}

		/// <summary>
		/// Temporarily set currently used database to master, if not already.
		/// Useful when querying sys.databases, for example, so only one query plan is cached.
		/// </summary>
		public IDisposable UseMasterDb()
			=> CurrentDatabase != Db.SqlMasterDb ? ((ICurrentDbControl)this).UseDatabase(Db.SqlMasterDb) : null;

		internal static void RemoveCacheEntryFromSnapshotEnabledDictionary(string dbName)
		{
			Argument.NotNullOrEmpty(dbName, nameof(dbName)); // Suggested By ReviewBot

			if (dbSnapshotEnabledDictionary.ContainsKey(dbName))
			{
				lock (dbSnapshotEnabledDictionary)
				{
					if (dbSnapshotEnabledDictionary.ContainsKey(dbName))
					{
						dbSnapshotEnabledDictionary.Remove(dbName);
					}
				}
			}
		}

		static readonly Dictionary<string, bool> dbSnapshotEnabledDictionary =
			new Dictionary<string, bool>();

		#endregion

		#region Session Properties

		public int SPID
		{
			get
			{
				if (spid == null)
				{
					using (var cmd = Command("SELECT @@spid"))
					{
						spid = Convert.ToInt32(cmd.ExecuteScalar());
					}
				}

				return spid.Value;
			}
		}

		int? spid;

		public DateTime LoginTime
		{
			get
			{
				if (loginTime == null)
				{
					using (var cmd = Command("SELECT login_time FROM sys.dm_exec_sessions WHERE session_id = @@spid"))
					{
						loginTime = Convert.ToDateTime(cmd.ExecuteScalar());
					}
				}

				return loginTime.Value;
			}
		}

		DateTime? loginTime;

		void ResetCachedSessionProperties()
		{
			spid = null;
			loginTime = null;
		}

		#endregion

		public string ImpersonatedLogin { get; set; }

		public string CurrentDatabase
		{
			get
			{
				var database = InternalConnection.Database;
				return database;
			}
			private set
			{
				Argument.NotNullOrEmpty(value, nameof(value));

				try
				{
					InternalConnection.ChangeDatabase(value);
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					if (ConnectionErrorHandler.HandleDisconnectionAndSecurityErrors(e))
					{
						InternalConnection.ChangeDatabase(value);
					}
					else
					{
						throw;
					}
				}
			}
		}

		IDisposable ICurrentDbControl.UseDatabase(string database)
		{
			string previousDatabase = CurrentDatabase;
			CurrentDatabase = database;
			return new DisposableAction(() => CurrentDatabase = previousDatabase);
		}

		string ICurrentDbControl.InitialDatabase
		{
			get { return fInitialDatabaseName; }
		}

		/// <summary>
		/// Run a dummy command to ensure the database connection is open.
		/// If it's not connected, the DisconnectionHandler will reconnect it.
		/// </summary>
		public void EnsureIsOpen()
		{
			using (var cmd = Command("--Connection.EnsureIsOpen"))
			{
				cmd.ExecuteNonQuery();
			}
		}

		/// <summary>
		/// Is this DbConnection a wrapper around the given SqlConnection?
		/// </summary>
		/// <param name="sqlConnection">The SqlConnection to test.</param>
		/// <returns>True if the SqlConnection passed is the same object that this DbConnection is wrapping. Otherwise False.</returns>
		public bool IsWrapping(IDbConnection sqlConnection)
		{
			Argument.NotNull(sqlConnection, nameof(sqlConnection));

			return ReferenceEquals(sqlConnection, InternalConnection);
		}

		public ConnectionState State
		{
			get
			{
				return InternalConnectionUnsafe is null ? ConnectionState.Closed : InternalConnectionUnsafe.State;
			}
		}

		internal virtual ConnectionErrorManager ConnectionErrorHandler
		{
			get
			{
				return connectionErrorHandler ?? (connectionErrorHandler = new ConnectionErrorManager(this));
			}
		}

		protected ConnectionErrorManager connectionErrorHandler;

		#endregion

		#region Commands & Adapters

		public bool IgnoreCommitTracker { get; set; }

		public int DefaultCommandTimeOutInSeconds
		{
			get
			{
				return fDefaultCommandTimeOutInSeconds;
			}
			set
			{
				if (value < 0)
				{
					throw new ArgumentException("Invalid argument.", nameof(value));
				}

				fDefaultCommandTimeOutInSeconds = value;
			}
		}

		int fDefaultCommandTimeOutInSeconds = fInitialDefaultCmdTimeout;

		public IDisposable TemporarySetDefaultCommandTimeOut(int timeoutSec)
		{
			var originalTimeoutSec = DefaultCommandTimeOutInSeconds;
			DefaultCommandTimeOutInSeconds = timeoutSec;

			return new DisposableAction(() => DefaultCommandTimeOutInSeconds = originalTimeoutSec);
		}

		public DbCommand Command(string sqlText)
		{
			Argument.NotNull(sqlText, nameof(sqlText));

			return Command(sqlText, null);
		}

		public DbCommand Command(string sqlText, int? cmdTimeoutInSeconds)
		{
			Argument.NotNull(sqlText, nameof(sqlText));
			if (!(!cmdTimeoutInSeconds.HasValue || cmdTimeoutInSeconds.Value >= 0))
			{
				throw new ArgumentException("Invalid argument.", nameof(cmdTimeoutInSeconds));
			}

			int timeoutInSeconds = cmdTimeoutInSeconds ?? DefaultCommandTimeOutInSeconds;

			return new DbCommand(sqlText, this, InternalConnection, internalTransaction, timeoutInSeconds); // The only place to use "new DbCommand"
		}

		public T ExecuteScalar<T>(string sqlText, Action<DbCommand> parameters = null)
		{
			Argument.NotNull(sqlText, nameof(sqlText));

			object result = ExecuteScalar(sqlText, parameters);

			if (result == DBNull.Value || result == null)
			{
				throw new ExecuteScalarReturnedNullException();
			}
			if (result is T)
			{
				return (T)result;
			}
			else
			{
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Statement returned a value of type {0} when a value of type {1} was expected", result.GetType().Name, typeof(T).Name));
			}
		}

		public object ExecuteScalar(string sqlText, Action<DbCommand> parameters = null)
		{
			Argument.NotNull(sqlText, nameof(sqlText));

			using (var cmd = Command(sqlText))
			{
				parameters?.Invoke(cmd);

				return cmd.ExecuteScalar();
			}
		}

		public int ExecuteNonQuery(string sqlText)
		{
			Argument.NotNull(sqlText, nameof(sqlText));

			using (var cmd = Command(sqlText))
			{
				return cmd.ExecuteNonQuery();
			}
		}

		public int ExecuteNonQuery(string sqlText, Action<DbCommand> parameters)
		{
			Argument.NotNull(sqlText, nameof(sqlText));
			Argument.NotNull(parameters, nameof(parameters));

			using (var cmd = Command(sqlText))
			{
				parameters(cmd);
				return cmd.ExecuteNonQuery();
			}
		}

		public int ExecuteNonQuery(string sqlText, int? cmdTimeoutInSeconds)
		{
			Argument.NotNull(sqlText, nameof(sqlText));

			using (var cmd = Command(sqlText, cmdTimeoutInSeconds))
			{
				return cmd.ExecuteNonQuery();
			}
		}

		public int ExecuteNonQueryWithRetry(string sqlText, RetryPolicy policy = null, EventHandler<RetryingEventArgs> retryingEventHandler = null, Func<Exception, bool> stopRetryCondition = null)
		{
			Argument.NotNull(sqlText, nameof(sqlText));

			var retryPolicy = policy ?? DataUtils.LockTimeoutRetryPolicy;

			if (retryingEventHandler != null)
			{
				retryPolicy.Retrying += retryingEventHandler;
			}

			try
			{
				return retryPolicy.ExecuteAction(() =>
				{
					try
					{
#if DEBUG
						if (RetryContext_ForTest != null
							&& (RetryContext_ForTest.Condition == null
								|| RetryContext_ForTest.Condition.Invoke(sqlText, RetryContext_ForTest.ExecutionCount)))
						{
							RetryContext_ForTest.ExecutionCount++;
							RetryContext_ForTest.Action?.Invoke(sqlText, RetryContext_ForTest.ExecutionCount);
						}
#endif

						return ExecuteNonQuery(sqlText);
					}
					catch (Exception e) when (!e.IsCriticalException() && stopRetryCondition?.Invoke(e) == true)
					{
						return -1;
					}
				});
			}
			finally
			{
				if (retryingEventHandler != null)
				{
					retryPolicy.Retrying -= retryingEventHandler;
				}
			}
		}

		public void ExecuteReader(string sqlText, Action<IDataRecord> action)
		{
			Argument.NotNullOrEmpty(sqlText, nameof(sqlText));
			Argument.NotNull(action, nameof(action));

			using (var cmd = Command(sqlText))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					action(reader);
				}
			}
		}

		public void ExecuteReader(string sqlText, Action<DbCommand> parameters, Action<IDataRecord> action)
		{
			Argument.NotNullOrEmpty(sqlText, nameof(sqlText));
			Argument.NotNull(parameters, nameof(parameters));
			Argument.NotNull(action, nameof(action));

			using (var cmd = Command(sqlText))
			{
				parameters(cmd);

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						action(reader);
					}
				}
			}
		}

		public bool Exists(string sqlFromAndWhereClause, Action<DbCommand> parameters = null)
		{
			var sql = FormattableString.Invariant($"SELECT CONVERT(bit, CASE WHEN EXISTS(SELECT NULL {sqlFromAndWhereClause}) THEN 1 ELSE 0 END);");

			using (var cmd = Command(sql))
			{
				parameters?.Invoke(cmd);
				return Convert.ToBoolean(cmd.ExecuteScalar(), CultureInfo.InvariantCulture);
			}
		}

		#endregion

		#region Transaction

		public delegate void TransactionedFunction();

		public void RunTransactioned(TransactionedFunction function)
		{
			Argument.NotNull(function, nameof(function)); // Suggested By ReviewBot

			BeginTransaction();

			try
			{
				function();
				CommitTransaction();
			}
#pragma warning disable ENT0001
			catch
			{
				try
				{
					RollbackTransaction();
				}
				catch (Exception)
				{
					try
					{
						EnsureIsOpen();
					}
					catch
#pragma warning restore ENT0001
					{
					}
				}
				throw;
			}
		}

#if DEBUG
		public
#endif
		void BeginTransaction()
		{
			BeginTransactionCore();
		}

		public void RunInTransaction(Action action, bool doCommit = true)
		{
			using (var tran = BeginTransactionWithManager())
			{
				action?.Invoke();
				if (doCommit)
				{
					tran.CommitTransaction();
				}
			}
		}

		public ITransactionManager BeginTransactionWithManager()
		{
			return BeginTransactionWithManager(null);
		}

		public ITransactionManager BeginTransactionWithManager(Action onAutoRollbackAction)
		{
			BeginTransactionCore();
			return new TransactionManager(this, onAutoRollbackAction);
		}

		/// <summary>
		/// Create a transaction manager which will only start the transaction when the
		/// next transaction request occurs but will keep the transaction open for the scope
		/// of the transaction manager lifetime.
		/// </summary>
		public IDelayedTransactionManager DelayedTransactionWithManager(ITransactionLockManager transactionLockManager)
		{
			if (delayedTransactionManager != null)
			{
				delayedTransactionManager.AggregateLockManager.AddLockManager(transactionLockManager);
				delayedTransactionManager.IncreaseNestingDepth();
				return delayedTransactionManager;
			}

			delayedTransactionManager = new DelayedTransactionManager(this, new AggregateLockManager(transactionLockManager), new TransactionManager(this));
			if (appTransactionCount > 0)
			{
				ModifyAppTransactionCount(1);
			}

			return delayedTransactionManager;
		}

		public bool TryGetTransactionLockManager(out ITransactionLockManager transactionLockManager)
		{
			if (delayedTransactionManager != null)
			{
				transactionLockManager = delayedTransactionManager.AggregateLockManager;
				return true;
			}
			else
			{
				transactionLockManager = null;
				return false;
			}
		}

		public bool IsDelayedTransaction => delayedTransactionManager != null;

		DelayedTransactionManager delayedTransactionManager;

		/// <summary>
		/// Starts a database transaction
		/// Or increments the transaction level in case it is already in a transaction context
		/// Note:
		///		Trap the Connection Error, trying it one more time in case the 1st one fails
		/// </summary>
		void BeginTransactionCore()
		{
			if (!IsInTransaction)
			{
				try
				{
					if (delayedTransactionManager?.IsFinishedOrDisposed ?? false)
					{
						ErrorReporter.ReportOnce(null, "Attempting to Open a Transaction in an already-finished delayed transaction. This transaction will most probably be leaked.", null);
					}

					internalTransaction = InternalConnection.BeginTransaction();
					++TopLevelTransactionsBegunCount;
				}
				catch (DatabaseUpgradedException)
				{
					throw;
				}
				catch (AggregateException ex) when (ex.Flatten().InnerExceptions.Any(e => e.FlattenInnerExceptions().Any(inner => inner is DatabaseUpgradedException)))
				{
					throw;
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					if (ConnectionErrorHandler.ReconnectIfApplicable(e))
					{
						EnsureIsOpen();
						internalTransaction = InternalConnection.BeginTransaction();
						++TopLevelTransactionsBegunCount;
					}
					else
					{
						var dbReconnectionHandling = this as IDbReconnectionHandling;
						ErrorReporter.ReportOnce(
							"UnableReconnected_DebugInformation",
							$"Unable to reconnect connection, state :{dbReconnectionHandling.State}, IsConnecting {dbReconnectionHandling.IsConnecting}, AppTransactionCount {dbReconnectionHandling.AppTransactionCount}",
							e);
						throw;
					}
				}

				MustRollBack = false;
			}

			var modifiedCount = IsDelayedTransaction && appTransactionCount == 0
				? 2
				: 1;

			RecordCurrentThreadId(modifiedCount);
		}

#if DEBUG
		public
#endif
		void RollbackTransaction()
		{
			if (appTransactionCount > 1)
			{
				MustRollBack = true;
				ModifyAppTransactionCount(-1);
				delayedTransactionManager?.TrackRollback();
				return;
			}

			try
			{
				RollbackTransactionCore();
			}
			finally
			{
				ResetAppTransaction();
			}
		}

		void RollbackTransactionCore()
		{
			try
			{
				if (IsInTransaction)
				{
					FireCommitTrackerOnRollback();
					if (internalTransaction != null)
					{
						internalTransaction.Rollback();
						internalTransaction.Dispose();
						delayedTransactionManager?.TrackRollback();
					}
				}
			}
			catch (SqlException e)
			{
				// The ROLLBACK TRANSACTION request has no corresponding BEGIN TRANSACTION.
				// it happens when we get an error in a trigger, and the trigger rolls back
				// the whole transaction automatically, and by time we catch the error and rollback,
				// the transaction would no longer be there.
				IDbErrorMatch errorHandler = new DbErrorMatch(e);

				if (errorHandler.ExceptionType != DbErrorType.RollBackTranHasNoCorrespondingBeginTran)
				{
					throw;
				}
			}
		}

#if DEBUG
		public
#endif
		void CommitTransaction()
		{
			if (appTransactionCount > 1)
			{
				ModifyAppTransactionCount(-1);
				return;
			}

			var committed = false;
			try
			{
				if (appTransactionCount == 1 && !MustRollBack)
				{
					if (internalTransaction == null)
					{
						throw new InvalidOperationException("internalTransaction is null");
					}

					internalTransaction.Commit();
					committed = true;
					FireCommitTrackerOnCommit();
				}
				else
				{
					RollbackTransactionCore();
					if (MustRollBack)
					{
						throw new TransactionException("Cannot commit => a nested transaction was rolled back", OdysseyDataErrorType.TransactionRolledBack);
					}
					else
					{
						throw new TransactionException("Cannot commit => no corresponding transaction", OdysseyDataErrorType.NoTransactionToCommit);
					}
				}
			}
			finally
			{
				ResetAppTransactionCore(committed);
			}
		}

		public bool MustRollBack { get; private set; }

		public bool IsInTransaction
		{
			get
			{
				return internalTransaction != null && internalTransaction.Connection != null;
			}
		}

		public long TopLevelTransactionsBegunCount { get; private set; }

		public virtual bool IsInTransactionOtherThanTransactionedTestCase
		{
			get { return IsInTransaction; }
		}

		#region Transaction Implementation

		/// <summary>
		/// Resets the transaction object and count at the application side
		/// </summary>
		void ResetAppTransaction() => ResetAppTransactionCore(false);

		/// <summary>
		/// Resets the transaction object and count at the application side.
		/// </summary>
		/// <param name="committed">true if due to a successful outermost commit</param>
		void ResetAppTransactionCore(bool committed)
		{
			internalTransaction = null;
			var initialAppTransactionCount = appTransactionCount;
			appTransactionCount = 0;
			if (enableRecordAppTransactionCountChanged)
			{
				appTransactionCountAddedStackTrace = new List<(int, StackTrace)>();
				appTransactionCountReducedStackTrace = new List<(int, StackTrace)>();
			}
			MustRollBack = false;
			ResetTransactionThreadsInfo();

			if (initialAppTransactionCount != 0 || committed)
			{
				RaiseAppTransactionCountReset(committed);
			}
		}

		/// <summary>
		/// Fires when AppTransactionCount is reset to zero after an outermost commit or rollback.
		/// </summary>
		public event EventHandler<AppTransactionCountResetEventArgs> AppTransactionCountReset;

		void RaiseAppTransactionCountReset(bool committed)
			=> AppTransactionCountReset?.Invoke(this, new AppTransactionCountResetEventArgs(committed));

		public class AppTransactionCountResetEventArgs : EventArgs
		{
			public AppTransactionCountResetEventArgs(bool committed) => Committed = committed;
			/// <summary>
			/// True if an outermost transaction was committed, false if rolled back.
			/// </summary>
			public bool Committed { get; }
		}

		[Conditional("DEBUG")]
		void FireCommitTrackerOnRollback()
		{
#if DEBUG
			DbCommitTracker.OnRollback(this);
#endif
		}

		[Conditional("DEBUG")]
		void FireCommitTrackerOnCommit()
		{
#if DEBUG
			DbCommitTracker.OnCommit(this);
#endif
		}

		IDbTransaction internalTransaction;

		#endregion

		#region Transaction threads monitoring

		void RecordCurrentThreadId(int modifiedCount)
		{
			try
			{
				if (transactionThreads == null)
				{
					transactionThreads = new List<int>();
				}

				var threadId = Thread.CurrentThread.ManagedThreadId;
				if (!transactionThreads.Contains(threadId))
				{
					if (transactionThreads.Count > 0)
					{
						if (!string.IsNullOrEmpty(lastDifferentThreadStackTraces))
						{
							ErrorReporter.ReportOnce(
								"SameTransactionInMultipleThreads_WithStackTrace",
								"Same transaction is being used from multiple threads. Last different thread stack trace:" + Environment.NewLine + lastDifferentThreadStackTraces,
								null);
						}
						else
						{
							ErrorReporter.ReportOnce("SameTransactionInMultipleThreads", "Same transaction is being used from multiple threads.", null);
						}

						// Store stack trace only for another thread - for performance
						lastDifferentThreadStackTraces = Environment.StackTrace;
					}

					transactionThreads.Add(threadId);
				}

				ModifyAppTransactionCount(modifiedCount);
			}
			catch (Exception ex) when (!ex.IsCriticalException()) { }
		}

		void ResetTransactionThreadsInfo()
		{
			transactionThreads = null;
			lastDifferentThreadStackTraces = null;
		}

		List<int> transactionThreads;
		string lastDifferentThreadStackTraces;

		#endregion

		#endregion

		#region ExecutedCommandCount

#if DEBUG

		internal void IncrementExecutedCommandCount(string commandText, IDataParameterCollection parameters)
		{
			Argument.NotNull(parameters, nameof(parameters));
			Argument.NotNull(commandText, nameof(commandText));
			Interlocked.Increment(ref executedCommandCount);
			Interlocked.Increment(ref executedCommandCountForAllConnections);

			if (executedCommands != null)
			{
				var queryWithMoreInfo = commandText;

				if (parameters.OfType<IDataParameter>().Any())
				{
					queryWithMoreInfo += Environment.NewLine + "Params" + Environment.NewLine;

					foreach (var parameter in parameters.OfType<IDataParameter>())
					{
						queryWithMoreInfo += string.Format(CultureInfo.InvariantCulture, "{0}: {1}" + Environment.NewLine, parameter.ParameterName, parameter.GetFormattedValue());
					}
				}

				if (executedCommands.IncludeStackTraces)
				{
					queryWithMoreInfo += Environment.NewLine + Environment.StackTrace;
				}

				executedCommands.Add(commandText, queryWithMoreInfo);
			}
		}

		internal void AddExecutionPlan(string originalQuery, string queryPlanXml)
		{
			executedCommands?.AddExecutionPlan(originalQuery, queryPlanXml);
		}

		int executedCommandCount;

		static int executedCommandCountForAllConnections;

		TrackedExecutedCommands executedCommands;

		internal class TrackedExecutedCommands
		{
			internal void Add(string originalQuery, string queryWithMoreInfo)
			{
				Commands.Add(new CommandInfo(originalQuery, queryWithMoreInfo));
			}

			internal void AddExecutionPlan(string originalQuery, string queryPlanXml)
			{
				var commandInfo = Commands.LastOrDefault(c => c.ExecutedCommand == originalQuery);

				if (commandInfo != null)
				{
					commandInfo.ExecutionPlans.Add(queryPlanXml);
				}
			}

			internal bool IncludeStackTraces { get; set; }
			internal bool IncludeQueryPlansForExecuteReaderCommands { get; set; }

			internal List<CommandInfo> Commands => commands;
			readonly List<CommandInfo> commands = new List<CommandInfo>();
			internal class CommandInfo
			{
				internal CommandInfo(string executedCommand, string executedCommandWithMoreInfo)
				{
					ExecutedCommand = executedCommand;
					ExecutedCommandWithMoreInfo = executedCommandWithMoreInfo;
				}

				internal string ExecutedCommand { get; }
				internal string ExecutedCommandWithMoreInfo { get; }

				internal List<string> ExecutionPlans => executionPlans;
				readonly List<string> executionPlans = new List<string>();
			}
		}

#endif

		#endregion

		#region Database Info

		#region DB Files

		internal enum DatabaseFileTypes
		{
			Data = 0,
			Log = 1,
		}

		/// <summary>
		/// Returns physical location of all data files.
		/// </summary>
		/// <param name="databaseName">The name of the database for which to return all data files</param>
		/// <returns>string array of full file paths</returns>
		public string[] GetDBDataFiles(string databaseName)
		{
			Argument.NotNullOrEmpty(databaseName, nameof(databaseName));

			return GetDbFiles(databaseName, DatabaseFileTypes.Data);
		}

		/// <summary>
		/// Returns physical location of all log files.
		/// </summary>
		/// <param name="databaseName">The name of the database for which to return all log files</param>
		/// <returns>string array of full file paths</returns>
		public string[] GetDBLogFiles(string databaseName)
		{
			Argument.NotNullOrEmpty(databaseName, nameof(databaseName));

			return GetDbFiles(databaseName, DatabaseFileTypes.Log);
		}

		/// <summary>
		/// Returns physical location of all database files.
		/// </summary>
		/// <param name="dbName">The name of the database for which to return all files</param>
		/// <returns>string array of full file paths</returns>
		public string[] GetDbFiles(string dbName)
		{
			Argument.NotNullOrEmpty(dbName, nameof(dbName));

			return GetDbFiles(dbName, fileType: null);
		}

		string[] GetDbFiles(string dbName, DatabaseFileTypes? fileType)
		{
			Argument.NotNullOrEmpty(dbName, nameof(dbName));

			string[] result;

			string fileTypePredicate = (fileType.HasValue) ? ("WHERE type = " + ((int)fileType.Value).ToString()) : "";
			string sqlText = string.Format(@"
				SELECT physical_name
				FROM [{0}].sys.database_files
				{1}", // Sql queries do not need translating
				dbName,
				fileTypePredicate);

			try
			{
				DataTable databases = DataUtils.GetDataTableFromQuery(this, sqlText);

				int dbCount = databases.Rows.Count;
				result = new string[dbCount];

				for (int i = 0; i < dbCount; i++)
				{
					var row = databases.Rows[i];
					var column = row[0];
					result[i] = column.ToString().Trim().ToUpper();
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				result = Array.Empty<string>();
			}

			return result;
		}

		#endregion

		#region Database Lists

		public IEnumerable<string> GetDatabases(DatabaseType databaseTypeFlags, bool? writable = null)
		{
			return GetDbList(databaseTypeFlags, sdDatabaseReadonlyFilter: writable.HasValue ? !writable.Value : null);
		}

		public IEnumerable<string> GetOperationalRepositoryAndFullRecoveryDbs()
		{
			var result = new List<string>
			{
				fInitialDatabaseName
			};

			var operationalAndRpositoryDbNames = GetDatabases(DatabaseType.Operational | DatabaseType.UserRepository | DatabaseType.Audit);
			var allDatabases = GetDatabases(DatabaseType.AllExclusive & ~DatabaseType.BI);

			string sql = string.Format(CultureInfo.InvariantCulture, @"
								SELECT db.name

								FROM sys.databases db

								WHERE
								(
									db.recovery_model <> 3
									AND db.name in ({0})
								)
								OR db.name in ({1})",
								string.Join(",", allDatabases.Select(db => "'" + db + "'")),
								string.Join(",", operationalAndRpositoryDbNames.Select(db => "'" + db + "'")));
			using (var cmd = Command(sql))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					var dbName = reader[0].ToString();
					if (!result.Contains(dbName))
					{
						result.Add(dbName);
					}
				}
			}

			return result;
		}

		public IEnumerable<string> GetReadOnlyDocManagerDbList()
			=> GetDbList(DatabaseType.SD, sdDatabaseReadonlyFilter: true);

		public bool IsAlwaysOnEnabled
		{
			get
			{
#if DEBUG
				if (IsAlwaysOnEnabledForTest != null)
				{
					return IsAlwaysOnEnabledForTest.Value;
				}
#endif

				if (isAlwaysOnEnabled == null)
				{
					var sqlText = "SELECT SERVERPROPERTY ('IsHadrEnabled')";
					using (var cmd = Command(sqlText))
					{
						isAlwaysOnEnabled = Convert.ToBoolean(cmd.ExecuteScalar());
					}
				}

				return isAlwaysOnEnabled.Value;
			}
		}
		bool? isAlwaysOnEnabled;

		/// <summary>
		/// Should not be stored in a lazy instantiated attribute as DB list is dynamic
		/// i.e. new databases may be created at any time
		/// </summary>
		/// <param name="sdDatabaseReadonlyFilter">Filter for the read-only flag.
		/// Set true to return only read-only databases, false for only writable, null for both</param>
		List<string> GetDbList(DatabaseType databaseTypeFlags, bool? sdDatabaseReadonlyFilter)
		{
			var dbQueryBuilder = new StringBuilder();
			var result = new SortedDictionary<int, List<string>>();
			var refDbsAreCached = exclusiveRefDbs != null || sharedRefDbs != null;
			var includeExclusiveRef = databaseTypeFlags.HasFlag(DatabaseType.ExclusiveRef);
			var includeSharedRef = databaseTypeFlags.HasFlag(DatabaseType.SharedRef);

			var includeAudit = databaseTypeFlags.HasFlag(DatabaseType.Audit);
			var includeEdw = databaseTypeFlags.HasFlag(DatabaseType.EDW);

			//
			// PREPARE DATABASE METADATA QUERY
			using (var cmd = Command(string.Empty))
			{
				if (databaseTypeFlags.HasFlag(DatabaseType.Main))
				{
					dbQueryBuilder.AppendLine($@"
SELECT d.name, 0 AS dbGroup
FROM   sys.databases AS d
WHERE name = @DbName
");

					cmd.AddParameter("@DbName", SqlDbType.NVarChar, 128, fInitialDatabaseName);
				}

				if (databaseTypeFlags.HasFlag(DatabaseType.SD))
				{
					AppendUnionAllIfRequired();
					dbQueryBuilder.AppendLine($@"
SELECT d.name, 1 AS dbGroup
FROM   sys.databases AS d
WHERE 1=1
	AND name LIKE @SdDbPattern
");

					cmd.AddParameter("@SdDbPattern", SqlDbType.NVarChar, 200, DataUtils.ReplaceSqlLikeWildcard(fInitialDatabaseName) + Db.StorageDocDbSuffixSqlPattern);

					if (sdDatabaseReadonlyFilter != null)
					{
						dbQueryBuilder.AppendLine("\tAND is_read_only = @isReadOnly");
						cmd.AddParameter("@isReadOnly", SqlDbType.Bit, 1, sdDatabaseReadonlyFilter.Value ? 1 : 0);
					}
				}

				if (includeExclusiveRef || includeSharedRef)
				{
					if (!refDbsAreCached)
					{
						exclusiveRefDbs = new List<string>();
						sharedRefDbs = new List<string>();

						AppendUnionAllIfRequired();
						dbQueryBuilder.AppendLine($@"
SELECT DISTINCT
	d.name,
	dbGroup = d.dbGroup + IIF(s.base_object_name IS NULL AND d.dbGroup = 2, 2, 0)
FROM refdbs AS d
	LEFT JOIN {fInitialDatabaseName.QuoteName()}.sys.synonyms AS s ON 1=1
		AND d.name = substring(s.base_object_name, 2, charindex(']', s.base_object_name) -2) COLLATE Database_Default
		AND s.name LIKE @synonymPattern
WHERE 1=2
	OR s.base_object_name IS NOT NULL
	OR d.dbGroup <> 3
");

						dbQueryBuilder.Insert(0, $@"
;WITH
	Refdbs AS
	(
		SELECT d.name, 3 AS dbGroup
		FROM   sys.databases AS d
		WHERE 1=1
			AND
			(1=2
				OR d.name LIKE @dbNamePatternShared
				OR d.name LIKE @dbNamePatternSharedAvailabilityGroup
			)

		UNION ALL

		SELECT DISTINCT d.name, 2 AS dbGroup
		FROM   sys.databases AS d

		WHERE 1=1
		AND d.name LIKE @dbNamePatternExclusive
	)
");

						cmd.AddParameter(
							"@dbNamePatternExclusive",
							SqlDbType.NVarChar,
							1000,
							$"{DataUtils.ReplaceSqlLikeWildcard(fInitialDatabaseName)}[_]{DataUtils.ReplaceSqlLikeWildcard(RefDbTableNameResolver.RefDbAffix)}[_]%");

						cmd.AddParameter(
							"@dbNamePatternShared",
							SqlDbType.NVarChar,
							1000,
							$"{DataUtils.ReplaceSqlLikeWildcard(RefDbTableNameResolver.SharedDbPrefix)}{DataUtils.ReplaceSqlLikeWildcard(RefDbTableNameResolver.RefDbAffix)}-%");
						cmd.AddParameter(
							"@dbNamePatternSharedAvailabilityGroup",
							SqlDbType.NVarChar,
							1000,
							$"{DataUtils.ReplaceSqlLikeWildcard(RefDbTableNameResolver.SharedAvailabilityGroupRefDbPrefix)}%");

						cmd.AddParameter(
							"@synonymPattern",
							SqlDbType.NVarChar,
							1000,
							$"{DataUtils.ReplaceSqlLikeWildcard(RefDbTableNameResolver.RefDbAffix)}%");
					}
				}

				if (databaseTypeFlags.HasFlag(DatabaseType.UserRepository))
				{
					AppendUnionAllIfRequired();
					dbQueryBuilder.AppendLine($@"
SELECT d.name, 5 AS dbGroup
FROM   sys.databases AS d
WHERE name = @UserRepositoryDbName
");

					cmd.AddParameter("@UserRepositoryDbName", SqlDbType.NVarChar, 128, fInitialDatabaseName + DbUserRepository.RepositoryDbSuffix);
				}

				if (includeAudit || includeEdw)
				{
					using (((ICurrentDbControl)this).UseDatabase(fInitialDatabaseName))
					{
						if (includeAudit)
						{
							try
							{
								var serverName = DbRegistry.BiAuditServer.LoadValue(this);
#if DEBUG
								if (string.IsNullOrWhiteSpace(serverName))
								{
									serverName = ServerName;
								}
#endif
								if (ServerName.Equals(serverName, StringComparison.OrdinalIgnoreCase))
								{
									AppendUnionAllIfRequired();
									dbQueryBuilder.AppendLine($@"
SELECT d.name, 6 AS dbGroup
FROM   sys.databases AS d
WHERE name = @AuditDbName
");

									cmd.AddParameter("@AuditDbName", SqlDbType.NVarChar, 128, fInitialDatabaseName + Db.AuditDatabaseSuffix);
								}
							}
							catch (SqlException ex)
							{
								var exceptionType = new DbErrorMatch(ex).ExceptionType;
								if (exceptionType != DbErrorType.InvalidObjectName && exceptionType != DbErrorType.InvalidColumnName)
								{
									throw;
								}
							}
						}

						if (includeEdw)
						{
							try
							{
								var serverName = DbRegistry.BiDataWarehouseServer.LoadValue(this);
#if DEBUG
								if (string.IsNullOrWhiteSpace(serverName))
								{
									serverName = ServerName;
								}
#endif
								if (ServerName.Equals(serverName, StringComparison.OrdinalIgnoreCase))
								{
									AppendUnionAllIfRequired();
									dbQueryBuilder.AppendLine($@"
SELECT d.name, 7 AS dbGroup
FROM   sys.databases AS d
WHERE name = @EdwDbName
");

									cmd.AddParameter("@EdwDbName", SqlDbType.NVarChar, 128, fInitialDatabaseName + Db.EdwDatabaseSuffix);
								}
							}
							catch (SqlException ex)
							{
								var exceptionType = new DbErrorMatch(ex).ExceptionType;
								if (exceptionType != DbErrorType.InvalidObjectName && exceptionType != DbErrorType.InvalidColumnName)
								{
									throw;
								}
							}
						}
					}
				}

				if (databaseTypeFlags.HasFlag(DatabaseType.SingleSharedRef))
				{
					AppendUnionAllIfRequired();
					dbQueryBuilder.AppendLine($@"
SELECT d.name, 8 AS dbGroup
FROM   sys.databases AS d
WHERE name = @SingleSharedRefDbName
");

					cmd.AddParameter("@SingleSharedRefDbName", SqlDbType.NVarChar, 128, RefDbTableNameResolver.SingleRefDatabaseName);
				}

				//
				// RUN METADATA QUERY TO SELECT DATABASE NAME LIST
				var useSynonyms = false;

				if (dbQueryBuilder.Length > 0)
				{
					dbQueryBuilder.AppendLine(@"
ORDER BY dbGroup ASC, name ASC
");
					cmd.CommandText = dbQueryBuilder.ToString();

					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							var dbGroup = (int)reader["dbGroup"];
							var dbName = (string)reader["name"];

							useSynonyms = useSynonyms || dbGroup == 2 || dbGroup == 3;

							if (dbGroup == 2 || (!useSynonyms && dbGroup == 4))
							{
								((List<string>)exclusiveRefDbs).Add(dbName);
							}
							else if (dbGroup == 3)
							{
								((List<string>)sharedRefDbs).Add(dbName);
							}

							if (dbGroup != 2 && dbGroup != 3 && dbGroup != 4) // do not include ref dbs here the are included from cache
							{
								if (!result.TryGetValue(dbGroup, out var dbNamesList))
								{
									result[dbGroup] = new List<string>();
								}

								result[dbGroup].Add(dbName);
							}
						}
					}
				}
			}

			if (includeExclusiveRef)
			{
				result[2] = exclusiveRefDbs.ToList();
			}

			if (includeSharedRef)
			{
				result[3] = sharedRefDbs.ToList();
			}

			return result.SelectMany(dbNameAndGroup => dbNameAndGroup.Value).ToList();

			void AppendUnionAllIfRequired()
			{
				if (dbQueryBuilder.Length > 0)
				{
					dbQueryBuilder.AppendLine("UNION ALL");
				}
			}
		}

		IEnumerable<string> exclusiveRefDbs;
		IEnumerable<string> sharedRefDbs;

		public string DbNameBySuffix(string dbSuffix)
		{
			if (string.IsNullOrWhiteSpace(dbSuffix))
			{
				return CurrentDatabase;
			}

			string dbName = CurrentDatabase + dbSuffix;
			if (IsExclusiveDatabase(dbName))
			{
				return CurrentDatabase + dbSuffix;
			}

			if (RefDbTableNameResolver.IsSharedDatabase(dbSuffix))
			{
				return dbSuffix;
			}

			return null;
		}

		public string DbSuffixByName(string dbName)
		{
			if (!string.IsNullOrWhiteSpace(dbName))
			{
				if (IsCurrentDatabase(dbName))
				{
					return string.Empty;
				}
				else if (IsExclusiveDatabase(dbName))
				{
					return dbName.Length > CurrentDatabase.Length ? dbName.Substring(CurrentDatabase.Length) : dbName;
				}
				else if (RefDbTableNameResolver.IsSharedDatabase(dbName))
				{
					return dbName;
				}
			}

			return null;
		}

		bool IsCurrentDatabase(string dbName)
		{
			Argument.NotNullOrEmpty(dbName, nameof(dbName)); // Suggested By ReviewBot

			return dbName.Equals(CurrentDatabase, StringComparison.OrdinalIgnoreCase);
		}

		bool IsExclusiveDatabase(string dbName)
		{
			Argument.NotNullOrEmpty(dbName, nameof(dbName));

			bool isExclusiveRefDb = RefDbTableNameResolver.IsExclusiveDatabase(CurrentDatabase, dbName); // Exclusive RefDb
			var isRepositoryDb = DbUserRepository.IsRepositoryDatabase(CurrentDatabase, dbName); // Repository
			var sdDb = string.Format("{0}{1}", CurrentDatabase, Db.SDDatabaseAffix);
			var isStorageDocsDb = dbName.StartsWith(sdDb, StringComparison.OrdinalIgnoreCase); // SD001

			return isExclusiveRefDb || isRepositoryDb || isStorageDocsDb;
		}

		#endregion

		#region Reference Databases

		void IPhysicalRefDbLocation.ClearRefDbNameBuffers()
		{
			refDbSetDictionary.TryRemove(fInitialDatabaseName, out _);
			exclusiveRefDbs = null;
			sharedRefDbs = null;
		}

		/// <summary>
		/// Returns reference database name based on refDbType and refCountryCode,
		/// </summary>
		/// <param name="refDbType">(Ent, Trf, Cmr)</param>
		/// <param name="refCountryCode">(AU, NZ, GB, etc.)</param>
		/// <returns>Reference Database Name</returns>
		string IPhysicalRefDbLocation.GetReferenceDatabaseName(RefDbTypeEnum refDbType, string refCountryCode)
		{
			if (refDbType == RefDbTypeEnum.Single)
			{
				return RefDbTableNameResolver.SingleRefDatabaseName;
			}
			var refDbDictionary = refDbSetDictionary.GetOrAdd(fInitialDatabaseName, () => new RefDbByTypeAndCountryDictionary());
			string result = refDbDictionary.GetReferenceDatabaseName(refDbType, refCountryCode);

			if (result == null)
			{
				result = GetRefDbNameFromSynonym(fInitialDatabaseName, refDbType, refCountryCode);
				refDbDictionary.SetReferenceDatabaseName(refDbType, refCountryCode, result);
			}

			return result;
		}

		string IPhysicalRefDbLocation.LoadRefDbNameFromSynonym(string mainDbName, RefDbTypeEnum refDbType,
			string refCountryCode)
		{
			return GetRefDbNameFromSynonym(mainDbName, refDbType, refCountryCode);
		}

		string GetRefDbNameFromSynonym(string mainDbName, RefDbTypeEnum refDbType, string refCountryCode)
		{
			Argument.NotNullOrEmpty(mainDbName, nameof(mainDbName));
			Argument.NotNullOrEmpty(refCountryCode, nameof(refCountryCode));

			string synonymPrefix = RefDbTableNameResolver.GetRefDbSynonymPrefix(refDbType, refCountryCode);
			string sqlText = string.Format(@"
				SELECT d.name
				FROM sys.databases d
				WHERE d.name = (
					SELECT TOP 1 substring(s.base_object_name, 2, charindex('].', s.base_object_name) - 2)
					FROM [{0}].sys.synonyms s
					WHERE s.name like '{1}%'
					AND s.base_object_name like '[[]%].%.[[]%]'
				) COLLATE {2};", // Sql queries do not need translating
				mainDbName, synonymPrefix, Db.DatabaseCollation);
			using (var cmd = Command(sqlText))
			{
				object objValue = cmd.ExecuteScalar();
				return objValue?.ToString();
			}
		}

		static readonly ConcurrentDictionary<string, RefDbByTypeAndCountryDictionary> refDbSetDictionary =
			new ConcurrentDictionary<string, RefDbByTypeAndCountryDictionary>();

		#endregion

		public bool DatabaseExists(string dbName)
		{
			Argument.NotNullOrEmpty(dbName, nameof(dbName));

			const string sql = "SELECT CONVERT(bit, CASE WHEN EXISTS (SELECT NULL FROM sys.databases WHERE name = @dbName) THEN 1 ELSE 0 END);";

			using (UseMasterDb())
			using (var cmd = Command(sql))
			{
				cmd.AddParameter("@dbName", SqlDbType.NVarChar, 128, dbName);

				return Convert.ToBoolean(cmd.ExecuteScalar(), CultureInfo.InvariantCulture);
			}
		}

		public bool IsCurrentDbInStandbyMode()
		{
			string sqlText = "SELECT databasepropertyex(db_name(), 'IsInStandBy')";
			using (var cmd = Command(sqlText))
			{
				object objResult = cmd.ExecuteScalar();
				return (objResult != null && objResult != DBNull.Value) && (Convert.ToInt32(objResult) == 1);
			}
		}

		public bool IsDbWriteable(string dbName)
		{
			Argument.NotNullOrEmpty(dbName, nameof(dbName));

			using (UseMasterDb())
			{
				return Exists("FROM sys.databases WHERE name = @dbName AND is_read_only = 0", (cmd) => cmd.AddParameter("@dbName", SqlDbType.NVarChar, 128, dbName));
			}
		}

		public bool IsDbWriteableAndOnline(string dbName)
		{
			Argument.NotNullOrEmpty(dbName, nameof(dbName));

			using (UseMasterDb())
			{
				return Exists("FROM sys.databases WHERE name = @dbName AND is_read_only = 0 AND state = 0", (cmd) => cmd.AddParameter("@dbName", SqlDbType.NVarChar, 128, dbName));
			}
		}

		public bool IsSecondaryReplicaDatabase(string dbName)
		{
			Argument.NotNullOrEmpty(dbName, nameof(dbName));

			return (
				IsAlwaysOnEnabled
				&& AlwaysOn.IsDbPartOfAlwaysOn(this, dbName)
				&& !AlwaysOn.IsDbOnPrimaryReplica(this, dbName)
			);
		}

		public void AlterDbWriteableState(string dbName, bool writeable)
		{
			Argument.NotNullOrEmpty(dbName, nameof(dbName));

			string sqlText = string.Format("ALTER DATABASE [{0}] SET {1} WITH ROLLBACK IMMEDIATE;", dbName,
				writeable ? "READ_WRITE" : "READ_ONLY");
			using (var cmd = Command(sqlText))
			{
				cmd.ExecuteNonQuery();
			}
		}

		public bool IsDbAutoCreateStats(string dbName)
		{
			Argument.NotNullOrEmpty(dbName, nameof(dbName));

			const string sql = "SELECT CONVERT(bit, CASE WHEN EXISTS(SELECT NULL FROM sys.databases WHERE name = @dbName AND is_auto_create_stats_on = 1) THEN 1 ELSE 0 END);";
			using (UseMasterDb())
			using (var cmd = Command(sql))
			{
				cmd.AddParameter("@dbName", SqlDbType.NVarChar, 128, dbName);
				var result = cmd.ExecuteScalar();
				return result != null && (bool)result;
			}
		}

		public void AlterDbAutoCreateStats(string dbName, bool isON)
		{
			var sql = string.Format("ALTER DATABASE [{0}] SET AUTO_CREATE_STATISTICS {1} WITH NO_WAIT;",
				dbName,
				(isON) ? "ON" : "OFF"
				);

			using (var cmd = Command(sql))
			{
				cmd.ExecuteNonQuery();
			}
		}

		public string DatabaseStateDescription(string dbName)
		{
			Argument.NotNullOrEmpty(dbName, nameof(dbName));

			using (UseMasterDb())
			using (var command = Command("SELECT state_desc FROM sys.databases WHERE name = @dbName"))
			{
				command.AddParameter("@dbName", SqlDbType.NVarChar, 128, dbName);
				var result = command.ExecuteScalar();
				return result?.ToString().ToUpper(CultureInfo.InvariantCulture);
			}
		}

		#endregion

		#region ServerName

		public string ServerName
		{
			get { return fServerName; }
		}

		/// <summary>
		/// Server name reported by the database.
		/// May be different to ServerName specified on Enterprise command line.
		/// </summary>
		public string ServerNameReportedByDatabase
		{
			get
			{
				if (serverNameReportedByDatabase == null)
				{
					using (var cmd = Command("SELECT SERVERPROPERTY('SERVERNAME')"))
					{
						var result = cmd.ExecuteScalar();
						if (result != null)
						{
							serverNameReportedByDatabase = result.ToString();
						}
						else
						{
							throw new InvalidOperationException("Could not get server name from database");
						}
					}
				}

				return serverNameReportedByDatabase;
			}
		}

		string serverNameReportedByDatabase;

		/// <summary>
		/// Application database server, as reported by the server.
		/// This WILL NEVER be "localhost", "(local)", "." or "127.0.0.1"
		/// </summary>
		public string ServerNameWithoutInstance
		{
			get
			{
				if (serverNameWithoutInstance == null)
				{
					serverNameWithoutInstance = GetServerNameWithoutInstance(ServerNameReportedByDatabase);
				}

				return serverNameWithoutInstance;
			}
		}

		string serverNameWithoutInstance;

		/// <summary>
		/// The name of the instance of this Server
		/// </summary>
		public string ServerInstanceName
		{
			get
			{
				if (serverInstanceName == null)
				{
					serverInstanceName = GetServerInstanceName(ServerNameReportedByDatabase);
				}

				return serverInstanceName;
			}
		}

		string serverInstanceName;

		internal static string GetServerInstanceName(string serverName)
		{
			Argument.NotNull(serverName, nameof(serverName));

			string result = string.Empty;
			if (serverName.Contains("\\"))
			{
				var splits = serverName.Split('\\');
				if (splits.Length >= 2)
				{
					result = splits[1] ?? string.Empty;
				}
			}

			return result;
		}

		internal static string GetServerNameWithoutInstance(string serverName)
		{
			Argument.NotNull(serverName, nameof(serverName));

			return serverName.Contains("\\") ? serverName.Split('\\')[0] : serverName;
		}

		public string ServerDomain
		{
			get
			{
				if (serverDomain == null)
				{
					try
					{
						string getServerDomainSql = @"
					DECLARE @domain varchar(255);
					EXEC xp_getnetname @domain output, 2;
					SELECT @domain;";

						using (var cmd = Command(getServerDomainSql))
						{
							object domainObj = cmd.ExecuteScalar();
							if (domainObj != null && domainObj != DBNull.Value)
							{
								serverDomain = domainObj.ToString();
								int domainEosIndex = serverDomain.IndexOf('\0');
								serverDomain = (domainEosIndex >= 0) ? serverDomain.Substring(0, domainEosIndex) : serverDomain;
							}
						}
					}
					catch (SqlException)
					{
						// The undocumented procedure [xp_getnetname] works in all of our internal servers.
						// If it fails for a given client, regard it as not an internal server.
					}
				}

				return serverDomain;
			}
		}

		string serverDomain;

		[Conditional("DEBUG")]
		static void CheckServerNameIsNotLocalhost(string serverName)
		{
			Argument.NotNullOrEmpty(serverName, nameof(serverName));

			var serverNameWithoutInstance = GetServerNameWithoutInstance(serverName);
			if (new[] { "localhost", "(local)", ".", "127.0.0.1" }.Contains(serverNameWithoutInstance.ToLowerInvariant()))
			{
				throw new InvalidOperationException(string.Format("You have tried to establish a DB Connection using a generic machine name. ({0}) Please use your real machine name in place of this. ({1})", serverNameWithoutInstance, System.Environment.MachineName));// Exception message only seen by developers
			}
		}

		#endregion

		#region Deadlock Priority

		/// <summary>
		/// Sets the DEADLOCK_PRIORITY variable of the current session
		/// </summary>
		public void SetDeadlockPriority(DeadlockPriority priority)
		{
			SetDeadlockPriorityCore((int)priority);
		}

		/// <summary>
		/// Sets the DEADLOCK_PRIORITY variable of the current session
		/// </summary>
		/// <param name="priority">Must be between -10 and 10</param>
		public void SetDeadlockPriority(int priority)
		{
			if (!(priority >= (int)Data.DeadlockPriority.Min && priority <= (int)Data.DeadlockPriority.Max))
			{
				throw new ArgumentException(FormattableString.Invariant($"Deadlock priority must be between {(int)Data.DeadlockPriority.Min} and {(int)Data.DeadlockPriority.Max}"), nameof(priority));
			}

			SetDeadlockPriorityCore(priority);
		}

		void SetDeadlockPriorityCore(int priority)
		{
			var sql = "SET DEADLOCK_PRIORITY " + priority.ToString(CultureInfo.InvariantCulture);

			using (var command = Command(sql))
			{
				command.ExecuteNonQuery();
			}
			DeadlockPriority = priority;
		}

		public int DeadlockPriority { get; private set; }

		#endregion

		#endregion

		#region Implementation

		#region Attributes

		protected const int fInitialDefaultCmdTimeout = 300;

		protected readonly string fServerName;
		protected readonly string fInitialDatabaseName;
		protected readonly DBCredentials fDBCredentials;

		readonly IErrorReporter errorReporter;
		protected IErrorReporter ErrorReporter => errorReporter ?? CargoWise.Common.ErrorReporter.InstanceInternal; // Ensure ErrorReporter.InstanceInternal is lazily loaded on demand

		protected bool hasConnectedBefore;
		bool isConnecting;

		readonly Thread instantiationThread = Thread.CurrentThread;

		IDbConnection internalConnectionUnsafe;

		protected IDbConnection InternalConnectionUnsafe
		{
			get
			{
				if (UndisposedLocksPresentAfterReconnect)
				{
					EnsureNoUndisposedLocks();
				}
				ThreadSentry.EnsureCurrentThreadIsOwner();
				return internalConnectionUnsafe;
			}
		}

		#endregion

		#region Internal Connection

		IDbConnection IDbConnectionInternals.InternalDbConnection
		{
			get { return InternalConnection; }
		}

		IDbTransaction IDbConnectionInternals.InternalDbTransaction
		{
			get { return internalTransaction; }
		}

		IDbConnection InternalConnection
		{
			get
			{
				if ((!hasConnectedBefore || InternalConnectionUnsafe is null) && !isConnecting)
				{
					try
					{
						isConnecting = true;
						OpenConnectionIfClosed();
						hasConnectedBefore = true;
					}
					/*
					 * The finally block only executes once the 'when'
					 * conditions on catch statements are executed further up the stack.
					 *
					 * Hence if 'finally' is used instead of the catch block here
					 * and 'when' condition further up the stack calls InternalConnection,
					 * it will fail to reopen connection because isConnecting
					 * is still set to true as finally block would not have executed yet
					*/
					catch
					{
						isConnecting = false;
						throw;
					}

					isConnecting = false;
				}

				return InternalConnectionUnsafe;
			}
		}

		#region Internal HTTP Client Connection

		Guid IDbConnectionInternals.HttpTransactionId
		{
			get
			{
				var httpConnection = InternalConnection as HttpConnection;
				return (httpConnection != null)
					? httpConnection.TransactionId
					: Guid.Empty;
			}
		}

		HttpConnection IDbConnectionInternals.HttpConnection
		{
			get { return InternalConnection as HttpConnection; }
		}

		#endregion

		#region SQL Server Connection

		System.Data.Common.DbConnection IDbConnectionInternals.SqlConnection
		{
			get { return InternalConnection as System.Data.Common.DbConnection; }
		}

		System.Data.Common.DbConnection IDbConnectionInternals.ADOConnection
		{
			get { return InternalConnection as System.Data.Common.DbConnection; }
		}

		System.Data.Common.DbTransaction IDbConnectionInternals.ADOTransaction
		{
			get { return internalTransaction as System.Data.Common.DbTransaction; }
		}

		#endregion

		public ISqlBulkCopy GetSqlBulkCopy(SqlBulkCopyOptions options = SqlBulkCopyOptions.Default, IDbTransaction transaction = null)
		{
			if (InternalConnection is HttpConnection httpConnection)
			{
				return new HttpBulkCopy(httpConnection);
			}
			else
			{
				return new SqlServerBulkCopy(InternalConnection as SqlConnection, options, (SqlTransaction)transaction);
			}
		}

		protected virtual IConnectionPooling ConnectionPoolingValue
		{
			get
			{
				return DbEnv.Instance.ConnectionPooling;
			}
		}

		protected virtual int ConnectionStringTimeoutValue
		{
			get
			{
				return DbEnv.Instance.ConnectionTimeout;
			}
		}

		#region Open and Close Connection

		/// <summary>
		/// Opens DB Connection, handling possible failures.
		/// </summary>
		protected void OpenConnectionIfClosed(bool useErrorHandler = true)
		{
			if (State == ConnectionState.Closed || State == ConnectionState.Broken)
			{
				OpenConnection(useErrorHandler);
				RunTasksAfterOpenConnection();
			}
		}

		/// <summary>
		/// ONLY to be called by OpenConnectionIfClosed()
		/// </summary>
		void OpenConnection(bool useErrorHandler)
		{
			try
			{
				OpenConnectionWithSplashInfo();
			}
			catch (SqlException e) when (useErrorHandler)
			{
				if (HandleError(e))
				{
					return;
				}

				// Throws exception if not handled
				throw;
			}
		}

		protected virtual void OpenConnectionWithSplashInfo()
		{
			using (GetConnectingSplashForm())
			{
				OpenConnectionCore();
			}
		}

		IDisposable GetConnectingSplashForm()
		{
#if DEBUG
			if (!ShowSplashFormForTests && NUnit.Framework.TestingState.IsRunningTests)
			{
				return null;
			}
#endif
			return InternalConnectionUnsafe is HttpConnection httpConnection ?
				httpConnection.NewConnectingSplashFormManager() :
				NewConnectingSplashFormManager();
		}
#if DEBUG
		[ThreadSafe]
		public static bool ShowSplashFormForTests;
#endif
		protected void OpenConnectionCore()
		{
			ResetConnectionCache();
			if (InternalConnectionUnsafe is null || internalConnectionUnsafe is System.Data.Common.DbConnection)
			{
				int i = 1;
				internalConnectionUnsafe?.Close();

				while (true)
				{
					try
					{
						if (internalConnectionUnsafe is null)
						{
							internalConnectionUnsafe = OpenNewDbConnection();

							// Tracking first open directly, then by StateChange
							DatabaseConnectionEventTracker.Instance.AddConnectionEvent(internalConnectionUnsafe, ConnectionStatesInfo);

							if (internalConnectionUnsafe is System.Data.Common.DbConnection internalConnectionDbConnection)
							{
								internalConnectionDbConnection.StateChange += StateChange;
								internalConnectionDbConnection.Disposed += DbConnection_Disposed;
							}
						}
						else
						{
							internalConnectionUnsafe.Open();
						}
						break;
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						if (i++ >= OpenRetries)
						{
							internalConnectionUnsafe?.Close();
							throw;
						}
						Thread.Sleep(1000);
					}
				}
			}
			var deadlockPriority = DbEnv.Instance.DeadlockPriority;
			if (deadlockPriority != (int)Data.DeadlockPriority.Medium)
			{
				SetDeadlockPriority(deadlockPriority);
			}
		}

		void ResetConnectionCache()
		{
			serverNameReportedByDatabase = null;
		}

		int openRetries = 3;

		public int OpenRetries
		{
			get
			{
				return openRetries;
			}
#if DEBUG
			set
			{
				openRetries = value;
			}
#endif
		}

		void StateChange(object sender, StateChangeEventArgs e)
		{
			if (sender is IDbConnection connection)
			{
				DatabaseConnectionEventTracker.Instance.AddConnectionEvent(connection, ConnectionStatesInfo);
			}
		}

		string ConnectionStatesInfo => $" [UpgradeCheck Disabled={IsUpgradeCheckDisabled}] [SchemaVersionCheck Disabled={Db.IsSchemaVersionCheckDisabled}] [DatabaseName Initialized={Db.DatabaseNameIsInitialized}] [Main Database={(Db.DatabaseNameIsInitialized ? Db.DatabaseName : null)}]";

#if DEBUG
		public
#else
		protected
#endif
		void CloseConnection()
		{
			if (InternalConnectionUnsafe is SqlConnection)
			{
				if ((InternalConnectionUnsafe as SqlConnection).State == ConnectionState.Closed)
				{
					return;
				}
			}
			InternalConnectionUnsafe?.Close();
			if (IsInTimeoutableConnections)
			{
				lock (hashLock)
				{
					timeoutableConnections.Remove(reference);
				}
				reference = null;
			}
		}

		protected virtual IDisposable NewConnectingSplashFormManager()
		{
			return null;
		}

		#endregion

		#region RunTasksAfterOpenConnection

		protected virtual void RunTasksAfterOpenConnection()
		{
			ResetAppTransaction();
			ResetCachedSessionProperties();
			CheckForUpgradeLoadSuspendAuditTriggersAndSetLockTimeout();
			EnsureNoUndisposedLocksOnReconnect();
			RefreshLastActivity();
			SuspendAuditTriggersAfterOpenConnection();
		}

		void SuspendAuditTriggersAfterOpenConnection()
		{
			auditTriggerSuspensionStack.Clear();
			if (suspendAuditTriggersRegistry)
			{
				_ = SuspendAuditTriggers(); // No need to dispose of the return value of this call, because if the connection is gets closed, session context is reset automatically.
			}
		}

		/// <summary>
		/// Check for a completed upgrade or an upgrade in progress.
		/// Only does the check if Db.IsSchemaVersionCheckDisabled is false and IsUpgradeCheckDisabled is false.
		/// At the same time, load SuspendAuditTriggers config from registry if set, and set or reset cached flag accordingly.
		/// Also sets lock timeout.
		/// Done with minimal db hits for speed - two if the lock timeout registry hasn't changed, otherwise three.
		///
		/// Throws <see cref="DatabaseUpgradeInProgressException"/> if an upgrade is in progress.
		/// Note, if regular logins are disabled during the upgrade this code will only be reached if this is an AdminConnection.
		/// 
		/// Throws <see cref="DatabaseUpgradedException"/> if the database has been upgraded, so the currently running software is out of date.
		/// <summary>
		void CheckForUpgradeLoadSuspendAuditTriggersAndSetLockTimeout()
		{
			var currentLockTimeout = LockTimeout.CachedFromRegistry;
			if (IsUpgradeCheckDisabled || Db.IsSchemaVersionCheckDisabled || !Db.DatabaseNameIsInitialized || CurrentDatabase != Db.DatabaseName)
			{
				// Every connection gets at least the default lock timeout.
				if (currentLockTimeout != LockTimeout.SqlDefault)
				{
					SetLockTimeout(currentLockTimeout);
				}
				return;
			}

			if (DbLockout.HasLockout(this))
			{
				// Note, we throw DatabaseUpgradeInProgress even if the lockout is for a Purge and not an Upgrade.
				// The exception handler is responsible for sorting it all out.
				CloseConnection();
				throw new DatabaseUpgradeInProgressException();
			}

			SetLockTimeout(currentLockTimeout);

			var dictionary = DataUtils.ReadBulkRegistry(this,
				DbRegistry.DatabaseMajorSchemaVersionName,
				DbRegistry.DatabaseMinorSchemaVersionName,
				DbRegistry.DatabaseMajorScriptVersionName,
				DbRegistry.DatabaseMinorScriptVersionName,
				DbRegistry.DatabaseMajorTransformationVersionName,
				DbRegistry.DatabaseMinorTransformationVersionName,
				DbRegistry.LockTimeoutName,
				DbRegistry.SuspendAuditTriggersName);

			var dbSchemaVersion = GetSchemaVersion(dictionary);
			var dbScriptVersion = GetScriptVersion(dictionary);
			var dbTransformationVersion = GetTransformationVersion(dictionary);
			CheckForUpgrade(dbSchemaVersion, dbScriptVersion, dbTransformationVersion);

			if (dictionary.TryGetValue(DbRegistry.LockTimeoutName, out var registryLockTimeout))
			{
				// Global static int update - thread safe since atomic.
				LockTimeout.CachedFromRegistry = registryLockTimeout;
				if (currentLockTimeout != registryLockTimeout)
				{
					SetLockTimeout(registryLockTimeout);
				}
			}

			if (dictionary.TryGetValue(DbRegistry.SuspendAuditTriggersName, out var value))
			{
				suspendAuditTriggersRegistry = value == 1;
			}
			else
			{
				suspendAuditTriggersRegistry = DbRegistry.SuspendAuditTriggersDefaultValue;
			}
		}

		static VersionLabel GetSchemaVersion(Dictionary<string, int> dictionary)
			=> GetVersionLabel(dictionary,
				DbRegistry.DatabaseMajorSchemaVersionName,
				DbRegistry.DatabaseMinorSchemaVersionName);

		static VersionLabel GetScriptVersion(Dictionary<string, int> dictionary)
			=> GetVersionLabel(dictionary,
				DbRegistry.DatabaseMajorScriptVersionName,
				DbRegistry.DatabaseMinorScriptVersionName);

		static VersionLabel GetTransformationVersion(Dictionary<string, int> dictionary)
			=> GetVersionLabel(dictionary,
				DbRegistry.DatabaseMajorTransformationVersionName,
				DbRegistry.DatabaseMinorTransformationVersionName);

		static VersionLabel GetVersionLabel(Dictionary<string, int> dictionary, string majorName, string minorName)
			=> new VersionLabel(SafeGet(dictionary, majorName), SafeGet(dictionary, minorName));

		static int SafeGet(Dictionary<string, int> dictionary, string key) => dictionary.TryGetValue(key, out var value) ? value : 0;

		/// <summary>
		/// Check for a completed upgrade by comparing the given versions from the db with the current software version.
		/// Throws <see cref="DatabaseUpgradedException"/> if the database has been upgraded, so the currently running software is out of date.
		/// </summary>
		void CheckForUpgrade(VersionLabel dbSchemaVersion, VersionLabel dbScriptVersion, VersionLabel dbTransformationVersion)
		{
			var diffVersionsBinaryVsDatabase = CompareDbVersions(dbSchemaVersion, dbScriptVersion, dbTransformationVersion);
			((IDbUpgradeSupport)Db.Instance).SetDatabaseHasBeenUpgraded(diffVersionsBinaryVsDatabase);

			if (diffVersionsBinaryVsDatabase != 0)
			{
				if (RecordDatabaseUpgradedExceptionAndContinue(diffVersionsBinaryVsDatabase))
				{
					ShouldStopDbConnectionCleaner = true;
					CloseConnection();
					throw new DatabaseUpgradedException(diffVersionsBinaryVsDatabase > 0);
				}
			}
		}

		/// <summary>
		/// Flag to disable the upgrade check when opening this connection.
		/// The upgrade check includes a check for an upgrade in progess and a check for an upgrade completed.
		/// To check for only an upgrade in progress, set this flag, open the connection and then call DbLockout.HasLockout.
		/// </summary>
		public bool IsUpgradeCheckDisabled { get; set; }

		bool RecordDatabaseUpgradedExceptionAndContinue(int changed)
		{
			if (DatabaseUpgradedExceptionHasBeenThrown)
			{
				var reportConnection = Db.Connection;
				using (reportConnection.IsUpgradeCheckDisabled ? null : new DisposableAction(
					() => reportConnection.IsUpgradeCheckDisabled = true,
					() => reportConnection.IsUpgradeCheckDisabled = false))
				{
					ErrorReporter.ReportOnce(null, string.Empty, new DatabaseUpgradeExceptionCaughtException(previousDatabaseUpgradedStackTrace, ApplicationDispatcher.ThreadException));
				}
				DbEnv.Instance.ConnectionGuiPlugin.HandleDatabaseUpgradeException(new DatabaseUpgradedException(changed > 0)); // restart CargoWiseOne
				return false;
			}

			DatabaseUpgradedExceptionHasBeenThrown = true;
			return true;
		}
		StackTrace previousDatabaseUpgradedStackTrace;

		public bool ShouldStopDbConnectionCleaner;

		public bool DatabaseUpgradedExceptionHasBeenThrown
		{
			get { return previousDatabaseUpgradedStackTrace != null; }
			set { previousDatabaseUpgradedStackTrace = value ? new StackTrace() : null; }
		}

		bool IDbReconnectionHandling.HasDbSchemaOrScriptOrTransformationVersionChanged()
		{
			var dictionary = DataUtils.ReadBulkRegistry(this,
				DbRegistry.DatabaseMajorSchemaVersionName,
				DbRegistry.DatabaseMinorSchemaVersionName,
				DbRegistry.DatabaseMajorScriptVersionName,
				DbRegistry.DatabaseMinorScriptVersionName,
				DbRegistry.DatabaseMajorTransformationVersionName,
				DbRegistry.DatabaseMinorTransformationVersionName);

			var dbSchemaVersion = GetSchemaVersion(dictionary);
			var dbScriptVersion = GetScriptVersion(dictionary);
			var dbTransformationVersion = GetTransformationVersion(dictionary);

			var diff = CompareDbVersions(dbSchemaVersion, dbScriptVersion, dbTransformationVersion);
			return diff != 0;
		}

		protected virtual int CompareDbVersions(VersionLabel dbSchemaVersion, VersionLabel dbScriptVersion, VersionLabel dbTransformationVersion)
		{
			var versions = GlobalServiceProvider.Instance.GetRequiredService<IDatabaseAspectVersions>();

			var diff = versions.SchemaVersion.CompareTo(dbSchemaVersion);

			if (diff != 0)
			{
				return diff;
			}

			diff = versions.ScriptVersion.CompareTo(dbScriptVersion);

			if (diff != 0)
			{
				return diff;
			}

			diff = versions.TransformationVersion.CompareTo(dbTransformationVersion);

			if (diff != 0)
			{
				return diff;
			}

			return 0;
		}

		#endregion

		#endregion

		#endregion

		#region SqlApplicationLock

		List<WeakReference<SqlApplicationLock>> locks;
		readonly IThreadSentry threadSentry;

		public bool UndisposedLocksPresentAfterReconnect { get; private set; }

		List<WeakReference<SqlApplicationLock>> Locks
		{
			get
			{
				return locks ?? (locks = new List<WeakReference<SqlApplicationLock>>());
			}
		}

		bool IsLockUndisposed(WeakReference<SqlApplicationLock> weakRef, out SqlApplicationLock sqlLock)
		{
			Argument.NotNull(weakRef, nameof(weakRef)); // Suggested By ReviewBot
			return weakRef.TryGetTarget(out sqlLock) && sqlLock != null && !sqlLock.IsDisposed;
		}

		public bool HasSqlLocks => UndisposedSqlLocks.Any();

		public IEnumerable<SqlApplicationLock> UndisposedSqlLocks
		{
			get
			{
				if (locks != null)
				{
					foreach (var weakRef in locks)
					{
						if (IsLockUndisposed(weakRef, out SqlApplicationLock sqlLock))
						{
							yield return sqlLock;
						}
					}
				}
			}
		}

		/// <summary>
		/// Try to get a sql lock with the given key on this connection.
		///
		/// NOTE:
		///		Just because this method returns true does not mean that you hold the lock forever
		///		You can lose the lock under other circumstances, for example, if the connection is reset
		///
		///		If you have an undisposed lock and the connection resets, you will get an SqlLockLostException as it reconnects.
		///
		///		For this reason you should probably use a transaction as well
		///
		/// NOTE:
		///		Try to reduce the number of sql locks you use. Each lock means many db hits
		///
		/// NOTE:
		///		Consider using the RunLocked method to automatically retry the lock+operation if disconnected.
		/// </summary>
		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", Justification = "Out parameter required by design")]
		public bool TryGetLock(string key, out SqlApplicationLock appLock, string dbName = null, SqlApplicationLockMode lockMode = SqlApplicationLockMode.Exclusive)
		{
			return TryGetLock(key, TimeSpan.Zero, out appLock, dbName, lockMode);
		}

		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", Justification = "Out parameter required by design")]
		public bool TryGetLock(string key, TimeSpan sqlLockTimeout, out SqlApplicationLock lockResult, string dbName = null, SqlApplicationLockMode lockMode = SqlApplicationLockMode.Exclusive)
		{
			if (!(!string.IsNullOrWhiteSpace(key) && key.Length <= 255))
			{
				throw new ArgumentException("Invalid argument.", nameof(key));
			}

			CleanUpDisposedSqlLocks();

			if (string.IsNullOrWhiteSpace(dbName))
			{
				dbName = Db.Connection.CurrentDatabase;
			}
			lockResult = SqlApplicationLock.Acquire(this, dbName, key, sqlLockTimeout, lockMode);

			if (lockResult != null && lockResult.WasAcquiredOnLastCheck)
			{
				Locks.Add(new WeakReference<SqlApplicationLock>(lockResult));
				return true;
			}

			return false;
		}

		public SqlApplicationLock AddLock(string key, string dbName, ISqlApplicationLockStrategy strategy)
		{
			Argument.NotNull(strategy, nameof(strategy));
			Argument.NotNullOrEmpty(dbName, nameof(dbName));
			if (!(!string.IsNullOrWhiteSpace(key) && key.Length <= 255))
			{
				throw new ArgumentException("Invalid argument.", nameof(key));
			}

			var appLock = new SqlApplicationLock(key, this, dbName, strategy);
			Locks.Add(new WeakReference<SqlApplicationLock>(appLock));
			return appLock;
		}

		/// <summary>
		/// It catches undisposed lock errors (SqlLockLostException) and retries up to the specified limit (max_tries).
		/// If unable to complete the process action (process) after all attempts, it returns an error code (LockedProcessResult.Error).
		/// </summary>
		public LockedProcessResult RunLocked(string key, Action<bool> process, Func<bool> isFinished = null, int max_tries = 3, string dbName = null, SqlApplicationLockMode lockMode = SqlApplicationLockMode.Exclusive)
		{
			if (!(!string.IsNullOrWhiteSpace(key) && key.Length <= 255))
			{
				throw new ArgumentException("Invalid argument.", nameof(key));
			}

			Argument.NotNull(process, nameof(process));

			for (int tries = 0; tries < max_tries; tries++)
			{
				if (TryGetLock(key, out SqlApplicationLock mutex, dbName: dbName, lockMode))
				{
					try
					{
						if (isFinished != null && isFinished())
						{
							return LockedProcessResult.AlreadyBeingProcessed;
						}

						process(tries == 0);
						return LockedProcessResult.Completed;
					}
					catch (Exception ex) when (ex.Find<SqlLockLostException>() != null) { }
					finally { mutex.Dispose(); }
				}
				else
				{
					return LockedProcessResult.AlreadyBeingProcessed;
				}
			}

			return LockedProcessResult.Error;
		}

		const int SqlLockThresholds = 100;

		void CleanUpDisposedSqlLocks()
		{
			if (Locks.Count >= SqlLockThresholds)
			{
				Locks.RemoveAll(DisposedSqlLockMatch);
			}
		}

		/// <summary>
		/// Check if an applock is currently acquired in memory.
		/// </summary>
		/// <param name="key">The applock key.</param>
		/// <returns></returns>
		public bool HasAquiredLock(string key)
		{
			Argument.NotNull(key, nameof(key));
			foreach (var l in Locks)
			{
				if (l != null && l.TryGetTarget(out SqlApplicationLock appLock) && appLock != null && !appLock.IsDisposed && key.Equals(appLock.Key, StringComparison.OrdinalIgnoreCase))
				{
					return appLock.IsHoldingLock();
				}
			}

			return false;
		}

		bool DisposedSqlLockMatch(WeakReference<SqlApplicationLock> weakRef)
		{
			Argument.NotNull(weakRef, nameof(weakRef));
			return !IsLockUndisposed(weakRef, out SqlApplicationLock @lock);
		}

		void EnsureNoUndisposedLocksOnReconnect()
		{
			try
			{
				EnsureNoUndisposedLocks();
				UndisposedLocksPresentAfterReconnect = false;
			}
			catch (SqlLockLostException)
			{
				UndisposedLocksPresentAfterReconnect = true;
				throw;
			}
		}

#if DEBUG
		public
#endif
		void EnsureNoUndisposedLocks()
		{
			var undisposedLocks = UndisposedSqlLocks.ToList();
			if (undisposedLocks.Any())
			{
				var message = new StringBuilder("A db reconnect was attempted while undisposed SqlLocks existed. Details below:\r\n", 500);
				foreach (var sqlApplicationLock in undisposedLocks)
				{
					message.AppendFormat(CultureInfo.InvariantCulture, "{0}\r\n", sqlApplicationLock?.ToString());
				}
				throw new SqlLockLostException(message.ToString(), undisposedLocks.Select(@lock => @lock.Key).ToArray()); // Exception message only seen by developers
			}
			else
			{
				UndisposedLocksPresentAfterReconnect = false;
			}
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				CloseConnection();
				previousDatabaseUpgradedStackTrace = null;
			}
		}
		#endregion

		#region IDbReconnectionHandling Members

		public int AppTransactionCount
		{
			get { return appTransactionCount; }
		}
		int appTransactionCount;

		void ModifyAppTransactionCount(int modifiedCount)
		{
			var newAppTransactionCount = appTransactionCount + modifiedCount;
			if (enableRecordAppTransactionCountChanged)
			{
				if (modifiedCount > 0)
				{
					appTransactionCountAddedStackTrace.Add((newAppTransactionCount, new StackTrace()));
				}
				else
				{
					appTransactionCountReducedStackTrace.Add((newAppTransactionCount, new StackTrace()));
				}
			}
			appTransactionCount = newAppTransactionCount;
		}

		#region AppTransactionCountLog

		public void EnableAppTransactionCountLog()
		{
			enableRecordAppTransactionCountChanged = true;
			appTransactionCountAddedStackTrace = new List<(int, StackTrace)>();
			appTransactionCountReducedStackTrace = new List<(int, StackTrace)>();
		}

		public void DisableAppTransactionCountLog()
		{
			enableRecordAppTransactionCountChanged = false;
			appTransactionCountAddedStackTrace = null;
			appTransactionCountReducedStackTrace = null;
		}

		bool enableRecordAppTransactionCountChanged;
		List<(int currentTransCount, StackTrace stackTrace)> appTransactionCountAddedStackTrace;
		List<(int currentTransCount, StackTrace stackTrace)> appTransactionCountReducedStackTrace;

		public string GetAppTransactionCountChangedLog()
		{
			if (enableRecordAppTransactionCountChanged)
			{
				var addedStackTraces = appTransactionCountAddedStackTrace.Aggregate("Added Stacktraces:\r\n", (tempResult, stacktrace) => tempResult + GetAppTransactionCountChangedStackTraceInfo(stacktrace.currentTransCount, stacktrace.stackTrace));
				var reducedStackTraces = appTransactionCountReducedStackTrace.Aggregate("Reduced Stacktraces:\r\n", (tempResult, stacktrace) => tempResult + GetAppTransactionCountChangedStackTraceInfo(stacktrace.currentTransCount, stacktrace.stackTrace));

				return $"{addedStackTraces}\r\n{reducedStackTraces}";
			}

			return null;
		}

		string GetAppTransactionCountChangedStackTraceInfo(int count, StackTrace stackTrace) => $"AppTransactionCount: {count} - StackTrace: {stackTrace}";

		#endregion

		ConnectionState IDbReconnectionHandling.State
		{
			get { return State; }
		}

		bool IDbReconnectionHandling.IsConnecting
		{
			get { return isConnecting; }
		}

		string IDbReconnectionHandling.ImpersonatedLogin => ImpersonatedLogin;

		string IDbReconnectionHandling.LoginName
		{
			get { return UserLogin; }
		}

		void IDbReconnectionHandling.CloseAndReopenConnection()
		{
			if (isConnecting)
			{
				throw new CloseReopenWhileConnectingException();
			}

			CloseConnection();

			// We are sure we will open connection next since we just closed it above.
			isConnecting = true;

			try
			{
				OpenConnectionIfClosed();
				isConnecting = false;
			}
			/*
			 * The finally block only executes once the 'when'
			 * conditions on catch statements are executed further up the stack.
			 *
			 * Hence if 'finally' is used instead of the catch block here
			 * and 'when' condition further up the stack calls InternalConnection,
			 * it will fail to reopen connection because isConnecting
			 * is still set to true as finally block would not have executed yet
			*/
			catch
			{
				isConnecting = false;
				throw;
			}
		}

		#endregion

		#region IDbConnected Members

		DbConnection IDbConnected.Connection
		{
			get { return this; }
		}

		#endregion

		#region IOpenConnectionErrorHandler Members

		public virtual bool HandleError(System.Data.Common.DbException sqlException)
		{
			var errorType = new DbErrorMatch(sqlException).ExceptionType;

#if DEBUG
			if (FixDatabaseLeftInSnapshotRestoringState(errorType))
			{
				return true;
			}
#endif

			if (DbLockout.IsUpgradeLockoutError(sqlException))
			{
				ShouldStopDbConnectionCleaner = true;
				throw new DatabaseUpgradeInProgressException();
			}

			if (DbErrorMatch.IsDbLoginError(errorType))
			{
				OpenConnectionIfClosed(useErrorHandler: false);
				return true;
			}

			return false;
		}

#if DEBUG
		bool FixDatabaseLeftInSnapshotRestoringState(DbErrorType errorType)
		{
			if (errorType == DbErrorType.CannotOpenDbRequestedInLogin)
			{
				using (var adminConnection = Db.NewAdminConnection(Db.SqlMasterDb))
				{
					string dbState;
					using (var cmd = adminConnection.Command("select state_desc from sys.databases where name = @dbName"))
					{
						cmd.AddParameter("dbName", SqlDbType.VarChar, fInitialDatabaseName);
						dbState = cmd.ExecuteScalar() as string;
					}
					if (!string.IsNullOrEmpty(dbState) && dbState.Equals("RESTORING", StringComparison.OrdinalIgnoreCase))
					{
						var snapshotName = fInitialDatabaseName + "-SS";
						bool snapshotExists;
						using (var cmd = adminConnection.Command("select count(*) from sys.databases where name = @snapshotName"))
						{
							cmd.AddParameter("snapshotName", SqlDbType.VarChar, snapshotName);
							snapshotExists = (int)cmd.ExecuteScalar() > 0;
						}
						if (snapshotExists)
						{
							using (var cmd = adminConnection.Command("RESTORE DATABASE [" + fInitialDatabaseName + "] FROM database_snapshot = '" + snapshotName + "'; DROP DATABASE [" + snapshotName + "];"))
							{
								cmd.ExecuteNonQuery();
							}
							OpenConnectionIfClosed();
							return true;
						}
					}
				}
			}
			return false;
		}
#endif

		#endregion

		#region SuspendAuditTriggers

		public IDisposable SuspendAuditTriggers() => AuditTriggerSuspension.PushNew(this, "1", preventOverride: false);
		public IDisposable EnableAllAuditTriggers() => AuditTriggerSuspension.PushNew(this, "NONE", preventOverride: true);

		readonly Stack<AuditTriggerSuspension> auditTriggerSuspensionStack = new Stack<AuditTriggerSuspension>();

		class AuditTriggerSuspension : IDisposable
		{
			AuditTriggerSuspension(DbConnection connection, string state, bool preventOverride)
			{
				this.connection = connection;
				this.state = state;
				this.preventOverride = preventOverride;
			}

			public static AuditTriggerSuspension PushNew(DbConnection connection, string state, bool preventOverride)
			{
				if (state == null)
				{
					throw new ArgumentNullException(nameof(state));
				}

				var suspender = new AuditTriggerSuspension(connection, state, preventOverride);
				var stack = connection.auditTriggerSuspensionStack;

				try
				{
					var peek = Peek(stack);
					if (peek?.preventOverride == true && peek.state != state)
					{
						throw new InvalidOperationException("Cannot override the suspension state of audit triggers.");
					}

					if (peek?.state != state)
					{
						ExecuteSetSessionContext(connection, state);
					}

					stack.Push(suspender);
				}
				catch (Exception ex) when (ex is InvalidOperationException && ex.Source == "System.Data")
				{
					if (connection.State != ConnectionState.Open)
					{
						stack.Clear();
					}
				}

				return suspender;
			}

			public void Dispose()
			{
				var stack = connection.auditTriggerSuspensionStack;
				if (!stack.Contains(this))
				{
					return;
				}

				var prevState = PeekState(stack);

				while (!ReferenceEquals(this, stack.Pop()))
				{
					// Keep popping
				}

				var newStatus = PeekState(stack);
				if (newStatus != prevState)
				{
					ExecuteSetSessionContext(connection, newStatus);
				}
			}

			static void ExecuteSetSessionContext(DbConnection connection, string state)
			{
				if (connection.State != ConnectionState.Open)
				{
					if (state == null)
					{
						// there's no point in opening the connection and setting the session context to null
						return;
					}

					_ = connection.InternalConnection;
				}

				using var command = connection.internalConnectionUnsafe?.CreateCommand();
				if (command == null)
				{
					return;
				}

				command.CommandType = CommandType.StoredProcedure;
				command.CommandText = "sys.sp_set_session_context";
				command.Transaction = ((IDbConnectionInternals)connection).InternalDbTransaction;

				AddParameter("@Key", DbType.String, 128, TriggerKey);
				AddParameter("@Value", DbType.Object, 8016, state ?? (object)DBNull.Value);

				_ = command.ExecuteNonQuery();

				void AddParameter(string name, DbType type, int size, object value)
				{
					var param = command.CreateParameter();
					param.ParameterName = name;
					param.DbType = type;
					param.Size = size;
					param.Value = value;

					_ = command.Parameters.Add(param);
				}
			}

			static string PeekState(Stack<AuditTriggerSuspension> stack) => Peek(stack)?.state;
			static AuditTriggerSuspension Peek(Stack<AuditTriggerSuspension> stack) => stack.Count == 0 ? null : stack.Peek();

			readonly string state;
			readonly DbConnection connection;
			readonly bool preventOverride;
		}

		[ThreadSafe]
		static bool suspendAuditTriggersRegistry = DbRegistry.SuspendAuditTriggersDefaultValue;

		internal const string TriggerKey = "Suspend_System_Audit_Columns_Guard";

		#endregion

		IDbCommand IDbConnectionWithSettings.CreateCommand(string sqlText)
		{
			var transaction = internalTransaction;
			var command = InternalConnection.CreateCommand();

			try
			{
				command.CommandTimeout = DefaultCommandTimeOutInSeconds;

				if (transaction != null)
				{
					command.Transaction = transaction;
				}

				if (!string.IsNullOrEmpty(sqlText))
				{
					command.CommandText = sqlText;
				}

				return command;
			}
			catch
			{
				command.Dispose();
				throw;
			}
		}

		IDbConnection IDbConnectionWithSettings.DbConnection => InternalConnection;
		IDbTransaction IDbConnectionWithSettings.Transaction => internalTransaction;

		sealed class TransactionManager : BaseTransactionManager<DbConnection>
		{
			public TransactionManager(DbConnection owner, Action rollbackAction = null) : base(owner, rollbackAction)
			{
				Argument.NotNull(owner, nameof(owner));
			}

			protected override void Commit()
			{
				owner.CommitTransaction();
			}

			protected override void Rollback()
			{
				try
				{
					owner.RollbackTransaction();
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
				}
			}
		}

		sealed class DelayedTransactionManager : IDelayedTransactionManager
		{
			readonly DbConnection dbConnection;
			readonly ITransactionManager transactionManager;
			readonly AggregateLockManager aggregateLockManager;

			int nestingDepth = 1;
			StackTrace rolledBackBy;

			public AggregateLockManager AggregateLockManager => aggregateLockManager;
			public bool IsDisposed { get; private set; }
			public bool IsRollingback => rolledBackBy != null || dbConnection.MustRollBack;
			public bool IsFinished { get; private set; }
			public bool IsFinishedOrDisposed => IsFinished || IsDisposed;

			public DelayedTransactionManager(
				DbConnection owner,
				AggregateLockManager aggregateLockManager,
				ITransactionManager transactionManager)
			{
				this.dbConnection = owner;
				this.aggregateLockManager = aggregateLockManager;
				this.transactionManager = transactionManager;
			}

			public void TrackRollback()
			{
				rolledBackBy = new StackTrace();
			}

			public void IncreaseNestingDepth()
			{
				nestingDepth++;
			}

			public void CommitTransaction()
			{
				if (nestingDepth > 1)
				{
					return;
				}

				if (dbConnection.MustRollBack)
				{
					RollbackTransaction();
				}
				else
				{
					IsFinished = true;
					transactionManager.CommitTransaction();
				}
			}

			public void RollbackTransaction()
			{
				if (nestingDepth > 1)
				{
					return;
				}

				IsFinished = true;
				transactionManager.RollbackTransaction();
			}

			public void Dispose()
			{
				if (--nestingDepth > 0)
				{
					return;
				}

				if (IsDisposed)
				{
					throw new ObjectDisposedException("Double dispose will lead to broken reference counting");
				}

				try
				{
					transactionManager.Dispose();
				}
				finally
				{
					AggregateLockManager.ReleaseSqlLocks();
					dbConnection.delayedTransactionManager = null;
					rolledBackBy = null;
					IsDisposed = true;
				}
			}

			public void ReportInvalidRollbackExceptionHandling()
			{
				if (rolledBackBy != null)
				{
					dbConnection.ErrorReporter.ReportOnce(
						"Invalid exception handling after rollback",
						$"If you rollback a delayed transaction you must throw back to the layer where the delayed transaction in managed. Rollback happened here: {rolledBackBy}",
						null);
				}
			}
		}

		sealed class AggregateLockManager : ITransactionLockManager
		{
			readonly List<ITransactionLockManager> lockManagers = new List<ITransactionLockManager>();
			readonly List<ISqlApplicationLock> sqlApplicationLocks = new List<ISqlApplicationLock>();

			public AggregateLockManager(ITransactionLockManager lockManager)
			{
				lockManagers.Add(lockManager);
			}

			public void AddLockManager(ITransactionLockManager lockManager)
			{
				lockManagers.Add(lockManager);
			}

			public void AddSqlLock(ISqlApplicationLock sqlLock)
			{
				sqlApplicationLocks.Add(sqlLock);
			}

			public void ReleaseSqlLocks()
			{
				foreach (var lockManager in lockManagers)
				{
					lockManager.ReleaseSqlLocks();
				}

				foreach (var sqlLock in sqlApplicationLocks)
				{
					sqlLock.Dispose();
				}
			}
		}
	}

	public enum LockedProcessResult
	{
		Error,
		Completed,
		AlreadyBeingProcessed,
	}

	#region Interfaces

	public interface IDbConnectionInternals
	{
		System.Data.Common.DbConnection SqlConnection { get; }
		System.Data.Common.DbConnection ADOConnection { get; }
		System.Data.Common.DbTransaction ADOTransaction { get; }

		HttpConnection HttpConnection { get; }
		Guid HttpTransactionId { get; }

		IDbTransaction InternalDbTransaction { get; }
		IDbConnection InternalDbConnection { get; }
	}

	public interface IDbReconnectionHandling : ICurrentDbControl
	{
		ConnectionState State { get; }
		int AppTransactionCount { get; }
		bool IsConnecting { get; }
		string ImpersonatedLogin { get; }
		string LoginName { get; }
		void CloseAndReopenConnection();
		bool HasDbSchemaOrScriptOrTransformationVersionChanged();
	}

	public interface ICurrentDbControl
	{
		IDisposable UseDatabase(string database);
		string ServerName { get; }
		string InitialDatabase { get; }
	}

	public interface IPhysicalRefDbLocation
	{
		string GetReferenceDatabaseName(RefDbTypeEnum refDbType, string refCountryCode);
		string LoadRefDbNameFromSynonym(string mainDbName, RefDbTypeEnum refDbType, string refCountryCode);
		void ClearRefDbNameBuffers();
	}
	#endregion
}

#region Test
#if DEBUG

namespace CargoWise.Data
{
	public abstract partial class DbConnection : IDisposable, IDbConnectionInternals, IDbReconnectionHandling, IDbConnected, ICurrentDbControl, IPhysicalRefDbLocation, IOpenConnectionErrorHandler
	{
		public event Action<DbCommand> OnExecute = DbCommitTracker.OnExecute;
		public event Action<DbCommand> OnBeforeExecute;

		public void InvokeOnExecute(DbCommand dbCommand)
		{
			OnExecute?.Invoke(dbCommand);
		}

		public void InvokeOnBeforeExecute(DbCommand dbCommand)
		{
			OnBeforeExecute?.Invoke(dbCommand);
		}

		public int ExecutedCommandCountForAllConnections
		{
			get { return executedCommandCountForAllConnections; }
		}

		public int ExecutedCommandCount
		{
			get { return executedCommandCount; }
		}

		public IEnumerable<string> ExecutedCommands => executedCommands?.Commands.Select(t => t.ExecutedCommandWithMoreInfo);

		[SuppressMessage("Microsoft.Design", "CA1006:Do not nest generic types in member signatures", Justification = "You're not my real dad")]
		public IEnumerable<Tuple<string, List<string>>> ExecutedCommandsAndQueryPlans => executedCommands?.Commands.Select(c => Tuple.Create(c.ExecutedCommandWithMoreInfo, c.ExecutionPlans));

		public IDisposable TrackExecutedCommands(bool includeStackTrace = false, bool includeQueryPlansForExecuteReaderCommands = false)
		{
			executedCommands = new TrackedExecutedCommands
			{
				IncludeStackTraces = includeStackTrace,
				IncludeQueryPlansForExecuteReaderCommands = includeQueryPlansForExecuteReaderCommands,
			};

			return new DisposableAction(() => executedCommands = null);
		}

		internal bool IsTrackingExecutedCommandQueryPlans => executedCommands?.IncludeQueryPlansForExecuteReaderCommands ?? false;

		public bool HasUndisposedSqlLocks(Func<SqlApplicationLock, bool> predicate = null)
		{
			return (predicate == null) ? UndisposedSqlLocks.Any() : UndisposedSqlLocks.Any(predicate);
		}

		public int UndisposedSqlLocksCount()
		{
			return Locks.Count;
		}

		public int GetSqlLockThresholds()
		{
			return SqlLockThresholds;
		}

		public bool? IsAlwaysOnEnabledForTest
		{
			private get;
			set;
		}

		public string GetQueryPlanXml_ForTest(string sqlText)
		{
			Argument.NotNull(sqlText, nameof(sqlText));
			sqlText = DbCommand.SanitizeExecuteAsReaderFlags(sqlText);

			try
			{
				using (var cmd = Command("SET SHOWPLAN_XML ON"))
				{
					cmd.ExecuteNonQuery();
				}
				using (var cmd = Command(sqlText))
				{
					return cmd.ExecuteScalar()?.ToString() ?? string.Empty;
				}
			}
			finally
			{
				using (var cmd = Command("SET SHOWPLAN_XML OFF"))
				{
					cmd.ExecuteNonQuery();
				}
			}
		}

		public void AddSharedReferenceDatabase_ForTest(string dbName)
		{
			var existingRefDbs = GetDbList(DatabaseType.SharedRef, sdDatabaseReadonlyFilter: null);
			existingRefDbs.Add(dbName);
			sharedRefDbs = existingRefDbs;
		}

		public void ClearRefDbNameBuffers_ForTest()
		{
			((IPhysicalRefDbLocation)this).ClearRefDbNameBuffers();
		}

		public IDisposable SetDatabaseUpgradedExceptionHasBeenThrown_ForTest(bool value = true)
		{
			var savedValue = DatabaseUpgradedExceptionHasBeenThrown;
			DatabaseUpgradedExceptionHasBeenThrown = value;
			return new DisposableAction(() => DatabaseUpgradedExceptionHasBeenThrown = savedValue);
		}

		public void ResetDatabaseUpgradedExceptionHasBeenThrown()
		{
			DatabaseUpgradedExceptionHasBeenThrown = false;
		}

		public IDisposable SetSqlServerVersionForTest(string sqlServerVersion)
		{
			var originalVersion = serverVersionNumber;
			serverVersionNumber = new SqlServerVersionNumber(sqlServerVersion);
			return new DisposableAction(() => serverVersionNumber = originalVersion);
		}

		public static void ResetSuspendAuditTriggersFromRegistry_ForTest()
		{
			suspendAuditTriggersRegistry = DbRegistry.SuspendAuditTriggersDefaultValue;
		}

		public class RetryContextForTest
		{
			public Func<string, int, bool> Condition { get; set; }
			public int ExecutionCount { get; set; }
			public Action<string, int> Action { get; set; }

			public IDisposable Prepare(Func<string, int, bool> condition, Action<string, int> action)
			{
				this.Condition = condition;
				this.Action = action;
				this.ExecutionCount = 0;

				return new DisposableAction(() => this.Reset());
			}

			public void Reset()
			{
				this.Condition = null;
				this.Action = null;
				this.ExecutionCount = 0;
			}
		}

		public RetryContextForTest RetryContext_ForTest { get; set; }

		public IDisposable PrepareRetryContextForTest(Func<string, int, bool> condition, Action<string, int> action)
		{
			// Use a temporary policy without wait to reduce the testing time
			var temporaryRetryPolicyDisposable = DataUtils.SetTemporaryLockTimeoutRetryPolicyForTest();

			RetryContext_ForTest = new RetryContextForTest();
			var retryContextDisposable = RetryContext_ForTest.Prepare(condition, action);

			return new DisposableAction(() =>
			{
				retryContextDisposable.Dispose();
				this.RetryContext_ForTest = null;

				temporaryRetryPolicyDisposable.Dispose();
			});
		}
	}
}

#endif
#endregion
#pragma warning restore CW1106 // Do Not Leave In Debug Messages
