using CargoWise.Customs.IE.MessageDefinitions;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IE.Messaging
{
	public interface IAdditionalMessageProcessing
	{
		void Process(EDIMessage originalMessage, EDIMessage targetMessage, ITransaction transactionProvider);
	}
}
