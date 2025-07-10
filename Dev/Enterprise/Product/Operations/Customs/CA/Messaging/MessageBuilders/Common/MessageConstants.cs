
namespace Enterprise.Customs.CA.Messaging
{
	public static class MessageConstants
	{
		public static class ModeOfTransportMessageCodes
		{
			public const string Marine = "1";
			public const string Rail = "2";
			public const string Highway = "3";
			public const string Air = "4";
		}

		public static class CustomsProcedureCodes
		{
			public const string ImportedGoods = "24";
			public const string InTransit = "23";
			public const string FROB = "26";
		}

		public static class CBSAWeightUnits
		{
			public const string MetricTon = "TNE";
			public const string Kilogram = "KGM";
			public const string Pound = "LBR";
			public const string MetricCarat = "CTM";
			public const string Milligram = "MGM";
			public const string Gram = "GRM";
			public const string Hectogram = "HGM";
			public const string KilogramofNamedSubstance = "KNS";
			public const string Kilogram90PercentAirDry = "KSD";
			public const string Deciton = "DTN";
			public const string MetricTonAirDry = "TSD";
			public const string Kiloton = "KTN";
			public const string Gigabecquerel = "GBQ";
			public const string Megabecquerel = "MBQ";
		}

		public static class ACIVolumeUnits
		{
			public const string CubicCentimetre = "C";
			public const string Cord = "D";
			public const string CubicFeet = "E";
			public const string BoardFoot100 = "F";
			public const string GallonsUK = "G";
			public const string HundredsTTTons = "H";
			public const string GallonsUSDry = "I";
			public const string GallonsUSLiquid = "J";
			public const string HundredsTTTonsShort = "K";
			public const string Load = "L";
			public const string CubicDecimetre = "M";
			public const string CubicInches = "N";
			public const string TonShort = "P";
			public const string TonMetric = "Q";
			public const string Car = "R";
			public const string TonLong = "S";
			public const string VolumetricUnit = "U";
			public const string Litre = "V";
			public const string CubicMeters = "X";
			public const string Barge = "B";
			public const string Container = "T";
			public const string LoadForEnterprise = "LD";
		}

		public static class UniversalShipmentMessageConstants
		{
			public const string MessageRefNumberPlaceHolder = "__MessageRefNumberPlaceHolder__";
			public const string TotalTransactionValue = "TotalTransactionValue";
			public const string ClientNetworkID = "ClientNetworkID";
		}

		public static class B3RecordIdentifiers
		{
			public const string Positive = "POS";
			public const string Negative = "NEG";
		}
	}
}
