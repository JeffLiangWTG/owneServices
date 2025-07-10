using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.GUI
{
	public class CommercialInvoiceForm : EU.GUI.CommercialInvoice.CommercialInvoiceForm
	{
		public CommercialInvoiceForm(JobComInvoiceHeader invoiceHeader)
			: base(invoiceHeader)
		{
		}

		protected override Customs.GUI.CommonInvoiceHeaderUserControl GetHeaderUserControl() => new InvoiceHeaderUserControl();

		protected override EUInvoiceLineUserControl GetImportInvoiceLineUserControl()
		{
			return new ImportComInvoiceLineUserControl();
		}

		protected override EUInvoiceLineUserControl GetExportInvoiceLineUserControl()
		{
			return new ExportComInvoiceLineUserControl();
		}
	}
}
