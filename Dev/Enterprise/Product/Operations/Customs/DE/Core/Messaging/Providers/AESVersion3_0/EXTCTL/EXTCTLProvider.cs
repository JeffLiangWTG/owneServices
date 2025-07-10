using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.AES;
using CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0;

namespace Enterprise.Customs.DE.Messaging.AESVersion3_0
{
	public sealed class EXTCTLProvider : IEXTCTL
	{
		public EXTCTLProvider(DEXTLF message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}

		public string MessageIdentifier => message.messageIdentification;

		public string ReferencedMessageIdentifier => message.correlationIdentifier;

		public string MovementReferenceNumber => message.ExportOperation.MRN;

		public IReadOnlyCollection<IEXTCTLTypeOfControls> TypeOfControls =>
			typeOfControls ??
			(typeOfControls = message.TypeOfControls.Select(t => new EXTCTLTypeOfControls(t.type, t.text)).ToArray());
		IReadOnlyCollection<IEXTCTLTypeOfControls> typeOfControls;

		readonly DEXTLF message;
	}
}
