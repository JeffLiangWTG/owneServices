using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Dash.Business;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using WTG.Shared.Dash.Common;

namespace Enterprise.Dash.ServiceTasks.MessageProcessors
{
	public abstract class DashMessageProcessor
	{
		protected DashMessageProcessor()
			: this(null, null, null, null)
		{
		}

		protected DashMessageProcessor(IFactory factory, ILogger serviceLogger, IShipamaxService shipamaxService, IDashErrorReporter errorReporter)
		{
			Factory = factory;
			ServiceLogger = serviceLogger;
			ShipamaxService = shipamaxService;
			DashErrorReporter = errorReporter;
		}

		protected IFactory Factory { get; }

		protected ILogger ServiceLogger { get; }
		public IShipamaxService ShipamaxService { get; }
		public IDashErrorReporter DashErrorReporter { get; }
		protected abstract int BatchSize { get; }

		protected abstract byte MaxRetryCount { get; }

		protected List<ZGuid> ExcludedPKs { get; } = [];

		public virtual void ProcessMessages(CancellationToken token)
		{
			if (!PreConditionsMet())
			{
				return;
			}

			ExcludedPKs.Clear();

			var totalProcessedMessages = 0;
			var totalFailedMessages = 0;
			var loadNextBatch = true;

			while (loadNextBatch)
			{
				token.ThrowIfCancellationRequested();

				var batch = Factory.Load<DashDocumentDataMessage>(QueryForLoadingMessages);

				if (batch.Length > 0)
				{
					ServiceLogger.Information(FormattableString.Invariant($"Loaded and started processing {batch.Length} of DashDocumentDataMessage records."));

					var messagesProcessedInCurrentBatch = 0;
					var failedMessagesInCurrentBatch = 0;

					foreach (var dashDocumentDataMessage in batch)
					{
						token.ThrowIfCancellationRequested();

						try
						{
							ProcessMessage(Factory, dashDocumentDataMessage, token);
							messagesProcessedInCurrentBatch++;
							dashDocumentDataMessage.EM_Status = EDIMessageStatusList.Codes.ProcessedOK;
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							var willBeReprocessed = dashDocumentDataMessage.EM_RetryCount < MaxRetryCount;
							var errorMessage = FormattableString.Invariant($"An exception occurred while processing DashDocumentDataMessage record '{dashDocumentDataMessage.PK}'.");
							LogErrorAndMarkMessagePKExcluded(ex.Message, dashDocumentDataMessage.PK, willBeReprocessed);
							using (DashErrorReporter.GatherAdditionalInformation(dashDocumentDataMessage))
							{
								ErrorReporter.ReportOnce($"{GetType()}_RunningError", errorMessage, ex);
							}
							failedMessagesInCurrentBatch++;

							if (!willBeReprocessed)
							{
								dashDocumentDataMessage.EM_Status = EDIMessageStatusList.Codes.Failed;
							}
							else
							{
								dashDocumentDataMessage.EM_RetryCount++;
							}
						}

						FinalizeMessageProcessing(Factory, dashDocumentDataMessage);

						Factory.Save();
					}

					totalProcessedMessages += messagesProcessedInCurrentBatch;
					totalFailedMessages += failedMessagesInCurrentBatch;
					ServiceLogger.Information(FormattableString.Invariant($"{messagesProcessedInCurrentBatch} message(s) successfully processed in current batch."));

					if (failedMessagesInCurrentBatch > 0)
					{
						ServiceLogger.Information(FormattableString.Invariant($"{failedMessagesInCurrentBatch} message(s) failed to process in current batch."));
					}
				}
				else
				{
					ServiceLogger.Information(FormattableString.Invariant($"No DashDocumentDataMessage records have been loaded to process."));
					loadNextBatch = false;
				}
			}

			ServiceLogger.Information(FormattableString.Invariant($"Total {totalProcessedMessages} message(s) successfully processed."));

			if (totalFailedMessages > 0)
			{
				ServiceLogger.Information(FormattableString.Invariant($"Total {totalFailedMessages} message(s) failed to process."));
			}
		}

		static void CreateEdiMessage(IFactory factory, DashDocumentDataMessage dashDocumentDataMessage, DashDocumentDataMessageData dashDocumentDataMessageData, string dataProcessingStep)
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			if (dashDocumentDataMessage == null)
			{
				throw new ArgumentNullException(nameof(dashDocumentDataMessage));
			}

			var message = (DashDocumentDataMessage)factory.New(typeof(DashDocumentDataMessage));
			message.MessageData = dashDocumentDataMessageData;

			// Transfer properties
			message.EM_GB = dashDocumentDataMessage.EM_GB;
			message.EM_GE = dashDocumentDataMessage.EM_GE;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.DashDocumentDataProcessing;
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Internal;
			message.EM_MessageType = dataProcessingStep;
			message.EM_MessageSubType = dashDocumentDataMessage.EM_MessageSubType;
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_LinkUniqueID = dashDocumentDataMessage.EM_LinkUniqueID;
			message.EM_LinkTable = dashDocumentDataMessage.EM_LinkTable;
			message.EM_IsActive = true;
		}

		protected void FinalizeMessageProcessing(IFactory factory, DashDocumentDataMessage dashDocumentDataMessage)
		{
			if (NextDashDocumentMessageCanBeScheduled(dashDocumentDataMessage))
			{
				var currentMessageDocumentDataContext = dashDocumentDataMessage.MessageData;

				if (currentMessageDocumentDataContext != null && currentMessageDocumentDataContext.DataProcessingSteps != null)
				{
					var dataProcessingStep = DataProcessingStepScheduler.GetNextProcessingStep(currentMessageDocumentDataContext.DataProcessingSteps, currentMessageDocumentDataContext.CurrentDataProcessingStep);

					if (!string.IsNullOrWhiteSpace(dataProcessingStep))
					{
						var newMessageDocumentDataContext = new DashDocumentDataMessageData
						{
							DataProcessingSteps = currentMessageDocumentDataContext.DataProcessingSteps,
							CurrentDataProcessingStep = currentMessageDocumentDataContext.CurrentDataProcessingStep + 1,
						};

						CreateEdiMessage(factory, dashDocumentDataMessage, newMessageDocumentDataContext, dataProcessingStep);
					}
				}
			}
		}

