using CargoWise.Customs.IE.MessageContracts.NCTS.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Business
{
	class IE014MessageProvider : NctsDepartureHeaderMessageProvider, IIE014Header
	{
		public IE014MessageProvider(NctsHeader nctsHeader, ZString justification) : base(nctsHeader)
		{
			this.justification = justification;
		}

		public IIE014TransitOperation TransitOperation => CachedValueHelper.GetValue(ref transitOperationCached, () => new IE014TransitOperationProvider(NctsHeader));
		CachedValue<IIE014TransitOperation> transitOperationCached;

		public IIE014InvalidationType InvalidationType => CachedValueHelper.GetValue(ref invalidationTypeProviderCached, () => new IE014InvalidationTypeProvider(NctsHeader, justification));
		CachedValue<IIE014InvalidationType> invalidationTypeProviderCached;

		public string DepartureOffice => MovementHeader.DepartureCustomsOfficeCode;

		public IHolder HolderOfTheTransit => CachedValueHelper.GetValue(ref holderOfTheTransit, () => HolderOfTransitProcedureProvider.New(NctsHeader.Principal, MovementHeader.BM_InBondEntryType));
		CachedValue<IHolder> holderOfTheTransit;

		readonly ZString justification;
	}
}
