using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestedType(typeof(WhsPickingAreaCollectionProvider))]
	sealed class WhsPickingAreaCollectionProviderTest : WhsAreaCollectionProviderBaseTest
	{
		public override void TestCreateCollectionType()
		{
			var whs = (BusinessObject)Factory.New<IWhsWarehouse>();
			var bothArea = CreateArea(whs, true, true);
			var putawayArea = CreateArea(whs, false, true);
			var pickingArea = CreateArea(whs, true, false);
			Provider.SetFilterCollection(new ZQuery(WhsWarehouseSchema.PK, whs.PK));
			AssertEquals(ObjectFactory.GetType<IWhsAreaCollection>(), Provider.Collection.GetType());
			AssertCollectionContains(bothArea, Provider.CollectionForFindbox);
			AssertCollectionNotContains(putawayArea, Provider.CollectionForFindbox);
			AssertCollectionContains(pickingArea, Provider.CollectionForFindbox);
		}
	}
}
