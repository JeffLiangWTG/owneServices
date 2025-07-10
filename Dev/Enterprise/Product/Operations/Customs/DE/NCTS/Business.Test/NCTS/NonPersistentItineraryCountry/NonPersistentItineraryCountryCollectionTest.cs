using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	[TestedType(typeof(NonPersistentItineraryCountryCollection))]
	class NonPersistentItineraryCountryCollectionTest : NonPersistentBusinessObjectCollectionTestCase<NonPersistentItineraryCountryCollection>
	{
		protected override NonPersistentItineraryCountryCollection GetCollectionToTest()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			return header.Itinerary;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new NonPersistentItineraryCountry(1, Factory);
		}
	}
}
