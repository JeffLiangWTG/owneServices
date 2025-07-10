using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI
{
	public sealed class ShipmentDetailsControlBag : ControlBag
	{
		protected override Control CreateTemplate() => new ShipmentDetailsUserControl();

		public static ShipmentDetailsControlBag Instance => instance ?? (instance = new ShipmentDetailsControlBag());

		[ThreadStatic]
		static ShipmentDetailsControlBag instance;

		ShipmentDetailsControlBag()
		{
			UcrAndBillTypeUserControl = RegisterControl(nameof(ShipmentDetailsUserControl.UcrAndBillTypeUserControl));
			CargoArrivalUserControl = RegisterControl(nameof(ShipmentDetailsUserControl.CargoArrivalUserControl));
		}

		public ControlReference UcrAndBillTypeUserControl;
		public ControlReference CargoArrivalUserControl;
	}
}
