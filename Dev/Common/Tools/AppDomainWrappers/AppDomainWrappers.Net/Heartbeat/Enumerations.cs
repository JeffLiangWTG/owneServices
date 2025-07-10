namespace AppDomainWrappers.Net
{
	public enum LockFileState
	{
		None,
		Waiting,
		Connected,
		Disconnected,
		Failure
	}

	public enum HeartbeatExecutionMode
	{
		Host,
		Client
	}
}
