using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
#pragma warning disable CW1108
#pragma warning disable CW1107

namespace Enterprise.Scheduler.GraphEngine
{
	#region SuppressResourceStringsCheckRegion
	public class StmQueueStateFactory : IQueueStateFactory<StmQueueState>
	{
		public StmQueueStateFactory(Func<GrEngineLogOptions> getLogOptions)
		{
			LogOptionsSetup = new Lazy<GrEngineLogOptions>(getLogOptions);
		}

		public StmQueueStateFactory(Func<GrEngineLogOptions> getLogOptions, INotifications notifications)
		{
			LogOptionsSetup = new Lazy<GrEngineLogOptions>(getLogOptions);
			Notifications = notifications;
		}

		public const string ServiceTaskCode = "UMI";

		public StmQueueState NewQueueState<TEntity>(TEntity businessObject, string[] keys, ZString messageNumber)
			where TEntity : BusinessObject
		{
			return new StmQueueState(businessObject, keys, messageNumber);
		}

		public GrEngineLogOptions LogOptions => LogOptionsSetup.Value;
		Lazy<GrEngineLogOptions> LogOptionsSetup { get; }
		INotifications Notifications { get; }

		#region move from C:\git\wtg\CargoWise\Dev\Enterprise\Product\Core\Scheduler\GraphEngine\Batching\StmQueueStateFactoryPartial_Batching.cs
		const string MAX_ROWS_PARAM = "@maxRows";
		const string APP_LOCK_PREFIX = "DEQUEUEGRENGINE";

		public QueueBatch<StmQueueState> GetDequeueBatch(BusinessObjectFactory factory, INotifications notifications, ISqlApplicationLockProvider lockProvider, GrEngineLogOptions logOptions, int dequeueBatchSize, int dequeueListSizeEstimate, GrEngineEnums.ChainOption chainOption)
		{
			var connection = ((IDbConnected)factory).Connection; // Sometimes you need to just grab the bull by the horns.

			var queuedRows = LoadPks(connection, dequeueBatchSize);
			// There is no risk in loading all PK's of QUE'd StmQueueState rows
			// as there is a finite limit on the number of StmQueueStateRows that may be created

			if (logOptions.AllWorkerLoads)
			{
				notifications.AddInfo(FormattableString.Invariant($"Loaded {queuedRows.Count} keys."));
			}

			if (queuedRows.Count > 0)
			{
				return LoadBatch(connection, queuedRows, notifications, lockProvider, logOptions, dequeueBatchSize, dequeueListSizeEstimate, chainOption);
			}
			else
			{
				return new QueueBatch<StmQueueState>(Enumerable.Empty<StmQueueState>());
			}
		}

