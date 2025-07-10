using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test;

sealed class ActiveBorderTransportMeansProviderTest : Customs.Business.Testing.DataProviderTestCase<ActiveBorderTransportMeansProvider>
{
	public void TestConstructor()
	{
		AssertNull(ActiveBorderTransportMeansProvider.NewOrNull(null));
	}

	public void TestIdentificationNumber()
	{
		manifestHeader.AMA_VehicleRegistration = "ABC123";
		AssertEquals("AMA_VehicleRegistration is set", "ABC123", Provider.IdentificationNumber);

		manifestHeader.AMA_LloydsNumber = "LLYD123";
		AssertEquals("Use AMA_VehicleRegistration when it and AMA_LloydsNumber is set", "ABC123", Provider.IdentificationNumber);

		manifestHeader.AMA_VehicleRegistration = ZString.Empty;
		AssertEquals("Use AMA_LloydsNumber when AMA_VehicleRegistration is empty", "LLYD123", Provider.IdentificationNumber);
	}

	public void TestTypeOfIdentification()
	{
		manifestHeader.MOTIdentifierType = "T1";
		AssertEquals("T1", Provider.TypeOfIdentification);
	}

	public void TestTypeOfMeansOfTransport()
	{
		manifestHeader.AMA_TransportMeans = "TM1";
		AssertEquals("TM1", Provider.TypeOfMeansOfTransport);
	}

	public void TestNationality()
	{
		manifestHeader.AMA_RN_NKConveyanceNationality = "DE";
		AssertEquals("DE", Provider.Nationality);
	}

	public void TestModeOfTransport()
	{
		var testCases = new[]
		{
			(transportMode: string.Empty, expectedModeOfTransport: 0),
			(transportMode: TransportTypeList.Codes.Sea, expectedModeOfTransport: 1),
			(transportMode: TransportTypeList.Codes.Rail, expectedModeOfTransport: 2),
			(transportMode: TransportTypeList.Codes.Road, expectedModeOfTransport: 3),
			(transportMode: TransportTypeList.Codes.Air, expectedModeOfTransport: 4),
			(transportMode: TransportTypeList.Codes.InlandWaterwayTransport, expectedModeOfTransport: 8)
		};

		foreach (var (transportMode, expectedModeOfTransport) in testCases)
		{
			manifestHeader.AMA_TransportMode = transportMode;
			AssertEquals(expectedModeOfTransport, Provider.ModeOfTransport);
		}
	}

	public void TestActualDepartureDate()
	{
		var actualDepartureDate = ZDateTimeOffset.Now;
		var expectedActualDepartureDate = actualDepartureDate.ToNullableDateTime();

		manifestHeader.AMA_A_DEP = actualDepartureDate;
		AssertEquals(expectedActualDepartureDate, Provider.ActualDepartureDate);
	}

	public void TestEstimatedDepartureDate()
	{
		var estimatedDepartureDate = ZDateTime.Now;
		var expectedEstimatedDepartureDate = estimatedDepartureDate.ToNullableDateTime();

		manifestHeader.AMA_E_DEP = estimatedDepartureDate;
		AssertEquals(expectedEstimatedDepartureDate, Provider.EstimatedDepartureDate);
	}

	public void TestEstimatedArrivalDate()
	{
		var estimatedArrivalDate = ZDateTime.Now;
		var expectedEstimatedArrivalDate = estimatedArrivalDate.UtcDateTime();

		manifestHeader.AMA_E_ARV = estimatedArrivalDate;
		AssertEquals(expectedEstimatedArrivalDate, Provider.EstimatedArrivalDate);
	}

	public void TestConveyanceReferenceNumber()
	{
		manifestHeader.MOTIdentifier = "CRN0001";
		AssertEquals("CRN0001", Provider.ConveyanceReferenceNumber);
	}

	public void TestCountriesOfRouting()
	{
		manifestHeader.Itinerary.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals(1, Provider.CountriesOfRouting.Count);
			AssertType<ItineraryProvider>(Provider.CountriesOfRouting.Single());
		});
	}

	protected override ActiveBorderTransportMeansProvider GetProvider() => ActiveBorderTransportMeansProvider.NewOrNull(manifestHeader);

	protected override void SetUp()
	{
		base.SetUp();
		manifestHeader = Factory.New<AsycudaManifestHeader>();
	}

	AsycudaManifestHeader manifestHeader;
}
