using Enterprise.xTMessaging.Shared;

namespace Enterprise.xTMessaging.Business
{
	public class Cw1DirectxTMessagingConfig : IDirectxTMessagingConfig
	{
		double? _cachedXTIdleConnectionKeepAliveInSeconds;
		public double XTIdleConnectionKeepAliveInSecondsValue
		{
			get
			{
				if (!_cachedXTIdleConnectionKeepAliveInSeconds.HasValue)
				{
					_cachedXTIdleConnectionKeepAliveInSeconds = DirectxTMessagingRegistry.Instance.XTIdleConnectionKeepAliveInSeconds.Value;
				}
				return _cachedXTIdleConnectionKeepAliveInSeconds.Value;
			}
		}

		double? _cachedXTIdleConnectionRetryPauseInSeconds;
		public double XTIdleConnectionRetryPauseInSecondsValue
		{
			get
			{
				if (!_cachedXTIdleConnectionRetryPauseInSeconds.HasValue)
				{
					_cachedXTIdleConnectionRetryPauseInSeconds = DirectxTMessagingRegistry.Instance.XTIdleConnectionRetryPauseInSeconds.Value;
				}
				return _cachedXTIdleConnectionRetryPauseInSeconds.Value;
			}
		}

		int? _cachedInterchangeCountPerBatchOnReceiving;
		public int InterchangeCountPerBatchOnReceivingValue
		{
			get
			{
				if (!_cachedInterchangeCountPerBatchOnReceiving.HasValue)
				{
					_cachedInterchangeCountPerBatchOnReceiving = DirectxTMessagingRegistry.Instance.InterchangeCountPerBatchOnReceiving.Value;
				}
				return _cachedInterchangeCountPerBatchOnReceiving.Value;
			}
		}

		int? _cachedXTServerMessageChunkSizeWhenSending;
		public int XTServerMessageChunkSizeWhenSendingValue
		{
			get
			{
				if (!_cachedXTServerMessageChunkSizeWhenSending.HasValue)
				{
					_cachedXTServerMessageChunkSizeWhenSending = DirectxTMessagingRegistry.Instance.XTServerMessageChunkSizeWhenSending.Value;
				}
				return _cachedXTServerMessageChunkSizeWhenSending.Value;
			}
		}
	}
}
