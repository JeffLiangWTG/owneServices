using CargoWise.EntityFramework;
using Enterprise.Freight.CFS.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSCADepotHouseCollectionTest : SeaCargoDepotTestCase
	{
		#region Typed Methods

		public void TestIndexer()
		{
			CusSCADepotContainer depotContainer = Factory.New<CusSCADepotContainer>();
			CusSCADepotHouseCollection houseBillCollection = new CusSCADepotHouseCollection(depotContainer, Factory);
			CusSCADepotHouse houseBill = Factory.New<CusSCADepotHouse>();
			houseBillCollection.Add(houseBill);
			AssertEquals(houseBill, houseBillCollection[0]);
		}

		public void TestTypedAddNew()
		{
			PackUnpackShipment shipment = Factory.New<PackUnpackShipment>();
			CusSCADepotHouseCollection houseBillCollection = new CusSCADepotHouseCollection(shipment, Factory);
			BusinessObject houseBill = houseBillCollection.AddNew();
			Assert(houseBill.GetType() == typeof(CusSCADepotHouse));
		}

		#endregion

		#region MessageType

		public void TestMessageType()
		{
			CFSShipment shipment = Factory.New<CFSShipment>();
			CusSCADepotHouseCollection houseBillCollection = new CusSCADepotHouseCollection(shipment, Factory);
			CusSCADepotHouse house1 = CreateCusSCADepotHouse(SeaCargoMessageTypes.ImpendingArrival, shipment.PK);
			CusSCADepotHouse house2 = CreateCusSCADepotHouse(SeaCargoMessageTypes.ImpendingArrival, true);
			CusSCADepotHouse house3 = CreateCusSCADepotHouse(SeaCargoMessageTypes.ImpendingArrival, false);
			CusSCADepotHouse house4 = CreateCusSCADepotHouse(SeaCargoMessageTypes.CargoStatusAdvice, false);
			CusSCADepotHouse house5 = CreateCusSCADepotHouse(SeaCargoMessageTypes.CargoStatusAdvice, true);
			CusSCADepotHouse house6 = CreateCusSCADepotHouse(SeaCargoMessageTypes.CargoStatusAdvice, shipment.PK);
			CusSCADepotHouse house7 = CreateCusSCADepotHouse(SeaCargoMessageTypes.DeliveryReport, true);
			CusSCADepotHouse house8 = CreateCusSCADepotHouse(SeaCargoMessageTypes.DeliveryReport, false);
			CusSCADepotHouse house9 = CreateCusSCADepotHouse(SeaCargoMessageTypes.CargoArrival, true);
			CusSCADepotHouse house10 = CreateCusSCADepotHouse(SeaCargoMessageTypes.CargoArrival, false);
			CusSCADepotHouse house11 = CreateCusSCADepotHouse(SeaCargoMessageTypes.UnpackReport, true);
			CusSCADepotHouse house12 = CreateCusSCADepotHouse(SeaCargoMessageTypes.UnpackReport, false);

			Factory.Save();
			houseBillCollection.Load();

			houseBillCollection.MessageType = SeaCargoMessageTypes.ImpendingArrival;
			houseBillCollection.Load();
			Assert("List should have House 1", houseBillCollection.Contains(house1.PK));
			Assert("List should not have House 2", !houseBillCollection.Contains(house2.PK));
			Assert("List should not have House 3", !houseBillCollection.Contains(house3.PK));
			Assert("List should not have House 4", !houseBillCollection.Contains(house4.PK));
			Assert("List should not have House 5", !houseBillCollection.Contains(house5.PK));
			Assert("List should not have House 6", !houseBillCollection.Contains(house6.PK));
			Assert("List should not have House 7", !houseBillCollection.Contains(house7.PK));
			Assert("List should not have House 8", !houseBillCollection.Contains(house8.PK));
			Assert("List should not have House 9", !houseBillCollection.Contains(house9.PK));
			Assert("List should not have House 10", !houseBillCollection.Contains(house10.PK));
			Assert("List should not have House 11", !houseBillCollection.Contains(house11.PK));
			Assert("List should not have House 12", !houseBillCollection.Contains(house12.PK));

			houseBillCollection.MessageType = SeaCargoMessageTypes.CargoStatusAdvice;
			houseBillCollection.Load();
			Assert("List should container House 6", houseBillCollection.Contains(house6.PK));
			Assert("List should not have House 1", !houseBillCollection.Contains(house1.PK));
			Assert("List should not have House 2", !houseBillCollection.Contains(house2.PK));
			Assert("List should not have House 3", !houseBillCollection.Contains(house3.PK));
			Assert("List should not have House 4", !houseBillCollection.Contains(house4.PK));
			Assert("List should not have House 5", !houseBillCollection.Contains(house5.PK));
			Assert("List should not have House 7", !houseBillCollection.Contains(house7.PK));
			Assert("List should not have House 8", !houseBillCollection.Contains(house8.PK));
			Assert("List should not have House 9", !houseBillCollection.Contains(house9.PK));
			Assert("List should not have House 10", !houseBillCollection.Contains(house10.PK));
			Assert("List should not have House 11", !houseBillCollection.Contains(house11.PK));
			Assert("List should not have House 11", !houseBillCollection.Contains(house11.PK));
			Assert("List should not have House 12", !houseBillCollection.Contains(house12.PK));
		}

		#endregion

		#region Find

		public void TestFind()
		{
			CusSCADepotContainer container = Factory.New<CusSCADepotContainer>();
			CusSCADepotHouse house1 = container.HouseBills.AddNew();
			house1.CX_HouseBill = "HOUSE1";
			CusSCADepotHouse house2 = container.HouseBills.AddNew();
			house2.CX_HouseBill = "HOUSE2";

			AssertEquals("House1 found when searching for HOUSE1", house1, container.HouseBills.Find("HOUSE1"));
			AssertEquals("House2 found when searching for HOUSE2", house2, container.HouseBills.Find("HOUSE2"));
			AssertEquals("null found when searching for HOUSE3 which doesn't exist", null, container.HouseBills.Find("HOUSE3"));
		}

		#endregion
	}
}
