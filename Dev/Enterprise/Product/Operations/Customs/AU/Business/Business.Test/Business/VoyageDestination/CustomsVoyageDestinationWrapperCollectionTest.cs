using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CustomsVoyageDestinationWrapperCollection))]
	sealed class CustomsVoyageDestinationWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CustomsVoyageDestinationWrapperCollection>
	{
		public void TestWeGetDestinationsAlreadyLoaded()
		{
			Voyage.Destinations.AddNew();
			AssertEquals("Collection.Count", 1, Collection.Count);
		}

		public void TestAllowNew()
		{
			AssertEquals("AllowNew", false, Collection.AllowNew);
		}

		public void TestWeAreKeptSynchronisedWithVoyageDestinations()
		{
			AssertEquals("Collection.Count", 0, Collection.Count);
			Voyage.Destinations.AddNew();
			AssertEquals("Collection.Count", 1, Collection.Count);
			Voyage.Destinations.Remove(Voyage.Destinations[0]);
			AssertEquals("Collection.Count", 0, Collection.Count);
		}

		[ExpectException(typeof(NotSupportedException))]
		public void TestAddNewUnsupported()
		{
			Collection.AddNew();
		}

		public override void TestDelete()
		{
			Assert(true);//This is a readonly wrapper
		}

		public override void TestTypedAddNew()
		{
			Assert(true);
		}

		protected override CustomsVoyageDestinationWrapperCollection GetCollectionToTest() => Collection;

		protected override BusinessObject GetNewElementToAddToTheCollection() => new CustomsVoyageDestinationWrapper(Wrapper, Factory.New<VoyageDestination>());

		new CustomsVoyageDestinationWrapperCollection Collection => Wrapper.Destinations;

		CustomsJobVoyageWrapper wrapper;
		CustomsJobVoyageWrapper Wrapper => wrapper ?? (wrapper = new CustomsJobVoyageWrapper(Voyage));

		JobVoyage voyage;
		JobVoyage Voyage => voyage ?? (voyage = Factory.New<JobVoyage>());
	}
}
