using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI;

public sealed class ShipmentDetailsControlBag : ControlBag
{
	protected override Control CreateTemplate() => new ShipmentDetailsUserControl();

	public static ShipmentDetailsControlBag Instance => instance ?? (instance = new ShipmentDetailsControlBag());

	[ThreadStatic]
	static ShipmentDetailsControlBag instance;

	ShipmentDetailsControlBag()
	{
		RegionOrTerritoryOfDestinationCodeFindBox = RegisterControl(nameof(ShipmentDetailsUserControl.RegionOrTerritoryOfDestinationCodeFindBox));
		RegionOrTerritoryOfDestinationDropEdit = RegisterControl(nameof(ShipmentDetailsUserControl.RegionOrTerritoryOfDestinationDropEdit));
		DestinationStateDropEdit = RegisterControl(nameof(ShipmentDetailsUserControl.DestinationStateDropEdit));
		PartialWriteoffCheckBox = RegisterControl(nameof(ShipmentDetailsUserControl.PartialWriteoffCheckBox));
		GoodsLocationCodeFindBox = RegisterControl(nameof(ShipmentDetailsUserControl.GoodsLocationCodeFindBox));
		ShipmentDetailsOriginUserControl = RegisterControl(nameof(ShipmentDetailsUserControl.ShipmentDetailsOriginUserControl));
		ShipmentDetailsFinalDestinationUserControl = RegisterControl(nameof(ShipmentDetailsUserControl.ShipmentDetailsFinalDestinationUserControl));
	}

	public ControlReference RegionOrTerritoryOfDestinationCodeFindBox { get; }

	public ControlReference RegionOrTerritoryOfDestinationDropEdit { get; }

	public ControlReference DestinationStateDropEdit { get; }

	public ControlReference PartialWriteoffCheckBox { get; }

	public ControlReference GoodsLocationCodeFindBox { get; }

	public ControlReference ShipmentDetailsOriginUserControl { get; }

	public ControlReference ShipmentDetailsFinalDestinationUserControl { get; }
}
