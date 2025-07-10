namespace Enterprise.Customs.DE.NCTS.Business;

public class NctsHeaderMessageSendingObject : EU.NCTS.Business.NctsHeaderMessageSendingObject
{
	public NctsHeaderMessageSendingObject(EU.NCTS.Business.NctsHeader nctsHeader) : base(nctsHeader)
	{
	}

	protected override EU.NCTS.Business.NctsHeaderMessageSendingObjectValidation GetNewValidation() => new NctsHeaderMessageSendingObjectValidation(this);
}
