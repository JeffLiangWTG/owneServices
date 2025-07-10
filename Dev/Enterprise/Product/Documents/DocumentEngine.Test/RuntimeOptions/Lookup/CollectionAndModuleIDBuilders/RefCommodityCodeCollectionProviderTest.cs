using System;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestedType(typeof(RefCommodityCodeCollectionProvider))]
	sealed class RefCommodityCodeCollectionProviderTest : CollectionProviderBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(RefCommodityCodeCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.RefCommodityCode;
	}
}
