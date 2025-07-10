using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

public sealed class TemporaryStorageBillConfiguration : EU.Business.CusTempStorage.TemporaryStorageBillConfiguration
{
	protected override ITemporaryStorageBillValidationDecider GetValidationDeciderCore() => new TemporaryStorageBillValidationDecider();
}
