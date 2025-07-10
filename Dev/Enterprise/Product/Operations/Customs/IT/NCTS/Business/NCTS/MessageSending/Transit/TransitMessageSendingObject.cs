using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.NCTS.Business;

public class TransitMessageSendingObject : NctsHeaderDepartureSADMessageSendingObject
{
	public TransitMessageSendingObject(NctsHeader nctsHeader) : base(nctsHeader)
	{
	}

	protected override IETHeader GetMessageHeader(NctsHeader nctsHeader) => new TransitHeaderWrapper(nctsHeader);

	protected override IETLine GetMessageLine(NctsDepartureCargoDesc goodsItem) => new TransitLineWrapper(goodsItem);
}
