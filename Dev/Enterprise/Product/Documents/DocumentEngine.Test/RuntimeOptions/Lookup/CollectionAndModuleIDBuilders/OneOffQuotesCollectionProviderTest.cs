using System;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestedType(typeof(OneOffQuotesCollectionProvider))]
	sealed class OneOffQuotesCollectionProviderTest : CollectionProviderBaseTest
	{
		protected override Type ExpectedCollectionType => Type.GetType("Enterprise.Freight.QuotedBookings.Business.ViewOneOffQuoteCollection, Enterprise.Freight.QuotedBookings.Business");

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.OneOffQuotes;
	}
}
