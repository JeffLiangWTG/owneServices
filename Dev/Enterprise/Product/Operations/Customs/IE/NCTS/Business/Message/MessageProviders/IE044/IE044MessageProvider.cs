using CargoWise.Customs.IE.MessageContracts.NCTS.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class IE044MessageProvider : NctsArrivalHeaderMessageProvider, IIE044Header
	{
		public IE044MessageProvider(NctsHeader nctsHeader) : base(nctsHeader)
		{
		}

		public IIE044TransitOperation TransitOperation => transitOperation ?? (transitOperation = new IE044TransitOperationProvider(NctsHeader));
		IIE044TransitOperation transitOperation;

		public string DestinationOffice => ArrivalMovementHeader.DestinationCustomsOfficeCodeForArrival;

		public string TraderAtDestination => NctsHeader.DestinationTrader?.Organisation?.GetEoriDetails();

		public IIE044UnloadingRemark UnloadingRemark => unloadingRemark ?? (unloadingRemark = new IE044UnloadingRemarkProvider(NctsHeader));
		IIE044UnloadingRemark unloadingRemark;

		public IIE044Consignment Consignment => CachedValueHelper.GetValue(
			NctsHeader.Factory,
			ref consignmentCache, () => NctsHeader.ArrivalMovementHeader.BM_NoChangesToReport ? null : new IE044ConsignmentProvider(NctsHeader)
		);
		CachedProperty<IIE044Consignment> consignmentCache;
	}
}
