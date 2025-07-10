using CargoWise.EntityFramework;
//using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class ExportInvoiceLineWrapper : NonPersistentBusinessObject
	{
		public ExportInvoiceLineWrapper(IExportInvoiceLine invoiceLine, BusinessObjectFactory factory)
		{
			this.InvoiceLine = invoiceLine;
		}
		public IExportInvoiceLine InvoiceLine { get; }
	}
}
