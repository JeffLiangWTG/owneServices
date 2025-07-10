using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class WarehousePickableDocketWrapperCollection : WarehouseJobGenericWrapperCollection
	{
		public WarehousePickableDocketWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public WarehousePickableDocketWrapperCollection(WhsLegacyPickableDocketCollection collectionToWrap, BusinessObjectFactory factory)
			: base(collectionToWrap, factory)
		{
		}
	}
}
