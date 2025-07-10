using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	[DependentBusinessObject(typeof(AsycudaBill), nameof(AsycudaBill.PackedItems))]
	public class AsycudaPackedItem : EU.H7.Business.AsycudaPackedItem
	{
		public AsycudaPackedItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;

		public new AsycudaBill Bill => (AsycudaBill)base.Bill;

		public new AsycudaPack Pack => (AsycudaPack)base.Pack;

		public new AsycudaPackedItemValidation Validation => (AsycudaPackedItemValidation)base.Validation;

		protected override ManifestBase.AsycudaPackedItemValidation GetNewValidation() => new AsycudaPackedItemValidation(this);

		[MaxLength(13)]
		public override ZString API_FormattedTariff { get => base.API_FormattedTariff; set => base.API_FormattedTariff = value; }
	}
}
