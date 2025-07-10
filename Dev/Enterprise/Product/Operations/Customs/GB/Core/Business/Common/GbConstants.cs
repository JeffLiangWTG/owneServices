using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.GB.Business
{
	public static class GbConstants
	{
		public const string DEPARTED_UK = "DEPARTED UK";
		public const string OK_TO_PROCEED = "OK TO PROCEED";
		public const string AirlineDeliveryScheduleCode = "ADS";

		public const string CdsCashAccountGuaranteeType = "Y";

		public static class CredentialRefSysConfigCodes
		{
			public const string BaseUrlLive = "AppHstLive";
			public const string BaseUrlTest = "AppHstTest";
			public const string BaseUrlPath = "AppUrlPath";
			public const string CallbackUrlLive = "ClbUrlLive";
			public const string CallbackUrlTest = "ClbUrlTest";
			public const string ClientIdLive = "ClientLive";
			public const string ClientIdTest = "ClientTest";
			public const string ClientIdDev = "ClientDev";
		}

		public static class StatusDescriptions
		{
			public const string ExpiredAccessToken = "Access Token - Access token expired";
		}
	}

	public static class GBCommonConstants
	{
		public static class EntryStatusCodes
		{
			public const string DeclarationAccepted = ThreeCharFunctionCode.Codes.ACC;
			public const string DeclarationCleared = ThreeCharFunctionCode.Codes.CLE;
			public const string DeclarationCancelled = ThreeCharFunctionCode.Codes.INV;
			public const string TaxCalculated = ThreeCharFunctionCode.Codes.TAX;
		}

		public static class TaxOverrideReasonCodes
		{
			public const string Override = "OVR";
		}

		public static class AdditonalInfoCodes
		{
			public const string Override = "OVR01";
			public const string NIAID = "NIAID";
			public const string NIREM = "NIREM";
			public const string NIDOM = "NIDOM";
			public const string NIIMP = "NIIMP";
			public const string NIEXP = "NIEXP";
			public const string NIOVR = "NIOVR";
			public const string NIPRO = "NIPRO";
			public const string NIQUO = "NIQUO";
			public const string NIHIS = "NIHIS";
			public const string RRS01 = "RRS01";
		}

		public static class NorthernIrelandDutyCodes
		{
			public const string CustomsDuty = "A50";
			public const string AdditionalDuty = "A70";
			public const string DefinitiveAntiDumpingDuty = "A80";
			public const string ProvisionalAntiDumpingDuty = "A85";
			public const string DefinitiveCountervailingDuty = "A90";
			public const string ProvisionalCountervailingDuty = "A95";
		}

		public static class AuthorisationTypeCodes
		{
			public const string EIR = "EIR";
			public const string SDE = "SDE";
		}

		public static class ImportDeclarationType
		{
			public const string FSD = "FSD";
		}

		public static class RefCusCodeListAttributeCodes
		{
			public const string GvmsArrived = "GvmsArrived";
			public const string GvmsPortId = "GvmsPortId";
			public const string Inventory = "Inventory";
			public const string SupportingDocumentReferenceNumber = "ReferenceNumber";
			public const string SupportingDocumentReason = "Reason";
		}

		public static class RefCusCodeListAttributeValue
		{
			static public class ReferenceNumber
			{
				public const string ReferenceNumberRequired = "Y";
			}
		}

		public static class CDSAdditionDeductionChargeTypeCodes
		{
			public const string BA = "BA";
			public const string BR = "BR";
			public const string BS = "BS";
			public const string BU = "BU";
		}

		public static class SourceTypeCodes
		{
			public const string CW1 = "CW1";
		}

		public static class WindsorFrameworkNIProtocolTariffCodes
		{
			public const string EUN = "EUN";
			public const string GB = "GB";
		}
	}
}
