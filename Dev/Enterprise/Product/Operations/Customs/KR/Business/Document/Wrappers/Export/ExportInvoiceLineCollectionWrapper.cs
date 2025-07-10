using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class ExportInvoiceLineCollectionWrapper : NonPersistentBusinessObjectCollection<ExportInvoiceLineWrapper>
	{
		public ExportInvoiceLineCollectionWrapper(IEnumerable<IExportInvoiceLine> exportInvoiceLines, BusinessObjectFactory factory)
			: base(factory)
		{
			PopulateElements(factory, exportInvoiceLines);
		}

		void PopulateElements(BusinessObjectFactory factory, IEnumerable<IExportInvoiceLine> exportInvoiceLines)
		{
			if (exportInvoiceLines != null)
			{
				foreach (var item in exportInvoiceLines)
				{
					Add(new ExportInvoiceLineWrapper(item, factory));
				}
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => throw new NotImplementedException();

		protected override bool AllowNewCore => false;
	}
}
