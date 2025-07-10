using CargoWise.Customs.DE.MessageContracts;

namespace Enterprise.Customs.DE.Messaging
{
	public interface ISRAREV : IDataProvider
	{
		string CancelledReferenceNumber { get; }

		string CancelledMRN { get; }

		string InterchangeRecipientEBS { get; }
	}
}
