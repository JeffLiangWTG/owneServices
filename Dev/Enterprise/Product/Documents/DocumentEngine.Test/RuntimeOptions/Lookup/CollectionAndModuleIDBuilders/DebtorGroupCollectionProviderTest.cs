using System;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestedType(typeof(DebtorGroupCollectionProvider))]
	sealed class DebtorGroupCollectionProviderTest : CollectionProviderBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(OrgDebtorGroupCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.OrgDebtorGroup;
	}
}
