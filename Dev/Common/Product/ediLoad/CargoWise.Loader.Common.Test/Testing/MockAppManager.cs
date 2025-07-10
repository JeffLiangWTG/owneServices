using System;
using CargoWise.ApplicationManager.Common;

namespace CargoWise.Loader.Common.Testing
{
	public class MockAppManager : IAppManager
	{
		public bool Disposed { get; private set; }
		public bool RetryOnce { get; set; }

		public void Dispose()
		{
			Disposed = true;
		}

		public virtual AppManagerResult Invoke(string assemblyPath, string typeName, object state, MutexRequest request)
		{
			throw new NotImplementedException();
		}
		#region IAppManager Members

		Version IAppManager.GetVersionNumber()
		{
			throw new NotImplementedException();
		}

		AppManagerResult IAppManager.Upgrade(string assemblyPath, string typeName, object state, int timeout)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
