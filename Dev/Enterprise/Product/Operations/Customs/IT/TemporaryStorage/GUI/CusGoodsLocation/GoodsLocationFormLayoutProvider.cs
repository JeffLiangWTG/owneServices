using Enterprise.Customs.EU.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.TemporaryStorage.GUI;

public sealed class GoodsLocationFormLayoutProvider : IGoodsLocationFormLayoutProvider
{
	IPanelLayoutProvider IGoodsLocationFormLayoutProvider.GetGoodsLocationLayout() => new CusGoodsLocationLayout();
}
