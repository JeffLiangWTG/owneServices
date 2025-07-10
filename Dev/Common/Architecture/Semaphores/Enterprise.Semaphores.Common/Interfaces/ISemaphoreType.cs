using CargoWise.Common;

namespace Enterprise.Semaphores.Common
{
	public interface ISemaphoreType
	{
		/// <summary>
		/// Semaphore LockInfo - Some information about lock object.
		/// </summary>
		string LockInfo { get; }

		/// <summary>
		/// Semaphore Category - Uniquely Identifies the semaphore type(three letters code).
		/// </summary>
		string Category { get; }

		/// <summary>
		/// Max number of concurrent active semaphore handles of this type.
		/// </summary>
		int MaxConcurrentHandles { get; }
	}

	internal class CommonSemaphoreType : ISemaphoreType
	{
		public CommonSemaphoreType()
		{
			LockInfo = "";
			Category = "";
		}

		public CommonSemaphoreType(string lockInfo, string category, int maxConcurrentHandles)
		{
			Argument.NotNull(lockInfo, nameof(lockInfo));
			Argument.NotNull(category, nameof(category));
			LockInfo = lockInfo;
			Category = category;
			MaxConcurrentHandles = maxConcurrentHandles;
		}

		public string LockInfo { get; private set; }

		public string Category { get; private set; }

		public int MaxConcurrentHandles { get; private set; }
	}
}
