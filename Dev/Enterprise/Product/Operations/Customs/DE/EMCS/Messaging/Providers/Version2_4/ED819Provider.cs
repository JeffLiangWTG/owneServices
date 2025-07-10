using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_4
{
	public class ED819Provider : IED819
	{
		public ED819Provider(ED819C message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly ED819C message;

		public ZString MessageGroup => message.Header.MessageGroup.XmlEnumToString();

		public string MessageIdentifier => message.Header.MessageIdentifier;

		public IEMCSEvent ExciseMovement => exciseMovementEad ?? (exciseMovementEad = new ED819EventProvider(message.Body.AlertOrRejectionOfAnEad.ExciseMovementEad));
		IEMCSEvent exciseMovementEad;

		public IReadOnlyCollection<IED819Reason> AlertOrRejectionReasons => alertOrRejectionReasons ?? (alertOrRejectionReasons = message.Body.AlertOrRejectionOfAnEad.AlertOrRejectionOfEadReason?.Select(x => new ED819ReasonProvider(x)).ToArray() ?? Array.Empty<IED819Reason>());
		IReadOnlyCollection<IED819Reason> alertOrRejectionReasons;
	}

	class ED819EventProvider : IEMCSEvent
	{
		public ED819EventProvider(ED819CBodyAlertOrRejectionOfAnEadExciseMovementEad exciseMovementEad)
		{
			this.exciseMovementEad = Argument.NotNull(exciseMovementEad, nameof(exciseMovementEad));
		}
		readonly ED819CBodyAlertOrRejectionOfAnEadExciseMovementEad exciseMovementEad;

		public ZString AdministrativeReferenceCode => exciseMovementEad.AdministrativeReferenceCode;

		public ZString SequenceNumber => exciseMovementEad.SequenceNumber;
	}

	class ED819ReasonProvider : IED819Reason
	{
		public ED819ReasonProvider(ED819CBodyAlertOrRejectionOfAnEadAlertOrRejectionOfEadReason alertOrRejectReason)
		{
			this.alertOrRejectReason = Argument.NotNull(alertOrRejectReason, nameof(alertOrRejectReason));
		}
		readonly ED819CBodyAlertOrRejectionOfAnEadAlertOrRejectionOfEadReason alertOrRejectReason;

		public ZString ReasonCode => alertOrRejectReason.AlertOrRejectionOfEadReasonCode;

		public ZString ComplementaryInformation => alertOrRejectReason.ComplementaryInformation;
	}
}
