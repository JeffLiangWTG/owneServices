using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CA.Business;

public class CBSATransportTypeList : CodeDescriptionPairList
{
	public CBSATransportTypeList()
	{
		AddPair(TransportTypeList.Codes.Air, Descriptions.Air);
		AddPair(TransportTypeList.Codes.InlandWaterwayTransport, Descriptions.InlandWaterwayTransport);
		AddPair(TransportTypeList.Codes.FixedTransportInstallations, Descriptions.FixedTransportInstallations);
		AddPair(TransportTypeList.Codes.Rail, Descriptions.Rail);
		AddPair(TransportTypeList.Codes.Road, Descriptions.Road);
		AddPair(TransportTypeList.Codes.Sea, Descriptions.Sea);
		AddPair(TransportTypeList.Codes.Mail, Descriptions.Mail);
	}

	public static class Descriptions
	{
		public const string Air = "4";
		public const string InlandWaterwayTransport = "8";
		public const string FixedTransportInstallations = "7";
		public const string Rail = "2";
		public const string Road = "3";
		public const string Sea = "1";
		public const string Mail = "5";
	}
}
