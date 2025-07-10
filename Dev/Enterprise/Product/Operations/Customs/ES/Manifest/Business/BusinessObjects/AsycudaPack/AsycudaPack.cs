using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.ES.Manifest.Business
{
	public class AsycudaPack : ASYCUDA.Business.AsycudaPack
	{
		public AsycudaPack(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new AsycudaContainerBillOrPackageLink Pivot => (AsycudaContainerBillOrPackageLink)base.Pivot;

		public new AsycudaBill Bill => (AsycudaBill)base.Bill;

		public new AsycudaContainer Container => (AsycudaContainer)base.Container;

		public new AsycudaPackPackedItemPivotCollection PackedItems => base.PackedItems;

		public new AsycudaPackLookups Lookups => (AsycudaPackLookups)base.Lookups;

		protected override ManifestBase.AsycudaPackPackedItemPivotCollection CreateNewAsycudaPackCollection() => new AsycudaPackPackedItemPivotCollection(this);

		protected override Type GetPackedItemTypeCore() => typeof(AsycudaPackedItem);

		protected override ManifestBase.AsycudaPackLookups GetNewLookups() => new AsycudaPackLookups(this);
	}
}
