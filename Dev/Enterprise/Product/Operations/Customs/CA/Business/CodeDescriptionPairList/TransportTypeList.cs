
//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class TransportTypeList : CodeDescriptionPairList
	{
		public TransportTypeList()
		{
			AddPair(Codes.Air, Customs.Business.TransportTypeList.Descriptions.Air);
			AddPair(Codes.InlandWaterwayTransport, Customs.Business.TransportTypeList.Descriptions.InlandWaterwayTransport);
			AddPair(Codes.FixedTransportInstallations, Customs.Business.TransportTypeList.Descriptions.FixedTransportInstallations);
			AddPair(Codes.Rail, Customs.Business.TransportTypeList.Descriptions.Rail);
			AddPair(Codes.Road, Customs.Business.TransportTypeList.Descriptions.Road);
			AddPair(Codes.Sea, Customs.Business.TransportTypeList.Descriptions.Sea);
			AddPair(Codes.Mail, Customs.Business.TransportTypeList.Descriptions.Mail);
			AddPair(Codes.NoCarrier, Descriptions.NoCarrier);
		}

		public static class Codes
		{
			public const string Air = Customs.Business.TransportTypeList.Codes.Air;
			public const string InlandWaterwayTransport = Customs.Business.TransportTypeList.Codes.InlandWaterwayTransport;
			public const string FixedTransportInstallations = Customs.Business.TransportTypeList.Codes.FixedTransportInstallations;
			public const string Rail = Customs.Business.TransportTypeList.Codes.Rail;
			public const string Road = Customs.Business.TransportTypeList.Codes.Road;
			public const string Sea = Customs.Business.TransportTypeList.Codes.Sea;
			public const string Mail = Customs.Business.TransportTypeList.Codes.Mail;
			public const string NoCarrier = "NOC";
		}

		public static class Descriptions
		{
			public static string Air => Customs.Business.TransportTypeList.Descriptions.Air;
			public static string InlandWaterwayTransport => Customs.Business.TransportTypeList.Descriptions.InlandWaterwayTransport;
			public static string Pipeline => Customs.Business.TransportTypeList.Descriptions.FixedTransportInstallations;
			public static string Rail => Customs.Business.TransportTypeList.Descriptions.Rail;
			public static string Road => Customs.Business.TransportTypeList.Descriptions.Road;
			public static string Sea => Customs.Business.TransportTypeList.Descriptions.Sea;
			public static string Mail => Customs.Business.TransportTypeList.Descriptions.Mail;
			public static string NoCarrier
			{
				get { return Res.GetString("7ACA5BDC-1610-4500-B21B-74BF967333E4", "No Carrier/Hand-Carried"); }
			}
		}

		public static ZString GetTransportModeNumber(ZString transportModeCode)
		{
			var result = ZString.Empty;
			switch (transportModeCode)
			{
				case TransportTypeList.Codes.Air:
					result = "1";
					break;
				case TransportTypeList.Codes.Road:
					result = "2";
					break;
				case TransportTypeList.Codes.Mail:
					result = "5";
					break;
				case TransportTypeList.Codes.Rail:
					result = "6";
					break;
				case TransportTypeList.Codes.FixedTransportInstallations:
					result = "7";
					break;
				case TransportTypeList.Codes.NoCarrier:
					result = "8";
					break;
				case TransportTypeList.Codes.Sea:
					result = "9";
					break;
				default:
					result = "0";
					break;
			}
			return result;
		}
	}
}
