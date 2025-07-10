using System;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestedType(typeof(OrgCreditorGroupCollectionProvider))]
	sealed class OrgCreditorGroupCollectionProviderTest : CollectionProviderBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(OrgCreditorGroupCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.OrgCreditorGroup;
	}
}
