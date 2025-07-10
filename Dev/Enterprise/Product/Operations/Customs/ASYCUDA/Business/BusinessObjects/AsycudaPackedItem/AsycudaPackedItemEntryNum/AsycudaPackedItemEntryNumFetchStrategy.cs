using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaPackedItemEntryNumFetchStrategy : AsycudaFetchStrategy
	{
		public AsycudaPackedItemEntryNumFetchStrategy(AsycudaPackedItemEntryNum packedItemEntryNum)
			: base(packedItemEntryNum)
		{
		}

		new protected AsycudaPackedItemEntryNum BusinessObject => (AsycudaPackedItemEntryNum)base.BusinessObject;
	}
}
