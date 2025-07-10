using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie704uk;
using CargoWise.Types;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1
{
	public sealed class IE704Provider : IIE704
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

		public IReadOnlyCollection<IIE704FunctionalError> Errors => errors ?? (errors = message.Body.GenericRefusalMessage.FunctionalError.Select(e => new IE704ErrorProvider(e)).ToArray());
		IReadOnlyCollection<IIE704FunctionalError> errors;
	}
}
