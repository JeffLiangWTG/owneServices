using CargoWise.Common;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AE.Business;

class MessageDataProviderFactory : IMessageDataProviderFactory
{
	internal static readonly Overridable<IMessageDataProviderFactory> Instance = new(new MessageDataProviderFactory());

	public IInboundMessageDataProvider GetMessageDataProvider(EDIMessage message)
	{
		return (string)message.EM_MessageType switch
		{
			AEConstants.Messaging.MessageTypes.CONTRL => new CONTRLDataProvider(message.EM_MessageText),
			AEConstants.Messaging.MessageTypes.CUSRES => new CUSRESDataProvider(message.EM_MessageText),
			AEConstants.Messaging.MessageTypes.XTTERR => new XTTERRDataProvider(message),
			AEConstants.Messaging.MessageTypes.DOCSUC => new DOCSUCDataProvider(message),
			AEConstants.Messaging.MessageTypes.DOCERR => new DOCERRDataProvider(message),
			_ => null,
		};
	}
}
