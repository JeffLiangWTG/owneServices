using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.ElectronicMessaging.Turkey
{
	public class EInvoicingEventMessageTRProcessor : EInvoicingEventMessageProcessor
	{
		internal new static class ContextTypeCode
		{
			public const string GovernmentAllocatedNumber = nameof(GovernmentAllocatedNumber);
			public const string ResponseMessage = nameof(ResponseMessage);
			public const string StatusCode = nameof(StatusCode);
			public const string IsCancelled = nameof(IsCancelled);
			public const string Message = nameof(Message);
			public const string IsApplicableForRetry = nameof(IsApplicableForRetry);
		}

		internal static class ApprovalConstants
		{
			public const string ApprovalAccepted = nameof(ApprovalAccepted);
			public const string ApprovalDeclinedAsAlreadyApproved = nameof(ApprovalDeclinedAsAlreadyApproved);
			public const string ApprovalDeclinedAsAlreadyRejected = nameof(ApprovalDeclinedAsAlreadyRejected);

			public const string RejectionAccepted = nameof(RejectionAccepted);
			public const string RejectionDeclinedAsAlreadyApproved = nameof(RejectionDeclinedAsAlreadyApproved);
			public const string RejectionDeclinedAsAlreadyRejected = nameof(RejectionDeclinedAsAlreadyRejected);
		}

		public static class MessageSubTypeCode
		{
			public const string MessageFailed = "messageFailed"; // message sub type does not need to be localised
			public const string MessageSucceed = "messageSucceed"; // message sub type does not need to be localised
			public const string SendInvoiceResponse = "SendInvoiceResponse"; // message sub type does not need to be localised
		}

		public EInvoicingEventMessageTRProcessor(IXmlSessionTracker logger, IEDIMessage message, UniversalEvent universalEvent, AccEInvoicingBatch invoiceBatch)
			: base(logger, message, universalEvent, invoiceBatch)
		{
			if (invoiceBatch.AIB_Status != EInvoicingBatchState.Discarded)
			{
				TransactionPivot = invoiceBatch.TransactionPivots.Cast<AccEInvoicingTransactionPivot>().FirstOrDefault();
				Transaction = invoiceBatch.Factory.Load<InvoicingBase>(TransactionPivot?.AIP_ParentID ?? ZGuid.Empty);
				if (Transaction != null)
				{
					TransactionPKs = new List<ZGuid>() { Transaction.PK };
				}
			}
		}

		InvoicingBase Transaction { get; }
		List<ZGuid> TransactionPKs { get; }
		AccEInvoicingTransactionPivot TransactionPivot { get; }

		public override void Process()
		{
			if (invoiceBatch.AIB_Status == EInvoicingBatchState.Discarded)
			{
				return;
			}

			if (TransactionPivot == null)
			{
				if (CheckIsPILBatchAndDiscardIfNecessary())
				{
					return;
				}

				ReportAndLogError("PivotNotFound", Res.GetString("EInvoicingEventMessageTRProcessor|PivotNotFound", "No update performed due to transaction pivot not found for invoice batch {0} in {1}.", invoiceBatch.AIB_BatchNumber, companyName));
				return;
			}

			if (Transaction == null)
			{
				ReportAndLogError("TransactionNotFound", Res.GetString("MessageProcessorTRTransactionNotFound", "Response Message was not attached to eDocs due to related transaction was not found for pivot in invoice batch {0} in {1}.", invoiceBatch.AIB_BatchNumber, companyName));
				return;
			}

			if (TransactionPivot.AIP_Status == EInvoicingPivotState.Succeed)
			{
				logger.LogBoth(LogType.Warning, Res.GetString("EInvoicingEventMessageTRProcessor|SucceedPivot", "No update performed due to transaction pivot having 'SUC' status for invoice batch {0} in {1}.", invoiceBatch.AIB_BatchNumber, companyName));
				return;
			}

			if (universalEvent.EventType.Value == AutoEvents.InterchangeAcknowledgedCode)
			{
				ProcessIAKEventMessage(TransactionPivot.AIP_ActionType);
			}
			else
			{
				ProcessIRJEventMessage();
			}

			UpdateSubmitPivotMessage();
		}

		#region PIL Event Message

		bool CheckIsPILBatchAndDiscardIfNecessary()
		{
			if (universalEvent?.EventParameters?.MessageSubType.Value.ToString() == TurkeyEInvoiceAPICommandList.Codes.GetInboxInvoiceList)
			{
				invoiceBatch.UpdateBatchAndPivotStatusAndErrorDescription(null, string.Empty, string.Empty, EInvoicingBatchState.Discarded);
				invoiceBatch.Factory.Save();
				return true;
			}
			return false;
		}

		#endregion

		#region IRJ Event Message

		void ProcessIRJEventMessage()
		{
			var retryIdentifier = Res.GetString("0BD9142D-BF65-4AF9-9920-10E8ED75A0DE", "Unexpected Error - Attempting Retry #");
			ZInt currentRetryCount = ZInt.Zero;

			if (IsRetryApplicable(retryIdentifier, out currentRetryCount))
			{
				currentRetryCount++;
				invoiceBatch.UpdateBatchAndPivotStatusAndErrorDescription(TransactionPKs, TransactionPivot.AIP_ErrorDescription, EInvoicingPivotState.Discarded, EInvoicingBatchState.Discarded, lastResponseReceivedUtc: LastResponseReceivedTime);
				var newPivotForRetry = Transaction.CreateNewEInvoicingPivot(Transaction.Factory, TransactionPivot.AIP_ActionType);
				newPivotForRetry.AIP_ErrorDescription = retryIdentifier + currentRetryCount.ToString();
				return;
			}

			invoiceBatch.UpdateBatchAndPivotStatusAndErrorDescription(TransactionPKs, universalEvent.EventParameters?.Reason ?? ZString.Empty, EInvoicingPivotState.Failed, lastResponseReceivedUtc: LastResponseReceivedTime);
			SendErrorNotificationEmail(Res.GetString("EInvoicingEventMessageTRProcessor|MessageFailed", "Message to {0} invoice authorization service was rejected for invoice batch {1} in {2} with the following reason: {3}", Core.Constants.CountryCodes.Turkey, invoiceBatch.AIB_BatchNumber, companyName, universalEvent.EventParameters?.Reason ?? ZString.Empty));

			if (TransactionPivot.AIP_ActionType != EInvoicingPivotActionType.Submit)
			{
				var statusCode = GetValueFromContextCollection(ContextTypeCode.StatusCode);
				if (statusCode == EInvoicingStatuses.Codes.Error)
				{
					var submitPivot = GetSubmitPivot(TransactionPivot.AIP_ParentID);
					if (submitPivot != null)
					{
						submitPivot.AIP_Status = EInvoicingPivotState.Failed;
					}
				}
			}
		}

		bool IsRetryApplicable(string retryIdentifier, out ZInt currentRetryCount)
		{
			var result = false;
			currentRetryCount = ZInt.Zero;

			if (GetValueFromContextCollection(ContextTypeCode.IsApplicableForRetry).IsTrue())
			{
				var retryUpperLimit = Registry.AccountingElectronicMessagingRegistry.Instance.EReportingAutomaticRetryLimit.GetValueWithoutFallback(TransactionPivot.Company.PK.ToGuid(), Guid.Empty, Guid.Empty);
				if (TransactionPivot.AIP_ErrorDescription.Contains(retryIdentifier)
					&& !ZInt.TryParse(TransactionPivot.AIP_ErrorDescription.Substring(TransactionPivot.AIP_ErrorDescription.IndexOf(retryIdentifier) + retryIdentifier.Length).Trim(), out currentRetryCount))
				{
					ReportAndLogError("InvalidRetryCounter", Res.GetString("EInvoicingEventMessageTRProcessor|InvalidRetryCounterValue", "Retry counter value is not a number. Value: {0}", TransactionPivot.AIP_ErrorDescription));
				}

				result = (retryUpperLimit > 0 && currentRetryCount < retryUpperLimit);
			}
			return result;
		}

		#endregion

		#region IAK Event Message

		void ProcessIAKEventMessage(ZString messageSubType)
		{
			switch (messageSubType)
			{
				case EInvoicingPivotActionType.Submit:
					ProcessSubmitMessage();
					break;

				case EInvoicingPivotActionType.DocumentAction:
					ProcessDocumentActionMessage();
					break;

				case EInvoicingPivotActionType.StatusCheck:
					ProcessStatusCheckMessage();
					break;

				case EInvoicingPivotActionType.Cancel:
					ProcessCancelEArchiveInvoiceMessage();
					break;

				case EInvoicingPivotActionType.ConfirmTransactionReceived:
					UpdateRequestStatusToSucceed();
					break;

				case EInvoicingPivotActionType.Approve:
					ProcessApproveMessage();
					break;

				case EInvoicingPivotActionType.Reject:
					ProcessRejectMessage();
					break;

				default:
					ReportAndLogError("UnknownMessageSubType", Res.GetString("EInvoicingEventMessageTRProcessor|UnknownMessageSubType",
						"No update performed due to invalid message sub type [{0}] found for invoice batch {1} in {2}.", messageSubType, invoiceBatch.AIB_BatchNumber, companyName));
					break;
			}
		}

		#endregion

		#region Status Check Response Processing

		void ProcessStatusCheckMessage()
		{
			if (universalEvent.ContextCollection == null)
			{
				ReportAndLogError("MissingContextCollection", Res.GetString("MessageProcessorTRMissingContextCollection",
					"eHub Allocated Number was not updated due to universal event message not having context collection for invoice batch {0} in {1}.", invoiceBatch.AIB_BatchNumber, companyName));
				return;
			}

			var incomingStatus = GetValueFromContextCollection(ContextTypeCode.StatusCode);

			var submitPivot = GetSubmitPivot(TransactionPivot.AIP_ParentID);
			if (submitPivot == null)
			{
				ReportAndLogError("MissingSubmitPivot", Res.GetString("MessageProcessorTRMissingSubmitPivot", "Submit Pivot not found for related invoice in invoice batch {0} in {1}.", invoiceBatch.AIB_BatchNumber, companyName));
				return;
			}

			submitPivot.AIP_ErrorDescription = new EInvoicingStatuses().GetDescriptionFromCode(incomingStatus);
			invoiceBatch.UpdatePivotsStatus(IsNotFinalStatus(incomingStatus) ? EInvoicingPivotState.Sent : EInvoicingPivotState.Succeed, TransactionPKs, null, LastResponseReceivedTime);

			var isFinalFailure = IsFinalFailure(incomingStatus);
			if (IsFinalSuccess(incomingStatus) || isFinalFailure)
			{
				submitPivot.AIP_Status = isFinalFailure ? EInvoicingPivotState.Failed : EInvoicingPivotState.Succeed;
			}
		}

		#endregion

		#region Submit Batch Response Processing

		void ProcessSubmitMessage()
		{
			if (universalEvent.ContextCollection == null)
			{
				var errorMessage = Res.GetString("EInvoicingEventMessageTRProcessor|MissingContextCollection",
					"E-Invoice response was not processed due to universal event message was not having context collection for invoice batch {0} in {1}.", invoiceBatch.AIB_BatchNumber, companyName);
				ReportAndLogError("MissingContextCollection", errorMessage);
				invoiceBatch.UpdateBatchAndPivotStatusAndErrorDescription(TransactionPKs, errorMessage, EInvoicingPivotState.Failed, lastResponseReceivedUtc: LastResponseReceivedTime);
				return;
			}

			TransactionPivot.AIP_LastResponseReceivedUtc = LastResponseReceivedTime;

			var errorMessages = GetValueFromContextCollection(ContextTypeCode.ResponseMessage);
			if (!string.IsNullOrEmpty(errorMessages))
			{
				invoiceBatch.UpdateBatchAndPivotStatusAndErrorDescription(TransactionPKs, errorMessages, EInvoicingPivotState.Failed, lastResponseReceivedUtc: LastResponseReceivedTime);
				SendErrorNotificationEmail(errorMessages);
				return;
			}

			var governmentAllocatedNumber = GetValueFromContextCollection(ContextTypeCode.GovernmentAllocatedNumber);
			if (string.IsNullOrEmpty(governmentAllocatedNumber))
			{
				var errorMessage = Res.GetString("EInvoicingEventMessageTRProcessor|GovernmentAllocatedNumberContextHasNoValue",
								"Government Allocated Number was not updated due to Government Allocated Number Context value was empty for invoice batch {0} in {1}.", invoiceBatch.AIB_BatchNumber, companyName);
				ReportAndLogError("GovernmentAllocatedNumberContextHasNoValue", errorMessage);
				invoiceBatch.UpdateBatchAndPivotStatusAndErrorDescription(TransactionPKs, errorMessage, EInvoicingPivotState.Failed, lastResponseReceivedUtc: LastResponseReceivedTime);
				return;
			}
			invoiceBatch.AIB_GovernmentAllocatedNumber = governmentAllocatedNumber;
			invoiceBatch.UpdateBatchAndPivotStatusAndErrorDescription(TransactionPKs, ZString.Empty, EInvoicingPivotState.Delivered, lastResponseReceivedUtc: LastResponseReceivedTime);

			Transaction.CreateNewEInvoicingPivot(Transaction.Factory, EInvoicingPivotActionType.DocumentAction);
			Transaction.CreateNewEInvoicingPivot(Transaction.Factory, EInvoicingPivotActionType.StatusCheck);
		}

		void ProcessApproveMessage()
		{
			var resultContext = GetValueFromContextCollection(ContextTypeCode.StatusCode);
			var transactionRelatedApprovalRequest = Transaction.TransactionRelatedApprovalRequest;
			switch (resultContext)
			{
				case ApprovalConstants.ApprovalDeclinedAsAlreadyApproved:
					transactionRelatedApprovalRequest.XP_ApprovalStatus = GenApprovalRequestApprovalStatus.Approved;
					logger.LogBoth(LogType.Debug, Res.GetString("EInvoicingEventMessageTRProcessor|InvoiceIsAlreadyApproved", "The invoice is already 'Approved' by the government. Invoice Number: {0}", Transaction.AH_TransactionNum));
					break;

				case ApprovalConstants.ApprovalDeclinedAsAlreadyRejected:
					Transaction.IsCancelled = true;
					transactionRelatedApprovalRequest.XP_ApprovalStatus = GenApprovalRequestApprovalStatus.Rejected;
					logger.LogBoth(LogType.Debug, Res.GetString("EInvoicingEventMessageTRProcessor|InvoiceIsAlreadyRejected", "The invoice is already 'Rejected'. You cannot approve it. Invoice Number: {0}", Transaction.AH_TransactionNum));
					break;

				case ApprovalConstants.ApprovalAccepted:
					transactionRelatedApprovalRequest.XP_ApprovalStatus = GenApprovalRequestApprovalStatus.Approved;
					logger.LogBoth(LogType.Information, Res.GetString("EInvoicingEventMessageTRProcessor|InvoiceIsApprovedSuccessfully", "The invoice is approved successfully. Invoice Number: {0}", Transaction.AH_TransactionNum));
					break;
			}

			UpdateRequestStatusToSucceed(GetValueFromContextCollection(ContextTypeCode.Message));
		}

		void ProcessRejectMessage()
		{
			var resultContext = GetValueFromContextCollection(ContextTypeCode.StatusCode);
			var transactionRelatedApprovalRequest = Transaction.TransactionRelatedApprovalRequest;
			switch (resultContext)
			{
				case ApprovalConstants.RejectionDeclinedAsAlreadyRejected:
					Transaction.IsCancelled = true;
					transactionRelatedApprovalRequest.XP_ApprovalStatus = GenApprovalRequestApprovalStatus.Rejected;
					logger.LogBoth(LogType.Debug, Res.GetString("EInvoicingEventMessageTRProcessor|InvoiceRejectionDeclinedAsAlreadyRejected", "The invoice is already 'Rejected'. Invoice Number: {0}", Transaction.AH_TransactionNum));
					break;

				case ApprovalConstants.RejectionDeclinedAsAlreadyApproved:
					transactionRelatedApprovalRequest.XP_ApprovalStatus = GenApprovalRequestApprovalStatus.Requested;
					var isValidationSuspended = Transaction.Factory.IsValidationSuspended;
					if (isValidationSuspended)
					{
						Transaction.Factory.ResumeValidation();
					}

					Transaction.RunPreSaveValidation();
					if (Transaction.HasErrors)
					{
						transactionRelatedApprovalRequest.XP_ApprovalStatus = GenApprovalRequestApprovalStatus.Error;
					}
					logger.LogBoth(LogType.Debug, Res.GetString("EInvoicingEventMessageTRProcessor|InvoiceRejectionDeclinedAsAlreadyApproved", "The invoice is already 'Approved' by the government. You cannot reject it. Invoice Number: {0}", Transaction.AH_TransactionNum));

					if (isValidationSuspended)
					{
						Transaction.Factory.SuspendValidation();
					}
					break;

				case ApprovalConstants.RejectionAccepted:
					Transaction.IsCancelled = true;
					transactionRelatedApprovalRequest.XP_ApprovalStatus = GenApprovalRequestApprovalStatus.Rejected;
					logger.LogBoth(LogType.Information, Res.GetString("EInvoicingEventMessageTRProcessor|InvoiceIsRejectedSuccessfully", "The invoice is rejected successfully. Invoice Number: {0}", Transaction.AH_TransactionNum));
					break;
			}

			UpdateRequestStatusToSucceed(GetValueFromContextCollection(ContextTypeCode.Message));
		}

		void UpdateRequestStatusToSucceed(string errorDescription = "")
		{
			invoiceBatch.UpdateBatchAndPivotStatusAndErrorDescription(TransactionPKs, errorDescription, EInvoicingPivotState.Succeed, lastResponseReceivedUtc: LastResponseReceivedTime);
		}

		#endregion

		#region Document Action Batch Response Processing

		void ProcessDocumentActionMessage()
		{
			CheckAttachedDocumentAndUpdateStatus();
		}

		#endregion

		#region Cancel EArchive Invoice Batch Response Processing

		void ProcessCancelEArchiveInvoiceMessage()
		{
			if (universalEvent.ContextCollection == null)
			{
				var errorMessage = Res.GetString("EInvoicingEventMessageTRProcessor|MissingContextCollectionOfCancellation",
					"E-Archive invoice cancel response was not processed due to universal event message was not having context collection for invoice batch {0} in {1}.", invoiceBatch.AIB_BatchNumber, companyName);
				ReportAndLogError("MissingContextCollectionOfCancellation", errorMessage);
				invoiceBatch.UpdateBatchAndPivotStatusAndErrorDescription(TransactionPKs, errorMessage, EInvoicingPivotState.Failed, lastResponseReceivedUtc: LastResponseReceivedTime);
				return;
			}

			var isCancelled = bool.Parse(GetValueFromContextCollection(ContextTypeCode.IsCancelled));
			var responseMessage = GetValueFromContextCollection(ContextTypeCode.Message);

			var originalTransaction = GetOriginalTransaction(TransactionPivot.AIP_ParentID);
			if (!isCancelled)
			{
				invoiceBatch.UpdateBatchAndPivotStatusAndErrorDescription(TransactionPKs, responseMessage, EInvoicingPivotState.Failed, lastResponseReceivedUtc: LastResponseReceivedTime);
				TransactionHeader.CreateEInvoicingPivotForRequest(originalTransaction.Factory, originalTransaction, EInvoicingPivotActionType.StatusCheck);
			}
			else
			{
				invoiceBatch.UpdateBatchAndPivotStatusAndErrorDescription(TransactionPKs, responseMessage, EInvoicingPivotState.Succeed, lastResponseReceivedUtc: LastResponseReceivedTime);
				var submitPivot = GetSubmitPivot(originalTransaction.PK);
				submitPivot.AIP_Status = EInvoicingPivotState.Failed;
			}

			AttachResponseMessageToeDocs(TransactionPivot.AIP_ParentID);
		}

		#endregion

		#region Helpers

		protected override bool ShouldSendEmail => !Transaction.IsNull;

		protected override string LogErrorNotFoundKey => "TransactionNotFound"; // Error Message Key for Developers Only

		protected override GEIEmailNotificationCreator GetEmailNotificationCreator(ZString errorMessage, ILogger logCollector)
		{
			return new GEIEmailNotificationCreator(ediMessage, Transaction, new List<ZString>() { errorMessage }, logCollector);
		}

		void CheckAttachedDocumentAndUpdateStatus()
		{
			var attachedDocument = universalEvent.AttachedDocumentCollection?.FirstOrDefault();
			if (attachedDocument == null || !universalEvent.AttachedDocumentCollection.Any())
			{
				var errorMessage = Res.GetString("EInvoicingEventMessageTRProcessor|MissingAttachedDocumentCollection", "Response Message does not have attached document. Response Message Context for invoice batch {0} in {1}.", invoiceBatch.AIB_BatchNumber, companyName);
				invoiceBatch.UpdateBatchAndPivotStatusAndErrorDescription(TransactionPKs, errorMessage, EInvoicingPivotState.Failed, lastResponseReceivedUtc: LastResponseReceivedTime);
				ReportAndLogError("AttachmentNotFound", errorMessage);
				return;
			}

			SetAttachedDocumentTypeForPDF(universalEvent.AttachedDocumentCollection);

			Transaction.DocManagerInfo.SetupEDocsFactoryToBeSavedWithMainFactory(false);
			invoiceBatch.UpdateBatchAndPivotStatusAndErrorDescription(TransactionPKs, ZString.Empty, EInvoicingPivotState.Succeed, lastResponseReceivedUtc: LastResponseReceivedTime);
		}

		const string PDF = nameof(PDF);

		void SetAttachedDocumentTypeForPDF(List<AttachedDocument> attachedDocuments)
		{
			var registry = AccountingConfigurationRegistry.Instance.ThirdPartyEInvoiceDocType;
			var registryValue = registry.GetFallBackValueAtAllLevels(Transaction.AH_GC.ToGuid(), Transaction.AH_GB.ToGuid(), Transaction.AH_GE.ToGuid());

			attachedDocuments.Cast<AttachedDocument>()
				.Where(y => Path.GetExtension(y.FileName).ToUpper().Contains(PDF))
				.ForEach(x =>
				{
					x.Type.Code = registryValue;
					x.Type.Description = (registry.Inner.DataType as CodePairRegistryDataType).LookUpList.GetDescriptionFromCode(registryValue);
				});
		}

		void UpdateSubmitPivotMessage()
		{
			var allPivots = GetActivePivotsByPriority(TransactionPivot.AIP_ParentID);
			var submitPivot = allPivots.First();

			if (submitPivot.AIP_ActionType != EInvoicingPivotActionType.Submit)
			{
				return;
			}

			var messageBuilder = new ZStringBuilder();
			foreach (var onePivot in allPivots)
			{
				var coreMessage = ExtractCoreMessage(onePivot);

				if (!string.IsNullOrWhiteSpace(coreMessage))
				{
					if (onePivot.AIP_ActionType != EInvoicingPivotActionType.Submit)
					{
						coreMessage = $"{onePivot.AIP_ActionType}:{coreMessage}"; // Simple concatenation format. Nothing to localize.
					}

					messageBuilder.Append(coreMessage);
				}
			}

			submitPivot.AIP_ErrorDescription = messageBuilder.ToStringWithDelimiterBetweenAppends("; ");
		}

		string ExtractCoreMessage(AccEInvoicingTransactionPivot pivot)
		{
			var errorDescription = pivot.AIP_ErrorDescription;

			if (pivot.AIP_ActionType == EInvoicingPivotActionType.Submit)
			{
				var startsPrefixed = EInvoicingPivotActionType.QueryActionTypes.Any(t => errorDescription.StartsWith($"{t}:", StringComparison.InvariantCultureIgnoreCase)); // Simple prefix format. Nothing to localize.
				if (startsPrefixed)
				{
					// SUB message does not have own message - only contains consolidated messages from other query pivots.
					errorDescription = string.Empty;
				}
				else
				{
					var parts = errorDescription.Split(new char[] { ';' }, 2);
					var statusDescription = parts.First();

					var eInvoicingStatuses = new EInvoicingStatuses();
					var eInvoicingStatusMessages = eInvoicingStatuses.GetAllCodes().Select(c => eInvoicingStatuses.GetDescriptionFromCode(c));

					if (eInvoicingStatusMessages.Contains((string)statusDescription))
					{
						errorDescription = statusDescription;
					}
				}
			}

			return errorDescription;
		}

		AccEInvoicingTransactionPivot GetSubmitPivot(ZGuid parentId)
		{
			var sqlFilter = new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, parentId);
			sqlFilter.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ActionType, EInvoicingPivotActionType.Submit);
			sqlFilter.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_Status, SQLComparisonOperator.NotEqual, EInvoicingPivotState.Discarded);
			var submitPivot = invoiceBatch.Factory.LoadTop1<AccEInvoicingTransactionPivot>(sqlFilter);

			return submitPivot;
		}

		AccEInvoicingTransactionPivot[] GetActivePivotsByPriority(ZGuid parentId)
		{
			var priorityOrder = new string[] {
				EInvoicingPivotActionType.Submit,
				EInvoicingPivotActionType.Cancel,
				EInvoicingPivotActionType.Adjustment,
				EInvoicingPivotActionType.Approve,
				EInvoicingPivotActionType.StatusCheck,
				EInvoicingPivotActionType.DocumentAction
			};

			var sqlFilter = new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, parentId);
			sqlFilter.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_Status, SQLComparisonOperator.NotEqual, EInvoicingPivotState.Discarded);
			var allPivots = invoiceBatch.Factory.Load<AccEInvoicingTransactionPivot>(sqlFilter);

			allPivots = allPivots.OrderBy(pivot => Array.IndexOf(priorityOrder, (string)pivot.AIP_ActionType)).ToArray();

			return allPivots;
		}

		TransactionHeader GetOriginalTransaction(ZGuid parentID)
		{
			var subQuery = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccTransactionHeaderSchema.AH_TransactionBelongsToGroup);
			subQuery.AddToFilter(AccTransactionHeaderSchema.PK, parentID);

			var query = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			query.AddSubQuery(subQuery, JoinCondition.And);

			return Transaction.Factory.LoadTop1<TransactionHeader>(query);
		}

		void AttachResponseMessageToeDocs(ZGuid originalTransactionPK)
		{
			var transaction = Transaction.Factory.Load<InvoicingBase>(originalTransactionPK);
			var responseMessage = base.ediMessage.EM_MessageText;

			if (string.IsNullOrEmpty(responseMessage))
			{
				logger.LogBoth(LogType.Warning, Res.GetString("EInvoicingEventMessageTRProcessor|CancelMessageEmpty", "Cancel Message was not attached to eDocs due to Response Message was empty for invoice batch {0} in {1}.", invoiceBatch.AIB_BatchNumber, companyName));
				return;
			}

			if (transaction == null)
			{
				ReportAndLogError("TransactionNotFound", Res.GetString("EInvoicingEventMessageTRProcessor|TransactionNotFound", "Cancel Message was not attached to eDocs due to related transaction was not found for pivot in invoice batch {0} in {1}.", invoiceBatch.AIB_BatchNumber, companyName));
				return;
			}

			var fileName = TurkeyEInvoiceAPICommandList.Codes.CancelReceivablesInvoice + "_IAK_" + ZDateTime.UtcNow.ToString("yyyyMMddhhmmss", CultureInfo.InvariantCulture) + ".xml";
			transaction.DocManagerInfo.AddFileOrDocument(MessageEncoding.UTF8WithoutBOM.GetBytes(responseMessage), fileName, ReferenceTypes.Accounting);
			transaction.DocManagerInfo.SetupEDocsFactoryToBeSavedWithMainFactory(false);
		}

		string GetValueFromContextCollection(string typeCode) => universalEvent.ContextCollection.FirstOrDefault(x => string.Equals(x.Type, typeCode, StringComparison.OrdinalIgnoreCase))?.Value ?? string.Empty;

		#region EInvoicingStatusCodes

		public class EInvoicingStatuses : CodeDescriptionPairList
		{
			public static class Codes
			{
				public const string Draft = "0";
				public const string Cancelled = "10";
				public const string Queued = "100";
				public const string Processing = "200";
				public const string SentToGib = "300";
				public const string Approved = "1000";
				public const string WaitingForApproval = "1100";
				public const string Declined = "1200";
				public const string Return = "1300";
				public const string EArchiveCancelled = "1400";
				public const string Error = "2000";
			}

			public static class Descriptions
			{
				public static MultilingualString Draft { get { return ResString.GetMultilingualString("556C0504-7C1E-43AA-BB04-8A78F16B90D5", "Draft"); } }
				public static MultilingualString Cancelled { get { return ResString.GetMultilingualString("859440D0-8203-4E54-9F29-963B04EA0452", "Canceled"); } }
				public static MultilingualString Queued { get { return ResString.GetMultilingualString("F2FFF22D-6011-477E-A9E0-5027BA587519", "Queued"); } }
				public static MultilingualString Processing { get { return ResString.GetMultilingualString("7EBBC355-122E-416C-AADC-4FEA85ACDF82", "Processing"); } }
				public static MultilingualString SentToGib { get { return ResString.GetMultilingualString("50247D7C-EA68-404B-9141-D6A9422EBF93", "Sent to Gib"); } }
				public static MultilingualString Approved { get { return ResString.GetMultilingualString("39B794E3-1595-412B-A657-0AC562E75D26", "Approved"); } }
				public static MultilingualString WaitingForApproval { get { return ResString.GetMultilingualString("376F5239-1E92-4663-8B6B-984656E5B168", "Waiting for approval"); } }
				public static MultilingualString Declined { get { return ResString.GetMultilingualString("FA52EE66-5A64-4979-99E0-3519F8A03DA9", "Declined"); } }
				public static MultilingualString Return { get { return ResString.GetMultilingualString("55927012-47F0-4E18-9AC2-610EE3A0E414", "Return"); } }
				public static MultilingualString EArchiveCancelled { get { return ResString.GetMultilingualString("E7599D39-DF1F-4704-B0CD-DE23F3263CE7", "E-Archive canceled"); } }
				public static MultilingualString Error { get { return ResString.GetMultilingualString("D3DD5F28-6DD9-46BB-80C9-5B5E9C33AC4B", "Error"); } }
			}

			public EInvoicingStatuses()
			{
				AddPair(Codes.Draft, Descriptions.Draft);
				AddPair(Codes.Cancelled, Descriptions.Cancelled);
				AddPair(Codes.Queued, Descriptions.Queued);
				AddPair(Codes.Processing, Descriptions.Processing);
				AddPair(Codes.SentToGib, Descriptions.SentToGib);
				AddPair(Codes.Approved, Descriptions.Approved);
				AddPair(Codes.WaitingForApproval, Descriptions.WaitingForApproval);
				AddPair(Codes.Declined, Descriptions.Declined);
				AddPair(Codes.Return, Descriptions.Return);
				AddPair(Codes.EArchiveCancelled, Descriptions.EArchiveCancelled);
				AddPair(Codes.Error, Descriptions.Error);
			}
		}

		readonly string[] FailureStatuses = { EInvoicingStatuses.Codes.Cancelled, EInvoicingStatuses.Codes.Declined, EInvoicingStatuses.Codes.Return, EInvoicingStatuses.Codes.EArchiveCancelled };
		bool IsFinalSuccess(string statusCode) => EInvoicingStatuses.Codes.Approved == statusCode;
		bool IsFinalFailure(string statusCode) => Array.IndexOf(FailureStatuses, statusCode) != -1;
		bool IsNotFinalStatus(string statusCode) => !IsFinalSuccess(statusCode) && !IsFinalFailure(statusCode);

		#endregion

		#endregion
	}
}
