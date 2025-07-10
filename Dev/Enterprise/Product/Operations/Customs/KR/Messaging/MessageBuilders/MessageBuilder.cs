using System.IO;

namespace Enterprise.Customs.KR.Messaging
{
	public abstract class MessageBuilder<T> : IMessageBuilder
	{
		public abstract T GenerateMessage();
		public Stream MessageContent => KRXmlObjectSerializer.Serialize(GenerateMessage());
	}
}
