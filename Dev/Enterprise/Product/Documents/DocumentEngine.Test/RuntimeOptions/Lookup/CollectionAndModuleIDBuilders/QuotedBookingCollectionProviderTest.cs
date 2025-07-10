using System;
using CargoWise.Application;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestedType(typeof(QuotedBookingCollectionProvider))]
	sealed class QuotedBookingCollectionProviderTest : CollectionProviderBaseTest
	{
		protected override Type ExpectedCollectionType => ObjectFactory.GetType<IViewQuotedBookingCollection>();

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.QuotedBookings;
	}
}
