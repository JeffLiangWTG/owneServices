using System;
using CargoWise.Types;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.EU.Business.CusTempStorage;

public class TemporaryStorageDutyAndTaxCollection : AsycudaPackedItemTaxCollection<TemporaryStorageDutyAndTax, TemporaryStoragePackedItem>
{
	public TemporaryStorageDutyAndTaxCollection(TemporaryStoragePackedItem master) : base(master)
	{
	}

	public override Type GetTypeOfElementsFromPK(ZGuid pk)
	{
		return typeof(TemporaryStorageDutyAndTax);
	}

	protected override bool AllowNewCore => false;

	protected override bool AllowRemoveCore => false;
}
