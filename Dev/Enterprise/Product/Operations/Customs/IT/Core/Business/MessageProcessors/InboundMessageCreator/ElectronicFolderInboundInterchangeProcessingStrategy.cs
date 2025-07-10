using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business;

sealed class ElectronicFolderInboundInterchangeProcessingStrategy : IInboundInterchangeProcessingStrategy
{
	void IInboundInterchangeProcessingStrategy.ProcessInterchange(EDIInterchange interchange)
	{
		var singleMessageCreator = new InboundSingleMessageCreator();
		var message = singleMessageCreator.CreateMessageForInterchange(interchange);

		message.EM_MessageType = MessageProcessorConstants.InterchangeTypes.ElectronicFolderResponseType;
		message.EM_MessageSubType = MessageProcessorConstants.InterchangeTypes.ElectronicFolderResponseType;
	}
}
