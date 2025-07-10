using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.Registry.Business.eServices;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.Messaging.Business
{
	public interface IMessageProcessorEvents
	{
		event Action BatchOfMessagesProcessed;
	}

	public abstract class BaseMessageProcessor : BaseMessageProcessor<EDIMessage>
	{
		protected BaseMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected BaseMessageProcessor()
		{ }
	}

	public class BaseMessageProcessor<T> : BatchProcess, IProcessor, IMessageProcessorEvents, IEDocsDelayedSaver where T : EDIMessage
	{
		public delegate DisposableBatch DequeueMessagesFunc(ZQuery query);

		protected BaseMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
			messagesAfterLastSave = 0;
			individualMessagesToBeProcessedCount = 0;
		}

		protected BaseMessageProcessor()
			: base()
		{
			messagesAfterLastSave = 0;
			individualMessagesToBeProcessedCount = 0;
		}

		protected List<ApplicationTypeMessageProcessor> MessageProcessors
		{
			get { return messageProcessors ?? (messageProcessors = GetMessageProcessors()); }
		}

		List<ApplicationTypeMessageProcessor> messageProcessors;

		/// <summary>
		/// Return a list of 
		/// </summary>
		/// <returns></returns>
		protected virtual List<ApplicationTypeMessageProcessor> GetMessageProcessors() => new List<ApplicationTypeMessageProcessor>();

		protected virtual string MessageStatusFailed
		{
			get { return EDIMessage.Status.Failed; }
		}

		int individualMessagesToBeProcessedCount;

		protected virtual IDisposable TrySwitchUserContext(T message)
		{
			return new DisposableAction(() => { });
		}

		protected override void Execute(CancellationToken token)
		{
			failedMessagesManager = new FailedMessagesManager(RetryType.NextExecution, ShouldRetryOnException);
			var totalItemsProcessed = 0;
			do
			{
				token.ThrowIfCancellationRequested();
				using (currentMessagesBatch = RetrieveNextProcessableMessages(DequeueMessages, failedMessagesManager))
				{
					totalItemsProcessed += currentMessagesBatch.ItemsInBatchCount;

					if (currentMessagesBatch.Length > 0)
					{
						try
						{
							ProcessBatch(currentMessagesBatch);
						}
						finally
						{
							BatchOfMessagesProcessed();
						}
					}
				}
			}
			while (totalItemsProcessed < MessagesPerExecution && !currentMessagesBatch.IsLastBatch);
			currentMessagesBatch = null;
		}
		protected DisposableBatch currentMessagesBatch;

		protected virtual bool MessageShouldBeProcessedInASeparateFactory
		{
			get { return false; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Is for diagnostic purposes")]
		void ProcessBatch(DisposableBatch messages)
		{
			var startTotalMemory = GC.GetTotalMemory(false);
			try
			{
				using (new LogDiagnosticTicks("Process Batch of messages", Logger))
				{
					ProcessBatchCore(messages, startTotalMemory);
				}

				messages.Notify();
			}
			catch (BatchProcessorOperationCancelledException)
			{
				throw;
			}
			catch (Exception e) when (!e.IsCriticalException() || e.Find<SqlLockLostException>() != null)
			{
				if (messagesAfterLastSave > 1)
				{
					Logger.LogWarning(Invariant($"Exception processing a group of {messagesAfterLastSave} messages: [{e.Message}]"));
					individualMessagesToBeProcessedCount = messagesAfterLastSave;
				}
			}
		}

		protected virtual void ProcessBatchCore(DisposableBatch messages, long startTotalMemory) => ProcessMessages(messages, startTotalMemory);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Is for diagnostic purposes)")]
		protected bool ProcessMessages(DisposableBatch messages, long startTotalMemory)
		{
			var processInASeparateFactory = MessageShouldBeProcessedInASeparateFactory;
			var indexOfLastMessage = messages.Length - 1;
			var continueProcessing = true;
			for (int i = 0; continueProcessing && i < messages.Length; i++)
			{
				var message = messages[i];

				using (PerformanceStatisticsCollector.StartMonitoring("ProcessMessage", message.EM_ApplicationCode + "-" + message.EM_MessageType + "-" + message.EM_MessageSubType))
				{
					if (message.EM_HeldUntilDate.IsEmpty || message.EM_HeldUntilDate <= ZDateTime.UtcNow)
					{
						var processor = GetApplicationTypeProcessor(message);
						var newFactory = processInASeparateFactory ? GetNewFactory() : null;
						using (newFactory?.AddDisposableService())
						{
							var messageToProcess = processInASeparateFactory ? newFactory.Load<T>(message.PK) : message;
							var forceSave = processInASeparateFactory || i == indexOfLastMessage;

							continueProcessing = ProcessMessage(processor, messageToProcess, startTotalMemory, forceSave, ProcessMessageCore, "process");
						}
					}
				}
			}
			return continueProcessing;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Is for diagnostic purposes")]
		protected internal bool ProcessMessage(ApplicationTypeMessageProcessor processor, T message, long startTotalMemory, bool forceSave, Action<ApplicationTypeMessageProcessor, T> processMessageFunc, ZString actionNameForLogging)
		{
			bool memoryConsumptionOverThreshold = false;
			messagesAfterLastSave++;
			try
			{
				using (new LogDiagnosticTicks(actionNameForLogging + " Message: " + message.EM_MessageNum, Logger))
				{
					processMessageFunc(processor, message);

					var memoryDiff = GC.GetTotalMemory(false) - startTotalMemory;
					var memoryThreshold = (int)ObjectFactory.Get<Enterprise.Integration.Customs.Shared.ICustomsDataRegistry>().MemoryThresholdForMessageProcessing.Value;
					memoryConsumptionOverThreshold = memoryDiff > memoryThreshold;

					if (messagesAfterLastSave == MessagesPerSave || forceSave || memoryConsumptionOverThreshold)
					{
						using (TrySwitchUserContext(message))
						{
							SaveAfterProcessingMessages(message.Factory, actionNameForLogging);
						}

						if (!memoryConsumptionOverThreshold)
						{
							memoryDiff = GC.GetTotalMemory(false) - startTotalMemory;
							memoryConsumptionOverThreshold = memoryDiff > memoryThreshold;
						}
					}
				}
			}
			catch (BatchProcessorOperationCancelledException)
			{
				throw;
			}
			catch (Exception e) when (!e.IsCriticalException() && !e.IsOutOfDiskSpaceException() && !e.IsUnableToCreateTempFileException())
			{
				if (messagesAfterLastSave == 1)
				{
					Logger.LogWarning("Exception " + actionNameForLogging + "ing message #" + message.EM_MessageNum + " individually: [" + e.Message + "]");
					failedMessagesManager.MarkMessageAsHavingException(GetNewFactory(), message, e, MessageStatusFailed, IsMessageQueued, Logger, false, processor.PostProcessOnException);
				}
				throw;
			}
			finally
			{
				if (individualMessagesToBeProcessedCount > 0)
				{
					--individualMessagesToBeProcessedCount;
				}
			}

			return !memoryConsumptionOverThreshold;
		}

		protected virtual void ProcessMessageCore(ApplicationTypeMessageProcessor processor, T message)
		{
			Logger.Log(Res.GetString("8a023a1a-1e5d-4c45-ac52-a78c15a350b3", "Processing Message #{0}", message.EM_MessageNum));

			if (processor != null)
			{
				var initialHeldUntilDate = message.EM_HeldUntilDate;
				try
				{
					processor.ProcessMessage(message);
				}
				catch (MessageProcessLockException exLock)
				{
					LogInvalidHeldUntilDate(initialHeldUntilDate, exLock);
					processor.SetHeldUntilDate(message, GetUnprocessedMessages(message.PK));
				}

				if (message.EM_Status == EDIMessage.Status.ProcessedOK || message.EM_Status == EDIMessage.Status.Recognised)
				{
					NumberOfOperationsSucceeded++;
				}

				if (IsMessageQueued(message) && !MessageHeldDateHasBeenChangedToUtcFuture(initialHeldUntilDate, message))
				{
					ReportIncorrectStatus(processor, message, initialHeldUntilDate, nameof(ProcessMessageCore));
					message.EM_Status = EDIMessage.Status.Error;
				}
			}
			else
			{
				message.EM_Status = MessageStatusFailed;
			}
		}

		protected BusinessObjectFactory GetNewFactory()
		{
			var currentFactory = GetNewFactoryCore();
			currentFactory.NameForDebugging = FactoryNameForDebugging;
			currentFactory.SuspendValidation();
			currentFactory.RefreshEnabled = false;
			currentFactory.SetValue<IMessageProcessorEvents>(() => this);

			return currentFactory;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Factory NameForDebugging")]
		public const string FactoryNameForDebugging = "Message Processing";

		protected virtual BusinessObjectFactory GetNewFactoryCore()
		{
			return new BusinessObjectFactory();
		}

		protected virtual bool IsMessageQueued(EDIMessage message)
		{
			return message.EM_Status == EDIMessage.Status.Queued;
		}

		FailedMessagesManager failedMessagesManager;

		public bool ShouldRetryOnException(int currentExceptionsCount, EDIMessage message, Exception lastException, int retryAttempts, string additionalErrorReportMessage = null)
		{
			return ShouldRetryOnExceptionCore(currentExceptionsCount, message, lastException, retryAttempts, additionalErrorReportMessage);
		}

		protected virtual bool ShouldRetryOnExceptionCore(int currentExceptionsCount, EDIMessage message, Exception lastException, int retryAttempts, string additionalErrorReportMessage = null)
		{
			var shouldRetry = ShouldErrorTypeBeRetried(lastException, currentExceptionsCount >= retryAttempts);

			if (ShouldErrorTypeBeReported(lastException) && !shouldRetry)
			{
				var combineMessage = $@"Message processing failed after {retryAttempts} retries. This is likely an issue on our side.
Message Number: {message.EM_MessageNum}
Message Type: {message.EM_MessageType}
Message SubType: {message.EM_MessageSubType}
Message Status: {message.EM_Status}
Message Application Code: {message.EM_ApplicationCode}
Message Content: {message.EM_MessageText.Truncate(10000)}
";
				if (!string.IsNullOrEmpty(additionalErrorReportMessage))
				{
					combineMessage += additionalErrorReportMessage;
				}
				ExceptionReporter.Instance.ReportDeveloperException(combineMessage, lastException);
			}

			return shouldRetry;
		}

		bool ShouldErrorTypeBeRetried(Exception lastException, bool isRetryLimitReached)
		{
			if (isRetryLimitReached
			|| lastException.IsExceptionPresentIncludingInner<System.Data.Common.DbException>(x =>
				{
					var dbErrorMatch = new DbErrorMatch(x);
					return ErrorTypesToNotRetry.Contains(dbErrorMatch.ExceptionType);
				}, matchExactType: false))
			{
				return false;
			}
			return true;
		}

		bool ShouldErrorTypeBeReported(Exception lastException)
		{
			if (lastException.IsExceptionPresentIncludingInner<System.Data.Common.DbException>(x =>
				{
					var dbErrorMatch = new DbErrorMatch(x);
					return ErrorTypesToNotReport.Contains(dbErrorMatch.ExceptionType);
				}, matchExactType: false))
			{
				return false;
			}

			return true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		protected ApplicationTypeMessageProcessor GetApplicationTypeProcessor(T message)
		{
			var processor = GetApplicationTypeProcessorCore(message);
			if (processor == null)
			{
				Logger.LogError(Res.GetString("{98541AA9-C122-4E5E-A08C-88F688FDF4B5}", "Failed to locate a processor for message (Application Code: {0}, Application Reference: {1}, Message Type: {2}, Message Sub Type: {3})", message.EM_ApplicationCode, message.EM_ApplicationReference, message.EM_MessageType, message.EM_MessageSubType));
			}
			return processor;
		}

		public virtual ApplicationTypeMessageProcessor GetApplicationTypeProcessorCore(T message)
		{
			return MessageProcessors.Find(p => message.MatchesFilter(p.MessageFilter));
		}

		protected void LogInvalidHeldUntilDate(ZDateTime heldUntilDate, MessageProcessLockException exLock)
		{
			if (heldUntilDate.IsEmpty || heldUntilDate.AddMinutes(3) < ZDateTime.UtcNow)
			{
				Logger.Log(exLock.Message);
			}
		}

		protected void ReportIncorrectStatus(ApplicationTypeMessageProcessor processor, EDIMessage message, ZDateTime initialHeldUntilDate, string callFrom)
		{
			var processorFullName = processor.GetType().FullName;
			var messageDetail = $@"Call from {callFrom}:
{processorFullName} did not update a received message's EM_Status away from QUE and did not satisfactorily update EM_HeldUntilDate to a date that's both ahead of the previous value and ahead of UtcNow.
Make sure you do one of those.
Make sure you use UTC time for EM_HeldUntilDate.
EM_PK={message.PK}
EM_Status={message.EM_Status}
EM_Status[DB]={message.EM_StatusInfo.OriginalValue}
EM_LinkUniqueID={message.EM_LinkUniqueID}
EM_LinkTable={message.EM_LinkTable}
initialHeldUntilDate={initialHeldUntilDate.DebuggerDisplay}
EM_HeldUntilDate={message.EM_HeldUntilDate.DebuggerDisplay}";
			ErrorReporter.ReportOnce(callFrom + "BaseMessageProcessor-BadState-" + processorFullName, messageDetail);
		}

		protected IEnumerable<T> GetUnprocessedMessages(ZGuid lastProcessedMessagePK)
		{
			return currentMessagesBatch?.Where(m => IsMessageQueued(m) && m.PK != lastProcessedMessagePK) ?? Array.Empty<T>();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Is for diagnostic purposes")]
		void SaveAfterProcessingMessages(BusinessObjectFactory factory, ZString actionName)
		{
			using (new LogDiagnosticTicks("Save", Logger))
			{
				LogSaving();
				SaveAfterProcessingMessagesCore(factory);
				Logger.Log(messagesAfterLastSave.ToString(CultureInfo.InvariantCulture) + " message" + (messagesAfterLastSave == 1 ? " " : "s ") + actionName + "ed");
			}
			messagesAfterLastSave = 0;
		}
		int messagesAfterLastSave;

		protected virtual void LogSaving()
		{
			Logger.Log("Saving...");
		}

		protected virtual void SaveAfterProcessingMessagesCore(BusinessObjectFactory factory)
		{
			foreach (DocManagerInfo docManagerInfo in eDocsManagersQueuedForSaving)
			{
				docManagerInfo.Save();
			}

			factory.Save();
		}

		#region IEDocsDelayedSaver Implementation

		void IEDocsDelayedSaver.QueueForSaving(DocManagerInfo docManagerInfo)
		{
			eDocsManagersQueuedForSaving.Add(docManagerInfo);
		}

		readonly List<DocManagerInfo> eDocsManagersQueuedForSaving = new List<DocManagerInfo>();

		#endregion

		protected bool MessageHeldDateHasBeenChangedToUtcFuture(ZDateTime initialHeldUntilDate, T message)
		{
			return
				(
					(initialHeldUntilDate == ZDateTime.Empty && message.EM_HeldUntilDate != ZDateTime.Empty)
					||
					(initialHeldUntilDate < message.EM_HeldUntilDate)
				)
				&&
				message.EM_HeldUntilDate > ZDateTime.UtcNow;
		}

		protected BusinessObjectFactory SharedFactory => sharedFactory ?? (sharedFactory = new BusinessObjectFactory());
		BusinessObjectFactory sharedFactory;

		public static ZQuery ValidTransmitDateMessageFilterUsingUtcTime
		{
			get
			{
				var result = new ZQuery(EDIMessageSchema.EM_HeldUntilDate, SQLComparisonOperator.Equal, null);
				result.AddToFilter(JoinCondition.Or, EDIMessageSchema.EM_HeldUntilDate, SQLComparisonOperator.LessThanOrEqualTo, ZDateTime.UtcNow);
				return result;
			}
		}

		public static ZQuery AddNotInEDIMessageQueueStateQuery(ZQuery query)
		{
			var result = new ZDBOnlyQuery(typeof(EDIMessage));
			result.AddToFilter(query);
			result.AddFilterAndZSQLParameterCollection("EM_PK NOT IN (SELECT EQS_EM FROM dbo.EDIMessageQueueState WITH (INDEX(NR_RX__EQS_EM), FORCESEEK) WHERE EQS_ApplicationCode = EM_ApplicationCode)", new ZSqlParameterCollection());
			return result;
		}

		public DisposableBatch RetrieveNextProcessableMessages(DequeueMessagesFunc dequeueMessages, FailedMessagesManager failedMessagesTracker)
		{
			var query = GetMessageProcessorQuery();
			var failedMessagePKsToSkip = failedMessagesTracker.GetMessagesWithNoRetry();
			if (failedMessagePKsToSkip.Any())
			{
				query.AddToFilter(EDIMessageSchema.PK, SQLComparisonOperator.NotEqual, failedMessagePKsToSkip);
			}
			var messages = dequeueMessages(query);
			SortProcessableMessageEvenFurther(messages.Inner);
			messagesAfterLastSave = 0;
			return messages;
		}

		public ZQuery GetMessageProcessorQuery()
		{
			var query = GetQueuedQuery();
			query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
			query.AddToFilter(EDIMessageSchema.EM_IsActive, true);
			query.AddToFilter(ValidBranchesForMessageFilter);
			query.AddToFilter(ValidTransmitDateMessageFilterUsingUtcTime);
			query.MaximumRows = MessagesPerBatch;
			query.OrderBy = GetProcessableMessagesOrder();
			query.TableIndexHints.Add(new TableIndexHint(GetProcessableMessagesOrderIndexName()));
			MarkAsNoResultQueryIfNoMessageAnticipated(query);

			// We need to force MS SQL to use LOOP JOIN or at 10000 records in StmQueueState 
			// it will switch to a very slow query plan using merge join.
			query.QueryHints |= QueryHints.LOOPJOIN;
			return query;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		public void AddApplicationCodeFilters(ZQuery query)
		{
			ZQuery applicationCodeFilter = new ZQuery();
			foreach (ApplicationTypeMessageProcessor processor in MessageProcessors)
			{
				applicationCodeFilter.AddToFilter(processor.MessageFilter, JoinCondition.Or);
			}

			query.AddToFilter(applicationCodeFilter);
		}

		void MarkAsNoResultQueryIfNoMessageAnticipated(ZQuery query)
		{
			if (!HasAnyMessageAnticipated())
			{
				query.IsNoResultQuery = true;
			}
		}

		protected virtual bool HasAnyMessageAnticipated()
		{
			return MessageProcessors.Count > 0;
		}

		protected virtual ZQuery GetQueuedQuery() => new ZQuery(EDIMessageSchema.EM_Status, EDIMessage.Status.Queued);

		protected virtual DisposableBatch DequeueMessages(ZQuery query)
		{
			AddApplicationCodeFilters(query);
			var factory = GetNewFactory();
			return new DisposableBatch(factory.Load<T>(query), new NullNotifiedDisposable(factory.AddDisposableService()));
		}

		protected virtual void SortProcessableMessageEvenFurther(T[] messages)
		{
			Array.Sort(messages, EDIMessageComparer);
		}

		protected virtual EDIMessageComparer EDIMessageComparer => new EDIMessageComparer(System.ComponentModel.ListSortDirection.Ascending);

		protected virtual EDIMessageOrder MessageOrder => EDIMessageOrder.Number;

		public string GetProcessableMessagesOrder()
		{
			switch (MessageOrder)
			{
				case EDIMessageOrder.Number:
					return EDIMessageSchema.Constants.EM_MessageNum + ", " + EDIMessageSchema.Constants.EM_SystemCreateTimeUtc;
				case EDIMessageOrder.CreateTime:
					return EDIMessageSchema.Constants.EM_SystemCreateTimeUtc + ", " + EDIMessageSchema.Constants.EM_MessageNum;
				default:
					throw new InvalidOperationException(Invariant($"Unexpected MessageOrder: {MessageOrder}."));
			}
		}

		string GetProcessableMessagesOrderIndexName()
		{
			switch (MessageOrder)
			{
				case EDIMessageOrder.Number:
					return EDIMessageSchema.Constants.Indexes.NR_RX__EM_ApplicationCode_EM_ReceiveTransmit_EM_MessageNum_EM_SystemCreateTimeUtc;
				case EDIMessageOrder.CreateTime:
					return EDIMessageSchema.Constants.Indexes.NR_RX__EM_ApplicationCode_EM_ReceiveTransmit_EM_SystemCreateTimeUtc_EM_MessageNum;
				default:
					throw new InvalidOperationException(Invariant($"Unexpected MessageOrder: {MessageOrder}."));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Error reporting")]
		protected virtual ZQuery ValidBranchesForMessageFilter
		{
			get
			{
				var company = GlbCompany.GetCurrentCompany(SharedFactory);
				if (company == null)
				{
					var envCompany = Env.CurrentCompany;
					if (envCompany != null)
					{
						company = new BusinessObjectFactory { NameForDebugging = "Factory to load environment company" }.Load<GlbCompany>(envCompany.PK);
					}

					if (company == null)
					{
						var message = string.Format(
@"Current environment company PK is {0}
Current environment branch PK is {1}
Current environment user context is {2}",
										 (Env.CurrentCompany != null ? Env.CurrentCompany.PK : Guid.Empty),
										 (Env.CurrentBranch != null ? Env.CurrentBranch.PK : Guid.Empty),
										 (Env.CurrentUserContext != null && Env.CurrentUserContext.User != null ? Env.CurrentUserContext.User.LoginName : ""));
						ErrorReporter.ReportOnce("4712113A-B534-418C-B515-53450781670B", message);
					}
				}

				var key = company.PK;

				if (!BranchesForMessageFilterList.TryGetValue(key, out var result))
				{
					var branchQuery = new ZDBOnlySubQuery(typeof(GlbBranch), GlbBranchSchema.PK);
					branchQuery.AddToFilter(GlbBranchSchema.GB_GC, key);

					result = new ZDBOnlyQuery(typeof(EDIMessage));
					((ZDBOnlyQuery)result).AddSubQuery(EDIMessageSchema.EM_GB, branchQuery, JoinCondition.And);

					BranchesForMessageFilterList.Add(key, result);
				}

				return result;
			}
		}

		static readonly ImmutableArray<DbErrorType> ErrorTypesToNotRetry = ImmutableArray.Create(
			DbErrorType.ArithmeticOverflowConvertingToDataType,
			DbErrorType.CannotConvertDataType

		);

		static readonly ImmutableArray<DbErrorType> ErrorTypesToNotReport = ImmutableArray.Create(
			DbErrorType.LockTimeoutExpired,
			DbErrorType.InsertConflictedWithCheckConstraint,
			DbErrorType.UpdateConflictedWithCheckConstraint,
			DbErrorType.InsertConflictedWithForeignKey,
			DbErrorType.UpdateConflictedWithForeignKey
		);

		Dictionary<ZGuid, ZQuery> BranchesForMessageFilterList
		{
			get { return branchesForMessageFilterList ?? (branchesForMessageFilterList = new Dictionary<ZGuid, ZQuery>()); }
		}
		Dictionary<ZGuid, ZQuery> branchesForMessageFilterList;

		int MessagesPerSave
		{
			get { return individualMessagesToBeProcessedCount > 0 ? 1 : MessagesPerSaveCore; }
		}

		protected virtual int MessagesPerSaveCore
		{
			get { return 50; }
		}

		static int MessagesPerBatch => eAdaptorRegistry.Instance.MessagesPerBatch.Value;
		protected static int MessagesPerExecution => eAdaptorRegistry.Instance.MessagesPerExecution.Value;

		void IProcessor.Process(INotifications notifications, CancellationToken token)
		{
			LoggingInformation previousLogger = Logger;
			try
			{
				Logger = new NotificationSubscriberLogger(notifications);
				ExecuteBatch(token);
			}
			finally
			{
				Logger = previousLogger;
			}
		}

		public event Action BatchOfMessagesProcessed = () => { };

		protected DisposableBatch CreateEmptyContinuationBatch(int itemsLoaded) => DisposableBatch.CreateEmptyContinuationBatch(itemsLoaded);

		protected DisposableBatch CreateEmptyBatch() => DisposableBatch.CreateEmptyBatch();

		/// <summary>
		/// This class is disposable so that consumers can use locking mechanisms to avoid doubling up on messages during parallelisation.
		/// </summary>
		public class DisposableBatch : INotifiedDisposable, IEnumerable<T>
		{
			DisposableBatch(bool isLastBatch, int itemsInBatchCount)
			{
				Inner = Array.Empty<T>();
				IsLastBatch = isLastBatch;
				ItemsInBatchCount = itemsInBatchCount;
			}

			public DisposableBatch(T[] set, params INotifiedDisposable[] disposableChain)
			{
				this.Inner = set;
				IsLastBatch = set.Length == 0;
				ItemsInBatchCount = set.Length;
				this.disposables.AddRange(disposableChain);
			}

			readonly List<INotifiedDisposable> disposables = new List<INotifiedDisposable>();

			#region public API

			internal T[] Inner { get; }

			public int Length => Inner.Length;

			public bool IsLastBatch { get; }
			public int ItemsInBatchCount { get; }

			public T this[int index]
			{
				get { return Inner[index]; }
			}

			public DisposableBatch Concat(DisposableBatch batch)
			{
				var set = this.Inner.Concat(batch.Inner).ToArray();
				return new DisposableBatch(set, this.disposables.Concat(batch.disposables).ToArray());
			}

			#endregion

			#region IEnumerable

			public IEnumerator<T> GetEnumerator()
			{
				return ((IEnumerable<T>)Inner).GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			#endregion

			#region IDisposable Support

			public void Notify()
			{
				foreach (var disposable in disposables)
				{
					disposable.Notify();
				}
			}

			bool disposedValue;

			protected virtual void Dispose(bool disposing)
			{
				if (!disposedValue)
				{
					if (disposing)
					{
						foreach (var disposable in disposables)
						{
							disposable.Dispose();
						}
					}

					disposedValue = true;
				}
			}

			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}

			#endregion

			#region static members

			/// <summary>
			/// The continuation batch indicates that the processor should loop again even though there are no items to be processed.
			/// </summary>
			internal static DisposableBatch CreateEmptyContinuationBatch(int itemsLoaded) => new DisposableBatch(false, itemsLoaded);

			internal static DisposableBatch CreateEmptyBatch() => new DisposableBatch(true, 0);

			#endregion
		}
	}
}
