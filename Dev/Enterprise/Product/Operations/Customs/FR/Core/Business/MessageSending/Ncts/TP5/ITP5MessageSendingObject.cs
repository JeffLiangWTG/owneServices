using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.MessageSending;

public interface ITP5MessageSendingObject : IMessageSendingObject
{
	new ZString MessageType { get; set; }
}
