using System;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestedType(typeof(OrgPartCategoryCollectionProvider))]
	sealed class OrgPartCategoryCollectionProviderTest : CollectionProviderBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(OrgPartCategoryCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.RefOrgPartCategory;
	}
}
