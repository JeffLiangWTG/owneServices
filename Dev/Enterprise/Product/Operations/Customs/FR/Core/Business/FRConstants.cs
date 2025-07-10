namespace Enterprise.Customs.FR.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
	public static class FRConstants
	{
		public static class CAED
		{
			public const string CommonAccessDocumentCode = "NZZZ";
		}

		public static class DocumentCodes
		{
			public const string ProvisonalAmountAuthorisedDocumentCode = "1AVP";
		}

		public static class VATRegistrationNumberValue
		{
			public const string OCCASIONNEL = "OCCASIONNEL";
		}

		public static class TariffBypassCodes
		{
			public const string ReasonEnabledCode = "E";
		}

		public static class ValuationBypassCodes
		{
			public const string ReasonRequiredCode = "I";
		}

		public static class IncoTermKeys
		{
			public const string ThisMemberState = "1";
			public const string AnotherMemberState = "2";
			public const string OutsideUnion = "3";
		}

		public static class TemporaryStorage
		{
			public const string AppCodeFRC = "FRC";
			public const string AppCodeIST = "IST";
			public const string AppCodeLAD = "LAD";
			public const string AppCodeSTO = "STO";
		}

		public static class HarbourRateFormulaCodes
		{
			public const string TwentyFootLCL = "20LCL";
			public const string FortyFootLCL = "40LCL";
			public const string FortyFiveFootLCL = "45LCL";
			public const string TonnesLCL = "TLCL";
			public const string TwentyFootFCL = "20FCL";
			public const string FortyFootFCL = "40FCL";
			public const string FortyFiveFootFCL = "45FCL";
			public const string TonnesFCL = "TFCL";
			public const string DangerousGoods = "UNDG";
			public const string Container = "CON";
		}

		public static class FrenchCurrency
		{
			public const string CurrencyCode = "EUR";
		}

		public static class PreviousDocuments
		{
			public const string N337 = "N337";
		}

		public static class ExportPreviousDocuments
		{
			public const string AAD = "AAD";
		}

		public static class MethodOfPayment
		{
			public const string _1 = "1";
			public const string _2 = "2";
			public const string AI2 = "3";
			public const string _6 = "6";
		}

		public static class MessageSpecialCharacter
		{
			public const string MessageID = "#ID_MESSAGE#";
		}

		public static class SupplementaryCodes
		{
			public const string listOfFRSupplementaryCodesPrefixes = "0, 1, Q, R, S, T, U, V, Z";
			public const string ProductOfNegligibleValueToDROMSupplementaryCode = "0089";
			public const string PromotionalProductToDROMSupplementaryCode = "0090";
			public const string FreeGoodsSupplementaryCode = "0097";
			public const string _1277 = "1277";
		}

		public static class GrantingOfSeaRateTypes
		{
			public const string ExternalGrantingOfSea = "OME";
			public const string RegionalGrantingOfSea = "OMR";
		}

		public static class NCTSMessage
		{
			public const string Operator = "OPE.FR";
			public const string NationalAdministration = "NTA.FR";
		}

		public static class RefCusConditionType
		{
			public const string Vat = "VAT";
		}

		public static class ThresholdsAndLimits
		{
			public const string NegligibleValueLimit = "174";
			public const string PromotionalProductToDROMValueLimit = "185";
			public const string ProductOfNegligibleValueToDROM = "189";
			public const string NegligibleValueProcedure = "C07";
			public const string C2CValueProcedureSuffix = "C08";
			public const string C2CValueProcedureRule177 = "177";
			public const string C2CValueProcedureRule178 = "178";
			public const string C2CValueProcedureRule179A = "179A";
			public const string C2CValueProcedureRule179B = "179B";
		}

		public static class GuaranteeAppTypes
		{
			public const string TR = "TR";
		}

		public static class ProcedureTypes
		{
			public const string IntoBondedWarehouseProcedure = "71";
		}

		public static class RectificationMotivationTypes
		{
			public const string PrelodgeRectification = "MODIF";
			public const string MissingDocuments = "REC18";
		}

		public static class Preferences
		{
			public static class Prefixes
			{
				public const string _1 = "1";
				public const string _2 = "2";
				public const string _3 = "3";
			}
		}
	}
}
