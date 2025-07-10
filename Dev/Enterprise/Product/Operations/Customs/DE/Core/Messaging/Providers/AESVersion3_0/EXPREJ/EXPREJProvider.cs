using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.AES;
using CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0;

namespace Enterprise.Customs.DE.Messaging.AESVersion3_0
{
	public class EXPREJProvider : IEXPREJ
	{
		public EXPREJProvider(DEXPJE message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly DEXPJE message;

		public string ExportStatus => message.ExportOperation.businessRejectionType.XmlEnumToString().ValueOrNullIfEmpty();

		public string MovementReferenceNumber => message.ExportOperation.MRN;

		public string LocalReferenceNumber => message.ExportOperation.LRN;

		public string ReferencedMessageIdentifier => message.correlationIdentifier;

		public string MessageIdentifier => message.messageIdentification;
	}
}
