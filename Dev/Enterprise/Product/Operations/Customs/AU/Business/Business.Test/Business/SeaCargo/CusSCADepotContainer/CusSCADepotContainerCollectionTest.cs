using CargoWise.EntityFramework;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSCADepotContainerCollectionTest : SeaCargoDepotTestCase
	{
		public void TestIndexer()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			CusSCADepotContainerCollection containerCollection = new CusSCADepotContainerCollection(container, Factory);
			CusSCADepotContainer depotContainer = Factory.New<CusSCADepotContainer>();
			containerCollection.Add(depotContainer);
			AssertEquals(depotContainer, containerCollection[0]);
		}

		public void TestTypedAddNew()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			CusSCADepotContainerCollection containerCollection = new CusSCADepotContainerCollection(container, Factory);
			BusinessObject depotContainer = containerCollection.AddNew();
			Assert(depotContainer.GetType() == typeof(CusSCADepotContainer));
		}
	}
}
