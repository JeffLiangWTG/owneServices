using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business.Web
{
	public class DuplicateEventHandlingMethodList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string ShowAll = CargoWise.Definitions.DuplicateEventHandlingMethodList.ShowAll;
			public const string ShowFirst = CargoWise.Definitions.DuplicateEventHandlingMethodList.ShowFirst;
			public const string ShowLast = CargoWise.Definitions.DuplicateEventHandlingMethodList.ShowLast;
		}

		public static class Descriptions
		{
			public static MultilingualString ShowAll { get { return ResString.GetMultilingualString("DuplicateEventHandlingMethodList|ShowAll", "Show All"); } }
			public static MultilingualString ShowFirst { get { return ResString.GetMultilingualString("DuplicateEventHandlingMethodList|ShowFirst", "Show First"); } }
			public static MultilingualString ShowLast { get { return ResString.GetMultilingualString("DuplicateEventHandlingMethodList|ShowLast", "Show Last"); } }
		}

		public DuplicateEventHandlingMethodList()
		{
			AddPair(Codes.ShowAll, Descriptions.ShowAll);
			AddPair(Codes.ShowFirst, Descriptions.ShowFirst);
			AddPair(Codes.ShowLast, Descriptions.ShowLast);
		}
	}
}
