namespace Enterprise.Customs.DE.Business
{
	public static class UniversalReferenceConstants
	{
		public static class RefCusCodeList
		{
			public static class Codes
			{
				public const string Code_00700 = "00700";
				public const string Code_00800 = "00800";
				public const string Code_00900 = "00900";
				public const string Code_9ZZX = "9ZZX";
				public const string Code_9ZZY = "9ZZY";
				public const string Code_9ZZZ = "9ZZZ";
				public const string Code_C019 = "C019";
				public const string Code_C516 = "C516";
				public const string Code_C601 = "C601";
				public const string Code_C626 = "C626";
				public const string Code_C627 = "C627";
				public const string Code_C651 = "C651";
				public const string Code_C658 = "C658";
				public const string Code_Y015 = "Y015";

				public const string ValuationCode_12 = "12";
			}
		}

		public static class RefCusCodeListTypes
		{
			public static class Codes
			{
				public const string Code_AA44E = "AA44E";
				public const string Code_AI44E = "AI44E";
				public const string Code_AR44E = "AR44E";
				public const string Code_C0754 = "C0754";
				public const string Code_CURRE = "CURRE";
				public const string Code_DC40E = "DC40E";
				public const string Code_TD44E = "TD44E";
			}
		}

		public static class TaxesOrFees
		{
			public static class Types
			{
				public const string TobaccoRetailSellingPrice = "TSP";
			}

			public static class Codes
			{
				public const string DV1Value = "DV1";
				public const string TobaccoRetailSellingPricePerUnit = "CIG";
				public const string TobaccoRetailSellingPricePerKilo = "TAB";
			}
		}

		public static class RefCusCodeListAttributes
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Attribute Name in ZZ DB")]
			public static class Name
			{
				public const string Division = "Division";
				public const string Reference = "Reference";
				public const string Complement = "Complement";
				public const string Detail = "Detail";
				public const string IssuingDate = "IssuingDate"; // Attribute Name in ZZ DB
				public const string ValidityDate = "ValidityDate"; // Attribute Name in ZZ DB
				public const string Value = "Value";
				public const string Unit = "Unit";
				public const string ComplementaryUnit = "ComplementaryUnit"; // Attribute Name in ZZ DB
				public const string MeasurementUnit = "MeasurementUnit"; // Attribute Name in ZZ DB
				public const string Authority = "Authority";
				public const string ItemNumber = "ItemNumber"; // Attribute Name in ZZ DB
				public const string Level = "Level";
				public const string A1150 = "A1150";
				public const string C0091 = "C0091";
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Attribute Value in ZZ DB")]
			public static class Value
			{
				public const string Certificates = "1";
				public const string ImportLegalPapers = "2";
				public const string ProofOfPreferentialStatus = "3";
				public const string MiscellaneousDocument = "4";
				public const string ExemptionsExplanations = "5";
				public const string ProofOfCommunityStatusOrStatusOfGoodsInFreeCirculation = "6";
				public const string Yes = "Y";
				public const string No = "N";
				public const string Header = "Header";
				public const string Item = "Item";
			}
		}

		public static class CustomsProcedureCodes
		{
			public const string _4054 = "4054";
			public const string _4254 = "4254";
		}

		public static class CountryCodes
		{
			public const string CountryCodeQQ = "QQ";
			public const string CountryCodeQR = "QR";
			public const string CountryCodeQS = "QS";
			public const string CountryCodeQU = "QU";
			public const string CountryCodeQV = "QV";
			public const string CountryCodeQP = "QP";
			public const string CountryCodeQW = "QW";
			public const string CountryCodeQX = "QX";
			public const string CountryCodeQY = "QY";
			public const string CountryCodeQZ = "QZ";
		}

		public static class Concessions
		{
			public const string C07 = "C07";
			public const string F48 = "F48";
			public const string F61 = "F61";
			public const string F62 = "F62";
			public const string F64 = "F64";
			public const string _6F0 = "6F0";
			public const string _8E3 = "8E3";
		}

