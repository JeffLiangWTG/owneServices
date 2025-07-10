using System;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.AES;
using CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0;

namespace Enterprise.Customs.DE.Messaging.AESVersion3_0
{
	public class EXPURGProvider : IEXPURG
	{
		public EXPURGProvider(DEXPUC message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly DEXPUC message;

		public DateTime LatestResponseDate => message.ExportOperation.limitForResponseDate;

		public string ReferencedMessageIdentifier => message.correlationIdentifier;

		public string MessageIdentifier => message.messageIdentification;

		public string MovementReferenceNumber => message.ExportOperation.MRN;

		public string LocalReferenceNumber => message.ExportOperation.LRN;
	}
}
