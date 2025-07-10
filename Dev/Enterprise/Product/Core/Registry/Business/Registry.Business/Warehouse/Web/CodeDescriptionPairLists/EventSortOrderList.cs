using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business.Web
{
	public class EventSortOrderList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string Chronological = CargoWise.Definitions.EventSortOrderList.Chronological;
			public const string ReversedChronological = CargoWise.Definitions.EventSortOrderList.ReversedChronological;
		}

		public static class Descriptions
		{
			public static MultilingualString Chronological { get { return ResString.GetMultilingualString("EventSortOrderList|Chronological", "Show events in chronological order"); } }
			public static MultilingualString ReversedChronological { get { return ResString.GetMultilingualString("EventSortOrderList|ReversedChronological", "Show events in reverse chronological order"); } }
		}

		public EventSortOrderList()
		{
			AddPair(Codes.Chronological, Descriptions.Chronological);
			AddPair(Codes.ReversedChronological, Descriptions.ReversedChronological);
		}
	}
}
