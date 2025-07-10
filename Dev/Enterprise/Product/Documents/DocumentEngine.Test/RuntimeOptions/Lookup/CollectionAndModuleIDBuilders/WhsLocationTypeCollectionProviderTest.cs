using System;
using CargoWise.Application;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(WhsLocationTypeCollectionProvider))]
	sealed class WhsLocationTypeCollectionProviderTest : CollectionProviderBaseTest
	{
		protected override Type ExpectedCollectionType => ObjectFactory.GetType<IWhsLocationTypeCollection>();

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.WhsConfigLocationType;
	}
}
