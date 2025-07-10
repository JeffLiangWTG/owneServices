using System;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestedType(typeof(ShippingProviderCollectionProvider))]
	sealed class ShippingProviderCollectionProviderTest : CollectionProviderBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(ShippingProviderCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.Organisation;
	}
}
