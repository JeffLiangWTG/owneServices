using System.Diagnostics.CodeAnalysis;

namespace Enterprise.Customs.NL.Business.Common;

public static class NLConstants
{
	public static class ProcedureCodes
	{
		public const string _05 = "05";
		public const string _07 = "07";
		public const string _10 = "10";
		public const string _11 = "11";
		public const string _21 = "21";
		public const string _22 = "22";
		public const string _23 = "23";
		public const string _31 = "31";
		public const string _40 = "40";
		public const string _41 = "41";
		public const string _42 = "42";
		public const string _45 = "45";
		public const string _46 = "46";
		public const string _47 = "47";
		public const string _49 = "49";
		public const string _51 = "51";
		public const string _52 = "52";
		public const string _53 = "53";
		public const string _54 = "54";
		public const string _61 = "61";
		public const string _63 = "63";
		public const string _68 = "68";
		public const string _71 = "71";
		public const string _76 = "76";
		public const string _77 = "77";
		public const string _78 = "78";
		public const string _95 = "95";
		public const string _96 = "96";

		public static class Concession
		{
			public const string F48 = "F48";
			public const string F49 = "F49";
		}
	}

	public static class TransportContractTypes
	{
		public const string WaybillFreightForwarder = "N703";
		public const string BillOfLadingFreightForwarder = "N714";
		public const string MasterBillOfLading = "N704";
		public const string MaritimeBillofLading = "N705";
		public const string MasterAirWaybill = "N741";
	}

	public static class EdiMessageSubTypes
	{
		public const string VATPartyInformation = "VAT";
		public const string GuaranteeAmountInformation = "DEF";
	}

	public static class SenderIdSuffixes
	{
		public const string DMS = ".05";
		public const string NCTS = ".04";
	}

	// this is codelist CL060, please keep sorted
	public static class WCoTypeCodes
	{
		public const string ArrivalNotification = "CC007C";
		public const string Amendment = "CC013C";
		public const string DeclarationInvalidationRequest = "CC014C";
		public const string DeclarationData = "CC015C";
		public const string UnloadingRemarks = "CC044C";
		public const string RequestOfRelease = "CC054C";
		public const string InformationAboutNonArrivedMovement = "CC141C";
		public const string PresentationNotification = "CC170C";
		public const string AmendmentAccepted = "CC404A";
		public const string InvalidationConfirmation = "CC410A";
		public const string ImportInvalidationRequest = "CC414A";
		public const string ImportDeclaration = "CC415A";
		public const string DeclarationAcceptance = "CC426A";
		public const string Acceptance = "CC428A";
		public const string Release = "CC429A";
		public const string SupplementReminder = "CC431A";
		public const string ImportPresentation = "CC432A";
		public const string ReminderForInformation = "CC438A";
		public const string NoRelease = "CC451A";
		public const string Rejection = "CC456A";
		public const string ControlNotification = "CC460A";
		public const string ExportAmendmentAccepted = "CC504C";
		public const string Cancellation = "CC509C";
		public const string ExportPresentation = "CC511C";
		public const string ExportAmendment = "CC513C";
		public const string ExportInvalidationRequest = "CC514C";
		public const string ExportDeclaration = "CC515C";
		public const string ExportAcceptance = "CC528C";
		public const string ExportRelease = "CC529C";
		public const string ExportSupplementReminder = "CC531C";
		public const string CancellationReply = "CC551C";
		public const string ExportRejection = "CC556C";
		public const string ExportControlNotification = "CC560C";
		public const string ExitReminder = "CC582C";
		public const string InformationOnNonExitedExport = "CC583C";
		public const string IncomingAmendment = "CCAMDA";
		public const string Ext = "CCEXTA";
		public const string ReceiveMessage = "CCRCVA";
		public const string PreliminaryDeclarationAccepted = "CCREGA";
		public const string RequestForInformation = "CCRFIA";
		public const string RequestForInformationReminder = "CCRRDA";
		public const string ControlFindingsInformationExport = "CCCREA";
	}

	public static class StatementTypes
	{
		public const string Customs = "CUS";
	}

	public static class DMSMessageValues
	{
		public const string GoodsLocationIdentificationTypeCode = "T";
		public const string TypeCodeMail = "EM";
		public const string TypeCodeTelephone = "TE";
	}

	public static class FormattedProcedureCodes
	{
		public const string F48 = "F48";
		public const string _1000 = "1000";
	}

