using Enterprise.Freight.CFS.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class SeaCargoDepotShipmentCollectionTest : SeaCargoTestCase
	{
		public void TestTypedAccessors()
		{
			PackUnpackShipment shipment = Factory.New<PackUnpackShipment>();
			SeaCargoDepotShipmentCollection collection = new SeaCargoDepotShipmentCollection(Factory);
			SeaCargoDepotShipment depotShipment = SeaCargoDepotShipment.Load(shipment);
			collection.Add(depotShipment);
			AssertEquals("Collection Return Type", depotShipment, collection[0]);
		}

		public void TestRemoveRelated()
		{
			SeaCargoDepotShipmentCollection collection = new SeaCargoDepotShipmentCollection(Factory);
			PackUnpackShipment shipment1 = Factory.New<PackUnpackShipment>();
			PackUnpackShipment shipment2 = Factory.New<PackUnpackShipment>();
			PackUnpackShipment shipment3 = Factory.New<PackUnpackShipment>();
			shipment1.JS_HouseBill = HouseBillNumber1;
			shipment2.JS_HouseBill = HouseBillNumber2;
			shipment3.JS_HouseBill = HouseBillNumber3;
			SeaCargoDepotShipment depotShipment1 = SeaCargoDepotShipment.Load(shipment1);
			SeaCargoDepotShipment depotShipment2 = SeaCargoDepotShipment.Load(shipment2);
			SeaCargoDepotShipment depotShipment3 = SeaCargoDepotShipment.Load(shipment3);
			collection.Add(depotShipment1);
			collection.Add(depotShipment2);
			collection.Add(depotShipment3);
			AssertEquals("Collection Count", 3, collection.Count);
			collection.RemoveRelated(shipment2);
			AssertEquals("Failed to remove by related container", 2, collection.Count);
			AssertEquals("Collections should still container container 1 and 3", true, collection.Contains(depotShipment1.PK) && collection.Contains(depotShipment3.PK));
			AssertEquals("Container 2 should have been removed", false, collection.Contains(depotShipment2.PK));
		}
	}
}
