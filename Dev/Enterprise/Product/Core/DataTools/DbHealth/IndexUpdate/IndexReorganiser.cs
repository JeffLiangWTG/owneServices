using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Diagnostics;
using static System.FormattableString;

namespace Enterprise.DbHealth.IndexUpdate
{
	class IndexReorganiser : IndexUpdater
	{
		public IndexReorganiser(ILogger logger, Func<string, ILowPriorityProcessPauser> backlogWaiterFactory = null)
			: base(logger)
		{
			this.backlogWaiterFactory = backlogWaiterFactory ?? (dbName => new LowPriorityProcessPauser(dbName));
		}

		protected override void RunOnGivenDB(DbConnection connection, string dbName)
		{
			if (registrySettings.Reorganize_ThresholdPercentage == 100)
			{
				Logger.Information(Invariant($"Reorganizing indexes on database {dbName.QuoteName()} skipped due to configuration (Reorganize Threshold = 100)"));

				return;
			}

			if (connection.IsDbWriteable(dbName))
			{
				Logger.Information(Invariant($"Checking for indexes to reorganise: DB [{dbName}]"));

				if (RefDbTableNameResolver.IsSharedDatabase(dbName))
				{
					RunWithAppLock(dbName, () =>
					{
						using (((ICurrentDbControl)connection).UseDatabase(dbName))
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

		void UpdateIndexes(DbConnection connection, string targetDbName)
		{
			var indexes = QueryIndexUpdateInfo(connection, targetDbName);

			ReorganizeIndexes(connection, indexes);
			RegistryWorker.CurrentTableOrView = "";
		}

		void ReorganizeIndexes(DbConnection connection, List<IndexUpdateInfo> indexes)
		{
			using (var currentProcessesInfo = new ThreadLocal<IndexUpdateInfo>(trackAllValues: true))
			{
				var indexesToReorganize = new Queue<IndexUpdateInfo>(indexes
					.Where(i => i.IsOverPageCountThreshold() && i.TargetAction == IndexAction.Reorganize && i.IsReorganiseAllowed(runDate))
					.OrderByDescending(i => i.PageCount)
					.ToList());

				if (indexesToReorganize.Count == 0)
				{
					return;
				}
#if DEBUG
				RunUserActionReorganize_ForTest();
#endif
				var maxProcessCount = Math.Min(registrySettings.Reorganize_MaxConcurrentProcesses, indexesToReorganize.Count);
				var tasks = new List<Task>(maxProcessCount);
				for (int i = 0; i < maxProcessCount; i++)
				{
					tasks.Add(ReorganizeInSeparateThread(Logger, indexesToReorganize, connection.ServerName, connection.CurrentDatabase, runDate, currentProcessesInfo, this.timeoutForReorganize));
				}

				try
				{
					Task.WaitAll(tasks.ToArray());
				}
				catch (AggregateException ex)
				{
					throw ex.InnerException;
				}
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		Task ReorganizeInSeparateThread(ILogger logger, Queue<IndexUpdateInfo> indexes, string serverName, string databaseName, DateTime actionDate, ThreadLocal<IndexUpdateInfo> currentProcessesInfo, int? timeout = null)
		{
			return Task.Factory.StartNew(() =>
			{
				var backlogWaiter = backlogWaiterFactory.Invoke(databaseName);

				using (Db.DisposableActionForDbConnection())
				using (var connection = Db.NewAdminConnection(serverName, databaseName))
				{
					while (true)
					{
						IndexUpdateInfo index = null;
						string additionalLogInfo;

						try
						{
							lock (indexes)
							{
								if (indexes.Count == 0)
								{
									return;
								}

								index = indexes.Dequeue();
								_ = backlogWaiter.Wait(logger, registrySettings.MaxBacklogWaitTime);

								additionalLogInfo = GetCurrentProcessesInfo(currentProcessesInfo.Values.Where(info => info != null));
								currentProcessesInfo.Value = index;

								LogInformation(logger, "Reorganizing fragmented index", index, fillFactor: null, additionalLogInfo: additionalLogInfo);
							}
#if DEBUG
							RunReorganizeDelay_ForTest();
#endif
							ReorganiseIndexAndUpdateInfo(connection, index, actionDate, timeout);
						}
						catch (SqlException e) when (new DbErrorMatch(e).ExceptionType == DbErrorType.TimeoutExpired)
						{
							// normally happens after 5 hours of reorganizing particular index
						}
						catch (SqlException e) when (new DbErrorMatch(e).ExceptionType == DbErrorType.LockTimeoutExpired)
						{
							// re-queue first time and ignore second time
							lock (indexes)
							{
								if (index.Retry)
								{
									LogInformation(logger, "Skipped due to LockTimeout", index, fillFactor: null, additionalLogInfo: null);
								}
								else
								{
									index.Retry = true;
									indexes.Enqueue(index);

									LogInformation(logger, "Re-queued due to LockTimeout", index, fillFactor: null, additionalLogInfo: null);
								}
							}
						}
						catch (BacklogWaiterTimeoutException)
						{
							throw;
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							logger.Error(
								Invariant($"Error reorganizing index: [{index.DatabaseName}].[{index.TableName}].[{index.IndexName}]")
								, ex);
						}
						finally
						{
							currentProcessesInfo.Value = null;
						}
					}
				}
			}
			, TaskCreationOptions.LongRunning
			);
		}

		static void ReorganiseIndexAndUpdateInfo(DbConnection connection, IndexUpdateInfo indexToUpdate, DateTime runDate, int? timeout = null)
		{
			Reorganise(connection, indexToUpdate, timeout);
			SetIndexRebuildInfoPropertiesTransactionally(connection, indexToUpdate, new KeyValuePair<string, object>(LastReorganisePtyName, runDate));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		static void Reorganise(DbConnection connection, IndexUpdateInfo indexToUpdate, int? timeout = null)
		{
			var sql = Invariant($"ALTER INDEX {indexToUpdate.IndexName.QuoteName()} ON {indexToUpdate.DatabaseName.QuoteName()}.{indexToUpdate.SchemaName.QuoteName()}.{indexToUpdate.TableName.QuoteName()} REORGANIZE;");

			_ = connection.ExecuteNonQuery(sql, timeout);
		}

#if DEBUG
		internal IndexReorganiser SetRunDate_ForTest(DateTime value)
		{
			runDate = value;

			return this;
		}
		internal Action UserActionReorganize_ForTest { get; set; }
		void RunUserActionReorganize_ForTest()
		{
			UserActionReorganize_ForTest?.Invoke();
		}
		internal int? reorganizeDelay_ms_ForTest;
		void RunReorganizeDelay_ForTest()
		{
			if (reorganizeDelay_ms_ForTest.HasValue)
			{
				Thread.Sleep(reorganizeDelay_ms_ForTest.Value);
			}
		}

		public int? TimeoutForReorganize_ForTest
		{
			get { return timeoutForReorganize; }
			set { timeoutForReorganize = value; }
		}
#endif

		#region Implementation

		DateTime runDate = Env.Time.CurrentUtcDate;

		int? timeoutForReorganize = DbCommand.Timeout.Hours(5);
		readonly Func<string, ILowPriorityProcessPauser> backlogWaiterFactory;

		static string GetCurrentProcessesInfo(IEnumerable<IndexUpdateInfo> currentProcessesInfo)
		{
			var builder = new StringBuilder("Currently running reorganizes:\r\n");

			var anyItemAdded = false;
			foreach (var index in currentProcessesInfo)
			{
				_ = builder
					.Append(Invariant($"Index: [{index.DatabaseName}].[{index.TableName}].[{index.IndexName}]; Pages: {index.PageCount}; Fragmentation: {index.AvgFragmentation:F1}%"))
					.AppendLine();

				anyItemAdded = true;
			}

			if (!anyItemAdded)
			{
				_ = builder.Append("No current reorganizes detected");
			}

			return builder.ToString();
		}

		#endregion // Implementation

		#region RunnerWithRegistryWorker

		protected override string CustomExceptionMessage
		{
			get { return "Index Reorganiser has timed out and will continue its execution in the next run"; }
		}

		#endregion // RunnerWithRegistryWorker
	}
}
