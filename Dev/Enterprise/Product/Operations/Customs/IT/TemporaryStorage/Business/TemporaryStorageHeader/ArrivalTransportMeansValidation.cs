using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

sealed class ArrivalTransportMeansValidation : EU.Business.CusTempStorage.ArrivalTransportMeansValidation
{
	public ArrivalTransportMeansValidation(AutoCusTransportMeans parent) : base(parent)
	{
	}

	protected override void CheckTPM_IdentificationNumber_NotEntered()
	{
		var parentHeader = Parent.ParentHeader;
		if (parentHeader != null && Parent.TPM_IdentificationNumber.IsEmpty)
		{
			var message = ValidationCaptions.ArrivalTransportMeans.YouHaveNotEnteredAnArrivalTransportMeans;
			switch (parentHeader.TransportType)
			{
				case MeansOfTransportList.Codes.ImoShipIdentificationNumber:
					message = ValidationCaptions.ArrivalTransportMeans.YouHaveNotEnteredAnIMONumber;
					break;
				case MeansOfTransportList.Codes.NameOfTheSeaGoingVessel:
					message = ValidationCaptions.ArrivalTransportMeans.YouHaveNotEnteredAVesselName;
					break;
				case MeansOfTransportList.Codes.WagonNumber:
					message = ValidationCaptions.ArrivalTransportMeans.YouHaveNotEnteredAWagonNumber;
					break;
				case MeansOfTransportList.Codes.RegistrationNumberOfTheRoadVehicle:
					message = ValidationCaptions.ArrivalTransportMeans.YouHaveNotEnteredARoadVehicleRegNo;
					break;
				case MeansOfTransportList.Codes.IataFlightNumber:
					message = ValidationCaptions.ArrivalTransportMeans.YouHaveNotEnteredAnIATAFlightNumber;
					break;
				case MeansOfTransportList.Codes.RegistrationNumberOfTheAircraft:
					message = ValidationCaptions.ArrivalTransportMeans.YouHaveNotEnteredAnAircraftRegNo;
					break;
				case MeansOfTransportList.Codes.EuropeanVesselIdentificationNumberEniCode:
					message = ValidationCaptions.ArrivalTransportMeans.YouHaveNotEnteredAnENICode;
					break;
				case MeansOfTransportList.Codes.NameOfTheInlandWaterwaysVessel:
					message = ValidationCaptions.ArrivalTransportMeans.YouHaveNotEnteredAVesselName;
					break;
				default:
					break;
			}
			Parent.TPM_IdentificationNumberInfo.AddMessageError(message);
		}
	}
}
