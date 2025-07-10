using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class TransportTypeList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string Air = Customs.Business.TransportTypeList.Codes.Air;
			public const string Sea = Customs.Business.TransportTypeList.Codes.Sea;
			public const string Mail = Customs.Business.TransportTypeList.Codes.Mail;
		}

		public static class Descriptions
		{
			public static string Air => Customs.Business.TransportTypeList.Descriptions.Air;
			public static string Sea => Customs.Business.TransportTypeList.Descriptions.Sea;
			public static string Mail => Customs.Business.TransportTypeList.Descriptions.Mail;
		}

		public TransportTypeList()
		{
			AddPair(Codes.Air, Descriptions.Air);
			AddPair(Codes.Sea, Descriptions.Sea);
			AddPair(Codes.Mail, Descriptions.Mail);
		}
	}
}
