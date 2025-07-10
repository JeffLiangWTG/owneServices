namespace Enterprise.Customs.IT.TemporaryStorage.Business;

public class TemporaryStorageLinkPackage : EU.Business.CusTempStorage.TemporaryStorageLinkPackage
{
	public TemporaryStorageLinkPackage(TemporaryStoragePackedItem packedItem)
		: base(packedItem)
	{
	}

	protected override EU.Business.CusTempStorage.TemporaryStorageLinkPackageValidation GetNewValidation()
	{
		return new TemporaryStorageLinkPackageValidation(this);
	}
}
