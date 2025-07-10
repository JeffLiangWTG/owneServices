using System;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestedType(typeof(GlobalCreditGroupCollectionProvider))]
	sealed class GlobalCreditGroupCollectionProviderTest : CollectionProviderBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(GlobalCreditGroupCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.Organisation;
	}
}
