using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.NCTS.GUI
{
	public class CusGoodsLocationControlBag : ControlBag
	{
		protected override Control CreateTemplate() => new CusGoodsLocationUserControl();

		public static CusGoodsLocationControlBag Instance => instance ?? (instance = new CusGoodsLocationControlBag());

		[ThreadStatic]
		static CusGoodsLocationControlBag instance;

		CusGoodsLocationControlBag()
		{
			OrganisationFindBox = RegisterControl(nameof(CusGoodsLocationUserControl.OrganisationFindBox));
			AuthorizationCodeFindBox = RegisterControl(nameof(CusGoodsLocationUserControl.AuthorizationCodeFindBox));
			AdditionalIdentifierDropEdit = RegisterControl(nameof(CusGoodsLocationUserControl.AdditionalIdentifierDropEdit));
		}

		public ControlReference OrganisationFindBox { get; }

		public ControlReference AuthorizationCodeFindBox { get; }

		public ControlReference AdditionalIdentifierDropEdit { get; }
	}
}
