using Enterprise.Customs.EU.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.NCTS.GUI;

public class GoodsLocationFormLayoutProvider : IGoodsLocationFormLayoutProvider
{
	public IPanelLayoutProvider GetGoodsLocationLayout() => new CusGoodsLocationLayout();
}
