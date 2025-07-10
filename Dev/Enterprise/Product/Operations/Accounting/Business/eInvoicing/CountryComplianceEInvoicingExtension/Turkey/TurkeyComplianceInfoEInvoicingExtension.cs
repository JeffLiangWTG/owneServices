using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.EInvoicing
{
	public class TurkeyComplianceInfoEInvoicingExtension : TurkeyComplianceInfo, IEInvoicingTransactionValidation, IMostRecentPivotProvider, IEReportingStatusMessageProvider
	{
		public ZString GetCantAmendErrorMessage(InvoicingBase originalTransaction, string transactionType)
			=> transactionType == TransactionTypes.CreditNote && originalTransaction != null
			&& originalTransaction.AH_Ledger == LedgerTypes.AccountsReceivable && originalTransaction.AH_TransactionType == TransactionTypes.Invoice
			&& IsEInvoicingFunctionalityEnabled(originalTransaction)
			&& !IsTransactionPivotNotExistingOrInExpectedStatus(originalTransaction, EInvoicingPivotState.Succeed)
			? ResString.GetMultilingualString("EB5B8196-2D05-4E0F-8027-C178F8415640", "You can amend the transaction with Credit Note only for AR invoices with Successful (SUC) E-Reporting Status.")
			: ZString.Empty;

		public ZString GetCantReverseErrorMessage(IReversing originalTransaction)
			=> originalTransaction is ARInvoice transaction && transaction != null
			&& transaction.AH_Ledger == LedgerTypes.AccountsReceivable && transaction.AH_TransactionType == TransactionTypes.Invoice
			&& IsEInvoicingFunctionalityEnabled(transaction)
			&& !IsTransactionPivotNotExistingOrInExpectedStatus(transaction, EInvoicingPivotState.Succeed, EInvoicingPivotState.Failed, EInvoicingPivotState.BatchedWithError)
			? ResString.GetMultilingualString("9467A566-BEB4-40B3-8D6B-81334DEC3BAB", "You can reverse only AR Invoices where the E-Reporting status is Succeeded (SUC), Failed (FAL), or Batched with Errors (BER).")
			: ZString.Empty;

		bool IsEInvoicingFunctionalityEnabled(InvoicingBase transaction) =>
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.GetFallBackValueAtAllLevels(transaction.AH_GC.ToGuid(), transaction.AH_GB.ToGuid(), transaction.AH_GE.ToGuid());

		bool IsTransactionPivotNotExistingOrInExpectedStatus(InvoicingBase transaction, params string[] statuses)
		{
			var pivot = transaction.GetMostRecentEInvoicingTransactionPivot();
			return pivot == null || statuses.ToList().Contains(pivot.AIP_Status);
		}

		public ZString GetValidationMessageForAfterPostAction(IEnumerable<AccEInvoicingTransactionPivot> pivots, string actionType)
			=> !pivots.Any(x => x.IsSubmitPivotSucceedOrDelivered && !(x.Batch?.AIB_GovernmentAllocatedNumber ?? ZString.Empty).IsEmpty)
			? NotEligibleForRequestsMessage : ZString.Empty;

		AccEInvoicingTransactionPivot IMostRecentPivotProvider.GetMostRecentPivot(ElectronicInvoicingTransactionProxy electronicInvoicingTransaction, AccTransactionHeader transaction)
		{
			var turkeyActionTypes = EInvoicingPivotActionType.CommandActionTypes.ToList();
			turkeyActionTypes.Add(EInvoicingPivotActionType.Approve);

			var query = new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ActionType, SQLComparisonOperator.Contains, turkeyActionTypes)
									.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ParentID, electronicInvoicingTransaction.PK)
									.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ParentTableCode, AccTransactionHeaderSchema.Constants.Prefix);
			var pivots = transaction.Factory.Load<AccEInvoicingTransactionPivot>(query);
			var sortedPivots = pivots.OrderByDescending(p => p.AIP_LastSentTimeUtc.IsEmpty ? p.AIP_SystemCreateTimeUtc : p.AIP_LastSentTimeUtc);
			var mostRecentPivotValue = sortedPivots.FirstOrDefault(x => x.AIP_Status != EInvoicingPivotState.Discarded && (x.AIP_ActionType == EInvoicingPivotActionType.Approve || x.AIP_ActionType == EInvoicingPivotActionType.Reject))
						?? sortedPivots.FirstOrDefault(x => x.AIP_Status != EInvoicingPivotState.Discarded)
						?? sortedPivots.FirstOrDefault();

			return mostRecentPivotValue;
		}

		string IEReportingStatusMessageProvider.GetStatusMessage(ZString complianceSubType, ZString approvalStatus, ZString pivotStatus, ZString pivotActionType, string currentMessage)
		{
			if (approvalStatus == GenApprovalRequestApprovalStatus.Rejected)
			{
				return BuildStatusMessage(ResString.GetMultilingualString("76a58ce5-587b-4807-90c2-77510c634aa3", "Invoice Has Been Rejected"), currentMessage);
			}

			if (approvalStatus == GenApprovalRequestApprovalStatus.Approved)
			{
				return BuildStatusMessage(ResString.GetMultilingualString("b26b5524-715f-4092-a8ee-df92372fa0e2", "Invoice Has Been Approved, Awaiting Allocation"), currentMessage);
			}

			if (pivotActionType == EInvoicingPivotActionType.ConfirmTransactionReceived)
			{
				if (complianceSubType == ComplianceSubTypeCodes.PIC)
				{
					return BuildStatusMessage(ResString.GetMultilingualString("98587c88-3100-475c-b23e-5bdd1f5b913e", "Awaiting Approval or Rejection Action"), currentMessage);
				}

				if (complianceSubType == ComplianceSubTypeCodes.PIN || complianceSubType == ComplianceSubTypeCodes.CIN)
				{
					return BuildStatusMessage(ResString.GetMultilingualString("9c2d7726-9c22-4a1b-b38b-c473a49ca3a1", "Successfully Received"), currentMessage);
				}
			}

			if (pivotStatus == EInvoicingPivotState.Queued || pivotStatus == EInvoicingPivotState.Batched || pivotStatus == EInvoicingPivotState.Sent)
			{
				if (pivotActionType == EInvoicingPivotActionType.Approve)
				{
					return BuildStatusMessage(ResString.GetMultilingualString("e4fdc138-58ff-4f17-b449-56ea5f556c4f", "Approval Request Has Been Sent"), currentMessage);
				}

				if (pivotActionType == EInvoicingPivotActionType.Reject)
				{
					return BuildStatusMessage(ResString.GetMultilingualString("445c7f86-4a0a-4998-9eca-577b4d298972", "Rejection Request Has Been Sent"), currentMessage);
				}
			}

			return currentMessage;
		}

		static string BuildStatusMessage(string customMessage, string currentMessage)
			=> customMessage + (!string.IsNullOrWhiteSpace(currentMessage) ? " | " + currentMessage : string.Empty);
	}
}
