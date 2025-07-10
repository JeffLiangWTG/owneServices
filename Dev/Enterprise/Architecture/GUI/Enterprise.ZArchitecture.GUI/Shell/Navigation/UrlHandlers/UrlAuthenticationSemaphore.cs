namespace Enterprise.ZArchitecture.Modules
{
	using Enterprise.Semaphores.Common;

	class UrlAuthenticationSemaphore : ISemaphoreType
	{
		#region Testing
#if DEBUG
		public UrlAuthenticationSemaphore() : this(string.Empty) { }
#endif
		#endregion

		public UrlAuthenticationSemaphore(string key)
		{
			lockInfo = string.Format("UrlAuthentication:{0}", key); // Programmatic constant
		}

		#region ISemaphoreType Members

		string ISemaphoreType.Category
		{
			get { return "URL"; }
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
