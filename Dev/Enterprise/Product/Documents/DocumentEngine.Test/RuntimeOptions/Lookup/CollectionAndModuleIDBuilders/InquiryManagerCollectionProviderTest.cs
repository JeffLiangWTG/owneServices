using System;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestedType(typeof(InquiryManagerCollectionProvider))]
	sealed class InquiryManagerCollectionProviderTest : CollectionProviderBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(SalesEnquiryCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.SalesEnquiry;
	}
}
