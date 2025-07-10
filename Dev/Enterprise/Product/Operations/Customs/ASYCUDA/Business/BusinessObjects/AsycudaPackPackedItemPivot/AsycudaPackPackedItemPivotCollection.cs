namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaPackPackedItemPivotCollection : ManifestBase.AsycudaPackPackedItemPivotCollection<AsycudaPackedItem, AsycudaPack>
	{
		public AsycudaPackPackedItemPivotCollection(AsycudaPackedItem packedItem)
			: base(packedItem)
		{
		}

		public AsycudaPackPackedItemPivotCollection(AsycudaPack pack)
			: base(pack)
		{
		}

		public new AsycudaPackedItem AddNewPackedItem() => (AsycudaPackedItem)base.AddNewPackedItem();
	}
}
