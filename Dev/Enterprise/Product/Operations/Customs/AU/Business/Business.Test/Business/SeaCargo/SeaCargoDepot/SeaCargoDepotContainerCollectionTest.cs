using Enterprise.Freight.CFS.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class SeaCargoDepotContainerCollectionTest : SeaCargoTestCase
	{
		public void TestTypedAccessors()
		{
			CFSContainer container = Factory.New<CFSContainer>();
			SeaCargoDepotContainerCollection collection = new SeaCargoDepotContainerCollection(Factory);
			SeaCargoDepotContainer depotContainer = SeaCargoDepotContainer.Load(container);
			collection.Add(depotContainer);
			AssertEquals("Collection Return Type", depotContainer, collection[0]);
		}

		public void TestRemoveRelated()
		{
			SeaCargoDepotContainerCollection collection = new SeaCargoDepotContainerCollection(Factory);
			CFSContainer container1 = Factory.New<CFSContainer>();
			CFSContainer container2 = Factory.New<CFSContainer>();
			CFSContainer container3 = Factory.New<CFSContainer>();
			container1.JC_ContainerNum = ContainerNumber1;
			container2.JC_ContainerNum = ContainerNumber2;
			container3.JC_ContainerNum = ContainerNumber3;
			SeaCargoDepotContainer depotContainer1 = SeaCargoDepotContainer.Load(container1);
			SeaCargoDepotContainer depotContainer2 = SeaCargoDepotContainer.Load(container2);
			SeaCargoDepotContainer depotContainer3 = SeaCargoDepotContainer.Load(container3);
			collection.Add(depotContainer1);
			collection.Add(depotContainer2);
			collection.Add(depotContainer3);

			AssertEquals("Collection Count", 3, collection.Count);
			collection.RemoveRelated(container2);
			AssertEquals("Failed to remove by related container", 2, collection.Count);
			AssertEquals("Collections should still container container 1 and 3", true, collection.Contains(depotContainer1.PK) && collection.Contains(depotContainer3.PK));
			AssertEquals("Container 2 should have been removed", false, collection.Contains(depotContainer2.PK));
		}
	}
}
