using Enterprise.Customs.EU.TemporaryStorage.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.TemporaryStorage.GUI;

sealed class UCC6TemporaryStorageLayoutProvider : ITemporaryStorageLayoutProvider
{
	public IPanelLayoutProvider GetTemporaryStorageDetailsLayout() => new UCC6TemporaryStorageLayout();

	public IPanelLayoutWithGridProvider GetTemporaryStorageBillWithGridLayout() => new UCC6TemporaryStorageBillWithGridLayout();

	public IPanelLayoutWithGridProvider GetTemporaryStoragePackagesWithGridLayout() => new UCC6TemporaryStoragePackagesWithGridLayout();

	public IPanelLayoutWithGridProvider GetTemporaryStoragePackedItemWithGridLayout() => new UCC6TemporaryStoragePackedItemWithGridLayout();

	public IPanelLayoutWithGridProvider GetTemporaryStoragePreviousDocumentsDetailsLayoutWithGrid() => new UCC6TemporaryStoragePreviousDocumentsDetailsLayoutWithGrid();

	public ITemporaryStorageBillDetailTabLayoutProvider GetTemporaryStorageBillDetailTabLayout() => new UCC6TemporaryStorageBillDetailTabLayout();

	public ITemporaryStorageGridColumnLayoutProviderFactory GetTemporaryStorageGridColumnLayoutProviderFactory() => new UCC6TemporaryStorageGridColumnLayoutProviderFactory();
}
