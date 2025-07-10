using Enterprise.Customs.CN.Business;

namespace Enterprise.Customs.CN.GUI.CommercialInvoice
{
	public partial class CommercialInvoiceForm : Customs.GUI.CommercialInvoiceForm
	{
		public CommercialInvoiceForm() { }

		public CommercialInvoiceForm(JobComInvoiceHeader header)
			: base(header)
		{
		}

		protected override Customs.GUI.CommonInvoiceHeaderUserControl GetHeaderUserControl() => new InvoiceHeaderUserControl();

		protected override Customs.GUI.BaseInvoiceLineUserControl GetNewInvoiceLineUserControl() => new InvoiceLineUserControl();
	}
}
