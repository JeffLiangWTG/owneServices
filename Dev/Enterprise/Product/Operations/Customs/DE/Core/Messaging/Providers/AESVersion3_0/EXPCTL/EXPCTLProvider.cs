using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.AES;
using CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0;

namespace Enterprise.Customs.DE.Messaging.AESVersion3_0
{
	public class EXPCTLProvider : IEXPCTL
	{
		public EXPCTLProvider(DEXPLD xmlObject)
		{
			this.xmlObject = Argument.NotNull(xmlObject, nameof(xmlObject));
		}
		readonly DEXPLD xmlObject;

		public string MovementReferenceNumber => xmlObject.ExportOperation.MRN;

		public string LocalReferenceNumber => xmlObject.ExportOperation.LRN;

		public string ReferencedMessageIdentifier => xmlObject.correlationIdentifier;

		public string MessageIdentifier => xmlObject.messageIdentification;

		public IReadOnlyCollection<IEXPCTLLine> Lines => lines ??= xmlObject.TypeOfControls.Select(x => new EXPCTLLineProvider(x)).ToArray();
		IReadOnlyCollection<IEXPCTLLine> lines;
	}
}
