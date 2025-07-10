using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging;

public interface IIM428Provider
{
	ZString LocalReferenceNumber { get; }

	ZString MovementReferenceNumber { get; }

	ZDateTime DeclarationAcceptanceDateAndTime { get; }

	ZString AdditionalDeclarationType { get; }

	ZString PreferredPaymentMethod { get; }

	ZString Remarks { get; }

	void UpdateMessageAttacheeCore(IMessageAttachee messageAttachee);
}
