namespace Enterprise.Core.Environment.Semaphores
{
	using Enterprise.Semaphores.Common;

	class EnterpriseActiveLoginSemaphore : ISemaphoreType
	{
		#region ISemaphoreType Members

		string ISemaphoreType.LockInfo
		{
			get { return "EnterpriseActiveLogin"; }
		}

		int ISemaphoreType.MaxConcurrentHandles
		{
			get { return 0; }
		}

		string ISemaphoreType.Category
		{
			get { return "LGN"; }
		}

		#endregion
	}
}
