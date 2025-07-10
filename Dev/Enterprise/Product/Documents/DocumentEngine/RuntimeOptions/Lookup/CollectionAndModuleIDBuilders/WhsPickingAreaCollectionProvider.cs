using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Integration;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	class WhsPickingAreaCollectionProvider : WhsAreaCollectionProvider
	{
		public WhsPickingAreaCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override IWhsAreaCollection GetAreaCollection(IWhsWarehouse warehouse)
		{
			return ObjectFactory.Get<IWhsAreaCollectionBuilder>().GetPickingAreas(BusinessObjectFactory, warehouse);
		}
	}
}
