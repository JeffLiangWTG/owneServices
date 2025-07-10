using System;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestedType(typeof(ReceivingAgentCollectionProvider))]
	sealed class ReceivingAgentCollectionProviderTest : CollectionProviderBaseTest
	{
		public void TestValidationAndDefaultAdded()
		{
			CollectionProviderTest.AssertValidationAndDefaultAdded(Provider, Env.Security.ReportsExternalAgentsFilter, typeof(AtLeastOneAgentFilterContainsOrgProxyValidator), GlbCompany.CurrentCompany.OrgProxy.PK);
		}

		protected override Type ExpectedCollectionType => typeof(ForwarderCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.Organisation;
	}
}
