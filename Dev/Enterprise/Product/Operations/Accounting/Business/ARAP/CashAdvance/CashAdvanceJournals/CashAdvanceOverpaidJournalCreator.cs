using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.ARAP.CashAdvance
{
	internal class CashAdvanceOverpaidJournalCreator : CashAdvanceJournalCreator
	{
		internal CashAdvanceOverpaidJournalCreator(Invoice relatedInvoice, List<ICashAdvanceRequirement> carRequirementLines)
		{
			Argument.NotNull(relatedInvoice, nameof(relatedInvoice));
			RelatedInvoice = relatedInvoice;
			CarRequirementLines = carRequirementLines;
		}
		Invoice RelatedInvoice { get; }
		List<ICashAdvanceRequirement> CarRequirementLines { get; }

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

		protected override ZDateTime GetInvoiceDate(CashAdvanceRequestHeader requestHeader) => RelatedInvoice.InvoiceDate;

		protected override ZString GetJournalCategory(ZString ledger) => TransactionCategory.Codes.CashAdvanceInvoice;

		protected override ZString GetJournalDescription(CashAdvanceRequestHeader requestHeader)
		{
			return Res.GetString("80f76692-10fc-4ca3-bcaf-ce59354226d6", "OVERPAYMENT OF ADVANCE PAYMENT {0} / JOB {1}", requestHeader.CAH_RequestReferenceNumber, requestHeader.Job?.JH_JobNum);
		}

		protected override Guid GetJournalGLAccount(ZString ledger)
		{
			switch (RelatedInvoice.AH_Ledger)
			{
				case LedgerTypes.AccountsReceivable:
					return (Guid)AccountingConfigurationRegistry.Instance.CashAdvanceClearingAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
				case LedgerTypes.AccountsPayable:
					return (Guid)AccountingConfigurationRegistry.Instance.PayablesCashAdvanceClearingAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
				default:
					return Guid.Empty;
			}
		}

		protected override ZDateTime GetPostDate(CashAdvanceRequestHeader requestHeader) => RelatedInvoice.PostDate;

		protected override ZDecimal GetLocalAmount(CashAdvanceRequestHeader requestHeader)
		{
			var invoiceLines = GetInvoiceLinesPaidByThisRequest(requestHeader);
			var totalInvoiceLinesOSAmount = invoiceLines.Sum(l => l.AL_OSAmount);
			var totalInvoiceLinesLocalAmount = Env.CurrentCompany.ExchangeRate.ForeignToLocal(totalInvoiceLinesOSAmount, requestHeader.CAH_ExchangeRate);

			var invoicedCAHLines = CarRequirementLines.Where(l => l.CashAdvanceRequest.PK == requestHeader.PK && l.IsInvoiced);
			var totalCAHLinesLocalAmount = invoicedCAHLines.Sum(l => l.LocalPaidAmount);
			return Math.Abs(totalCAHLinesLocalAmount) - Math.Abs(totalInvoiceLinesLocalAmount);
		}

		protected override ZDecimal GetOSAmount(CashAdvanceRequestHeader requestHeader)
		{
			var invoiceLines = GetInvoiceLinesPaidByThisRequest(requestHeader);
			var totalInvoiceLinesOSAmount = invoiceLines.Sum(l => l.AL_OSAmount);

			var invoicedCAHLines = CarRequirementLines.Where(l => l.CashAdvanceRequest.PK == requestHeader.PK && l.IsInvoiced);
			var totalCAHLinesOSAmount = invoicedCAHLines.Sum(l => l.OSPaidAmount);
			return Math.Abs(totalCAHLinesOSAmount) - Math.Abs(totalInvoiceLinesOSAmount);
		}

		List<InvoicingLineBase> GetInvoiceLinesPaidByThisRequest(CashAdvanceRequestHeader requestHeader)
		{
			var invoiceLinesPaidByThisRequest = new List<InvoicingLineBase>();
			var invoiceLines = RelatedInvoice.Lines.Cast<InvoicingLineBase>();
			var invoicedCAHLines = requestHeader.Lines.OfType<AccCashAdvanceRequestLine>().Where(l => l.IsInvoiced);
			foreach (var invoiceLine in invoiceLines)
			{
				if (invoicedCAHLines.Any(l => l.RelatedJobCharge.PK == (invoiceLine.RelatedJobCharge?.PK ?? ZGuid.Empty)))
				{
					invoiceLinesPaidByThisRequest.Add(invoiceLine);
				}
			}
			return invoiceLinesPaidByThisRequest;
		}

		protected override void OnNewJournalCreatedCore(CashAdvanceRequestHeader requestHeader, Journal.Journal journal)
		{
			base.OnNewJournalCreatedCore(requestHeader, journal);
			//setup the link between invoice and overpaid journal
			journal.AH_TransactionBelongsToGroup = RelatedInvoice.PK;
		}

		protected override bool IsGenerateRequiredCore(CashAdvanceRequestHeader requestHeader) => GetOSAmount(requestHeader) > 0;
	}
}
