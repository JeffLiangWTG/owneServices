namespace CargoWise.Bi.Maintenance
{
	public class IndexMaintenanceResultCode
	{
		public IndexMaintenanceResultCode(IndexErrorCode errorCode, IndexRebuildResultCode isIndexRebuilt)
			: this(errorCode, isIndexRebuilt, CdcHistorySummaryErrorCode.NoAttempt)
		{
		}

		public IndexMaintenanceResultCode(IndexErrorCode errorCode, IndexRebuildResultCode isIndexRebuilt, CdcHistorySummaryErrorCode cdcHistorySummaryErrorCode)
		{
			ErrorCode = errorCode;
			IsIndexRebuilt = isIndexRebuilt;
			CdcHistorySummaryErrorCode = cdcHistorySummaryErrorCode;
		}

		public readonly IndexErrorCode ErrorCode;
		public readonly IndexRebuildResultCode IsIndexRebuilt;
		public readonly CdcHistorySummaryErrorCode CdcHistorySummaryErrorCode;
	}

	public enum IndexErrorCode
	{
		NoIndexesReorganized = -1,
		AllIndexesReorganized = 0,
		TableError = 1,
		Timeout = 2,
		SqlError = 3
	}

	public enum IndexRebuildResultCode
	{
		NoIndexCorruption = 0,
		AllCorruptedIndexRebuilt = 1,
		SomeCorruptedIndexRebuilt = 2
	}

	public enum CdcHistorySummaryErrorCode
	{
		NoAttempt = -1,
		NoIndexCorruption = 0,
		IndexRebuilt = 1,
		IndexNotRebuilt = 2,
		SqlError = 3
	}
}
