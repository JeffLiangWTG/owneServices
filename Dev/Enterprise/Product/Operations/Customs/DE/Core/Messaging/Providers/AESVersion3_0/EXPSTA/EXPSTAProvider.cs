using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.AES;
using CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0;

namespace Enterprise.Customs.DE.Messaging.AESVersion3_0
{
	public class EXPSTAProvider : IEXPSTA
	{
		public EXPSTAProvider(DEXPSE message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly DEXPSE message;

		public string MessageIdentifier => message.messageIdentification;

		public string ReferencedMessageIdentifier => message.correlationIdentifier;

		public string ExportStatus => message.ExportOperation.exportStatus.XmlEnumToString().ValueOrNullIfEmpty();

		public string MovementReferenceNumber => message.ExportOperation.MRN;

		public string LocalReferenceNumber => message.ExportOperation.LRN;

		public string CustomsOfficeOfExport => message.CustomsOfficeOfExport?.referenceNumber;
	}
}
