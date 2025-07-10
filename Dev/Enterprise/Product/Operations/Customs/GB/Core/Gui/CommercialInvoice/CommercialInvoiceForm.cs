using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.GUI;

namespace Enterprise.Customs.GB.GUI
{
	public class CommercialInvoiceForm : EU.GUI.CommercialInvoice.CommercialInvoiceForm
	{
		public CommercialInvoiceForm(JobComInvoiceHeader invoiceHeader) : base(invoiceHeader)
		{
		}

		protected override EUInvoiceLineUserControl GetImportInvoiceLineUserControl()
		{
			return new ImportInvoiceLineUserControl();
		}

		protected override EUInvoiceLineUserControl GetExportInvoiceLineUserControl()
		{
			return new ExportInvoiceLineUserControl();
		}
	}
}
