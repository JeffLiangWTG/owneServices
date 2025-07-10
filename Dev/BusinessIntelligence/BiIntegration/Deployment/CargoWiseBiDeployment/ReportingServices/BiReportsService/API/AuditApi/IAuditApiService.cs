namespace CargoWise.Bi.Deployment.ReportingServices
{
	public interface IAuditApiService
	{
		ChangeSummaryResponse GetChangedTablesList(string afterLsn);

		ChangeDetailResponse GetChangeDetail(string schemaName, string tableName, string afterLsn, string afterSeqVal, int afterCommandId = 0, int afterOperation = 0, string maxLsn = null, int pageSize = 1000);
	}
}
