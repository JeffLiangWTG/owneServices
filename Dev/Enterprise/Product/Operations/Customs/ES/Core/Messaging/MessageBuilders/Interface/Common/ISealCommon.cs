using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public interface ISealCommon
	{
		ZString SequenceNumber { get; }
		ZString SealNumber { get; }
	}
}
