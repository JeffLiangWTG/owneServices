using CargoWise.Customs.IT.MessageContracts.NCTS.Departure;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.NCTS.Business.Testing;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml.Testing;

sealed class PlaceOfLoadingWrapperTest : TestCaseWithFactory
{
	public void TestNewOrNull()
	{
		CombineAssertions("When Movement Header not valid, NewOrNull", () =>
		{
			AssertNull(PlaceOfLoadingWrapper.NewOrNull(Factory.New<NctsDepartureMovementHeader>()));
			AssertNull("Movement Header with white spaces", PlaceOfLoadingWrapper.NewOrNull(CreateMovementHeader(" ", " ")));
			AssertNull("Movement Header with port BM_PortOfPresentationCode length is 1", PlaceOfLoadingWrapper.NewOrNull(CreateMovementHeader("A", " ")));
			AssertNull("Movement Header with port BM_PortOfPresentationCode length is 3", PlaceOfLoadingWrapper.NewOrNull(CreateMovementHeader("ABC", " ")));
			AssertNull("Movement Header with port BM_PortOfPresentationCode length is 4", PlaceOfLoadingWrapper.NewOrNull(CreateMovementHeader("ABCD", " ")));
		});

		CombineAssertions("When Movement Header  is valid, NewOrNull", () =>
		{
			AssertNotNull("Movement Header CountryCode", PlaceOfLoadingWrapper.NewOrNull(CreateMovementHeader("IT", " ")));
			AssertNotNull("Movement Header UNLOCode", PlaceOfLoadingWrapper.NewOrNull(CreateMovementHeader("ITMIL", " ")));
			AssertNotNull("Movement Header Location", PlaceOfLoadingWrapper.NewOrNull(CreateMovementHeader("", "a")));
		});
	}

	public void TestUNLOCode()
	{
		var wrapper = PlaceOfLoadingWrapper.NewOrNull(CreateMovementHeader("  ITV", " "));
		AssertEquals(nameof(IGeoLocationDetails.UNLOCode), "ITV", wrapper.UNLOCode);

		wrapper = PlaceOfLoadingWrapper.NewOrNull(CreateMovementHeader("IT", " "));
		AssertNullOrEmpty(nameof(IGeoLocationDetails.UNLOCode), wrapper.UNLOCode);
	}

	public void TestCountryCode()
	{
		var wrapper = PlaceOfLoadingWrapper.NewOrNull(CreateMovementHeader("ITMIL", " "));
		AssertNullOrEmpty(nameof(IGeoLocationDetails.CountryCode), wrapper.CountryCode);

		wrapper = PlaceOfLoadingWrapper.NewOrNull(CreateMovementHeader("IT", " "));
		AssertEquals(nameof(IGeoLocationDetails.CountryCode), "IT", wrapper.CountryCode);
	}

	public void TestLocation()
	{
		var wrapper = PlaceOfLoadingWrapper.NewOrNull(CreateMovementHeader("ITMIL", " "));
		AssertNullOrEmpty("when Place eOf Loading is empty", wrapper.Location);

		wrapper = PlaceOfLoadingWrapper.NewOrNull(CreateMovementHeader("", "   Milano  "));
		AssertEquals("when Place eOf Loading is not empty", "Milano", wrapper.Location);
	}

	NctsDepartureMovementHeader CreateMovementHeader(string portOfPresentationCode, string placeOfLoading)
	{
		var movementHeader = Factory.NewDepartureNctsHeader().MovementHeader;
		movementHeader.BM_PortOfPresentationCode = portOfPresentationCode;
		movementHeader.BM_PlaceOfLoading = placeOfLoading;
		return movementHeader;
	}
}
