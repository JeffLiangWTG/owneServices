using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class WarehouseOrderWrapperForManifestCollection : WarehousePickableDocketWrapperCollection
	{
		public WarehouseOrderWrapperForManifestCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public WarehouseOrderWrapperForManifestCollection(WhsLegacyPickableDocketCollection collectionToWrap, BusinessObjectFactory factory)
			: base(factory)
		{
			AddOrderListFrom(collectionToWrap);
		}

		void AddOrderListFrom(WhsLegacyPickableDocketCollection collectionToWrap)
		{
			foreach (WhsOrder order in collectionToWrap)
			{
				Add(new WarehouseOrderWrapperForManifest(order, Factory));
			}
		}
	}
}
