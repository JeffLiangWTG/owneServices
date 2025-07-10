using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business;

sealed class DefaultInboundInterchangeProcessingStrategy : IInboundInterchangeProcessingStrategy
{
	void IInboundInterchangeProcessingStrategy.ProcessInterchange(EDIInterchange interchange)
	{
		var singleMessageCreator = new InboundSingleMessageCreator();
		_ = singleMessageCreator.CreateMessageForInterchange(interchange);
	}
}
