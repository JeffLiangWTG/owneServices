namespace Enterprise.Customs.CL.Manifest.Business;

[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is not a database field")]
public static class WrappersConstants
{
	internal const string EmptyString = "";

	internal const string DateFormatLong = "dd-MM-yyyy hh:mm";

	internal const string DateFormatShort = "dd-MM-yyyy";

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Weight Unit Code")]
	public static class WeightUnitCode
	{
		public const string Lbr = "Lbr";
		public const string Tne = "Tne";
		public const string Hgm = "Hgm";
		public const string Grm = "Grm";
		public const string Ktn = "Ktn";
		public const string Onz = "Onz";
		public const string Kgm = "Kgm";
		public const string Stn = "Stn";
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Volume Unit Code")]
	public static class VolumeUnitCode
	{
		public const string Cmq = "Cmq";
		public const string Ftq = "Ftq";
		public const string Inq = "Inq";
		public const string Ltr = "Ltr";
		public const string Mtq = "Mtq";
		public const string Gli = "Gli";
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Length Unit Code")]
	public static class LengthUnitCode
	{
		public const string Cmt = "Cmt";
		public const string Fot = "Fot";
		public const string Inh = "Inh";
		public const string Ktn = "Ktn";
		public const string Mtr = "Mtr";
		public const string Uni = "Uni";
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Operation Type")]
	public static class OperationType
	{
		public const string I = "I";
		internal const string S = "S";
		internal const string Tr = "Tr";
		internal const string Trb = "Trb";
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Reference Type")]
	public static class ReferenceType
	{
		public const string Ref = "Ref";
		public const string Madre = "Madre";
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Document Type")]
	public static class DocumentType
	{
		public const string Mfto = "Mfto";
		public const string Mftoa = "Mftoa";
		public const string Bl = "Bl";
		public const string Ga = "Ga";
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Action Type")]
	public static class ActionType
	{
		public const string A = "A";
		public const string I = "I";
		public const string M = "M";
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Observation Name")]
	public static class ObservationName
	{
		public const string Gral = "Gral";
		public const string Mot = "Mot";
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Location Name")]
	public static class LocationName
	{
		public const string Le = "Le";
		public const string Pe = "Pe";
		public const string Pd = "Pd";
		public const string Ld = "Ld";
		public const string Lem = "Lem";
		public const string Lrm = "Lrm";
		public const string Trb = "Trb";
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Date Name")]
	public static class DateName
	{
		public const string Fpres = "Fpres";
		public const string Fem = "Fem";
		public const string Fzarpe = "Fzarpe";
		public const string Femb = "Femb";
		public const string Farribo = "Farribo";
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Participation Type")]
	public static class ParticipationType
	{
		public const string Fpres = "Fpres";
		public const string Fem = "Fem";
		public const string Fzarpe = "Fzarpe";
		public const string Femb = "Femb";
		public const string Farribo = "Farribo";
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Participation Name")]
	public static class ParticipationName
	{
		public const string Alm = "Alm";
		public const string Caer = "Caer";
		public const string Cnte = "Cnte";
		public const string Cons = "Cons";
		public const string Emb = "Emb";
		public const string Emi = "Emi";
		public const string Emido = "Emido";
		public const string Noti = "Noti";
		public const string NotiTwo = "Noti2";
		public const string Rep = "Rep";
	}
}
