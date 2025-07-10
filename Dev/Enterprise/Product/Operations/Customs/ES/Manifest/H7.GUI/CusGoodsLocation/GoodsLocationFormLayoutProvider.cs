using Enterprise.Customs.EU.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.Manifest.H7.GUI
{
	public sealed class GoodsLocationFormLayoutProvider : IGoodsLocationFormLayoutProvider
	{
		IPanelLayoutProvider IGoodsLocationFormLayoutProvider.GetGoodsLocationLayout() => new CusGoodsLocationLayout();
	}
}
