namespace Enterprise.Core.Environment.Semaphores
{
	using Enterprise.Semaphores.Common;

	class EnterpriseWinzorActiveLoginSemaphore : ISemaphoreType
	{
		#region ISemaphoreType Members

		string ISemaphoreType.LockInfo
		{
			get { return "EnterpriseWinzorActiveLogin"; }
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
