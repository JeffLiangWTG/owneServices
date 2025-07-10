using CargoWise.EntityFramework;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSCADepotContainerListTest : SeaCargoDepotTestCase
	{
		public void TestIndexer()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			CusSCADepotContainerList containerList = new CusSCADepotContainerList(Factory, "");
			CusSCADepotContainer depotContainer = Factory.New<CusSCADepotContainer>();
			containerList.Add(depotContainer);
			AssertEquals(depotContainer, containerList[0]);
		}

		public void TestTypedAddNew()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			CusSCADepotContainerList containerList = new CusSCADepotContainerList(Factory, "");
			BusinessObject depotContainer = containerList.AddNew();
			Assert(depotContainer.GetType() == typeof(CusSCADepotContainer));
		}

		public void TestMessageType()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			CusSCADepotContainerList containerList = new CusSCADepotContainerList(Factory, "");

			CusSCADepotContainer container1 = CreateCusSCADepotContainer(SeaCargoMessageTypes.ImpendingArrival, container.PK);
			CusSCADepotContainer container2 = CreateCusSCADepotContainer(SeaCargoMessageTypes.ImpendingArrival, true);
			CusSCADepotContainer container3 = CreateCusSCADepotContainer(SeaCargoMessageTypes.ImpendingArrival, false);
			CusSCADepotContainer container4 = CreateCusSCADepotContainer(SeaCargoMessageTypes.CargoStatusAdvice, container.PK);
			CusSCADepotContainer container5 = CreateCusSCADepotContainer(SeaCargoMessageTypes.CargoStatusAdvice, true);
			CusSCADepotContainer container6 = CreateCusSCADepotContainer(SeaCargoMessageTypes.CargoStatusAdvice, false);
			CusSCADepotContainer container7 = CreateCusSCADepotContainer(SeaCargoMessageTypes.DeliveryReport, true);
			CusSCADepotContainer container8 = CreateCusSCADepotContainer(SeaCargoMessageTypes.DeliveryReport, false);
			CusSCADepotContainer container9 = CreateCusSCADepotContainer(SeaCargoMessageTypes.CargoArrival, true);
			CusSCADepotContainer container10 = CreateCusSCADepotContainer(SeaCargoMessageTypes.CargoArrival, false);
			CusSCADepotContainer container11 = CreateCusSCADepotContainer(SeaCargoMessageTypes.UnpackReport, true);
			CusSCADepotContainer container12 = CreateCusSCADepotContainer(SeaCargoMessageTypes.UnpackReport, false);

			Factory.Save();
			containerList.Load(SimulateAFindBoxQueryForCusSCAContainer);

			int currentCount = containerList.Count;
			Assert("Full Count should be > 10", containerList.Count >= 5);
			containerList.MessageType = SeaCargoMessageTypes.ImpendingArrival;
			containerList.Load(SimulateAFindBoxQueryForCusSCAContainer);

			Assert("List should have Container 3", containerList.Contains(container3.PK));
			Assert("List should not have Container 1", !containerList.Contains(container1.PK));
			Assert("List should not have Container 2", !containerList.Contains(container2.PK));
			Assert("List should not have Container 4", !containerList.Contains(container4.PK));
			Assert("List should not have Container 5", !containerList.Contains(container5.PK));
			Assert("List should not have Container 6", !containerList.Contains(container6.PK));
			Assert("List should not have Container 7", !containerList.Contains(container7.PK));
			Assert("List should not have Container 8", !containerList.Contains(container8.PK));
			Assert("List should not have Container 9", !containerList.Contains(container9.PK));
			Assert("List should not have Container 10", !containerList.Contains(container10.PK));
			Assert("List should not have Container 11", !containerList.Contains(container11.PK));
			Assert("List should not have Container 12", !containerList.Contains(container12.PK));

			containerList.MessageType = SeaCargoMessageTypes.CargoStatusAdvice;
			containerList.Load(SimulateAFindBoxQueryForCusSCAContainer);
			Assert("List should have Container 6", containerList.Contains(container6.PK));
			Assert("List should not have Container 1", !containerList.Contains(container1.PK));
			Assert("List should not have Container 2", !containerList.Contains(container2.PK));
			Assert("List should not have Container 3", !containerList.Contains(container3.PK));
			Assert("List should not have Container 4", !containerList.Contains(container4.PK));
			Assert("List should not have Container 5", !containerList.Contains(container5.PK));
			Assert("List should not have Container 7", !containerList.Contains(container7.PK));
			Assert("List should not have Container 8", !containerList.Contains(container8.PK));
			Assert("List should not have Container 9", !containerList.Contains(container9.PK));
			Assert("List should not have Container 10", !containerList.Contains(container10.PK));
			Assert("List should not have Container 11", !containerList.Contains(container11.PK));
			Assert("List should not have Container 12", !containerList.Contains(container12.PK));
		}
	}
}
