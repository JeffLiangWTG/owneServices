using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_5
{
	public class ED704Provider : IED704
	{
		public ED704Provider(ED704C message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly ED704C message;

		public ZString MessageGroup => message.Header.MessageGroup.XmlEnumToString();

		public string MessageIdentifier => message.Header.MessageIdentifier;

		public ZString CorrelationIdentifier => message.Header.CorrelationIdentifier;

		public ZString LocalReferenceNumber => message.Body.GenericRefusalMessage.Attributes?.LocalReferenceNumber;

		public ZString AdministrativeReferenceCode => message.Body.GenericRefusalMessage.Attributes?.AdministrativeReferenceCode;

		public IReadOnlyCollection<IEMCSError> Errors => errors ?? (errors = message.Body.GenericRefusalMessage.Error.Select(x => new ED704ErrorProvider(x)).ToArray());
		IReadOnlyCollection<IEMCSError> errors;
	}

	class ED704ErrorProvider : IEMCSError
	{
		public ED704ErrorProvider(ED704CBodyGenericRefusalMessageError error)
		{
			this.error = Argument.NotNull(error, nameof(error));
		}
		readonly ED704CBodyGenericRefusalMessageError error;

		public ZString ErrorNumber => error.ErrorNumber;

		public ZString LineNumber => error.LineNumber;

		public ZString ColumnNumber => error.ColumnNumber;

		public ZString ErrorType => error.ErrorType.XmlEnumToString();

		public ZString ErrorReason => error.ErrorReason;
	}
}
