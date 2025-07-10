namespace Enterprise.Customs.IT.TemporaryStorage.Business;

public class TemporaryStorageLinkPackageValidation : EU.Business.CusTempStorage.TemporaryStorageLinkPackageValidation
{
	readonly TemporaryStoragePackedItem packedItem;

	public TemporaryStorageLinkPackageValidation(TemporaryStorageLinkPackage parent)
		: base(parent)
	{
		packedItem = (TemporaryStoragePackedItem)parent.PackedItem;
	}

	protected override void CheckIsLinked()
	{
		base.CheckIsLinked();

		var message = Res.GetString("76F2BF60-47D1-4EEB-BF7F-BDB02BB8F94A", "Only rows with the same Pack Unit are allowed to be selected.");

		var itLinkPackages = packedItem.TemporaryStorageLinkPackages as TemporaryStorageLinkPackageCollection<TemporaryStorageLinkPackage>;

		if (itLinkPackages == null || !Parent.IsLinked || !itLinkPackages.HasMismatchingPackUQLinkedRows)
		{
			Parent.RemoveRowError(message);
			return;
		}
		Parent.AddRowError(message);
	}
}
