using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.IE;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public static class JobDeclarationInlandTransportResDataHelper
	{
		public static ResourceStringData GetInlandTransportModeCaption(JobDeclaration jobDeclaration)
		{
			if (jobDeclaration.IsUCC5AndIsImport)
			{
				return Res.GetData("1A038C5B-DA7C-41E0-B822-5E3FB1492875", "[7/5] Trans. Mode", "[7/5] Transport Mode", "[7/5] Inland Transport Mode", "[7/5] Inland mode of transport");
			}
			return Res.GetData("37D0E051-D608-4C8B-B301-301FDB7887EE", englishCaption: "Trans. Mode", englishFullDescription: "[19 04 001 000] Inland Mode of Transport");
		}

		public static ResourceStringData GetInlandTransactionIDCaption(JobDeclaration declaration) => GetInlandTransactionRes(declaration.JE_TransportModeInland, declaration.JE_TransportMeans, declaration.JE_MessageType, declaration.JE_ApplicationCode);

		static ResourceStringData Res_ROA => Res.GetData("BD710590-6AF3-4ECA-905A-243F385F9D9E", englishCaption: "Registration No.", englishFullDescription: "[19 05 017 000] Vehicle Registration Number");
		static ResourceStringData Res_AIR_IATAFlightNumber => Res.GetData("52864CB3-6D4D-4450-8907-07F204D3D295", englishCaption: "Flight No.", englishFullDescription: "[19 05 017 000] Flight No.");
		static ResourceStringData Res_AIR_AircraftNumber => Res.GetData("38BA2A1D-52D6-4AD2-8859-04AA127C6C1E", englishCaption: "Registration No.", englishFullDescription: "[19 05 017 000] Aircraft Registration Number");
		static ResourceStringData Res_IWT_EUVesselCode => Res.GetData("7FFD3562-9944-4289-B768-54895C65E498", englishCaption: "ENI Code", englishFullDescription: "[19 05 017 000] European Vessel Identification Number");
		static ResourceStringData Res_IWT_InlandWaterwaysVesselName => Res_VesselName;
		static ResourceStringData Res_RAI_WagonNumber => Res.GetData("373097B4-1A43-4F67-A042-5B73F8FE532C", englishCaption: "Wagon No.", englishFullDescription: "[19 05 017 000] Wagon Number");
		static ResourceStringData Res_RAI_TrainNumber => Res.GetData("0B3F10AA-6918-43FA-BE6B-5565713602A1", englishCaption: "Train No.", englishFullDescription: "[19 05 017 000] Train Number");
		static ResourceStringData Res_SEA_IMOShipNumber => Res.GetData("039A4B63-3F1B-4130-A079-4DC887EC8291", englishCaption: "Lloyds No.", englishFullDescription: "[19 05 017 000] Lloyds Number");
		static ResourceStringData Res_SEA_SeaGoingShipName => Res_VesselName;
		static ResourceStringData Res_VesselName => Res.GetData("6E10F520-7DAA-4DE1-998F-A1F6DB308B3D", englishCaption: "Vessel Name", englishFullDescription: "[19 05 017 000] Vessel Name");
		static ResourceStringData Res_TransportId_EXP => Res.GetData("80B0B959-074B-447A-AA61-B87BADE8FEE8", englishCaptionOrFullDescription: "Transport ID");
		static ResourceStringData Res_TransportId_IMP => Res.GetData("B77C8C5D-91BB-4F57-B480-8AB3B5BBEBE9", englishCaption: "Transport ID", englishFullDescription: "[19 06 017 000] Arrival transport means < Identification number");
		static ResourceStringData Res_ROA_ImportUCC5 => Res.GetData("5DBECB3E-3569-4F21-943C-A8373CA51C27", englishCaption: "[7/9] Reg. No.", englishFullDescription: "[7/9] Vehicle Registration Number");
		static ResourceStringData Res_AIR_IATAFlightNumber_ImportUCC5 => Res.GetData("BF7C5428-8205-41BF-9546-DB3B5DB66A53", englishCaptionOrFullDescription: "[7/9] Flight No.");
		static ResourceStringData Res_AIR_AircraftNumber_ImportUCC5 => Res.GetData("181767D7-3E81-4EFB-A058-BE322F4EC453", englishCaption: "[7/9] Reg. No.", englishFullDescription: "[7/9] Aircraft Registration Number");
		static ResourceStringData Res_IWT_EUVesselCode_ImportUCC5 => Res.GetData("2C21B691-94E9-4DAF-A6D9-E7C7E38D95D6", englishCaption: "[7/9] ENI Code", englishFullDescription: "[7/9] European Vessel Identification Number");
		static ResourceStringData Res_IWT_InlandWaterwaysVesselName_ImportUCC5 => Res_VesselName_ImportUCC5;
		static ResourceStringData Res_RAI_WagonNumber_ImportUCC5 => Res.GetData("2C714B08-84BE-4F60-AD15-7031D4FAC7AE", englishCaption: "[7/9] Wagon No.", englishFullDescription: "[7/9] Wagon Number");
		static ResourceStringData Res_RAI_TrainNumber_ImportUCC5 => Res.GetData("0A11386D-870D-46A5-879B-99100A96FB1E", englishCaption: "[7/9] Train No.", englishFullDescription: "[7/9] Train Number");
		static ResourceStringData Res_SEA_IMOShipNumber_ImportUCC5 => Res.GetData("51017D5E-C8AD-421E-941B-2EC1989E763C", englishCaption: "[7/9] Lloyds No.", englishFullDescription: "[7/9] Lloyds Number");
		static ResourceStringData Res_SEA_SeaGoingShipName_ImportUCC5 => Res_VesselName_ImportUCC5;
		static ResourceStringData Res_VesselName_ImportUCC5 => Res.GetData("B08AE0EA-7EC1-41B9-8EE6-BC218A12FBF4", englishCaptionOrFullDescription: "[7/9] Vessel Name");
		static ResourceStringData Res_TransportId_IMPUCC5 => Res.GetData("6DF96FCA-D439-4780-AEDE-D399DC4BCD63", englishCaption: "[7/9] Transport ID", englishFullDescription: "[7/9] Arrival transport means < Identification number");

		static ResourceStringData GetInlandTransactionRes(string transportMode, string transportMeans, string messageType, string applicationCode) => (transportMode, transportMeans, messageType, applicationCode) switch
		{
			(TransportTypeList.Codes.Road, _, IEJobMessageTypeList.Codes.Import, ImportDeclarationApplicationCodeList.Codes.V1) => Res_ROA_ImportUCC5,
			(TransportTypeList.Codes.Road, _, _, _) => Res_ROA,

			(TransportTypeList.Codes.Air, TransportMeansList.Codes.IataFlightNumber, IEJobMessageTypeList.Codes.Import, ImportDeclarationApplicationCodeList.Codes.V1) => Res_AIR_IATAFlightNumber_ImportUCC5,
			(TransportTypeList.Codes.Air, TransportMeansList.Codes.IataFlightNumber, _, _) => Res_AIR_IATAFlightNumber,
			(TransportTypeList.Codes.Air, TransportMeansList.Codes.RegistrationNumberOfTheAircraft, IEJobMessageTypeList.Codes.Import, ImportDeclarationApplicationCodeList.Codes.V1) => Res_AIR_AircraftNumber_ImportUCC5,
			(TransportTypeList.Codes.Air, TransportMeansList.Codes.RegistrationNumberOfTheAircraft, _, _) => Res_AIR_AircraftNumber,

			(TransportTypeList.Codes.InlandWaterwayTransport, TransportMeansList.Codes.EuropeanVesselIdentificationNumberEniCode, IEJobMessageTypeList.Codes.Import, ImportDeclarationApplicationCodeList.Codes.V1) => Res_IWT_EUVesselCode_ImportUCC5,
			(TransportTypeList.Codes.InlandWaterwayTransport, TransportMeansList.Codes.EuropeanVesselIdentificationNumberEniCode, _, _) => Res_IWT_EUVesselCode,
			(TransportTypeList.Codes.InlandWaterwayTransport, TransportMeansList.Codes.NameOfTheInlandWaterwaysVessel, IEJobMessageTypeList.Codes.Import, ImportDeclarationApplicationCodeList.Codes.V1) => Res_IWT_InlandWaterwaysVesselName_ImportUCC5,
			(TransportTypeList.Codes.InlandWaterwayTransport, TransportMeansList.Codes.NameOfTheInlandWaterwaysVessel, _, _) => Res_IWT_InlandWaterwaysVesselName,

			(TransportTypeList.Codes.Rail, TransportMeansList.Codes.WagonNumber, IEJobMessageTypeList.Codes.Import, ImportDeclarationApplicationCodeList.Codes.V1) => Res_RAI_WagonNumber_ImportUCC5,
			(TransportTypeList.Codes.Rail, TransportMeansList.Codes.WagonNumber, _, _) => Res_RAI_WagonNumber,
			(TransportTypeList.Codes.Rail, TransportMeansList.Codes.TrainNumber, IEJobMessageTypeList.Codes.Import, ImportDeclarationApplicationCodeList.Codes.V1) => Res_RAI_TrainNumber_ImportUCC5,
			(TransportTypeList.Codes.Rail, TransportMeansList.Codes.TrainNumber, _, _) => Res_RAI_TrainNumber,

			(TransportTypeList.Codes.Sea, TransportMeansList.Codes.ImoShipIdentificationNumber, IEJobMessageTypeList.Codes.Import, ImportDeclarationApplicationCodeList.Codes.V1) => Res_SEA_IMOShipNumber_ImportUCC5,
			(TransportTypeList.Codes.Sea, TransportMeansList.Codes.ImoShipIdentificationNumber, _, _) => Res_SEA_IMOShipNumber,
			(TransportTypeList.Codes.Sea, TransportMeansList.Codes.NameOfTheSeaGoingVessel, IEJobMessageTypeList.Codes.Import, ImportDeclarationApplicationCodeList.Codes.V1) => Res_SEA_SeaGoingShipName_ImportUCC5,
			(TransportTypeList.Codes.Sea, TransportMeansList.Codes.NameOfTheSeaGoingVessel, _, _) => Res_SEA_SeaGoingShipName,

			(_, _, IEJobMessageTypeList.Codes.Import, ImportDeclarationApplicationCodeList.Codes.V1) => Res_TransportId_IMPUCC5,
			(_, _, IEJobMessageTypeList.Codes.Import, _) => Res_TransportId_IMP,
			(_, _, _, _) => Res_TransportId_EXP,
		};
	}
}
