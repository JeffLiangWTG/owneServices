using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Integration;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	class FTZWarehouseCollectionProvider : WarehouseCollectionProvider
	{
		public FTZWarehouseCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			return ObjectFactory.Get<IWhsWarehouseCollectionWithSecurityCheck>("IWhsWarehouseCollectionWithSecurityCheck", BusinessObjectFactory, WarehouseCollectionType.FTZWarehouse);
		}
	}
}
