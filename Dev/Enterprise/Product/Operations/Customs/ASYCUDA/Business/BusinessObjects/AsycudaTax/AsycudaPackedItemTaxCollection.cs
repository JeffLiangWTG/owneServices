using CargoWise.EntityFramework;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public interface IAsycudaPackedItemTaxCollection<out B, out MasterB> : ManifestBase.IAsycudaPackedItemTaxCollection<B, MasterB>
		where B : AsycudaTax
		where MasterB : AsycudaPackedItem
	{
	}

	public class AsycudaPackedItemTaxCollection<B, MasterB> : ManifestBase.AsycudaPackedItemTaxCollection<B, MasterB>, IAsycudaPackedItemTaxCollection<B, MasterB>
		where B : AsycudaTax
		where MasterB : AsycudaPackedItem
	{
		public AsycudaPackedItemTaxCollection(MasterB master)
			: base(master)
		{ }

		#region Fetch Strategy
		protected override IBusinessObjectCollectionFetchStrategy GetFetchStrategy()
		{
			return new CollectionFetchStrategy(this);
		}

		class CollectionFetchStrategy : AsycudaCollectionFetchStrategy<IAsycudaPackedItemTaxCollection<B, MasterB>>
		{
			public CollectionFetchStrategy(IAsycudaPackedItemTaxCollection<B, MasterB> collection)
				: base(collection)
			{
			}
		}
		#endregion
	}
}
