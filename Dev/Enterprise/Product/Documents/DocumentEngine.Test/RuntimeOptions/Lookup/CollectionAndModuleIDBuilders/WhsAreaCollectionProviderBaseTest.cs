using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.DocumentEngine.Testing;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestsSubclassesOf(typeof(WhsAreaCollectionProvider), typeof(TestExcludeCollectionProviderAllHaveTestCase))]
	abstract class WhsAreaCollectionProviderBaseTest : CollectionProviderBaseTest
	{
		#region TestCreateCollectionType

		public virtual void TestCreateCollectionType()
		{
			var whs = (BusinessObject)Factory.New<IWhsWarehouse>();
			var bothArea = CreateArea(whs, true, true);
			var putawayArea = CreateArea(whs, false, true);
			var pickingArea = CreateArea(whs, true, false);
			Provider.SetFilterCollection(new ZQuery(WhsWarehouseSchema.PK, whs.PK));
			AssertEquals(ObjectFactory.GetType<IWhsAreaCollection>(), Provider.Collection.GetType());
			AssertCollectionContains(bothArea, Provider.CollectionForFindbox);
			AssertCollectionContains(putawayArea, Provider.CollectionForFindbox);
			AssertCollectionContains(pickingArea, Provider.CollectionForFindbox);
		}

		protected BusinessObject CreateArea(BusinessObject whs, bool isPicking, bool isPutaway)
		{
			var area = (BusinessObject)Factory.New<IWhsArea>();
			area[WhsAreaSchema.WA_WW_Whs] = whs.PK;
			area[WhsAreaSchema.WA_IsPickingArea] = isPicking;
			area[WhsAreaSchema.WA_IsPutawayArea] = isPutaway;
			return area;
		}

		#endregion

		#region TestGetCollectionForFindbox

		public void TestCollectionForFindbox()
		{
			var whs = (BusinessObject)Factory.New<IWhsWarehouse>();
			Provider.SetFilterCollection(new ZQuery(WhsWarehouseSchema.PK, whs.PK));
			AssertEquals(ObjectFactory.GetType<IWhsAreaCollection>(), Provider.CollectionForFindbox.GetType());
		}

		#endregion

		#region TestGetCollectionForFindboxDoesNotThrowAmbiguousMatchException

		[ExpectNoExceptions]
		public void TestGetCollectionForFindboxDoesNotThrowAmbiguousMatchException()
		{
			AssertEquals(ObjectFactory.GetType<IWhsAreaCollection>(), Provider.CollectionForFindbox.GetType());

			Provider.SetFilterCollection(new ZQuery(WhsWarehouseSchema.PK, Guid.NewGuid()));
			AssertEquals(ObjectFactory.GetType<IWhsAreaCollection>(), Provider.CollectionForFindbox.GetType());
		}

		#endregion

		protected override Type ExpectedCollectionType => ObjectFactory.GetType<IWhsAreaCollection>();

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.WhsConfigArea;
	}
}
