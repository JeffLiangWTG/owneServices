using Enterprise.Customs.AsycudaCustoms.Business;

namespace Enterprise.Customs.AsycudaCustoms.GUI.CommercialInvoice
{
	public partial class CommercialInvoiceForm : Customs.GUI.CommercialInvoiceForm
	{
		public CommercialInvoiceForm() { }

		public CommercialInvoiceForm(JobComInvoiceHeader header)
			: base(header)
		{
		}

		protected override Customs.GUI.CommonInvoiceHeaderUserControl GetHeaderUserControl() => new InvoiceHeaderUserControl();

		protected override Customs.GUI.BaseInvoiceLineUserControl GetNewInvoiceLineUserControl() => new BaseInvoiceLineUserControl();

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}
	}
}
