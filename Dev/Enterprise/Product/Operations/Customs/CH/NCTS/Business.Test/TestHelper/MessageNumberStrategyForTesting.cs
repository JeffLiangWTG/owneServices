using Enterprise.Messaging.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

sealed class MessageNumberStrategyForTesting : IMessageNumberStrategy
{
	internal MessageNumberStrategyForTesting(string messageNumber)
	{
		this.messageNumber = messageNumber;
	}
	readonly string messageNumber;

	public string GetMessageReferenceNumber() => messageNumber;
}
