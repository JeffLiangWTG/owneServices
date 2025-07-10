using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public static class WebRegistryDateFormats
	{
		public const string Standard = "STD";
		public const string WebUserLocale = "LOC";
	}

	public class DateFormatCodeDescriptionPairList : CodeDescriptionPairList
	{
		public DateFormatCodeDescriptionPairList()
		{
			AddPair(WebRegistryDateFormats.Standard, ResString.GetMultilingualString("b1376265-65d6-439d-b885-4a5df4f25486", "DD-MMM-YY in English"));
			AddPair(WebRegistryDateFormats.WebUserLocale, ResString.GetMultilingualString("7350f036-2ba3-4a6b-827a-53df373549de", "DD-MMM-YY in locale of web user"));
		}
	}
}
