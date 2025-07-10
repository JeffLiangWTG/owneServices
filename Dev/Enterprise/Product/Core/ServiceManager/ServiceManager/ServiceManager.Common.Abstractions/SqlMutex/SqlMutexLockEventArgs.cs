using System;

namespace ServiceManager.Common.Abstractions
{
	public class SqlMutexLockEventArgs : EventArgs
	{
		public SqlMutexLockEventArgs(string lockInfo, string category, int lockResult)
		{
			_ = lockInfo ?? throw new ArgumentNullException(nameof(lockInfo));
			_ = category ?? throw new ArgumentNullException(nameof(category));

			LockInfo = lockInfo;
			Category = category;
			LockResult = lockResult;
		}

		public string Category { get; }
		public string LockInfo { get; }
		public int LockResult { get; }
	}
}
