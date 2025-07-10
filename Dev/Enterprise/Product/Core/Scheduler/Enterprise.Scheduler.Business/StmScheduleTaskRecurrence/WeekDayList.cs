using Enterprise.ZArchitecture.Core;

namespace Enterprise.Scheduler.Business
{
	public partial class WeekDayList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string Sunday = "SUN";
			public const string Monday = "MON";
			public const string Tuesday = "TUE";
			public const string Wednesday = "WED";
			public const string Thursday = "THU";
			public const string Friday = "FRI";
			public const string Saturday = "SAT";
		}

		public static class Descriptions
		{
			public const string Sunday = "1";
			public const string Monday = "2";
			public const string Tuesday = "3";
			public const string Wednesday = "4";
			public const string Thursday = "5";
			public const string Friday = "6";
			public const string Saturday = "7";
		}

		public WeekDayList()
		{
			AddPair(ResString.GetMultilingualString("WeekDay|SUN", Codes.Sunday), Descriptions.Sunday);
			AddPair(ResString.GetMultilingualString("WeekDay|MON", Codes.Monday), Descriptions.Monday);
			AddPair(ResString.GetMultilingualString("WeekDay|TUE", Codes.Tuesday), Descriptions.Tuesday);
			AddPair(ResString.GetMultilingualString("WeekDay|WED", Codes.Wednesday), Descriptions.Wednesday);
			AddPair(ResString.GetMultilingualString("WeekDay|THU", Codes.Thursday), Descriptions.Thursday);
			AddPair(ResString.GetMultilingualString("WeekDay|FRI", Codes.Friday), Descriptions.Friday);
			AddPair(ResString.GetMultilingualString("WeekDay|SAT", Codes.Saturday), Descriptions.Saturday);
		}
	}
}
