using System;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestedType(typeof(GlbCompanyCollectionProvider))]
	sealed class GlbCompanyCollectionProviderTest : CollectionProviderBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(GlbCompanyCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.GlbCompany;
	}
}
