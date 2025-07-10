namespace Enterprise.Customs.MX.Manifest.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "HBL Message")]
	public static class AWBRequestConstants
	{
		public const string MessageName = "House Waybill";
		public const string MessageVersion = "3.00";
		public const string StreetsSeparator = " ";
		public const string TransshipmentTransportMode = "Pre-Carriage";
		public const string ImportExportTransportMode = "On-Carriage";
		public const string TypeCodeDefault = "Item703";
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Amendment Manifest from Main Menu, Cancel Manifest from Main Menu, Send Manifest from Main Menu")]
	public static class MessageActions
	{
		public const string Amendment = "Update";
		public const string Delete = "Deletion";
		public const string New = "Creation";
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Weight Unit Code")]
	public static class WeightUnitCode
	{
		public const string Lbr = "Lbr";
		public const string Tne = "Tne";
		public const string Hgm = "Hgm";
		public const string Grm = "Grm";
		public const string Ktn = "Ktn";
		public const string Onz = "Onz";
		public const string Dtn = "Dtn";
		public const string Ltn = "Ltn";
		public const string Mgm = "Mgm";
		public const string Ctm = "Ctm";
		public const string Kgm = "Kgm";
		public const string Stn = "Stn";
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Weight Unit Code")]
	public static class VolumeUnitCode
	{
		public const string Cmq = "Cmq";
		public const string Ftq = "Ftq";
		public const string Inq = "Inq";
		public const string Ltr = "Ltr";
		public const string Dmq = "Dmq";
		public const string Ydq = "Ydq";
		public const string Mal = "Mal";
		public const string Gll = "Gll";
		public const string Mtq = "Mtq";
	}
}
