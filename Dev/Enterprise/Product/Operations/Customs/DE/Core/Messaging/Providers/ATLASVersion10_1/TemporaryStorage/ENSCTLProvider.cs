using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1
{
	public class ENSCTLProvider : IENSCTL
	{
		public ENSCTLProvider(DEIACA message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly DEIACA message;

		public string MessageIdentifier => message.messageIdentification;

		public string MessageRecipientIdentificationNumber => message.MessageRecipient.identificationNumber;

		public string MessageRecipientSubsidiaryNumber => message.MessageRecipient.subsidiaryNumber;

		public string MRN => message.MRN;

		public string TransportDocumentNumber => message.transportDocument?.documentNumber;
	}
}
