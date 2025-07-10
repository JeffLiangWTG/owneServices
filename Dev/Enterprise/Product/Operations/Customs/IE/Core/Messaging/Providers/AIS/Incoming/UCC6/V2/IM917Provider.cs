using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions;

namespace Enterprise.Customs.IE.Messaging
{
	public class IM917Provider
	{
		public IM917Provider(IIM917XmlObject xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly IIM917XmlObject xmlObject;

		public IReadOnlyCollection<INegativeAcknowledgementError> Errors => errors ?? (errors = xmlObject.XmlNegativeAcknowledgements?.Select(x => new NegativeAcknowledgementErrorProvider(x)).ToArray() ?? Array.Empty<INegativeAcknowledgementError>());
		INegativeAcknowledgementError[] errors;
	}
}
