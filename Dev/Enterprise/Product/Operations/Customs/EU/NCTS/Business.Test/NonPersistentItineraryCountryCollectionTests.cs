using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NonPersistentItineraryCountryCollection))]
	class NonPersistentItineraryCountryCollectionTests : NonPersistentBusinessObjectCollectionTestCase<NonPersistentItineraryCountryCollection>
	{
		protected override NonPersistentItineraryCountryCollection GetCollectionToTest()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			return header.Itinerary;
		}

		protected override CargoWise.EntityFramework.BusinessObject GetNewElementToAddToTheCollection()
		{
			return new NonPersistentItineraryCountry();
		}
	}
}
