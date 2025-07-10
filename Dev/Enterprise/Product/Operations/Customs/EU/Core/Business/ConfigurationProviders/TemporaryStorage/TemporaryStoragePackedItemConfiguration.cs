namespace Enterprise.Customs.EU.Business.CusTempStorage;

public class TemporaryStoragePackedItemConfiguration
{
	public ITemporaryStoragePackedItemValidationDecider GetValidationDecider() => GetValidationDeciderCore();
	protected virtual ITemporaryStoragePackedItemValidationDecider GetValidationDeciderCore() => new TemporaryStoragePackedItemValidationDecider();
}
