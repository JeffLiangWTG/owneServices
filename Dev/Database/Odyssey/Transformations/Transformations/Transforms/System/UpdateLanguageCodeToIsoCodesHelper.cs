using System.Collections.Generic;

namespace Enterprise.DbUpgrader.Transformation.DataModification
{
	public static class UpdateLanguageCodeToIsoCodesHelper
	{
		public static Dictionary<string, string> LanguageCodeMapping => new Dictionary<string, string>
		{
			{ "ENG", "EN" }, { "AFK", "AF-ZA" }, { "ALB", "SQ-AL" }, { "ARB", "AR-AE" }, { "ARM", "HY-AM" },
			{ "AZR", "AZ-AZ" }, { "BSQ", "EU-ES" }, { "BLR", "BE-BY" }, { "BLG", "BG-BG" }, { "BEN", "BN-BD" },
			{ "BOS", "BS-BA" }, { "BUR", "MY-MM" }, { "CAT", "CA-ES" }, { "CHS", "ZH-CN" }, { "CHT", "ZH-TW" },
			{ "CRT", "HR-HR" }, { "CZE", "CS-CZ" }, { "DAN", "DA-DK" }, { "DIV", "DV-MV" }, { "DCH", "NL-NL" },
			{ "DZO", "DZ-BT" }, { "EUS", "EN-US" }, { "EGB", "EN-GB" }, { "EST", "ET-EE" }, { "FAE", "FO-FO" },
			{ "FRS", "FA-IR" }, { "FIN", "FI-FI" }, { "FRN", "FR-FR" }, { "GAL", "GL-ES" }, { "GRG", "KA-GE" },
			{ "GRM", "DE-DE" }, { "GRK", "EL-GR" }, { "GUJ", "GU-IN" }, { "HBW", "HE-IL" }, { "HND", "HI-IN" },
			{ "HUN", "HU-HU" }, { "ICE", "IS-IS" }, { "IND", "ID-ID" }, { "ITL", "IT-IT" }, { "JPN", "JA-JP" },
			{ "KAN", "KN-IN" }, { "KAZ", "KK-KZ" }, { "KHM", "KM-KH" }, { "KNK", "KOK-IN" }, { "KOR", "KO-KR" },
			{ "KYR", "KY-KG" }, { "LAO", "LO-LA" }, { "LTV", "LV-LV" }, { "LTH", "LT-LT" }, { "MAC", "MK-MK" },
			{ "MAL", "MS-MY" }, { "MAR", "MR-IN" }, { "MNG", "MN-MN" }, { "NOR", "NB-NO" }, { "PUS", "PS-AF" },
			{ "POL", "PL-PL" }, { "PRT", "PT-PT" }, { "PBR", "PT-BR" }, { "PJB", "PA-IN" }, { "ROM", "RO-RO" },
			{ "RSN", "RU-RU" }, { "SAN", "SA-IN" }, { "SER", "SR-BA" }, { "SLK", "SK-SK" }, { "SLN", "SL-SI" },
			{ "SLA", "ES-LA" }, { "SPN", "ES-ES" }, { "SWA", "SW-KE" }, { "SWE", "SV-SE" }, { "SYR", "SYR-SY" },
			{ "TAM", "TA-IN" }, { "TAT", "TT-RU" }, { "TEL", "TE-IN" }, { "TRK", "TR-TR" }, { "THA", "TH-TH" },
			{ "UKR", "UK-UA" }, { "URD", "UR-PK" }, { "UZB", "UZ-UZ" }, { "VTN", "VI-VN" }
		};
	}
}
