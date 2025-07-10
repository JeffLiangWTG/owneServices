using System;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestedType(typeof(TransactionBranchCollectionProvider))]
	sealed class TransactionBranchCollectionProviderTest : CollectionProviderBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(GlbBranchCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.GlbBranch;
	}
}
