using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging;

public interface IIM933Provider
{
	ZString MovementReferenceNumber { get; }

	ZString NotificationRejectionReason { get; }

	ZDateTime NotificationRejectionDate { get; }
}
