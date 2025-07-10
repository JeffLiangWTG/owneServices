using EDIFACTConstants = Enterprise.Customs.AE.Business.AEConstants.Messaging.EDIFACT;

namespace Enterprise.Customs.AE.Business;

public class CUSCARInterchangeHeaderProvider : InterchangeHeaderProvider
{
	public CUSCARInterchangeHeaderProvider(string senderId, string senderInternalId, string senderInternalSubId) : base(senderId, senderInternalId, senderInternalSubId, EDIFACTConstants.EDIRecipient)
	{
	}

	public override string ProcessingPriority => EDIFACTConstants.ProcessingPriority;
}
