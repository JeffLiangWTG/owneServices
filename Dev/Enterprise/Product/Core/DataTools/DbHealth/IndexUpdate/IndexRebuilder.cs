using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Diagnostics;
using static System.FormattableString;

namespace Enterprise.DbHealth.IndexUpdate
{
	partial class IndexRebuilder : IndexUpdater
	{
		public IndexRebuilder(ILogger logger, Func<string, ILowPriorityProcessPauser> backlogWaiterFactory = null)
			: base(logger)
		{
			this.backlogWaiterFactory = backlogWaiterFactory ?? (dbName => new LowPriorityProcessPauser(dbName));
		}

		protected override void RunOnGivenDB(DbConnection connection, string dbName)
		{
			if (registrySettings.Rebuild_ThresholdPercentage == 100)
			{
				Logger.Information(Invariant($"Rebuilding indexes on database {dbName.QuoteName()} skipped due to configuration (Rebuild Threshold = 100)"));

				return;
			}

			if (connection.IsDbWriteable(dbName))
			{
				using (connection.TemporarySetDeadlockPriority(DeadlockPriority.Max))
				{
					Logger.Information(Invariant($"Checking for fragmented indexes: DB [{dbName}]"));

					if (RefDbTableNameResolver.IsSharedDatabase(dbName))
					{
						RunWithAppLock(dbName, () =>
						{
							using (((ICurrentDbControl)connection).UseDatabase(dbName))
							using (registrySettings.TemporarySetAbortAfterWait(OnlineIndexRebuildLowPriorityAbortAfterWaitList.Codes.Self))
							{
								UpdateIndexes(connection, dbName);
							}
						});
					}
					else
					{
						UpdateIndexes(connection, dbName);
					}
				}
			}
		}

		/// <summary>
		/// Page Splits
		/// 
		/// A correctly chosen fill-factor value can reduce potential page splits by providing enough space for index expansion as data is added to the underlying table.
		/// When a new row is added to a full index page, the Database Engine moves approximately half the rows to a new page to make room for the new row.
		/// This reorganization is known as a page split. A page split makes room for new records, but can take time to perform and is a resource intensive operation.
		/// Also, it can cause fragmentation that causes increased I/O operations.
		/// When frequent page splits occur, the index can be rebuilt by using a new or existing fill-factor value to redistribute the data.
		/// For more information, see Reorganizing and Rebuilding Indexes.
		/// 
		/// Although a low, nonzero fill-factor value may reduce the requirement to split pages as the index grows,
		/// the index will require more storage space and can decrease read performance.
		/// Even for an application oriented for many insert and update operations, the number of database reads typically outnumber database writes by a factor of 5 to 10.
		/// Therefore, specifying a fill factor other than the default can decrease database read performance by an amount inversely proportional to the fill-factor setting.
		/// For example, a fill-factor value of 50 can cause database read performance to decrease by two times.
		/// Read performance is decreased because the index contains more pages, therefore increasing the disk IO operations required to retrieve the data.
		/// 
		/// Adding Data to the End of the Table
		/// A nonzero fill factor other than 0 or 100 can be good for performance if the new data is evenly distributed throughout the table.
		/// However, if all the data is added to the end of the table, the empty space in the index pages will not be filled.
		/// For example, if the index key column is an IDENTITY column, the key for new rows is always increasing and the index rows are logically added to the end of the index.
		/// If existing rows will be updated with data that lengthens the size of the rows, use a fill factor of less than 100.
		/// The extra bytes on each page will help to minimize page splits caused by extra length in the rows.
		/// 
		/// See also: http://technet.microsoft.com/en-us/library/ms189858.aspx
		/// </summary>
		void UpdateIndexes(DbConnection connection, string targetDbName)
		{
			this.indexes = new Queue<IndexUpdateInfo>(QueryIndexUpdateInfo(connection, targetDbName)
				.Where(i => i.IsOverPageCountThreshold() && i.TargetAction == IndexAction.Rebuild)
				.OrderByDescending(i => i.PageCount)
				);

			RunUserActionUpdateIndexes_ForTest();
			if (indexes.Count > 0)
			{
				var backlogWaiter = backlogWaiterFactory.Invoke(targetDbName);

				RunUserActionRebuild_ForTest();

				using (connection.TemporarySetLockTimeout(DbConnection.LockTimeout.Infinite))
				{
					while (indexes.Count > 0)
					{
						var index = indexes.Dequeue();

						try
						{
							RunUserActionRebuild2_ForTest(index.DatabaseName);

							backlogWaiter.Wait(Logger, registrySettings.MaxBacklogWaitTime);
							RegistryWorker.CurrentTableOrView = Invariant($"{index.SchemaName}.{index.TableName}.{index.IndexName}");

							RebuildIndexAndUpdateInfo(connection, index, index.CurrentFillFactor, 0);
						}
						catch (BacklogWaiterTimeoutException)
						{
							throw;
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							// log the error and go to the next index
							Logger.Error(
								Invariant($"Error rebuilding index: [{index.DatabaseName}].[{index.TableName}].[{index.IndexName}]")
								, ex);
						}
					}
				}
			}

			RegistryWorker.CurrentTableOrView = "";
		}

