using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Business
{
	public static class UniversalReferenceConstants
	{
		public static class RefCusCodeListTypes
		{
			public static class Codes
			{
				public const string Country = "CL008";
				public const string DiversionRejectionCode = "CL046";
				public const string CL063 = "CL063";
				public const string CL140 = "CL140";
				public const string CL180 = "CL180";
				public const string CL215 = "CL215";
				public const string CL560 = "CL560";
				public const string CL570 = "CL570";
				public const string CL716 = "CL716";
				public const string LegalBasisCode = "LBC";
				public const string NotificationType = "CL384";
				public const string SupportingDocumentExportType = "DC44E";
				public const string GoodsLocation = "FAC";
			}
		}

		public static class ExitReportEnquiryInformationCodeTypes
		{
			public static class Codes
			{
				public const string WillNotExit = "1";
				public const string ExpectedToExit = "2";
				public const string ExitedNoAlternativeEvidence = "3";
				public const string ExitedAlternativeEvidence = "4";
			}
		}

		public static class PreviousProcedureCodes
		{
			public static class Codes
			{
				public const string _00 = "00";
				public const string _07 = "07";
				public const string _41 = "41";
				public const string _43 = "43";
				public const string _45 = "45";
				public const string _51 = "51";
				public const string _53 = "53";
				public const string _54 = "54";
				public const string _71 = "71";
				public const string _76 = "76";
				public const string _77 = "77";
				public const string _78 = "78";
				public const string _10 = "10";
				public const string _11 = "11";
				public const string _21 = "21";
				public const string _22 = "22";
				public const string _23 = "23";
			}
		}

		public static class ExitReportAlternativeEvidenceTypes
		{
			public static class Codes
			{
				public const string DeliveryNoteSignedByConsigneeOutsideCustomsTerritory = "11";
				public const string ProofOfPayment = "12";
				public const string Invoice = "13";
				public const string DeliveryNote = "14";
				public const string DocumentSignedByOperatorTakingGoodsOutOfUnion = "15";
				public const string DocumentProcessedByCustomsInLineWithTheirRules = "16";
				public const string OperatorsRecordsOfGoodsSuppliedToShipsAircraftOffshore = "17";
			}
		}

		public static class TaxOrFeeType
		{
			public static class Codes
			{
				public const string Livestock = "VATA";
				public const string Reduced = "VATF";
				public const string Standard = "VATS";
				public const string SecondReduced = "VATT";
				public const string Zero = "VATZ";
			}

			public static class ExciseTaxes
			{
				public const string MineralOilTax = "X1";
				public const string TobaccoProductsTax = "X2";
				public const string AlcoholProductsTax = "X3";
				public const string MineralOilTaxCarbon = "Y";
			}
		}

		public static class ProcedureCodes
		{
			public static class ProcedureCode
			{
				public const string _0700 = "0700";
				public const string _21 = "21";
				public const string _22 = "22";
				public const string _31 = "31";
				public const string _40 = "40";
				public const string _42 = "42";
				public const string _44 = "44";
				public const string _51 = "51";
				public const string _53 = "53";
				public const string _61 = "61";
				public const string _6121 = "6121";
				public const string _6122 = "6122";
				public const string _63 = "63";
				public const string _71 = "71";
				public const string _7121 = "7121";
				public const string _76 = "76";
				public const string _77 = "77";
				public const string _78 = "78";
				public const string _95 = "95";
				public const string _96 = "96";
			}

			public static readonly ISet<ZString> ProcedureCodeList = new HashSet<ZString> { ProcedureCode._40, ProcedureCode._42, ProcedureCode._61, ProcedureCode._63, ProcedureCode._95, ProcedureCode._96 };

			public static readonly ISet<ZString> BR11106MutuallyExclusiveCodes = new HashSet<ZString> { Concession.C07, Concession.C08 };

			public static readonly ISet<ZString> BR11107MutuallyExclusiveCodes = new HashSet<ZString> { Concession.C07, Concession._1C1 };

			public static class Concession
			{
				public const string _1C1 = "1C1";
				public const string C07 = "C07";
				public const string C08 = "C08";
				public const string F15 = "F15";
				public const string F05 = "F05";
				public const string F21 = "F21";
				public const string F22 = "F22";
				public const string F48 = "F48";
				public const string F49 = "F49";
			}

			public static readonly ISet<ZString> additionalProcedureCodesForBR1115 = new HashSet<ZString> { Concession.F21, Concession.F22 };
		}

		public static class AuthorizationUsageType
		{
			public static class Codes
			{
				public const string REX = "REX";
			}
		}

		public static class AuthorizationUsage
		{
			public static class Codes
			{
				public const string EUS = "EUS";
				public const string IPO = "IPO";
				public const string TEA = "TEA";
				public const string CW1 = "CW1";
				public const string CW2 = "CW2";
				public const string CWP = "CWP";
			}
		}

		public static class PrimaryPreference
		{
			public static class Codes
			{
				public const string NormalThirdCountryTariffDuty = "100";
				public const string TemporaryTariffSuspension = "110";
				public const string GSPRateWithoutConditionsOfLimits = "200";
				public const string TariffSuspensionSubjectToCertificateUnderGSP = "218";
				public const string TariffQuotaUnderGSP = "220";
				public const string TariffQuotaSubjectToCertificateUnderGSP = "225";
				public const string ApplicationOfGSPRatesSubjectToCertificate = "250";
				public const string TemporaryTariffQuota = "320";
			}

			public static class LastTwoCodes
			{
				public const string _20 = "20";
				public const string _25 = "25";
				public const string _28 = "28";
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Quantity constants")]
		public static class QuantityUnits
		{
			public const string ASVPercentage = "ASV%";
			public const string ASV = "ASV";
		}

		public static class IERefCusRateCodes
		{
			public const string VatOnSecurities = "1B1";
			public const string DeferredVAT = "1B2";
			public const string SpecialArrangementsVAT = "1B3";
			public const string SecuritiesOnDuties = "1A1";
			public const string ExciseCarbonTax = "1C1";
			public const string OtherDutiesValuationDeposits = "1D1";
			public const string QuotaDeposit = "1D3";
			public const string SugarAndPoultryDeposits = "1D5";
			public const string SecuritiesForEndUse = "1D6";
			public const string Excises = "1E1";
			public const string Securities_Other = "1S1";
			public const string ExciseMineralOil = "2E2";
			public const string CustomsDutiesOnIndustrialProducts = "A00";
			public const string CustomsAdditionalDuties = "A20";
			public const string DefinitiveAntidumpingDuties = "A30";
			public const string ProvisionalAntidumpingDuties = "A35";
			public const string DefinitiveCountervailingDuties = "A40";
			public const string ProvisionalCountervailingDuties = "A45";
		}

		public static class RefCusRateTypes
		{
			public const string SecurityDeposit = "SEC";
			public const string ExportDuty = "EXP";
			public const string Excise = "EXC";
			public const string CountervailingDuty = "CVD";
			public const string Duty = "DTY";
			public const string AntiDumpingDuty = "ADD";
			public const string Interest = "INT";
			public const string Levies = "LEV";
			public const string Miscellaneous = "MSC";
			public const string VAT = "VAT";
		}
	}
}
