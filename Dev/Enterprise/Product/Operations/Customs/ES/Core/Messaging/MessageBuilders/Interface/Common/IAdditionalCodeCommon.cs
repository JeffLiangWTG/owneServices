using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders;

public interface ICommonAdditionalCode
{
	ZString SequenceNumber { get; }
	ZString Code { get; }
}
