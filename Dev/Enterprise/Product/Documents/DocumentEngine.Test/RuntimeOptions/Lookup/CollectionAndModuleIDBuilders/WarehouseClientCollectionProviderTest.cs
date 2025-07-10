using System;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(WarehouseClientCollectionProvider))]
	sealed class WarehouseClientCollectionProviderTest : CollectionProviderBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(WarehouseClientCollectionWithSecurityCheck);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.Organisation;
	}
}
