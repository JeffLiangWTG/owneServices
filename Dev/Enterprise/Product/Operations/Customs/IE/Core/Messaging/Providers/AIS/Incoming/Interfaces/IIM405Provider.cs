using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging;

public interface IIM405Provider
{
	ZString MovementReferenceNumber { get; }

	ZDateTime AmendmentRejectionDate { get; }

	ZString AmendmentRejectionMotivationText { get; }

	ZString Remarks { get; }
}
