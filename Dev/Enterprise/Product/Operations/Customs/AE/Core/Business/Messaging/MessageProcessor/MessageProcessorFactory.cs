using CargoWise.Common;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AE.Business;

sealed class MessageProcessorFactory : IMessageProcessorFactory
{
	internal static readonly Overridable<IMessageProcessorFactory> Instance = new(new MessageProcessorFactory());

	public IMessageProcessor<IInboundMessageDataProvider> GetMessageProcessor(EDIMessage message)
	{
		return (string)message.EM_MessageType switch
		{
			AEConstants.Messaging.MessageTypes.CONTRL => new CONTRLMessageProcessor(),
			AEConstants.Messaging.MessageTypes.CUSRES => new CUSRESMessageProcessor(),
			AEConstants.Messaging.MessageTypes.XTTERR => new XTTERRMessageProcessor(),
			AEConstants.Messaging.MessageTypes.DOCSUC => new DOCSUCMessageProcessor(),
			AEConstants.Messaging.MessageTypes.DOCERR => new DOCERRMessageProcessor(),
			_ => null,
		};
	}
}
