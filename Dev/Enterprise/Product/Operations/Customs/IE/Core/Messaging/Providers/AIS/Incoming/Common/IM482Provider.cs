using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Messaging
{
	public class IM482Provider
	{
		public IM482Provider(IIM482XmlObject xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly IIM482XmlObject xmlObject;

		public ZString MRN => xmlObject.ImportOperation?.Mrn;

		public ZString CaseId => xmlObject.ImportOperation?.Lrn;

		public ZDateTime RequestDate => (xmlObject.ImportOperation?.RequestDate).ConvertToZDateTime();

		public ZDateTime DateLimit => (xmlObject.ImportOperation?.DateLimit).ConvertToZDateTime();

		public IReadOnlyCollection<IDocumentAdditionalInformationProvider> AdditionalInformations
			=> additionalInformations ?? (additionalInformations = xmlObject.DocumentAdditionalInformation?.Select(x => new DocumentAdditionalInformationProvider(x)).ToArray() ?? Array.Empty<DocumentAdditionalInformationProvider>());

		IReadOnlyCollection<IDocumentAdditionalInformationProvider> additionalInformations;
	}
}
