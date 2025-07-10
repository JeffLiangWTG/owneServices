using Enterprise.Customs.EU.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.UCC6TemporaryStorage
{
	public class UCC6TemporaryStorageGoodsLocationFormLayoutProvider : IGoodsLocationFormLayoutProvider
	{
		public IPanelLayoutProvider GetGoodsLocationLayout() => new UCC6TemporaryStorageCusGoodsLocationLayout();
	}
}
