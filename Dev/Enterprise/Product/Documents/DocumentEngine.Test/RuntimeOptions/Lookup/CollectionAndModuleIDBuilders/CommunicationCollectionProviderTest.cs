using System;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestedType(typeof(CommunicationCollectionProvider))]
	sealed class CommunicationCollectionProviderTest : CollectionProviderBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(OrgSalesCallCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.Communication;
	}
}
