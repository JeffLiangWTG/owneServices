using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE704;
using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Messaging;

namespace Enterprise.Customs.IE.EMCS.Messaging.Phase4_1
{
	public class IE704Provider : IIE704
	{
		public IE704Provider(Ie704Type message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly Ie704Type message;

		public ZString MrnNumber => AdministrativeReferenceCode;

		public ZString MrnNumberSequenceNumber => message.Body.GenericRefusalMessage.Attributes?.SequenceNumber;

		public ZString AdministrativeReferenceCode => message.Body.GenericRefusalMessage.Attributes?.AdministrativeReferenceCode;

		public ZString LocalReferenceNumber => message.Body.GenericRefusalMessage.Attributes?.LocalReferenceNumber;

		public IReadOnlyCollection<IFunctionalError> Errors => errors ?? (errors = message.Body.GenericRefusalMessage.FunctionalError.Select(e => new IE704ErrorProvider(e)).ToArray());

		IReadOnlyCollection<IFunctionalError> errors;
	}

	public class IE704ErrorProvider : IFunctionalError
	{
		public IE704ErrorProvider(FunctionalErrorType functionalError)
		{
			this.functionalError = Argument.NotNull(functionalError, nameof(functionalError));
		}
		readonly FunctionalErrorType functionalError;

		public ZString ErrorLocation => functionalError.ErrorLocation;

		public ZString ErrorType => functionalError.ErrorType.XmlEnumToString();

		public ZString ErrorReason => functionalError.ErrorReason;

		public ZString OriginalAttributeValue => functionalError.OriginalAttributeValue;
	}
}
