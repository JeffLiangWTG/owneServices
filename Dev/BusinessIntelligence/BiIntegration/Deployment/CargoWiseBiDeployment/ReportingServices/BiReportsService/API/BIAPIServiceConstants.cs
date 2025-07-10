namespace CargoWise.Bi.Deployment.ReportingServices
{
	public static class BIAPIServiceConstants
	{
		public const string BIADMIN = "BIADMIN"; // schema name
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "cdc column name")]
		public const string LSNPERIOD = "__$lsn_period";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "cdc column name")]
		public const string UPDATEMASK = "__$update_mask";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "cdc column name")]
		public const string SEQVAL = "__$seqval";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "cdc column name")]
		public const string STARTLSN = "__$start_lsn";
		public const string JSON = "JSON"; // cdc response format
		public const string CSV = "CSV"; // cdc response format
	}
}
