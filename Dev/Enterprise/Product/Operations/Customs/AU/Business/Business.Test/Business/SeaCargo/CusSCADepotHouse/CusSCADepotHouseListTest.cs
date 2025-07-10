using CargoWise.EntityFramework;
using Enterprise.Freight.CFS.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSCADepotHouseListTest : SeaCargoDepotTestCase
	{
		public void TestIndexer()
		{
			CusSCADepotContainer depotContainer = Factory.New<CusSCADepotContainer>();
			CusSCADepotHouseList houseBillCollection = new CusSCADepotHouseList(Factory);
			CusSCADepotHouse houseBill = Factory.New<CusSCADepotHouse>();
			houseBillCollection.Add(houseBill);
			AssertEquals(houseBill, houseBillCollection[0]);
		}

		public void TestTypedAddNew()
		{
			CFSShipment shipment = Factory.New<CFSShipment>();
			CusSCADepotHouseList houseBillCollection = new CusSCADepotHouseList(Factory);
			BusinessObject houseBill = houseBillCollection.AddNew();
			Assert(houseBill.GetType() == typeof(CusSCADepotHouse));
		}

		public void TestMessgeType()
		{
			CusSCADepotHouseList houseBillList = new CusSCADepotHouseList(Factory);
			CusSCADepotHouse house1 = CreateCusSCADepotHouse(SeaCargoMessageTypes.ImpendingArrival, true);
			CusSCADepotHouse house2 = CreateCusSCADepotHouse(SeaCargoMessageTypes.ImpendingArrival, false);
			CusSCADepotHouse house3 = CreateCusSCADepotHouse(SeaCargoMessageTypes.CargoStatusAdvice, false);
			CusSCADepotHouse house4 = CreateCusSCADepotHouse(SeaCargoMessageTypes.CargoStatusAdvice, true);
			CusSCADepotHouse house5 = CreateCusSCADepotHouse(SeaCargoMessageTypes.DeliveryReport, true);
			CusSCADepotHouse house6 = CreateCusSCADepotHouse(SeaCargoMessageTypes.DeliveryReport, false);
			CusSCADepotHouse house7 = CreateCusSCADepotHouse(SeaCargoMessageTypes.CargoArrival, true);
			CusSCADepotHouse house8 = CreateCusSCADepotHouse(SeaCargoMessageTypes.CargoArrival, false);
			CusSCADepotHouse house9 = CreateCusSCADepotHouse(SeaCargoMessageTypes.UnpackReport, true);
			CusSCADepotHouse house10 = CreateCusSCADepotHouse(SeaCargoMessageTypes.UnpackReport, false);

			Factory.Save();
			houseBillList.Load(SimulateAFindBoxQueryForCusSCAHouse);

			int currentCount = houseBillList.Count;
			Assert("Full Count should be > 10", houseBillList.Count >= 5);
			houseBillList.MessageType = SeaCargoMessageTypes.ImpendingArrival;
			houseBillList.Load(SimulateAFindBoxQueryForCusSCAHouse);
			Assert("List should have House 2", houseBillList.Contains(house2.PK));
			Assert("List should not have House 1", !houseBillList.Contains(house1.PK));
			Assert("List should not have House 3", !houseBillList.Contains(house3.PK));
			Assert("List should not have House 4", !houseBillList.Contains(house4.PK));
			Assert("List should not have House 5", !houseBillList.Contains(house5.PK));
			Assert("List should not have House 6", !houseBillList.Contains(house6.PK));
			Assert("List should not have House 7", !houseBillList.Contains(house7.PK));
			Assert("List should not have House 8", !houseBillList.Contains(house8.PK));
			Assert("List should not have House 9", !houseBillList.Contains(house9.PK));
			Assert("List should not have House 10", !houseBillList.Contains(house10.PK));

			CusSCADepotHouse house11 = CreateCusSCADepotHouse(SeaCargoMessageTypes.ImpendingArrival, false);
			houseBillList.Load(SimulateAFindBoxQueryForCusSCAHouse);
			Assert("List should have house 11", houseBillList.Contains(house11.PK));

			houseBillList.MessageType = SeaCargoMessageTypes.CargoStatusAdvice;
			houseBillList.Load(SimulateAFindBoxQueryForCusSCAHouse);
			Assert("List should container House 3", houseBillList.Contains(house3.PK));
			Assert("List should not have House 1", !houseBillList.Contains(house1.PK));
			Assert("List should not have House 2", !houseBillList.Contains(house2.PK));
			Assert("List should not have House 4", !houseBillList.Contains(house4.PK));
			Assert("List should not have House 5", !houseBillList.Contains(house5.PK));
			Assert("List should not have House 6", !houseBillList.Contains(house6.PK));
			Assert("List should not have House 7", !houseBillList.Contains(house7.PK));
			Assert("List should not have House 8", !houseBillList.Contains(house8.PK));
			Assert("List should not have House 9", !houseBillList.Contains(house9.PK));
			Assert("List should not have House 10", !houseBillList.Contains(house10.PK));
			Assert("List should not have House 11", !houseBillList.Contains(house11.PK));
		}
	}
}
