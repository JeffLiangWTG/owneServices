namespace Enterprise.Customs.BE.Business;

public static class UniversalReferenceConstants
{
	public const string TransportModeAir = "AIR";
	public const string TransportModeSea = "SEA";

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Attribute Code in ZZ DB")]
	public const string DALocatie = "D&A Locatie";
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Attribute Code in ZZ DB")]
	public const string LocatieInLuchtHaven = "Luchthaven";
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Attribute Code in ZZ DB")]
	public const string LocatieInZeeHaven = "Zeehaven";
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Attribute Code in ZZ DB")]
	public const string Pakhuis = "Pakhuis";
	public const string Export = "EXP"; // Attribute Type in ZZ DB

	public const string Role = "ROLE"; // Attribute Type in ZZ DB
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Attribute Type in ZZ DB")]
	public const string Kantoor = "Kantoor";
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Attribute Type in ZZ DB")]
	public const string Type = "Type";
	public const string SubType = "SubType"; // Attribute Type in ZZ DB

	public static string[] GetValidLocationOfGoodsTypeAir() => new string[] { DALocatie, LocatieInLuchtHaven, Pakhuis };
	public static string[] GetValidLocationOfGoodsTypeSea() => new string[] { DALocatie, LocatieInZeeHaven, Pakhuis };
	public static string[] GetValidLocationOfGoodsTypeOther() => new string[] { DALocatie, LocatieInLuchtHaven, LocatieInZeeHaven, Pakhuis };

	public static string[] ValidLocationOfGoods(string transportModeInland) =>
		transportModeInland == TransportModeAir ? GetValidLocationOfGoodsTypeAir() :
		transportModeInland == TransportModeSea ? GetValidLocationOfGoodsTypeSea() :
		GetValidLocationOfGoodsTypeOther();

	public static class RefCusCodeListTypes
	{
		public static class Codes
		{
			public const string Code_AA44E = "AA44E";
			public const string Code_AI44E = "AI44E";
			public const string Code_AI44I = "AI44I";
			public const string Code_AR44E = "AR44E";
			public const string Code_AR44I = "AR44I";
			public const string Code_C0754 = "C0754";
			public const string Code_CURRE = "CURRE";
			public const string Code_DC40E = "DC40E";
			public const string Code_TD44E = "TD44E";
			public const string Code_TD44I = "TD44I";
			public const string Code_IM17 = "IM17";
			public const string Code_EX17 = "EX17";
		}
	}

	public static class RefCusCodeListAttributes
	{
		public static class Name
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Attribute Name in ZZ DB")]
			public const string Reference = "Reference";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Attribute Name in ZZ DB")]
			public const string Value = "Value";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Attribute Name in ZZ DB")]
			public const string Detail = "Detail";
		}

		public static class Value
		{
			public const string Yes = "Y";
			public const string No = "N";
		}
	}
}
