namespace Enterprise.Customs.CN.Business
{
	public class TransportTypeList : ZArchitecture.Core.CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string Air = Customs.Business.TransportTypeList.Codes.Air;
			public const string Sea = Customs.Business.TransportTypeList.Codes.Sea;
			public const string Mail = Customs.Business.TransportTypeList.Codes.Mail;
			public const string Road = Customs.Business.TransportTypeList.Codes.Road;
			public const string Rail = Customs.Business.TransportTypeList.Codes.Rail;
			public const string FixedTransportInstallations = Customs.Business.TransportTypeList.Codes.FixedTransportInstallations;
			public const string PassengerCarried = "PHC";
		}

		public static class Descriptions
		{
			public static string Air => Customs.Business.TransportTypeList.Descriptions.Air;
			public static string Sea => Customs.Business.TransportTypeList.Descriptions.Sea;
			public static string Mail => Customs.Business.TransportTypeList.Descriptions.Mail;
			public static string Road => Customs.Business.TransportTypeList.Descriptions.Road;
			public static string Rail => Customs.Business.TransportTypeList.Descriptions.Rail;
			public static string FixedTransportInstallations => Customs.Business.TransportTypeList.Descriptions.FixedTransportInstallations;
			public static string PassengerCarried => Res.GetString("f96f2d4a-eb9a-4ab7-b382-99d99c91bbae", "Passenger Carried");
		}

		public TransportTypeList()
		{
			AddPair(Codes.Air, Descriptions.Air);
			AddPair(Codes.Sea, Descriptions.Sea);
			AddPair(Codes.Mail, Descriptions.Mail);
			AddPair(Codes.Road, Descriptions.Road);
			AddPair(Codes.Rail, Descriptions.Rail);
			AddPair(Codes.FixedTransportInstallations, Descriptions.FixedTransportInstallations);
			AddPair(Codes.PassengerCarried, Descriptions.PassengerCarried);
		}
	}
}
