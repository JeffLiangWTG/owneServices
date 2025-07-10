using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.EventMessageProcessor;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.ElectronicMessaging.Malaysia
{
	class EInvoicingEventMessageMYProcessor : GlobalEInvoicingEventMessageProcessor
	{
		public EInvoicingEventMessageMYProcessor(EventMessageProcessorData eventMessageProcessorData, ICountryEInvoicingObjectFactory countryEInvoicingObjectFactory)
			: base(eventMessageProcessorData, countryEInvoicingObjectFactory)
		{
		}

		protected override void ProcessIAKEventMessage()
		{
			base.ProcessIAKEventMessage();

			switch (MessageSubType)
			{
				case MalaysiaEInvoiceAPICommandList.Codes.SubmitTransaction:
					HandleSubmitTransaction();
					break;
				case MalaysiaEInvoiceAPICommandList.Codes.GetSubmission:
					HandleGetSubmission();
					break;
				case MalaysiaEInvoiceAPICommandList.Codes.GetDocumentDetail:
					HandleGetInvalidDocumentDetail();
					break;
				default:
					break;
			}
		}

		void HandleSubmitTransaction()
		{
			if (SubmitResult == MalaysiaProcessorConstants.SubmitResult.Accepted)
			{
				EventAndDatabaseTransaction.Transaction.CreateNewEInvoicingPivot(invoiceBatch.Factory, Constants.EInvoicingPivotActionType.StatusCheck);
			}
		}

		void HandleGetSubmission()
		{
			if (SubmissionPivot.AIP_Status == Constants.EInvoicingPivotState.Succeed)
			{
				return;
			}

			var submissionStatus = universalEvent.ContextCollection.FirstOrDefault(context => context.Type == MalaysiaProcessorConstants.DataContext.MalaysiaSubmissionStatus)?.Value ?? string.Empty;
			if (submissionStatus == MalaysiaProcessorConstants.SubmissionStatus.Valid)
			{
				SubmissionPivot.AIP_Status = Constants.EInvoicingPivotState.Succeed;

				EventAndDatabaseTransaction.Transaction.CreateNewEInvoicingPivot(invoiceBatch.Factory, Constants.EInvoicingPivotActionType.DocumentAction);
				CreatePivotForAmendment(EventAndDatabaseTransaction.Transaction);
			}
			else if (submissionStatus == MalaysiaProcessorConstants.SubmissionStatus.Invalid)
			{
				DiscardPivotAndBatch(Constants.EInvoicingPivotActionType.DocumentDetail);
				EventAndDatabaseTransaction.Transaction.CreateNewEInvoicingPivot(invoiceBatch.Factory, Constants.EInvoicingPivotActionType.DocumentDetail);
			}
			else
			{
				SubmissionPivot.AIP_Status = Constants.EInvoicingPivotState.InProcessing;
			}

			SubmissionPivot.AIP_ErrorDescription = ZString.Empty;
		}

		void DiscardPivotAndBatch(string actionType)
		{
			var pivotShouldBeDiscarded = TransactionAllPivots.FirstOrDefault(x => x.AIP_ActionType == actionType);
			if (pivotShouldBeDiscarded != null)
			{
				pivotShouldBeDiscarded.AIP_Status = Constants.EInvoicingPivotState.Discarded;
				var batchOfPivot = pivotShouldBeDiscarded.Batch;
				if (batchOfPivot != null)
				{
					batchOfPivot.AIB_Status = Constants.EInvoicingBatchState.Discarded;
				}
			}
		}

		void CreatePivotForAmendment(AccTransactionHeader header)
		{
			var filter = new ZQuery();
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, header.PK);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_IsCancelled, false);
			var transactions = header.Factory.Load<TransactionHeader>(filter);

			foreach (var transaction in transactions)
			{
				ObjectFactory.Get<IEInvoicingTransactionProxyFactory>().GetProxy(transaction)?.EvaluateEligibilityAndQueue();
			}
		}

		void HandleGetInvalidDocumentDetail()
		{
			if (SubmissionPivot.AIP_Status == Constants.EInvoicingPivotState.Succeed)
			{
				return;
			}

			SubmissionPivot.AIP_Status = Constants.EInvoicingPivotState.Failed;
			SubmissionPivot.AIP_ErrorDescription = EventProcessorHelper.CheckLengthAndTruncIfNeeded(invoiceBatch.Company, universalEvent.EventParameters?.Reason ?? ZString.Empty);
		}

		protected override void ProcessIRJEventMessage()
		{
			base.ProcessIRJEventMessage();

			var errorMessage = EventProcessorHelper.CheckLengthAndTruncIfNeeded(invoiceBatch.Company, EventAndDatabaseTransaction.EventData.FailureReason);

			if (MessageSubType == MalaysiaEInvoiceAPICommandList.Codes.GetDocumentDetail)
			{
				SubmissionPivot.AIP_Status = Constants.EInvoicingPivotState.Failed;
				SubmissionPivot.AIP_ErrorDescription = ZString.Empty;
			}
			else if (MessageSubType == MalaysiaEInvoiceAPICommandList.Codes.GetSubmission)
			{
				SubmissionPivot.AIP_Status = Constants.EInvoicingPivotState.Failed;
				SubmissionPivot.AIP_ErrorDescription = errorMessage;
			}
		}

		ZString MessageSubType => messageSubType ?? (ZString)(messageSubType = universalEvent.EventParameters?.MessageSubType ?? string.Empty);
		ZString? messageSubType;

		ZString SubmitResult => submitResult ?? (ZString)(submitResult = universalEvent.ContextCollection.FirstOrDefault(context => context.Type == MalaysiaProcessorConstants.DataContext.MalaysiaSubmitResult)?.Value ?? string.Empty);
		ZString? submitResult;

		AccEInvoicingTransactionPivot SubmissionPivot
		{
			get
			{
				if (MessageSubType == MalaysiaEInvoiceAPICommandList.Codes.SubmitTransaction)
				{
					return invoiceBatch.TransactionPivots[0];
				}

				return TransactionAllPivots.FirstOrDefault(x => x.AIP_ActionType == Constants.EInvoicingPivotActionType.Submit);
			}
		}

		IEnumerable<AccEInvoicingTransactionPivot> TransactionAllPivots => transactionAllPivots ??= GetAllPivots();
		IEnumerable<AccEInvoicingTransactionPivot> transactionAllPivots;

		IEnumerable<AccEInvoicingTransactionPivot> GetAllPivots()
		{
			return Factory.Load<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, invoiceBatch.TransactionPivots[0].AIP_ParentID)
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ParentTableCode, AccTransactionHeaderSchema.Constants.Prefix)
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_Status, SQLComparisonOperator.NotEqual, Constants.EInvoicingPivotState.Discarded))
					.ToList();
		}

		BusinessObjectFactory Factory => invoiceBatch.Factory;

		protected override ZString PivotStatusForIAK
		{
			get
			{
				if (MessageSubType == MalaysiaEInvoiceAPICommandList.Codes.SubmitTransaction && SubmitResult == MalaysiaProcessorConstants.SubmitResult.Accepted)
				{
					return Constants.EInvoicingPivotState.Delivered;
				}
				else
				{
					return EventAndDatabaseTransaction.EventData.PivotStatus;
				}
			}
		}
	}
}
