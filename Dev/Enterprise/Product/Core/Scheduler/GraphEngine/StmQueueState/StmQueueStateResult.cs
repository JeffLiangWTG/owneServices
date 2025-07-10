namespace Enterprise.Scheduler.GraphEngine
{
	/// <summary>
	/// Please never use this enum as a retry mechanism.
	/// It is *only* for notififying logic errors in the GrEngine.
	/// Regards,
	/// Dan. :)
	/// </summary>
	public enum QueueStateResultType
	{
		None = 0,
		Processed,
		Failed,
	}

	public sealed class QueueStateResult<TQueueState>
		where TQueueState : class, IQueueState
	{
		public QueueStateResult(TQueueState state, QueueStateResultType type)
		{
			State = state;
			Type = type;
		}

		public TQueueState State { get; }
		public QueueStateResultType Type { get; }
	}
}
