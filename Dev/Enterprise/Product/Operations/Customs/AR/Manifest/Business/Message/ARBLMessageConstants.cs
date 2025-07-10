namespace Enterprise.Customs.AR.Manifest.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Customs Response, Error Message, ID Request Message")]
	internal static class ARBLMessageConstants
	{
		internal const string Accepted = "Request Accepted by AFIP";

		internal const string NewLine = "\r\n";

		internal const string Processing = "Processing Log";

		internal const string Rejected = "Request Rejected by AFIP";

		internal const string ID = "ID: ";

		internal const string Description = "Description:";

		internal const string Reason = "Reason: ";
	}
}
