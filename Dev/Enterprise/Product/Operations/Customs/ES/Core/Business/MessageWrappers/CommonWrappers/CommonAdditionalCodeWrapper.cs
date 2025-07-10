using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class CommonAdditionalCodeWrapper : ICommonAdditionalCode
{
	public CommonAdditionalCodeWrapper(ZShort seqNum, ZString code)
	{
		SequenceNumber = seqNum.ToString();
		Code = code;
	}

	public ZString SequenceNumber { get; }

	public ZString Code { get; }
}
