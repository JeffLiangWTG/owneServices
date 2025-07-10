using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public static class JobDeclarationInlandTransportResDataHelper
	{
		public static ResourceStringData GetInlandTransactionRes(ZString transportMode, ZString transportMeans)
		{
			ResourceStringData result = null;

			switch (transportMode)
			{
				case TransportTypeList.Codes.Sea:
					switch (transportMeans)
					{
						case TransportMeansList.Codes.ImoShipIdentificationNumber:
							result = Res_SEA_IMOShipNumber;
							break;
						case TransportMeansList.Codes.NameOfTheSeaGoingVessel:
							result = Res_VesselName;
							break;
						default:
							break;
					}
					break;
			}

			return result ?? Res_TransportId;
		}

		static ResourceStringData Res_TransportId => Res.GetData("EUJobDeclarationInlandTransportResDataHelper|TransportId", englishCaptionOrFullDescription: "Transport ID");
		static ResourceStringData Res_SEA_IMOShipNumber => Res.GetData("EUJobDeclarationInlandTransportResDataHelper|SEA_IMOShipNumber", "IMO No.", englishFullDescription: "Lloyds / IMO Number");
		static ResourceStringData Res_VesselName => Res.GetData("EUJobDeclarationInlandTransportResDataHelper|VesselName", englishCaptionOrFullDescription: "Vessel Name");
	}
}
