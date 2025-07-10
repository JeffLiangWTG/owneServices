using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI
{
	public sealed class GoodsItemDetailsControlBag : ControlBag
	{
		public GoodsItemDetailsControlBag()
		{
			IsVehiclesCheckBox = RegisterControl(nameof(GoodsItemDetailsUserControl.IsVehiclesCheckBox));
			ExciseCodeDropEdit = RegisterControl(nameof(GoodsItemDetailsUserControl.ExciseCodeDropEdit));
			PVPValueCalcDropEdit = RegisterControl(nameof(GoodsItemDetailsUserControl.PVPValueCalcDropEdit));
		}

		public static GoodsItemDetailsControlBag Instance => instance ?? (instance = new GoodsItemDetailsControlBag());

		[ThreadStatic]
		static GoodsItemDetailsControlBag instance;

		public ControlReference IsVehiclesCheckBox { get; }

		public ControlReference ExciseCodeDropEdit { get; }

		public ControlReference PVPValueCalcDropEdit { get; }

		protected override Control CreateTemplate() => new GoodsItemDetailsUserControl();
	}
}
