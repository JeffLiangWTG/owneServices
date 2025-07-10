using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_5
{
	public class ED819Provider : IED819
	{
		public ED819Provider(ED819D message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly ED819D message;

		public ZString MessageGroup => message.Header.MessageGroup.XmlEnumToString();

		public string MessageIdentifier => message.Header.MessageIdentifier;

		public IEMCSEvent ExciseMovement => exciseMovement ?? (exciseMovement = new ED819EventProvider(message.Body.AlertOrRejectionOfAnEad.ExciseMovement));
		IEMCSEvent exciseMovement;

		public IReadOnlyCollection<IED819Reason> AlertOrRejectionReasons => alertOrRejectionReasons ?? (alertOrRejectionReasons = message.Body.AlertOrRejectionOfAnEad.AlertOrRejectionOfEadEsadReason?.Select(x => new ED819ReasonProvider(x)).ToArray() ?? Array.Empty<IED819Reason>());
		IReadOnlyCollection<IED819Reason> alertOrRejectionReasons;
	}

	class ED819EventProvider : IEMCSEvent
	{
		public ED819EventProvider(ED819DBodyAlertOrRejectionOfAnEadExciseMovement exciseMovement)
		{
			this.exciseMovement = Argument.NotNull(exciseMovement, nameof(exciseMovement));
		}
		readonly ED819DBodyAlertOrRejectionOfAnEadExciseMovement exciseMovement;

		public ZString AdministrativeReferenceCode => exciseMovement.AdministrativeReferenceCode;

		public ZString SequenceNumber => exciseMovement.SequenceNumber;
	}

	class ED819ReasonProvider : IED819Reason
	{
		public ED819ReasonProvider(ED819DBodyAlertOrRejectionOfAnEadAlertOrRejectionOfEadEsadReason alertOrRejectReason)
		{
			this.alertOrRejectReason = Argument.NotNull(alertOrRejectReason, nameof(alertOrRejectReason));
		}
		readonly ED819DBodyAlertOrRejectionOfAnEadAlertOrRejectionOfEadEsadReason alertOrRejectReason;

		public ZString ReasonCode => alertOrRejectReason.AlertOrRejectionOfMovementReasonCode;

		public ZString ComplementaryInformation => alertOrRejectReason.ComplementaryInformation;
	}
}
