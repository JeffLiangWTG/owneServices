using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0;

namespace Enterprise.Customs.DE.Messaging.AESVersion3_0
{
	public sealed class ERRNCKProvider : IERRNCK
	{
		public ERRNCKProvider(DEERRG message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly DEERRG message;

		public string MessageIdentifier => message.messageIdentification;

		public string ReferencedMessageIdentifier => message.correlationIdentifier;

		public string ReferenceNumber => message.Header?.MRN;

		public string LocalReferenceNumber => message.Header?.LRN;

		public string MessageGroup => message.messageGroup.XmlEnumToString().ValueOrNullIfEmpty();

		public IReadOnlyCollection<IERRNCKError> Errors => errors ?? (errors = message.Error.Select(x => new ERRNCKErrorProvider(x)).ToArray());
		IReadOnlyCollection<IERRNCKError> errors;
	}
}
