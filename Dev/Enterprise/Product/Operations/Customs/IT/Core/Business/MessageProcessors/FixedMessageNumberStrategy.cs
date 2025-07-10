using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business;

public class FixedMessageNumberStrategy : IMessageNumberStrategy
{
	public FixedMessageNumberStrategy(ZString messageNumber)
	{
		this.messageNumber = messageNumber;
	}

	public FixedMessageNumberStrategy(int messageNumber)
	{
		this.messageNumber = new ZString(messageNumber.ToString());
	}

	readonly ZString messageNumber;

	public string GetMessageReferenceNumber() => messageNumber;
}
