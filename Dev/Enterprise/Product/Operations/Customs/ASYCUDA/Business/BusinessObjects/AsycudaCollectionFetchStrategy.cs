using CargoWise.EntityFramework;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public abstract class AsycudaCollectionFetchStrategy<T> : BusinessObjectCollectionFetchStrategy
		where T : IBusinessObjectCollection
	{
		protected AsycudaCollectionFetchStrategy(T collection)
				: base(collection)
		{
		}

		protected new T Collection
		{
			get { return (T)base.Collection; }
		}

		protected sealed override void FetchForViewCore(BusinessObject[] businessObjects, TableColumn[] columns)
		{
			FetchForViewAsycuda(businessObjects, columns);
		}

		protected virtual void FetchForViewAsycuda(BusinessObject[] businessObjects, TableColumn[] columns)
		{
		}
	}
}
