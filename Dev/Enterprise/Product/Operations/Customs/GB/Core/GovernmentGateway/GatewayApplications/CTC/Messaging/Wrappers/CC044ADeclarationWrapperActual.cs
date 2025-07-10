using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Messaging;

namespace Enterprise.Customs.GB.GovernmentGateway.GatewayApplications.CTC.Messaging
{
	public class CC044ADeclarationWrapperActual : CC044ADeclarationWrapper, ICC044ADeclaration
	{
		public CC044ADeclarationWrapperActual(Business.NctsHeader nctsHeader)
			: base(nctsHeader)
		{
			amh = Argument.NotNull(nctsHeader.ArrivalMovementHeader, nameof(nctsHeader.ArrivalMovementHeader));
			umh = Argument.NotNull(nctsHeader.UnloadingMovementHeader, nameof(nctsHeader.UnloadingMovementHeader));
		}
		public IReadOnlyCollection<ISealID> Seals => seals ?? (seals = amh.Seals.Cast<Seal>().Select(x => new SealWrapper(x.CY_Data)).ToArray());
		IReadOnlyCollection<ISealID> seals;

		public IReadOnlyCollection<IUnloadedGoodsItem> UnloadedGoodsItems => unloadedGoodsItems ?? (unloadedGoodsItems = umh.GoodsItems.Cast<NctsArrivalAndUnloadingCargoDesc>().Select(line => new UnloadedGoodsItemWrapper(line)).ToArray());
		IReadOnlyCollection<IUnloadedGoodsItem> unloadedGoodsItems;

		public int NumberOfSeals => Seals.Count;

		readonly NctsArrivalMovementHeader amh;
		readonly NctsUnloadingMovementHeader umh;
	}
}
