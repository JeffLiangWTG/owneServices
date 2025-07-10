using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_4
{
	public class ED802Provider : IED802
	{
		public ED802Provider(ED802B message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly ED802B message;

		public ZString MessageGroup => message.Header.MessageGroup.XmlEnumToString();

		public string MessageIdentifier => message.Header.MessageIdentifier;

		public IEMCSEvent ExciseMovement => exciseMovementEad ?? (exciseMovementEad = new ED802EMCSEventProvider(message.Body.ReminderMessageForExciseMovement.ExciseMovementEad));
		IEMCSEvent exciseMovementEad;

		public ZDateTime LimitDateTime => message.Body.ReminderMessageForExciseMovement.Attributes.LimitDateAndTime;

		public ZString ReminderInformation => message.Body.ReminderMessageForExciseMovement.Attributes.ReminderInformation;

		public ZString ReminderMessageType => message.Body.ReminderMessageForExciseMovement.Attributes.ReminderMessageType.XmlEnumToString();
	}

	class ED802EMCSEventProvider : IEMCSEvent
	{
		public ED802EMCSEventProvider(ED802BBodyReminderMessageForExciseMovementExciseMovementEad exciseMovementEad)
		{
			this.exciseMovementEad = Argument.NotNull(exciseMovementEad, nameof(exciseMovementEad));
		}
		readonly ED802BBodyReminderMessageForExciseMovementExciseMovementEad exciseMovementEad;

		public ZString AdministrativeReferenceCode => exciseMovementEad.AdministrativeReferenceCode;

		public ZString SequenceNumber => exciseMovementEad.SequenceNumber;
	}
}
