namespace Enterprise.Customs.MX.Manifest.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant text")]
	internal static class SEA309Constants
	{
		internal const string DoubleZero = "00";
		internal const string DoubleZ = "ZZ";
		internal const string SendToTest = "CUSTOMSTS";
		internal const string SendToProd = "CUSTOMS";
		internal const string Separator = "U";
		internal const string ControlVersion = "00605";
		internal const string ACKRequest = "0";
		internal const string UsageIndicator = "P";
		internal const string ComponentElementSeparator = ":";
		internal const string FunctionalIdentifierCode = "SO";
		internal const string AgencyCode = "X";
		internal const string VersionGS = "006050";
		internal const string TrasactionSetIdentifierCode = "309";
		internal const string TypeCode = "O";
		internal const string VesselCodeQualifier = "L";
		internal const string NatureImpo = "SI";
		internal const string NatureExpo = "SE";
		internal const string IncludedGroups = "1";
		internal const string NewManifest = "W"; // Send Manifest from Main Menu
		internal const string DeleteManifest = "D"; // Cancel Manifest from Main Menu
		internal const string AmendmentManifest = "R"; // Amendment Manifest from Main Menu
		internal const string OldManifest = "Y";
		internal const string HBLCode = "30";
		internal const string HBLReference = "HO";
		internal const string ExpReference = "MXE";
		internal const string ImpReference = "MXI";
		internal const string CommodityCode = "J";
		internal const string UnitorMeasurementCode = "CE";
		internal const string To = "to";
		internal const char Minus = '-';
		internal const string ContainerName = "ANLO";
		internal const string ContainerInitials = "AC";
	}

	internal static class VolumeConstants
	{
		internal const string CubicCentimeters = "C";
		internal const string CubicFeet = "E";
		internal const string USGallons = "G";
		internal const string CubicDecimetres = "M";
		internal const string CubicInches = "N";
		internal const string Litre = "L";
		internal const string CubicMetres = "X";
	}

	internal static class WeightConstants
	{
		internal const string Kilograms = "K";
		internal const string Pounds = "L";
		internal const string Tonnes = "E";
		internal const string LongTons = "T";
		internal const string ShortTones = "S";
	}

	internal static class Parties
	{
		internal const string ShipperCode = "SH";
		internal const string ConsigneeCode = "TIN";
		internal const string NotifyCode = "N1";
		internal const string ContactFunction = "IC";
		internal const string Communication = "WP";
	}

	internal static class EmptyFullIndicator
	{
		internal const string Empty = "E";
		internal const string Loaded = "L";
		internal const string FullContainer = "F";
	}
}
