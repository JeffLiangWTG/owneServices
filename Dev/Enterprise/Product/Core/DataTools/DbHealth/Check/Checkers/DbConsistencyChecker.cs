using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Data;
using CargoWise.Data.SqlServer;
using CargoWise.Types;
using Enterprise.DbHealth.Shared;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using static System.FormattableString;

namespace Enterprise.DbHealth.Check
{
	class DbConsistencyChecker : RunnerWithRegistryWorker, IChecker
	{
		#region Constructor

		public DbConsistencyChecker(DbConnection workerConnection, string databaseName)
		{
			this.workerConnection = workerConnection;
			this.databaseName = databaseName;
			this.isPrimary = true;
		}

		public DbConsistencyChecker(DbConnection primaryConnection, DbConnection workerConnection, string databaseName)
		{
			this.primaryConnection = primaryConnection;
			this.workerConnection = workerConnection;
			this.databaseName = databaseName;
			this.isPrimary = false;
		}

		readonly DbConnection primaryConnection;
		readonly DbConnection workerConnection;
		readonly string databaseName;
		readonly bool isPrimary;
		const string SharedRefDb_LatestConsistencyCheck_ExtendedPropertyName = "LatestConsistencyCheck";

		public static string GetSharedDbExtendedPropertyName(DbConnection connection)
		{
			return SharedRefDb_LatestConsistencyCheck_ExtendedPropertyName;
		}

		#endregion

		#region overrides

		#region RunnerWithRegistryWorker

		public override IDbRegistryWorker GetNewWorker()
		{
			return (isPrimary)
				? new DbConsistencyRegistryWorker(workerConnection, databaseName)
				: new DbConsistencySecondaryRegistryWorker(workerConnection, databaseName);
		}

		protected override string CustomExceptionMessage
		{
			get { return "Db Consistency check timed out"; }
		}

		protected override int InitialTimeoutMs
		{
			get { return Env.Registry.DbccInitialTimeout; }
		}

		protected override void HandleRegistryWorkerIsStuck()
		{
			AddWarningToList(RegistryWorker.CurrentDatabase, RegistryWorker.IsStuckMessage, "");
		}

		#endregion // RunnerWithRegistryWorker

		#endregion // overrides

		#region Properties

		protected DbHealthWarningList WarningList { get; set; }

		public ILogger Logger { get; set; }

		#endregion

		#region Checks

		#region CheckerBase Members

		void IChecker.Check(DbConnection connection, DbHealthWarningList warningList, ILogger logger)
		{
			var dbNames = GetDatabasesToCheck(connection);
			this.WarningList = warningList;
			this.Logger = logger;

			Run(connection, dbNames);
		}

		string IChecker.Description
		{
			get { return "Check database does not have consistency/allocation errors"; }
		}

#if DEBUG
		protected virtual
#endif
		IEnumerable<string> GetDatabasesToCheck(DbConnection connection)
		{
			if (databaseName.EndsWith(Db.AuditDatabaseSuffix, StringComparison.OrdinalIgnoreCase) ||
				databaseName.EndsWith(Db.EdwDatabaseSuffix, StringComparison.OrdinalIgnoreCase))
			{
				return new List<string> { databaseName };
			}
			else
			{
				return connection.GetDatabases(DatabaseType.All & ~DatabaseType.EDW);
			}
		}

		public TimeSpan MinimumTimeBetweenSharedRefDbChecks { get; } = TimeSpan.FromDays(7);

		#endregion

