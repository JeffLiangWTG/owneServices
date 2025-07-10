namespace Enterprise.Customs.JP.Business
{
	public class TransportTypeList : ZArchitecture.Core.CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string Air = Customs.Business.TransportTypeList.Codes.Air;
			public const string Sea = Customs.Business.TransportTypeList.Codes.Sea;
		}

		public static class Descriptions
		{
			public static string Air => Customs.Business.TransportTypeList.Descriptions.Air;
			public static string Sea => Customs.Business.TransportTypeList.Descriptions.Sea;
		}

		public TransportTypeList()
		{
			AddPair(Codes.Air, Descriptions.Air);
			AddPair(Codes.Sea, Descriptions.Sea);
		}
	}
}
