namespace Enterprise.Customs.ES.Business.CusTempStorage;

public sealed class TemporaryStorageBillConfiguration : EU.Business.CusTempStorage.TemporaryStorageBillConfiguration
{
	protected override EU.Business.CusTempStorage.ITemporaryStorageBillValidationDecider GetValidationDeciderCore() => new TemporaryStorageBillValidationDecider();
}
