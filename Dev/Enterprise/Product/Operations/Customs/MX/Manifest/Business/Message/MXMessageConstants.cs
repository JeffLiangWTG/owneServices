using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.MX.Manifest.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Message strings")]
	public static class MXMessageConstants
	{
		public const string MessageServiceTaskCategory = "MXC";

		public const string MXCustomsForAirMode = "MXCustomsAirMode";

		public const string MXCustomsForSeaMode = "MXCustomsSeaMode";

		public const string MXCustomsForSeaModeTesting = "MXCustomsSeaModeTesting";

		internal const string HBL = "HBL";

		internal const string AcceptedByCustoms = "accepted by Customs";

		internal const string RejectedMessage = "has been rejected";

		internal const string Send = "Send";

		internal const string Cancel = "Cancel";

		internal const string Amend = "Amend";

		internal const string DescriptionError = " Description: ";

		public const string ErrorType = "Error Type : ";

		public const string NotificationType = "Notification Type : ";

		public const string ErrorDescription = "Error Description : ";

		internal static MultilingualString MessageCreateFailure => ResString.GetMultilingualString("47924D51-61B7-4269-ACF5-35DD32662AEB", "Failed to create message");

		internal static MultilingualString MessageSendFailure => ResString.GetMultilingualString("E1241C74-F971-4B21-A7B1-27C9A1173CA9", "Failed to send message");

		internal static MultilingualString MessageSendSuccessful => ResString.GetMultilingualString("77C14379-DBD2-4B22-9515-6325B35CCA3A", "Message sent successfully");

		public const string XER = "XER";
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Customs Response, Error Message, ID Request Message, Sending Message, Code of error Message, Number errros Message, Sent to customs Message")]
	internal static class MXBLMessageConstants
	{
		internal const string Accepted = "ACEPTADO";

		internal const string ErrorPosition = " Error in this position: ";

		internal const string ManifestNumber = "ManifiestoNumero";

		internal const string NewLine = "\r\n";

		internal const string Processing = "Processing Log";

		internal const string Rejected = "RECHAZADO";

		internal const string WrongXML = "Can not deserialize the object";

		internal const string IdRequest = "ID Request";

		internal const string SendingBill = "The sending of";

		internal const string CodeOfError = "Code of the error";

		internal const string NumberErrors = "Number of errors";

		internal const string SentToCustoms = "sent to Customs";
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Customs Response")]
	internal static class MXAWBMessageConstants
	{
		internal const string AcceptedProcessed = "Processed";

		internal const string ID = "ID";

		internal const string Name = "Name";

		internal const string Status = "Status";

		internal const string Error = "Error";

		internal const string TrueStatus = "true";
	}
}
