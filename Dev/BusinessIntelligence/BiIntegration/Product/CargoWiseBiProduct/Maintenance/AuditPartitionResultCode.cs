namespace CargoWise.Bi.Maintenance
{
	public enum AuditPartitionResultCode
	{
		Success = 0,
		UnhandledFailure = 1,
		CannotCreatePartitionFunction = 2,
		CannotCreatePartitionSchema = 3,
		CannotCreatePartitionedIndex = 4,
		NotAllTablesPartitioned = 5,
		CannotMoveDataForPartitionSplitting = 6,
		CannotSplitRightmostPartition = 7,
		CannotMoveDataToOriginalPartitions = 8,
		CannotSwitchPartitions = 9,
		CannotMergePartitions = 10,
		CannotDeleteFromLsnTimeMapping = 11,
		UnsupportedDateRange = 12,
		MissingCdcHistorySummaryTable = 13,
		CannotCreatePartitionedIndexForCdcHistorySummary = 14,
		CannotPartitionCdcHistorySummary = 15,
		NonClusteredIndexCreationFailed = 16
	}
}
