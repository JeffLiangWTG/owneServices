using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	class APCreditNoteCollection : InvoicingBaseCollection
	{
		public APCreditNoteCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pk)
		{
			return typeof(APCreditNote);
		}
	}
}