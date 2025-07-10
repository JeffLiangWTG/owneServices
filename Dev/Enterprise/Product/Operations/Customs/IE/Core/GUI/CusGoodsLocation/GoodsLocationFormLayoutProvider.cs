using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
{
	public class GoodsLocationFormLayoutProvider : EU.GUI.IGoodsLocationFormLayoutProvider
	{
		public IPanelLayoutProvider GetGoodsLocationLayout() => new CusGoodsLocationLayout();
	}
}
