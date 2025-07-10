using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BE.Business.Declaration;

public static class JobDeclarationInlandTransportResDataHelper
{
	public static ResourceStringData GetInlandTransactionIDCaption(BaseJobDeclaration declaration) => GetInlandTransactionRes(declaration.JE_TransportModeInland, declaration.JE_TransportMeans);

	static ResourceStringData ResDataRoad => Res.GetData("A3F63B6F-6BD1-4390-A054-BC33622C3792", englishCaption: "Registration No.", englishFullDescription: "[19 05 017 000] Vehicle Registration Number");
	static ResourceStringData ResDataAirIATAFlightNumber => Res.GetData("A2B43C12-A016-4053-8797-5B9A20B05B13", englishCaption: "Flight No.", englishFullDescription: "[19 05 017 000] Flight No.");
	static ResourceStringData ResDataAirAircraftNumber => Res.GetData("D9A751A3-88BD-408A-A430-E76D04DE269D", englishCaption: "Registration No.", englishFullDescription: "[19 05 017 000] Aircraft Registration Number");
	static ResourceStringData ResDataInlandWaterwaysEUVesselCode => Res.GetData("E3D44CAD-B8B8-438E-AD57-397604F95BCC", englishCaption: "ENI Code", englishFullDescription: "[19 05 017 000] European Vessel Identification Number");
	static ResourceStringData ResDataInlandWaterwaysInlandWaterwaysVesselName => ResDataVesselName;
	static ResourceStringData ResDataRailWagonNumber => Res.GetData("EAB075EE-A83D-4E5D-B7E2-F0384886A061", englishCaption: "Wagon No.", englishFullDescription: "[19 05 017 000] Wagon Number");
	static ResourceStringData ResDataRailTrainNumber => Res.GetData("CEAACC90-1428-4226-BAF1-1A0DF48C3901", englishCaption: "Train No.", englishFullDescription: "[19 05 017 000] Train Number");
	static ResourceStringData ResDataSeaIMOShipNumber => Res.GetData("41360FD8-4228-41CE-811B-D28BA6662161", englishCaption: "Lloyds No.", englishFullDescription: "[19 05 017 000] Lloyds Number");
	static ResourceStringData ResDataSeaSeaGoingShipName => ResDataVesselName;

	static ResourceStringData ResDataVesselName => Res.GetData("DE198F44-42AC-405B-8103-7FF0E99BD537", englishCaption: "Vessel Name", englishFullDescription: "[19 05 017 000] Vessel Name");
	static ResourceStringData ResDataTransportId => Res.GetData("75C61AB8-545E-4A61-A687-F308567DAC93", englishCaption: "Transport ID", englishFullDescription: "[19 06 017 000] Identification number of the transport");

	static ResourceStringData GetInlandTransactionRes(string transportMode, string transportMeans)
	{
		ResourceStringData result = null;

		switch (transportMode)
		{
			case TransportTypeList.Codes.Road:
				result = ResDataRoad;
				break;
			case TransportTypeList.Codes.Air:
				switch (transportMeans)
				{
					case TransportMeansList.Codes.IataFlightNumber:
						result = ResDataAirIATAFlightNumber;
						break;
					case TransportMeansList.Codes.RegistrationNumberOfTheAircraft:
						result = ResDataAirAircraftNumber;
						break;
					default:
						break;
				}
				break;
			case TransportTypeList.Codes.InlandWaterwayTransport:
				switch (transportMeans)
				{
					case TransportMeansList.Codes.NameOfTheInlandWaterwaysVessel:
						result = ResDataInlandWaterwaysInlandWaterwaysVesselName;
						break;
					case TransportMeansList.Codes.EuropeanVesselIdentificationNumberEniCode:
						result = ResDataInlandWaterwaysEUVesselCode;
						break;
					default:
						break;
				}
				break;
			case TransportTypeList.Codes.Rail:
				switch (transportMeans)
				{
					case TransportMeansList.Codes.WagonNumber:
						result = ResDataRailWagonNumber;
						break;
					case TransportMeansList.Codes.TrainNumber:
						result = ResDataRailTrainNumber;
						break;
					default:
						break;
				}
				break;
			case TransportTypeList.Codes.Sea:
				switch (transportMeans)
				{
					case TransportMeansList.Codes.ImoShipIdentificationNumber:
						result = ResDataSeaIMOShipNumber;
						break;
					case TransportMeansList.Codes.NameOfTheSeaGoingVessel:
						result = ResDataSeaSeaGoingShipName;
						break;
					default:
						break;
				}
				break;
		}

		return result ?? ResDataTransportId;
	}
}
