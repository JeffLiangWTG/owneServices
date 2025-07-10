using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class CommonDangerousGoodsWrapper : ICommonDangerousGoods
	{
		public CommonDangerousGoodsWrapper(ZShort seqNum, ZString code)
		{
			SequenceNumber = seqNum.ToString();
			UNDangerousCode = code;
		}

		public ZString SequenceNumber { get; }

		public ZString UNDangerousCode { get; }
	}
}
