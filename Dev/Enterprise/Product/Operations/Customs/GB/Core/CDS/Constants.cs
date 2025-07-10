using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.GB.CDS
{
	public static class Constants
	{
		public static class NumbericFunctionCodes
		{
			public const string DeclarationAccepted = "01";
			public const string MessageRegistered = "02";
			public const string MessageRejected = "03";
			public const string DeclarationIncomplete = "04";
			public const string DeclarationSubjectToPhysicalControl = "05";
			public const string DeclarationSubjectToPhysicalControl2 = "06";
			public const string DeclarationUpdatedByCustoms = "07";
			public const string GoodsMayBeReleased = "08";
			public const string DeclarationCleared = "09";
			public const string DeclarationCancelled = "10";
			public const string AdditionalMessageProcessed = "11";
			public const string DutiesTaxesCalculatedAndDue = "13";
			public const string InsufficientDefermentBalance = "14";
			public const string PaymentDue = "15";
			public const string GoodsExitedCustomsUnion = "16";
			public const string ExceptionalIrregularityNeedsToBeHandled = "17";
			public const string ExitOfGoodsFromEUNotConfirmed = "18";
			public const string DefraControl = "50";
			public const string IncomingQueryNotification = "51";
		}

		public static class ThreeCharFunctionCodes
		{
			public const string DeclarationAccepted = ThreeCharFunctionCode.Codes.ACC;
			public const string MessageRegistered = ThreeCharFunctionCode.Codes.RCV;
			public const string MessageRejected = ThreeCharFunctionCode.Codes.REJ;
			public const string DeclarationIncomplete = ThreeCharFunctionCode.Codes.INC;
			public const string DeclarationSubjectToPhysicalControl = ThreeCharFunctionCode.Codes.CTL;
			public const string DeclarationSubjectToPhysicalControl2 = ThreeCharFunctionCode.Codes.DOC;
			public const string DeclarationUpdatedByCustoms = ThreeCharFunctionCode.Codes.RES;
			public const string GoodsMayBeReleased = ThreeCharFunctionCode.Codes.ROG;
			public const string DeclarationCleared = ThreeCharFunctionCode.Codes.CLE;
			public const string DeclarationCancelled = ThreeCharFunctionCode.Codes.INV;
			public const string AdditionalMessageProcessed = ThreeCharFunctionCode.Codes.REQ;
			public const string DutiesTaxesCalculatedAndDue = ThreeCharFunctionCode.Codes.TAX;
			public const string InsufficientDefermentBalance = ThreeCharFunctionCode.Codes.CPI;
			public const string PaymentDue = ThreeCharFunctionCode.Codes.CPR;
			public const string GoodsExitedCustomsUnion = ThreeCharFunctionCode.Codes.EOG;
			public const string ExceptionalIrregularityNeedsToBeHandled = ThreeCharFunctionCode.Codes.EXT;
			public const string ExitOfGoodsFromEUNotConfirmed = ThreeCharFunctionCode.Codes.GER;
			public const string DeclarationAmendment = ThreeCharFunctionCode.Codes.AMD;
			public const string DeclarationRequestForChange = ThreeCharFunctionCode.Codes.COR;
			public const string DeclarationArrivalNotification = ThreeCharFunctionCode.Codes.GPR;
			public const string DefraControl = ThreeCharFunctionCode.Codes.ALV;
			public const string IncomingQueryNotification = ThreeCharFunctionCode.Codes.QRY;
		}

		public static class EHubEventTypes
		{
			public const string MessageRejected = "MRJ";
			public const string MessageResponseRejected = "MRR";
			public const string DocumentSent = "DSN";
		}

		public static class OldCHIEFReportCodes
		{
			public const string DeclarationAccepted = "E2";
			public const string MessageRegistered = "H2/P2";
			public const string MessageRejected = "27 (or N3/S3)";
			public const string DeclarationSubjectToPhysicalControl = "E1/X1";
			public const string GoodsMayBeReleased = "N5";
			public const string DeclarationCancelled = "N4/S4/S8";
			public const string DutiesTaxesCalculatedAndDue = "E2";
			public const string InsufficientDefermentBalance = "E9";
			public const string ExitOfGoodsFromEUNotConfirmed = "S0";
			public const string IncomingQueryNotification = "N6/S6";
		}

		public static class ResponseFunctionCategory
		{
			public const string PositiveReplies = "PositiveReplies";
			public const string Rejections = "Rejections";
			public const string UnsolicitedUpdates = "UnsolicitedUpdates";
		}

		public static class EDIInterchange
		{
			public const string GBCustoms = "GBCustoms";
		}

		public static class DocumentCodes
		{
			public const string C501 = "C501";
			public const string C502 = "C502";
			public const string C503 = "C503";
			public const string C504 = "C504";
			public const string C505 = "C505";
			public const string C506 = "C506";
			public const string C507 = "C507";
			public const string C508 = "C508";
			public const string C509 = "C509";
			public const string C510 = "C510";
			public const string C511 = "C511";
			public const string C512 = "C512";
			public const string C513 = "C513";
			public const string C514 = "C514";
			public const string C515 = "C515";
			public const string C516 = "C516";
			public const string C517 = "C517";
			public const string C518 = "C518";
			public const string C519 = "C519";
			public const string C520 = "C520";
			public const string C521 = "C521";
			public const string C522 = "C522";
			public const string C523 = "C523";
			public const string C524 = "C524";
			public const string C525 = "C525";
			public const string C526 = "C526";
			public const string C601 = "C601";
			public const string Y027 = "Y027";
			public const string _1ATR = "1ATR";
			public const string _1AOR = "1AOR";
			public const string _1AVR = "1AVR";
		}

		public static class GuaranteeCodes
		{
			public const string Y = "Y";
		}

		public static class Classification
		{
			public static class IDs
			{
				public const string ReducedRate = "VATR";
				public const string VATExempt = "VATE";
				public const string ZeroRated = "VATZ";
			}

			public static class IdentificationTypeCodes
			{
				public const string TRA = "TRA";
				public const string GN = "GN";
				public const string TRC = "TRC";
				public const string TSP = "TSP";
			}
		}

		public static class StatementTypeCodes
		{
			public const string AES = "AES";
		}

		public static class OriginTypeCodes
		{
			public const string NonPreferential = "1";
			public const string Preferential = "2";
		}

		public static class MethodOfPayment
		{
			public const string DeferredPayment = "E";
			public const string PostponedVatAccounting = "G";
		}

		public static class StatusNameCode
		{
			public const string FinalCustomsDebt = "4";
			public const string ProvisionalCustomsDebt = "67";
		}

		public static class SpecificCircumstanceCodes
		{
			public const string A20 = "A20";
		}

		public static class MessagingCodes
		{
			public const string NoSeals = "NOSEALS";
		}

		public static class TaxTypeCodes
		{
			public const string Subsidy = "STA";
		}

		public static class NorthernIrelandModeCodes
		{
			public const string NII = "NII";
			public const string NIE = "NIE";
			public const string G2N = "G2N";
			public const string N2G = "N2G";
		}

		public static class RefCusCodeListAttributeCodes
		{
			public const string GoodsVehicleMovementSystem = "GVMS";
		}

		public static class EDIMessageApplicationReferencesForAmendment
		{
			public const string Current = "Current";
		}

		public static class InterchangeType
		{
			public const string CargoMessageType = "CAR";
		}

		public static class Pentant
		{
			public const char SeparatorChar = '~';
		}
	}
}
