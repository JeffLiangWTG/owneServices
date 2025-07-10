using System;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestedType(typeof(OpportunityManagerCollectionProvider))]
	sealed class OpportunityManagerCollectionProviderTest : CollectionProviderBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(OrgOpportunityCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.Opportunity;
	}
}