		protected override void RunOnGivenDB(DbConnection connection, string dbName)
		{
			if (RefDbTableNameResolver.IsSharedDatabase(dbName))
			{
				var latestConsistencyCheckProperty = DataUtils.LoadDbExtendedProperty(connection, GetSharedDbExtendedPropertyName(connection), dbName);
				if (!DateTimeOffset.TryParse(latestConsistencyCheckProperty, out var latestConsistencyCheck))
				{
					latestConsistencyCheck = DateTimeOffset.MinValue;
				}

				if (ZDateTimeOffset.UtcNow.ToDateTimeOffset() - latestConsistencyCheck < MinimumTimeBetweenSharedRefDbChecks)
				{
					Logger.Log(LogType.Information, Invariant($"Skipped check for database '{dbName}' as it was last checked on {latestConsistencyCheck}. Minimum time between checks is set to {MinimumTimeBetweenSharedRefDbChecks}."));
					return;
				}
			}

			try
			{
				PerformActualCheck(dbName, connection);
			}
			catch (SqlException ex)
			{
				AddWarningToList(dbName, ex.Message);
			}

			if (RefDbTableNameResolver.IsSharedDatabase(dbName))
			{
				var propertyConnection = (isPrimary || dbName == RefDbTableNameResolver.SingleRefDatabaseName)
					? connection
					: primaryConnection;
				DataUtils.SaveDbExtendedProperty(propertyConnection, GetSharedDbExtendedPropertyName(connection), ZDateTimeOffset.UtcNow.ToDateTimeOffset().ToString(), dbName);
			}
		}

		internal const string DatabaseConsistencyCheckLockKey = "DatabaseConsistencyCheck";

		/// <summary>
		/// Throws exception if DB has consistency/allocation errors
		/// 
		/// Example of an exception (and its error collection) thrown:
		///   8928 - Object ID 3, index ID 2: Page (1:30) could not be processed. See other errors for details.
		///   8944 - Table error: Object ID 3, index ID 2, page (1:30), row 229...
		///   8990 - CHECKDB/CHECKTABLE found 0 allocation errors and 2 consistency errors in table 'sys.columns' (object ID 3).
		///   8989 - CHECKDB/CHECKTABLE found 0 allocation errors and 2 consistency errors in database 'dbname'.
		///   8958 - repair_allow_data_loss is the minimum repair level for the errors found by DBCC CHECKDB/CHECKTABLE (dbname).
		/// </summary>
		void PerformActualCheck(string dbName, DbConnection connection)
		{
			var runPhysicalOnly = isPrimary && Env.Registry.DbccRunCheckdbWithPhysicalOnly && AlwaysOn.IsDbPartOfAlwaysOn(connection, dbName);
			var runByParts = isPrimary || Env.Registry.DbccRunSecondariesByParts;
			var result = (runPhysicalOnly)
				? RunCheckdbWithPhysicalOnly(connection, dbName)
				: (runByParts)
					? RunDatabaseChecksByParts(connection, dbName)
					: RunCheckdbInOneGo(connection, dbName);

			switch (result)
			{
				case LockedProcessResult.Completed:
					break;
				case LockedProcessResult.AlreadyBeingProcessed:
					Logger.Log(LogType.Information, "Database " + dbName + " is already being processed.");
					break;
				case LockedProcessResult.Error:
					Logger.Log(LogType.Warning, "Database " + dbName + " failed to completely process and will be processed on next cycle of DB Consistency Check.");
					break;
			}
		}

