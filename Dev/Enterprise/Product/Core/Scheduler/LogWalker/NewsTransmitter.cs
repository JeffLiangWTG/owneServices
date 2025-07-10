using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.LogWalker
{
	[Serializable]
	public class NewsTransmitter
	{
		readonly LogSubscriber subscriber;
		readonly LogSubscriber[] allSubscribers;
		[NonSerialized]
		readonly IStmJobQueueLogger jobQueueLogger;
		[NonSerialized]
		readonly IStmJobQueueLoggerConfig config;

		public NewsTransmitter(LogSubscriber subscriber, LogSubscriber[] allSubscribers, SubscriberParameters subscriberParameters)
		{
			this.config = subscriberParameters.StmJobQueueLoggerConfig;
			this.jobQueueLogger = subscriberParameters.StmJobQueueLogger;

			this.subscriber = subscriber;
			this.allSubscribers = allSubscribers;

			this.subscriber.SetBackgroundWorkerFactory(subscriberParameters.BackgroundWorkerFactory);
		}

		protected internal LogSubscriberProcessResult ProcessLogQueueBatch(int batchSize, CancellationToken token = new CancellationToken())
		{
			try
			{
				return ProcessLogQueueBatchCore(batchSize, token);
			}
			catch (OperationCanceledException)
			{
				AppendToNotificationLog(LogType.Warning, "Operation Cancelled");
				throw;
			}
			catch (ThreadAbortException)
			{
				AppendToNotificationLog(LogType.Warning, "Thread aborted.");
				return new LogSubscriberProcessResult { Request = LogSubscriberProcessRequest.Abort };
			}
			catch (SqlLockLostException)
			{
				AppendToNotificationLog(LogType.Warning, "Sql lock lost.");
				return new LogSubscriberProcessResult { Request = LogSubscriberProcessRequest.Requeue };
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				string message = string.Format(CultureInfo.InvariantCulture, "LogWalker Subscriber Unhandled Error [{0}]", subscriber.FriendlyName);
				Globals.Message.ShowDeveloperException(message, ex);
				return new LogSubscriberProcessResult();
			}
		}

		LogSubscriberProcessResult ProcessLogQueueBatchCore(int batchSize, CancellationToken token)
		{
			var totalProcessed = 0;
			var sw = Stopwatch.StartNew();
			BatchResult lastResult;
			do
			{
				lastResult = ProcessLogQueueSafe(batchSize, token);
				totalProcessed += lastResult.ItemsProcessed;
			}
			while (ContinueTransmitting(batchSize, lastResult, sw, token));

			return new LogSubscriberProcessResult
			{
				ItemsFound = totalProcessed,
				Request = (lastResult.ForceYield || lastResult.ItemsLoaded == 0)
					? LogSubscriberProcessRequest.Yield
					: LogSubscriberProcessRequest.Requeue
			};
		}

		// virtual for test only
		internal virtual bool ContinueTransmitting(int batchSize, BatchResult lastResult, Stopwatch sw, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			if (!lastResult.ForceYield
				&& lastResult.ItemsLoaded > 0
				&& batchSize != 0)
			{
				if (sw.Elapsed > TimeSpan.FromSeconds(SystemDataRegistry.Instance.SecondsUntilLogWalkerStopsAllocatingLogBatchesToASubscriber.Value))
				{
					AppendToNotificationLog(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Subscriber {0} yields after running for more than {1} seconds",
						subscriber.Name,
						SystemDataRegistry.Instance.SecondsUntilLogWalkerStopsAllocatingLogBatchesToASubscriber.Value));
					return false;
				}
				else
				{
					return true;
				}
			}
			else
			{
				return false;
			}
		}

		internal class BatchResult
		{
			internal int ItemsLoaded { get; set; }
			internal int ItemsProcessed { get; set; }
			internal bool ForceYield { get; set; }
		}

		BatchResult ProcessLogQueueSafe(int batchSize, CancellationToken token)
		{
			try
			{
				var items = ReadAndProcessQueue(batchSize, token);
				if (items.ItemsProcessed > 0)
				{
					AppendToNotificationLog(LogType.Debug, "finished processing logs.");
				}

				return items;
			}
			catch (SqlLockLostException)
			{
				throw;
			}
			catch (SqlException sqlEx) when (sqlEx.Message.StartsWith("A transport-level error has occurred when sending the request to the server."))
			{
				throw;
			}
			catch (ThreadAbortException)
			{
				throw;
			}
			catch (OperationCanceledException)
			{
				throw;
			}
#pragma warning disable ENT0001
			catch (Exception ex) // has specific handling for critical
#pragma warning disable ENT0001
			{
				if (ex.IsCriticalException())
				{
					AppendExceptionLogSafe("Exception happened during processing logs queue.", ex);
					throw;
				}

				if (!ExceptionReporter.Instance.HandleSpecificExceptions(ex))
				{
					ErrorReporter.ReportOnce("Unhandled exception in News Transmitter", ex);
				}
				AppendToNotificationLog(LogType.Error, string.Format(CultureInfo.InvariantCulture, "Unhandled News transmitter error:\r\n{0}", ex.Message), ex);

				return new BatchResult { ForceYield = true };
			}
		}

		BatchResult ReadAndProcessQueue(int batchSize, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			using (var logsProcessingStack = new ProcessableLogGroupStack())
			using (var queuedLogs = ReadQueue(batchSize))
			{
				if (queuedLogs.Values.Count > 0)
				{
					bool alreadyLogging = false;
					var filteredLogs = queuedLogs.Values.Where(l => CheckLogQueued(l.Item, ref alreadyLogging)).CollectMaxBy(l => l.Item.SJ_RetryCount);
					if (filteredLogs.Any())
					{
						foreach (var logGroup in GetLogGroups(filteredLogs, subscriber, new RecurringLogTracker(), 0, priorExceptions: Array.Empty<Exception>()).Reverse())
						{
							if (!TryCancelLogGroupsWithEvidenceOfHandledExceptions(logGroup))
							{
								logsProcessingStack.Push(logGroup);
							}
						}

						var batch = ProcessAndSave(logsProcessingStack, token);
						batch.ItemsLoaded = queuedLogs.ItemsLoaded;

						return batch;
					}
				}
				else if (queuedLogs.ItemsLoaded > 0)
				{
					return new BatchResult { ItemsLoaded = queuedLogs.ItemsLoaded }; // items loaded were skipped due to AppLock => continue
				}
			}

			return new BatchResult { ForceYield = true }; // End processing for no logs were processed.
		}

		bool CheckLogQueued(IQueuedLog queuedLog, ref bool alreadyLogging)
		{
			if (queuedLog.SJ_Status != JobQueueStatus.StatusQueued)
			{
				return false;
			}

			if (!alreadyLogging)
			{
				var lagTime = ZDateTime.UtcNow - queuedLog.SJ_PostedTimeUtc;
				if (lagTime > config.LogAllowedLag)
				{
					jobQueueLogger.GoForthAndLog();
				}

				alreadyLogging = true;
			}

			return true;
		}

		IEnumerable<ProcessableLogGroup> GetLogGroups(IList<AppLockedItem<IQueuedLog>> filteredLogs, LogSubscriber logSubscriber, RecurringLogTracker logTracker, int depth, IList<Exception> priorExceptions)
		{
			foreach (var batch in logSubscriber.GroupLogs(filteredLogs))
			{
				var firstLog = batch.First();
				var initialLogGroup = new ProcessableLogGroup(firstLog.Item.Factory, logSubscriber, depth: depth, retryCount: batch.Key.Retries, queuedLogs: batch, recursionTracker: logTracker, priorExceptions: priorExceptions);
				yield return initialLogGroup;
			}
		}
		
		protected virtual BusinessObjectFactory GetNewBusinessObjectFactory()
		{
			return new BusinessObjectFactory { RefreshEnabled = false, NameForDebugging = "NewsTransmitter Read Factory" };
		}

		// virtual for test only
		protected virtual StmJobQueueBatch ReadQueue(int batchSize)
		{
			var factory = GetNewBusinessObjectFactory();

			var query = new ZDBOnlyQuery(typeof(StmJobQueue));
			query.AddToFilter(StmJobQueueSchema.SJ_FilterName, subscriber.Name);
			query.AddToFilter(StmJobQueueSchema.SJ_Status, JobQueueStatus.StatusQueued);
			query.AddToFilter(StmJobQueueSchema.SJ_ProcessOnOrAfterUtc, SQLComparisonOperator.LessThanOrEqualTo, ZDateTime.UtcNow);

			if (SystemDataRegistry.Instance.EnableLogWalkerForceSeekOnReadQueueQuery.Value)
			{
				query.TableHints = TableHints.FORCESEEK;
				query.TableIndexHints.Add(new TableIndexHint("NR_RC__SJ_FilterName_SJ_Status_SJ_ProcessOnOrAfterUtc"));
			}

			query.OrderBy = StmJobQueueSchema.SJ_ProcessOnOrAfterUtc.Name;

			var items = factory.LoadWithApplocks<StmJobQueue>(GetApplockKey(subscriber), GetApplockColumns(), query, batchSize);
			var itemsLocked = items
				.ItemsWithLocks
				.Select(s => new AppLockedItem<IQueuedLog>(s.Item, s.Lock))
				.ToArray();

			return new StmJobQueueBatch(itemsLocked)
			{
				ItemsLoaded = items.ItemsLoaded
			};
		}

		public static string GetApplockKey(LogSubscriber subscriber) => string.Format(CultureInfo.InvariantCulture, "LogSubscriber:{0}", subscriber.Name);
		public static SchemaGuidColumn[] GetApplockColumns() => new[] { StmJobQueueSchema.SJ_ProcessTaskParentID, StmJobQueueSchema.SJ_ParentID };

		internal virtual BatchResult ProcessAndSave(ProcessableLogGroupStack logsProcessingStack, CancellationToken token)
		{
			var result = new BatchResult();
			var maximumDepth = SystemDataRegistry.Instance.LogWalkerRecursOnNewEvents.Value ? SystemDataRegistry.Instance.LogWalkerMaxEventRecursion.Value : 0;

			while (logsProcessingStack.Count > 0)
			{
				token.ThrowIfCancellationRequested();
				var logsGroup = logsProcessingStack.Pop();
				try
				{
					result.ItemsProcessed += logsGroup.Count;
					var newGroups = ProcessLogGroup(maximumDepth, logsGroup);
					newGroups.ForEach(group => logsProcessingStack.Push(group));
				}
				catch
				{
					logsGroup.FreeLocks();
					throw;
				}
			}
			return result;
		}

		bool TryCancelLogGroupsWithEvidenceOfHandledExceptions(ProcessableLogGroup logsGroup)
		{
			var retryCount = logsGroup.RetryCount;
			if (retryCount < 0)
			{
				ErrorReporter.ReportOnce("Retry count below zero...");
				AppendToNotificationLog(LogType.Warning, "Retry count below zero...");
				SaveLogsAsFailed(logsGroup);
				return true;
			}
			else if (retryCount >= RetryCountLimitForUnhandledExceptions)
			{
				// If Logs are being cancelled here, it is because the LWK crashed or closed unexpectedly
				// That is, we are loading logs that we have already tried to process by themselves, but they failed.
				// There is a possibility of a false positive here (e.g. the service task is killed due to connectivity issues twice for the same log)
				// Because of this code we can not guarantee that all processable logs are processed in the event of major stability issues
				AppendToNotificationLog(LogType.Warning, "Loaded logs that have aborted processing for unknown reasons.");
				SaveLogsAsFailed(logsGroup);
				return true;
			}
			else
			{
				return false;
			}
		}

		IEnumerable<ProcessableLogGroup> ProcessLogGroup(int maximumDepth, ProcessableLogGroup logsGroup)
		{
			var logsToProcess = logsGroup.Logs;

			if (logsGroup.RetryCount > 0)
			{
				AppendToNotificationLog(LogType.Debug, string.Format(CultureInfo.InvariantCulture, "Processing log (PK = '{0}') individually.", logsToProcess.Single().PK));
			}
			foreach (var queuedLog in logsToProcess)
			{
				queuedLog.SJ_RetryCount++;
			}
			logsToProcess.First().Factory.Save();

			try
			{
				return ProcessLogGroupCore(maximumDepth, logsGroup);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				AppendToNotificationLog(LogType.Warning, Res.GetString("9ac8c4d5-fb16-4bb7-87ca-77e953e1c73b", "failed to process logs. Affected records will be processed again one-by-one.\r\n{0}", ex.Message), ex);
				logsGroup.FreeLocks();
			}

			return Enumerable.Empty<ProcessableLogGroup>();
		}

		IEnumerable<ProcessableLogGroup> ProcessLogGroupCore(int maximumDepth, ProcessableLogGroup currentLogGroup)
		{
			var logsToProcess = currentLogGroup.Logs;
			var saveTracker = new LogSubscriberSaveTracker();
			currentLogGroup.RecursionTracker.TrackLogs(logsToProcess);
			if (currentLogGroup.Depth > 0)
			{
				AppendToNotificationLog(LogType.Information, Res.GetString("a2f6b7ad-f589-4a91-8954-1af103e58689", "recurring with subscriber [{1}] and depth {2} for {0} logged event(s).", currentLogGroup.Count, currentLogGroup.Subscriber.FriendlyName, currentLogGroup.Depth));
			}

			using (var context = currentLogGroup.Subscriber.SetContextForLogsGroup(currentLogGroup.BatchKey, logsToProcess))
			using (currentLogGroup.Subscriber.SetCurrentSubscriber())
			using (currentLogGroup.Factory.AddDisposableService())
			using (var disposableManager = new DisposableManager())
			{
				if (context.SkipGroup)
				{
					return SkipLogGroup(currentLogGroup);
				}
				else
				{
					try
					{
						AppendToNotificationLog(LogType.Debug, string.Format(CultureInfo.InvariantCulture, "processing {0} logged event(s).", currentLogGroup.Count));
						using (var delayedTransaction = SetupDelayedTransaction(currentLogGroup.Factory))
						{
							LogSubscriberResult subscriberResult;
							saveTracker.AddValidFactoryToSave(currentLogGroup.Subscriber.FactoryName);

							using (saveTracker.TrackInvalidSaves())
							{
								subscriberResult = currentLogGroup.Subscriber.ProcessLogs(currentLogGroup.Logs.ToArray());
							}

							MarkLogsAsProcessed(subscriberResult);
							if (!subscriberResult.ProcessedLogs.Any() && subscriberResult.UnprocessedLogs.Any())
							{
								// If all we have are unprocessed logs, then somethings terrible has happened.
								SaveLogsAsFailed(currentLogGroup);
								delayedTransaction.CommitTransaction();
								ErrorReporter.ReportOnce("No logs in the batch were processed.");
								return Enumerable.Empty<ProcessableLogGroup>();
							}
							else
							{
								using (saveTracker.TrackInvalidSaves())
								{
									return SaveProcessedLogs(maximumDepth, currentLogGroup, subscriberResult, delayedTransaction, disposableManager);
								}
							}
						}
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						switch (currentLogGroup.Subscriber.TryHandleException(ex, currentLogGroup.Logs, currentLogGroup.RetryCount))
						{
							case LogSubscriber.ExceptionHandlingResult.SaveAsSuccessfulInNewFactory:
								{
									return SkipLogGroup(currentLogGroup,
											factory => currentLogGroup.Subscriber.FinalizeExceptionHadlingAsSaveAsSuccessfulInNewFactory(factory, ex));
								}
							case LogSubscriber.ExceptionHandlingResult.Unhandled:
							default:
								{
									bool isConcurrencyError = GetConcurrencyRelatedException(ex) != null;
									if ((ex.ShouldReprocess() && currentLogGroup.RetryCount < SystemDataRegistry.Instance.RetryAttemptsOnLogWalkerRecoverableErrors.Value) || currentLogGroup.RetryCount == 0)
									{
										var logType = isConcurrencyError ? LogType.Warning : LogType.Information;
										var notificationMessage = isConcurrencyError ? "A conflict during save will cause logs to be processed one at a time." : $"failed to process logs. Affected records will be processed again one-by-one.\r\n{ex.Message}";
										AppendToNotificationLog(logType, notificationMessage);
										return currentLogGroup.SplitGroup(ex);
									}

									if (isConcurrencyError)
									{
										AppendToNotificationLog(LogType.Information, FormattableString.Invariant($"Failed with this concurrency exception:\r\n{ex.Message}"));
										if (currentLogGroup.PriorExceptions.Skip(1).All(c => GetConcurrencyRelatedException(c) != null))
										{
											var registry = SystemDataRegistry.Instance.LogWalkerLogging.Value;
											var shouldReport = registry.GetBoolFromCode(SystemDataRegistry.LogWalkerLoggingKeys.ConcurrencyErrorReport);
											if (shouldReport)
											{
												if (!saveTracker.ReportConcurrencyErrorCausedByInvalidSave(currentLogGroup, ex) && ex is not ZConcurrencyCheckFailureException)
												{
													ExceptionReporter.Instance.ReportDeveloperException(currentLogGroup.Subscriber.GetPrettyPrinter().PrettyPrintSubscriberErrorInfo(currentLogGroup.Logs.Single()), new AggregateException(currentLogGroup.PriorExceptions.Append(ex)));
												}
											}
										}
										else
										{
											// If one of the exceptions thrown by this log group on a previous retry wasn't a concurrency error, then we need to know about it.
											ReportLWKFailure(currentLogGroup, currentLogGroup.PriorExceptions.Append(ex), true);
										}
									}
									else if (IsBusinessFailure(ex))
									{
										AppendToNotificationLog(LogType.Error, FormattableString.Invariant($"Logs could not be processed due to an error:\r\n{ex.Message}"));
									}
									else
									{
										ReportLWKFailure(currentLogGroup, currentLogGroup.PriorExceptions.Append(ex), false);
										AppendToNotificationLog(LogType.Warning, FormattableString.Invariant($"Logs could not be processed due to an error:\r\n{ex.Message}"));
									}

									SaveLogsAsFailed(currentLogGroup);
									return Enumerable.Empty<ProcessableLogGroup>();
								}
						}
					}
				}
			}
		}

		void ReportLWKFailure(ProcessableLogGroup currentLogGroup, IEnumerable<Exception> exceptions, bool reportWithoutExceptionHandling)
		{
			Exception exception;
			if (exceptions.AllSame(f => f.GetType()))
			{
				exception = exceptions.Last();
			}
			else
			{
				var message = string.Join(System.Environment.NewLine, exceptions.SelectMany((Exception ex) => ex.Message.Split(new string[3] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)));
				exception = new Exception($"This log failed to processes for more than one reason!{System.Environment.NewLine}{message}", exceptions.Last());
			}
			var tableCodes = currentLogGroup.Logs.Select(l => l.SJ_ParentTableCode).OrderBy(l => l).ToArray();

			if (reportWithoutExceptionHandling)
			{
				ExceptionReporter.Instance.ReportDeveloperException(currentLogGroup.Subscriber.GetPrettyPrinter().PrettyPrintSubscriberErrorInfo(currentLogGroup.Logs.Single()), exception);
			}
			else
			{
				var message = currentLogGroup.Subscriber.GetPrettyPrinter().PrettyPrintSubscriberErrorInfo(currentLogGroup.Logs.Single());
				if (exceptions.Last() is ZCannotSaveException)
				{
					message += "\r\nZCannotSaveExceptions are considered unhandled by default in NewsTransmitter, if your exception is generating an Error Report but is a legitimate business failure that just needs to be Logged as an Error, then use the ExceptionType.BusinessFailure for the specific ZCannotSaveException that need it.";
				}

				ErrorReporter.ReportOnce(message, exception);
			}
		}

		IDelayedTransactionManager SetupDelayedTransaction(BusinessObjectFactory factory)
		{
			return SystemDataRegistry.Instance.LogSubscriberUseSharedTransaction.Value ?
				factory.DelayedTransaction() :
				factory.GetDummyDelayedTransaction();
		}

		IEnumerable<ProcessableLogGroup> SkipLogGroup(ProcessableLogGroup currentLogGroup, Action<BusinessObjectFactory> prepareForSaving = null)
		{
			try
			{
				AppendToNotificationLog(LogType.Debug, string.Format(CultureInfo.InvariantCulture, "ignoring {0} logged event(s).", currentLogGroup.Count));
				var factory = GetNewBusinessObjectFactory();
				var logs = currentLogGroup.Logs.Select(l => l is StmJobQueue jobQueue ? factory.Load<StmJobQueue>(jobQueue.PK) : l).ToList();
				MarkLogsAsProcessed(new LogSubscriberResult(logs, Array.Empty<IQueuedLog>()));
				prepareForSaving?.Invoke(factory);
				factory.Save();
				currentLogGroup.FreeLocks();
				return Enumerable.Empty<ProcessableLogGroup>();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				AppendToNotificationLog(LogType.Warning, Res.GetString("876de274-4de2-447a-9e63-8478f978a011", "Unexpected error when trying to skip the log group."));
				ErrorReporter.ReportOnce("Unexpected exception in \"Skip Log Group\" which in theory should be immune to concurrency style errors due to locking, " +
				"and surely all of the other IO exceptions we throw are considered 'Critical Exceptions'", ex);
				throw;
			}
		}

		protected virtual IEnumerable<ProcessableLogGroup> SaveProcessedLogs(int maximumDepth, ProcessableLogGroup logGroup, LogSubscriberResult subscriberResult, ITransactionManager transactionCommitter, DisposableManager disposableManager)
		{
			var recurringLogHandler = new RecurringLogHandler(logGroup, this.subscriber.DefaultLogger, allSubscribers, maximumDepth);
			disposableManager.Subscribe(recurringLogHandler);
			logGroup.Factory.Save();

			var newGroups = recurringLogHandler.GetNewGroups();

			// This is the place where we dispose of stuff that should be disposed of immediately after this save
			transactionCommitter.CommitTransaction();
			logGroup.FreeLocks(subscriberResult.ProcessedLogs);

			var result = newGroups.SelectMany(g =>
			{
				var factory = GetNewBusinessObjectFactory();
				var reloaded = g.LockedQueues.ReloadRowsToAvoidRaceCondition<IQueuedLog, StmJobQueue>(factory, new ZQuery(StmJobQueueSchema.PK, g.LockedQueues.Values.Select(s => s.PK)), s => s.PK);

				if (reloaded.Values.Any()) // There is an obscure race condition here.
				{
					return GetLogGroups(reloaded.ItemsWithLocks.Select(s => new AppLockedItem<IQueuedLog>(s.Item, s.Lock)).ToArray(), g.Subscriber, new RecurringLogTracker(logGroup.RecursionTracker), logGroup.Depth + 1, logGroup.PriorExceptions);
				}
				else
				{
					return Enumerable.Empty<ProcessableLogGroup>();
				}
			}).WhereNotNull().ToArray();

			recurringLogHandler.MarkAsSucceeded();

			if (subscriberResult.UnprocessedLogs.Any())
			{
				var factory = GetNewBusinessObjectFactory();
				return result.Append(logGroup.GetSubset(factory, subscriberResult.UnprocessedLogs, logGroup.PriorExceptions));
			}
			else
			{
				return result;
			}
		}

		Exception GetConcurrencyRelatedException(Exception ex)
		{
			if (ex == null)
			{
				return null;
			}

			if (ex is ZSaveConcurrencyException
				|| ex is ZDataConcurrencyException
				|| ex is ZConcurrencyCheckFailureException
				|| (ex is SqlException sqlEx
					&& new DbErrorMatch(sqlEx).ExceptionType.In(DbErrorType.CannotInsertDuplicateUniqueIndexKey, DbErrorType.SqlExceptionDdlDisconnection)))
			{
				return ex;
			}

			return GetConcurrencyRelatedException(ex.InnerException);
		}

		bool IsBusinessFailure(Exception ex) => ex is ZCannotSaveException exception && exception.IsBusinessFailure;

		void SaveLogsAsFailed(ProcessableLogGroup logsToProcess)
		{
			try
			{
				var factoryForSaving = new BusinessObjectFactory { RefreshEnabled = false, NameForDebugging = "Bad Log Marker" };
				var items = new List<IQueuedLog>();
				AppendToNotificationLog(LogType.Warning, "The following logs have failed with unhandled errors:");
				var prettyPrinter = subscriber.GetPrettyPrinter();
				foreach (var log in logsToProcess.Logs)
				{
					var stmJobQueue = log as StmJobQueue;
					if (stmJobQueue != null)
					{
						AppendToNotificationLog(LogType.Warning, prettyPrinter.PrettyPrint(log));
						items.Add(factoryForSaving.Load<StmJobQueue>(stmJobQueue.PK));
					}
					else
					{
						items.Add(log);
					}
				}

				foreach (var item in items)
				{
					item.SJ_Status = JobQueueStatus.StatusFailed;
				}

				factoryForSaving.Save();
				logsToProcess.FreeLocks();
			}
			catch (Exception innerEx) when (!innerEx.IsCriticalException())
			{
				ErrorReporter.ReportOnce("Exception marking logs as bad", innerEx);
				throw; // We can't gracefully recover from this, so don't even try.
			}
		}

		public static IList<IQueuedLog> CreatLogQueueItems(BusinessObjectFactory factory, IEnumerable<StmALog> recurringLogGroup, LogSubscriber subscriber)
		{
			return recurringLogGroup.Select(log => CreateLogQueueItem(factory, log, subscriber)).ToArray();
		}

		#region Create Log Queue Item

		public static IQueuedLog CreateStmJobQueue(BusinessObjectFactory factory, IQueuedLog log, LogSubscriber subscriber, ZString sj_SE_NKEvent, ZString sj_GS_NKUser, ZString sj_GB_NKBranch, ZString sj_GE_NKDepartment, ZString sj_Reference)
		{
			var job = factory.New<StmJobQueue>();

			using (job.GetValidationSuspender())
			{
				job.SJ_FilterName = subscriber.Name;
				job.SJ_SE_NKEvent = sj_SE_NKEvent;
				job.SJ_PostedTimeUtc = ZDateTime.UtcNow; // Arg. The StmALog will not have an SL_PostedTimeUtc until it is in
				job.SJ_EventTime = log.SJ_EventTime;
				job.SJ_EventTimeUtc = log.SJ_EventTimeUtc;
				job.SJ_GS_NKUser = sj_GS_NKUser;
				job.SJ_GB_NKBranch = sj_GB_NKBranch;
				job.SJ_GE_NKDepartment = sj_GE_NKDepartment;
				job.SJ_IsCancelled = false;
				job.SJ_IsEstimate = log.SJ_IsEstimate;
				job.SJ_Reference = sj_Reference;
				job.SJ_ParentTableCode = log.SJ_ParentTableCode;
				job.SJ_ParentID = log.SJ_ParentID;
				job.SJ_ALogReference = log.SJ_ALogReference;
				job.SJ_Status = "QUE";
				job.SJ_IsDelayFired = false;
			}

			return job;
		}

		static StmJobQueue CreateStmJobQueue(BusinessObjectFactory factory, StmALog log, LogSubscriber subscriber)
		{
			var job = factory.New<StmJobQueue>();

			using (job.GetValidationSuspender())
			{
				job.SJ_FilterName = subscriber.Name;
				job.SJ_SE_NKEvent = log.SL_SE_NKEvent;
				if (!log.SL_PostedTimeUtc.IsEmpty)
				{
					job.SJ_PostedTimeUtc = log.SL_PostedTimeUtc;
				}
				else
				{
					job.SJ_PostedTimeUtc = ZDateTime.UtcNow; // Arg. The StmALog will not have an SL_PostedTimeUtc until it is in
				}
				job.SJ_EventTime = log.SL_EventTime;
				job.SJ_EventTimeUtc = log.SL_EventTimeUtc;
				job.SJ_EventTimeUtc = log.SL_EventTimeUtc.IsEmpty ? log.SL_EventTime : log.SL_EventTimeUtc;
				job.SJ_GS_NKUser = log.SL_GS_NKUser;
				job.SJ_GE_NKDepartment = log.SL_GE_NKDepartment;
				job.SJ_GB_NKBranch = log.SL_GB_NKBranch;
				job.SJ_IsCancelled = log.SL_IsCancelled;
				job.SJ_IsEstimate = log.SL_IsEstimate;
				job.SJ_Reference = log.SL_Reference;
				job.SJ_ParentTableCode = ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(log.SL_Table);
				job.SJ_ParentID = log.SL_Parent;
				job.SJ_ALogReference = log.PK;
				job.SJ_Status = "QUE";
				job.SJ_IsDelayFired = false;
			}

			return job;
		}

		protected static IQueuedLog CreateLogQueueItem(BusinessObjectFactory factory, StmALog log, LogSubscriber subscriber)
		{
			return CreateStmJobQueue(factory, log, subscriber);
		}

		#endregion

		void MarkLogsAsProcessed(LogSubscriberResult subscriberResult)
		{
			foreach (var queuedLog in subscriberResult.ProcessedLogs)
			{
				queuedLog.SJ_Status = JobQueueStatus.StatusProcessed;
			}

			foreach (var queuedLog in subscriberResult.UnprocessedLogs)
			{
				queuedLog.SJ_RetryCount--; // Un-retry these logs
			}
		}

		const int RetryCountLimitForUnhandledExceptions = 3; // 1 in a batch and 1 individually

		#region Notifications

		void AppendExceptionLogSafe(string message, Exception ex)
		{
			try
			{
				this.subscriber.DefaultLogger.Log(LogType.Error, message + "\r\n" + ex.Message, ex);
			}
			catch (Exception ex1) when (!ex1.IsCriticalException())
			{
				// Ignore - we are logging other exception
			}
		}

		void AppendToNotificationLog(LogType type, string message, Exception ex = null) => this.subscriber.DefaultLogger.Log(type, message, ex);

		#endregion
	}
}
