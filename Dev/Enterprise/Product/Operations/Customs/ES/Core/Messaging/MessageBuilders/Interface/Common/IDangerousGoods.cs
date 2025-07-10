using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public interface ICommonDangerousGoods
	{
		ZString SequenceNumber { get; }
		ZString UNDangerousCode { get; }
	}
}
