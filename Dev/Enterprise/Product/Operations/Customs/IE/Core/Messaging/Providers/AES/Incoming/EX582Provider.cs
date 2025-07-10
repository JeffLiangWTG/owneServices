using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.EX582;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging
{
	public class EX582Provider
	{
		public EX582Provider(Ex582 xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Ex582 xmlObject;

		public ZString MRN => xmlObject.ExportOperation?.Mrn;
		public ZString LRN => xmlObject.ExportOperation?.Lrn;
		public ZDateTime RequestDate => new ZDateTime(xmlObject.ExportOperation?.RequestDate);
		public ZDateTime DateLimit => new ZDateTime(xmlObject.ExportOperation?.DateLimit);
		public IReadOnlyCollection<DocumentAdditionalInformationProvider> AdditionalInformations
			=> additionalInformations ?? (additionalInformations = xmlObject.AdditionalInformation?.Select(x => new DocumentAdditionalInformationProvider(x)).ToArray() ?? Array.Empty<DocumentAdditionalInformationProvider>());
		IReadOnlyCollection<DocumentAdditionalInformationProvider> additionalInformations;
	}
}
