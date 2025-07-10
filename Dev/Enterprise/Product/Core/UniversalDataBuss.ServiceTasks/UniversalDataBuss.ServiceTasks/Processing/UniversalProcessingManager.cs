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
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.GraphEngine.ServiceTasks;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.Registry.Business.eServices;
using Enterprise.Scheduler.GraphEngine;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.CodeMapping;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Messaging.Business.BaseMessageProcessor<Enterprise.Messaging.Business.XmlMessaging.XmlEDIMessage>;

namespace Enterprise.UniversalDataBuss.ServiceTasks
{
	interface IUniversalProcessingManager : IProcessingManager
	{
		XmlEDIGrEngine GrEngine { get; }
		void ProcessBatch(DisposableBatch messages, CancellationToken token, FailedMessagesManager failedMessagesManager);
		void FlipGrEngine();
	}

	sealed partial class UniversalProcessingManager : IUniversalProcessingManager
	{
		static int MessagesPerExecution => eAdaptorRegistry.Instance.MessagesPerExecution.Value;
		static int FlipperMessagesPerExecution => eAdaptorRegistry.Instance.UMIMessagesPerExecution.Value;

		public UniversalProcessingManager(IEnumerable<string> messageSubTypes, IEnumerable<string> excludedMessageSubTypes, GrEngineServiceSetting settings = GrEngineServiceSetting.Disabled, IFactoryService factoryService = null, ISqlApplicationLockProvider lockProvider = null)
		{
			if (factoryService == null)
			{
				factoryService = new FactoryService();
				foreach (var factory in GetFactories(factoryService))
				{
					factory.registerFactory();
				}
			}
			else
			{
				this.factoryService = factoryService;
			}

			this.messageSubTypes = messageSubTypes;
			this.excludedMessageSubTypes = excludedMessageSubTypes;
			this.factoryService = factoryService;
			this.grEngineServiceSetting = settings;
			this.lockProvider = lockProvider;
			baseMessageProcessor = new MessageProcesorExploder(GetMessageProcessors);
		}

		readonly IFactoryService factoryService;
		public XmlEDIGrEngine GrEngine
		{
			get
			{
				if (grEngine == null)
				{
					grEngine = factoryService
						.GetFactory<Func<GrEngineServiceSetting, ISqlApplicationLockProvider, LoggingInformation, XmlEDIGrEngine>>()
						.Invoke(grEngineServiceSetting, lockProvider, Logger);
				}
				return grEngine;
			}
		}
		XmlEDIGrEngine grEngine;
		readonly IEnumerable<string> messageSubTypes;
		readonly IEnumerable<string> excludedMessageSubTypes;
		readonly MessageProcesorExploder baseMessageProcessor;
		readonly GrEngineServiceSetting grEngineServiceSetting;
		readonly ISqlApplicationLockProvider lockProvider;
		readonly Stopwatch stopwatchForProcessMessage = Stopwatch.StartNew();

		public LoggingInformation Logger
		{
			get
			{
				return baseMessageProcessor.Logger;
			}
		}

		void Log(string message, LogType logType, bool isLogForTest = false)
		{
			if (isLogForTest)
			{
				LogIfDebug(message, logType);
			}
			else
			{
				Logger.Log(message, logType);
			}
		}

		partial void LogIfDebug(string message, LogType logType);

		void LoggingFinishedProcessingMessage(XmlEDIMessage message)
		{
			stopwatchForProcessMessage.Stop();
			Log(Res.GetString("38dd6d7d-efdd-4fcd-89be-db8f5f934454", "Finished processing Message #{0} (Elapsed time: {1} ms)", message.EM_MessageNum, stopwatchForProcessMessage.ElapsedMilliseconds), LogType.Information);
		}

