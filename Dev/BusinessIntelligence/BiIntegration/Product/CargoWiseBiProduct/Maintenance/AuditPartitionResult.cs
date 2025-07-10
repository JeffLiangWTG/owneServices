namespace CargoWise.Bi.Maintenance
{
	public class AuditPartitionResult
	{
		public AuditPartitionResult(AuditPartitionResultCode code, string infoMessage, string errorMessage)
		{
			Code = code;
			InfoMessage = infoMessage;
			ErrorMessage = errorMessage;
		}
		public AuditPartitionResultCode Code { get; private set; }
		public string InfoMessage { get; private set; }
		public string ErrorMessage { get; private set; }

		#region SuppressResourceStringsCheckRegion

		public bool IsSuccess => Code == AuditPartitionResultCode.Success;

		public string GetErrorDescription()
		{
			string errorDescription = null;
			switch (Code)
			{
				case AuditPartitionResultCode.Success:
					break;
				case AuditPartitionResultCode.UnhandledFailure:
					errorDescription = "Unexpected (unhandled) failure";
					break;
				case AuditPartitionResultCode.CannotCreatePartitionFunction:
					errorDescription = "Cannot create partition function";
					break;
				case AuditPartitionResultCode.CannotCreatePartitionSchema:
					errorDescription = "Cannot create partition schema";
					break;
				case AuditPartitionResultCode.CannotCreatePartitionedIndex:
					errorDescription = "Cannot create partitioned index (note: a table can be left as heap, or partitioned heap)";
					break;
				case AuditPartitionResultCode.NotAllTablesPartitioned:
					errorDescription = "Some Audit tables are not partitioned";
					break;
				case AuditPartitionResultCode.CannotMoveDataForPartitionSplitting:
					errorDescription = "Cannot move the data for partition splitting";
					break;
				case AuditPartitionResultCode.CannotSplitRightmostPartition:
					errorDescription = "Cannot split the right most partition";
					break;
				case AuditPartitionResultCode.CannotMoveDataToOriginalPartitions:
					errorDescription = "Cannot move the data back to the original paritions and finalize partition splitting";
					break;
				case AuditPartitionResultCode.CannotSwitchPartitions:
					errorDescription = "Cannot switch partitions";
					break;
				case AuditPartitionResultCode.CannotMergePartitions:
					errorDescription = "Cannot merge partitions";
					break;
				case AuditPartitionResultCode.CannotDeleteFromLsnTimeMapping:
					errorDescription = "Cannot delete old entries from LsnTimeMapping table";
					break;
				case AuditPartitionResultCode.UnsupportedDateRange:
					errorDescription = "Unsupported Date Range";
					break;
				case AuditPartitionResultCode.MissingCdcHistorySummaryTable:
					errorDescription = "CdcHistorySummary table is missing.";
					break;
				case AuditPartitionResultCode.CannotCreatePartitionedIndexForCdcHistorySummary:
					errorDescription = "Cannot create partitioned index for CdcHistorySummary table";
					break;
				case AuditPartitionResultCode.CannotPartitionCdcHistorySummary:
					errorDescription = "Cannot create partition for CdcHistorySummary table";
					break;
				case AuditPartitionResultCode.NonClusteredIndexCreationFailed:
					errorDescription = "CdcHistorySummary table is partitioned but nonclustered index creation failed";
					break;
				default:
					errorDescription = $"Unknown error code ({Code}) returned.";
					break;
			}
			return errorDescription;
		}

		#endregion
	}
}
