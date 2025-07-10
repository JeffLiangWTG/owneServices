using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

sealed public class TemporaryStorageMeansOfTransportList : CodeDescriptionPairList
{
	public TemporaryStorageMeansOfTransportList()
	{
		AddPair(MeansOfTransportList.Codes.ImoShipIdentificationNumber, MeansOfTransportList.Descriptions.ImoShipIdentificationNumber);
		AddPair(MeansOfTransportList.Codes.NameOfTheSeaGoingVessel, MeansOfTransportList.Descriptions.NameOfTheSeaGoingVessel);
		AddPair(MeansOfTransportList.Codes.WagonNumber, MeansOfTransportList.Descriptions.WagonNumber);
		AddPair(MeansOfTransportList.Codes.RegistrationNumberOfTheRoadVehicle, MeansOfTransportList.Descriptions.RegistrationNumberOfTheRoadVehicle);
		AddPair(MeansOfTransportList.Codes.IataFlightNumber, MeansOfTransportList.Descriptions.IataFlightNumber);
		AddPair(MeansOfTransportList.Codes.RegistrationNumberOfTheAircraft, MeansOfTransportList.Descriptions.RegistrationNumberOfTheAircraft);
		AddPair(MeansOfTransportList.Codes.EuropeanVesselIdentificationNumberEniCode, MeansOfTransportList.Descriptions.EuropeanVesselIdentificationNumberEniCode);
		AddPair(MeansOfTransportList.Codes.NameOfTheInlandWaterwaysVessel, MeansOfTransportList.Descriptions.NameOfTheInlandWaterwaysVessel);
	}
}
