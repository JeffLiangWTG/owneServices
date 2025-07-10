using System;
using CargoWise.Application;
using Enterprise.Warehouse.Integration;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(FTZWarehouseCollectionProvider))]
	sealed class FTZWarehouseCollectionProviderTest : WarehouseCollectionProviderBaseTest
	{
		protected override Type ExpectedCollectionTypeForWeb => ObjectFactory.GetType<IWhsWarehouseCollectionWithSecurityCheck>();
	}
}
