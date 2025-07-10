using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.NCTS.GUI
{
	sealed class GoodsItemDetailsControlBag : ControlBag
	{
		protected override Control CreateTemplate() => new GoodsItemDetailsLayoutsUserControl();

		public static GoodsItemDetailsControlBag Instance => instance ?? (instance = new GoodsItemDetailsControlBag());

		[ThreadStatic]
		static GoodsItemDetailsControlBag instance;

		GoodsItemDetailsControlBag()
		{
			GoodsItemDetailsItemNoPlusMainPackUserControl = RegisterControl(nameof(GoodsItemDetailsLayoutsUserControl.GoodsItemDetailsItemNoPlusMainPackUserControl));
		}

		public ControlReference GoodsItemDetailsItemNoPlusMainPackUserControl { get; }
	}
}