		protected bool NextDashDocumentMessageCanBeScheduled(DashDocumentDataMessage dashDocumentDataMessage)
		{
			return dashDocumentDataMessage.EM_Status == EDIMessageStatusList.Codes.ProcessedOK || (dashDocumentDataMessage.EM_Status == EDIMessageStatusList.Codes.Failed && ContinueDocumentPostProcessingAfterMessageFailure);
		}

		protected virtual bool PreConditionsMet()
		{
			if (!EDocsParsingHelper.IsDocumentParsingEnabled())
			{
				ServiceLogger.Error((NoResString)"Failed to run service task. Please check if registry for at least one of the parsing types is enabled at System -> DocManager -> Document Ingestion -> Parse Types.");
				return false;
			}

			return true;
		}

		ZQuery QueryForLoadingMessages
		{
			get
			{
				var query = new ZDBOnlyQuery(typeof(DashDocumentDataMessage)) { MaximumRows = BatchSize }
					.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.DashDocumentDataProcessing)
					.AddToFilter(EDIMessageSchema.EM_IsActive, true)
					.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Internal)
					.AddToFilter(EDIMessageSchema.EM_Status, EDIMessageStatusList.Codes.Queued);

				query.TableIndexHints.Add(new TableIndexHint(EDIMessageSchema.Constants.Indexes.NR_RX__EM_ApplicationCode_EM_ReceiveTransmit_EM_MessageNum_EM_SystemCreateTimeUtc));

				AddServiceTaskSpecificQueryFilters(query);

				if (ExcludedPKs.Count > 0)
				{
					_ = query.AddToFilter(EDIMessageSchema.PK, SQLComparisonOperator.NotEqual, ExcludedPKs);
				}

				return query;
			}
		}

		string GetDocumentChanges(Guid docId, string token)
		{
			var changes = ShipamaxService.CheckEDocsChanges(docId, token);

			if (changes.Any())
			{
				var sb = new StringBuilder();

				foreach(var change in changes)
				{
					var messageLine = $"Change Type: {change.Field}. Old value: {change.OldValue}, New Value: {change.NewValue}.";
					sb.AppendLine(messageLine);
				}

				return sb.ToString().TrimEnd('\r', '\n');
			}

			return string.Empty;
		}

		protected abstract void AddServiceTaskSpecificQueryFilters(ZQuery query);

		protected virtual void ProcessMessage(IFactory factory, DashDocumentDataMessage dashDocumentDataMessage, CancellationToken token)
		{
			var dashDocument = factory.Load<DashDocument>(dashDocumentDataMessage.EM_LinkUniqueID);

			if (dashDocument == null)
			{
				var message = $"DashDocument record is not found. DashDocument ID stored in DashDocumentDataMessage: {dashDocumentDataMessage.EM_LinkUniqueID}.";
				ServiceLogger.Log(LogType.Error, message);
				return;
			}

			if (!EDocsParsingHelper.IsParseTypeEnabled(dashDocument.DDD_ParseType))
			{
				var message = $"DashDocument's parse type is not enabled for parsing. Parse Type: {dashDocument.DDD_ParseType}. DashDocument ID: {dashDocument.PK}.";
				ServiceLogger.Log(LogType.Warning, message);
				return;
			}

			if (dashDocument.DDD_IsObsolete)
			{
				var message = $"DashDocument record is obsolete. DashDocument ID: {dashDocument.PK}.";
				ServiceLogger.Log(LogType.Warning, message);
				return;
			}

			if (dashDocument.DDD_DocID.IsEmpty)
			{
				var message = $"StorageDoc ID must not be empty. DashDocument ID: {dashDocument.PK}.";
				ServiceLogger.Log(LogType.Warning, message);
				return;
			}

			var documentChanges = GetDocumentChanges(dashDocument.DDD_DocID.ToGuid(), dashDocument.DDD_DocToken);

			if (!documentChanges.IsNullOrEmpty())
			{
				var message = $"Document related to DashDocument with ID {dashDocument.PK} has changed. There are the following changes: {documentChanges}";
				ServiceLogger.Log(LogType.Information, message);
				return;
			}

			ProcessMessageCore(factory, dashDocument, dashDocumentDataMessage, token);
		}

		protected abstract void ProcessMessageCore(IFactory factory, DashDocument dashDocument, DashDocumentDataMessage dashDocumentDataMessage, CancellationToken token);

		protected virtual bool ContinueDocumentPostProcessingAfterMessageFailure => true;

		void LogErrorAndMarkMessagePKExcluded(string failureReason, ZGuid dashDocumentDataMessagePK, bool willBeReprocessed)
		{
			var errorMessage = willBeReprocessed
				? FormattableString.Invariant($"DashDocumentDataMessage record '{dashDocumentDataMessagePK}' will be re-processed in the next service task run. Failure reason: {failureReason}")
				: FormattableString.Invariant($"DashDocumentDataMessage record '{dashDocumentDataMessagePK}' failed, reaching the maximum number of retry attempts. Failure reason: {failureReason}");
			ExcludedPKs.Add(dashDocumentDataMessagePK);
			ServiceLogger.Error(errorMessage);
		}
	}
}
