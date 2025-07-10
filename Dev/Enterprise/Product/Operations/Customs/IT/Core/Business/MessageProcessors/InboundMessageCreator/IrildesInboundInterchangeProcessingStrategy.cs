using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business;

sealed class IrildesInboundInterchangeProcessingStrategy : IInboundInterchangeProcessingStrategy
{
	void IInboundInterchangeProcessingStrategy.ProcessInterchange(EDIInterchange interchange)
	{
		var irildesRequestInterchange = MessageProcessorHelper.GetSentInterchangeWithTrackingId(interchange.EI_SessionGUID, interchange.Factory)
			?? throw new CouldNotFindRelatedBusinessObjectException(MessageProcessorHelper.UnableToLocateTheRelatedSentInterchangeWithSessionGuid(interchange.EI_SessionGUID));

		if (ShouldSkipIrildesResponse(interchange))
		{
			irildesRequestInterchange.CreateOrReOpenPollingTransaction();
		}
		else
		{
			var singleMessageCreator = new InboundSingleMessageCreator();
			_ = singleMessageCreator.CreateMessageForInterchange(interchange);

			irildesRequestInterchange.DeletePollingTransactions();
		}
	}

	bool ShouldSkipIrildesResponse(EDIInterchange interchange) => new IrildesResponseMessage(interchange.EI_BodyText) is var responseMessage
		&& (responseMessage.IsElaborationKO && responseMessage.HasIrildesNotAvailableTag) || responseMessage.IsServiceNotAvailable;
}
