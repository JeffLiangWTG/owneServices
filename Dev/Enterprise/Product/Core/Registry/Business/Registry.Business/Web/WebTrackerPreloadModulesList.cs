using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public class WebTrackerPreloadModulesList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string Accounts = "ACC";
			public const string Admin = "ADM";
			public const string Bookings = "BKG";
			public const string Cartage = "CRT";
			public const string CFSShipments = "CFS";
			public const string Containers = "CNT";
			public const string Declaration = "DCL";
			public const string ImporterSecurityFiling = "ISF";
			public const string LinerAndAgency = "LAA";
			public const string Orders = "ORD";
			public const string Quotes = "QUO";
			public const string Reports = "RPT";
			public const string Schedules = "SCH";
			public const string Shipments = "SHP";
			public const string Terms = "TRM";
			public const string Warehousing = "WHS";
		}

		public WebTrackerPreloadModulesList()
		{
			AddPair(Codes.Accounts, ResString.GetMultilingualString("61289900-4472-4AF4-80E8-BF2396FC824F", "Accounts"));
			AddPair(Codes.Admin, ResString.GetMultilingualString("85BDEE54-280E-4DB3-B156-B765476E15EB", "Admin"));
			AddPair(Codes.Bookings, ResString.GetMultilingualString("E18A034B-C06B-47B8-89AE-5628814D87CF", "Bookings"));
			AddPair(Codes.CFSShipments, ResString.GetMultilingualString("526E91F6-5B56-4CAA-A77C-CC6A36A41DA1", "CFS Shipments"));
			AddPair(Codes.Containers, ResString.GetMultilingualString("32717AF6-DB5F-44A5-8CAE-872DF3516736", "Containers"));
			AddPair(Codes.Declaration, ResString.GetMultilingualString("F9041E3E-059E-476F-B8EE-76A399EBD202", "Declarations"));
			AddPair(Codes.ImporterSecurityFiling, ResString.GetMultilingualString("11AF3848-5AAC-46CE-9E63-4C48D2367D6A", "Importer Security Filing"));
			AddPair(Codes.LinerAndAgency, ResString.GetMultilingualString("AE7E56F4-2745-46AA-B918-82A8E168A262", "Liner and Agency"));
			AddPair(Codes.Orders, ResString.GetMultilingualString("9A94CBBD-BCC4-483A-B4E5-307D3E12B81E", "Orders"));
			AddPair(Codes.Cartage, ResString.GetMultilingualString("D51FF140-607B-4D4F-8B96-5E73CCC7130E", "Port Transport"));
			AddPair(Codes.Reports, ResString.GetMultilingualString("5DC00BE2-93D4-4741-9DD0-78A4DBBD451B", "Reports"));
			AddPair(Codes.Schedules, ResString.GetMultilingualString("FF028335-9D70-4707-9C3D-B1053F1CF132", "Schedules"));
			AddPair(Codes.Shipments, ResString.GetMultilingualString("7E93BAB1-CAFB-4C85-BDCC-3430F8417CCD", "Shipments"));
			AddPair(Codes.Quotes, ResString.GetMultilingualString("A48B91DA-A077-473D-808B-EFE3F1819930", "Spot Quotes"));
			AddPair(Codes.Terms, ResString.GetMultilingualString("0C014B90-BB93-4A0F-B37E-2407CD2FB249", "Terms"));
			AddPair(Codes.Warehousing, ResString.GetMultilingualString("0A0E98BF-4D4E-4521-9500-4407F7ABE536", "Warehouse"));
		}
	}
}
