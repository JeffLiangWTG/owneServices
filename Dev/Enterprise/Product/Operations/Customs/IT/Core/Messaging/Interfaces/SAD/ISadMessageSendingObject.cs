using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.SAD;

public interface ISadMessageSendingObject
{
	ZBool FallbackProcedure { get; }
	ZString DeclarantTaxNumber { get; }
}
