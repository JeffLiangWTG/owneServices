using System;
using CargoWise.Application;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.Integration.Rating;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestedType(typeof(QuotationsCollectionProvider))]
	sealed class QuotationsCollectionProviderTest : CollectionProviderBaseTest
	{
		protected override Type ExpectedCollectionType => ObjectFactory.GetType<IQuoteCollection>();

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.Quotations;
	}
}
