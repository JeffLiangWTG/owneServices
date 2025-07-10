using System;
using CargoWise.Application;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(WhsInventoryHeldCodeCollectionProvider))]
	sealed class WhsInventoryHeldCodeCollectionProviderTest : CollectionProviderBaseTest
	{
		protected override Type ExpectedCollectionType => ObjectFactory.GetType<IWhsInventoryHeldCodeCollection>();

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.WhsInventoryHeldCodes;
	}
}
