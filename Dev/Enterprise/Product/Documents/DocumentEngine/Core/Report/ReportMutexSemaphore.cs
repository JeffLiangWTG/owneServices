
using Enterprise.Registry.Business;
using Enterprise.Semaphores.Common;

namespace Enterprise.DocumentEngine
{
	public class ReportMutexSemaphore : ISemaphoreType
	{
		#region ISemaphoreType Members

		string ISemaphoreType.Category
		{
			get { return "RMX"; }
		}

		int ISemaphoreType.MaxConcurrentHandles
		{
			get { return SystemDataRegistry.Instance.ReportMaxConnections.Value; }
		}

		string ISemaphoreType.LockInfo
		{
			get { return "ReportMutex"; }
		}

		#endregion
	}
}
