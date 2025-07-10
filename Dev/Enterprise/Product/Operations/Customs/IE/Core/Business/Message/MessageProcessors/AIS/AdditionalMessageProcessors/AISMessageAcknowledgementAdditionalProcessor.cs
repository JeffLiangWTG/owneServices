using CargoWise.Customs.IE.MessageDefinitions;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class AISMessageAcknowledgementAdditionalProcessor : MessageAcknowledgementAdditionalProcessor
	{
		protected override InboundMessageInterpreter<ITransaction> GetMessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, ITransaction transactionProvider) => new AISMessageAcknowledgementInterpreter(message, transactionProvider);
	}
}
