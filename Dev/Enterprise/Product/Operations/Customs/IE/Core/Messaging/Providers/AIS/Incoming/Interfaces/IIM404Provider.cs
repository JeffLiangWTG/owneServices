using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging;

public interface IIM404Provider
{
	ZString MovementReferenceNumber { get; }

	ZDateTime AmendmentAcceptanceDateAndTime { get; }

	ZString PreferredPaymentMethod { get; }

	ZString Remarks { get; }

	void UpdateMessageAttacheeCore(IMessageAttachee messageAttachee);
}
