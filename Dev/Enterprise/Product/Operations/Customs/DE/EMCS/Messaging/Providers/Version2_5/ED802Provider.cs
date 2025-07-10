using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_5
{
	public class ED802Provider : IED802
	{
		public ED802Provider(ED802C message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly ED802C message;

		public ZString MessageGroup => message.Header.MessageGroup.XmlEnumToString();

		public string MessageIdentifier => message.Header.MessageIdentifier;

		public IEMCSEvent ExciseMovement => exciseMovement ?? (exciseMovement = new ED802EMCSEventProvider(message.Body.ReminderMessageForExciseMovement.ExciseMovement));
		IEMCSEvent exciseMovement;

		public ZDateTime LimitDateTime => message.Body.ReminderMessageForExciseMovement.Attributes.LimitDateAndTime;

		public ZString ReminderInformation => message.Body.ReminderMessageForExciseMovement.Attributes.ReminderInformation;

		public ZString ReminderMessageType => message.Body.ReminderMessageForExciseMovement.Attributes.ReminderMessageType.XmlEnumToString();
	}

	class ED802EMCSEventProvider : IEMCSEvent
	{
		public ED802EMCSEventProvider(ED802CBodyReminderMessageForExciseMovementExciseMovement exciseMovement)
		{
			this.exciseMovement = Argument.NotNull(exciseMovement, nameof(exciseMovement));
		}
		readonly ED802CBodyReminderMessageForExciseMovementExciseMovement exciseMovement;

		public ZString AdministrativeReferenceCode => exciseMovement.AdministrativeReferenceCode;

		public ZString SequenceNumber => exciseMovement.SequenceNumber;
	}
}
