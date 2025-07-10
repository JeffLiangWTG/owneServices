using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business;

sealed public class TemporaryStorageMeansOfTransportList : CodeDescriptionPairList
{
	public TemporaryStorageMeansOfTransportList()
	{
		AddPair(TransportMeansList.Codes.ImoShipIdentificationNumber, MeansOfTransportList.Descriptions.ImoShipIdentificationNumber);
		AddPair(TransportMeansList.Codes.NameOfTheSeaGoingVessel, MeansOfTransportList.Descriptions.NameOfTheSeaGoingVessel);
		AddPair(TransportMeansList.Codes.WagonNumber, MeansOfTransportList.Descriptions.WagonNumber);
		AddPair(TransportMeansList.Codes.RegistrationNumberOfTheRoadVehicle, MeansOfTransportList.Descriptions.RegistrationNumberOfTheRoadVehicle);
		AddPair(TransportMeansList.Codes.RegistrationNumberOfTheRoadTrailer, TransportMeansList.Descriptions.RegistrationNumberOfTheRoadTrailer);
		AddPair(TransportMeansList.Codes.IataFlightNumber, MeansOfTransportList.Descriptions.IataFlightNumber);
		AddPair(TransportMeansList.Codes.RegistrationNumberOfTheAircraft, MeansOfTransportList.Descriptions.RegistrationNumberOfTheAircraft);
		AddPair(TransportMeansList.Codes.EuropeanVesselIdentificationNumberEniCode, MeansOfTransportList.Descriptions.EuropeanVesselIdentificationNumberEniCode);
		AddPair(TransportMeansList.Codes.NameOfTheInlandWaterwaysVessel, MeansOfTransportList.Descriptions.NameOfTheInlandWaterwaysVessel);
	}
}
