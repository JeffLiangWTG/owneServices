using CargoWise.EntityFramework;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public interface IAsycudaTaxCollection<out B, out MasterB> : ManifestBase.IAsycudaTaxCollection<B, MasterB>
		where B : AsycudaTax
		where MasterB : AsycudaBill
	{
		new B this[int i] { get; }
	}

	public class AsycudaTaxCollection<B, MasterB> : ManifestBase.AsycudaTaxCollection<B, MasterB>, IAsycudaTaxCollection<B, MasterB>
		where B : AsycudaTax
		where MasterB : AsycudaBill
	{
		public AsycudaTaxCollection(MasterB master)
			: base(master)
		{ }

		#region Fetch Strategy
		protected override IBusinessObjectCollectionFetchStrategy GetFetchStrategy()
		{
			return new CollectionFetchStrategy(this);
		}

		class CollectionFetchStrategy : AsycudaCollectionFetchStrategy<IAsycudaTaxCollection<B, MasterB>>
		{
			public CollectionFetchStrategy(IAsycudaTaxCollection<B, MasterB> collection)
				: base(collection)
			{
			}
		}
		#endregion
	}
}
