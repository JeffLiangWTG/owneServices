using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business;

sealed class IvistoInboundInterchangeProcessingStrategy : IInboundInterchangeProcessingStrategy
{
	void IInboundInterchangeProcessingStrategy.ProcessInterchange(EDIInterchange interchange)
	{
		var ivistoRequestInterchange = MessageProcessorHelper.GetSentInterchangeWithTrackingId(interchange.EI_SessionGUID, interchange.Factory)
			?? throw new CouldNotFindRelatedBusinessObjectException(MessageProcessorHelper.UnableToLocateTheRelatedSentInterchangeWithSessionGuid(interchange.EI_SessionGUID));

		if (ShouldSkipIvistoResponse(interchange))
		{
			ivistoRequestInterchange.CreateOrReOpenPollingTransaction();
		}
		else
		{
			var singleMessageCreator = new InboundSingleMessageCreator();
			_ = singleMessageCreator.CreateMessageForInterchange(interchange);

			ivistoRequestInterchange.DeletePollingTransactions();
		}
	}

	bool ShouldSkipIvistoResponse(EDIInterchange interchange) => new Ucc6IvistoResponseMessage(interchange.EI_BodyText) is var responseMessage
		&& (responseMessage.IsElaborationKO && responseMessage.HasIvistoNotAvailableTag()) || responseMessage.IsServiceNotAvailable;
}
