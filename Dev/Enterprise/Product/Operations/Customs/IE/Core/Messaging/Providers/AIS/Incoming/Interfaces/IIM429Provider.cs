using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IE.Messaging;

public interface IIM429Provider
{
	ZString LocalReferenceNumber { get; }

	ZString MovementReferenceNumber { get; }

	ZString AdditionalDeclarationType { get; }

	ZString PreferredPaymentMethod { get; }

	ZString Remarks { get; }

	void UpdateMessageAttacheeCore(IMessageAttachee messageAttachee);

	string GetEntryStatus(EDIMessage message);
}