		public void ExecuteBatch(CancellationToken token)
		{
			Log(Res.GetString("f2277f13-17f0-442b-aaf5-95d84915fe4f", "Starting executing batch"), LogType.Debug);
			var stopwatchForExecute = Stopwatch.StartNew();
			var failedMessagesManager = new FailedMessagesManager(RetryType.CurrentExecution, ShouldRetryOnException);
			var totalItemsProcessed = 0;
			DisposableBatch batch;
			var stopwatchForRetrieve = Stopwatch.StartNew();
			var stopwatchForProcess = Stopwatch.StartNew();
			do
			{
				token.ThrowIfCancellationRequested();
				Log(Res.GetString("aa21a985-56e9-4dc3-ae4e-892405b727e5", "Starting retrieving next processable messages"), LogType.Debug);
				stopwatchForRetrieve.Restart();
				using (batch = baseMessageProcessor.RetrieveNextProcessableMessages(DequeueMessages, failedMessagesManager))
				{
					stopwatchForRetrieve.Stop();
					Log(Res.GetString("d1ecfa5f-ae95-45bb-982b-d8121a8d27d6", "Finished retrieving next processable messages (Elapsed time: {0} ms, Number of records: {1})", stopwatchForRetrieve.ElapsedMilliseconds, batch.ItemsInBatchCount), LogType.Debug);
					totalItemsProcessed += batch.ItemsInBatchCount;
					try
					{
						Log(Res.GetString("bd6db540-308f-463f-88a4-b4da7efc9607", "Starting processing batch"), LogType.Information);
						stopwatchForProcess.Restart();
						ProcessBatch(batch, token, failedMessagesManager);
						stopwatchForProcess.Stop();
						Log(Res.GetString("562927f4-1659-4294-8577-ae22faa0176a", "Finished processing batch (Elapsed time: {0} ms)", stopwatchForProcess.ElapsedMilliseconds), LogType.Information);
					}
					catch (Exception e) when (!e.IsCriticalException())
					{
						Log(FormattableString.Invariant($"Exception processing a group of {batch.Length} messages: [{e.Message}]"), LogType.Warning, true);
						if (ShouldExitOnException(e))
						{
							throw;
						}
					}
				}
			}
			while (totalItemsProcessed < MessagesPerExecution && !batch.IsLastBatch);
			stopwatchForExecute.Stop();
			Log(Res.GetString("72de977e-70b8-4f1b-9b61-b7d25f9b0343", "Finished executing batch (Elapsed time: {0} ms, Processed {1} messages)", stopwatchForExecute.ElapsedMilliseconds, totalItemsProcessed), LogType.Debug);
		}

		public void ProcessBatch(DisposableBatch messages, CancellationToken token, FailedMessagesManager failedMessagesManager)
		{
			foreach (var currentMessage in messages)
			{
				// For the time being we need this separate factory to be attached to the message, as it will be used by some of the exception handling lower
				// down. As the factory saves are raised through the layers we should bring this exception handling together at this layer.
				var messageFactory = new MessageLoggingFactory();
				var message = (XmlEDIMessage)messageFactory.ImportFromAnotherFactory(currentMessage);
				var xmlSessionTracker = new XmlSessionTracker(Logger);

				try
				{
					MessageProcessingHookForTest("PreProcessing", message);
					if (!GrEngine.IsEnabled || GrEngine.Setting == GrEngineServiceSetting.Worker)
					{
						if (message.EM_RetryCount > eAdaptorRegistry.Instance.RetryAttemptsOnUniversalXMLProcessingRecoverableErrors.Value)
						{
							RejectMessage(message, xmlSessionTracker);
							continue;
						}
						message.EM_RetryCount++;
						messageFactory.Save();
					}

					if (!message.EM_HeldUntilDate.IsEmpty && message.EM_HeldUntilDate > ZDateTime.UtcNow)
					{
						continue;
					}

					Log(Res.GetString("110D6107-B683-4F7A-9736-573B7AEF0EBD", "Starting processing Message #{0}", message.EM_MessageNum), LogType.Information);
					stopwatchForProcessMessage.Restart();

					var codeMapper = new CodeMappingManager(xmlSessionTracker);
					IDataMessageFactory dataMessageFactory;

					try
					{
						dataMessageFactory = message.GetUniversalDataMessageFactory();
					}
					catch (UnsupportedMessageTypeException)
					{
						xmlSessionTracker.LogBoth(LogType.Warning, Res.GetString("54434DCD-1A3B-48F4-84A6-3B4715742447", "Cannot process Message Sub Type [{0}]. Messages of Sub Type [{0}] can only be processed using eAdaptor HTTP+XML", message.EM_MessageSubType));
						RejectMessage(message, xmlSessionTracker);
						continue;
					}
					catch (InvalidMessageTypeException)
					{
						xmlSessionTracker.LogBoth(LogType.Warning, Res.GetString("274DF567-CC9B-4A9D-ADCB-9B14C6BC321D", "Invalid Message Sub Type [{0}]. Please check the message before trying again.", message.EM_MessageSubType));
						RejectMessage(message, xmlSessionTracker);
						continue;
					}

					if (!dataMessageFactory.TopLevelDataObjectFactory.TryGetTopLevelDataObject(message, codeMapper, xmlSessionTracker, out var topLevelDataObject))
					{
						RejectMessage(message, xmlSessionTracker);
						continue;
					}

					using (xmlSessionTracker.SetCurrentMessageContext(message.PK))
					using (topLevelDataObject)
					{
						if (!dataMessageFactory.UserContextExtractor.TryGetUserContext(message, topLevelDataObject, xmlSessionTracker, out var userContext))
						{
							RejectMessage(message, xmlSessionTracker);
							continue;
						}
						messageFactory.AdditionalInformation = $"Top Level Processor: {dataMessageFactory.TopLevelDataObjectProcessor.GetType()}";

						using (Env.Instance.SuppressSwitchContextCheck(ensureContextIsRestoredAfterSuppression: false))
						using (dataMessageFactory.UserContextScopeManager.EnterUserContext(userContext))
						{
							ProcessMessageWithExceptionHandling(
								message,
								topLevelDataObject,
								dataMessageFactory.TopLevelDataObjectProcessor,
								codeMapper,
								xmlSessionTracker,
								failedMessagesManager);
						}
					}

					currentMessage.Factory.DeactivateActiveCollectionsAndCaches();
					token.ThrowIfCancellationRequested();
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					if (GrEngine.Setting == GrEngineServiceSetting.KeyGen)
					{
						if (message.EM_RetryCount > eAdaptorRegistry.Instance.RetryAttemptsOnUniversalXMLProcessingRecoverableErrors.Value)
						{
							RejectMessage(message, xmlSessionTracker);
						}

						if (!(ex is System.Data.Common.DbException sqlEx && new DbErrorMatch(sqlEx).ExceptionType == DbErrorType.GeneralNetworkError))
						{
							ErrorReporter.ReportOnce("c51c9699-3fcf-4c34-8f86-eb3997b8cee9", $"KeyGen Exception occured. PK: {message.PK}", ex);
							var isolatedFactory = new BusinessObjectFactory() { RefreshEnabled = false, NameForDebugging = "KeyGen Exception" };
							var isolatedMessage = isolatedFactory.Load<EDIMessage>(message.PK);
							isolatedMessage.EM_RetryCount++;
							isolatedFactory.Save();
						}
					}
					throw;
				}
			}

			void RejectMessage(XmlEDIMessage rejectedMessage, XmlSessionTracker xmlSessionTracker)
			{
				var isolatedFactory = new BusinessObjectFactory() { RefreshEnabled = false, NameForDebugging = "Rejected Message Factory" };
				using (isolatedFactory.AddDisposableService())
				{
					var isolatedMessage = isolatedFactory.Load<EDIMessage>(rejectedMessage.PK);
					UniversalMessageProcessingManager.LogFailedMessage(isolatedMessage, MessageStatus.Rejected, xmlSessionTracker);
					isolatedFactory.Save();
				}

				if (GrEngine.Setting == GrEngineServiceSetting.KeyGen)
				{
					Log(Res.GetString("e54c60cb-b144-4378-9f34-e0f8942b872a", "Failed Key Gen Message #{0}", rejectedMessage.EM_MessageNum), LogType.Warning);
				}
				else
				{
					Log(Res.GetString("aeea34cf-dddc-4f77-9577-71b3431cd743", "Failed processing Message #{0}", rejectedMessage.EM_MessageNum), LogType.Warning);
				}
			}

			messages.Notify();
		}

