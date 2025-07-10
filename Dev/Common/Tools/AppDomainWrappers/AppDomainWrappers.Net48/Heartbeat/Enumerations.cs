namespace AppDomainWrappers.Net48
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
