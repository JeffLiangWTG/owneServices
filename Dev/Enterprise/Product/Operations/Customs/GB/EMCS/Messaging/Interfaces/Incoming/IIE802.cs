using CargoWise.Types;

namespace Enterprise.Customs.GB.EMCS.Messaging
{
	public interface IIE802 : IEMCSInboundProvider
	{
		IEMCSEvent ExciseMovementEad { get; }
		ZDateTime LimitDateTime { get; }
		ZString ReminderInformation { get; }
		ZString ReminderMessageType { get; }
	}
}
