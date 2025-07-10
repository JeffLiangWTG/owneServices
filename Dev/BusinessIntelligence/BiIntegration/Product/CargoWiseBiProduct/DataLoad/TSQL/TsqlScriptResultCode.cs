namespace CargoWise.Bi.Product.DataLoad
{
	#region Audit

	enum AuditMasterLoadResult
	{
		Success = 0,
		TableConfigurationError = 1,
		NoMaxLsn = 2,
		NoNewTransactions = 3,
		SuccessWithLostChanges = 4,
		SqlError = 5,
		CorruptedIndex = 6,
		LsnTimeMappingError = 7,
		CorruptedIndexCdcHistorySummary = 8
	}

	#endregion

	#region EDW

	enum EdwTableStatus
	{
		New = 0,
		Idle = 1,
		Loaded = 2,
		Transformed = 3,
	}

	public enum EdwInitialMasterLoadResult
	{
		IncrementalMasterLoad = 0,
		InitialLoad = 1,
		IncrementalLoad = 2,
		Merge = 3,
		Transform = 4,
	}

	enum EdwStagingInitialLoadResult
	{
		Success = 0,
		ControlProcessFailure = 1,
		LoadProcessFailure = 2,
	}

	enum EdwStagingIncrementalLoadResult
	{
		Success = 0,
		NoNewTransactions = 1,
		RetryRequired = 2,
		InitialLoadRequired = 3,
		CdcScanRequired = 4,
		SuccessInitialLoad = 5,
		TransformRequired = 6,
		InitialTransformRequired = 7,
		LostChangesDetected = 8,
	}

	public enum EdwStagingMergeResult
	{
		Success = 0,
		InitialAndIncrementalLoadRequired = 1,
		DeleteError = 2,
		Others = 3,
	}

	public enum EdwTransformLoadResult
	{
		Success = 0,
		ControlProcessFailure = 1,
		TransformFailure = 2,
		IncrementalLoadRequired = 3,
		InitialLoadRequired = 4,
		SuccessInitialLoad = 5,
		InitialTransformRequired = 6,
		CorruptedIndex = 7
	}

	#endregion
}
