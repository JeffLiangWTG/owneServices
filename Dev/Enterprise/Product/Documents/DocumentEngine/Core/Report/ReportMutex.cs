using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Enterprise.Semaphores.Common;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngine
{
	public class ReportMutex : IDisposable
	{
		public bool Lock(Report lockObject)
		{
			bool result = false;
			if (!IsLocked(lockObject))
			{
				ISemaphoreType mutexSemaphore = new ReportMutexSemaphore();
				ISemaphoreHandle mutexHandle = EnvProxy.Instance.SemaphoreProvider.CreateSemaphoreHandle(mutexSemaphore);
				if (mutexHandle.Success)
				{
					handles.Add(lockObject, mutexHandle);
					result = true;
				}
			}

			return result;
		}

		public bool Unlock(Report lockObject)
		{
			bool result = false;
			if (IsLocked(lockObject))
			{
				handles[lockObject].Dispose();
				handles.Remove(lockObject);
				result = true;
			}

			return result;
		}

		public bool IsLocked(Report lockObject)
		{
			return handles.ContainsKey(lockObject);
		}

		public string GetFormattedCurrentLocks()
		{
			var result = new StringBuilder();
			foreach (var info in EnvProxy.Instance.SemaphoreProvider.GetActiveSemaphoreHandles(new ReportMutexSemaphore()).OrderBy(i => i.CreateTimeUtc))
			{
				result.AppendFormat(Res.GetString("daadc350-3880-4cf7-9d04-147d73d981e7", "Start Time UTC: {0}, Host Name: {1}, Type: {2}{3}",
					info.CreateTimeUtc.ToString("hh:mm:ss tt"),
					info.OwnerSession.HostName,
					info.OwnerSession.HeartbeatType,
					System.Environment.NewLine));
			}

			return result.ToString();
		}

		#region IDisposable Members

		public void Dispose()
		{
			Dispose(true);

			// This object will be cleaned up by the Dispose method.
			// Therefore, you should call GC.SupressFinalize to
			// take this object off the finalization queue 
			// and prevent finalization code for this object
			// from executing a second time.
			GC.SuppressFinalize(this);
		}

		~ReportMutex()
		{
			Dispose(false);
		}

		void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					UnlockAll();
				}
			}
			disposed = true;
		}

		bool disposed;

		protected void UnlockAll()
		{
			Report[] lockObjects = new Report[handles.Count];
			handles.Keys.CopyTo(lockObjects, 0);
			foreach (Report lockObject in lockObjects)
			{
				Unlock(lockObject);
			}
		}

		#endregion

		readonly Dictionary<Report, ISemaphoreHandle> handles = new Dictionary<Report, ISemaphoreHandle>();
	}
}
