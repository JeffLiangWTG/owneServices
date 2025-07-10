using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.ARAP.CashAdvance
{
	internal class CashAdvanceInvoicedJournalCreator : CashAdvanceJournalCreator
	{
		internal CashAdvanceInvoicedJournalCreator(Invoice relatedInvoice)
		{
			Argument.NotNull(relatedInvoice, nameof(relatedInvoice));
			RelatedInvoice = relatedInvoice;
		}
		Invoice RelatedInvoice { get; }

		protected override ZString GetJournalCategory(ZString ledger) => TransactionCategory.Codes.CashAdvanceInvoice;

		protected override Guid GetJournalGLAccount(ZString ledger)
		{
			switch (ledger)
			{
				case LedgerTypes.AccountsReceivable:
					return (Guid)AccountingConfigurationRegistry.Instance.CashAdvanceClearingAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
				case LedgerTypes.AccountsPayable:
					return (Guid)AccountingConfigurationRegistry.Instance.PayablesCashAdvanceClearingAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
				default:
					return ZGuid.Empty.ToGuid();
			}
		}

		protected override string GetDebitCreditSign(ZString ledger)
		{
			switch (ledger)
			{
				case LedgerTypes.AccountsReceivable:
					return Core.Constants.DebitCredit.Debit;
				case LedgerTypes.AccountsPayable:
					return Core.Constants.DebitCredit.Credit;
				default:
					return string.Empty;
			}
		}

		protected override ZString GetJournalDescription(CashAdvanceRequestHeader requestHeader)
		{
			switch (requestHeader.CAH_Ledger)
			{
				case LedgerTypes.AccountsReceivable:
					return Res.GetString("343b63cf-7906-45e7-8e76-08c4f797a6a9", "Invoice raised for charges prepaid on Advance Payment Request {0} by AR Receipt(s) {1}.", requestHeader.CAH_RequestReferenceNumber, CashAdvanceReceiptsOrPaymentsNumber);
				case LedgerTypes.AccountsPayable:
					return Res.GetString("ee8a4036-7314-47be-9a39-a7d578ae1e08", "Invoice received for charges prepaid on Advance Payment Request {0} by AP Payment {1}", requestHeader.CAH_RequestReferenceNumber, CashAdvanceReceiptsOrPaymentsNumber);
				default:
					return string.Empty;
			}
		}

		protected override ZDateTime GetPostDate(CashAdvanceRequestHeader requestHeader) => RelatedInvoice.AH_PostDate;

		protected override ZDateTime GetInvoiceDate(CashAdvanceRequestHeader requestHeader) => ZDateTime.Now;

		protected override ZDecimal GetLocalAmount(CashAdvanceRequestHeader requestHeader)
		{
			var invoicedLines = GetInvoicedCashAdvanceRequestLines(requestHeader);
			return invoicedLines.Sum(l => l.CAL_LocalPaidAmount);
		}

		protected override ZDecimal GetOSAmount(CashAdvanceRequestHeader requestHeader)
		{
			var invoicedLines = GetInvoicedCashAdvanceRequestLines(requestHeader);
			return invoicedLines.Sum(l => l.CAL_OSPaidAmount);
		}

		List<AccCashAdvanceRequestLine> GetInvoicedCashAdvanceRequestLines(CashAdvanceRequestHeader requestHeader)
		{
			var invoiceLines = RelatedInvoice.Lines.Cast<InvoicingLineBase>();
			var invoicedCAHLines = requestHeader.Lines.OfType<AccCashAdvanceRequestLine>().Where(l => l.IsInvoiced);
			var cahLinesInvoicedByThisInvoice = new List<AccCashAdvanceRequestLine>();
			foreach (AccCashAdvanceRequestLine invoicedCAHLine in invoicedCAHLines)
			{
				if (invoiceLines.Any(l => (l.RelatedJobCharge?.PK ?? ZGuid.Empty) == invoicedCAHLine.RelatedJobCharge.PK))
				{
					cahLinesInvoicedByThisInvoice.Add(invoicedCAHLine);
				}
			}
			return cahLinesInvoicedByThisInvoice;
		}

		protected override void RunPreJournalCreationValidationCore(CashAdvanceRequestHeader requestHeader, ZStringBuilder errorMessageBuilder)
		{
			base.RunPreJournalCreationValidationCore(requestHeader, errorMessageBuilder);
			if (string.IsNullOrEmpty(CashAdvanceReceiptsOrPaymentsNumber))
			{
				errorMessageBuilder.Append(Res.GetString("987675a4-0129-4e55-8dc2-38e044b11df1", "There is no matching {0} {1} journal for Advance Payment request {2}"
								, requestHeader.CAH_Ledger
								, CashAdvanceReceiptOrPaymentJournalCreator.GetMatchingJournalTransactionCategory(requestHeader.CAH_Ledger)
								, requestHeader.CAH_RequestReferenceNumber));
			}
		}

		protected override void OnJournalCreationBegin(CashAdvanceRequestHeader requestHeader)
		{
			base.OnJournalCreationBegin(requestHeader);

			var loader = new CashAdvanceReceiptOrPaymentLoader(requestHeader);
			var receiptInfo = loader.Get();
			if (receiptInfo?.Any() ?? false)
			{
				var receiptsAsCSV = string.Join(",", receiptInfo.ToArray());
				CashAdvanceReceiptsOrPaymentsNumber = receiptsAsCSV;
			}
		}

		protected override void OnJournalCreationEnd(CashAdvanceRequestHeader requestHeader)
		{
			CashAdvanceReceiptsOrPaymentsNumber = string.Empty;
		}

		string CashAdvanceReceiptsOrPaymentsNumber { get; set; }
	}
}