		LockedProcessResult RunDatabaseChecksByParts(DbConnection connection, string dbName)
		{
			Logger.Information($"Checking [{dbName}] database - by parts");

			return connection.RunLocked(DatabaseConsistencyCheckLockKey, (_) =>
			{
				int startStepForCurrentDb = RegistryWorker.StartStepForDb(dbName);

				if (startStepForCurrentDb <= (int)StepsOfDbCheck.Views)
				{
					if (startStepForCurrentDb <= (int)StepsOfDbCheck.Catalog)
					{
						if (startStepForCurrentDb <= (int)StepsOfDbCheck.Tables)
						{
							if (startStepForCurrentDb <= (int)StepsOfDbCheck.Allocation)
							{
								RegistryWorker.CurrentStep = (int)StepsOfDbCheck.Allocation;
								DoRunCheckAlloc(dbName, connection);
							}

							RegistryWorker.CurrentStep = (int)StepsOfDbCheck.Tables;
							string startTableForCurrentDb = RegistryWorker.StartTableForDb(dbName);
							DoRunCheckTables(dbName, startTableForCurrentDb, connection);
						}

						RegistryWorker.CurrentStep = (int)StepsOfDbCheck.Catalog;
						DoRunCheckCatalog(dbName, connection);
					}

					RegistryWorker.CurrentStep = (int)StepsOfDbCheck.Views;
					string startViewForCurrentDb = RegistryWorker.StartViewForDb(dbName);
					DoRunCheckViews(dbName, startViewForCurrentDb, connection);
				}
			}, null, 1, dbName);
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		LockedProcessResult RunCheckdbWithPhysicalOnly(DbConnection connection, string dbName)
		{
			Logger.Information($"Checking [{dbName}] database - physical only");

			return connection.RunLocked(DatabaseConsistencyCheckLockKey, (_) =>
			{
				var sql = @"-- DBCC CHECKDB with PHYSICAL_ONLY
DBCC CHECKDB (@dbName)
	WITH
		PHYSICAL_ONLY
		, NO_INFOMSGS
";

				using (connection.TemporarySetLockTimeout(DbConnection.LockTimeout.Infinite))
				using (var cmd = connection.Command(sql, DbCommand.Timeout.Infinite))
				{
					cmd.AddParameter("@dbName", SqlDbType.NVarChar, 128, dbName);

					cmd.ExecuteNonQuery();
				}
			}, null, 1, dbName);
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		LockedProcessResult RunCheckdbInOneGo(DbConnection connection, string dbName)
		{
			Logger.Information($"Checking [{dbName}] database - using DBCC CHECKDB");

			return connection.RunLocked(DatabaseConsistencyCheckLockKey, (_) =>
			{
				var sql = @"-- DBCC CHECKDB
DBCC CHECKDB (@dbName) WITH NO_INFOMSGS
";

				using (connection.TemporarySetLockTimeout(DbConnection.LockTimeout.Infinite))
				using (var cmd = connection.Command(sql, DbCommand.Timeout.Infinite))
				{
					cmd.AddParameter("@dbName", SqlDbType.NVarChar, 128, dbName);

					try
					{
						cmd.ExecuteNonQuery();
					}
					catch (SqlException ex)
					{
						Logger.Information($"Failed to run DBCC CHECKDB:\r\n{ex.Message}");

						throw;
					}
				}
			}, null, 1, dbName);
		}

		#region Check Allocation

		/// <summary>
		/// Checks the consistency of disk space allocation structures for a specified database.
		/// Throws exception if it has errors.
		/// 
		/// Example of an exception (and its error collection) thrown:
		/// [todo: add sample here]
		/// </summary>
		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void DoRunCheckAlloc(string dbName, DbConnection connection)
		{
			var sql = string.Format(CultureInfo.InvariantCulture, @"-- DBCC CHECKALLOC
DBCC CHECKALLOC ('{0}') WITH NO_INFOMSGS;
"
				, dbName
				);

			using (var cmd = connection.Command(sql, DbCommand.Timeout.Infinite))
			{
				try
				{
					ExecuteCommand(cmd);
				}
				catch (SqlException ex)
				{
					Logger.Information($"Failed to run DBCC CHECKALLOC:\r\n{ex.Message}");

					throw;
				}
			}
		}

		#endregion

		#region Check table and indexed view integrity

		void DoRunCheckTables(string dbName, string startTable, DbConnection connection)
		{
			var startTableFilter = (String.IsNullOrWhiteSpace(startTable) || !DataUtils.ObjectExists(connection, "[" + dbName + "]." + startTable))
				? String.Empty
				: string.Format(CultureInfo.InvariantCulture, "AND '[' + sch.name + '].[' + obj.name + ']' >= '{0}'", startTable);

			var sql = string.Format(CultureInfo.InvariantCulture, selectOrderedObjectSqlText,
				dbName,          // 0
				"'S', 'U'",      // 1
				startTableFilter // 2
				);

			var tablesToCheck = Utilities.GetDataTableFromQuery(connection, sql);
			CheckTableOrView(tablesToCheck, dbName, connection);
		}

		void DoRunCheckViews(string dbName, string startView, DbConnection connection)
		{
			string additionalIndexedViewFilter = string.Format(CultureInfo.InvariantCulture, "AND exists(SELECT null FROM [{0}].sys.indexes ind WHERE ind.object_id = obj.object_id) ", dbName);

			string startViewFilter = (String.IsNullOrWhiteSpace(startView) || !DataUtils.ObjectExists(connection, "[" + dbName + "]." + startView))
				? String.Empty
				: string.Format(CultureInfo.InvariantCulture, "AND '[' + sch.name + '].[' + obj.name + ']' >= '{0}'", startView);

			string sqlText = string.Format(CultureInfo.InvariantCulture, selectOrderedObjectSqlText,
				dbName, "'V'", additionalIndexedViewFilter + startViewFilter);

			DataTable indexedViews = Utilities.GetDataTableFromQuery(connection, sqlText);
			CheckTableOrView(indexedViews, dbName, connection);
		}

		/// <summary>
		/// Checks the integrity of all the pages and structures that make up the table or indexed view.
		/// Throws exception if it has errors.
		/// 
		/// Example of an exception (and its error collection) thrown:
		/// [todo: add sample here]
		/// </summary>
		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void CheckTableOrView(DataTable objectTable, string dbName, DbConnection connection)
		{
			foreach (DataRow row in objectTable.Rows)
			{
				var obj_schema = row["obj_schema"].ToString();
				var obj_name = row["obj_name"].ToString();
				var fullObjectName = string.Format(CultureInfo.InvariantCulture, "[{0}].[{1}].[{2}]", dbName, obj_schema, obj_name);

				RegistryWorker.CurrentTableOrView = string.Format(CultureInfo.InvariantCulture, "[{0}].[{1}]", obj_schema, obj_name);
				CheckAndReturnRemainingTimeoutInSeconds();

				if (!TryUseTablock(fullObjectName))
				{
					using (var cmd = connection.Command(GetSqlCheckTableWithShapshot(), DbCommand.Timeout.Infinite))
					{
						cmd.AddParameter("@full_obj_name", SqlDbType.NVarChar, 4000, fullObjectName);
						ExecuteCommand(cmd);

						Logger.Information(string.Format(CultureInfo.InvariantCulture, "{0} - DBCC CHECKTABLE using SNAPSHOT complete.", fullObjectName));
					}
				}
			}

			RegistryWorker.CurrentTableOrView = string.Empty;
		}

		[SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		protected override int CheckAndReturnRemainingTimeoutInSeconds()
		{
			try
			{
				return base.CheckAndReturnRemainingTimeoutInSeconds();
			}
			catch (CustomTimeoutException)
			{
				Logger.Information(CustomExceptionMessage);
				throw;
			}
		}

		protected override void ExecuteCommand(DbCommand cmd)
		{
			cmd.ExecuteNonQuery();
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
#if DEBUG
		protected virtual
#endif
		bool TryUseTablock(string fullObjectName)
		{
			var userWaitThreshold = Env.Registry.DbccUserWaitThreshold;
			var serviceTaskWaitThreshold = Env.Registry.DbccServiceTaskWaitThreshold;
			var pollingInterval = Env.Registry.DbccPollingInterval;

			var completedSuccessfully = true;

			using (var runnerReadyEvent = new ManualResetEventSlim(false))
			using (var observerSource = new CancellationTokenSource())
			using (var runnerSource = new CancellationTokenSource())
			{
				var runnerSPID = int.MinValue;
				var observerToken = observerSource.Token;
				var runnerToken = runnerSource.Token;

				var observerTask = Task.Run(() =>
				{
					runnerReadyEvent.Wait();

					using (Db.DisposableActionForDbConnection())
					using (var hostConnection = Db.NewAdminConnection())
					{
						using (var cmd = hostConnection.Command(sqlCancelTask, DbCommand.Timeout.Infinite))
						{
							cmd.AddParameter("@blocking_spid", SqlDbType.Int, runnerSPID);
							cmd.AddParameter("@wait_user_ms", SqlDbType.BigInt, userWaitThreshold);
							cmd.AddParameter("@wait_runner_ms", SqlDbType.BigInt, serviceTaskWaitThreshold);

							try
							{
								while (!runnerToken.WaitHandle.WaitOne(pollingInterval) && !runnerToken.IsCancellationRequested)
								{
									var blockedProcess = Convert.ToString(cmd.ExecuteScalar(), CultureInfo.InvariantCulture);
									if (!String.IsNullOrWhiteSpace(blockedProcess))
									{
										completedSuccessfully = false;

										var message = string.Format(CultureInfo.InvariantCulture, "{0} - The operation was cancelled due to blocking {1} process. Switched to using Snapshot.", fullObjectName, blockedProcess);
										Logger.Information(message);

										break;
									}
								}
							}
							catch (ObjectDisposedException)
							{
								/*The cancellation can cause ObjectDisposedException if the current task gets timeout. So just eat it up here.*/
							}
							finally
							{
								observerSource.Cancel();
							}
						}
					}
				});

				var runnerTask = Task.Run(async () =>
				{
					using (Db.DisposableActionForDbConnection())
					using (var runnerConnection = Db.NewAdminConnection())
					{
						runnerSPID = runnerConnection.SPID;
						runnerReadyEvent.Set();

						var cmd = runnerConnection.Command(GetSqlCheckTableWithTablock(userWaitThreshold), DbCommand.Timeout.Infinite);
						cmd.AddParameter("@full_obj_name", SqlDbType.NVarChar, 4000, fullObjectName);

						try
						{
							runnerConnection.ThreadSentry.RelinquishThreadOwnership();
							await cmd.ExecuteNonQueryAsync(observerToken);
						}
						catch (InvalidOperationException)
						{
							/*The cancellation can cause ObjectDisposedException if the current task gets timeout. So just eat it up here.*/
							/*Or the DB connection is closed and waiting fo the next server task run */
						}
						finally
						{
							runnerConnection.ThreadSentry.TakeThreadOwnership();
							runnerSource.Cancel();
						}
					}
				});

#if DEBUG
				RunActionForTest();
#endif

				try
				{
					Task.WaitAll(observerTask, runnerTask);
				}
				catch (AggregateException aggEx)
				{
					if (aggEx.InnerException != null)
					{
						var sqlEx = aggEx.InnerException as SqlException;
						if (sqlEx != null)
						{
							var errorMatcher = new DbErrorMatch(sqlEx);
							if (errorMatcher.ExceptionType == DbErrorType.SevereError)
							{
								// skip the following error (ErrorCode == -2146232060):
								// A severe error occurred on the current command.  The results, if any, should be discarded.
								// Operation cancelled by user.
							}
							else
							{
								completedSuccessfully = false;

								if (errorMatcher.ExceptionType == DbErrorType.LockTimeoutExpired)
								{
									var message = string.Format(CultureInfo.InvariantCulture, "{0} - The operation was cancelled due to lock timeout. Switched to using Snapshot.", fullObjectName);
									Logger.Information(message);
								}
								else
								{
									var message = string.Format(CultureInfo.InvariantCulture, "{0} - The operation was cancelled due to unhandled exception. Switched to using Snapshot.", fullObjectName);
									message += System.Environment.NewLine + aggEx.ToString() + System.Environment.NewLine;
									Logger.Warning(message);
								}
							}
						}
					}
				}
				finally
				{
					observerSource.Cancel();
					runnerSource.Cancel();
				}

				return completedSuccessfully;
			}
		}

		#region SQL

		const string selectOrderedObjectSqlText = @"-- DBCC Select objects
			SELECT
				obj_schema = sch.name,
				obj_name   = obj.name
			FROM
				[{0}].sys.objects obj 
				INNER JOIN [{0}].sys.schemas sch
					ON obj.schema_id = sch.schema_id
			WHERE
				type in ({1})
				{2}
			ORDER BY
				sch.name, obj.name
			;";

		internal string GetSqlCheckTableWithShapshot()
		{
			return @"-- DBCC CHECKTABLE with SNAPSHOT
if (OBJECT_ID(@full_obj_name) is NOT NULL) DBCC CHECKTABLE (@full_obj_name) WITH NO_INFOMSGS, MAXDOP=1";
		}

		internal string GetSqlCheckTableWithTablock(int lockTimeout)
		{
			return string.Format(CultureInfo.InvariantCulture, @"-- DBCC CHECKTABLE with TABLOCK
SET LOCK_TIMEOUT {0};
if (OBJECT_ID(@full_obj_name) is NOT NULL) DBCC CHECKTABLE (@full_obj_name) WITH TABLOCK, NO_INFOMSGS, MAXDOP=1"
				, lockTimeout
				);
		}

		internal const string sqlCancelTask = @"-- DBCC Check cancellation
SELECT TOP(1)
	BlockedProcess =
		CASE
			WHEN w.wait_duration_ms > @wait_runner_ms AND s.program_name in ('CargoWiseOneServiceHost', 'CargoWiseOneServiceRunner', 'ediEnterpriseServiceHost', 'ediEnterpriseServiceRunner', 'ediWebPrint') THEN 'Service Task'
			ELSE 'user'
		END
FROM
	sys.dm_os_waiting_tasks   AS w
	JOIN sys.dm_exec_sessions AS s ON s.session_id = w.session_id
WHERE 1=1
	AND w.blocking_session_id <> w.session_id
	AND w.blocking_session_id = @blocking_spid
	AND
	(1=2
		OR w.wait_duration_ms > @wait_runner_ms AND s.program_name in ('CargoWiseOneServiceHost', 'CargoWiseOneServiceRunner', 'ediEnterpriseServiceHost', 'ediEnterpriseServiceRunner', 'ediWebPrint')
		OR w.wait_duration_ms > @wait_user_ms
	);
";

		#endregion // SQL

		#endregion // Check table and indexed view integrity

		#region Check Catalog

		/// <summary>
		/// Checks for catalog consistency within the specified database.
		/// Throws exception if it has errors
		/// 
		/// Example of an exception (and its error collection) thrown:
		/// [todo: add sample here]
		/// </summary>
		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void DoRunCheckCatalog(string dbName, DbConnection connection)
		{
			var sql = string.Format(CultureInfo.InvariantCulture, @"-- DBCC CHECKCATALOG
DBCC CHECKCATALOG ('{0}') WITH NO_INFOMSGS;
"
				, dbName
				);

			using (var cmd = connection.Command(sql, DbCommand.Timeout.Infinite))
			{
				try
				{
					ExecuteCommand(cmd);
				}
				catch (SqlException ex)
				{
					Logger.Information($"Failed to run DBCC CHECKCATALOG:\r\n{ex.Message}");

					throw;
				}
			}
		}

		#endregion

		#endregion // Checks

		#region Format and append warnings

		protected void AddWarningToList(string dbName, string errorMessage)
		{
			string action =
				"Please contact your IT technician to check for faulty hardware, as this is a common cause of the problem. " +
				"After confirming that you hardware is in working order, please contact the CargoWise implementation department " +
				"who can attempt to fix the consistency error as a chargeable service.";

			AddWarningToList(dbName, errorMessage, action);
		}

		protected void AddWarningToList(string dbName, string errorMessage, string action)
		{
			string errorDetail = GetWarningDescription(dbName, errorMessage);
			DatabaseWarning warning = new DatabaseWarning(dbName, DatabaseWarning.DbConsistencyWarning, errorDetail, action);
			WarningList.Add(warning);
		}

		string GetWarningDescription(string dbName, string errorMessage)
		{
			var result = new StringBuilder();

			string repairLevelPattern = @"(?<repair>REPAIR_ALLOW_DATA_LOSS|REPAIR_FAST|REPAIR_REBUILD)?.+DBCC\s+";
			var repairLevelRegex = new Regex(repairLevelPattern, RegexOptions.IgnoreCase);
			var repairLevelMatch = repairLevelRegex.Match(errorMessage);

			if (repairLevelMatch.Success)
			{
				result.AppendLine(Invariant($"The database {dbName} has consistency/allocation errors."))
					.Append("This errors are mostly caused by a hardware problem (faulty hard disk or RAID controller).");
			}
			else
			{
				result.AppendLine(Invariant($"The database {dbName} has failed a consistency check."))
					.Append("This means that your database may be corrupted, with the possibility of data loss.");
			}

			result.AppendLine().AppendLine().Append(errorMessage);

			return result.ToString();
		}

		#endregion

#if DEBUG
		#region Test

		internal Action ActionForTest;

		void RunActionForTest()
		{
			if (ActionForTest != null)
			{
				ActionForTest.Invoke();
			}
		}

		#endregion
#endif
	}
}
