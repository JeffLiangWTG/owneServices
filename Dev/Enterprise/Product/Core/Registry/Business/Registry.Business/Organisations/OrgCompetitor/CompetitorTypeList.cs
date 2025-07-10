using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public class CompetitorTypeList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string Customs = "CMB";
			public const string Forwarding = "CMF";
			public const string LandTransport = "CMD";
			public const string Warehouse = "CMW";
		}

		public static class Descriptions
		{
			public static MultilingualString Customs { get { return ResString.GetMultilingualString("CompetitorTypeList|Customs", "Customs"); } }
			public static MultilingualString Forwarding { get { return ResString.GetMultilingualString("CompetitorTypeList|Forwarding", "Forwarding"); } }
			public static MultilingualString LandTransport { get { return ResString.GetMultilingualString("CompetitorTypeList|LandTransport", "Land Transport"); } }
			public static MultilingualString Warehouse { get { return ResString.GetMultilingualString("CompetitorTypeList|Warehouse", "Warehouse"); } }
		}

		public CompetitorTypeList()
		{
			AddPair(Codes.Customs, Descriptions.Customs);
			AddPair(Codes.Forwarding, Descriptions.Forwarding);
			AddPair(Codes.LandTransport, Descriptions.LandTransport);
			AddPair(Codes.Warehouse, Descriptions.Warehouse);
		}
	}
}
