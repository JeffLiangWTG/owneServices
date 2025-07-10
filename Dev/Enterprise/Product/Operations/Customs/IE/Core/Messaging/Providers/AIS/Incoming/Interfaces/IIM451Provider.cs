using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging;

public interface IIM451Provider
{
	ZString AdditionalDeclarationType { get; }

	ZString LocalReferenceNumber { get; }

	ZString MovementReferenceNumber { get; }

	ZString DecisionReason { get; }

	ZString PreferredPaymentMethod { get; }

	ZString Remarks { get; }
}
