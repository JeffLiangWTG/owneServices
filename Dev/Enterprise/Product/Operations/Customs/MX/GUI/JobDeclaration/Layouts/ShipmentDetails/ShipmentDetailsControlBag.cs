using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.MX.GUI
{
	public sealed class ShipmentDetailsControlBag : ControlBag
	{
		protected override Control CreateTemplate() => new ShipmentDetailsUserControl();

		public static ShipmentDetailsControlBag Instance => instance ?? (instance = new ShipmentDetailsControlBag());

		[ThreadStatic]
		static ShipmentDetailsControlBag instance;

		ShipmentDetailsControlBag()
		{
			GoodsOriginDropEdit = RegisterControl(nameof(ShipmentDetailsUserControl.GoodsOriginDropEdit));
			GoodsDestinationDropEdit = RegisterControl(nameof(ShipmentDetailsUserControl.GoodsDestinationDropEdit));
		}

		public ControlReference GoodsOriginDropEdit;
		public ControlReference GoodsDestinationDropEdit;
	}
}
