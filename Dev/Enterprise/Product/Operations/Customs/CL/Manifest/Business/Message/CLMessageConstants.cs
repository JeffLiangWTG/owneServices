using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CL.Manifest.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Message constants")]
	public static class CLMessageConstants
	{
		internal const string Accepted = "Aprobado";

		internal const string BillNotFound = "Can't find the Bill related";

		public const string CLCustomsForSeaMode = "CLCustomsSeaMode";

		public const string CLCustomsForAirMode = "CLCustomsAirMode";

		internal const string Message = "Mensaje";

		public const string MessageServiceTaskCategory = "CHL";

		internal const string NewLine = "\r\n";

		internal const string Processing = "Processing Log";

		internal const string Separator = ": ";

		internal const string WrongXML = "Can not deserialize the object";

		internal const string Cancelation = "CAN";

		internal const string FileNameSeparator = "-";

		internal const string FileNameExtension = ".xml";

		internal const string Header = "Header";

		internal const string ClientID = "ClientID";

		internal const string FileName = "FileName";

		public const string EHub = "eHub";

		internal const string Send = "Send";

		internal const string Cancel = "Cancel";

		internal const string Amend = "Amend";

		internal static MultilingualString MessageCreateFailure => ResString.GetMultilingualString("6B924392-174E-49A5-9A41-4FBE252D7B30", "Failed to create message");

		internal static MultilingualString MessageSendFailure => ResString.GetMultilingualString("A57E1DC2-E65A-4523-BF4D-3CD65776A57F", "Failed to send message");

		internal static MultilingualString MessageSendSuccessful => ResString.GetMultilingualString("CB51D011-5CA8-4739-9CA8-0C8BF78F08DE", "Message sent successfully");
	}
}
