using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class DHRInvoiceLineWrapper : NonPersistentBusinessObject
	{
		public DHRInvoiceLineWrapper(IImportDHRInvoiceLine invoiceLine, BusinessObjectFactory factory)
			: base(factory)
		{
			this.invoiceLine = invoiceLine;
		}

		public IImportDHRInvoiceLine invoiceLine;
	}
}
