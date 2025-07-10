using Enterprise.Customs.EU.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public sealed class GoodsLocationFormLayoutProvider : IGoodsLocationFormLayoutProvider
{
	IPanelLayoutProvider IGoodsLocationFormLayoutProvider.GetGoodsLocationLayout() => new CusGoodsLocationLayout();
}
