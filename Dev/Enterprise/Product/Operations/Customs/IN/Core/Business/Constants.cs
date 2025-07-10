namespace Enterprise.Customs.IN.Business;

public static class Constants
{
	public static class Messaging
	{
		public const string INMessageNumPlaceHolder = "<<MSG>>";
		public const string MessageDownloaded = "Downloaded";
	}

	public static class MessageID
	{
		public const string AirCgm = "CMCHI01";
		public const string AirCgmAcknowledgement = "CHCMI02";
		public const string SeaCgm = "CMCHI21";
		public const string SeaCgmAcknowledgement = "CHCMI21A";
		public const string ShippingBill = "CACHE01";
		public const string ReplyToShippingBill = "CACHE04";
		public const string GoodsRegistration = "CACHE05";
		public const string BillOfEntry = "CACHI01";
		public const string BillOfEntryAmendment = "CACHI01_A";
		public const string ReplyToBillOfEntry = "CACHI06";
		public const string AirIgm = "ALCHI01";
		public const string SeaIgm = "SACHI01";
		public const string AirEgm = "ALCHE01";
		public const string SeaEgm = "SACHE18";
	}

	public static class MessageType
	{
		public const string XtErrorResponse = "XER";
	}

	public static class NatureOfCargo
	{
		public const string Containerised = "C";
		public const string BreakBulk = "P";
		public const string Bulk = "DB";
		public const string Liquid = "LB";
		public const string ContainerisedAndPackaged = "CP";
	}

	public static class CusSupportingInfo
	{
		public const string DutyFreeImportAuthorization = "DFIA";
		public const string Export = "EXP";
	}

	public static class IncoTerm
	{
		public const string CIF = "CIF";
		public const string FOB = "FOB";
		public const string CF = "CF";
		public const string CI = "CI";
	}

	public static class MessageDownloadFileExtension
	{
		public const string GoodRegistration = ".gr";
	}

	public static class AccessoryStatus
	{
		public const string _1 = "1";
		public const string _2 = "2";
	}
}
