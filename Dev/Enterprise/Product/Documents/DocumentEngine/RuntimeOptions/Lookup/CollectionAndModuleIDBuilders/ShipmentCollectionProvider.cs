using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class ShipmentCollectionProvider : CollectionProvider
	{
		public ShipmentCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			return ObjectFactory.Get<IShipmentCollection>("IShipmentCollection", BusinessObjectFactory);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.JobShipment;
	}
}
