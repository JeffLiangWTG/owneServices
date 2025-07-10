using System;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.KR.Business
{
	public class ExtendReExportDateMessageSendingObjectCollection : NonPersistentBusinessObjectCollection<ExtendReExportDateMessageSendingObject>
	{
		public ExtendReExportDateMessageSendingObjectCollection(JobDeclaration declaration)
			: base(declaration.Factory)
		{
			this.declaration = declaration;
			PopulateElements();
		}
		readonly JobDeclaration declaration; 

		void PopulateElements()
		{
			RemoveAll();
			foreach (CusEntryHeader entry in declaration.ActiveEntryHeaders)
			{
				foreach (var invoiceLine in entry.InvoiceLines.Cast<JobComInvoiceLine>().GroupBy(x => x.JI_ScheduledReExportDate))
				{
					Add(new ExtendReExportDateMessageSendingObject(entry, invoiceLine.Key));
				}
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => throw new NotImplementedException();
		protected override bool AllowNewCore => false;
	}
}