		void RebuildIndexAndUpdateInfo(DbConnection connection, IndexUpdateInfo indexToUpdate, int currentFillFactor, int fillFactorChange)
		{
			var fillFactor = (fillFactorChange == 0)
				? currentFillFactor
				: Math.Max(MinAutoAdjustedFillFactor, Math.Min(MaxAutoAdjustedFillFactor, indexToUpdate.CurrentFillFactor + fillFactorChange));

			var message = Invariant($"Rebuilding {((fillFactorChange <= 0) ? "index" : "low fragmentation index to increase fillfactor")}");
			var fillFactorText = $"{fillFactor}%({((fillFactor == currentFillFactor) ? "=" : ((fillFactor > currentFillFactor) ? "+" : "-"))})";

			LogInformation(Logger, message, indexToUpdate, fillFactorText);

			if (RebuildIndex(connection, indexToUpdate, runOnline: true, fillFactor: fillFactor))
			{
				SetIndexRebuildInfoPropertiesTransactionally(
				connection, indexToUpdate,
				new KeyValuePair<string, object>(LastRebuildPtyName, runDate),
				new KeyValuePair<string, object>(LastReorganisePtyName, runDate),
				new KeyValuePair<string, object>(PendingFillFactorReductionPtyName, false)
			);
			}
		}

		bool RebuildIndex(DbConnection connection, IndexUpdateInfo indexToUpdate, bool runOnline, int? fillFactor = null)
		{
			try
			{
				var action = RunRebuildCommand(connection, indexToUpdate, runOnline, fillFactor);
				if (action == ObserverAction.Cancel)
				{
					LogInformation(Logger, "Canceled due to blocking process", indexToUpdate);

					return false;
				}
				else if (action == ObserverAction.Requeue)
				{
					// re-queue rebuild

					if (indexToUpdate.Retry)
					{
						LogInformation(Logger, "Skipped due to blocking process", indexToUpdate);
					}
					else
					{
						RequeueRebuild(indexToUpdate, "Re-queued due to blocking process");
					}

					return false;
				}
			}
			catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.OnlineOperationCannotBePerformed)
			{
				// re-run rebuild off-line
				return RebuildIndex(connection, indexToUpdate, false, fillFactor);
			}
			catch (SqlException ex) when (!ex.IsCriticalException() && !indexToUpdate.Retry)
			{
				// re-queue rebuild
				RequeueRebuild(indexToUpdate, "Re-queued due to exception");

				return false;
			}

			return true;
		}

		void RequeueRebuild(IndexUpdateInfo indexToUpdate, string message)
		{
			LogInformation(Logger, message, indexToUpdate);

			indexToUpdate.Retry = true;
			this.indexes.Enqueue(indexToUpdate);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		ObserverAction RunRebuildCommand(DbConnection connection, IndexUpdateInfo indexToUpdate, bool runOnline, int? fillFactor = null)
		{
			var options = new List<string>();

			options.Add(Invariant($"MAXDOP={registrySettings.Rebuild_MaxDop}"));

			if (fillFactor.HasValue)
			{
				options.Add(Invariant($"FILLFACTOR={fillFactor.Value}"));
			}

			var observer = ConfigureObserverAndOnlineOption(connection, runOnline, out var optionOnline);
			options.Add(optionOnline);

			var sql = Invariant($"ALTER INDEX {indexToUpdate.IndexName.QuoteName()} ON {indexToUpdate.DatabaseName.QuoteName()}.{indexToUpdate.SchemaName.QuoteName()}.{indexToUpdate.TableName.QuoteName()} REBUILD WITH ({string.Join(", ", options)});");

			RunUserActionRebuild3_ForTest();

			if (observer != null)
			{
				return observer.Run(connection, sql, feedbackMethod: (message) => LogInformation(Logger, message, indexToUpdate));
			}
			else
			{
				using (var cmd = connection.Command(sql, DbCommand.Timeout.Infinite))
				{
					try
					{
						_ = cmd.ExecuteNonQuery();
					}
					catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.LockTimeoutExpired && registrySettings.Rebuild_AbortAfterWait == OnlineIndexRebuildLowPriorityAbortAfterWaitList.Codes.Self)
					{
						if (BlockedQueries.Contains(sql))
						{
							return ObserverAction.Cancel;
						}
						else
						{
							BlockedQueries.Add(sql);
							return ObserverAction.Requeue;
						}
					}
				}

				return ObserverAction.None;
			}
		}

		readonly List<string> BlockedQueries = new List<string>();