		/// <summary>
		/// This is a fallback mechanism to increase the throughput of GrEngine services.
		/// This mechanism will cause blocked items to be loaded with the assumption that they will be unblocked by items in the same batch.
		/// </summary>
		QueueBatch<StmQueueState> LoadBatch(DbConnection connection, List<(Guid pk, Guid? chainId)> queuedItems, INotifications notifications, ISqlApplicationLockProvider lockProvider, GrEngineLogOptions logOptions, int dequeueBatchSize, int dequeueListSizeEstimate, GrEngineEnums.ChainOption chainOption)
		{
			var result = new List<StmQueueState>(dequeueListSizeEstimate);
			var appLocks = new List<ISqlApplicationLock>(dequeueListSizeEstimate);
			var queue = new Queue<(Guid pk, Guid? chainId)>(queuedItems);

			while (queue.Any() && result.Count < dequeueBatchSize)
			{
				var (itemId, chainId) = queue.Dequeue();
				if (TryGetLocks(ref itemId, ref chainId, appLocks, out var newAppLocks, lockProvider, chainOption))
				{
					LogLocksTaken(notifications, newAppLocks, logOptions);

					try
					{
#if DEBUG
						ThrowExceptionForTest();
#endif
						// It is important to ensure that the state hasn't changed between the initial load and the locking
						// We do this by loading again.
						List<StmQueueState> queueStateList;
						const string pk = "@pk";
						const string queuedStatus = "@queuedStatus";
						var filter = FormattableString.Invariant($@"WHERE SQS_PK = {pk}
AND SQS_Status = {queuedStatus}");
						using (var command = connection.Command(MakeCommand(filter)))
						{
							command.AddParameter(pk, SqlDbType.UniqueIdentifier, itemId);
							command.AddParameter(queuedStatus, StmQueueStateSchema.SQS_Status.SqlDbType, QueueStatusCodes.Codes.Queued);
							queueStateList = StmQueueState.Load(command, 1);
						}

						if (queueStateList.Count == 1)
						{
							var queueState = queueStateList[0];
							result.Add(queueState);
							appLocks.AddRange(newAppLocks);
							var maxRows = dequeueBatchSize - result.Count;

							if (!queueState.ChainID.Equals(chainId ?? Guid.Empty))
							{
								ErrorReporter.Instance.Report("Chain ID should never change after a row is queued", FormattableString.Invariant($"Chain ID should never change after a row is queued: OriginalChainID:{chainId}, QueueStateChainID:{queueState.ChainID}, QueueStateParentID:{queueState.ParentID}"), null);
																																																																															// We continue on anyway and just hope for the best.
																																																																															// The worst possible outcome of this is two messages are processed in parallel when they should not be.
																																																																															// but it is too late to stop it at this point
							}
							else if (chainOption == GrEngineEnums.ChainOption.Yes && chainId.HasValue && maxRows > 0)
							{
								result.AddRange(LoadChain(connection, queueState.ChainID, maxRows));
							}
						}
						else
						{
							newAppLocks.ForEach(l => l.Dispose()); // Couldn't load the StmQueueState, since it was probably already processed.
						}
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						appLocks.ForEach(l => l.Dispose());
						newAppLocks.ForEach(l => l.Dispose());
						throw;
					}
				}
			}

			if (logOptions.AllWorkerLoads && result.Count > 0)
			{
				notifications.AddInfo(FormattableString.Invariant($"Messages loaded ({result.Count}): {string.Join(", ", result.Select(s => s.ParentMessageNumber))}"));
			}

			return new QueueBatch<StmQueueState>(result, appLocks.ToArray());
		}

		void LogLocksTaken(INotifications notifications, List<ISqlApplicationLock> newAppLocks, GrEngineLogOptions logOptions)
		{
			if (logOptions.AllWorkerLocks)
			{
				notifications.AddInfo(FormattableString.Invariant($"Locks taken: {string.Join(", ", newAppLocks.Select(s => s.Key))}"));
			}
		}

#if DEBUG
		void ThrowExceptionForTest()
		{
			if (Globals.IsTest && ShouldThrowExceptionForTestInLoadBatch)
			{
				throw new InvalidOperationException();
			}
		}

		public bool ShouldThrowExceptionForTestInLoadBatch { get; set; }
#endif

		bool TryGetLocks(ref Guid item, ref Guid? chainId, List<ISqlApplicationLock> appLocks, out List<ISqlApplicationLock> newAppLocks, ISqlApplicationLockProvider lockProvider, GrEngineEnums.ChainOption chainOption)
		{
			// The rule is, that if something is queued it can no longer get a chain id added.
			// This means we only have to take 1 lock for the whole chain.
			// And it means that queue'd items can't change after we loaded them.
			// THIS RULE WAS BROKEN - a queued item with an item lock was getting a chain id and a second chain lock allowed it to get processed twice.
			// This is now fixed in WI00244129.
			// We continue to take both an item lock and chain lock to minimize risk.
			newAppLocks = new List<ISqlApplicationLock>(2);
			if (lockProvider.TryGetLock(APP_LOCK_PREFIX + item.ToString().ToUpperInvariant(), out var itemAppLock))
			{
				if (appLocks.Any(l => l.Key == itemAppLock.Key))
				{
					//lets not get the same item lock in the same process.
					itemAppLock.Dispose();
				}
				else
				{
					if (chainOption == GrEngineEnums.ChainOption.Yes && chainId.HasValue)
					{
						if (lockProvider.TryGetLock(APP_LOCK_PREFIX + chainId.ToString().ToUpperInvariant(), out var chainAppLock))
						{
							if (appLocks.Any(l => l.Key == chainAppLock.Key))
							{
								//if we have alredy got this chain app lock in this process then we are already processing the chain
								itemAppLock.Dispose();
								chainAppLock.Dispose();
							}
							else
							{
								newAppLocks.Add(itemAppLock);
								newAppLocks.Add(chainAppLock);
							}
						}
						else
						{
							itemAppLock.Dispose();
						}
					}
					else
					{
						newAppLocks.Add(itemAppLock);
					}
				}
			}
			return newAppLocks.Any();
		}

		IEnumerable<StmQueueState> LoadChain(DbConnection connection, Guid chainKey, int maxRows)
		{
			if (chainKey == Guid.Empty)
			{
				ErrorReporter.ReportOnce("Something terrible has happened.");
				return Enumerable.Empty<StmQueueState>();
			}
			else
			{
				const string blockedStatus = "@BlockedState";
				const string chainIdParam = "@ChainId";
				var filter = FormattableString.Invariant($@"
WHERE SQS_ChainID = {chainIdParam}
AND SQS_Status = {blockedStatus}
ORDER BY SQS_ParentMessageNumber");
				var sql = MakeCommand(filter, maxRows);

				using (var command = connection.Command(sql))
				{
					command.AddParameter(MAX_ROWS_PARAM, SqlDbType.Int, maxRows);
					command.AddParameter(chainIdParam, StmQueueStateSchema.SQS_ChainID.SqlDbType, chainKey);
					command.AddParameter(blockedStatus, StmQueueStateSchema.SQS_Status.SqlDbType, QueueStatusCodes.Codes.Blocked);

					return StmQueueState.Load(command, maxRows);
				}
			}
		}

		static string MakeCommand(string filter, int? maxRows = null)
		{
			var columns = string.Join(",", StmQueueState.Columns().Select(s => s.Name));
			var maxRowsExpression = maxRows.HasValue ? FormattableString.Invariant($"TOP ({MAX_ROWS_PARAM})") : string.Empty;
			return FormattableString.Invariant($" SELECT {maxRowsExpression} {columns} FROM dbo.StmQueueState {filter}");
		}

		List<(Guid id, Guid? chainId)> LoadPks(DbConnection connection, int dequeueBatchSize)
		{
			const string queueStateParam = "@QueuedState";
			var queryText = FormattableString.Invariant($@"
SELECT SQS_PK, SQS_ChainID 
FROM dbo.StmQueueState 
WHERE SQS_Status = {queueStateParam}
AND --need lock on ChainID if exists
	CASE WHEN SQS_ChainID = '00000000-0000-0000-0000-000000000000' THEN 1 ELSE APPLOCK_TEST('public', '{APP_LOCK_PREFIX}' + convert(nvarchar(50), SQS_ChainID), 'exclusive', 'session') END = 1
AND --need lock on PK always
	APPLOCK_TEST('public', '{APP_LOCK_PREFIX}' + convert(nvarchar(50), SQS_PK), 'exclusive', 'session') = 1
ORDER BY SQS_ParentMessageNumber");

			try
			{
				using (var command = connection.Command(queryText))
				{
					command.AddParameter(MAX_ROWS_PARAM, SqlDbType.Int, dequeueBatchSize);
					command.AddParameter(queueStateParam, SqlDbType.VarChar, QueueStatusCodes.Codes.Queued);
					var result = new List<(Guid, Guid?)>();
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							var chainID = reader.GetGuid(1);
							result.Add((reader.GetGuid(0), Guid.Empty.Equals(chainID) ? null : chainID));
						}
					}
					return result;
				}
			}
			catch (SqlException ex)
			{
				ErrorReporter.ReportOnce("Unexpected exception loading batch.", ex);
				return new List<(Guid, Guid?)>();
			}
		}
		#endregion C:\git\wtg\CargoWise\Dev\Enterprise\Product\Core\Scheduler\GraphEngine\Batching\StmQueueStateFactoryPartial_Batching.cs

		#region move from C:\git\wtg\CargoWise\Dev\Enterprise\Product\Core\Scheduler\GraphEngine\GrEngineDequeuer.cs
		public int Notify(IEnumerable<QueueStateResult<StmQueueState>> batch)
		{
			var modified = 0;
			var groups = batch.GroupBy(g => g.Type);

			foreach (var group in groups)
			{
				switch (group.Key)
				{
					case QueueStateResultType.Failed: // Failure gets marked as success. This is because error handling should be implemented by consumers.
					case QueueStateResultType.Processed:
						modified += UpdateColumn(group.Select(s => s.State), (StmQueueStateSchema.SQS_Status, QueueStatusCodes.Codes.Notify), (StmQueueStateSchema.SQS_ProcessedTimeUtc, ZDateTime.UtcNow.ToDateTime()));
						break;

					case QueueStateResultType.None:
						// Do nothing.
						break;

					default:
						throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Unsupported type {0} for notification", group.Key));
				}
			}

			return modified;
		}

		public int UpdateColumn(IEnumerable<StmQueueState> items, params (SchemaColumn columnName, object value)[] values)
		{
			var builder = new StringBuilder();
			var count = BuildUpdateColumnQuery(builder, items, values);
			Db.Connection.ExecuteNonQuery(builder.ToString());
			return count;
		}

		public int BuildUpdateColumnQuery(StringBuilder builder, IEnumerable<StmQueueState> items, params (SchemaColumn columnName, object value)[] values)
		{
			var counter = 0;
			foreach (var item in items)
			{
				BuildUpdateColumnQuery(builder, item, values);
				counter++;
			}
			return counter;
		}

		public void BuildUpdateColumnQuery(StringBuilder builder, StmQueueState item, params (SchemaColumn columnName, object value)[] values)
		{
			var columnSetters = string.Join(", ", values.Select((value, i) => FormattableString.Invariant($"{value.columnName.Name} = @{i}")));
			var valuesWithPk = values.Append((StmQueueStateSchema.PK, item.Identifier)); // This is the last parameter, and is not part of the parameter list, so we manually declare the parameter in the EXEC statement.
			var columnTypes = string.Join(", ", valuesWithPk.Select((value, i) => FormattableString.Invariant($"@{i} {value.columnName.SqlDbTypeDeclaration}")));
			var parameterBindings = string.Join(", ", valuesWithPk.Select((value, i) => FormattableString.Invariant($"@{i} = '{value.value}'")));
			builder.AppendLine(FormattableString.Invariant($"EXEC sys.sp_executesql N'UPDATE dbo.StmQueueState SET {columnSetters} WHERE SQS_PK = @{values.Length} AND SQS_Status not in (''NTF'', ''PRS'')', N'{columnTypes}', {parameterBindings};"));
		}

		public IEnumerable<StmQueueState> LoadAll(int enqueueBatchSize)
		{
			using (var command = Db.Connection.Command(StmQueueQueryBaseString()))
			{
				StmQueueQueryBaseAddParams(command);
				return LoadStmQueueState(command, enqueueBatchSize);
			}
		}

		public const string CodeParam = "@Code";

		void StmQueueQueryBaseAddParams(DbCommand command)
		{
			command.AddParameter(CodeParam, SqlDbType.Char, ServiceTaskCode);
		}

		string StmQueueQueryBaseString(int? numberOfRows = null)
		{
			var names = string.Join(",", StmQueueState.Columns().Select(c => c.Name));
			return string.Format(CultureInfo.InvariantCulture,
				@"SELECT {0} {1}
FROM dbo.StmQueueState
WHERE SQS_ServiceTaskCode = {2}",
				numberOfRows.HasValue && numberOfRows.Value != int.MaxValue && numberOfRows.Value > 0 ? "TOP " + numberOfRows.Value : string.Empty,
				names,
				CodeParam);
		}

		public IList<StmQueueState> LoadAllUnprocessed(int enqueueBatchSize)
		{
			var paramQueued = "@Queued";
			var paramBlocked = "@Blocked";
			var query = string.Format(CultureInfo.InvariantCulture,
				"{0} AND SQS_Status in ({1}, {2})",
				StmQueueQueryBaseString(),
				paramQueued,
				paramBlocked);

			using (var command = Db.Connection.Command(query))
			{
				StmQueueQueryBaseAddParams(command);
				command.AddParameter(paramQueued, SqlDbType.Char, QueueStatusCodes.Codes.Queued);
				command.AddParameter(paramBlocked, SqlDbType.Char, QueueStatusCodes.Codes.Blocked);

				return LoadStmQueueState(command, enqueueBatchSize);
			}
		}

		public IList<StmQueueState> LoadPreKeys(int enqueueBatchSize, int? maximumRows, ZQuery filter)
		{
			var additionalFilter = filter.FilterString;
			var paramPrekey = "@Prekey";

			var query = string.Format(CultureInfo.InvariantCulture,
				"{0} AND SQS_Status = {1} {2} ORDER BY {3}",
				StmQueueQueryBaseString(maximumRows),
				paramPrekey,
				string.IsNullOrEmpty(additionalFilter) ? additionalFilter : "AND " + additionalFilter,
				StmQueueStateSchema.Constants.SQS_ParentMessageNumber);

			using (var command = Db.Connection.Command(query))
			{
				command.AddParameter(paramPrekey, SqlDbType.Char, QueueStatusCodes.Codes.PreKey);
				foreach (var parameter in filter.Params)
				{
					command.AddParameter(parameter);
				}
				StmQueueQueryBaseAddParams(command);

				return LoadStmQueueState(command, enqueueBatchSize);
			}
		}

		IList<StmQueueState> LoadStmQueueState(DbCommand command, int expectedRowCount)
		{
			return StmQueueState.Load(command, expectedRowCount);
		}

		public IEnumerable<StmQueueState> LoadNotified(int enqueueBatchSize)
		{
			var dateParam = "@ProcessedTime";
			var processedParam = "@Processed";
			var notifyParam = "@Notify";
			var insertedRowidentifier = "inserted.";

			var columns = string.Join(",", StmQueueState.Columns().Select(c => insertedRowidentifier + c.Name));
			var querystring = string.Format(CultureInfo.InvariantCulture, @"UPDATE dbo.StmQueueState SET SQS_Status = {0} OUTPUT {1} WHERE SQS_Status = {2}",
				processedParam,
				columns,
				notifyParam);

			using (var command = Db.Connection.Command(querystring))
			{
				command.AddParameter(processedParam, SqlDbType.Char, QueueStatusCodes.Codes.Processed);
				command.AddParameter(notifyParam, SqlDbType.Char, QueueStatusCodes.Codes.Notify);
				command.AddParameterBasedOnDbColumn(dateParam, ZDateTime.UtcNow.ToDateTime(), StmQueueStateSchema.SQS_ProcessedTimeUtc);
				return StmQueueState.Load(command, enqueueBatchSize);
			}
		}

#if DEBUG
		public void SetProviderThrowExceptionForTestInLoadBatch(bool shouldThrow)
		{
			ShouldThrowExceptionForTestInLoadBatch = shouldThrow;
		}
#endif
		#endregion C:\git\wtg\CargoWise\Dev\Enterprise\Product\Core\Scheduler\GraphEngine\GrEngineDequeuer.cs

		#region move from C:\git\wtg\CargoWise\Dev\Enterprise\Product\Core\Scheduler\GraphEngine\GrEngineEnqueuer.cs
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		public void DeleteOldProcessed(INotifications notifications, int hours)
		{
			var daysParam = "@Days";
			var grEngineCode = "@Code";
			var processedParam = "@Processed";

			var commandSql = string.Format(CultureInfo.InvariantCulture,
				@"DELETE FROM dbo.StmQueueState
WHERE SQS_ProcessedTimeUtc < {0}
AND SQS_SystemCreateTimeUtc < {0}
AND SQS_ServiceTaskCode = {1}
AND SQS_Status = {2}",
				daysParam,
				grEngineCode,
				processedParam);

			using (var command = Db.Connection.Command(commandSql))
			{
				command.AddParameter(daysParam, SqlDbType.DateTime, ZDateTime.UtcNow.AddHours(-hours).ToDateTime());
				command.AddParameter(grEngineCode, SqlDbType.Char, ServiceTaskCode);
				command.AddParameter(processedParam, SqlDbType.Char, QueueStatusCodes.Codes.Processed);

				var rowCount = command.ExecuteNonQuery();
				if (rowCount > 0)
				{
					notifications?.Add(CargoWise.ComponentModel.NotificationType.Information, string.Format(CultureInfo.InvariantCulture, "Deleting {0} queues from {1} hours ago.", rowCount, hours));
				}
			}
		}
		#endregion

		#region move from C:\git\wtg\CargoWise\Dev\Enterprise\Product\Core\Scheduler\GraphEngine\GrEngineImplementation\PreKeyBacklogChecker.cs
		public int CountBacklog(string stmQueueStatus, GrEngineEnums.Status includeOrExclude)
		{
			const string status = "@status";
			const string code = "@code";
			using (var cmd = Db.Connection.Command($"select count(*) from dbo.StmQueueState where SQS_Status {(includeOrExclude == GrEngineEnums.Status.Include ? "=" : "<>")} @status and SQS_ServiceTaskCode = @code"))
			{
				cmd.AddParameter(status, StmQueueStateSchema.SQS_Status.SqlDbType, stmQueueStatus);
				cmd.AddParameter(code, StmQueueStateSchema.SQS_ServiceTaskCode.SqlDbType, ServiceTaskCode);
				return (int)cmd.ExecuteScalar();
			}
		}
		#endregion

		#region move from C:\git\wtg\CargoWise\Dev\Enterprise\Product\Core\Scheduler\GraphEngine\StmQueueState\StmQueueState.cs
		public void Update(IEnumerable<StmQueueState> queues)
		{
			var builder = new StringBuilder();
			var chainIdChangedDictionary = new Dictionary<Guid, List<StmQueueState>>();
			var statusChangedDictionary = new Dictionary<string, List<StmQueueState>>();
			var syncedList = new List<StmQueueState>();
			var batchSize = eAdaptorRegistry.Instance.StmQueueStateUpdateBatchSize.Value;
			var batchRecordCounter = 0;

			foreach (var queueState in queues)
			{
				if (queueState.ChainIdChanged)
				{
					chainIdChangedDictionary.GetOrAdd(queueState.ChainID, () => new List<StmQueueState>()).Add(queueState);
				}
				if (queueState.StatusChanged)
				{
					statusChangedDictionary.GetOrAdd(queueState.Status, () => new List<StmQueueState>()).Add(queueState);
				}
			}
			Notifications?.AddInfo(FormattableString.Invariant($"chainIdChangedDictionary ({chainIdChangedDictionary.Count})"));
			Notifications?.AddInfo(FormattableString.Invariant($"statusChangedDictionary ({statusChangedDictionary.Count})"));

			foreach (var queueGroup in chainIdChangedDictionary)
			{
				foreach (var item in queueGroup.Value)
				{
					BuildUpdateColumnQuery(builder, item, (StmQueueStateSchema.SQS_ChainID, queueGroup.Key));
					batchRecordCounter++;
					syncedList.Add(item);

					if (batchRecordCounter == batchSize)
					{
						ExecuteCommand();
					}
				}
			}
			Notifications?.AddInfo(FormattableString.Invariant($"End of chainIdChangedDictionary loop"));

			foreach (var queueGroup in statusChangedDictionary)
			{
				foreach (var item in queueGroup.Value)
				{
					BuildUpdateColumnQuery(builder, item, (StmQueueStateSchema.SQS_Status, queueGroup.Key));
					batchRecordCounter++;
					syncedList.Add(item);

					if (batchRecordCounter == batchSize)
					{
						ExecuteCommand();
					}
				}
			}
			Notifications?.AddInfo(FormattableString.Invariant($"End of statusChangedDictionary loop"));

			ExecuteCommand();

			void ExecuteCommand()
			{
				Notifications?.AddInfo(FormattableString.Invariant($"Start of ExecuteCommand"));
				if (batchRecordCounter == 0)
				{
					return;
				}

				Db.Connection.ExecuteNonQuery(builder.ToString());
				Notifications?.AddInfo(FormattableString.Invariant($"ExecuteCommand ({batchRecordCounter})"));
				syncedList.ForEach(queuedState => queuedState.SetAsDatabaseSynced());

				builder.Clear();
				syncedList.Clear();
				batchRecordCounter = 0;
				Notifications?.AddInfo(FormattableString.Invariant($"End of ExecuteCommand"));
			}
		}

		public void InsertAsQueued(IEnumerable<StmQueueState> queues)
		{
			Notifications?.AddInfo(FormattableString.Invariant($"Start of InsertAsQueued"));
			if (queues.Any())
			{
				var syncedList = new List<StmQueueState>();
				using (var table = new DataTable(StmQueueStateSchema.Constants.TableName))
				{
					table.Locale = CultureInfo.InvariantCulture;
					foreach (var col in StmQueueStateSchema.All)
					{
						table.Columns.Add(col.Name, col.DotNetType);
					}

					var now = ZDateTime.UtcNow.ToDateTime();
					foreach (var queue in queues)
					{
						var row = table.NewRow();
						row[StmQueueStateSchema.Constants.PK] = queue.Identifier;
						row[StmQueueStateSchema.Constants.SQS_ParentID] = queue.ParentID;
						row[StmQueueStateSchema.Constants.SQS_ParentTableCode] = queue.TableCode;
						row[StmQueueStateSchema.Constants.SQS_ParentMessageNumber] = queue.ParentMessageNumber;
						row[StmQueueStateSchema.Constants.SQS_ServiceTaskCode] = ServiceTaskCode;
						row[StmQueueStateSchema.Constants.SQS_Keys] = string.Join(StmQueueState.Delimiter, queue.Keys);
						row[StmQueueStateSchema.Constants.SQS_SystemCreateTimeUtc] = now;
						row[StmQueueStateSchema.Constants.SQS_SystemCreateUser] = GlbStaff.CurrentUser.GS_Code;
						row[StmQueueStateSchema.Constants.SQS_ChainID] = queue.ChainID;
						row[StmQueueStateSchema.Constants.SQS_Status] = queue.Status;
						table.Rows.Add(row);
						syncedList.Add(queue);
					}
					Notifications?.AddInfo(FormattableString.Invariant($"InsertAsQueued syncedList ({syncedList.Count})"));

					SaveDataTable(table);
					Notifications?.AddInfo(FormattableString.Invariant($"InsertAsQueued SaveDataTable)"));
				}

				syncedList.ForEach(queuedState => queuedState.SetAsDatabaseSynced());
			}
			Notifications?.AddInfo(FormattableString.Invariant($"End of InsertAsQueued"));
		}

		static void SaveDataTable(DataTable table)
		{
			using (var dataSet = new DataSet())
			{
				dataSet.Locale = CultureInfo.InvariantCulture;
				dataSet.Tables.Add(table);

				var saver = new ZSqlSaver(dataSet, Db.Connection, ObjectFactory.Get<IApplicationSchemaResolver>());
				saver.Save();
			}
		}
		#endregion

		#region move from C:\git\wtg\CargoWise\Dev\Enterprise\Product\Core\Scheduler\GraphEngine\QueryExtensions.cs
		public void AddFilterOutAlreadyQueuedItems(ZDBOnlyQuery query)
		{
			var parameters = new ZSqlParameterCollection();
			query.AddFilterAndZSQLParameterCollection(
				string.Format(CultureInfo.InvariantCulture, "{0} not in (select SQS_ParentID from dbo.StmQueueState where SQS_ParentID is not null and SQS_Status <> '{1}')",
					query.PKColumn.Name,
					QueueStatusCodes.Codes.Processed),
				parameters);
		}

		#endregion
	}
	#endregion
}
