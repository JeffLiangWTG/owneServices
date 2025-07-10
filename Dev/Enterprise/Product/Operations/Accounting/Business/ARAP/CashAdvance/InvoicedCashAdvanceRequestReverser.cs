using System.Linq;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.Base.Reversing;

namespace Enterprise.Accounting.Business.ARAP.CashAdvance
{
	public class InvoicedCashAdvanceRequestReverser : ICashAdvanceRequestProcessingByInvoiceVisitor, ICashAdvanceRequestProcessingByJournalVisitor
	{
		public void Visit(ARInvoice arInvoice) => VisitInternal(arInvoice);

		public void Visit(APInvoice apInvoice) => VisitInternal(apInvoice);

		void VisitInternal(Invoice invoice)
		{
			if (invoice != null && invoice.Lines.Any())
			{
				var carInfoByInvoice = new CashAdvanceRequestInfoByInvoice(invoice);
				var carRequirementLines = carInfoByInvoice.CashAdvanceRequirements;
				if (carRequirementLines?.Any() ?? false)
				{
					var invoicedLines = carRequirementLines.Where(l => l.IsInvoiced).ToArray();
					if (invoicedLines.Any())
					{
						foreach (var invoicedLine in invoicedLines)
						{
							invoicedLine.UndoInvoicedStatus(invoice);
						}

						var overpaymentJournals = invoice.LoadOverpaymentCAIJournals();
						var reversingFactory = new ReversingFactory();
						foreach (var journal in overpaymentJournals)
						{
							var reversing = reversingFactory.NewReversing(journal);
							reversing.Reverse();
						}
					}
				}
			}
		}

		public void Visit(ARJournal arJournal) => VisitInternal(arJournal);

		public void Visit(APJournal apJournal) => VisitInternal(apJournal);

		void VisitInternal(Journal.Journal journal)
		{
			var reversingFactory = new ReversingFactory();
			var reverser = reversingFactory.NewReversing(journal);
			if (!reverser.CanReverseTransaction)
			{
				throw new CannotGenerateCashAdvanceJournalException(null, Res.GetString("e36b896f-8e9d-4295-a989-335e04a0b142", "JNL {0} {1} cannot be reversed for the following reason.\r\n{2}"
																, journal.AH_TransactionCategory
																, journal.AH_TransactionNum
																, reverser.CantReverseErrorMessage));
			}
			reverser.Reverse();
		}
	}
}