		void ProcessMessageWithExceptionHandling(
			XmlEDIMessage message,
			ITopLevelDataObject topLevelDataObject,
			ITopLevelDataObjectProcessor processor,
			CodeMappingManager codeMapper,
			XmlSessionTracker xmlSessionTracker,
			FailedMessagesManager failedMessagesManager)
		{
			var importFactory = new UniversalObjectFactory(new BusinessObjectFactory() { NameForDebugging = "Universal Message Processing" }, delayLogSaveResult: true);
			var messageFactory = message.Factory;

			bool shouldSaveMessageFactory = false;

			void ProcessMessage()
			{
				try
				{
					using (var trans = eAdaptorRegistry.Instance.UniversalXMLExtendedTransactionProtectionEnabled.Value ? messageFactory.DelayedTransaction() : null)
					{
						message.ProcessUniversalMessage(
							importFactory,
							xmlSessionTracker,
							topLevelDataObject,
							processor,
							true,
							codeMapper,
							trans);

						if (SaveAtEndOfImport(importFactory, messageFactory, message))
						{
							trans?.CommitTransaction();
						}
						else
						{
							shouldSaveMessageFactory = true;
						}
					}
				}
				catch (MessageProcessingBusinessFailureException ex)
				{
					Log(Res.GetString("7bc7e089-b19e-4b3c-a4ed-6bc148545235", "Exception processing message {0}: [{1}]", message.EM_MessageNum, ex.Message), LogType.Warning);
					failedMessagesManager.AddExceptionLogNote(messageFactory, message, ex);
					if (!ex.ShouldRetry || message.EM_RetryCount >= 2)
					{
						message.EM_Status = EDIMessage.Status.Rejected;
						new ImportResultNotifier(message, xmlSessionTracker).Notify();
						messageFactory.Save();
					}
					else
					{
						messageFactory.Save();
					}
				}
			}

			using (importFactory.BOFactory.AddDisposableService())
			using (messageFactory.AddDisposableService())
			{
				try
				{
					importFactory.BOFactory.SuspendValidation();
					var contextSwitchLogger = eAdaptorRegistry.Instance.MessageUserContextTracingEnabled.Value ? new UserContextSwitchLogger() : null;

					using (contextSwitchLogger != null ? Env.StartContextSwitchTrace(contextSwitchLogger) : null)
					using (importFactory.BOFactory.ServiceContainer.AddService(new WorkflowUserContextManager()).SetWorkflowUserContext(Env.CurrentUserContext, contextSwitchLogger))
					{
						if (GrEngine.IsEnabled)
						{
							// Only UMQ should ever save in GrEngine mode
							if (GrEngine.Setting == GrEngineServiceSetting.Worker)
							{
								ProcessMessage();
							}
							else if (GrEngine.ShouldCreateKeys)
							{
								Log(string.Format(CultureInfo.InvariantCulture, (NoResString)"Calculating Keys for {0}", message.EM_MessageNum), LogType.Information, true);
								this.factoryService
									.GetFactory<Func<XmlEDIGrEngine, LoggingInformation, IEnumerable<string>, GrEngineKeyGenMessageProcessor>>()
									.Invoke(GrEngine, Logger, messageSubTypes).ProcessMessage(message);
							}

							GrEngine.NudgeMaster();
						}
						else
						{
							ProcessMessage();
						}
					}

					if (shouldSaveMessageFactory)
					{
						SaveMessageFactoryIfFailed();
					}

					LoggingFinishedProcessingMessage(message);
				}
				catch (DuplicateMessageException ex)
				{
					Log(string.Format(CultureInfo.InvariantCulture, (NoResString)"Universal Event with ApplicationReference {0} ignored, as it is a duplicate", ex.ApplicationReference), LogType.Information, true);
					message.EM_Status = EDIMessage.Status.Discarded;
					new ImportResultNotifier(message, xmlSessionTracker).Notify();
					messageFactory.Save();
					LoggingFinishedProcessingMessage(message);
				}
				catch (Exception e) when (!e.IsCriticalException() && !e.IsOutOfDiskSpaceException() && !e.IsUnableToCreateTempFileException())
				{
					HandleSaveExceptionBeforeRetrying(e);
					Log($"Exception processing message #{message.EM_MessageNum} individually: [{e.Message}]", LogType.Warning, true);
					var creator = ObjectFactory.Get<ITriggerActionDiagnostics>();
					failedMessagesManager.MarkMessageAsHavingException(baseMessageProcessor.GetNewFactory(), message, e, baseMessageProcessor.MessageStatusFailed, baseMessageProcessor.IsMessageQueued, Logger, xmlSessionTracker.IsCurrentMessageContextSet(message.PK), null, creator.GetUnsavedFiredTriggersInformation(importFactory.BOFactory));
					throw;
				}
				finally
				{
					messageFactory.DeactivateActiveCollectionsAndCaches();
					importFactory.BOFactory.DeactivateActiveCollectionsAndCaches();
				}
			}

			void SaveMessageFactoryIfFailed()
			{
				if (!(message.ShouldSaveResultsFromUniversalXmlProcessing()))
				{
					// The message import has failed so we still need to save the message factory for logging and updating EDIMessage state
					messageFactory.Save();
				}
			}

			void HandleSaveExceptionBeforeRetrying(Exception e)
			{
				if (e is ZSaveException)
				{
					try
					{
						var notifier = new NullNotificationHandler();
						ZExceptionReporting.HandleSaveException(e, notifier, throwOnMergeFailure: true);
						if (!notifier.Message.IsEmpty)
						{
							Log(notifier.Message, LogType.Warning, true);
						}
					}
					catch (Exception ex) when (ex.Find<ZSaveException>() != null)
					{
						// Handle by retrying
					}
				}
			}
		}

