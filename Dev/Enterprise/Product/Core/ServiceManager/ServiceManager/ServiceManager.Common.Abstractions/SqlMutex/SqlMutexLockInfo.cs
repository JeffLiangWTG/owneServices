namespace ServiceManager.Common.Abstractions
{
	public class SqlMutexLockInfo
	{
		public SqlMutexLockInfo(string? category = null, int? processId = null, string? lockInfo = null, string? workStationName = null)
		{
			Category = category;
			ProcessId = processId;
			LockInfo = lockInfo;
			WorkStationName = workStationName;
		}

		public int? ProcessId { get; }

		public string? Category { get; }

		public string? LockInfo { get; }

		public string? WorkStationName { get; }
	}
}
