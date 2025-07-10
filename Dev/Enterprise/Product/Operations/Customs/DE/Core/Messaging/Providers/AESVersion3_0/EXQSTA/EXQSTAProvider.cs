using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.AES;
using CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0;

namespace Enterprise.Customs.DE.Messaging.AESVersion3_0
{
	public class EXQSTAProvider : IEXQSTA
	{
		public EXQSTAProvider(DEXQSB message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly DEXQSB message;

		public string ReferencedMessageIdentifier => message.correlationIdentifier;

		public string MovementReferenceNumber => message.ExportOperation.MRN;

		public string MessageIdentifier => message.messageIdentification;
	}
}
