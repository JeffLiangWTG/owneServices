using System;
using CargoWise.Application;
using Enterprise.DocumentEngine.Testing;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestsSubclassesOf(typeof(WarehouseCollectionProvider), typeof(TestExcludeCollectionProviderAllHaveTestCase))]
	abstract class WarehouseCollectionProviderBaseTest : CollectionProviderBaseTest
	{
		public void TestCreateCollectionForWeb()
		{
			Globals.IsWeb = true;
			AssertEquals(ExpectedCollectionTypeForWeb, Provider.Collection.GetType());
		}

		public void TestGetCollectionForFindboxForWeb()
		{
			Globals.IsWeb = true;
			AssertEquals(ExpectedCollectionTypeForWeb, Provider.CollectionForFindbox.GetType());
		}

		protected override Type ExpectedCollectionType => ObjectFactory.GetType<IWhsWarehouseCollectionWithSecurityCheck>();

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.WhsConfigWarehouse;

		protected virtual Type ExpectedCollectionTypeForWeb => ObjectFactory.GetType<IWhsWarehouseCollectionWithSecurityCheckForWeb>();
	}
}
