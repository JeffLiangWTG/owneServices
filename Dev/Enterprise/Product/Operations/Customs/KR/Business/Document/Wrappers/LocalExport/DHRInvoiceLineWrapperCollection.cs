using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class DHRInvoiceLineWrapperCollection : NonPersistentBusinessObjectCollection<DHRInvoiceLineWrapper>
	{
		public DHRInvoiceLineWrapperCollection(IEnumerable<IImportDHRInvoiceLine> invoiceLines, BusinessObjectFactory factory)
		{
			PopulateElements(invoiceLines, factory);
		}

		void PopulateElements(IEnumerable<IImportDHRInvoiceLine> invoiceLines, BusinessObjectFactory factory)
		{
			if (invoiceLines != null)
			{
				foreach (var item in invoiceLines)
				{
					Add(new DHRInvoiceLineWrapper(item, factory));
				}
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => throw new NotImplementedException();

		protected override bool AllowNewCore => false;
	}
}
