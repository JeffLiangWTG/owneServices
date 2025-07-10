using CargoWise.Types;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business
{
	public interface IMessageSendingAction
	{
		ZString MessageType { get; }
		IMessageAttachee MessageAttachee { get; }
		string MessageCreated(string messageText);
		void AddMessage(OutboundEDIMessage message);
	}
}
