using CargoWise.Customs.DE.MessageContracts;

namespace Enterprise.Customs.DE.Messaging
{
	public interface IENSCTL : IDataProvider
	{
		string MessageRecipientIdentificationNumber { get; }

		string MessageRecipientSubsidiaryNumber { get; }

		string MRN {  get; }

		string TransportDocumentNumber { get; }
	}
}
