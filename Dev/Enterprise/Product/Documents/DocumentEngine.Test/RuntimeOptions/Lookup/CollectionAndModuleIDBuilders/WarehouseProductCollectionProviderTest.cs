using System;
using CargoWise.Application;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestedType(typeof(WarehouseProductCollectionProvider))]
	sealed class WarehouseProductCollectionProviderTest : OrgSupplierPartCollectionProviderBaseTest
	{
		protected override Type ExpectedCollectionType => ObjectFactory.GetType<IWhsOrgSupplierPartCollection>();

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.WhsConfigProduct;
	}
}
