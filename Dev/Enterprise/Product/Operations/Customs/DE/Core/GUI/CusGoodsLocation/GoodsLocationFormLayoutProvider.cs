using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI;

public class GoodsLocationFormLayoutProvider : EU.GUI.IGoodsLocationFormLayoutProvider
{
	public IPanelLayoutProvider GetGoodsLocationLayout() => new CusGoodsLocationLayout();
}
