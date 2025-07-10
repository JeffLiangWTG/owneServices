using CargoWise.Types;

namespace Enterprise.Accounting.ElectronicMessaging.Vietnam
{
	public class VietnamEInvoiceDocumentResponse
	{
		public ZString xml { get; set; }

		public ZString pdf { get; set; }

		public object doc { get; set; }
	}
}
