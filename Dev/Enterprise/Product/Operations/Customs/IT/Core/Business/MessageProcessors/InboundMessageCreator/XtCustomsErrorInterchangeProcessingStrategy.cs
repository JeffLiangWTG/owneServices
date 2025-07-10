using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business;

sealed class XtCustomsErrorInterchangeProcessingStrategy : IInboundInterchangeProcessingStrategy
{
	void IInboundInterchangeProcessingStrategy.ProcessInterchange(EDIInterchange interchange)
	{
		var originalSentInterchange = MessageProcessorHelper.GetSentInterchangeWithTrackingId(interchange.EI_SessionGUID, interchange.Factory)
			?? throw new CouldNotFindRelatedBusinessObjectException(MessageProcessorHelper.UnableToLocateTheRelatedSentInterchangeWithSessionGuid(interchange.EI_SessionGUID));

		var singleMessageCreator = new InboundSingleMessageCreator();
		_ = singleMessageCreator.CreateMessageForInterchange(interchange);

		ProcessPollingTransactionsIfNeeded(originalSentInterchange);
	}

	static void ProcessPollingTransactionsIfNeeded(ITEDIInterchange originalSentInterchange)
	{
		if (originalSentInterchange.EI_InterchangeType.ToString() is EDIMessageTypeList.Codes.IvistoRequest or EDIMessageTypeList.Codes.IrildesRequest)
		{
			originalSentInterchange.CreateOrReOpenPollingTransaction();
		}
	}
}
