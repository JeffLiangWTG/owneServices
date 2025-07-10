using CargoWise.Common;
namespace Enterprise.Semaphores.Common
{
	public class PendingUserActionSemaphore : ISemaphoreType
	{
		public const string LockInfoPrefix = "EnterprisePendingUserAction:";

		#region Testing
#if DEBUG
		public PendingUserActionSemaphore() : this("0") { } // for the reflection test
#endif
		#endregion

		public PendingUserActionSemaphore(string key)
		{
			Argument.NotNullOrEmpty(key, nameof(key));

			lockInfo = string.Format("{0}{1}", LockInfoPrefix, key);
		}

		#region ISemaphoreType Members

		string ISemaphoreType.Category
		{
			get { return "ACT"; }
		}

		string ISemaphoreType.LockInfo
		{
			get { return lockInfo; }
		}

		readonly string lockInfo;

		int ISemaphoreType.MaxConcurrentHandles
		{
			get { return 0; }
		}

		#endregion
	}
}
