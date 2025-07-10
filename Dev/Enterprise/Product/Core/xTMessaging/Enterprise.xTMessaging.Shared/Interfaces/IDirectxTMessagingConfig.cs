namespace Enterprise.xTMessaging.Shared
{
	public interface IDirectxTMessagingConfig
	{
		double XTIdleConnectionKeepAliveInSecondsValue { get; }
		double XTIdleConnectionRetryPauseInSecondsValue { get; }
		int InterchangeCountPerBatchOnReceivingValue { get; }
		int XTServerMessageChunkSizeWhenSendingValue { get; }
	}
}
