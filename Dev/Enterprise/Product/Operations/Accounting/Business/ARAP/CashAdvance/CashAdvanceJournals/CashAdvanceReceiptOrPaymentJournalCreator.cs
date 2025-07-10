using System;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.ARAP.CashAdvance
{
	internal class CashAdvanceReceiptOrPaymentJournalCreator : CashAdvanceJournalCreator
	{
		internal CashAdvanceReceiptOrPaymentJournalCreator(ZDateTime matchDate)
		{
			MatchDate = matchDate;
		}
		ZDateTime MatchDate { get; }

		protected override string GetDebitCreditSign(ZString ledger)
		{
			switch (ledger)
			{
				case LedgerTypes.AccountsReceivable:
					return Core.Constants.DebitCredit.Credit;
				case LedgerTypes.AccountsPayable:
					return Core.Constants.DebitCredit.Debit;
				default:
					return string.Empty;
			}
		}

		protected override ZDateTime GetInvoiceDate(CashAdvanceRequestHeader requestHeader) => ZDateTime.Now;

		protected override ZString GetJournalCategory(ZString ledger) => GetMatchingJournalTransactionCategory(ledger);

		protected override ZString GetJournalDescription(CashAdvanceRequestHeader requestHeader) =>
			Res.GetString("4cd135cf-21a9-43e6-a166-3f445e7745a6", "Payment of Advance Payment Request [{0}]", requestHeader.CAH_RequestReferenceNumber);

		protected override Guid GetJournalGLAccount(ZString ledger)
		{
			switch (ledger)
			{
				case LedgerTypes.AccountsReceivable:
					return (Guid)AccountingConfigurationRegistry.Instance.CashAdvanceClearingAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
				case LedgerTypes.AccountsPayable:
					return (Guid)AccountingConfigurationRegistry.Instance.PayablesCashAdvanceClearingAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
				default:
					return Guid.Empty;
			}
		}

		protected override ZDateTime GetPostDate(CashAdvanceRequestHeader requestHeader) =>
			Journal.Journal.GetPostDate(MatchDate);

		protected override ZDecimal GetLocalAmount(CashAdvanceRequestHeader requestHeader) => requestHeader.CAH_LocalOutstandingAmount;

		protected override ZDecimal GetOSAmount(CashAdvanceRequestHeader requestHeader) => requestHeader.CAH_OSOutstandingAmount;

		protected override void OnNewJournalCreatedCore(CashAdvanceRequestHeader requestHeader, Journal.Journal journal)
		{
			base.OnNewJournalCreatedCore(requestHeader, journal);
			journal.AH_CAH_CashAdvanceRequestHeader = requestHeader.PK;
		}

		internal static ZString GetMatchingJournalTransactionCategory(ZString ledger)
		{
			switch (ledger)
			{
				case LedgerTypes.AccountsReceivable:
					return TransactionCategory.Codes.CashAdvanceReceived;
				case LedgerTypes.AccountsPayable:
					return TransactionCategory.Codes.CashAdvancePaid;
				default:
					return ZString.Empty;
			}
		}

		internal static ZString GetCashAdvanceReceiptOrPaymentTransactionType(ZString ledger)
		{
			switch (ledger)
			{
				case LedgerTypes.AccountsReceivable:
					return TransactionTypes.Receipt;
				case LedgerTypes.AccountsPayable:
					return TransactionTypes.Payment;
				default:
					return ZString.Empty;
			}
		}
	}
}
