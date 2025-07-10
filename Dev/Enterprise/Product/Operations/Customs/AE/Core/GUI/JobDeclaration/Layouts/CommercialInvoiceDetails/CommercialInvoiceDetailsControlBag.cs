using System;
using System.Windows.Forms;
using Enterprise.Customs.AE.GUI;
using Enterprise.ZArchitecture.GUI;

public sealed class CommercialInvoiceDetailsControlBag : ControlBag
{
	protected override Control CreateTemplate() => new CommercialInvoiceDetailsUserControl();

	public static CommercialInvoiceDetailsControlBag Instance => instance ??= new CommercialInvoiceDetailsControlBag();

	[ThreadStatic]
	static CommercialInvoiceDetailsControlBag instance;

	CommercialInvoiceDetailsControlBag()
	{
		PaymentMethodDropEdit = RegisterControl(nameof(CommercialInvoiceDetailsUserControl.PaymentMethodDropEdit));
		ValuationCodeDropEdit = RegisterControl(nameof(CommercialInvoiceDetailsUserControl.ValuationCodeDropEdit));
		TotNoOfInvPagesCalcEdit = RegisterControl(nameof(CommercialInvoiceDetailsUserControl.TotNoOfInvPagesCalcEdit));
		InvoiceTypeDropEdit = RegisterControl(nameof(CommercialInvoiceDetailsUserControl.InvoiceTypeDropEdit));
		AttestationNoTextBox = RegisterControl(nameof(CommercialInvoiceDetailsUserControl.AttestationNoTextBox));
	}

	public ControlReference PaymentMethodDropEdit;
	public ControlReference ValuationCodeDropEdit;
	public ControlReference TotNoOfInvPagesCalcEdit;
	public ControlReference InvoiceTypeDropEdit;
	public ControlReference AttestationNoTextBox;
}
