using System;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestedType(typeof(CommissionAgreementCollectionProvider))]
	sealed class CommissionAgreementCollectionProviderTest : CollectionProviderBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(OrgCommissionAgreementCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.OrgCommissionAgreement;
	}
}
