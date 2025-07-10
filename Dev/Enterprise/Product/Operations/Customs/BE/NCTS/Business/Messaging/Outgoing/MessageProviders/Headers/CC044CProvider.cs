using System;
using System.Linq;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class CC044CProvider : NctsHeaderSharedDataProvider, ICC044C
	{
		public CC044CProvider(NctsHeader nctsHeader) : base(nctsHeader)
		{
		}

		public string OtherThingsToReport => nctsHeader.ArrivalMovementHeader.OtherThingsToReport;

		public string TraderIdentificationNumber => CachedValueHelper.GetValue(ref traderIdentificationNumber, () => nctsHeader.DestinationTrader?.Organisation.GetIdentificationNumber());
		CachedValue<string> traderIdentificationNumber;

		public bool Conform => nctsHeader.ArrivalMovementHeader.BM_NoChangesToReport;

		public bool UnloadingCompletion => nctsHeader.ArrivalMovementHeader.BM_UnloadingCompleted;

		public string StateOfSeals => HasDeclaredSeals ? nctsHeader.ArrivalMovementHeader.BM_StateOfSealsBoolean ? "1" : "0" : null;
		bool HasDeclaredSeals => CachedValueHelper.GetValue(ref hasDeclaredSeals, () => nctsHeader.ArrivalHeaderContainers.Cast<NctsArrivalHeaderContainer>().Any(c => !c.TotalSealCount.IsEmpty && c.BC_UnloadedState != NctsUnloadedStateList.Codes.NEW) || nctsHeader.EnRouteIncidents.Any(i => i.IncidentContainers.Any(c => !c.TotalSealCount.IsEmpty)));
		CachedValue<bool> hasDeclaredSeals;

		public DateTime Unloadingdate => nctsHeader.ArrivalMovementHeader.BM_UnloadingDate.IsValid ? nctsHeader.ArrivalMovementHeader.BM_UnloadingDate.ToDateTime() : DateTime.MinValue;

		public string UnloadingRemark => nctsHeader.ArrivalMovementHeader.BM_UnloadingRemarks;

		public IConsignmentType06 Consignment => CachedValueHelper.GetValue(ref consignment, () => EmitConsignment ? new ConsignmentType06Provider(nctsHeader) : null);
		CachedValue<ConsignmentType06Provider> consignment;

		public override string MessageType => Constants.MessageTypes.CC044C;

		bool EmitConsignment => (!nctsHeader.ArrivalMovementHeader.BM_NoChangesToReport && nctsHeader.Bills.Any(x => x.MovementDetail.B9_UnloadedState != NctsUnloadedStateList.Codes.DEC))
							 || nctsHeader.ArrivalMovementHeader.ArrivalTransportInfos.Any(x => x.TPM_TransportState != NctsUnloadedStateList.Codes.DEC)
							 || nctsHeader.ArrivalHeaderContainers.Cast<NctsArrivalHeaderContainer>().Any(x => x.BC_UnloadedState != NctsUnloadedStateList.Codes.DEC)
							 || nctsHeader.ArrivalHeaderContainers.Cast<NctsArrivalHeaderContainer>().SelectMany(x => x.Seals).Cast<CusSeal>().Any(x => x.BK_UnloadingState != NctsUnloadedStateList.Codes.DEC && x.BK_UnloadingState != NctsUnloadedStateList.Codes.DAM);
	}
}
