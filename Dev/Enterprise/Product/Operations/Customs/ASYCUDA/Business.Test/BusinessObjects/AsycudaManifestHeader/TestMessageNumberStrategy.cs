using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	sealed class TestMessageNumberStrategy : IMessageNumberStrategy
	{
		internal TestMessageNumberStrategy(string messageNumber)
		{
			this.messageNumber = messageNumber;
		}

		public string GetMessageReferenceNumber() => messageNumber;

		readonly string messageNumber;
	}
}
