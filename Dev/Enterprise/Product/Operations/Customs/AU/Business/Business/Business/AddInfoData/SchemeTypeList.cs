using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRSchemeList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string GEN = "GEN";
			public const string CA = "CA";
			public const string DC = "DC";
			public const string DCS = "DCS";
			public const string DCT = "DCT";
			public const string EXT = "EXT";
			public const string FI = "FI";
			public const string LDC = "LDC";
			public const string MY = "MY";
			public const string NewZeland = "NZ";
			public const string PapuaNewGuinea = "PG";
			public const string Singapore = "SG";
			public const string Thai = "TH";
			public const string ThaiFTA = "THSS";
			public const string US = "US";
		}

		public static class Descriptions
		{
			public const string CA = "CANADA-AUST";
			public const string DC = "DEVELOPING COUNTRIES-DC RATE";
			public const string DCS = "DEVELOPING COUNTRIES-DCS RATE";
			public const string DCT = "DEVELOPING COUNTRIES-DCT RATE";
			public const string EXT = "EXTERNAL TERRITORIES-NORFOLK, CHRISTMAS, COCOS (KEELING) ISLANDS";
			public const string FI = "FORUM ISLAND COUNTRIES";
			public const string LDC = "LEAST DEVELOPED COUNTRY";
			public const string MY = "MALAYSIA-AUST";
			public const string NewZeland = "NZ-AUST";
			public const string PapuaNewGuinea = "Papua New Guinea";
			public const string Singapore = "SING-AUST";
			public const string Thai = "THAI-AUST";
			public const string ThaiFTA = "THAI-AUST FTA - EN ROUTE SPECIAL SAFEGUARD GOODS";
			public const string US = "USA-AUST";
			public const string GEN = "General";
		}

		public CMRSchemeList()
		{
			AddPair(Codes.GEN, (Descriptions.GEN));
			AddPair(Codes.CA, (Descriptions.CA));
			AddPair(Codes.DC, (Descriptions.DC));
			AddPair(Codes.DCS, (Descriptions.DCS));
			AddPair(Codes.DCT, (Descriptions.DCT));
			AddPair(Codes.EXT, (Descriptions.EXT));
			AddPair(Codes.FI, (Descriptions.FI));
			AddPair(Codes.LDC, (Descriptions.LDC));
			AddPair(Codes.MY, (Descriptions.MY));
			AddPair(Codes.NewZeland, (Descriptions.NewZeland));
			AddPair(Codes.PapuaNewGuinea, (Descriptions.PapuaNewGuinea));
			AddPair(Codes.Singapore, (Descriptions.Singapore));
			AddPair(Codes.Thai, (Descriptions.Thai));
			AddPair(Codes.ThaiFTA, (Descriptions.ThaiFTA));
			AddPair(Codes.US, (Descriptions.US));
		}
	}
}
