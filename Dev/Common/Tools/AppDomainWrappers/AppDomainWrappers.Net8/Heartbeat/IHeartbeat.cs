namespace AppDomainWrappers.Net8
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
