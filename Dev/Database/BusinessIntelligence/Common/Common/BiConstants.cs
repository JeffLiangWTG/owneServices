namespace CargoWise.Bi.Common
{
	public static class BiConstants
	{
		public const string MainDbSchemaVersionExtPtyName = "MainDbSchemaVersion";
		public const string UpgradedCdcTableListExtPtyName = "UpgradedCdcTableList";
		public const string BiAdminSchemaName = "biadmin"; // Schema name for BI control tables
		public const string BiTempSchemaName = "bitemp"; // Schema name for BI temp tables

		public const string EdwBaseTablePrefix = "BAS__";
		public const string EdwAggregateTablePrefix = "AGG__";
		public const string EdwModelViewPrefix = "MDL__";

		public const string EdwAggregateViewPrefix = "vw_";
		public const string EdwAggregateInitialLoadProcPrefix = "usp_IniLoad_";
		public const string EdwAggregateIncrementalLoadProcPrefix = "usp_IncLoad_";

		public const string BimNudgeTimeParamName = "BIM_NUDGE_TIME";

		public const string AspMaintainanceLockKey = "ASP_MAINTANANCE_LOCK_";

		public const string AspBatchSize = "ASP_BATCH_SIZE";
		public const string LastIndexRebuildUtcDt = "LAST_INDEX_REBUILD_UTC_DT";
		public const string LastMaxLsnProcessed = "LAST_MAX_LSN_PROCESSED";
		public const string LastMaxLsnTimeProcessed = "LAST_MAX_LSN_TIME_PROCESSED";
		public const string LastEtlErrorMessage = "LAST_ETL_ERROR_MESSAGE";
		public const string AdmCdcHistorySummaryPartitioningStatus = "ADM_CDC_HISTORY_SUMMARY_PARTITIONING_STATUS";
		public const string AdmCdcHistorySummaryPartitioningStatusModifiedTimestamp = "ADM_CDC_HISTORY_SUMMARY_PARTITIONING_STATUS_MODIFIED_TIMESTAMP";
		public const string LastPartitionPurgeUtcDt = "LAST_PARTITION_PURGE_UTC_DT";
		public const string LastTranslationTransformDateUtc = "LAST_TRANSLATION_TRANSFORM_UTC";
		public const string UpdateTranslationTableFlag = "UPDATE_TRANSLATION_TABLE_FLAG";
		public const string CdcHistorySummaryErrorNumberParamName = "ADM_CDC_HISTORY_SUMMARY_INDEX_REORGANIZE_SQL_ERROR_NUMBER";
		public const string CdcHistorySummaryErrorMessageParamName = "ADM_CDC_HISTORY_SUMMARY_INDEX_REORGANIZE_SQL_ERROR_MESSAGE";
		public const string LastDateTransformRunUtc = "LAST_DATE_TRANSFORM_RUN_UTC";
		public const string LastLsnTransformedUtc = "LAST_LSN_TRANSFORMED_UTC";
		public const string InitialLoadEndDt = "INITIAL_LOAD_END_DT";
		public const string InitialLoadRequested = "INITIAL_LOAD_REQUESTED";
		public const string MaxLsnToBeProcessed = "MAX_LSN_TO_BE_PROCESSED";
		public const string LastLsnProcessed = "LAST_LSN_PROCESSED";
	}
}
