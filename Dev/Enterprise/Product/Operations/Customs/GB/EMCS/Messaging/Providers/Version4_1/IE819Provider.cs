using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie819;
using CargoWise.Types;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1
{
	public sealed class IE819Provider : IIE819
	{
		public IE819Provider(Ie819Type message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly Ie819Type message;

		public IEMCSEvent ExciseMovementEad => exciseMovementEad ?? (exciseMovementEad = new IE819EventProvider(message.Body.AlertOrRejectionOfEadesad.ExciseMovement));
		IEMCSEvent exciseMovementEad;

		public IReadOnlyCollection<IIE819Reason> AlertOrRejectionReasons => alertOrRejectionReasons ?? (alertOrRejectionReasons = message.Body.AlertOrRejectionOfEadesad.AlertOrRejectionOfEadEsadReason?.Select(x => new IE819ReasonProvider(x)).ToArray() ?? Array.Empty<IIE819Reason>());
		IReadOnlyCollection<IIE819Reason> alertOrRejectionReasons;

		public ZString MrnNumber => ExciseMovementEad.AdministrativeReferenceCode;

		public ZString MrnNumberSequenceNumber => ExciseMovementEad.SequenceNumber;
	}

	sealed class IE819EventProvider : IEMCSEvent
	{
		public IE819EventProvider(ExciseMovementType exciseMovementEad)
		{
			this.exciseMovementEad = Argument.NotNull(exciseMovementEad, nameof(exciseMovementEad));
		}
		readonly ExciseMovementType exciseMovementEad;

		public ZString AdministrativeReferenceCode => exciseMovementEad.AdministrativeReferenceCode;

		public ZString SequenceNumber => exciseMovementEad.SequenceNumber;
	}

	sealed class IE819ReasonProvider : IIE819Reason
	{
		public IE819ReasonProvider(AlertOrRejectionOfEadEsadReasonType alertOrRejectReason)
		{
			this.alertOrRejectReason = Argument.NotNull(alertOrRejectReason, nameof(alertOrRejectReason));
		}
		readonly AlertOrRejectionOfEadEsadReasonType alertOrRejectReason;

		public ZString ReasonCode => alertOrRejectReason.AlertOrRejectionOfMovementReasonCode;

		public ZString ComplementaryInformation => alertOrRejectReason.ComplementaryInformation.Value;
	}
}
