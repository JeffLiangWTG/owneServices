using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public sealed class ShipmentDetailsControlBag : ControlBag
{
	protected override Control CreateTemplate() => new ShipmentDetailsUserControl();

	public static ShipmentDetailsControlBag Instance => instance ?? (instance = new ShipmentDetailsControlBag());

	[ThreadStatic]
	static ShipmentDetailsControlBag instance;

	ShipmentDetailsControlBag()
	{
		ShipmentDetailsOriginUserControl = RegisterControl(nameof(ShipmentDetailsUserControl.ShipmentDetailsOriginUserControl));
		ShipmentDetailsFinalDestinationUserControl = RegisterControl(nameof(ShipmentDetailsUserControl.ShipmentDetailsFinalDestinationUserControl));
		LocationQualifierDropEdit = RegisterControl(nameof(ShipmentDetailsUserControl.LocationQualifierDropEdit));
		GoodsLocationDUserControl = RegisterControl(nameof(ShipmentDetailsUserControl.GoodsLocationDUserControl));
		GoodsLocationFUserControl = RegisterControl(nameof(ShipmentDetailsUserControl.GoodsLocationFUserControl));
		GoodsLocationFCUserControl = RegisterControl(nameof(ShipmentDetailsUserControl.GoodsLocationFCUserControl));
		GoodsLocationLBLCUserControl = RegisterControl(nameof(ShipmentDetailsUserControl.GoodsLocationLBLCUserControl));
		SubLocationTextBox = RegisterControl(nameof(ShipmentDetailsUserControl.SubLocationTextBox));
		LocationOfGoodsUserControl = RegisterControl(nameof(ShipmentDetailsUserControl.LocationOfGoodsUserControl));
		AdditionalDeliveryTermsTextBox = RegisterControl(nameof(ShipmentDetailsUserControl.AdditionalDeliveryTermsTextBox));
	}

	public ControlReference ShipmentDetailsOriginUserControl { get; }
	public ControlReference ShipmentDetailsFinalDestinationUserControl { get; }
	public ControlReference LocationQualifierDropEdit { get; }
	public ControlReference GoodsLocationDUserControl { get; }
	public ControlReference GoodsLocationFUserControl { get; }
	public ControlReference GoodsLocationFCUserControl { get; }
	public ControlReference GoodsLocationLBLCUserControl { get; }
	public ControlReference SubLocationTextBox { get; }
	public ControlReference LocationOfGoodsUserControl { get; }
	public ControlReference AdditionalDeliveryTermsTextBox { get; }
}
