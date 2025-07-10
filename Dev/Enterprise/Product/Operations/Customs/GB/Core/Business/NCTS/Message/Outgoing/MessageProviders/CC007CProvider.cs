using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class CC007CProvider : NctsHeaderSharedDataProvider, ICC007C
	{
		public DateTime ArrivalNotificationDateAndTime
		{
			get
			{
				var arrivalDate = nctsHeader.ArrivalMovementHeader.BM_ArrivalDate;
				var notificationDateTime = arrivalDate.IsEmpty || !arrivalDate.IsValid ? ZDateTime.Now.ToUniversalBranchTime() : arrivalDate.ToUniversalBranchTime();
				return DataProviderHelper.GetProviderDateTime(notificationDateTime);
			}
		}

		public bool SimplifiedProcedure => Authorisations.Count > 0;

		public bool IncidentFlag => nctsHeader.EnRouteIncidents.Count > 0;

		public string TraderIdentificationNumber => CachedValueHelper.GetValue(ref traderIdentificationNumber, () => new PartyProvider(nctsHeader.DestinationTrader, nctsHeader.IsInPhase5TransitionPeriod).IdentificationNumber ?? string.Empty);
		CachedValue<string> traderIdentificationNumber;

		public string TraderCommunicationLanguage => traderCommunicationLanguage ?? (traderCommunicationLanguage = nctsHeader.BH_CommunicationLanguage);
		string traderCommunicationLanguage;

		public IReadOnlyCollection<IAuthorisation> Authorisations => authorisations ?? (authorisations = nctsHeader.CusAuthorizationUsages
			.OrderBy(cau => cau.AGC_Code)
			.ThenBy(cau => cau.AGC_Number)
			.Select((cau, index) => new AuthorisationProvider(cau, index + 1)).ToArray());
		IReadOnlyCollection<IAuthorisation> authorisations;

		public IConsignmentType01 Consignment => consignment ?? (consignment = new ConsignmentType01Provider(nctsHeader));
		IConsignmentType01 consignment;

		public string Discharge => nctsHeader.ArrivalMovementHeader.BM_DischargeType;

		public string VoletPageNumber => nctsHeader.ArrivalMovementHeader.BM_CarnetTotalPages.ToString();

		public override string MessageRecipient => string.Format(CultureInfo.InvariantCulture, "NTA.{0}", nctsHeader.IsPhase5 ? nctsHeader.CommonMovementHeader.DestinationCustomsOfficeCodeCountryForArrival : nctsHeader.DestinationCustomsOfficeCodeCountryForArrival);

		public CC007CProvider(NctsHeader nctsHeader) : base(nctsHeader)
		{
		}

		public override string MessageType => Constants.MessageTypes.CC007C;
	}
}
