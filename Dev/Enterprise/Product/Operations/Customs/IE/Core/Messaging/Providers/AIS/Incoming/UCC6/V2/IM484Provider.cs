using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Messaging
{
	public class IM484Provider : IIM484Provider
	{
		public IM484Provider(IIM484XmlObject xmlObject)
		{
			this.xmlObject = xmlObject;
		}

		readonly IIM484XmlObject xmlObject;

		public ZString LocalReferenceNumber => xmlObject.ImportOperation?.Lrn;

		public ZString MovementReferenceNumber => xmlObject.ImportOperation?.Mrn;

		public ZDateTime DateLimit => (xmlObject.ImportOperation?.DateLimit).ConvertToZDateTime();

		public ZDateTime RequestDate => (xmlObject.ImportOperation?.RequestDate).ConvertToZDateTime();

		public IReadOnlyCollection<IDocumentAdditionalInformationProvider> AdditionalInformations
			=> additionalInformations ?? (additionalInformations = xmlObject.GoodsShipments?.Select(x => new DocumentAdditionalInformationProvider(x)).ToArray() ?? Array.Empty<DocumentAdditionalInformationProvider>());
		IReadOnlyCollection<IDocumentAdditionalInformationProvider> additionalInformations;
	}
}
