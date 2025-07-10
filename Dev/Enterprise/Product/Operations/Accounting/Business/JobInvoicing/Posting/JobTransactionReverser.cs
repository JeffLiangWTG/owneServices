using System.Collections.Generic;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Reversing;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public partial class JobTransactionReverser
	{
		public JobTransactionReverser(IEnumerable<InvoicingBase> invoicesAndCreditNotesToReverse)
		{
			TransactionsToReverse = invoicesAndCreditNotesToReverse;
		}

		public IEnumerable<InvoicingBase> ReverseAllInvoices(string reversingReason, string reversingCode)
		{
			CFXJournalReverser cFXReverser = new CFXJournalReverser();

			var reversedInvoices = new List<InvoicingBase>();

			foreach (InvoicingBase invoiceCreditNote in TransactionsToReverse)
			{
				cFXReverser.ReverseJournal(invoiceCreditNote);

				ReversingFactory reversingFactory = new ReversingFactory();
				ReversingBase reverser = reversingFactory.NewReversing(invoiceCreditNote);
				if (reverser.CanReverseTransaction)
				{
					reverser.Reverse();
					reverser.ReverseTransaction.ReversingReason = reversingReason;
					reverser.ReverseTransaction.ReversingCode = reversingCode;
					reversedInvoices.Add(invoiceCreditNote);
					if (invoiceCreditNote.ReverseInvoice != null)
					{
						invoiceCreditNote.ReverseInvoice.ApprovingUserPKList = invoiceCreditNote.ApprovingUserPKList;
						invoiceCreditNote.ReverseInvoice.ApprovalDate = invoiceCreditNote.ApprovalDate;
					}
				}
			}

			return reversedInvoices;
		}

		#region Implementation

		IEnumerable<InvoicingBase> TransactionsToReverse;

		#endregion
	}
}

