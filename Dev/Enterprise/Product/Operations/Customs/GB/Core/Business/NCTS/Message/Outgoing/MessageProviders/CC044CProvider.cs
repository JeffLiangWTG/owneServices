using System;
using System.Globalization;
using System.Linq;
using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class CC044CProvider : NctsHeaderSharedDataProvider, ICC044C
	{
		public CC044CProvider(NctsHeader nctsHeader) : base(nctsHeader)
		{
		}

		public override string MessageRecipient => string.Format(CultureInfo.InvariantCulture, "NTA.{0}", nctsHeader.IsPhase5 ? nctsHeader.CommonMovementHeader.DestinationCustomsOfficeCodeCountryForArrival : nctsHeader.DestinationCustomsOfficeCodeCountryForArrival);

		public string OtherThingsToReport => nctsHeader.ArrivalMovementHeader.OtherThingsToReport;

		public string TraderIdentificationNumber => CachedValueHelper.GetValue(ref traderIdentificationNumber, () => new PartyProvider(nctsHeader.DestinationTrader, nctsHeader.IsInPhase5TransitionPeriod).IdentificationNumber ?? string.Empty);
		CachedValue<string> traderIdentificationNumber;

		public bool Conform => nctsHeader.ArrivalMovementHeader.BM_NoChangesToReport;

		public bool UnloadingCompletion => nctsHeader.ArrivalMovementHeader.BM_UnloadingCompleted;

		public bool StateOfSealsOK => nctsHeader.ArrivalMovementHeader.BM_StateOfSealsBoolean;

		public bool HasDeclaredSeals => nctsHeader.ArrivalMovementHeader.Header.ArrivalHeaderContainers.Cast<NctsArrivalHeaderContainer>().Any(c => !c.TotalSealCount.IsEmpty && c.BC_UnloadedState != NctsUnloadedStateList.Codes.NEW) || nctsHeader.ArrivalMovementHeader.Header.EnRouteIncidents.Any(i => i.IncidentContainers.Any(c => !c.TotalSealCount.IsEmpty));

		public DateTime Unloadingdate => nctsHeader.ArrivalMovementHeader.BM_UnloadingDate.IsEmpty ? DateTime.MinValue : DataProviderHelper.GetProviderDateTime(nctsHeader.ArrivalMovementHeader.BM_UnloadingDate.ToZDateTime());

		public string UnloadingRemark => nctsHeader.ArrivalMovementHeader.BM_UnloadingRemarks;

		public IConsignmentType06 Consignment => consignment ?? (consignment = new ConsignmentType06Provider(nctsHeader));
		ConsignmentType06Provider consignment;

		public override string MessageType => Constants.MessageTypes.CC044C;
	}
}
