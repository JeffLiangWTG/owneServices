using Enterprise.Customs.EU.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.H7.GUI
{
	sealed class GoodsLocationFormLayoutProvider : IGoodsLocationFormLayoutProvider
	{
		IPanelLayoutProvider IGoodsLocationFormLayoutProvider.GetGoodsLocationLayout() => new CusGoodsLocationLayout();
	}
}
