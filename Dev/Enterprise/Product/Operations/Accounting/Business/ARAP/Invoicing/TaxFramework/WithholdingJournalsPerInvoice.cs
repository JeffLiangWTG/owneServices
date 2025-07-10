using System.Collections.Generic;
using CargoWise.Common;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework
{
	public class WithholdingJournalsPerInvoice
	{
		public WithholdingJournalsPerInvoice(InvoicingBase parentInvoice, IReadOnlyCollection<WithholdingJournalForDisplay> journals)
		{
			ParentInvoice = Argument.NotNull(parentInvoice, nameof(parentInvoice));
			Journals = Argument.NotNull(journals, nameof(journals));
		}

		public InvoicingBase ParentInvoice { get; }

		public IReadOnlyCollection<WithholdingJournalForDisplay> Journals { get; }
	}
}
