using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.MessageSending
{
	public interface IMessageSendingObject
	{
		IFRMessagesOwner MessagesOwner { get; }

		object DataSource { get; }

		ZString MessageType { get; }
	}
}