		class NullNotificationHandler : INotificationHandler
		{
			ZString message;

			public ZString Message { get => message; }

			public void ReportInformation(string message, string caption)
			{
				// Continue handling exception
			}

			public void ReportError(string message, string caption, string errorContext = null, Exception exception = null)
			{
				this.message = message + "\n" + caption;
				// Continue handling exception
			}
		}

		bool SaveAtEndOfImport(UniversalObjectFactory importFactory, BusinessObjectFactory messageFactory, XmlEDIMessage message)
		{
			if (message.ShouldSaveResultsFromUniversalXmlProcessing())
			{
				// This is a workaround until saves have all migrated to this top level.
				// At this point, UniversalShipment and others have already saved the importFactory or else thrown an exception,
				// however the update to the message has not yet saved so we must save again here.
				// UniversalEvent has not yet saved, however we do not want the UniversalObjectFactory save handling
				// as it is inconsistent with current behaviour. UniversalObjectFactory will swallow exceptions, and we
				// need to use the baseMessageProcessor exception handling.
				//
				// Note that this will prevent PostSaveActions from executing in the case of a UniversalEvent, however
				// this was already the case and we should use Factory DisposableService in future.
				importFactory.BOFactory.Save();
				messageFactory.Save();
				return true;
			}
			return false;
		}

