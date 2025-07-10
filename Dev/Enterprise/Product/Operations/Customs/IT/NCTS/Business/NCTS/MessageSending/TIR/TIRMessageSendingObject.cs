using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.NCTS.Business;

public class TIRMessageSendingObject : NctsHeaderDepartureSADMessageSendingObject
{
	public TIRMessageSendingObject(NctsHeader nctsHeader) : base(nctsHeader)
	{
	}

	protected override IETHeader GetMessageHeader(NctsHeader nctsHeader) => new TIRHeaderWrapper(nctsHeader);

	protected override IETLine GetMessageLine(NctsDepartureCargoDesc goodsItem) => new TIRLineWrapper(goodsItem);
}
