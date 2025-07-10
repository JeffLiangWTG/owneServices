using CargoWise.Customs.IE.MessageContracts.NCTS.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Business
{
	class IE054MessageProvider : NctsDepartureHeaderMessageProvider, IIE054Header
	{
		public IE054MessageProvider(NctsHeader nctsHeader, ZString releaseRequest) : base(nctsHeader)
		{
			this.releaseRequest = releaseRequest;
		}

		public string CustomsOfficeOfDeparture => MovementHeader.DepartureCustomsOfficeCode;

		public IHolder HolderOfTheTransitProcedure => CachedValueHelper.GetValue(ref holderOfTheTransitProcedureCached, () => HolderOfTransitProcedureProvider.New(NctsHeader.Principal, MovementHeader.BM_InBondEntryType));
		CachedValue<IHolder> holderOfTheTransitProcedureCached;

		public IIE054TransitOperation TransitOperation => CachedValueHelper.GetValue(ref transitOperationCached, () => new IE054TransitOperationProvider(NctsHeader, releaseRequest));
		CachedValue<IIE054TransitOperation> transitOperationCached;

		readonly ZString releaseRequest;
	}
}
