using System;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestedType(typeof(TransportClientCollectionProvider))]
	sealed class TransportClientCollectionProviderTest : CollectionProviderBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(OrgHeaderCollection);

		protected override Type ExpectedCollectionForFindBoxType => typeof(TransportClientCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.Organisation;
	}
}
