using Enterprise.Customs.EU.TemporaryStorage.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI
{
	public class G5V1TemporaryStorageLayoutProvider : ITemporaryStorageLayoutProvider
	{
		IPanelLayoutProvider ITemporaryStorageLayoutProvider.GetTemporaryStorageDetailsLayout() => new G5V1TemporaryStorageLayout();

		IPanelLayoutWithGridProvider ITemporaryStorageLayoutProvider.GetTemporaryStorageBillWithGridLayout() => new UCC6TemporaryStorageBillWithGridLayout();

		IPanelLayoutWithGridProvider ITemporaryStorageLayoutProvider.GetTemporaryStoragePackagesWithGridLayout() => new G5V1TemporaryStoragePackagesWithGridLayout();

		IPanelLayoutWithGridProvider ITemporaryStorageLayoutProvider.GetTemporaryStoragePackedItemWithGridLayout() => new G5V1TemporaryStoragePackedItemWithGridLayout();

		IPanelLayoutWithGridProvider ITemporaryStorageLayoutProvider.GetTemporaryStoragePreviousDocumentsDetailsLayoutWithGrid() => new G5V1TemporaryStoragePreviousDocumentsDetailsLayoutWithGrid();

		ITemporaryStorageBillDetailTabLayoutProvider ITemporaryStorageLayoutProvider.GetTemporaryStorageBillDetailTabLayout() => new UCC6TemporaryStorageBillDetailTabLayout();

		ITemporaryStorageGridColumnLayoutProviderFactory ITemporaryStorageLayoutProvider.GetTemporaryStorageGridColumnLayoutProviderFactory() => new G5V1TemporaryStorageGridColumnLayoutProviderFactory();
	}
}
