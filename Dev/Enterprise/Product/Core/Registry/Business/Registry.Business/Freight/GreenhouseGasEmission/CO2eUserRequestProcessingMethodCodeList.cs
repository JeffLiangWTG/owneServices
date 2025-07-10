using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public class CO2eUserRequestProcessingMethodCodeList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string Ehub = "EHUB";
			public const string Api = "API";
		}

		public static class Descriptions
		{
			public static MultilingualString Ehub { get { return ResString.GetMultilingualString("CO2eUserRequestProcessingMethodCodeList|Ehub", "eHub"); } }
			public static MultilingualString Api { get { return ResString.GetMultilingualString("CO2eUserRequestProcessingMethodCodeList|Api", "API"); } }
		}

		public CO2eUserRequestProcessingMethodCodeList()
		{
			AddPair(Codes.Api, Descriptions.Api);
			AddPair(Codes.Ehub, Descriptions.Ehub);
		}
	}
}
