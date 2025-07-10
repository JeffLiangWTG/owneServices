using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;
public sealed class TemporaryStoragePackedItemConfiguration : EU.Business.CusTempStorage.TemporaryStoragePackedItemConfiguration
{
	protected override ITemporaryStoragePackedItemValidationDecider GetValidationDeciderCore() => new TemporaryStoragePackedItemValidationDecider();
}
