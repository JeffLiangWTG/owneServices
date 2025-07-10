using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.BR.Business
{
	public class NFeInvoiceLineExportObjectCollection : NonPersistentBusinessObjectCollection<NFeInvoiceLineExportObject>
	{
		public NFeInvoiceLineExportObjectCollection(CusEntryHeader entryHeader)
			: base(entryHeader.Factory)
		{
			this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
		}

		readonly CusEntryHeader entryHeader;

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException("Creating of new elements is not allowed.");
		}

		public override void Load()
		{
			var invoiceLines = entryHeader.InvoiceLines.Cast<JobComInvoiceLine>().ToList();

			foreach (var removedLine in this.Cast<NFeInvoiceLineExportObject>().Where(x => !invoiceLines.Contains(x.InvoiceLine)).ToArray())
			{
				RemoveAndDelete(removedLine);
			}

			AddRange(invoiceLines.Where(x => FindByInvoiceLine(x) == null).Select(x => NFeInvoiceLineExportObject.New(x)));
		}

		NFeInvoiceLineExportObject FindByInvoiceLine(JobComInvoiceLine invoiceLine) => this.Cast<NFeInvoiceLineExportObject>().FirstOrDefault(x => x.InvoiceLine == invoiceLine);
	}
}
