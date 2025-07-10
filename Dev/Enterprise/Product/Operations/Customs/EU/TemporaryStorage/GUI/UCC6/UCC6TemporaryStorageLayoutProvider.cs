using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public class UCC6TemporaryStorageLayoutProvider : ITemporaryStorageLayoutProvider
	{
		IPanelLayoutProvider ITemporaryStorageLayoutProvider.GetTemporaryStorageDetailsLayout() => new UCC6TemporaryStorageLayout();

		IPanelLayoutWithGridProvider ITemporaryStorageLayoutProvider.GetTemporaryStorageBillWithGridLayout() => new UCC6TemporaryStorageBillWithGridLayout();

		IPanelLayoutWithGridProvider ITemporaryStorageLayoutProvider.GetTemporaryStoragePackagesWithGridLayout() => new UCC6TemporaryStoragePackagesWithGridLayout();

		IPanelLayoutWithGridProvider ITemporaryStorageLayoutProvider.GetTemporaryStoragePackedItemWithGridLayout() => new UCC6TemporaryStoragePackedItemWithGridLayout();

		IPanelLayoutWithGridProvider ITemporaryStorageLayoutProvider.GetTemporaryStoragePreviousDocumentsDetailsLayoutWithGrid() => new UCC6TemporaryStoragePreviousDocumentsDetailsLayoutWithGrid();

		ITemporaryStorageBillDetailTabLayoutProvider ITemporaryStorageLayoutProvider.GetTemporaryStorageBillDetailTabLayout() => new UCC6TemporaryStorageBillDetailTabLayout();

		ITemporaryStorageGridColumnLayoutProviderFactory ITemporaryStorageLayoutProvider.GetTemporaryStorageGridColumnLayoutProviderFactory() => new UCC6TemporaryStorageGridColumnLayoutProviderFactory();
	}
}
