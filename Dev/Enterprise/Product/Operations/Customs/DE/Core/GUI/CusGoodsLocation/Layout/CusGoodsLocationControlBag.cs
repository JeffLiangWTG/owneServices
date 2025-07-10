using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public sealed class CusGoodsLocationControlBag : ControlBag
	{
		public CusGoodsLocationControlBag()
		{
			LoadingPlaceTextBox = RegisterControl(nameof(CusGoodsLocationUserControl.LoadingPlaceTextBox));
			AdditionalIdentifierDropEdit = RegisterControl(nameof(CusGoodsLocationUserControl.AdditionalIdentifierDropEdit));
		}

		public static CusGoodsLocationControlBag Instance => instance ?? (instance = new CusGoodsLocationControlBag());

		[ThreadStatic]
		static CusGoodsLocationControlBag instance;

		public ControlReference LoadingPlaceTextBox { get; }

		public ControlReference AdditionalIdentifierDropEdit { get; }

		protected override Control CreateTemplate() => new CusGoodsLocationUserControl();
	}
}
