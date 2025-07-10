using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public static class AutoratingViaPortHelper
	{
		public static class JobType
		{
			public static class Code
			{
				public const string ForwardingConsol = "FCN";
				public const string Shipment = "SHP";
				public const string QuotedBooking = "QSH";
			}

			public static CodeDescriptionPair ForwardingConsol =>
				new CodeDescriptionPair(
					Code.ForwardingConsol,
					ResString.GetMultilingualString("0AC40633-0AE0-4E5C-85A6-534D451A415C", "Consol"));

			public static CodeDescriptionPair Shipment =>
				new CodeDescriptionPair(
					Code.Shipment,
					ResString.GetMultilingualString("3618B02E-1609-4D0B-AB71-AE5E1831C42F", "Shipment"));

			public static CodeDescriptionPair QuotedBooking =>
				new CodeDescriptionPair(
					Code.QuotedBooking,
					ResString.GetMultilingualString("E59C26D4-D25D-4AF4-9ECF-317F37A45ADE", "Quick Booking"));
		}

		public static class DirectionOption
		{
			public static class Code
			{
				public const string All = "ALL";
				public const string Export = "EXP";
				public const string Import = "IMP";
			}

			public static CodeDescriptionPair All =>
				new CodeDescriptionPair(
					Code.All,
					ResString.GetMultilingualString("EA5234D5-287D-4E9D-ACD3-5900CD2AB8EC", "All Directions"));

			public static CodeDescriptionPair Export =>
				new CodeDescriptionPair(
					Code.Export,
					ResString.GetMultilingualString("10DB88DB-CE7E-43EC-B0DB-0F9B3385F384", "Export"));

			public static CodeDescriptionPair Import =>
				new CodeDescriptionPair(
					Code.Import,
					ResString.GetMultilingualString("E502F6C0-C5EF-4DC8-9FA0-91F21232AB8B", "Import"));
		}

		public static class LocationSourceOption
		{
			public static class Code
			{
				public const string FirstLoad = "1L";
				public const string NotFirstLoad = "N1L";
				public const string VoyageLoad = "VL";
				public const string NotVoyageLoad = "NVL";
				public const string LastDischarge = "LD";
				public const string NotLastDischarge = "NLD";
				public const string VoyageDischarge = "VD";
				public const string NotVoyageDischarge = "NVD";
				public const string LastModeRouteSetDischarge = "LRD";
			}

			public static CodeDescriptionPair FirstLoad =>
				new CodeDescriptionPair(
					Code.FirstLoad,
					ResString.GetMultilingualString("91FED75B-297F-4AE5-9FB6-F1B816737CA4", "1st Load"));

			public static CodeDescriptionPair NotFirstLoad =>
				new CodeDescriptionPair(
					Code.NotFirstLoad,
					ResString.GetMultilingualString("230BE663-FF47-40A4-ABAD-24C30784E828", "≠ 1st Load"));

			public static CodeDescriptionPair VoyageLoad =>
				new CodeDescriptionPair(
					Code.VoyageLoad,
					ResString.GetMultilingualString("7A884CD7-8A5D-4114-8F2E-E35F2641D8B0", "Voyage Load"));

			public static CodeDescriptionPair NotVoyageLoad =>
				new CodeDescriptionPair(
					Code.NotVoyageLoad,
					ResString.GetMultilingualString("C473F226-696D-4CDE-B1BD-3CFC9D2B1274", "≠ Voyage Load"));

			public static CodeDescriptionPair LastDischarge =>
				new CodeDescriptionPair(
					Code.LastDischarge,
					ResString.GetMultilingualString("B1C386FE-4A12-4C9A-8588-CDBA63D94B95", "Last Discharge"));

			public static CodeDescriptionPair NotLastDischarge =>
				new CodeDescriptionPair(
					Code.NotLastDischarge,
					ResString.GetMultilingualString("D12C0506-3CBE-4D93-B4AB-B304B3E6B089", "≠ Last Discharge"));

			public static CodeDescriptionPair VoyageDischarge =>
				new CodeDescriptionPair(
					Code.VoyageDischarge,
					ResString.GetMultilingualString("AED0DB98-B61C-40B2-A3FB-7CAAB27D3317", "Voyage Discharge"));

			public static CodeDescriptionPair NotVoyageDischarge =>
				new CodeDescriptionPair(
					Code.NotVoyageDischarge,
					ResString.GetMultilingualString("45C3095D-33C5-41B6-8F12-97F151312CC7", "≠ Voyage Discharge"));

			public static CodeDescriptionPair LastModeRouteSetDischarge =>
				new CodeDescriptionPair(
					Code.LastModeRouteSetDischarge,
					ResString.GetMultilingualString("4FCFB34F-4F38-4B11-AAE6-B957FDF2468B", "Last Mode & Route Set Discharge"));
		}
	}
}
