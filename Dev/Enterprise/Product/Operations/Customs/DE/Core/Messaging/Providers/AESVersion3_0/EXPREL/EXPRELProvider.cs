using System;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.AES;
using CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0;

namespace Enterprise.Customs.DE.Messaging.AESVersion3_0
{
	public class EXPRELProvider : IEXPREL
	{
		public EXPRELProvider(DEXPRF message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly DEXPRF message;

		public string MovementReferenceNumber => message.ExportOperation.MRN;

		public string LocalReferenceNumber => message.ExportOperation.LRN;

		public DateTime IssuingDateTime => message.ExportOperation.releaseDateAndTime;

		public string ReferencedMessageIdentifier => message.correlationIdentifier;

		public string MessageIdentifier => message.messageIdentification;
	}
}