	// Please add to EntryStatusNew and this will be removed
	public static class EntryStatus
	{
		public const string AdvanceDeclarationSent = "100";
		public const string Rejection_415_not_SubStyle_XY = "120";
		public const string ExportRejection_515_not_SubStyle_XY = "120";
		public const string AdvanceDeclarationReceived = "200";
		public const string _220 = "220";
		public const string Rejection_432 = "230";
		public const string ExportRejection_511 = "230";
		public const string Accepted = "300";
		public const string DocumentsControl = "310";
		public const string NuclearMaterials = "320";
		public const string NonIntrusiveInspection = "330";
		public const string PhysicalInspection = "340";
		public const string IdentificationOfShipment = "341";
		public const string IntrusiveInspection = "342";
		public const string QualityControl = "343";
		public const string CharacteristicsOfGoods = "344";
		public const string Sampling = "345";
		public const string OtherControl = "350";
		public const string RequestForInformation = "360";
		public const string ExportReminder_NoExitInformationReceived = "370";
		public const string InformationSentToCustoms = "375";
		public const string ExportRejection_CRE = "376";
		public const string Rejection_CRI = "376";
		public const string InformationReceivedByCustoms = "377";
		public const string NoRelease_NRE = "380";
		public const string CancellationReply = "380";
		public const string _400 = "400";
		public const string _410 = "410";
		public const string _420 = "420";
		public const string ProvisionalRelease = "500";
		public const string SupplementReminder = "510";
		public const string SupplementSent = "520";
		public const string ExportRelease = "529";
		public const string SupplementReceivedByCustoms = "530";
		public const string Rejection_415_for_SubStyle_XY = "540";
		public const string ExportRejection_515_for_SubStyle_XY = "540";
		public const string ReleasedAndTaxed = "550";
		public const string NoRelease = "560";
		public const string InvalidationRequestSent = "600";
		public const string Rejection_414 = "610";
		public const string ExportRejection_514 = "610";
		public const string InvalidationRequestReceivedByCustoms = "620";
		public const string Cancelled = "630";
		public const string AmendmentRequestSent = "700";
		public const string Rejection_413 = "710";
		public const string ExportRejection_513 = "710";
		public const string AmendmentRequestReceivedByCustoms = "720";
		public const string AmendmentAccepted = "730";
		public const string _800 = "800";
		public const string NoExitInformationRecievedYet = "810";
		public const string ExitInformationDetailsSent = "820";
		public const string ExitInformationDetailsReceived = "830";
		public const string ExportRejection_583 = "840";
		public const string ExportCancellation = "850";
	}

	// AKA Customs status, please keep values sorted
	public static class EntryStatusNew
	{
		public const string Amended = "AMD";
		public const string Cancelled = "CAN";
		public const string PhysicalInspection = "CTL";
		public const string DocumentsControl = "DOC";
		public const string GoodsExitedEU = "EXT";
		public const string MRNAllocated = "MRN";
		public const string NoRelease = "NRL";
		public const string PreLodged = "PRE";
		public const string ProvisionalRelease = "PRL";
		public const string Received = "RCV";
		public const string DeclarationRejected = "REJ";
		public const string Released = "REL";
		public const string RequestForInformation = "RFI";
		public const string NotReceiveResponseBeforeEndOfFallback = "NRC";
	}

	// Please add to StatusNew and this will be removed
	public static class Status
	{
		public const string Rejection = "REJ";
		public const string NoRelease_NRE = "NRE";
		public const string CLE = "CLE";
		public const string Cancelled = "CAN";
		public const string ROG = "ROG";
		public const string Reminder = "REM";
		public const string MRN = "MRN";
		public const string Received = "RCV";
		public const string Admentment = "AMD";
		public const string Request = "REQ";
		public const string ExitConfirm = "EOG";
	}

	// AKA Logical status, please keep values sorted
	public static class StatusNew
	{
		public const string Accepted = "ACC";
		public const string Cancelled = "CAN";
		public const string Error = "ERR";
		public const string Invalid = "INV";
		public const string Rejection = "REJ";
		public const string ReminderReceived = "REM";
		public const string SentToCustoms = "SNT";
	}

	public static class RejectionStatus
	{
		// import
		public const string Rejection_413 = "413";
		public const string Rejection_414 = "414";
		public const string Rejection_415 = "415";
		public const string Rejection_432 = "432";
		public const string Rejection_CRI = "CRI";
		//Export
		public const string Rejection_513 = "513";
		public const string Rejection_514 = "514";
		public const string Rejection_515 = "515";
		public const string Rejection_511 = "511";
		public const string Rejection_CRE = "CRE";
		public const string Rejection_583 = "583";
	}

