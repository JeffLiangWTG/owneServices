using System;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestedType(typeof(GlbGroupCollectionProvider))]
	sealed class GlbGroupCollectionProviderTest : CollectionProviderBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(GlbGroupCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.GlbGroup;
	}
}
