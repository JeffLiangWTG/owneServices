using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.EX584;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.AES
{
	public class EX584Provider
	{
		public EX584Provider(Ex584 xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Ex584 xmlObject;

		public ZString MovementReferenceNumber => xmlObject.ExportOperation?.Mrn;
		public ZString LocalReferenceNumber => xmlObject.ExportOperation?.Lrn;
		public ZDateTime RequestDate => new ZDateTime(xmlObject.ExportOperation?.RequestDate);
		public ZDateTime DateLimit => new ZDateTime(xmlObject.ExportOperation?.DateLimit);
		public IReadOnlyCollection<DocumentAdditionalInformationProvider> AdditionalInformation => additionalInformation ?? (additionalInformation = xmlObject.GoodsShipment?.Select(x => new DocumentAdditionalInformationProvider(x)).ToArray() ?? Array.Empty<DocumentAdditionalInformationProvider>());
		IReadOnlyCollection<DocumentAdditionalInformationProvider> additionalInformation;
	}
}
