using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	class MalaysiaEInvoicingEligibilityDecider : IEInvoicingEligibilityDecider
	{
		string IEInvoicingEligibilityDecider.GetAdditionalTraceLog(IEInvoicingEligibilityLiteTransaction transaction)
		{
			var originalInvoice = transaction.OriginalTransactionIfExists as AccTransactionHeader;

			return @$"Is Transaction Cancelled: {transaction.IsCancelled}
Compliance Sub Type ({transaction.ComplianceSubType})
Original Transaction Type: {originalInvoice?.AH_TransactionType}";
		}

		public bool IsTransactionEligible(IEInvoicingEligibilityLiteTransaction transaction)
		{
			return (transaction.Ledger == LedgerTypes.AccountsReceivable || transaction.Ledger == LedgerTypes.AccountsPayable)
					&& IsEligible(transaction)
					&& !ComplianceSubTypeIsEmpty(transaction);
		}

		bool IsEligible(IEInvoicingEligibilityLiteTransaction transaction)
		{
			if ((transaction.TransactionType == TransactionTypes.Invoice || transaction.TransactionType == TransactionTypes.CreditNote) && !transaction.IsCancelled)
			{
				if (transaction.OriginalTransactionIfExists == null)
				{
					return transaction.TransactionType == TransactionTypes.Invoice || (transaction.Ledger == LedgerTypes.AccountsPayable && transaction.TransactionType == TransactionTypes.CreditNote);
				}

				return HasOriginalReferenceIsIssuedSuccessfully(transaction);
			}

			return false;
		}

		bool HasOriginalReferenceIsIssuedSuccessfully(IEInvoicingEligibilityLiteTransaction transaction)
		{
			if (transaction.OriginalTransactionIfExists is AccTransactionHeader header)
			{
				return header.AH_TransactionType == TransactionTypes.Invoice
					&& header.IsSubmitSuccess();
			}

			return false;
		}

		bool ComplianceSubTypeIsEmpty(IEInvoicingEligibilityLiteTransaction transaction) => transaction.ComplianceSubType.IsEmpty;
	}
}
