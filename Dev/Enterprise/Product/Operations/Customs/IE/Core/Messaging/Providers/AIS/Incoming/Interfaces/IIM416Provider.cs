using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging;

public interface IIM416Provider
{
	ZString AdditionalDeclarationType { get; }

	ZString LocalReferenceNumber { get; }

	ZDateTime RejectionDate { get; }

	ZString RejectionMotivationText { get; }

	ZBool HasFunctionalErrors { get; }

	string EntryStatus {  get; }
}
