using System.Collections.Generic;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.CashAdvance
{
	public class CashAdvanceMatchingTransactionDetail
	{
		internal CashAdvanceMatchingTransactionDetail(CashAdvanceRequestHeader cah, Journal.Journal cashAdvanceInvoicedJournal)
		{
			CashAdvanceInvoicedJournal = cashAdvanceInvoicedJournal;
			RequestHeader = cah;
			InvoiceLineToRequestLineMapping = new Dictionary<InvoicingLineBase, (BaseCharge Charge, AccCashAdvanceRequestLine CashAdvanceRequestLine)>();
		}

		internal Journal.Journal CashAdvanceInvoicedJournal { get; }

		internal CashAdvanceRequestHeader RequestHeader { get; }

		internal Dictionary<InvoicingLineBase, (BaseCharge Charge, AccCashAdvanceRequestLine CashAdvanceRequestLine)> InvoiceLineToRequestLineMapping { get; }
	}
}
