//#define PERFORMANCE_TESTING

using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Messaging.Business
{
	public abstract class BranchMessageProcessor : BaseMessageProcessor<EDIMessage>
	{
		protected BranchMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected BranchMessageProcessor()
		{
		}

		Dictionary<ZGuid, ApplicationTypeMessageProcessor> MessageToMessageProcessorMappings => messageToMessageProcessorMappings ?? (messageToMessageProcessorMappings = new Dictionary<ZGuid, ApplicationTypeMessageProcessor>());
		Dictionary<ZGuid, ApplicationTypeMessageProcessor> messageToMessageProcessorMappings;

		public override ApplicationTypeMessageProcessor GetApplicationTypeProcessorCore(EDIMessage message)
		{
			if (!MessageToMessageProcessorMappings.TryGetValue(message.PK, out var processor))
			{
				processor = base.GetApplicationTypeProcessorCore(message);
				MessageToMessageProcessorMappings.Add(message.PK, processor);
			}
			return processor;
		}

		protected override ZQuery GetQueuedQuery()
		{
			var result = base.GetQueuedQuery();
			result.AddToFilter(JoinCondition.Or, EDIMessageSchema.EM_Status, EDIMessage.Status.PreProcessedOK);
			return result;
		}

		protected override bool IsMessageQueued(EDIMessage message)
		{
			return message.EM_Status == EDIMessage.Status.Queued || message.EM_Status == EDIMessage.Status.PreProcessedOK;
		}

		protected override void ProcessBatchCore(DisposableBatch messages, long startTotalMemory)
		{
			try
			{
				var messagesGroupedByBranch = PreProcessMessages(messages, startTotalMemory);
				if (messagesGroupedByBranch != null)
				{
					ProcessMessagesInCorrectBranch(messagesGroupedByBranch, startTotalMemory);
				}
			}
			finally
			{
				MessageToMessageProcessorMappings.Clear();
			}
		}

		void ProcessMessagesInCorrectBranch(Dictionary<ZGuid, List<EDIMessage>> messagesGroupedByBranch, long startTotalMemory)
		{
			foreach (var messagesByBranch in messagesGroupedByBranch)
			{
				var branchPk = messagesByBranch.Key;
				var branchMessages = messagesByBranch.Value;

				using (branchPk.IsValid && branchPk != GlbBranch.CurrentBranch.PK ? DisposableEnvironment.ForBranch(branchPk.ToGuid()) : null)
				{
					if (!ProcessMessages(new DisposableBatch(branchMessages.ToArray()), startTotalMemory))
					{
						break;
					}
				}
			}
		}

		Dictionary<ZGuid, List<EDIMessage>> PreProcessMessages(DisposableBatch messages, long startTotalMemory)
		{
			var messagesGroupedByBranch = new Dictionary<ZGuid, List<EDIMessage>>();
			var processInASeparateFactory = MessageShouldBeProcessedInASeparateFactory;
			var indexOfLastMessage = messages.Length - 1;
			var failedMessagePKs = new List<ZGuid>();
			for (int i = 0; i < messages.Length; i++)
			{
				var message = messages[i];

				using (PerformanceStatisticsCollector.StartMonitoring("PreProcessMessage", message.EM_ApplicationCode + "-" + message.EM_MessageType + "-" + message.EM_MessageSubType))
				{
					if (message.EM_Status == EDIMessage.Status.Queued)
					{
						var processor = GetApplicationTypeProcessor(message);
						if (processor == null)
						{
							failedMessagePKs.Add(message.PK);
						}
						else if (processor.RequiresPreProcessing)
						{
							Logger.Log(Res.GetString("E7FFCF01-0E24-432B-A530-79FD2E92B2F3", "Pre-Process Message #{0}", message.EM_MessageNum));
							var messageToProcess = processInASeparateFactory ? GetNewFactory().Load<EDIMessage>(message.PK) : message;
							var forceSave = processInASeparateFactory || i == indexOfLastMessage;
							if (!ProcessMessage(processor, messageToProcess, startTotalMemory, forceSave, PreProcessMessageCore, "pre-process"))
							{
								messagesGroupedByBranch = null;
								break;
							}
							if (processInASeparateFactory)
							{
								message.EM_GB = messageToProcess.EM_GB;
								message.EM_Status = messageToProcess.EM_Status;
								message.EM_HeldUntilDate = messageToProcess.EM_HeldUntilDate;
							}
						}
					}

					if (
						!failedMessagePKs.Contains(message.PK) &&
						!((string)message.EM_Status.ToUpper()).In(MessageStatusFailed, EDIMessage.Status.Error, EDIMessage.Status.Discarded) &&
						(message.EM_HeldUntilDate.IsEmpty || message.EM_HeldUntilDate <= ZDateTime.UtcNow))
					{
						var branchPK = message.EM_GB;
						if (!messagesGroupedByBranch.TryGetValue(branchPK, out var branchList))
						{
							branchList = new List<EDIMessage>();
							messagesGroupedByBranch.Add(branchPK, branchList);
						}
						branchList.Add(message);
					}
				}
			}
			if (failedMessagePKs.Count > 0)
			{
				var newFactory = GetNewFactory();
				newFactory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.PK, failedMessagePKs)).ForEach(x => x.EM_Status = MessageStatusFailed);
				try
				{
					newFactory.Save();
				}
				catch (ZSaveException ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
				}
			}

			return messagesGroupedByBranch;
		}

		protected void PreProcessMessageCore(ApplicationTypeMessageProcessor processor, EDIMessage message)
		{
			var initialHeldUntilDate = message.EM_HeldUntilDate;
			try
			{
				processor.PreProcessMessage(message);
			}
			catch (MessageProcessLockException exLock)
			{
				LogInvalidHeldUntilDate(initialHeldUntilDate, exLock);
				processor.SetHeldUntilDate(message, GetUnprocessedMessages(message.PK));
			}

			if (message.EM_Status == EDIMessage.Status.Queued && !MessageHeldDateHasBeenChangedToUtcFuture(initialHeldUntilDate, message))
			{
				ReportIncorrectStatus(processor, message, initialHeldUntilDate, nameof(PreProcessMessageCore));
				message.EM_Status = EDIMessage.Status.Error;
			}
		}

		protected virtual bool ExcludeBranchFilter => false;
		protected override ZQuery ValidBranchesForMessageFilter => ExcludeBranchFilter ? new ZQuery() : base.ValidBranchesForMessageFilter;

#if DEBUG
		public bool ExcludeBranchFilter_Exposed => ExcludeBranchFilter;
		public ZQuery ValidBranchesForMessageFilter_Exposed => ValidBranchesForMessageFilter;
#endif
	}
}
