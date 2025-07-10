using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging;

public interface IIM409Provider
{
	ZString MovementReferenceNumber { get; }

	ZBool InvalidationDecision { get; }

	ZBool InvalidationInitiatedByCustoms { get; }

	ZString InvalidationJustification { get; }

	ZDateTime DateOfInvalidationDecision { get; }

	ZDateTime DateOfInvalidationRequest { get; }

	ZDateTime DateOfInvalidation { get; }
}
