using System;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class ItineraryWrapperTest : DataProviderTestCase<ItineraryWrapper>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new ItineraryWrapper(null, 1));
	}

	public void TestSequenceNumeric()
	{
		AssertEquals("SequenceNumeric", 1, wrapper.SequenceNumeric);
	}

	public void TestRoutingCountryCode()
	{
		itineraryCountry.CY_Code = "NL";
		AssertEquals("RoutingCountryCode", "NL", wrapper.RoutingCountryCode);
	}

	protected override ItineraryWrapper GetProvider() => wrapper;

	protected override void SetUp()
	{
		base.SetUp();

		itineraryCountry = Factory.New<ItineraryCountry>();
		wrapper = new ItineraryWrapper(itineraryCountry, 1);
	}
	ItineraryWrapper wrapper;
	ItineraryCountry itineraryCountry;
}