		IndexRebuilderObserver ConfigureObserverAndOnlineOption(DbConnection connection, bool runOnline, out string optionOnline)
		{
			optionOnline = "";

			var online = runOnline
				&& registrySettings.Rebuild_Online
				&& connection.ServerEdition == DbConnection.SqlServerEdition.EnterpriseDeveloper
				;

			if (registrySettings.Rebuild_UseObserver)
			{
				TimeSpan initialWait;
				ObserverAction startAction;
				ObserverAction endAction;

				if (online)
				{
					// start : LCK_M_SCH_S, LCK_M_IS, LCK_M_S
					// end   : LCK_M_SCH_M

					var abortAfterWait = OnlineIndexRebuildLowPriorityAbortAfterWaitList.Codes.None;
					optionOnline = Invariant($"ONLINE=ON /*observer*/(WAIT_AT_LOW_PRIORITY (MAX_DURATION={registrySettings.Rebuild_MaxWaitInMinutes} MINUTES, ABORT_AFTER_WAIT={abortAfterWait}))");

					// => wait for registrySettings.Rebuild_MaxWaitInMinutes
					initialWait = TimeSpan.FromMinutes(registrySettings.Rebuild_MaxWaitInMinutes);

					if (registrySettings.Rebuild_AbortAfterWait == OnlineIndexRebuildLowPriorityAbortAfterWaitList.Codes.Blockers)
					{
						startAction = ObserverAction.KillBlockers;
						endAction = ObserverAction.KillBlockers;
					}
					else if (registrySettings.Rebuild_AbortAfterWait == OnlineIndexRebuildLowPriorityAbortAfterWaitList.Codes.Self)
					{
						startAction = ObserverAction.Requeue;
						endAction = ObserverAction.Cancel;
					}
					else // if (registrySettings.Rebuild_AbortAfterWait == OnlineIndexRebuildLowPriorityAbortAfterWaitList.Codes.None)
					{
						startAction = ObserverAction.Requeue;
						endAction = ObserverAction.KillBlockers;
					}
				}
				else
				{
					// start : LCK_M_SCH_S, LCK_M_SCH_M
					// end   : null

					optionOnline = "ONLINE=OFF /*observer*/";

					initialWait = TimeSpan.Zero;

					startAction = ObserverAction.KillBlockers;
					endAction = ObserverAction.None;
				}

				return new IndexRebuilderObserver(online, initialWait, startAction, endAction);
			}
			else
			{
				if (online)
				{
					var abortAfterWait = registrySettings.Rebuild_AbortAfterWait;
					if (registrySettings.Rebuild_MaxWaitInMinutes == 0 && registrySettings.Rebuild_AbortAfterWait == OnlineIndexRebuildLowPriorityAbortAfterWaitList.Codes.Self)
					{
						// illegal combination, so have to deal with this
						abortAfterWait = OnlineIndexRebuildLowPriorityAbortAfterWaitList.Codes.None;
					}

					optionOnline = Invariant($"ONLINE=ON (WAIT_AT_LOW_PRIORITY (MAX_DURATION={registrySettings.Rebuild_MaxWaitInMinutes} MINUTES, ABORT_AFTER_WAIT={abortAfterWait}))");
				}
				else
				{
					optionOnline = "ONLINE=OFF";
				}
			}

			return null;
		}

		partial void RunUserActionRebuild_ForTest();
		partial void RunUserActionRebuild2_ForTest(string dbName);
		partial void RunUserActionRebuild3_ForTest();
		partial void RunUserActionUpdateIndexes_ForTest();

		#region Implementation

		readonly DateTime runDate = Env.Time.CurrentUtcDate;
		Queue<IndexUpdateInfo> indexes;

		public const int RebuildPeriodInDays = 28;
		const int MinAutoAdjustedFillFactor = 70;
		const int MaxAutoAdjustedFillFactor = 95;

		readonly Func<string, ILowPriorityProcessPauser> backlogWaiterFactory;

		#endregion // Implementation

		#region RunnerWithRegistryWorker

		protected override string CustomExceptionMessage
		{
			get { return "Index Rebuilder has timed out and will continue its execution in the next run"; }
		}

		#endregion
	}
}

#region Test
#if DEBUG

#region Partial class

namespace Enterprise.DbHealth.IndexUpdate
{
	partial class IndexRebuilder
	{
		internal Action UserActionRebuild_ForTest { get; set; }
		partial void RunUserActionRebuild_ForTest()
		{
			UserActionRebuild_ForTest?.Invoke();
		}

		internal Action<string> UserActionRebuild2_ForTest { get; set; }
		partial void RunUserActionRebuild2_ForTest(string dbName)
		{
			UserActionRebuild2_ForTest?.Invoke(dbName);
		}

		internal Action UserActionRebuild3_ForTest { get; set; }
		partial void RunUserActionRebuild3_ForTest()
		{
			UserActionRebuild3_ForTest?.Invoke();
		}
		internal Action UserActionUpdateIndexes_ForTest { get; set; }
		partial void RunUserActionUpdateIndexes_ForTest()
		{
			UserActionUpdateIndexes_ForTest?.Invoke();
		}
	}
}

#endregion // Partial class

#endif
#endregion
