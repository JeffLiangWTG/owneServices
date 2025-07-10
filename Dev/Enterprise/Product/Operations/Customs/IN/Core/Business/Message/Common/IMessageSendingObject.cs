using CargoWise.Types;

namespace Enterprise.Customs.IN.Business;

public interface IMessageSendingObject
{
	IMessageAttachee MessageAttachee { get; }
	ZString MessageType { get; }
}
