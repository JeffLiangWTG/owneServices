using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.EU.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public class CommercialInvoiceForm : EU.GUI.CommercialInvoice.CommercialInvoiceForm
	{
		public CommercialInvoiceForm(JobComInvoiceHeader invoiceHeader)
			: base(invoiceHeader)
		{
		}

		protected override EUInvoiceLineUserControl GetImportInvoiceLineUserControl() => new ImportInvoiceLineUserControl();

		protected override EUInvoiceLineUserControl GetExportInvoiceLineUserControl() => new ExportInvoiceLineUserControl();
	}
}
