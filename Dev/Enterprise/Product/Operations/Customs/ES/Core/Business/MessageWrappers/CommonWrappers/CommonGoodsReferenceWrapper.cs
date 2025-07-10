using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class CommonGoodsReferenceWrapper : ICommonGoodsReference
{
	public CommonGoodsReferenceWrapper(ZShort seqNum, ZShort itemNum)
	{
		SequenceNumber = seqNum.ToString();
		GoodsItemNumber = itemNum.ToString();
	}

	public ZString SequenceNumber { get; }

	public ZString GoodsItemNumber { get; }
}