		bool ShouldRetryOnException(int currentExceptionsCount, EDIMessage message, Exception lastException, int retryAttempts, string additionalErrorReportMessage = null)
		{
			if (currentExceptionsCount > retryAttempts)
			{
				ErrorReporter.ReportOnce($"Messages should eventually be rejected. Message Logs: {string.Join(System.Environment.NewLine, Logger.Logs)}", lastException);
				return false;
			}

			return lastException.ShouldReprocess() && baseMessageProcessor.ShouldRetryOnException(currentExceptionsCount, message, lastException, retryAttempts, additionalErrorReportMessage);
		}

		bool ShouldExitOnException(Exception exception)
		{
			if (exception.IsCriticalException())
			{
				return true;
			}

			if (exception.IsOutOfDiskSpaceException())
			{
				return true;
			}

			if (exception.IsUnableToCreateTempFileException())
			{
				return true;
			}

			if (exception is System.Data.Common.DbException sqlEx && ZExceptionExtensions.IsInfrastructureDbError(sqlEx))
			{
				return true;
			}

			if (exception.IsExceptionPresentIncludingInner<System.Data.Common.DbException>(ZExceptionExtensions.IsInfrastructureDbError, matchExactType: false))
			{
				return true;
			}

			return false;
		}

		public void FlipGrEngine()
		{
			const int MaxLoopsWithoutChanges = 3;
			var hoursBeforeDelete = eAdaptorRegistry.Instance.ParallelUMIQueueHistoryInHours.Value;
			var totalChanged = 0;
			var loopsWithNoChanges = 0;
			var shouldInitializeGrEngine = false;

			Log(string.Format(CultureInfo.InvariantCulture, (NoResString)"Releasing messages to {0}.", UMIServiceTaskWorker.CODE), LogType.Information, true);

			var stopWatch = Stopwatch.StartNew();
			var iteration = 1;
			do
			{
				Log(string.Format(CultureInfo.InvariantCulture, (NoResString)"Start of iteration {0}", iteration), LogType.Information);

				if (shouldInitializeGrEngine)
				{
					grEngine = null;
				}
				shouldInitializeGrEngine = true;
				var result = EnqueueGrEngine();
				var changed = result.NewEntities.Count + result.OldEntities.Count;
				totalChanged += changed;

				if (changed > 0)
				{
					GrEngine.NudgeKeyGen();
				}
				if (GrEngine.Enqueuer.ItemsLoaded > 0)
				{
					GrEngine.NudgeWorker();
					Log(string.Format(CultureInfo.InvariantCulture, (NoResString)"Nudge Worker. GrEngine.Capacity ({0}). ItemsLoaded ({1})", GrEngine.Capacity, GrEngine.Enqueuer.ItemsLoaded), LogType.Information);

					if (changed == 0)
					{
						if (loopsWithNoChanges == 0)
						{
							Log(Res.GetString("72111e15-f043-4e26-a8c0-8d5e00cbb26c", "Could not flip any of the [{0}] items remaining. Waiting for items to be processed.", GrEngine.Enqueuer.ItemsLoaded), LogType.Debug, true); // SuppresCodeSmell Reason = Logs are English only.
						}
						++loopsWithNoChanges;
					}
				}
				else if (changed == 0)
				{
					loopsWithNoChanges = 0;
				}
				Log(string.Format(CultureInfo.InvariantCulture, (NoResString)"End of iteration {0}. Elapsed time ({1})", iteration++, stopWatch.Elapsed), LogType.Information);
				if (stopWatch.Elapsed > TimeSpan.FromSeconds(20))
				{
					Log(string.Format(CultureInfo.InvariantCulture, (NoResString)"Break"), LogType.Information);
					break;
				}
				stopWatch.Restart();
#if DEBUG
				iterationCompletedHook?.Invoke(GrEngine);
#endif
			}
			while (loopsWithNoChanges < MaxLoopsWithoutChanges && GrEngine.Enqueuer.ItemsLoaded > 0 && totalChanged < FlipperMessagesPerExecution);

			Log(string.Format(CultureInfo.InvariantCulture, (NoResString)"Messages Released: {0} Messages Loaded: {1} ", totalChanged, GrEngine.Enqueuer.ItemsLoaded), LogType.Information, true);

			var setup = GrEngine.Setup;
			var factory = setup.Factory;
			Log(string.Format(CultureInfo.InvariantCulture, (NoResString)"Deleting messages older than {0} hours(s).", hoursBeforeDelete), LogType.Information);
			factory.DeleteOldProcessed(Logger, hoursBeforeDelete);
		}

