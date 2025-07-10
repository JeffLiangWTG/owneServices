namespace Enterprise.Customs.AR.Manifest.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "HBL Message, Customs Response, Code of error Message")]
	internal static class ARAWBMessageConstants
	{
		internal const string MessageName = "House Waybill";
		internal const string MessageVersion = "3.00";
		internal const string StreetsSeparator = " ";
		internal const string TransshipmentTransportMode = "Pre-Carriage";
		internal const string ImportExportTransportMode = "On-Carriage";
		internal const string AcceptedProcessed = "Processed";
		internal const string ID = "ID";
		internal const string Name = "Name";
		internal const string Status = "Status";
		internal const string CodeOfError = "Code of the error";
		internal const string DescriptionError = " Description: ";
		internal const string TypeCodeDefault = "Item703";
	}
}
