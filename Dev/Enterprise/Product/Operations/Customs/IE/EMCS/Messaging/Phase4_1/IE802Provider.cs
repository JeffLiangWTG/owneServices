using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE802;
using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Messaging;

namespace Enterprise.Customs.IE.EMCS.Messaging.Phase4_1
{
	public class IE802Provider : IIE802
	{
		public IE802Provider(Ie802Type message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly Ie802Type message;

		public IEMCSEvent ExciseMovementEad => exciseMovementEad ?? (exciseMovementEad = new IE802EMCSEventProvider(message.Body.ReminderMessageForExciseMovement.ExciseMovement));
		IEMCSEvent exciseMovementEad;

		public ZDateTime DateAndTimeOfIssuanceOfReminder => message.Body.ReminderMessageForExciseMovement.Attributes
			.DateAndTimeOfIssuanceOfReminder;

		public ZDateTime LimitDateTime => message.Body.ReminderMessageForExciseMovement.Attributes.LimitDateAndTime;

		public ZString ReminderInformation => message.Body.ReminderMessageForExciseMovement.Attributes.ReminderInformation?.Value;

		public ZString ReminderMessageType => message.Body.ReminderMessageForExciseMovement.Attributes.ReminderMessageType.XmlEnumToString();

		public ZString MrnNumber => ExciseMovementEad.AdministrativeReferenceCode;

		public ZString MrnNumberSequenceNumber => ExciseMovementEad.SequenceNumber;
	}

	class IE802EMCSEventProvider : IEMCSEvent
	{
		public IE802EMCSEventProvider(ExciseMovementType exciseMovementEad)
		{
			this.exciseMovementEad = Argument.NotNull(exciseMovementEad, nameof(exciseMovementEad));
		}
		readonly ExciseMovementType exciseMovementEad;

		public ZString AdministrativeReferenceCode => exciseMovementEad.AdministrativeReferenceCode;

		public ZString SequenceNumber => exciseMovementEad.SequenceNumber;
	}
}
