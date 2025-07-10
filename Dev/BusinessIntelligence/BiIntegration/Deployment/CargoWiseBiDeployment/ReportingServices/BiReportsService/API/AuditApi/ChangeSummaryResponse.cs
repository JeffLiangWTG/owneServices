namespace CargoWise.Bi.Deployment.ReportingServices
{
	// Using lower case for the struct fields to match the JSON response

	public struct ChangeSummaryResponse
	{
		public string afterLsn; // The lsn with which these changes have occured after, is the same as the parameter from the request
		public string maxLsn; // The database Lsn highwatermark
		public int totalItems; // The total number of items in the response
		public ChangedTable[] items;
	}

	public struct ChangedTable
	{
		public string schemaName;
		public string tableName;
	}
}
