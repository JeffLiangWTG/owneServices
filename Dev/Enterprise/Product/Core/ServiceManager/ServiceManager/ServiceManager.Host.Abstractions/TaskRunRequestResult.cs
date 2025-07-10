namespace ServiceManager.Host.Abstractions
{
	public enum TaskRunRequestResult
	{
		Success,
		ConfigurationError,
		ProcessDidNotStart,
		Inactive,
		TaskIsDisabled,
		TaskAlreadyCompleted,
	}
}
