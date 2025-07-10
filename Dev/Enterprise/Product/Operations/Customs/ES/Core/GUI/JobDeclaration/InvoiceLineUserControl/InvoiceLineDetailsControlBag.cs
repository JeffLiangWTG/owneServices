using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI;

public sealed class InvoiceLineDetailsControlBag : ControlBag
{
	InvoiceLineDetailsControlBag()
	{
		CommercialReferenceTextBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.CommercialReferenceTextBox));
		T2LItemNumberCalcEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.T2LItemNumberCalcEdit));
		CountryOfDestinationCodeFindBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.CountryOfDestinationCodeFindBox));
		RegionOfDestinationCodeFindBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.RegionOfDestinationCodeFindBox));
		MethodOfPaymentDropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.MethodOfPaymentDropEdit));
		MethodOfPayment2DropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.MethodOfPayment2DropEdit));
		VATIGICTypeDropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.VATIGICTypeDropEdit));
		AIEMTypeDropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.AIEMTypeDropEdit));
		ExciseExemptionDropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.ExciseExemptionDropEdit));
		ExciseCodeDropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.ExciseCodeDropEdit));
		GlobalWarmingPotentialCalcEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.GlobalWarmingPotentialCalcEdit));
		PVPCalcFindBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.PVPCalcFindBox));
		REAProductCodeDropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.REAProductCodeDropEdit));
		READirectConsumptionCheckBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.READirectConsumptionCheckBox));
		HasNonRecycledPlasticsCheckBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.HasNonRecycledPlasticsCheckBox));
	}

	public static InvoiceLineDetailsControlBag Instance => instance ??= new InvoiceLineDetailsControlBag();

	[ThreadStatic]
	static InvoiceLineDetailsControlBag instance;

	protected override Control CreateTemplate() => new InvoiceLineDetailsUserControl();

	public ControlReference CommercialReferenceTextBox { get; }

	public ControlReference T2LItemNumberCalcEdit { get; }

	public ControlReference CountryOfDestinationCodeFindBox { get; }

	public ControlReference RegionOfDestinationCodeFindBox { get; }

	public ControlReference MethodOfPaymentDropEdit { get; }

	public ControlReference MethodOfPayment2DropEdit { get; }

	public ControlReference VATIGICTypeDropEdit { get; }

	public ControlReference AIEMTypeDropEdit { get; }

	public ControlReference ExciseExemptionDropEdit { get; }

	public ControlReference ExciseCodeDropEdit { get; }

	public ControlReference GlobalWarmingPotentialCalcEdit { get; }

	public ControlReference PVPCalcFindBox { get; }

	public ControlReference REAProductCodeDropEdit { get; }

	public ControlReference READirectConsumptionCheckBox { get; }

	public ControlReference HasNonRecycledPlasticsCheckBox { get; }
}
