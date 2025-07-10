using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders;

public interface ICommonAuthorisation
{
	ZString SequenceNumber { get; }
	ZString Type { get; }
	ZString ReferenceNumber { get; }
	ZString Holder { get; }
}
