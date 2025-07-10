using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.EInvoicing
{
	public class KoreaSouthComplianceInfoEInvoicingExtension : KoreaSouthComplianceInfo,
		IEInvoicingTransactionValidation,
		IComplianceInfoEInvoicingGUIActionProvider,
		IEInvoicingTransactionUpdater,
		IComplianceInfoEInvoicingGUIActionStatusRequest
	{
		#region IComplianceInfoEInvoicingGUIActionStatusRequest

		public ZString StatusRequestMenuName => Res.GetString("13810F90-F607-40CB-8049-A61AA4CC31B4", "Request e-Invoice Status");

		public ZString StatusRequestActionInformation
		{
			get
			{
				return Res.GetString("5D95CA5C-5D69-46C6-8212-36097015AB96", @"Your request is being processed.

Please note:
1. Only transactions that have an E-Reporting Status of 'FAL - Failed' are eligible for 'Request e-Invoice Status'.
2. This action will update the E-Reporting Status of transactions in the same batch as the currently selected transaction(s) to 'DLV - Delivered' and submit a request to NTS to check on the invoice submission status.");
			}
		}

		#endregion

		#region IComplianceInfoEInvoicingGUIActionProvider

		public bool IsCountryEnableComplianceEInvoicing(bool isAPTransaction = false) => !isAPTransaction && AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value;

		public IEnumerable<AccTransactionHeader> GetEligibleInvoices(IEnumerable<AccTransactionHeader> selectedTransactions)
		{
			return selectedTransactions?.OfType<TransactionHeader>().GroupBy(x => x.EInvoicingBatchNumber).Select(x => x.First());
		}

		public bool ExistActiveDocumentRequestPivot(ZDateTime lastSentTime)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region IEInvoicingTransactionUpdater

		public void UpdateTransaction(BusinessObjectFactory factory, TransactionHeader transaction, string actionType)
		{
			switch (actionType)
			{
				case EInvoicingPivotActionType.StatusCheck:
					UpdateTransactionForStatusCheck(factory, transaction); break;
			}
		}

		void UpdateTransactionForStatusCheck(BusinessObjectFactory factory, TransactionHeader transaction)
		{
			var transactionInNewFactory = factory.Load<TransactionHeader>(transaction.PK);
			var pivot = transactionInNewFactory.GetMostRecentEInvoicingTransactionPivot();

			pivot?.Batch.ClearQueryTimes();

			foreach (AccEInvoicingTransactionPivot pivotInBatch in pivot?.Batch?.TransactionPivots)
			{
				pivotInBatch.AIP_Status = EInvoicingPivotState.Delivered;
				pivotInBatch.AIP_ErrorDescription = string.Empty;
			}
		}

		#endregion

		public ZString GetCantAmendErrorMessage(InvoicingBase originalTransaction, string transactionType)
		{
			if (!AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value)
			{
				return ZString.Empty;
			}

			var isAmendStandAloneARCRD = originalTransaction.IsARCreditNote && originalTransaction.OriginalTransactionReference.IsEmpty;
			if (isAmendStandAloneARCRD)
			{
				return Res.GetString("56804F7A-3E2C-474B-9846-DBC44D6742B4", "Amendments to Stand-alone AR Credit Notes are not supported in Korea, please reverse the transaction in Receivables Transactions.");
			}

			return ZString.Empty;
		}

		public ZString GetCantReverseErrorMessage(IReversing originalTransaction)
		{
			if (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value
				&& originalTransaction is InvoicingBase invoicingBase && invoicingBase.IsARInvoiceOrCreditNote
				&& originalTransaction is IAmending amending && amending.OriginalTransaction != null)
			{
				return Res.GetString("763FAD78-B7B9-41B2-8D5C-FE3CE7D0EFDB", "Reversing of Amendment Transactions are not allowed in Korea when e-Reporting is enabled. Please Amend the Original Transaction in Job Billing module.");
			}

			return ZString.Empty;
		}

		public ZString GetValidationMessageForAfterPostAction(IEnumerable<AccEInvoicingTransactionPivot> pivots, string actionType)
		{
			var pivot = pivots.FirstOrDefault(x => x.AIP_ActionType == EInvoicingPivotActionType.Submit);

			if (pivot.AIP_Status != EInvoicingPivotState.Failed)
			{
				return Res.GetString("CDF62489-87B8-4827-8ED0-DCEB066D7D63", "The transaction is not eligible for requests because the E-Reporting Status of the transaction is not 'FAL - Failed'.");
			}

			return null;
		}
	}
}
