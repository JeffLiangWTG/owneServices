using CargoWise.Common;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie802;
using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Messaging;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1
{
	public sealed class IE802Provider : IIE802
	{
		public IE802Provider(Ie802Type message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly Ie802Type message;

		public IEMCSEvent ExciseMovementEad => exciseMovementEad ?? (exciseMovementEad = new IE802EMCSEventProvider(message.Body.ReminderMessageForExciseMovement.ExciseMovement));
		IEMCSEvent exciseMovementEad;

		public ZDateTime LimitDateTime => message.Body.ReminderMessageForExciseMovement.Attributes.LimitDateAndTime;

		public ZString ReminderInformation => message.Body.ReminderMessageForExciseMovement.Attributes.ReminderInformation?.Value;

		public ZString ReminderMessageType => message.Body.ReminderMessageForExciseMovement.Attributes.ReminderMessageType.XmlEnumToString();

		public ZString MrnNumber => ExciseMovementEad.AdministrativeReferenceCode;

		public ZString MrnNumberSequenceNumber => ExciseMovementEad.SequenceNumber;
	}

	sealed class IE802EMCSEventProvider : IEMCSEvent
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
