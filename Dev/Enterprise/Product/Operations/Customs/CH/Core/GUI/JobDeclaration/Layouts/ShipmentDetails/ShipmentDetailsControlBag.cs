using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

public sealed class ShipmentDetailsControlBag : ControlBag
{
	protected override Control CreateTemplate() => new ShipmentDetailsUserControl();

	public static ShipmentDetailsControlBag Instance => instance ?? (instance = new ShipmentDetailsControlBag());

	[ThreadStatic]
	static ShipmentDetailsControlBag instance;

	ShipmentDetailsControlBag()
	{
		PaymentMethodDropEdit = RegisterControl(nameof(ShipmentDetailsUserControl.PaymentMethodDropEdit));
		VatPaidByDropEdit = RegisterControl(nameof(ShipmentDetailsUserControl.VatPaidByDropEdit));
		ClearanceLocationDropEdit = RegisterControl(nameof(ShipmentDetailsUserControl.ClearanceLocationDropEdit));
		LocationOfGoodsDropEdit = RegisterControl(nameof(ShipmentDetailsUserControl.LocationOfGoodsDropEdit));
		AdditionalDecisionInfoCheckBox = RegisterControl(nameof(ShipmentDetailsUserControl.AdditionalDecisionInfoCheckBox));
		PaymentMethodUserControl = RegisterControl(nameof(ShipmentDetailsUserControl.PaymentMethodUserControl));
		VatPaidByUserControl = RegisterControl(nameof(ShipmentDetailsUserControl.VatPaidByUserControl));
	}

	public ControlReference PaymentMethodDropEdit { get; }
	public ControlReference VatPaidByDropEdit { get; }
	public ControlReference ClearanceLocationDropEdit { get; }
	public ControlReference LocationOfGoodsDropEdit { get; }
	public ControlReference AdditionalDecisionInfoCheckBox { get; }
	public ControlReference PaymentMethodUserControl { get; }
	public ControlReference VatPaidByUserControl { get; }
}
