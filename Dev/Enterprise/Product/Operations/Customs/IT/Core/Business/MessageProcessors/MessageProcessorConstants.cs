namespace Enterprise.Customs.IT.Business;

public static class MessageProcessorConstants
{
	public static class InterchangeTypes
	{
		public const string ExitVerification = "MRN";
		public const string SingleWindowRequest = "SWR";
		public const string SingleWindowPositiveAckResponseMessageType = "WSA";
		public const string SingleWindowNegativeAckResponseMessageType = "WSE";
		public const string SingleWindowPdfResponseMessageType = "PDF";
		public const string SingleWindowStatusResponseMessageType = "SWS";
		public const string Ucc6AcknowledgementType = "ACK";
		public const string Ucc6ResponseMessageType = "RES";
		public const string Ucc6CancellationType = "CAN";
		public const string Ucc6AmendmentType = "AMD";
		public const string Ucc6XTradeErrorType = "ERR";
		public const string Ucc6NewMessageType = "NEW";
		public const string Ucc6XTradeSignatureErrorType = "XSE";
		public const string ElectronicFolderResponseType = "EFR";
		public const string ElectronicFolderQueryType = "EFQ";
		public const string ImportType = "IMP";
		public const string IrildesResponseMessageType = "IRR";
		public const string ExportType = "EXP";
		public const string TransitType = "TRA";
	}

	public static class MessageSubTypes
	{
		public const string BCategory = "B";
		public const string HCategory = "H";
	}

	public static class AidaXmlResponseStatusNumbers
	{
		public const int Deposited = 2;
		public const int Confirmed = 4;
		public const int UnderControl = 5;
	}
}
