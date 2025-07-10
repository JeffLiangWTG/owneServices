using CargoWise.Types;

namespace Enterprise.Customs.IE.EMCS.Messaging
{
	public interface IIE802 : IEMCSInboundProvider
	{
		IEMCSEvent ExciseMovementEad { get; }

		ZDateTime DateAndTimeOfIssuanceOfReminder { get; }
		ZDateTime LimitDateTime { get; }
		ZString ReminderInformation { get; }
		ZString ReminderMessageType { get; }
	}
}
