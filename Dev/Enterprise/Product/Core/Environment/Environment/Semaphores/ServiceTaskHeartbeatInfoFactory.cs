using System;
using System.Diagnostics;
using Enterprise.Semaphores.Common;

namespace Enterprise.Environment.Semaphore
{
	class ServiceTaskHeartbeatInfoFactory : IHeartbeatInfoFactory
	{
		public IHeartbeatInfo New()
		{
			return new HeartbeatInfo(Guid.Empty, System.Environment.MachineName, Guid.Empty, ProcessId, "PRC", null);
		}

		static readonly int ProcessId = Process.GetCurrentProcess().Id;
	}
}
