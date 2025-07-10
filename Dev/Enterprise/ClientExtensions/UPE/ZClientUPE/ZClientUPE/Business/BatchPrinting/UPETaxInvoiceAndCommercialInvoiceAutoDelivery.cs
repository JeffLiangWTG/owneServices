using Enterprise.DocumentEngine;

namespace Enterprise.Client.UPE.Business
{
	public class UPETaxInvoiceAndCommercialInvoiceAutoDelivery : UPETaxInvoiceAutoDelivery
	{
		public UPETaxInvoiceAndCommercialInvoiceAutoDelivery(Callout callout)
			: base(callout)
		{
		}

		protected override DocumentCommand DocumentCommand
		{
			get { return new UPEDocumentMenuItemLoader(Factory).LoadUPSTaxInvoiceAndCommercialInvoice(); }
		}
	}
}
