using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.Customs.IE.MessageContracts.NCTS.Interfaces;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.NCTS.Business
{
	class IE170MessageProvider : NctsDepartureHeaderMessageProvider, IIE170Header
	{
		public IE170MessageProvider(NctsHeader nctsHeader) : base(nctsHeader)
		{
		}

		public IIE170TransitOperation TransitOperation => CachedValueHelper.GetValue(ref transitOperationCached, () => new IE170TransitOperationProvider(NctsHeader.MovementHeader));
		CachedValue<IIE170TransitOperation> transitOperationCached;

		public string CustomsOfficeOfDeparture => MovementHeader.DepartureCustomsOfficeCode;

		public IHolder HolderOfTheTransitProcedure => CachedValueHelper.GetValue(ref holderOfTheTransitProcedureCached, () => HolderOfTransitProcedureProvider.New(NctsHeader.Principal, MovementHeader.BM_InBondEntryType));
		CachedValue<IHolder> holderOfTheTransitProcedureCached;

		public IRepresentative Representative => CachedValueHelper.GetValue(ref representativeCached, () => new IE170RepresentativeProvider(NctsHeader));
		CachedValue<IRepresentative> representativeCached;

		public IIE170Consignment Consignment => CachedValueHelper.GetValue(ref consignmentCached, () => new IE170ConsignmentProvider(NctsHeader));
		CachedValue<IIE170Consignment> consignmentCached;
	}
}