	public static class ControlTypes
	{
		public const string DocumentsControl = "10";
		public const string NuclearMaterials = "20";
		public const string NonIntrusiveInspection = "30";
		public const string PhysicalInspection = "40";
		public const string IdentificationOfShipment = "41";
		public const string IntrusiveInspection = "42";
		public const string QualityControl = "43";
		public const string NatureOfGoods = "44";
		public const string Sampling = "45";
		public const string Other = "50";
	}

	public static class MessageStatuses
	{
		public const string DOC = "DOC";
		public const string FYC = "FYC";
	}

	public static class StatusNameCodes
	{
		public const string Released = "4";
		public const string NoRelease = "109";
		public const string ProvisionalRelease = "115";
	}

	public static class GuaranteeReferenceTypes
	{
		public const string Guarantee = "GRN";
	}

	public static class RuleCodes
	{
		public const string Office = "OFF";
	}

	public static class EDIInterchange
	{
		public const string NLCustoms = "NLCustomsDMS";
	}

	public static class EntryStyles
	{
		public const string DeclarationForEndUse = "H1";
		public const string DeclarationForCustWarehouse = "H2";
		public const string DeclarationTemporaryAdmission = "H3";
		public const string DeclarationInwardProcessing = "H4";
		public const string ImportSpecialFiscalTerritoriesDeclaration = "H5";
		public const string DeclarationFreeCirculation = "H6";
		public const string ProbablyNoControlRequiredConsignment = "H7";
		public const string ExportReExport = "B1";
		public const string SpecialProcessing = "B2";
		public const string UnionGoods = "B3";
		public const string SpecialFiscalTerritory = "B4";
		public const string ImportSimplifiedDeclaration = "I1";
		public const string ImportDeclarationI2 = "I2";
		public const string ExportDeclarationC1 = "C1";
		public const string ExportDeclarationC2 = "C2";
	}

	public static class PreviousDocumentTypes
	{
		public const string AcknowledgmentOfMRN = "NMRN";
		public const string C651 = "C651";
		public const string N705 = "N705";
	}

	public static class Classification
	{
		public static class IdentificationTypeCodes
		{
			public const string TRA = "TRA";
			public const string TRC = "TRC";
			public const string TSP = "TSP";
			public const string GN = "GN";
			public const string CV = "CV";
			public const string UIN = "UIN";
		}
	}

	[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant string")]
	public static class EDIMessageApplicationReferences
	{
		public const string Current = "Current";
		public const string Control = "CONTROL";
	}

	public static class Notes
	{
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant string")]
		public static class Descriptions
		{
			public const string DataImportLogText = "Data Import Log Text";
			public const string ProcessingLog = "Processing Log";
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant strings")]
		public static class Texts
		{
			public const string FailedToDeserialize = "Failed to deserialize xml message.";
			public const string CouldNotFindMessageTypeForWCOTypeCode = "Could not find message type for WCO Type Code";
			public const string NctsHeaderNotFoundWithMRN = "Could not find a NCTS movement with MRN ";
			public const string NctsHeaderNotFoundWithLRN = "Could not find a NCTS movement with LRN ";
			public const string NctsHeaderNotFoundWithCorrelationId = "Could not find a NCTS movement with Correlation Identifier ";
		}
	}

	public static class CustomMsgAttributes
	{
		public const string Subject = "custom.Subject";
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant strings")]
		public static class SubjectAttributes
		{
			public const string V = "v";
			public const string A = "a";
			public const string K = "k";
			public const string S = "s";
		}
	}

	public static class CustomMsgValues
	{
		public static class SubjectAttributeValues
		{
			public const string A = "DMS.NL";
			public const string S = "0";
		}
	}

	public static class CusPermitHeaderTypes
	{
		public const string TransitOperation = "TRD";
		public const string ACR = "ACR";
		public const string SSE = "SSE";
	}

	public static class AuthorisationTypes
	{
		public const string C520 = "C520";
		public const string C521 = "C521";
		public const string C522 = "C522";
		public const string C523 = "C523";
		public const string C524 = "C524";
	}

	[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Html element")]
	public static class HtmlContent
	{
		public const string Break = "</br>";
	}

	public static class ServiceTypes
	{
		public const string ControlByCustoms = "CTL";
	}

	public static class CTStatusCodes
	{
		public const string C = "C";
	}

	public static class SecurityCodes
	{
		public const string _0 = "0";
	}
}
