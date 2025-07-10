using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM482;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.UCC5
{
	public class IM482Provider : IDocumentRequestMessageProvider
	{
		public IM482Provider(Im482 xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Im482 xmlObject;

		public ZString MovementReferenceNumber => xmlObject.Declaration.Mrn;

		public ZString LRN => xmlObject.Declaration.Lrn25;

		public ZDateTime RequestDate
		{
			get
			{
				new ZString(xmlObject.Declaration.RequestDate).TryParseToDate(out var requestDate);
				return requestDate;
			}
		}

		public ZDateTime DateLimit
		{
			get
			{
				new ZString(xmlObject.Declaration.DateLimit).TryParseToDate(out var limitDate);
				return limitDate;
			}
		}

		public IReadOnlyCollection<DocumentAdditionalInformationProvider> AdditionalInformations => additionalInformations ??= xmlObject.AdditionalInformation?.Select(x => new DocumentAdditionalInformationProvider(x)).ToArray() ?? Array.Empty<DocumentAdditionalInformationProvider>();
		IReadOnlyCollection<DocumentAdditionalInformationProvider> additionalInformations;
	}
}
