namespace ServiceManager.Host.Abstractions
{
	public enum LogMessageStage
	{
		RequestIsCreated,
		DelayedRequestIsCreated,
		EnqueuedRequest,
		AbsorbedByAnotherRequest,
		ReplacesRequest,
		DequeuedRequest,
		RequestIsOverDue,
		ReprocessRequest,
		IgnoreRequest,
		RequestIsSentToRunner,
		TypeNameIsEmpty,
		PoolKeyError,
		ObtainingRunner,
		RunnerIsFound,
		RunnerIsCreated,
		StoppingAccociatedRunners,
		ValidatingRequest,
		ValidatedRequest,
		RequestIsQueuedByRunner,
		NoRunsAttemptsRemaining,
	}
}
