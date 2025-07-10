using Enterprise.Customs.EU.TemporaryStorage.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI;

public sealed class G5V1TemporaryStorageGridColumnLayoutProviderFactory : UCC6TemporaryStorageGridColumnLayoutProviderFactory
{
	public override IGridColumnLayoutProvider CreateTemporaryStorageGridColumnLayoutProviderForPreviousDocumentsDetails() => new G5V1TemporaryStoragePreviousDocumentsDetailsGridColumnLayout();
}
