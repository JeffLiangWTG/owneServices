using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Integration;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	class WhsPutawayAreaCollectionProvider : WhsAreaCollectionProvider
	{
		public WhsPutawayAreaCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override IWhsAreaCollection GetAreaCollection(IWhsWarehouse warehouse)
		{
			return ObjectFactory.Get<IWhsAreaCollectionBuilder>().GetPutawayAreas(BusinessObjectFactory, warehouse);
		}
	}
}
