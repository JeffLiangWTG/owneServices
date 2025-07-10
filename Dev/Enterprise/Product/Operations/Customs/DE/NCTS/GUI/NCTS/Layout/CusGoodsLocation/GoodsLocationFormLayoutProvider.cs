using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.NCTS.GUI
{
	public class GoodsLocationFormLayoutProvider : EU.GUI.IGoodsLocationFormLayoutProvider
	{
		public IPanelLayoutProvider GetGoodsLocationLayout() => new CusGoodsLocationLayout();
	}
}
