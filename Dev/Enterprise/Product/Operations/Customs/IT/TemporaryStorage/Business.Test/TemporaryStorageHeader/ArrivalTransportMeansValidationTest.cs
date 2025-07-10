using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

sealed class ArrivalTransportMeansValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckArrivalTransportMeans()
	{
		var tempHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();

		tempHeader.ArrivalTransportMeansCode = ZString.Empty;

		CombineAssertions(() =>
		{
			tempHeader.TransportType = ZString.Empty;
			tempHeader.ArrivalTransportMeans.Validation.ValidateTPM_IdentificationNumber();
			AssertHasMessageError("Arrival Transport Means Code should have 'empty' message when field is empty and TransportType is empty", tempHeader.ArrivalTransportMeansCodeInfo, ValidationCaptions.ArrivalTransportMeans.YouHaveNotEnteredAnArrivalTransportMeans);

			tempHeader.TransportType = "10";
			tempHeader.ArrivalTransportMeans.Validation.ValidateTPM_IdentificationNumber();
			AssertHasMessageError("Arrival Transport Means Code should have 'IMO' message when field is empty and TransportType = '10'", tempHeader.ArrivalTransportMeansCodeInfo, ValidationCaptions.ArrivalTransportMeans.YouHaveNotEnteredAnIMONumber);

			tempHeader.TransportType = "11";
			tempHeader.ArrivalTransportMeans.Validation.ValidateTPM_IdentificationNumber();
			AssertHasMessageError("Arrival Transport Means Code should have 'Vessel' message when field is empty and TransportType = '11'", tempHeader.ArrivalTransportMeansCodeInfo, ValidationCaptions.ArrivalTransportMeans.YouHaveNotEnteredAVesselName);

			tempHeader.TransportType = "20";
			tempHeader.ArrivalTransportMeans.Validation.ValidateTPM_IdentificationNumber();
			AssertHasMessageError("Arrival Transport Means Code should have 'Wagon' message when field is empty and TransportType = '20'", tempHeader.ArrivalTransportMeansCodeInfo, ValidationCaptions.ArrivalTransportMeans.YouHaveNotEnteredAWagonNumber);

			tempHeader.TransportType = "30";
			tempHeader.ArrivalTransportMeans.Validation.ValidateTPM_IdentificationNumber();
			AssertHasMessageError("Arrival Transport Means Code should have 'Road' message when field is empty and TransportType = '30'", tempHeader.ArrivalTransportMeansCodeInfo, ValidationCaptions.ArrivalTransportMeans.YouHaveNotEnteredARoadVehicleRegNo);

			tempHeader.TransportType = "40";
			tempHeader.ArrivalTransportMeans.Validation.ValidateTPM_IdentificationNumber();
			AssertHasMessageError("Arrival Transport Means Code should have 'IATA' message when field is empty and TransportType = '40'", tempHeader.ArrivalTransportMeansCodeInfo, ValidationCaptions.ArrivalTransportMeans.YouHaveNotEnteredAnIATAFlightNumber);

			tempHeader.TransportType = "41";
			tempHeader.ArrivalTransportMeans.Validation.ValidateTPM_IdentificationNumber();
			AssertHasMessageError("Arrival Transport Means Code should have 'Aircraft' message when field is empty and TransportType = '41'", tempHeader.ArrivalTransportMeansCodeInfo, ValidationCaptions.ArrivalTransportMeans.YouHaveNotEnteredAnAircraftRegNo);

			tempHeader.TransportType = "80";
			tempHeader.ArrivalTransportMeans.Validation.ValidateTPM_IdentificationNumber();
			AssertHasMessageError("Arrival Transport Means Code should have 'ENI' message when field is empty and TransportType = '80'", tempHeader.ArrivalTransportMeansCodeInfo, ValidationCaptions.ArrivalTransportMeans.YouHaveNotEnteredAnENICode);

			tempHeader.TransportType = "81";
			tempHeader.ArrivalTransportMeans.Validation.ValidateTPM_IdentificationNumber();
			AssertHasMessageError("Arrival Transport Means Code should have 'Vessel' message when field is empty and TransportType = '81'", tempHeader.ArrivalTransportMeansCodeInfo, ValidationCaptions.ArrivalTransportMeans.YouHaveNotEnteredAVesselName);
		});
	}
}

