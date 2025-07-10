namespace Enterprise.Customs.AE.Business;

public class CONTRLInterchangeHeaderProvider : InterchangeHeaderProvider
{
	public CONTRLInterchangeHeaderProvider(string senderId, string senderInternalId, string senderInternalSubId, string recipientId) : base(senderId, senderInternalId, senderInternalSubId, recipientId)
	{
	}

	public override string ProcessingPriority => null;
}
