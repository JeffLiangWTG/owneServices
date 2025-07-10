using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	class WarehouseCollectionProvider : CollectionProvider
	{
		public WarehouseCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			return Globals.IsWeb ?
				ObjectFactory.Get<IWhsWarehouseCollectionWithSecurityCheckForWeb>("IWhsWarehouseCollectionWithSecurityCheckForWeb", BusinessObjectFactory) :
				ObjectFactory.Get<IWhsWarehouseCollectionWithSecurityCheck>("IWhsWarehouseCollectionWithSecurityCheck", BusinessObjectFactory);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.WhsConfigWarehouse;
	}
}
