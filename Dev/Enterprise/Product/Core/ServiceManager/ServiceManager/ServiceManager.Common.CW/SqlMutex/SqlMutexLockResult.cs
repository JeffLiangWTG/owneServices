using System;

namespace ServiceManager.Common.CW
{
	class SqlMutexLockResult
	{
		public SqlMutexLockResult(int returnValue, DateTime expiresAtTimeUtc, DateTime acquiredAtTimeUtc, string returnMessage)
		{
			ReturnValue = returnValue;
			ExpiresAtDateTimeUtc = expiresAtTimeUtc;
			AcquiredDateTimeUtc = acquiredAtTimeUtc;
			ReturnMessage = returnMessage;
		}

		public int ReturnValue { get; }
		public DateTime ExpiresAtDateTimeUtc { get; }
		public DateTime AcquiredDateTimeUtc { get; }
		public string ReturnMessage { get; }

		public bool HasAcquiredLock =>
			ReturnValue == LockReturnSuccess &&
			ExpiresAtDateTimeUtc != DateTime.MinValue &&
			AcquiredDateTimeUtc != DateTime.MinValue &&
			ExpiresAtDateTimeUtc != DateTime.MaxValue &&
			AcquiredDateTimeUtc != DateTime.MaxValue &&
			ExpiresAtDateTimeUtc >= AcquiredDateTimeUtc;

		public const int LockReturnSuccess = 0;
	}
}
