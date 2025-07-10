using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public interface ITemporaryStorageLayoutProvider
	{
		IPanelLayoutProvider GetTemporaryStorageDetailsLayout();

		IPanelLayoutWithGridProvider GetTemporaryStorageBillWithGridLayout();

		IPanelLayoutWithGridProvider GetTemporaryStoragePackagesWithGridLayout();

		IPanelLayoutWithGridProvider GetTemporaryStoragePackedItemWithGridLayout();

		IPanelLayoutWithGridProvider GetTemporaryStoragePreviousDocumentsDetailsLayoutWithGrid();

		ITemporaryStorageBillDetailTabLayoutProvider GetTemporaryStorageBillDetailTabLayout();

		ITemporaryStorageGridColumnLayoutProviderFactory GetTemporaryStorageGridColumnLayoutProviderFactory();
	}
}
