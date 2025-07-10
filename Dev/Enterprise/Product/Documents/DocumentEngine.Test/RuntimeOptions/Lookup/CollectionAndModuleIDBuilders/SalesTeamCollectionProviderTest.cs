using System;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestedType(typeof(SalesTeamCollectionProvider))]
	sealed class SalesTeamCollectionProviderTest : CollectionProviderWithCodeSupportBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(SalesTeamCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.SalesTeam;

		protected override int ExpectedMaxLength => GlbGroupSchema.GG_Code.MaxLength;
	}
}
