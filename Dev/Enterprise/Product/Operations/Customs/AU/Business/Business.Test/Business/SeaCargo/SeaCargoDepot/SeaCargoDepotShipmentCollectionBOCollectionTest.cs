using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.CFS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(SeaCargoDepotShipmentCollection))]
	sealed class SeaCargoDepotShipmentCollectionBOCollectionTest : NonPersistentBusinessObjectCollectionTestCase<SeaCargoDepotShipmentCollection>
	{
		protected override SeaCargoDepotShipmentCollection GetCollectionToTest()
		{
			return new SeaCargoDepotShipmentCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			CFSShipment shipment = Factory.New<CFSShipment>();
			return SeaCargoDepotShipment.Load(shipment);
		}
	}
}
