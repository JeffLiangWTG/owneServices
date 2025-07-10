using System;
using Enterprise.Customs.IN.Business;

namespace Enterprise.Customs.IN.GUI;

public partial class CommercialInvoiceForm : Customs.GUI.CommercialInvoiceForm
{
	[Obsolete("Do not call. Only for designer use.")]
	public CommercialInvoiceForm()
	{
	}

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
