using CargoWise.Customs.IE.MessageDefinitions;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;
using BaseEDIMessage = Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.IE.Business
{
	public class MessageAcknowledgementAdditionalProcessor : IAdditionalMessageProcessing
	{
		public void Process(BaseEDIMessage originalMessage, BaseEDIMessage targetMessage, ITransaction transactionProvider)
		{
			if (originalMessage is not null && GetMessageInterpreter(originalMessage, transactionProvider) is InboundMessageInterpreter<ITransaction> interpreter)
			{
				targetMessage.EM_MessageInterpretation = interpreter.GetInterpretation();
			}

			if (targetMessage.EM_LinkedObject is CusEntryHeader entryHeader)
			{
				entryHeader.Declaration.LogCustomsCommencedIfNeeded();
			}
		}

		protected virtual InboundMessageInterpreter<ITransaction> GetMessageInterpreter(BaseEDIMessage message, ITransaction transactionProvider) => null;
	}
}
