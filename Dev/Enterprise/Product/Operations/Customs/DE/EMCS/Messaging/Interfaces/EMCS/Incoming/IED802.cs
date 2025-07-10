using CargoWise.Types;

namespace Enterprise.Customs.DE.EMCS.Messaging
{
	public interface IED802 : IEmcsDataProvider
	{
		IEMCSEvent ExciseMovement { get; }
		ZDateTime LimitDateTime { get; }
		ZString ReminderInformation { get; }
		ZString ReminderMessageType { get; }
	}
}
