using System;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestedType(typeof(OverseasAgentCollectionProvider))]
	sealed class OverseasAgentCollectionProviderTest : CollectionProviderBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(OrgHeaderCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.Organisation;
	}
}
