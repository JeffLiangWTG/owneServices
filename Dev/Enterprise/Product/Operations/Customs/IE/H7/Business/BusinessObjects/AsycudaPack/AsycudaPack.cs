using System;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.H7.Business
{
	[DependentBusinessObject(typeof(AsycudaBill), nameof(AsycudaBill.Packs))]
	public class AsycudaPack : EU.H7.Business.AsycudaPack
	{
		public AsycudaPack(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new AsycudaBill Bill => (AsycudaBill)base.Bill;

		protected override Type GetPackedItemTypeCore() => typeof(AsycudaPackedItem);

		public new AsycudaPackedItem PackedItem => (AsycudaPackedItem)base.PackedItem;

		protected override ManifestBase.AsycudaPackValidation GetNewValidation() => new AsycudaPackValidation(this);
	}
}
