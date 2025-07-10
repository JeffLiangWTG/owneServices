using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Messaging
{
	public interface IEnRouteEventSeal
	{
		ZString SealCount { get; }

		IReadOnlyCollection<ISealID> ContainerSeals { get; }
	}
}
