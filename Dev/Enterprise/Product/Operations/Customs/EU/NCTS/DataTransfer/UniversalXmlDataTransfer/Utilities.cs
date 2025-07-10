using System.Collections.Immutable;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.DataTransfer
{
	public static class Utilities
	{
		public readonly static ImmutableArray<(ZString transportMode, ZString idType, ZString transportMeansCode)> TransportMeansMapping = ImmutableArray.CreateRange(new (ZString, ZString, ZString)[] {
			(ModeOfTransportList.Codes._1_SeaTransport, NctsTransportTypeOfIdList.Codes._10, TransportMeansList.Codes.ImoShipIdentificationNumber),
			(ModeOfTransportList.Codes._1_SeaTransport, NctsTransportTypeOfIdList.Codes._11, TransportMeansList.Codes.NameOfTheSeaGoingVessel),
			(ModeOfTransportList.Codes._2_RailTransport, NctsTransportTypeOfIdList.Codes._20, TransportMeansList.Codes.WagonNumber),
			(ModeOfTransportList.Codes._2_RailTransport, NctsTransportTypeOfIdList.Codes._21, TransportMeansList.Codes.TrainNumber),
			(ModeOfTransportList.Codes._3_RoadTransport, NctsTransportTypeOfIdList.Codes._30, TransportMeansList.Codes.RegistrationNumberOfTheRoadVehicle),
			(ModeOfTransportList.Codes._3_RoadTransport, NctsTransportTypeOfIdList.Codes._31, TransportMeansList.Codes.RegistrationNumberOfTheRoadVehicle),
			(ModeOfTransportList.Codes._4_AirTransport, NctsTransportTypeOfIdList.Codes._40, TransportMeansList.Codes.IataFlightNumber),
			(ModeOfTransportList.Codes._4_AirTransport, NctsTransportTypeOfIdList.Codes._41, TransportMeansList.Codes.RegistrationNumberOfTheAircraft),
			(ModeOfTransportList.Codes._8_InlandWaterwayTransport, NctsTransportTypeOfIdList.Codes._80, TransportMeansList.Codes.EuropeanVesselIdentificationNumberEniCode),
			(ModeOfTransportList.Codes._8_InlandWaterwayTransport, NctsTransportTypeOfIdList.Codes._81, TransportMeansList.Codes.NameOfTheInlandWaterwaysVessel)
		});

		public static ZString GetCountryDescriptionFromCode(this UniversalDataObjectWriterHelper helper, ZString code)
			=> helper.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, code)?.Description ?? ZString.Empty;
	}
}
