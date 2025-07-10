using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public class ThemeCodeDescriptionPairList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string STD = "STD";
			public const string ALT = "ALT";
			public const string CLS = "CLS";
			public const string CUS = "CUS";
		}

		public static class Descriptions
		{
			public static MultilingualString STD { get { return ResString.GetMultilingualString("aed99538-2d16-4dea-9adf-76a55033eccb", "Standard"); } }
			public static MultilingualString ALT { get { return ResString.GetMultilingualString("963338ba-c7d6-4c5d-b3fc-d48ac8e2c2aa", "Alternate"); } }
			public static MultilingualString CLS { get { return ResString.GetMultilingualString("565f94d6-bf05-4da1-b9dc-ed7584601097", "Classic"); } }
			public static MultilingualString CUS { get { return ResString.GetMultilingualString("8d67aee8-3695-420e-945e-acc7d4306096", "Custom"); } }
		}

		public ThemeCodeDescriptionPairList()
		{
			AddPair(Codes.STD, Descriptions.STD);
			AddPair(Codes.ALT, Descriptions.ALT);
			AddPair(Codes.CLS, Descriptions.CLS);
			AddPair(Codes.CUS, Descriptions.CUS);
		}
	}
}
