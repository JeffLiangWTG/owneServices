#if NETFRAMEWORK
using CargoWise.Common;
#elif NET
using Argument = CargoWise.Common.Argument;
#endif
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Messaging;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class EnRouteEventSealWrapper : IEnRouteEventSeal
	{
		public EnRouteEventSealWrapper(EnRouteSeal enRouteSeal)
		{
			this.enRouteSeal = Argument.NotNull(enRouteSeal, nameof(enRouteSeal));
		}
		readonly EnRouteSeal enRouteSeal;

		public ZString SealCount => ContainerSeals.Count.ToString(System.Globalization.CultureInfo.InvariantCulture);

		public IReadOnlyCollection<ISealID> ContainerSeals => containerSeals ?? (containerSeals = enRouteSeal.SealContainers.Cast<SealContainer>()
			.DistinctBy(x => x.BC_Seal1)
			.Select(x => new SealWrapper(x.BC_Seal1)).ToArray());
		IReadOnlyCollection<ISealID> containerSeals;
	}
}
