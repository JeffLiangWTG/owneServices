using System;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.GB.H7.Business
{
	[DependentBusinessObject(typeof(AsycudaBill), nameof(AsycudaBill.Packs))]
	public class AsycudaPack : EU.H7.Business.AsycudaPack
	{
		public AsycudaPack(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new AsycudaPackValidation Validation => (AsycudaPackValidation)base.Validation;

		public new AsycudaPackedItem PackedItem => (AsycudaPackedItem)base.PackedItem;

		protected override ManifestBase.AsycudaPackValidation GetNewValidation() => new AsycudaPackValidation(this);

		protected override Type GetPackedItemTypeCore() => typeof(AsycudaPackedItem);
	}
}
