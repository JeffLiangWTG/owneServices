using Enterprise.Customs.JP.Business;

namespace Enterprise.Customs.JP.GUI.CommercialInvoice
{
	public partial class CommercialInvoiceForm : Customs.GUI.CommercialInvoiceForm
	{
		public CommercialInvoiceForm() { }

		public CommercialInvoiceForm(JobComInvoiceHeader header)
			: base(header)
		{
		}

		protected override Customs.GUI.CommonInvoiceHeaderUserControl GetHeaderUserControl()
		{
			return new InvoiceHeaderUserControl();
		}

		protected override Customs.GUI.BaseInvoiceLineUserControl GetNewInvoiceLineUserControl()
		{
			if (Invoice.IsImport)
			{
				return new ImportInvoiceLineUserControl();
			}
			else
			{
				return new ExportInvoiceLineUserControl();
			}
		}
	}
}
