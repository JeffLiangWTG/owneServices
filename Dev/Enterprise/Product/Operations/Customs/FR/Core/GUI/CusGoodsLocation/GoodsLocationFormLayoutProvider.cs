using Enterprise.Customs.EU.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI
{
	public sealed class GoodsLocationFormLayoutProvider : IGoodsLocationFormLayoutProvider
	{
		public IPanelLayoutProvider GetGoodsLocationLayout() => new CusGoodsLocationLayout();
	}
}
