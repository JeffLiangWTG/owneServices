using static Enterprise.xTMessaging.Shared.Constants;

namespace Enterprise.xTMessaging.Shared
{
	public class MessageHandlingResult
	{
		public MessageHandlingResult(MessageHandlingResultOperation operation, int receivingRetryCount)
		{
			Operation = operation;
			ReceivingRetryCount = receivingRetryCount;
		}

		public MessageHandlingResultOperation Operation { get; }
		public int ReceivingRetryCount { get; }
	}
}
