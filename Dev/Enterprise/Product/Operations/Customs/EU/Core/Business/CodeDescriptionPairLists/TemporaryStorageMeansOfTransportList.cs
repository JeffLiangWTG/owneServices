using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business;

sealed public class TemporaryStorageMeansOfTransportList : CodeDescriptionPairList
{
	public TemporaryStorageMeansOfTransportList()
	{
		AddPair(MeansOfTransportList.Codes.ImoShipIdentificationNumber, MeansOfTransportList.Descriptions.ImoShipIdentificationNumber);
		AddPair(MeansOfTransportList.Codes.WagonNumber, MeansOfTransportList.Descriptions.WagonNumber);
		AddPair(InlandMeansOfTransportList.Codes.TrainNumber, InlandMeansOfTransportList.Descriptions.TrainNumber);
		AddPair(MeansOfTransportList.Codes.RegistrationNumberOfTheRoadVehicle, MeansOfTransportList.Descriptions.RegistrationNumberOfTheRoadVehicle);
		AddPair(InlandMeansOfTransportList.Codes.RegistrationNumberOfTheRoadTrailer, InlandMeansOfTransportList.Descriptions.RegistrationNumberOfTheRoadTrailer);
		AddPair(MeansOfTransportList.Codes.RegistrationNumberOfTheAircraft, MeansOfTransportList.Descriptions.RegistrationNumberOfTheAircraft);
		AddPair(MeansOfTransportList.Codes.EuropeanVesselIdentificationNumberEniCode, MeansOfTransportList.Descriptions.EuropeanVesselIdentificationNumberEniCode);
	}
}
