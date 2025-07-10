using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE917;
using CargoWise.Types;

namespace Enterprise.Customs.IE.EMCS.Messaging.Phase4_1
{
	public class IE917Provider : IIE917
	{
		public IE917Provider(Ie917Type message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly Ie917Type message;

		public IReadOnlyCollection<IXMLError> Errors => errors ?? (errors = message.Body.XmlNegativeAcknowledgement.XmlError.Select(e => new IE917ErrorProvider(e)).ToArray());
		IReadOnlyCollection<IXMLError> errors;

		public ZString MrnNumber => AdministrativeReferenceCode;

		public ZString MrnNumberSequenceNumber => message.Body.XmlNegativeAcknowledgement.Attributes?.SequenceNumber;

		public ZString AdministrativeReferenceCode => message.Body.XmlNegativeAcknowledgement.Attributes?.AdministrativeReferenceCode;
	}

	class IE917ErrorProvider : IXMLError
	{
		public IE917ErrorProvider(XmlErrorType xmlError)
		{
			this.xmlError = Argument.NotNull(xmlError, nameof(xmlError));
		}
		readonly XmlErrorType xmlError;

		public ZString ErrorLineNumber => xmlError.ErrorLineNumber;

		public ZString ErrorColumnNumber => xmlError.ErrorColumnNumber;

		public ZString ErrorReason => xmlError.ErrorReason;

		public ZString ErrorLocation => xmlError.ErrorLocation;

		public ZString OriginalAttributeValue => xmlError.OriginalAttributeValue;
	}
}
