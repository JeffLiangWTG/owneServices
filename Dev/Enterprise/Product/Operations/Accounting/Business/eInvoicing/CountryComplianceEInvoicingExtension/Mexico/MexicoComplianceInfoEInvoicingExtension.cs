using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.EInvoicing
{
	public class MexicoComplianceInfoEInvoicingExtension : MexicoComplianceInfo,
		IEInvoicingTransactionValidation,
		IComplianceInfoEInvoicingGUIActionProvider,
		IComplianceInfoEInvoicingGUIActionDocumentRequest
	{
		#region IEInvoicingTransactionValidation

		ZString IEInvoicingTransactionValidation.GetCantAmendErrorMessage(InvoicingBase originalTransaction, string transactionType) => ZString.Empty;

		ZString IEInvoicingTransactionValidation.GetCantReverseErrorMessage(IReversing originalTransaction) => ZString.Empty;

		ZString IEInvoicingTransactionValidation.GetValidationMessageForAfterPostAction(IEnumerable<AccEInvoicingTransactionPivot> pivots, string actionType)
			=> !pivots.Any(x => x.AIP_Status == EInvoicingPivotState.Succeed) ? NotEligibleForRequestsMessage : ZString.Empty;

		#endregion

		#region IComplianceInfoEInvoicingGUIActionDocumentRequest

		ZString IComplianceInfoEInvoicingGUIActionDocumentRequest.DocumentRequestMenuName => Res.GetString("bcc9f069-c64b-41f4-90cc-8cc453d2a7e2", "Request e-Invoice PDF Copy");
		ZString IComplianceInfoEInvoicingGUIActionDocumentRequest.DocumentRequestActionInformation => RequestPDFCopyProcessingMessage;

		#endregion

		#region IComplianceInfoEInvoicingGUIActionProvider

		bool IComplianceInfoEInvoicingGUIActionProvider.IsCountryEnableComplianceEInvoicing(bool isAPTransaction) => !isAPTransaction && AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value;

		IEnumerable<AccTransactionHeader> IComplianceInfoEInvoicingGUIActionProvider.GetEligibleInvoices(IEnumerable<AccTransactionHeader> selectedTransactions)
			=> selectedTransactions
				.Where(x => x.AH_Ledger == LedgerTypes.AccountsReceivable
				&& (x.AH_TransactionType == TransactionTypes.Invoice || x.AH_TransactionType == TransactionTypes.CreditNote)
				&& GetEInvoicingEligibleComplianceSubTypeList().Contains(x.AH_ComplianceSubType)).ToList();

		bool IComplianceInfoEInvoicingGUIActionProvider.ExistActiveDocumentRequestPivot(ZDateTime lastSentTime)
			=> false;

		#endregion

		ZString NotEligibleForRequestsMessage => Res.GetString("54da68ed-acf8-4cf2-8645-e7fea9bf98c9", "The transaction is not eligible for requests as the E-Reporting Status is not equal to SUC.");
		ZString RequestPDFCopyProcessingMessage => Res.GetString("b84d4b27-63fa-46c1-b83c-cc6f21c99ba4", @"Your request for a PDF copy of the tax invoice is being processed. 
Please Note:
- Request for a copy of the tax invoice will be made only for electronic invoices that have E-Reporting Status equal to SUC");
	}
}
