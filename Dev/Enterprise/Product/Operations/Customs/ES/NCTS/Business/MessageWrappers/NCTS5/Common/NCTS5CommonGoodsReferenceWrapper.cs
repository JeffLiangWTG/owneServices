using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NCTS5CommonGoodsReferenceWrapper : INCTSCommonGoodsReference
	{
		public NCTS5CommonGoodsReferenceWrapper(ZShort seqNum, ZInt goodsItemNumber)
		{
			SequenceNumber = seqNum.ToString();
			DeclarationGoodsItemNumber = goodsItemNumber.ToString();
		}

		public ZString SequenceNumber { get; }

		public ZString DeclarationGoodsItemNumber { get; }
	}
}
