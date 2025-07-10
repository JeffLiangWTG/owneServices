using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.DE.EMCS.Messaging
{
	public interface IED803 : IEmcsDataProvider
	{
		IEMCSEvent ExciseMovementEad { get; }
		ZDateTime NotificationDateTime { get; }
		ZString NotificationType { get; }
		IReadOnlyCollection<ZString> DownstreamARCs { get; }
	}
}
