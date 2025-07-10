using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1
{
	public sealed class ERRNCKProvider : IERRNCK
	{
		public ERRNCKProvider(DEERRF message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly DEERRF message;

		public string MessageIdentifier => message.MetaData.MessageIdentifier;

		public string ReferencedMessageIdentifier => message.Header.ReferencedMessageIdentifier;

		public string ReferenceNumber => message.Header.ReferenceNumber;

		public string LocalReferenceNumber => message.Header.LocalReferenceNumber;

		public string MessageGroup => null;

		public IReadOnlyCollection<IERRNCKError> Errors => errors ?? (errors = message.Error.Select(x => new ERRNCKErrorProvider(x)).ToArray());

		IReadOnlyCollection<IERRNCKError> errors;
	}
}
