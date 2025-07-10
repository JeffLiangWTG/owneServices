using System;
using CargoWise.Application;
using Enterprise.Warehouse.Integration;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(TransitWarehouseCollectionProvider))]
	sealed class TransitWarehouseCollectionProviderTest : WarehouseCollectionProviderBaseTest
	{
		protected override Type ExpectedCollectionType => ObjectFactory.GetType<IWhsWarehouseCollectionForTransitWarehouse>();
		protected override Type ExpectedCollectionTypeForWeb => ObjectFactory.GetType<IWhsWarehouseCollectionForTransitWarehouse>();
	}
}
