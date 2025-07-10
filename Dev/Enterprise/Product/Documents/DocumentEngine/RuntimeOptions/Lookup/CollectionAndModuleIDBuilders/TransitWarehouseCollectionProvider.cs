using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Integration;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	class TransitWarehouseCollectionProvider : WarehouseCollectionProvider
	{
		public TransitWarehouseCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			return ObjectFactory.Get<IWhsWarehouseCollectionForTransitWarehouse>("IWhsWarehouseCollectionForTransitWarehouse", BusinessObjectFactory);
		}
	}
}
