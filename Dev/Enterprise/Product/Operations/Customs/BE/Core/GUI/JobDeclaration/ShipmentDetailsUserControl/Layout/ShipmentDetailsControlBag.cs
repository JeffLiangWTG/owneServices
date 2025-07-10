using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.GUI;

public sealed class ShipmentDetailsControlBag : ControlBag
{
	protected override Control CreateTemplate() => new ShipmentDetailsUserControl();

	public static ShipmentDetailsControlBag Instance => instance ?? (instance = new ShipmentDetailsControlBag());

	[ThreadStatic]
	static ShipmentDetailsControlBag instance;

	ShipmentDetailsControlBag()
	{
		LocationOfGoodsCodeFindBox = RegisterControl(nameof(ShipmentDetailsUserControl.LocationOfGoodsCodeFindBox));
		PresentationStartDateEdit = RegisterControl(nameof(ShipmentDetailsUserControl.PresentationStartDateEdit));
		ShipmentDetailsOriginUserControl = RegisterControl(nameof(ShipmentDetailsUserControl.ShipmentDetailsOriginUserControl));
		ShipmentDetailsFinalDestinationUserControl = RegisterControl(nameof(ShipmentDetailsUserControl.ShipmentDetailsFinalDestinationUserControl));
	}
	public ControlReference LocationOfGoodsCodeFindBox;
	public ControlReference PresentationStartDateEdit;
	public ControlReference ShipmentDetailsOriginUserControl;
	public ControlReference ShipmentDetailsFinalDestinationUserControl;
}
