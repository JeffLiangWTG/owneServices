using System;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestedType(typeof(GlbBranchCollectionProvider))]
	class GlbBranchCollectionProviderTest : CollectionProviderWithCodeSupportBaseTest
	{
		public void TestValidationAndDefaultAdded()
		{
			CollectionProviderTest.AssertValidationAndDefaultAdded(Provider, Env.Security.ReportsExternalBranchFilter, typeof(RequiredBranchFilterValidator), GlbBranch.CurrentBranch.PK);
		}

		protected override Type ExpectedCollectionType => typeof(GlbBranchCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.GlbBranch;

		protected override int ExpectedMaxLength => GlbBranchSchema.GB_Code.MaxLength;
	}
}
