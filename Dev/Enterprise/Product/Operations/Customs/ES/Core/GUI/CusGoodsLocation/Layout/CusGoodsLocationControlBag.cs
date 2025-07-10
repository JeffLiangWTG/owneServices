using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI
{
	public sealed class CusGoodsLocationControlBag : ControlBag
	{
		public CusGoodsLocationControlBag()
		{
			ESAuthorizationCodeFindBox = RegisterControl(nameof(CusGoodsLocationUserControl.ESAuthorizationCodeFindBox));
		}

		public static CusGoodsLocationControlBag Instance => instance ?? (instance = new CusGoodsLocationControlBag());

		[ThreadStatic]
		static CusGoodsLocationControlBag instance;

		public ControlReference ESAuthorizationCodeFindBox { get; }

		protected override Control CreateTemplate() => new CusGoodsLocationUserControl();
	}
}
