using System;
using System.Threading;

namespace AppDomainWrappers.Net48
{
	public interface IHeartbeat
	{
		event EventHandler<LockFileStateChangedEventArgs> HostStateChanged;
		event EventHandler<LockFileStateChangedEventArgs> ClientStateChanged;
		string GetFileName();
		string InitialiseMyLockFile(string directoryPath, bool overwrite = false);
		CancellationTokenSource DoLockMyLockFile();
		CancellationTokenSource DoMonitorLockFile(string lockFilePath, bool waitForConnection);
		bool DoCancelLockFileMonitor();
		bool DoCancelMyLockFile();
	}
}
