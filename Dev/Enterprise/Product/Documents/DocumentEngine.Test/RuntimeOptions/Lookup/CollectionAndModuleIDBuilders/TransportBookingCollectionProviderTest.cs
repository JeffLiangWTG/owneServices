using System;
using CargoWise.Application;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.TransportBookings.Shared;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestedType(typeof(TransportBookingCollectionProvider))]
	sealed class TransportBookingCollectionProviderTest : CollectionProviderBaseTest
	{
		protected override Type ExpectedCollectionType => ObjectFactory.GetType<IDtbBookingCollection>();

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.DtbBooking;
	}
}