		GrEngineEnqueuer<XmlEDIMessage, StmQueueState>.LoadEntitiesResult EnqueueGrEngine()
		{
			return GrEngine.Enqueuer.Enqueue(new BusinessObjectFactory(), new Lazy<ZQuery>(GetGrEngineWatermarkQuery), Logger);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		ZQuery GetGrEngineWatermarkQuery()
		{
			var earliestMessageWithoutKeyGenerated = new ZDBOnlyQuery(typeof(EDIMessage));

			var unqueuedStmFilter = string.Format(CultureInfo.InvariantCulture, (NoResString)"select SQS_ParentID from dbo.StmQueueState where SQS_Status in ('{0}', '{1}', '{2}')",
				QueueStatusCodes.Codes.Blocked, QueueStatusCodes.Codes.PreKey, QueueStatusCodes.Codes.Queued);
			var notInEm = string.Format(CultureInfo.InvariantCulture, "EM_PK not in ({0})", unqueuedStmFilter);
			earliestMessageWithoutKeyGenerated.AddFilterAndZSQLParameterCollection(notInEm, new ZSqlParameterCollection());
			earliestMessageWithoutKeyGenerated.AddToFilter(baseMessageProcessor.GetMessageProcessorQuery());
			baseMessageProcessor.AddApplicationCodeFilters(earliestMessageWithoutKeyGenerated);
			earliestMessageWithoutKeyGenerated.OrderBy = baseMessageProcessor.GetProcessableMessagesOrder();

			var filterText = earliestMessageWithoutKeyGenerated.FilterString;

			var sqlText = $"select TOP 1 {EDIMessage.Schema.EM_MessageNum} from dbo.{EDIMessage.Schema.TableName} where {filterText} ORDER BY {earliestMessageWithoutKeyGenerated.OrderBy} OPTION (LOOP JOIN)";

			using (var command = Db.Connection.Command(sqlText))
			{
				command.AddParameters(earliestMessageWithoutKeyGenerated.Params);
				var waterMark = command.ExecuteScalar();
				if (waterMark != null && waterMark != DBNull.Value)
				{
					return new ZQuery(StmQueueStateSchema.SQS_ParentMessageNumber, SQLComparisonOperator.LessThan, waterMark);
				}
				else
				{
					return new ZQuery();
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "do we have MAX?")]
		ZQuery GetMessagesWaitingForKeyGenerationQuery()
		{
			using (var cmd = Db.Connection.Command("SELECT MAX(SQS_ParentMessageNumber) FROM dbo.StmQueueState"))
			{
				var maxMessageNumberInQueue = cmd.ExecuteScalar();
				if (maxMessageNumberInQueue == DBNull.Value)
				{
					return new ZQuery();
				}
				return new ZQuery(EDIMessageSchema.EM_MessageNum, SQLComparisonOperator.LessThanOrEqualTo, maxMessageNumberInQueue);
			}
		}

		public static IEnumerable<(Type factoryType, Action registerFactory)> GetFactories(IFactoryService factoryService)
		{
			yield return (typeof(XmlEDIGrEngine), () =>
			{
				factoryService.RegisterFactory<Func<GrEngineServiceSetting, ISqlApplicationLockProvider, LoggingInformation, XmlEDIGrEngine>>(
						(setting, lockProvider, logger) => new XmlEDIGrEngine(setting, lockProvider, logger));
			}
			);

			yield return (typeof(GrEngineMessageProcessor), () =>
			{
				factoryService.RegisterFactory<Func<LoggingInformation, IEnumerable<string>, GrEngineMessageProcessor>>(
					(logger, messageSubTypes) =>
						new GrEngineMessageProcessor(logger, messageSubTypes, factoryService));
			}
			);

			yield return (typeof(UniversalMessageProcessor), () =>
			{
				factoryService.RegisterFactory<Func<LoggingInformation, IEnumerable<string>, UniversalMessageProcessor>>(
					(logger, excludedMessageSubTypes) =>
						new UniversalMessageProcessor(logger, excludedMessageSubTypes, factoryService));
			}
			);

			yield return (typeof(GrEngineKeyGenMessageProcessor), () =>
			{
				factoryService.RegisterFactory<Func<XmlEDIGrEngine, LoggingInformation, IEnumerable<string>, GrEngineKeyGenMessageProcessor>>(
					(grEngine, logger, messageSubTypes) =>
						new GrEngineKeyGenMessageProcessor(grEngine, logger, messageSubTypes, factoryService));
			}
			);
		}

		List<ApplicationTypeMessageProcessor> GetMessageProcessors()
		{
			var result = new List<ApplicationTypeMessageProcessor>();
			if (GrEngine.IsEnabled)
			{
				result.Add(this.factoryService
					.GetFactory<Func<LoggingInformation, IEnumerable<string>, GrEngineMessageProcessor>>()
					.Invoke(Logger, messageSubTypes));
			}
			else
			{
				result.Add(this.factoryService
					.GetFactory<Func<LoggingInformation, IEnumerable<string>, UniversalMessageProcessor>>()
					.Invoke(Logger, excludedMessageSubTypes));
			}
			return result;
		}

		DisposableBatch DequeueMessages(ZQuery query)
		{
			if (GrEngine.IsEnabled)
			{
				return GetGrEngineBatch(query);
			}
			else
			{
				baseMessageProcessor.AddApplicationCodeFilters(query);
				return new DisposableBatch(baseMessageProcessor.GetNewFactory().Load<XmlEDIMessage>(query));
			}
		}

		DisposableBatch GetGrEngineBatch(ZQuery query)
		{
			if (GrEngine.ShouldCreateKeys)
			{
				var totalBatch = baseMessageProcessor.CreateEmptyBatch();

				// We split the pre-key query into application code filters to utilise an index
				foreach (var processor in baseMessageProcessor.MessageProcessors)
				{
					var applicationCodeFilter = new ZQuery();
					var clonedQuery = query.DeepClone();

					applicationCodeFilter.AddToFilter(processor.MessageFilter);
					clonedQuery.AddToFilter(applicationCodeFilter);

					// We need to force SQL server to use this index or else we sometimes get a plan that scans EDIMessage table invalidating the locking strategy
					clonedQuery.TableIndexHints.Add(new TableIndexHint("NR_RX__EM_ApplicationCode_EM_ReceiveTransmit_EM_MessageNum_EM_SystemCreateTimeUtc"));
					clonedQuery.QueryHints |= QueryHints.MAXDOP1; // Parallelism leads performance problems for this query
					var batch = CreatePreKeyBatch(baseMessageProcessor.GetNewFactory(), clonedQuery);
					totalBatch = totalBatch.Concat(batch);
				}

				return totalBatch;
			}
			else if (GrEngine.ShouldDequeue(null))
			{
				baseMessageProcessor.AddApplicationCodeFilters(query);
				return DequeueNextBatchWithExceptionHandling(baseMessageProcessor.GetNewFactory(), query);
			}
			else
			{
				throw new InvalidOperationException("Somethings gone wrong. This task can neither enqueue or dequeue.");
			}
		}

		DisposableBatch CreatePreKeyBatch(BusinessObjectFactory factory, ZQuery query)
		{
			void LogLoadedItems(XmlEDIMessage[] messages)
			{
				if (messages.Length > 0 && GrEngine.Setup.LogOptions.AllKeygenLoads)
				{
					Logger.Log(FormattableString.Invariant($"Messages loaded ({messages.Length}): {string.Join(", ", messages.Select(m => m.EM_MessageNum))}"));
				}
			}

			void LogLockTaken(string lockStr)
			{
				if (GrEngine.Setup.LogOptions.AllKeygenLocks)
				{
					Logger.Log(FormattableString.Invariant($"Lock taken: {lockStr}"));
				}
			}

			var lockLogger = new AppLockLogger<XmlEDIMessage>(LogLockTaken, LogLoadedItems);
			var result = GrEngine.PreEnqueuer.LoadPreKeyBatch(factory, nameof(CreatePreKeyBatch), query, GetCheckFilter(), GrEngine.BatchSize, lockLogger, GetMessagesWaitingForKeyGenerationQuery);
			return new DisposableBatch(result.Values.ToArray(), new NullNotifiedDisposable(result));
		}

		ZQuery GetCheckFilter()
		{
			var prequeuedQuery = new ZDBOnlyQuery(typeof(XmlEDIMessage));
			prequeuedQuery.AddFilterAndZSQLParameterCollection(
				FormattableString.Invariant($"{EDIMessageSchema.Constants.PK} not in (select {StmQueueStateSchema.Constants.SQS_ParentID} from dbo.StmQueueState where {StmQueueStateSchema.Constants.SQS_Status} in ('{QueueStatusCodes.Codes.PreKey}', '{QueueStatusCodes.Codes.Queued}', '{QueueStatusCodes.Codes.Blocked}'))"),
				new ZSqlParameterCollection()
			);

			return prequeuedQuery;
		}

		DisposableBatch DequeueNextBatchWithExceptionHandling(BusinessObjectFactory factory, ZQuery filter)
		{
			var batch = GrEngine.Dequeuer.LoadBatch(factory, Logger);
			try
			{
				var unfilteredSet = batch.Load<XmlEDIMessage>(factory);
				var messages = new List<XmlEDIMessage>();
				var queuedStms = new List<StmQueueState>();
				var unqueuedStms = new List<StmQueueState>();
				foreach (var pair in unfilteredSet.Join(batch, m => m.PK, s => s.ParentID, (message, stmQueue) => new { Message = message, StmQueue = stmQueue }))
				{
					if (pair.Message.MatchesFilter(filter))
					{
						queuedStms.Add(pair.StmQueue);
						messages.Add(pair.Message);
					}
					else
					{
						unqueuedStms.Add(pair.StmQueue);
					}
				}

				GrEngine.Dequeuer.Notify(unqueuedStms.Select(s => new QueueStateResult<StmQueueState>(s, QueueStateResultType.Failed)));

				return new DisposableBatch(messages.ToArray(), new MessageStatusChangeNotifier<StmQueueState>(queuedStms.ToArray(), GrEngine.Dequeuer, batch));
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				try
				{
					batch.Dispose();
				}
				catch (Exception panicEx) when (!panicEx.IsCriticalException())
				{
					ExceptionReporter.Instance.ReportException("7bbef349-dd73-46b9-9b48-1a3aed97b286", panicEx);
				}
				throw;
			}
		}

		public void Dispose()
		{
			baseMessageProcessor.Dispose();
			grEngine?.Dispose();
		}

		sealed class MessageProcesorExploder : BaseMessageProcessor<XmlEDIMessage>
		{
			readonly GetMessageProcessorsFunc getMessageProcessorsFunc;

			public delegate List<ApplicationTypeMessageProcessor> GetMessageProcessorsFunc();

			public MessageProcesorExploder(GetMessageProcessorsFunc getMessageProcessorsFunc)
			{
				this.getMessageProcessorsFunc = getMessageProcessorsFunc;
			}

			public new List<ApplicationTypeMessageProcessor> MessageProcessors => base.MessageProcessors;
			public new DisposableBatch CreateEmptyBatch() => base.CreateEmptyBatch();
			public new BusinessObjectFactory GetNewFactory() => base.GetNewFactory();
			public new bool IsMessageQueued(EDIMessage message) => base.IsMessageQueued(message);
			public new string MessageStatusFailed => EDIMessage.Status.Rejected;

			protected override List<ApplicationTypeMessageProcessor> GetMessageProcessors()
			{
				return getMessageProcessorsFunc();
			}

			protected override int MessagesPerSaveCore => 1;
			protected override ZQuery ValidBranchesForMessageFilter => new ZQuery();
		}

		partial void MessageProcessingHookForTest(string debugName, XmlEDIMessage message);
	}

#if DEBUG

	sealed partial class UniversalProcessingManager
	{
		internal Lazy<ZQuery> GetGrEngineWatermarkQueryForTest()
		{
			return new Lazy<ZQuery>(GetGrEngineWatermarkQuery);
		}

		internal ZQuery GetMessagesWaitingForKeyGenerationQueryForTest()
		{
			return GetMessagesWaitingForKeyGenerationQuery();
		}
		internal void ExecuteBatch()
		{
			ExecuteBatch(CancellationToken.None);
		}

		partial void LogIfDebug(string message, LogType logType)
		{
			Logger.Log(message, logType);
		}

		internal DisposableBatch GetMessagesForTest()
		{
			return DequeueMessages(baseMessageProcessor.GetMessageProcessorQuery());
		}

		static readonly Overridable<Action<string, XmlEDIMessage>> MessageProcessingHookAction = new Overridable<Action<string, XmlEDIMessage>>();

		public static void SetMessageProcessingHookForTest(Action<string, XmlEDIMessage> action)
		{
			MessageProcessingHookAction.Value = action;
		}

		partial void MessageProcessingHookForTest(string debugName, XmlEDIMessage message)
		{
			if (MessageProcessingHookAction.Value != null)
			{
				MessageProcessingHookAction.Value.Invoke(debugName, message);
			}
		}

		[ThreadSafe]
		static Action<XmlEDIGrEngine> iterationCompletedHook;

		public static void SetIterationCompletedHookForTest(Action<XmlEDIGrEngine> action)
		{
			iterationCompletedHook = action;
		}
	}

#endif
}
