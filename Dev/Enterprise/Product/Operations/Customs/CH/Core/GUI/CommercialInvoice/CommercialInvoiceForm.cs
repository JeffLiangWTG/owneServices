using System;
using Enterprise.Customs.CH.Business;

namespace Enterprise.Customs.CH.GUI;

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

	protected override Customs.GUI.BaseInvoiceLineUserControl GetNewInvoiceLineUserControl()
	{
		Customs.GUI.BaseInvoiceLineUserControl result;

		if (JobDeclaration.IsImport)
		{
			result = new ImportInvoiceLineUserControl();
		}
		else
		{
			result = new ExportInvoiceLineUserControl();
		}

		return result;
	}

	protected override void InitialiseForm()
	{
		base.InitialiseForm();
		InitializeComponent();
	}
}
