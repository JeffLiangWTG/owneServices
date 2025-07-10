namespace Enterprise.Customs.FR.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
	public static class UniversalReferenceConstants
	{
		public static class RefCusCodeListTypes
		{
			public static class Codes
			{
				public const string DispositionTariffParticular = "DTP";
				public const string NationalAdditionalCode = "CANA";
				public const string Fallback = "FBK";
				public const string VatCana = "VCANA";
				public const string PortTaxCombination = "PTX";
				public const string ExpectedNextCustomsProcedure = "CL907";
				public const string MotivationForInvalidationRequest = "INVMO";
				public const string MotivationForRectificationRequest = "RECMO";
				public const string FrenchNationalTaxCode = "TAX";
			}
		}

		public static class RefCusCodeList
		{
			public static class ChargeType
			{
				public const string AK = "AK";
				public const string BA = "BA";
				public const string CA = "CA";
				public const string AN = "AN";
				public const string BG = "BG";
				public const string BC = "BC";
				public const string CZ = "CZ";
			}

			public static class CustomsUq
			{
				public static class Volumn
				{
					public const string Decilitre = "001";
					public const string HundredMetresCube = "004";
					public const string Hectolitre = "HLT";
					public const string ThousandLitres = "KLT";
					public const string Litre = "LTR";
					public const string MetreCube = "MTQ";
				}

				public static class Number
				{
					public const string ParTeteDeBetail = "002";
					public const string HundredPieces = "CEN";
					public const string ThousandPieces = "MIL";
					public const string NombreDePieces = "NAR";
					public const string NombreDelements = "NCL";
					public const string NombreDePaires = "NPR";
				}

				public static class Weight
				{
					public const string HundredKgDemiBrut = "003";
					public const string TonneNettDeViandeImporteeDeductionFaiteDuPoidsDesAbats = "007";
					public const string CapaciteDeChargeEnTonnesMetriques = "CCT";
					public const string Carats = "CTM";
					public const string KilogrammeDeDihydrostreptomycine = "DHS";
					public const string Hectokilogramme = "DTN";
					public const string GrammeIsotopeFissile = "GFI";
					public const string Gramme = "GRM";
					public const string KilogrammeDeChlorureDeCholine = "KCC";
					public const string TonneDeChlorureDePotassium = "KCL";
					public const string Kilogramme = "KGM";
					public const string KilogrammeDeMethylamine = "KMA";
					public const string KilogrammeDazote = "KNI";
					public const string KilogrammeDePeroxydeDhydrogene = "KNS";
					public const string KilogrammeDhydroxydeDePotassium = "KPH";
					public const string KilogrammeDoxydeDePotassium = "KPO";
					public const string KilogrammeDePentaoxydeDeDiphosphore = "KPP";
					public const string KilogrammeDeMatiereSecheA90Percent = "KSD";
					public const string KilogrammeDhydroxydeDeSodium = "KSH";
					public const string KilogrammeDuranium = "KUR";
					public const string ThousandPaires = "MPR";
					public const string Tonne = "TNE";
					public const string Tonne3 = "TNE3";
					public const string KilogrammeN = "KGMN";
					public const string TonneN = "TNEN";
				}

				public static class Alcohol
				{
					public const string DecilitreDAlcoolPur = "006";
					public const string PercentageVol = "ASV";
					public const string LitreDalcoolPur = "LPA";
					public const string HectolitreDAlcoolPur = "005";
				}

				public static class Length
				{
					public const string Hectometre = "HMT";
					public const string Kilometre = "KMT";
					public const string Metre = "MTR";
				}

				public static class Surface
				{
					public const string MetreCarre = "MTK";
				}

				public static class Energy
				{
					public const string ThousandKilowattsHeure = "MWH";
					public const string Terajoule = "TJO";
				}
			}

			public static class AdditionalInformationCodes
			{
				public const string UnidentifiedVATLiableInFrance = "G0008";
				public const string FretCargo = "A0010";
				public const string FallbackProcedure = "F5000";
				public const string StandardDeclaration = "E0001";
				public const string AI2WithVisaExemption = "G6090";
				public const string AI2WithoutVisaExemption = "G6100";
				public const string TariffBypassNeedingMotivation = "K0001";
			}

			public static class AdditionalReferenceCodes
			{
				public const string FallbackProcedure = "1PDS";
				public const string PortCode = "1CPT";
			}

			public static class SupportingDocumentsCodes
			{
				public const string T2LDocument = "N825";
				public const string T2LFDocument = "C620";
			}

			public static class PreviousDocumentsCodes
			{
				public const string TemporayStorage = "N337";
				public const string NMRN = "NMRN";
			}
		}

		public static class RefCusCodeListAttributeNames
		{
			public static class Names
			{
				public const string PercentInEu = "PercentInEu";
				public const string PercentOutEU = "PercentOutEU";
				public const string PercentDomestic = "PercentDomestic";
				public const string State = "State";
				public const string CountryCode = "CountryCode";
				public const string VatProcedure = "VatProcedure";
				public const string SpecialMention = "SpecialMention";
				public const string THI = "THI";
				public const string Designation = "Designation";
				public const string Port = "PORT";
				public const string CustomsOffice = "CUSTOMSOFFICE";
				public const string PostCode = "PostCode";
				public const string IsD48 = "IsD48";
				public const string IsDTP = "IsDTP";
				public const string UnitsOfQuantityMapType = "UQMapType";
				public const string IsIntelligentBorder = "IsIntelligentBorder";
				public const string IsODS = "IsODS";
				public const string Remark = "Remark";
			}
		}

		public static class RefCusTariffAdditionalCodeCategories
		{
			public const string SIP = "SIP";
			public const string SEP = "SEP";
		}

		public static class RefCusRateCodes
		{
			public const string StatisticalValueBasis = "STA";
			public const string ATVA = "ATVA";
			public const string A00 = "A00";
			public const string A10 = "A10";
			public const string A20 = "A20";
			public const string A30 = "A30";
			public const string A35 = "A35";
			public const string A40 = "A40";
			public const string A45 = "A45";
			public const string B00 = "B00";
			public const string A325 = "A325";
			public const string A336 = "A336";
			public const string A365 = "A365";
			public const string A375 = "A375";
			public const string A377 = "A377";
			public const string A378 = "A378";
			public const string A385 = "A385";
			public const string A387 = "A387";
			public const string A388 = "A388";
			public const string A395 = "A395";
			public const string A397 = "A397";
			public const string A398 = "A398";
			public const string A400 = "A400";
			public const string A406 = "A406";
			public const string A435 = "A435";
			public const string A445 = "A445";
			public const string A465 = "A465";
			public const string A505 = "A505";
			public const string A735 = "A735";
			public const string A765 = "A765";
			public const string A795 = "A795";
			public const string A825 = "A825";
			public const string B225 = "B225";
			public const string C125 = "C125";
			public const string C255 = "C255";
			public const string C267 = "C267";
			public const string C276 = "C276";
			public const string C295 = "C295";
			public const string C302 = "C302";
			public const string C435 = "C435";
			public const string C455 = "C455";
			public const string C495 = "C495";
			public const string C516 = "C516";
			public const string C682 = "C682";
			public const string C684 = "C684";
			public const string C686 = "C686";
			public const string C688 = "C688";
			public const string C690 = "C690";
			public const string C692 = "C692";
			public const string C694 = "C694";
			public const string C696 = "C696";
			public const string C698 = "C698";
			public const string C700 = "C700";
			public const string C702 = "C702";
			public const string C704 = "C704";
			public const string C706 = "C706";
			public const string C845 = "C845";
			public const string D285 = "D285";
			public const string D325 = "D325";
			public const string D435 = "D435";
			public const string D465 = "D465";
			public const string D525 = "D525";
			public const string D545 = "D545";
			public const string D575 = "D575";
			public const string D755 = "D755";
			public const string D785 = "D785";
			public const string E485 = "E485";
			public const string E596 = "E596";
			public const string E615 = "E615";
			public const string E626 = "E626";
			public const string E636 = "E636";
			public const string G065 = "G065";
			public const string G235 = "G235";
			public const string G305 = "G305";
			public const string G310 = "G310";
			public const string G355 = "G355";
			public const string G375 = "G375";
			public const string G935 = "G935";
			public const string GCDP = "GCDP";
			public const string GCOL = "GCOL";
			public const string GCTC = "GCTC";
			public const string GCTG = "GCTG";
			public const string GDAP = "GDAP";
			public const string GDCP = "GDCP";
			public const string GDDA = "GDDA";
			public const string GEAS = "GEAS";
			public const string J195 = "J195";
			public const string J198 = "J198";
			public const string J201 = "J201";
			public const string J225 = "J225";
			public const string J236 = "J236";
			public const string J246 = "J246";
			public const string J335 = "J335";
			public const string K216 = "K216";
			public const string K832 = "K832";
			public const string K833 = "K833";
			public const string K835 = "K835";
			public const string K837 = "K837";
			public const string K840 = "K840";
			public const string K843 = "K843";
			public const string K845 = "K845";
			public const string K863 = "K863";
			public const string K867 = "K867";
			public const string K873 = "K873";
			public const string K875 = "K875";
			public const string K877 = "K877";
			public const string K880 = "K880";
			public const string K897 = "K897";
			public const string K900 = "K900";
			public const string K911 = "K911";
			public const string K912 = "K912";
			public const string K913 = "K913";
			public const string K914 = "K914";
			public const string K915 = "K915";
			public const string K933 = "K933";
			public const string K935 = "K935";
			public const string K937 = "K937";
			public const string K938 = "K938";
			public const string K939 = "K939";
			public const string K942 = "K942";
			public const string K943 = "K943";
			public const string K944 = "K944";
			public const string K945 = "K945";
			public const string K946 = "K946";
			public const string K947 = "K947";
			public const string K948 = "K948";
			public const string K949 = "K949";
			public const string K950 = "K950";
			public const string K951 = "K951";
			public const string K952 = "K952";
			public const string K953 = "K953";
			public const string K960 = "K960";
			public const string K962 = "K962";
			public const string K963 = "K963";
			public const string K964 = "K964";
			public const string K970 = "K970";
			public const string K973 = "K973";
			public const string L280 = "L280";
			public const string L295 = "L295";
			public const string L300 = "L300";
			public const string L302 = "L302";
			public const string L304 = "L304";
			public const string L306 = "L306";
			public const string L308 = "L308";
			public const string L310 = "L310";
			public const string L311 = "L311";
			public const string L312 = "L312";
			public const string L313 = "L313";
			public const string L315 = "L315";
			public const string L383 = "L383";
			public const string L385 = "L385";
			public const string L387 = "L387";
			public const string L390 = "L390";
			public const string L393 = "L393";
			public const string L394 = "L394";
			public const string L403 = "L403";
			public const string L405 = "L405";
			public const string L407 = "L407";
			public const string L410 = "L410";
			public const string L412 = "L412";
			public const string L423 = "L423";
			public const string L425 = "L425";
			public const string L433 = "L433";
			public const string L437 = "L437";
			public const string L440 = "L440";
			public const string L443 = "L443";
			public const string L444 = "L444";
			public const string L453 = "L453";
			public const string L455 = "L455";
			public const string L457 = "L457";
			public const string L463 = "L463";
			public const string L467 = "L467";
			public const string L473 = "L473";
			public const string L505 = "L505";
			public const string L535 = "L535";
			public const string L540 = "L540";
			public const string L542 = "L542";
			public const string L544 = "L544";
			public const string L565 = "L565";
			public const string L605 = "L605";
			public const string L610 = "L610";
			public const string L644 = "L61L6440";
			public const string L646 = "L646";
			public const string L648 = "L648";
			public const string L652 = "L652";
			public const string L654 = "L654";
			public const string L656 = "L656";
			public const string L658 = "L658";
			public const string L660 = "L660";
			public const string L662 = "L662";
			public const string L664 = "L664";
			public const string L666 = "L666";
			public const string L668 = "L668";
			public const string L670 = "L670";
			public const string L672 = "L672";
			public const string L674 = "L674";
			public const string L676 = "L676";
			public const string L678 = "L678";
			public const string L680 = "L680";
			public const string L682 = "L682";
			public const string L684 = "L684";
			public const string L686 = "L686";
			public const string L688 = "L688";
			public const string M115 = "M115";
			public const string M125 = "M125";
			public const string M130 = "M130";
			public const string M165 = "M165";
			public const string M195 = "M195";
			public const string M503 = "M503";
			public const string M507 = "M507";
			public const string M560 = "M560";
			public const string M565 = "M565";
			public const string M605 = "M605";
			public const string M610 = "M610";
			public const string M810 = "M810";
			public const string M820 = "M820";
			public const string M825 = "M825";
			public const string M830 = "M830";
			public const string M835 = "M835";
			public const string M840 = "M840";
			public const string N112 = "N112";
			public const string N114 = "N114";
			public const string N116 = "N116";
			public const string N118 = "N118";
			public const string N120 = "N120";
			public const string N240 = "N240";
			public const string N245 = "N245";
			public const string N250 = "N250";
			public const string N255 = "N255";
			public const string N270 = "N270";
			public const string N340 = "N340";
			public const string N345 = "N345";
			public const string N350 = "N350";
			public const string N355 = "N355";
			public const string N380 = "N380";
			public const string N410 = "N410";
			public const string N565 = "N565";
			public const string N580 = "N580";
			public const string N585 = "N585";
			public const string N610 = "N610";
			public const string N625 = "N625";
			public const string N825 = "N825";
			public const string NPER = "NPER";
			public const string P625 = "P625";
			public const string P630 = "P630";
			public const string P635 = "P635";
			public const string P640 = "P640";
			public const string P645 = "P645";
			public const string P650 = "P650";
			public const string P655 = "P655";
			public const string P700 = "P700";
			public const string P912 = "P912";
			public const string P913 = "P913";
			public const string P914 = "P914";
			public const string P916 = "P916";
			public const string P917 = "P917";
			public const string P937 = "P937";
			public const string P938 = "P938";
			public const string P942 = "P942";
			public const string P943 = "P943";
			public const string P944 = "P944";
			public const string P945 = "P945";
			public const string P946 = "P946";
			public const string P947 = "P947";
			public const string P948 = "P948";
			public const string P949 = "P949";
			public const string P962 = "P962";
			public const string P964 = "P964";
			public const string P966 = "P966";
			public const string P968 = "P968";
			public const string P970 = "P970";
			public const string Q125 = "Q125";
			public const string Q415 = "Q415";
			public const string Q416 = "Q416";
			public const string Q420 = "Q420";
			public const string Q422 = "Q422";
			public const string Q455 = "Q455";
			public const string Q605 = "Q605";
			public const string Q610 = "Q610";
			public const string Q622 = "Q622";
			public const string Q624 = "Q624";
			public const string Q626 = "Q626";
			public const string Q628 = "Q628";
			public const string Q630 = "Q630";
			public const string Q632 = "Q632";
			public const string Q634 = "Q634";
			public const string Q755 = "Q755";
			public const string Q780 = "Q780";
			public const string Q785 = "Q785";
			public const string Q800 = "Q800";
			public const string Q855 = "Q855";
			public const string Q865 = "Q865";
			public const string Q880 = "Q880";
			public const string R730 = "R730";
			public const string R732 = "R732";
			public const string R734 = "R734";
			public const string REST = "REST";
			public const string TIPG = "TIPG";
			public const string TIPS = "TIPS";
			public const string U165 = "U165";
			public const string U167 = "U167";
			public const string U195 = "U195";
			public const string U235 = "U235";
			public const string U265 = "U265";
			public const string U315 = "U315";
			public const string U395 = "U395";
			public const string U397 = "U397";
			public const string U425 = "U425";
			public const string U437 = "U437";
			public const string V335 = "V335";
			public const string V340 = "V340";
			public const string V345 = "V345";
			public const string V350 = "V350";
			public const string V355 = "V355";
			public const string V360 = "V360";
			public const string V365 = "V365";
			public const string V395 = "V395";
			public const string V400 = "V400";
			public const string V410 = "V410";
			public const string V540 = "V540";
			public const string V630 = "V630";
			public const string V670 = "V670";
			public const string V830 = "V830";
			public const string V835 = "V835";
			public const string V895 = "V895";
			public const string V897 = "V897";
			public const string V900 = "V900";
			public const string V902 = "V902";
			public const string V905 = "V905";
			public const string V906 = "V906";
			public const string V910 = "V910";
			public const string V915 = "V915";
			public const string V916 = "V916";
			public const string V920 = "V920";
			public const string V925 = "V925";
			public const string V980 = "V980";
			public const string X216 = "X216";
			public const string X218 = "X218";
			public const string X220 = "X220";
		}

		public static class AuthorizationNumber
		{
			public const string AuthorizedPlaceAuthorizationPrefix = "FRTST";
			public const string ApprovedPlaceAuthorizationPrefix = "LADT";
		}

		public static class RefCusRateTypeFormula
		{
			public const string STATVAL = "STATVAL";
		}

		public static class RefCusRateFormula
		{
			public const string Precalcule = "{\"Precalcule\"}";
		}

		public static class RefCusProcedure
		{
			public static class Procedure
			{
				public const string _53 = "53";
			}

			public static class Concession
			{
				public const string C08 = "C08";
				public const string F48 = "F48";
				public const string _1DP = "1DP";
			}
		}
	}
}
