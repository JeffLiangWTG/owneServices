namespace Enterprise.Customs.BE.Business;

public static class Constants
{
	public static class InterchangeMessageTypes
	{
		public const string AcknowledgementMessage = "ACK";
		public const string CustomsServiceErrorUniversalEventResponseMessage = "UniversalInterchange";
		public const string CC928C = "928";
		public const string IE928 = "928";
	}

	public static class CustomMsgAttributes
	{
		public const string CustomsReference = "custom.CustomsReference";
		public const string Language = "custom.Language";
	}

	public static class InterchangeRecievers
	{
		public const string LIVE = "LIVE";
		public const string TEST = "TEST";
		public const string PRE = "PRE";
	}

	public static class InlandTransportModesAsNumber
	{
		public const string Sea = "1";
		public const string Rail = "2";
		public const string Road = "3";
		public const string Air = "4";
		public const string PostMail = "5";
		public const string FixedTransportInstallations = "7";
		public const string InlandWaterwayTransport = "8";
		public const string OwnPropulsion = "9";
	}

	public static class MessageXmlElements
	{
		public const string MessageType = "messageType";
	}

	public static class EntryInstructionStyle
	{
		public const string _A = "A";
		public const string _C = "C";
		public const string _D = "D";
		public const string _E = "E";
		public const string _H = "H";
		public const string _I = "I";
		public const string _J = "J";
	}

	public static class SupportingDocumentTypes
	{
		public const string C517 = nameof(C517);
		public const string C518 = nameof(C518);
		public const string C519 = nameof(C519);
	}

	public static class LocationQualifiers
	{
		public const string A = nameof(A);
		public const string C = nameof(C);
	}

		public static class BECMessageTypes
		{
			public static class Outgoing
			{
				public const string CC511C = "CC511C";
				public const string CC513C = "CC513C";
				public const string CC514C = "CC514C";
				public const string CC515C = "CC515C";
				public const string IE415B = "IE415B";
				public const string IETS115 = "IETS115";
			}
			public const string CustomsServiceErrorUniversalEvent = "UER";
		}

	public static class BECMessageSubtypes
	{
		public static class Outgoing
		{
			public const string TransitAccompanyingDocument = "TAD";
			public const string FOL = "FOL";
		}
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Logging")]
	public static class MessageProcessingNotes
	{
		public const string ProcessingLog = "Processing Log";
		public const string ExceptionLog = "Inner Exception Log";
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Html element")]
	public static class HtmlContent
	{
		public const string Break = "</br>";
	}

	public static class AESWarehouseCodes
	{
		public const string WarehouseTypeR = "R";
		public const string WarehouseTypeS = "S";
		public const string WarehouseTypeU = "U";
		public const string WarehouseTypeV = "V";
		public const string WarehouseTypeY = "Y";
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Date format")]
	public static class DateTimeFormats
	{
		public const string LongTimeIncludingSecondsFormat = "dd-MMM-yy HH:mm:ss";
	}

	public static class Qualifiers
	{
		public const string NietVanToepassing = "NVT";
	}
}