		public static class SupportingDocumentTypes
		{
			public const string _3LLA231 = "3LLA231";
			public const string _9DAB = "9DAB";
			public const string _9DEE = "9DEE";
			public const string _9DFC = "9DFC";
			public const string _9DFD = "9DFD";
			public const string _9DFE = "9DFE";
			public const string _9ZZX = "9ZZX";
			public const string _9ZZY = "9ZZY";
			public const string C034 = "C034";
			public const string C612 = "C612";
			public const string C613 = "C613";
			public const string C614 = "C614";
			public const string C626 = "C626";
			public const string C627 = "C627";
			public const string C651 = "C651";
			public const string C990 = "C990";
			public const string D019 = "D019";
			public const string N720 = "N720";
			public const string N820 = "N820";
			public const string N821 = "N821";
			public const string N822 = "N822";
			public const string N830 = "N830";
			public const string N952 = "N952";
			public const string N955 = "N955";
			public const string N990 = "N990";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "constant document Type")]
			public const string Nzzz = "Nzzz";
			public const string X001 = "X001";
			public const string Y015 = "Y015";
		}

		public static class AgreedPlaceCodes
		{
			public const string _1 = "1";
			public const string _3 = "3";
		}

		public static class EntryStatus
		{
			public const string CCM = "CCM";
			public const string CON = "CON";
			public const string CTL = "CTL";
			public const string CWT = "CWT";
			public const string DFC = "DFC";
			public const string DIP = "DIP";
			public const string DWH = "DWH";
			public const string ERR = "ERR";
			public const string FUP = "FUP";
			public const string PIP = "PIP";
			public const string PFC = "PFC";
			public const string PWH = "PWH";
			public const string RC1 = "RC1";
			public const string RC2 = "RC2";
			public const string REJ = "REJ";
			public const string REV = "REV";
			public const string RL1 = "RL1";
			public const string RL2 = "RL2";
			public const string RL3 = "RL3";
			public const string RL4 = "RL4";
			public const string RL5 = "RL5";
			public const string RLB = "RLB";
			public const string SFC = "SFC";
			public const string SIP = "SIP";
			public const string SWH = "SWH";
			public const string TX1 = "TX1";
			public const string TX2 = "TX2";
			public const string TX3 = "TX3";
			public const string TX4 = "TX4";
			public const string TX5 = "TX5";
			public const string TX6 = "TX6";
			public const string TX7 = "TX7";
			public const string TX8 = "TX8";
			public const string TRA = "TRA";
			public const string TXF = "TXF";
			public const string TXR = "TXR";
			public const string URG = "URG";
		}

		public static class ATLASReferenceNumberIdentifier
		{
			public const string ATA = "ATA";
			public const string ATE = "ATE";
			public const string ATD = "ATD";
		}

		public static class AdditionalInfoCodes
		{
			public const string _20300 = "20300";
			public const string T0000 = "T0000";
			public const string X0000 = "X0000";
			public const string X0001 = "X0001";
			public const string X0004 = "X0004";
		}

		public static class CusExitDetailStatus
		{
			public const string _301 = "301";
			public const string _310 = "310";
			public const string _342 = "342";
			public const string _353 = "353";
			public const string _371 = "371";
			public const string _372 = "372";
		}

		public static class CusLineTariffCodes
		{
			public const string _1042 = "1042";
		}

		public static class RefCusConditionTypes
		{
			public const string _420 = "420";
			public const string _465 = "465";
			public const string _474 = "474";
			public const string _475 = "475";
			public const string _477 = "477";
		}

		public static class ValuationCodes
		{
			public const string _32 = "32";
		}

		public static class MethodOfPaymentTypes
		{
			public const string A = "A";
			public const string B = "B";
			public const string C = "C";
			public const string D = "D";
			public const string E = "E";
			public const string F = "F";
			public const string G = "G";
			public const string L = "L";
			public const string S = "S";
			public const string Z = "Z";
		}

		public static class TransportIds
		{
			public const string LKW = "LKW";
		}

		public static class TransportDocumentTypes
		{
			public const string N703 = "N703";
			public const string N740 = "N740";
		}
	}
}
