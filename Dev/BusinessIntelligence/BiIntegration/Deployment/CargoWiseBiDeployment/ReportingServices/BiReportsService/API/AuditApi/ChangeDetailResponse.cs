using Newtonsoft.Json;

namespace CargoWise.Bi.Deployment.ReportingServices
{
	public struct ChangeDetailResponse
	{
		public string schemaName;
		public string tableName;
		public int totalItems;
		public int pageSize;

		public LastItemDetail lastItem;
		public ChangeDetail[] items;
	}

	public struct LastItemDetail
	{
		[JsonProperty("__$start_lsn")]
		public string start_lsn;
		[JsonProperty("__$seqval")]
		public string seqval;
		[JsonProperty("__$command_id")]
		public int command_id;
		[JsonProperty("__$operation")]
		public int operation;
	}

	public struct ChangeDetail
	{
		public string version;
		public int schemaChangeCount;
		public ColumnDetail[] columns;
		public ChangeData[] changes;
		internal byte[] max_lsn;
	}

	public struct ColumnDetail
	{
		public string name;
		public string type;
		public int? maxLength;
		public int? precision;
		public int? scale;
	}

	public struct ChangeData
	{
		[JsonProperty("__$start_lsn")]
		public string start_lsn;
		[JsonProperty("__$seqval")]
		public string seqval;
		[JsonProperty("__$update_mask")]
		public string update_mask;
		[JsonProperty("__$lsn_period")]
		public int lsn_period;
		[JsonProperty("__$command_id")]
		public int command_id;
		[JsonProperty("__$operation")]
		public int operation;

		public ColumnData[] data;
	}

	public struct ColumnData
	{
		public string columnName;
		[JsonProperty(NullValueHandling = NullValueHandling.Include)]
		public string value;
	}
}
