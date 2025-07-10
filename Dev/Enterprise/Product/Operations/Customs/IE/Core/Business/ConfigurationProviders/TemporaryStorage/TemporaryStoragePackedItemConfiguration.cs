using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.IE.Business.CusTempStorage;
public sealed class TemporaryStoragePackedItemConfiguration : EU.Business.CusTempStorage.TemporaryStoragePackedItemConfiguration
{
	protected override ITemporaryStoragePackedItemValidationDecider GetValidationDeciderCore() => new TemporaryStoragePackedItemValidationDecider();
}
