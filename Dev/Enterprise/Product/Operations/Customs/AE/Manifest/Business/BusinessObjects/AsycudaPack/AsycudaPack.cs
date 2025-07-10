using System;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AE.Manifest.Business;

public class AsycudaPack : ASYCUDA.Business.AsycudaPack
{
	public AsycudaPack(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new AsycudaBill Bill => (AsycudaBill)base.Bill;

	public new AsycudaContainer Container => (AsycudaContainer)base.Container;

	protected override ManifestBase.AsycudaPackValidation GetNewValidation() => new AsycudaPackValidation(this);

	public new AsycudaPackValidation Validation => (AsycudaPackValidation)base.Validation;

	public new AsycudaPackedItem PackedItem => (AsycudaPackedItem)base.PackedItem;

	protected override Type GetPackedItemTypeCore() => typeof(AsycudaPackedItem);

	public new AsycudaPackLookups Lookups => (AsycudaPackLookups)base.Lookups;

	protected override ManifestBase.AsycudaPackLookups GetNewLookups() => new AsycudaPackLookups(this);
}
