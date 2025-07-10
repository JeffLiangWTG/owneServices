namespace Enterprise.Customs.ES.Business
{
	public static class MessageProcessorConstants
	{
		public static class EntryStatusCodes
		{
			public const string Cancelled = "CAN";
			public const string CustomsDeclarationAccepted = "CDA";
			public const string ClearedWithPendingDocuments = "CDP";
			public const string ClearedWithPendingJustificationCertificatesDJP = "CLJ";
			public const string ClearedWithPendingComplementaryDeclarations = "CLP";
			public const string Cleared = "CLR";
			public const string DiversionRequest = "DVS";
			public const string Error = "ERR";
			public const string HighRiskNotification = "HRN";
			public const string PreDeclarationAccepted = "PDA";
			public const string IncompletePreDeclaration = "PDI";
			public const string PendingForEuOffice = "PCO";
			public const string Unknown = "UNK";
			public const string EffectiveDeparture = "EFD";
			public const string ControlsAtEuOffice = "CCO";
			public const string Invalidated = "INV";
			public const string GoodsStoppedAtDeparture = "STD";
			public const string RequestingDataFromEUOffice = "RCE";
		}

		public static class ResponseMessageCodeList
		{
			public const string AcceptedCode = "0000";
			public const string AcceptedDeclarationTIR = "962";
			public const string AcceptedDeclarationT2LPOUS = "A";
		}

		public static class ImportActivationResultCode
		{
			public const string AcceptedDeclaration = "A";
			public const string RejectedDeclaration = "R";
		}

		public static class DocumentStatus
		{
			public const string Accepted = "ACC";
			public const string Cancelled = "CAN";
		}

		public static class SupportingDocumentProcedure
		{
			public const string Accord = "A";
			public const string Regularize = "R";
			public const string NotProvided = "N";
		}

		public static class ImportQueryDeclarationType
		{
			public const string PDI = "PDI";
			public const string PDS = "PDS";
			public const string PDC = "PDC";
		}

		public static class ImportQueryProcedureType
		{
			public const string A = "A";
			public const string B = "B";
			public const string C = "C";
			public const string X = "X";
			public const string Y = "Y";
			public const string Z = "Z";
		}

		public static class CustomsAdministration
		{
			public const string AEAT = "AEAT";
			public const string ATC = "ATC";
		}

		public static class EdifactCodes
		{
			public const string UNHSegmentCode = "UNH";
		}

		public static class ExtraDataFromProcessing
		{
			public const string SymbolToSeparateExtraDataForPrettyFormatter = "*";
			public const string GuaranteeStatusPrefixForPrettyFormatter = "GuaranteeStatus:";
		}
	}
}
