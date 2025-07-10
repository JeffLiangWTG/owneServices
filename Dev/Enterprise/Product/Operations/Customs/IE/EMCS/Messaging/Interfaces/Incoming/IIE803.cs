using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.IE.EMCS.Messaging
{
	public interface IIE803 : IEMCSInboundProvider
	{
		IEMCSEvent ExciseMovementEad { get; }
		ZDateTime NotificationDateTime { get; }
		ZString NotificationType { get; }
		IReadOnlyCollection<ZString> DownstreamARCs { get; }
	}
}
