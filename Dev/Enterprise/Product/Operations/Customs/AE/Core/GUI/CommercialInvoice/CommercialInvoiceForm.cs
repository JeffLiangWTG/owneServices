using Enterprise.Customs.AE.Business;

namespace Enterprise.Customs.AE.GUI;

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

	protected override void InitialiseForm()
	{
		base.InitialiseForm();
		InitializeComponent();
	}
}
