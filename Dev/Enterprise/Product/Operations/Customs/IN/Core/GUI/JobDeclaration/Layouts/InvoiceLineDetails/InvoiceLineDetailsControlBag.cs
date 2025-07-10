using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI;

public sealed class InvoiceLineDetailsControlBag : ControlBag
{
	public static InvoiceLineDetailsControlBag Instance => instance ??= new InvoiceLineDetailsControlBag();

	[ThreadStatic]
	static InvoiceLineDetailsControlBag instance;

	InvoiceLineDetailsControlBag()
	{
		PMVFieldsUserControl = RegisterControl(nameof(InvoiceLineDetailsLayoutUserControl.PMVFieldsUserControl));
		TotalPMVCalcFindBox = RegisterControl(nameof(InvoiceLineDetailsLayoutUserControl.TotalPMVCalcFindBox));
		UnitPriceCalcFindBox = RegisterControl(nameof(InvoiceLineDetailsLayoutUserControl.UnitPriceCalcFindBox));
		UnitQuantityCalcDropEdit = RegisterControl(nameof(InvoiceLineDetailsLayoutUserControl.UnitQuantityCalcDropEdit));
		AccessoryStatusDropEdit = RegisterControl(nameof(InvoiceLineDetailsLayoutUserControl.AccessoryStatusDropEdit));
		EndUseCodeFindBox = RegisterControl(nameof(InvoiceLineDetailsLayoutUserControl.EndUseCodeFindBox));
		RewardItemDropEdit = RegisterControl(nameof(InvoiceLineDetailsLayoutUserControl.RewardItemDropEdit));
		TransitCountryDropEdit = RegisterControl(nameof(InvoiceLineDetailsLayoutUserControl.TransitCountryDropEdit));
		IGSTPaymentGroupBox = RegisterControl(nameof(InvoiceLineDetailsLayoutUserControl.IGSTPaymentGroupBox));
		AccessoryDescriptionLongTextBox = RegisterControl(nameof(InvoiceLineDetailsLayoutUserControl.AccessoryDescriptionLongTextBox));
	}

	protected override Control CreateTemplate() => new InvoiceLineDetailsLayoutUserControl();

	public ControlReference PMVFieldsUserControl { get; }
	public ControlReference TotalPMVCalcFindBox { get; }
	public ControlReference UnitPriceCalcFindBox { get; }
	public ControlReference UnitQuantityCalcDropEdit { get; }
	public ControlReference AccessoryStatusDropEdit { get; }
	public ControlReference EndUseCodeFindBox { get; }
	public ControlReference IGSTPaymentGroupBox { get; }
	public ControlReference RewardItemDropEdit { get; }
	public ControlReference TransitCountryDropEdit { get; }
	public ControlReference AccessoryDescriptionLongTextBox { get; }
}
