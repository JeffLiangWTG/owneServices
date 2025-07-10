using CargoWise.EntityFramework;
using Enterprise.Edifact.Generic.V4;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AE.Business;

sealed class CONTRLInterchangeSegmentProvider : IInterchangeSegmentProvider
{
	public bool TryGetHeaderSegment(BusinessObject messageParent, out UNBSegment headerSegment)
	{
		headerSegment = default;
		if (messageParent is EDIMessage { Interchange: { } requestInterchange })
		{
			var requestInterchangeHeaderSegment = new NAICInterchangeUnpacker().RetrieveInterchangeHeader(requestInterchange);

			var senderId = requestInterchangeHeaderSegment.InterchangeRecipient.RecipientIdentification;
			var senderInternalId = requestInterchangeHeaderSegment.InterchangeRecipient.InterchangeRecipientInternalIdentification;
			var senderInternalSubId = requestInterchangeHeaderSegment.InterchangeRecipient.InterchangeRecipientInternalSubIdentification;

			var recipientId = requestInterchangeHeaderSegment.InterchangeSender.SenderIdentification;

			headerSegment = CommonMessageBuilder.GetUNBSegment(new CONTRLInterchangeHeaderProvider(senderId, senderInternalId, senderInternalSubId, recipientId));
			return true;
		}
		return false;
	}
}
