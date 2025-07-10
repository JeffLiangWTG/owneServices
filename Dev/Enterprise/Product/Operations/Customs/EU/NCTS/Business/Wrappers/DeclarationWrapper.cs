using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Messaging;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public abstract class DeclarationWrapper : IDeclaration
	{
		public DeclarationWrapper(NctsHeader nctsHeader)
		{
			this.nctsHeader = Argument.NotNull(nctsHeader, nameof(nctsHeader));
			this.commonMovementHeader = Argument.NotNull(nctsHeader.CommonMovementHeader, nameof(nctsHeader.CommonMovementHeader));
			this.isPhase5 = nctsHeader.IsPhase5;
		}
		protected readonly bool isPhase5;
		protected readonly NctsHeader nctsHeader;
		protected readonly NctsCommonMovementHeader commonMovementHeader;

		public ZBool IsProduction => Environment.Env.Instance.IsProductionSystem;

		public ZString LocalReferenceNumber => nctsHeader.LocalReferenceNumber;

		public ZString MovementReferenceNumber => nctsHeader.MovementReferenceNumber;

		public ITrader Principal => CachedValueHelper.GetValue(ref principal, () => TraderWrapper.New(nctsHeader.Principal, true, IsAddressExtended));
		CachedValue<ITrader> principal;

		public ITrader Consignee => CachedValueHelper.GetValue(ref consignee, () => TraderWrapper.New(nctsHeader.Consignee, false, IsAddressExtended));
		CachedValue<ITrader> consignee;

		public ZString DepartureCustomsOfficeReferenceNumber => isPhase5 ? commonMovementHeader.DepartureCustomsOfficeCode : nctsHeader.DepartureCustomsOfficeCode;

		public ZString DestinationCustomsOfficeReferenceNumber => isPhase5 ? commonMovementHeader.DestinationCustomsOfficeCode : nctsHeader.DestinationCustomsOfficeCode;

		protected IEnumerable<NctsDepartureCargoDesc> EffectiveDepartureGoodsItems
			=> effectiveDepartureGoodsItems ??= isPhase5 ? nctsHeader.Bills.SelectMany(x => x.GoodsItems).ToArray() : nctsHeader.MovementHeader.GoodsItems.ToArray();
		NctsDepartureCargoDesc[] effectiveDepartureGoodsItems;

		protected ZBool IsAddressExtended => nctsHeader.IsPhase5 && !nctsHeader.IsInPhase5TransitionPeriod;
	}
}
