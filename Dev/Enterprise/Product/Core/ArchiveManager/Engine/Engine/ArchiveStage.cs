using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ArchiveManager.Integration;
using Enterprise.Billing.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Engine
{
	public class ArchiveStage : IArchiveStage, IArchiveSystemSetup
	{
		public ArchiveStage(IArchiveStageDescriptor descriptor, IArchiveSystemDescriptor systemDescriptor)
		{
			this.descriptor = descriptor;
			this.systemDescriptor = systemDescriptor;
			ArchiveableRelationships = new Dictionary<string, List<ArchiveableRelationship>>();
			mainArchiveableType = new ArchiveableType(descriptor.MainArchivePKColumn, descriptor.MainArchiveNKColumn);
			IncludeTimeTakenInTheARCLogs = SystemDataRegistry.Instance.IncludeTimeTakenInTheARCLogs.Value;
		}

		public string Name
				=> descriptor.Name;

		protected enum ReasonsForSkipping
		{
			AlreadyLoaded,
			LoadedNothing,
			HeavyLockContention,
			ArchivingFailed,
		}

		public virtual void BeginRun(IArchiveConfiguration config, IArchiveSchedule schedule, IArchiveLogger logger)
		{
			_config = config;
			_schedule = schedule;
			_logger = logger;

			descriptor.Setup(this, schedule, config);

			if (descriptor.IsStageUsingTempTables)
			{
				ArchiveTableHelper.CreateTempTables(this);
			}

			Initialised = true;
		}

		public void LogEndOfStageSummaries()
		{
			LogSummaryOfNumberLoadedAndTimeTaken();
			LogSummaryOfNumberDeletedAndTimeTaken();
			LogSummaryOfDocumentsGeneratedAndTimeTaken();
		}

		protected virtual void LogSummaryOfNumberLoadedAndTimeTaken()
		{
			_logger.LogInfo(systemDescriptor.Code, $"Time taken to load {totalNumberOfArchiveSetsLoaded} Archive Set(s) of {mainArchiveableType.PKColumn.TableName} " +
				$"record(s) in {totalNumberOfBatchesLoaded} batch(es): {timeTakenToLoadAllArchiveSets}ms.");
		}

		protected virtual void LogSummaryOfNumberDeletedAndTimeTaken()
		{
			var totalTimeTakenToDelete = allLoadedArchiveSets.Sum(set => set.TimeTakenToDelete);
			_logger.LogInfo(systemDescriptor.Code, $"Time taken to {systemDescriptor.Noun.GetUnresolvedString().ToLower()} {totalNumberOfArchiveSetsLoaded} {mainArchiveableType.PKColumn.TableName} record(s) " +
				$"and their related record(s): " +
				$"{totalTimeTakenToDelete}ms");
		}

		protected virtual void LogSummaryOfDocumentsGeneratedAndTimeTaken()
		{
			return;
		}

		public virtual void EndRun()
		{
			if (IncludeTimeTakenInTheARCLogs)
			{
				LogEndOfStageSummaries();
			}

			_config.AMUsageReportValues[UsageProperties.TotalMissingDocumentsGeneratedDuringArchiving] = NumberOfDocumentsGenerated;
			_config.AMUsageReportValues[UsageProperties.TotalMainRecordLoaded] = NumberOfMainRecordsLoaded;

			descriptor.Finalise(systemDescriptor, _logger, _schedule, _config);

			Initialised = false;
		}

		public virtual int BatchSize
			=> SystemDataRegistry.Instance.BatchSizeControl.Value;

		[Serializable]
		public class NotInitialisedException : Exception
		{
			public NotInitialisedException(string errorMessage) : base(errorMessage)
			{ }

#if NETFRAMEWORK
			protected NotInitialisedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
			{ }
#endif
		}

		protected long timeTakenToLoadAllArchiveSets;

		protected int totalNumberOfArchiveSetsLoaded;

		protected int totalNumberOfBatchesLoaded;

		protected List<ArchiveSet> allLoadedArchiveSets = new List<ArchiveSet>();

		protected virtual int NumberOfBatchesToLoadPerDriverRun => 20;

		public static TimeSpan TimeToWaitBeforeFlushingQueue => TimeSpan.FromHours(12);

		protected enum StageStates
		{
			Running,
			NoMoreRecords,
			ShutdownDueToLockContention,
			Error
		}

		protected StageStates state = StageStates.Running;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods", Justification = "Queue Tables Have No BizO")]
		void RunDriverIfNecessary(IArchiveWatermark watermark, IArchiveSchedule schedule, IArchiveStage stage)
		{
			int numberOfRowsLoaded;
			var schedulePk = schedule?.SchedulePK ?? Guid.Empty;
			var schedulePKString = schedulePk != Guid.Empty
				? (NoResString)"AIM_S5_ParentSchedule = @SchedulePK" : (NoResString)"AIM_S5_ParentSchedule IS NULL";

			var countNumberOfItemsInQueueSQLThatAreNotAlreadyBeingDealtWith = @$"
SELECT COUNT(*) FROM dbo.ArchiveMainItemQueue WHERE {schedulePKString} AND AIM_StageName = @StageName
AND AIM_IsLoading = 0 AND AIM_IsLoaded = 0 AND AIM_IsSkipped = 0;";

			try
			{
				using (var cmd = Db.Connection.Command(countNumberOfItemsInQueueSQLThatAreNotAlreadyBeingDealtWith))
				{
					if (schedulePk != Guid.Empty)
					{
						cmd.AddParameter("@SchedulePK", SqlDbType.UniqueIdentifier, schedulePk);
					}

					cmd.AddParameter("@StageName", SqlDbType.NVarChar, 125, StageNameForQueries);
					var result = (cmd.ExecuteScalar() as int?).Value;

					if (result > 0)
					{
						return;
					}
				}
			}
			catch (SqlException ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("ArchiveDriverPreLoadException", $"An exception occurred while attempting to " +
					$"perform pre-load operations for the ARC driver. Stage Name: {stage.Name}. Exception Message: {ex.Message}.");
				throw;
			}

			var mainArchiveableTypeFilter = GetMainBusinessObjectFilter(_config);
			var loadMainRecordQuery = new ZQuery(mainArchiveableTypeFilter);
			var queryBuilder = new StringBuilder(300);

			var pksToIgnoreFilter = @$"{mainArchiveableType.PKColumn.Name}
NOT IN (SELECT AIM_ParentID FROM dbo.ArchiveMainItemQueue)";
			var mainArchiveNKString = mainArchiveableType.NKColumn != null ? mainArchiveableType.NKColumn.Name : "''";
			var schedulePKValueToInsert = schedulePk != Guid.Empty
				? "@SchedulePK" : "NULL";

			_ = queryBuilder.Append(
@$"INSERT INTO dbo.ArchiveMainItemQueue (AIM_PK, AIM_ParentTableCode, AIM_ParentID, AIM_ParentNK, AIM_MainArchiveDate,
AIM_S5_ParentSchedule, AIM_StageName, AIM_SystemCreateTimeUtc, AIM_SystemCreateUser,
AIM_SystemLastEditTimeUtc, AIM_SystemLastEditUser)
SELECT TOP {NumberOfBatchesToLoadPerDriverRun * BatchSize} newid(), '{mainArchiveableType.PKColumn.ColumnPrefix}',
{mainArchiveableType.PKColumn.Name}, {mainArchiveNKString}, {descriptor.MainDateFilterColumn.Name}, {schedulePKValueToInsert}, @StageName, GetUtcDate(),
'~BP', GetUtcDate(), '~BP'
FROM dbo.{mainArchiveableType.PKColumn.TableName} WHERE ({pksToIgnoreFilter}) AND ");

			AppendWatermarkFilterToQuery(watermark, queryBuilder);

			_ = queryBuilder.Append(loadMainRecordQuery.LiteralTextSqlFormatted);

			if (!string.IsNullOrEmpty(loadMainRecordQuery.OrderBy))
			{
				_ = queryBuilder.Append((NoResString)" ORDER BY " + loadMainRecordQuery.OrderBy);
			}

			_ = queryBuilder.Append(";");

			if (schedule != null)
			{
				_ = queryBuilder.AppendLine(@$"
SELECT TOP 1 @newWatermarkDate = AIM_MainArchiveDate, @newWatermarkNK = AIM_ParentNK, @newWatermarkPK = AIM_ParentID
FROM dbo.ArchiveMainItemQueue WHERE {schedulePKString} AND AIM_StageName = @StageName
AND AIM_IsLoading = 0 AND AIM_IsLoaded = 0 AND AIM_IsSkipped = 0
ORDER BY AIM_MainArchiveDate DESC, AIM_ParentNK DESC, AIM_ParentID DESC
");
			}

			var mainQuery = string.Empty;
			try
			{
				using (var command = Db.Connection.Command(queryBuilder.ToString()))
				{
					AddWatermarkParametersToQuery(watermark, command);

					if (schedulePk != Guid.Empty)
					{
						command.AddParameter("@SchedulePK", SqlDbType.UniqueIdentifier, schedulePk);
					}

					command.AddParameter("@StageName", SqlDbType.NVarChar, 125, StageNameForQueries);

					if (schedule != null)
					{
						command.AddOutputParameter("@newWatermarkDate", SqlDbType.DateTime, 8, 0, 0, DBNull.Value);
						command.AddOutputParameter("@newWatermarkNK", SqlDbType.NVarChar, 125, 0, 0, DBNull.Value);
						command.AddOutputParameter("@newWatermarkPK", SqlDbType.UniqueIdentifier, 36, 0, 0, DBNull.Value);
					}

					mainQuery = command.CommandText;
					command.CommandTimeout = LoadArchiveSetBatchTimeout * 60;

					Stopwatch sw = null;
					if (IncludeTimeTakenInTheARCLogs)
					{
						sw = new Stopwatch();
						sw.Start();
					}

					numberOfRowsLoaded = command.ExecuteNonQuery();

					if (IncludeTimeTakenInTheARCLogs)
					{
						sw.Stop();
						timeTakenToLoadAllArchiveSets += sw.ElapsedMilliseconds;
						totalNumberOfArchiveSetsLoaded += numberOfRowsLoaded;
						totalNumberOfBatchesLoaded += (numberOfRowsLoaded / BatchSize) + (numberOfRowsLoaded % BatchSize != 0 ? 1 : 0);
					}

					if (schedule != null)
					{
						Helpers.SetWatermarkUsingQueryResults(schedule, stage.Name, command, _config);
					}

					_logger.LogInfo(systemDescriptor.Code, $"Loaded next batch of {numberOfRowsLoaded} {mainArchiveableType.PKColumn.TableName}{Helpers.GetTimeTaken(sw)}");
				}
			}
			catch (SqlException ex) when (!ex.IsCriticalException())
			{
				if (new DbErrorMatch(ex).ExceptionType == DbErrorType.TimeoutExpired)
				{
					var message = string.IsNullOrEmpty(mainQuery)
						? (NoResString)"Timed out while retrieving the next ArchiveSet"
						: (NoResString)"Timed out while retrieving the next ArchiveSet, the query is: " + mainQuery;

					_logger.LogAndReportError("ArchiveStageBatchRetrievalTimeout", systemDescriptor.Code, message, ex);
				}

				throw;
			}

			return;
		}

		public TimeSpan TotalWaitingTimeSpanForDriverLock
		{
			get
			{
				return totalWaitingTimeSpanForDriverLock;
			}
			set
			{
				totalWaitingTimeSpanForDriverLock = value;
			}
		}

		TimeSpan totalWaitingTimeSpanForDriverLock = TimeSpan.FromMinutes(7.5);

		public static TimeSpan GetTimeToWaitForDriverLock(TimeSpan timeSpentWaitingSoFar)
		{
			if (timeSpentWaitingSoFar < TimeSpan.FromSeconds(1))
			{
				return TimeSpan.FromMilliseconds(100);
			}
			else if (timeSpentWaitingSoFar < TimeSpan.FromMinutes(1))
			{
				return TimeSpan.FromSeconds(3);
			}
			else if (timeSpentWaitingSoFar < TimeSpan.FromMinutes(5))
			{
				return TimeSpan.FromSeconds(10);
			}
			else
			{
				return TimeSpan.FromSeconds(1);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods", Justification = "Querying queue table for which no bizo exists")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public virtual IEnumerable<IArchiveSet> GetNextArchiveSet(IArchiveWatermark watermark, IArchiveSchedule schedule, IArchiveStage stage)
		{
			var archiveSets = new List<ArchiveSet>();

			var currentWaitingTimeSpan = TimeSpan.FromSeconds(0);
			SqlApplicationLock runDriverLock = null;
			var obtainLockResult = false;

			while (currentWaitingTimeSpan < totalWaitingTimeSpanForDriverLock)
			{
				obtainLockResult = Db.Connection.TryGetLock("ArchiveManagerRunDriverLock", out runDriverLock);

				if (obtainLockResult)
				{
					break;
				}
				else
				{
					var nextWaitTime = GetTimeToWaitForDriverLock(currentWaitingTimeSpan);
					currentWaitingTimeSpan += nextWaitTime;
					Thread.Sleep(nextWaitTime);
				}
			}

			if (!obtainLockResult)
			{
				_logger.LogWarning("Archive Manager has detected heavy lock contention with other instances of itself and will not start any" +
					" new processes for the current schedule.");
				return archiveSets;
			}

			using (runDriverLock)
			{
				RunDriverIfNecessary(watermark, schedule, stage);

				var schedulePk = schedule?.SchedulePK ?? Guid.Empty;
				var schedulePKString = schedulePk != Guid.Empty
					? (NoResString)"AIM_S5_ParentSchedule = @SchedulePK" : (NoResString)"AIM_S5_ParentSchedule IS NULL";
				var selectTopBatchFromQueueQuery = $@"
SELECT TOP ({BatchSize}) AIM_ParentID, AIM_ParentNK FROM dbo.ArchiveMainItemQueue WHERE
{schedulePKString} AND AIM_StageName = @StageName AND
AIM_IsLoading = 0 AND AIM_IsLoaded = 0;";

				var updateLoadedItemsSQL = $@"
UPDATE dbo.ArchiveMainItemQueue SET AIM_IsLoading = 1, AIM_SystemLastEditTimeUtc = GETUTCDATE(),
AIM_SystemLastEditUser = '~BP' WHERE {schedulePKString} AND AIM_StageName = @StageName AND 
AIM_ParentID in (select value from @tvp)";

				using (var cmd = Db.Connection.Command(selectTopBatchFromQueueQuery))
				{
					if (schedulePk != Guid.Empty)
					{
						cmd.AddParameter("@SchedulePK", SqlDbType.UniqueIdentifier, schedule.SchedulePK);
					}

					cmd.AddParameter("@StageName", SqlDbType.NVarChar, 125, StageNameForQueries);

					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							var pk = reader.GetGuid(0);
							var nk = reader.GetString(1);

							ArchiveSet set;
							if (pk != Guid.Empty)
							{
								var mainArchiveItem = new ArchiveItem(mainArchiveableType.PKColumn, pk, null, Guid.Empty, false, mainArchiveableType.PKColumn.ColumnPrefix);
								set = new ArchiveSet(systemDescriptor, StageNameForQueries, schedule?.SchedulePK ?? Guid.Empty, mainArchiveItem, mainArchiveableType, GetMainBusinessObjectFilter(_config), nk);
								archiveSets.Add(set);
							}
						}
					}
				}

				if (archiveSets.Count > 0)
				{
					using (var cmd = Db.Connection.Command(updateLoadedItemsSQL))
					{
						if (schedulePk != Guid.Empty)
						{
							cmd.AddParameter("@SchedulePK", SqlDbType.UniqueIdentifier, schedule.SchedulePK);
						}

						cmd.AddParameter("@StageName", SqlDbType.NVarChar, 125, StageNameForQueries);
						cmd.AddTableValuedParameter((NoResString)"@tvp", "dbo.TVP_uniqueidentifier", archiveSets.Select(a => a.MainArchiveItem.PK));

						cmd.ExecuteNonQuery();
					}
				}
			}

			if (IncludeTimeTakenInTheARCLogs)
			{
				allLoadedArchiveSets.AddRange(archiveSets);
			}

			if (archiveSets.Count == 0)
			{
				state = StageStates.NoMoreRecords;
			}

			return archiveSets;
		}

		protected void AppendWatermarkFilterToQuery(IArchiveWatermark watermark, StringBuilder queryBuilder)
		{
			if (watermark != null)
			{
				if (descriptor.MainArchiveNKColumn != null)
				{
					_ = queryBuilder.Append($"(({descriptor.MainDateFilterColumn.Name} > @watermarkDate) OR " +
						$"({descriptor.MainDateFilterColumn.Name} = @watermarkDate AND {descriptor.MainArchiveNKColumn.Name} > @watermarkNK) OR " +
						$"({descriptor.MainDateFilterColumn.Name} = @watermarkDate AND {descriptor.MainArchiveNKColumn.Name} = @watermarkNK " +
						$"AND {descriptor.MainArchivePKColumn.Name} > @watermarkPK))");
				}
				else
				{
					_ = queryBuilder.Append($"(({descriptor.MainDateFilterColumn.Name} > @watermarkDate) OR " +
						$"({descriptor.MainDateFilterColumn.Name} = @watermarkDate AND {descriptor.MainArchivePKColumn.Name} > @watermarkPK))");
				}

				queryBuilder.AppendLine(" AND");
			}
		}

		protected void AddWatermarkParametersToQuery(IArchiveWatermark watermark, DbCommand command)
		{
			if (watermark != null)
			{
				if (descriptor.MainArchiveNKColumn != null)
				{
					command.AddParameter("@watermarkNK", System.Data.SqlDbType.NVarChar, 128, watermark.WatermarkNK);
				}
				var watermarkDate = watermark.WatermarkDate.IsValid ? watermark.WatermarkDate : ZDateTime.MinSmallDateTimeValue;
				command.AddParameter("@watermarkDate", System.Data.SqlDbType.DateTime, watermarkDate);
				command.AddParameter("@watermarkPK", System.Data.SqlDbType.UniqueIdentifier, watermark.WatermarkPK);
			}
		}

		protected int NumberOfMainRecordsLoaded { get; set; }

		public TimeSpan SleepSpanBeforeGetNextArchiveSetRetry { get; set; } = TimeSpan.FromSeconds(30);

		public virtual bool ExecuteStage(IArchiveSystem system, IArchiveStage stage, IArchiveConfiguration config, IArchiveLogger logger, IArchiveSchedule schedule, CancellationToken token/*, ref bool forcedStop*/)
		{
			try
			{
				BeginRun(config, schedule, logger);

				do
				{
					var archiveSets = Enumerable.Empty<IArchiveSet>();
					var numberOfTimesToRetryBeforeGivingUp = 3;
					while (true)
					{
						if (CheckForStoppingCondition(token, logger))
						{
							return false;
						}
						try
						{
							archiveSets = GetNextArchiveSet(schedule.GetWatermark(stage.Name), schedule, stage);
							break;
						}
						catch (Exception e) when (!e.IsCriticalException())
						{
							if (numberOfTimesToRetryBeforeGivingUp <= 0)
							{
								throw;
							}
							else
							{
								Thread.Sleep(SleepSpanBeforeGetNextArchiveSetRetry);
								numberOfTimesToRetryBeforeGivingUp--;
							}
						}
					}

					NumberOfMainRecordsLoaded += archiveSets.Count();

					if (!archiveSets.Any())
					{
						return state == StageStates.NoMoreRecords;
					}

					foreach (var archiveSet in archiveSets)
					{
						ExecuteForArchiveSet(system, stage, config, logger, archiveSet);
					}
				}
				while (true);
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				state = StageStates.Error;

				var errorMessage = $"Error encountered during {system.Descriptor.PresentTenseVerb.GetUnresolvedString().ToLower()}. Run aborted.";
				logger.LogAndReportError("ArchiveStage.ExecuteStageException", systemDescriptor.Code, errorMessage, e);

				return false;
			}
			finally
			{
				EndRun();
			}
		}

		protected bool CheckForStoppingCondition(CancellationToken token, IArchiveLogger logger)
		{
			string result = null;

			if (token.IsCancellationRequested)
			{
				result = $"Information|{systemDescriptor.Code}|{Name} was stopped by a cancellation request.";
			}
			else if (ZDateTime.UtcNow > _config.StopRunTime)
			{
				result = $"Information|{systemDescriptor.Code}|Max Run Duration reached for {systemDescriptor.Name}.";
			}

			if (result != null)
			{
				logger.LogInfo(result);
			}

			return result != null;
		}

		public int NumberOfDocumentsGenerated { get; private set; }

		public TimeSpan TotalWaitingTimeSpanForOverlapLock
		{
			get
			{
				return totalWaitingTimeSpanForOverlapLock;
			}
			set
			{
				totalWaitingTimeSpanForOverlapLock = value;
			}
		}

		TimeSpan totalWaitingTimeSpanForOverlapLock = TimeSpan.FromSeconds(90);

		public static TimeSpan GetTimeToWaitForOverlapLock(TimeSpan timeWaitingSoFar)
		{
			if (timeWaitingSoFar < TimeSpan.FromSeconds(1))
			{
				return TimeSpan.FromMilliseconds(50);
			}
			else if (timeWaitingSoFar < TimeSpan.FromSeconds(10))
			{
				return TimeSpan.FromMilliseconds(300);
			}
			else if (timeWaitingSoFar < TimeSpan.FromSeconds(45))
			{
				return TimeSpan.FromMilliseconds(1500);
			}
			else
			{
				return TimeSpan.FromMilliseconds(200);
			}
		}

		public void ExecuteForArchiveSet(IArchiveSystem system, IArchiveStage stage, IArchiveConfiguration config, IArchiveLogger logger, IArchiveSet archiveSet)
		{
			using (Db.DisposableActionForDbConnection())
			using (Logs.SuppressFiringWorkflow())
			using (ProcessTask.Loader.SuppressTemplateApplication(true))
			{
				var branch = GlbBranch.GetOneActiveBranchPerCompany().FirstOrDefault();
				using (branch != null ? DisposableEnvironment.ForBranch(branch.PK.ToGuid()) : null)
				{
					Stopwatch sw = null;
					if (IncludeTimeTakenInTheARCLogs)
					{
						sw = new Stopwatch();
						sw.Start();
					}

					var stepResult = archiveSet.Load(logger);

					if (IncludeTimeTakenInTheARCLogs)
					{
						sw.Stop();
					}

					if (stepResult.ErrorsEncountered.Count == 0)
					{
						if (archiveSet.Count == 0)
						{
							LogDetailsOfSkippedJobHeader(system, config, logger, archiveSet, ReasonsForSkipping.LoadedNothing);
							archiveSet.MarkToIgnoreForThisRun();
							return;
						}

						var waitingTimeSoFarTimeSpan = TimeSpan.FromSeconds(0);
						SqlApplicationLock checkForOverlapLock = null;
						var obtainLockResult = false;

						while (waitingTimeSoFarTimeSpan < totalWaitingTimeSpanForOverlapLock)
						{
							obtainLockResult = Db.Connection.TryGetLock("ArchiveManagerCheckForOverlapLock", out checkForOverlapLock);

							if (obtainLockResult)
							{
								break;
							}
							else
							{
								var nextWaitTime = GetTimeToWaitForOverlapLock(waitingTimeSoFarTimeSpan);
								waitingTimeSoFarTimeSpan += nextWaitTime;
								Thread.Sleep(nextWaitTime);
							}
						}

						if (!obtainLockResult)
						{
							LogGeneralReasonForSkipping(system, logger, archiveSet, ReasonsForSkipping.HeavyLockContention);
							archiveSet.MarkToIgnoreForThisRun();
							return;
						}

						using (checkForOverlapLock)
						{
							if (CheckForOverlap(archiveSet))
							{
								LogDetailsOfSkippedJobHeader(system, config, logger, archiveSet, ReasonsForSkipping.AlreadyLoaded);
								archiveSet.MarkToIgnoreForThisRun();
								return;
							}
							else
							{
								ArchiveTableHelper.MarkArchiveSetAsFullyLoaded(archiveSet);
							}
						}

						LogLoadedArchiveSetDetails(system, config, logger, archiveSet, sw);

						var archiveToImageResult = stage.ArchiveToImages(archiveSet);
						NumberOfDocumentsGenerated += archiveSet.TotalNumberOfDocumentsGeneratedInSet;

						if (archiveToImageResult.ErrorsEncountered.Count > 0)
						{
							archiveSet.MarkToIgnoreForThisRun();
						}
						else
						{
							descriptor.OnArchiveSetProcessed(archiveSet);
						}
					}
				}
			}
		}

		protected virtual bool CheckForOverlap(IArchiveSet archiveSet)
		{
			return ArchiveTableHelper.IsAnyArchiveItemAlreadyLoaded(archiveSet);
		}

		public virtual void LogLoadedArchiveSetDetails(IArchiveSystem system, IArchiveConfiguration config, IArchiveLogger logger, IArchiveSet archiveSet, Stopwatch sw = null)
		{
			StringBuilder infoBuilder;
			if (!config.IsVerboseLog)
			{
				var recordDictionary = new Dictionary<string, int>();
				foreach (var archiveItem in archiveSet.GetArchiveItems())
				{
					if (recordDictionary.ContainsKey(archiveItem.PKColumn.TableName))
					{
						recordDictionary[archiveItem.PKColumn.TableName]++;
					}
					else
					{
						recordDictionary.Add(archiveItem.PKColumn.TableName, 1);
					}
				}

				infoBuilder = new StringBuilder(recordDictionary.Count * 30);
				foreach (var key in recordDictionary.Keys)
				{
					_ = infoBuilder.AppendLine(key + ": " + recordDictionary[key].ToString(CultureInfo.InvariantCulture));
				}
			}
			else
			{
				var recordDictionaryForVerboseLog = new Dictionary<string, List<string>>();
				foreach (var archiveItem in archiveSet.GetArchiveItems())
				{
					if (recordDictionaryForVerboseLog.ContainsKey(archiveItem.PKColumn.TableName))
					{
						recordDictionaryForVerboseLog[archiveItem.PKColumn.TableName].Add(archiveItem.HumanReadableName);
					}
					else
					{
						var list = new List<string>();
						list.Add(archiveItem.HumanReadableName);
						recordDictionaryForVerboseLog.Add(archiveItem.PKColumn.TableName, list);
					}
				}

				infoBuilder = new StringBuilder(recordDictionaryForVerboseLog.Count * 30);
				foreach (var key in recordDictionaryForVerboseLog.Keys)
				{
					_ = infoBuilder.AppendLine(key + ": " + recordDictionaryForVerboseLog[key].Count.ToString(CultureInfo.InvariantCulture));
					for (int i = 0; i < recordDictionaryForVerboseLog[key].Count; i++)
					{
						if (!string.IsNullOrEmpty(recordDictionaryForVerboseLog[key][i]))
						{
							_ = infoBuilder.AppendLine(recordDictionaryForVerboseLog[key][i]);
						}
					}
				}
			}
			LogDetails(system, logger, archiveSet, sw, infoBuilder);
		}

		protected virtual void LogDetails(IArchiveSystem system, IArchiveLogger logger, IArchiveSet archiveSet, Stopwatch sw, StringBuilder infoBuilder)
		{
			var archiveItemCode = GetArchiveItemCodeWithFallback(archiveSet);
			logger.LogInfo(system.Descriptor.Code, $"Loaded {archiveSet.MainArchiveItem.PKColumn.TableName} '{archiveItemCode}' and {archiveSet.Count - 1} related records{Helpers.GetTimeTaken(sw)}{System.Environment.NewLine}{infoBuilder}");
		}

		protected virtual void LogDetailsOfSkippedJobHeader(IArchiveSystem system, IArchiveConfiguration config, IArchiveLogger logger, IArchiveSet archiveSet, ReasonsForSkipping reason)
		{
			if (config.IsVerboseLog && archiveSet.IgnoredPK != Guid.Empty)
			{
				var factory = new BusinessObjectFactory();
				var mainQuery = new ZQuery(JobHeaderSchema.PK, archiveSet.IgnoredPK);
				_ = mainQuery.AddToFilter(JobHeaderSchema.JH_Status, SQLComparisonOperator.NotEqual, "CLS");
				var jobHeaderWhichCauseSkipping = factory.Load<JobHeader>(mainQuery).FirstOrDefault();
				if (jobHeaderWhichCauseSkipping != null)
				{
					string childName = jobHeaderWhichCauseSkipping.JH_JobNum;
					string childType = jobHeaderWhichCauseSkipping.JH_ParentTableCode;

					var companyGuid = jobHeaderWhichCauseSkipping.JH_GC;
					var company = factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, companyGuid));
					var companyCode = company.GC_Code;

					switch (childType)
					{
						case "JS":
							childType = (NoResString)"Job Shipment";
							break;
						case "JK":
							childType = (NoResString)"Job Consol";
							break;
						case "JE":
							childType = (NoResString)"Job Declaration";
							break;
						case "JJ":
							childType = (NoResString)"Job Cartage";
							break;
					}

					var archiveItemCode = GetArchiveItemCodeWithFallback(archiveSet);
					var childRecord = $"{childType} '{childName}'";
					logger.LogInfo(system.Descriptor.Code, $"Skipped {archiveSet.MainArchiveItem.PKColumn.TableName} with Code '{archiveItemCode}' because it was already processed as a part of another dataset. Please verify that the Job Status for {childRecord} under all login Companies is closed [This item has been found under company code : {companyCode}] and that the Accounting period and General Ledger periods are also closed");
				}
				else
				{
					LogGeneralReasonForSkipping(system, logger, archiveSet, reason);
				}
			}
			else
			{
				LogGeneralReasonForSkipping(system, logger, archiveSet, reason);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Logs do not need to be translated")]
		protected virtual void LogGeneralReasonForSkipping(IArchiveSystem system, IArchiveLogger logger, IArchiveSet archiveSet, ReasonsForSkipping reason)
		{
			var archiveItemCode = GetArchiveItemCodeWithFallback(archiveSet);
			var baseMessage = $"Skipped {archiveSet.MainArchiveItem.PKColumn.TableName} with Code '{archiveItemCode}', ";

			switch (reason)
			{
				case ReasonsForSkipping.AlreadyLoaded:
					logger.LogInfo(system.Descriptor.Code, baseMessage + (NoResString)"because it is already loaded as part of another archive set.");
					break;

				case ReasonsForSkipping.LoadedNothing:
					logger.LogInfo(system.Descriptor.Code, baseMessage + (NoResString)"because one of its related records did not meet archiving criteria.");
					break;

				case ReasonsForSkipping.HeavyLockContention:
					var lockContentionMessage = "Archive Manager has detected heavy lock contention with other instances of itself while processing " +
						$"{archiveSet.MainArchiveItem.PKColumn.TableName} with code '{archiveItemCode}'. This set has been " +
						"skipped for the time being.";
					logger.LogWarning(system.Descriptor.Code, lockContentionMessage);
					break;
			}
		}

		public virtual IArchiveStepResult ArchiveToImages(IArchiveSet archiveSet)
		{
			var stepResult = new ArchiveStepResult();

			var preparationActions = new List<IArchivePreparationAction>();
			var prepActionEnumerable = descriptor.GetPreparationAction(_logger, archiveSet, archiveSystemCache, _config);
			if (prepActionEnumerable != null)
			{
				foreach (IArchivePreparationAction prepAction in prepActionEnumerable)
				{
					if (prepAction != null)
					{
						preparationActions.Add(prepAction);
					}
				}
			}

			var archiveActions = new List<IArchiveAction>();
			var archiveActionEnumerable = descriptor.GetArchiveAction(_logger, archiveSet);
			if (archiveActionEnumerable != null)
			{
				foreach (IArchiveAction archiveAction in archiveActionEnumerable)
				{
					if (archiveAction != null)
					{
						archiveActions.Add(archiveAction);
					}
				}
			}

			archiveActions.Add(GetPurgeAction(archiveSet));

			foreach (var action in preparationActions)
			{
				try
				{
					action.Execute();
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					var errorMessage = $"Failed to archive this set containing MainArchiveItem={archiveSet.MainArchiveItem}. " +
						$"The preparation action '{action.GetType().Name}' failed. " +
						$"{e.Data["ArchiveItemInfo"]}" +
						$"{e.Data["DocumentCommandInfo"]}" +
						$"{e.Data["ArchiveOfflineFilePathInfo"]}" +
						$"{e.Data["ArchiveImageGenerationAction.ExecuteForBizOInfo"]}" +
						$"{System.Environment.NewLine}Exception={e}";

					_logger.LogAndReportError("ArchiveStage.ArchiveToImagesException", systemDescriptor.Code, errorMessage, e);
					stepResult.ErrorsEncountered.Add(errorMessage);
				}
			}

			var errorsFixed = false;
			if (stepResult.ErrorsEncountered.Count == 0)
			{
				var numberOfTimesToTryBeforeGivingUp = 6;
				while (true)
				{
					using (var transactionManager = new MultiTransactionManager<ArchiveStage>(this, archiveActions))
					{
						var currentActionDescription = string.Empty;
						try
						{
							foreach (var action in archiveActions)
							{
								currentActionDescription = action.GetType().Name;
								action.Execute();
							}

							transactionManager.CommitTransaction();
							errorsFixed = true;
							break;
						}
						catch (Exception e) when (!e.IsCriticalException())
						{
							string errorMessage;

							try
							{
								transactionManager.RollbackTransaction();
							}
							catch (Exception rollbackE) when (!rollbackE.IsCriticalException())
							{
								errorMessage = (NoResString)"Failed a rollback action: " + rollbackE.ToString();

								_logger.LogAndReportError("ArchiveStage.ArchivetoImagesRollbackException", systemDescriptor.Code, errorMessage, rollbackE);
								stepResult.ErrorsEncountered.Add(errorMessage);

								break;
							}

							if (numberOfTimesToTryBeforeGivingUp-- == 0)
							{
								if (CanBeHandledNextRun(e))
								{
									errorMessage = (NoResString)"This archive set will not be removed from the database during this run as new related objects have been added during eDocs generation. It will be removed during next run." + System.Environment.NewLine + e.ToString();
								}
								else if (DeadlockedFoundCanBeHandledNextRun(e))
								{
									errorMessage =
										$"This archive set will not be removed from the database during this run as there is a Deadlock. It will be removed during next run. Archive System: " +
										$"{systemDescriptor.Name}. Archive Stage: {descriptor.Name}. On or Before Date: {_config.ArchiveJobsOnOrBeforeThisDate}. Archive Declarations: {_config.ShouldIncludeDeclarations}." +
										$" Type of Action that Triggered Exception: {currentActionDescription}."
										+ System.Environment.NewLine + e.ToString();

									var valuesToReport = new List<(string, object)>();
									valuesToReport.AddRange(new (string, object)[]
									{
										(UsageProperties.DatabaseName, Db.DatabaseName),
										(UsageProperties.ArchiveSystemName, systemDescriptor.Name.ToString()),
										(UsageProperties.ArchiveStageName, Name),
										(UsageProperties.IncludeCustomsJobsInArchiving, _config.ShouldIncludeDeclarations),
										(UsageProperties.ArchiveActionName, currentActionDescription),
										(UsageProperties.ArchiveItemNK, archiveSet.MainArchiveItemNK),
									});
									UsageCollector.Report(UsageFeatures.Codes.ArchiveManagerDeadlock, valuesToReport.ToArray());

									_logger.LogWarning(systemDescriptor.Code, errorMessage);
									break;
								}
								else
								{
									errorMessage = (NoResString)"Failed to archive this set." + System.Environment.NewLine + e.ToString();
								}

								stepResult.ErrorsEncountered.Add(errorMessage);
								_logger.LogError(systemDescriptor.Code, errorMessage);
								break;
							}

							else
							{
								Thread.Sleep(500);
							}
						}
					}
				}
			}

			if (errorsFixed)
			{
				stepResult.ErrorsEncountered.Clear();
			}

			return stepResult;
		}

		public virtual PurgeAction GetPurgeAction(IArchiveSet archiveSet)
		{
			return new PurgeAction(archiveSet, _logger);
		}

		// A.K: I'd probably move this method to IArchiveAction (read PurgeAction) in a separate WI and refactor it so it deals with a list of objects that can be generated during missing eDocs generation
		// reason being the behaviour of it can be action specific
		// to fix the defect - I'll just add JobShipmentGateway to the check

		public static bool CanBeHandledNextRun(Exception e)
		{
			var containsJobTableName = e.Message.Contains(JobContainerPenaltySchema.Constants.TableName)
				|| e.Message.Contains(JobShipmentGatewaySchema.Constants.TableName)
				|| e.Message.Contains(JobPickupDeliveryConfirmSchema.Constants.TableName);

			return e.Message.Contains((NoResString)"The DELETE statement conflicted with the REFERENCE constraint") && containsJobTableName;
		}

		public static bool DeadlockedFoundCanBeHandledNextRun(Exception e)
		{
			var deadlockMessage = (NoResString)"was deadlocked on lock resources with another process and has been chosen as the deadlock victim";
			return e.Message.Contains(deadlockMessage) || (e.InnerException != null && e.InnerException.Message.Contains(deadlockMessage));
		}

		public ZQuery GetMainBusinessObjectFilter(IArchiveConfiguration config)
		{
			var result = descriptor.GetMainArchiveableFilter(config);

			if (descriptor.MainArchiveNKColumn != null)
			{
				result.OrderBy = $"{descriptor.MainDateFilterColumn.Name}, {descriptor.MainArchiveNKColumn.Name}, {descriptor.MainArchivePKColumn.Name}";
			}
			else
			{
				result.OrderBy = $"{descriptor.MainDateFilterColumn.Name}, {descriptor.MainArchivePKColumn.Name}";
			}

			return result;
		}

		public string GetArchiveItemCodeWithFallback(IArchiveSet archiveSet)
			=> archiveSet.MainArchiveItemNK.IsNullOrEmpty()
				? archiveSet.MainArchiveItem.PK.ToString()
				: archiveSet.MainArchiveItemNK;

		#region IArchiveSystemSetup Members

		void IArchiveSystemSetup.AddRelationshipToAllPKs(params SchemaColumn[] childFKColumns)
		{
			var childSchemaColumns = new Dictionary<string, KeyValuePair<string, SchemaColumn>>
			{
				{ mainArchiveableType.PKColumn.TableName, new KeyValuePair<string, SchemaColumn>(mainArchiveableType.PKColumn.TableName, mainArchiveableType.PKColumn) }
			};

			foreach (var relationshipList in ArchiveableRelationships.Values)
			{
				foreach (var relationship in relationshipList)
				{
					if (!childSchemaColumns.ContainsKey(relationship.ChildName))
					{
						childSchemaColumns.Add(relationship.ChildName, new KeyValuePair<string, SchemaColumn>(relationship.ChildName, relationship.ChildPKColumn));
					}
				}
			}

			foreach (var childColumn in childFKColumns)
			{
				foreach (var parentColumnPair in childSchemaColumns.Values)
				{
					((IArchiveSystemSetup)this).AddRelationship(parentColumnPair.Key, parentColumnPair.Value, childColumn.TableName, childColumn);
				}
			}
		}

		void IArchiveSystemSetup.AddRelationship(SchemaColumn parentPKColumn, SchemaColumn childFKColumn)
			=> ((IArchiveSystemSetup)this).AddRelationship(parentPKColumn.TableName, parentPKColumn, childFKColumn.TableName, childFKColumn);

		void IArchiveSystemSetup.AddRelationship(SchemaColumn parentPKColumn, SchemaColumn childFKColumn, bool isReversed)
			=> ((IArchiveSystemSetup)this).AddRelationship(parentPKColumn.TableName, parentPKColumn, childFKColumn.TableName, childFKColumn, isReversed);

		void IArchiveSystemSetup.AddRelationship(string parentNameOverride, SchemaColumn parentPKColumn, string childNameOverride, SchemaColumn childFKColumn)
			=> ((IArchiveSystemSetup)this).AddRelationship(parentNameOverride, parentPKColumn, childNameOverride, childFKColumn, false);

		void IArchiveSystemSetup.AddRelationship(string parentNameOverride, SchemaColumn parentPKColumn, string childNameOverride, SchemaColumn childFKColumn, bool isReversed)
		{
			if (parentPKColumn.SqlDbType != childFKColumn.SqlDbType)
			{
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "parentPKColumn and childPKColumn must have the same SqlDbType, currently child '{0}' is {1} and parent '{2}' is {3}", childFKColumn.TableName, childFKColumn.SqlDbType, parentPKColumn.TableName, parentPKColumn.SqlDbType));
			}

			var resolver = new EnterpriseSchemaResolver();
			var childPKCol = resolver.GetPkColumn(childFKColumn.TableName);
			var parentPKCol = resolver.GetPkColumn(parentPKColumn.TableName);

			var relationship = new ArchiveableRelationship(parentNameOverride, parentPKCol, parentPKColumn, childNameOverride, childPKCol, childFKColumn, isReversed);

			List<ArchiveableRelationship> relationshipList;
			if (ArchiveableRelationships.ContainsKey(parentNameOverride))
			{
				relationshipList = ArchiveableRelationships[parentNameOverride];
			}
			else
			{
				relationshipList = new List<ArchiveableRelationship>();
				ArchiveableRelationships.Add(parentNameOverride, relationshipList);
			}

			if (relationshipList.Count == 0 || !relationshipList.Exists(delegate (ArchiveableRelationship r)
			{ return r.ParentName == parentNameOverride && r.ParentPKColumn == parentPKColumn && r.ChildName == childNameOverride && r.ChildFKColumn == childFKColumn; }))
			{
				relationshipList.Add(relationship);
			}
		}

		public List<(Guid, Guid, string)> GetNextArchiveSet()
			=> throw new NotImplementedException();

		#endregion

#if DEBUG
		public void OnArchiveSetProcessed(IArchiveSet archiveSet)
			=> descriptor.OnArchiveSetProcessed(archiveSet);
#endif

		public Dictionary<string, List<ArchiveableRelationship>> ArchiveableRelationships { get; }
		private protected readonly IArchiveStageDescriptor descriptor;
		private protected readonly IArchiveSystemDescriptor systemDescriptor;
		private protected IArchiveLogger _logger;
		private protected IArchiveConfiguration _config;
		private protected IArchiveSchedule _schedule;
		public string StageNameForQueries => (_config.ShouldIncludeDeclarations ? Name + "WithDeclarations" : Name + "WithoutDeclarations").Replace(" ", "").Replace("_", "");
		public static int LoadArchiveSetBatchTimeout
			=> SystemDataRegistry.Instance.LoadArchiveSetBatchTimeout.Value;
		protected ArchiveableType mainArchiveableType { get; private set; }
		public bool Initialised { get; set; }
		readonly protected bool IncludeTimeTakenInTheARCLogs;
		protected readonly ArchiveSystemCache archiveSystemCache = new ArchiveSystemCache();
	}
}
