using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public sealed class InvoiceLineDetailsControlBag : ControlBag
{
	public InvoiceLineDetailsControlBag()
	{
		PortTaxRateDropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.PortTaxRateDropEdit));
		VatTypeAndDescriptionUserControl = RegisterControl(nameof(InvoiceLineDetailsUserControl.VatTypeAndDescriptionUserControl));
		GoodsOriginDropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.GoodsOriginDropEdit));
		OriginCountryStateUserControl = RegisterControl(nameof(InvoiceLineDetailsUserControl.OriginCountryStateUserControl));
		CountryOfDestinationDropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.CountryOfDestinationDropEdit));
		CountryOfExportDropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.CountryOfExportDropEdit));
		InvoiceNumberDropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.InvoiceNumberDropEdit));
	}

	public static InvoiceLineDetailsControlBag Instance => instance ?? (instance = new InvoiceLineDetailsControlBag());

	[ThreadStatic]
	static InvoiceLineDetailsControlBag instance;

	protected override Control CreateTemplate() => new InvoiceLineDetailsUserControl();

	public ControlReference PortTaxRateDropEdit { get; }
	public ControlReference VatTypeAndDescriptionUserControl { get; }
	public ControlReference GoodsOriginDropEdit { get; }
	public ControlReference OriginCountryStateUserControl { get; }
	public ControlReference CountryOfDestinationDropEdit { get; }
	public ControlReference CountryOfExportDropEdit { get; }
	public ControlReference InvoiceNumberDropEdit { get; }
}
