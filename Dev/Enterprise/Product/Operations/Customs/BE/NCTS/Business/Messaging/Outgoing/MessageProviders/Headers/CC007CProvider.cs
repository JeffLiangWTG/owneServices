using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class CC007CProvider : NctsHeaderSharedDataProvider, ICC007C
	{
		public CC007CProvider(NctsHeader nctsHeader) : base(nctsHeader)
		{
		}

		public DateTime ArrivalNotificationDateAndTime
		{
			get
			{
				var arrivalDate = nctsHeader.ArrivalMovementHeader.BM_ArrivalDate;
				var notificationDateTime = arrivalDate.IsEmpty ? ZDateTime.UtcNow : arrivalDate;
				return DateTime.SpecifyKind(notificationDateTime.ToUniversalBranchTime().ToDateTime(), DateTimeKind.Unspecified);
			}
		}

		public bool SimplifiedProcedure => nctsHeader.CusAuthorizationUsages.Count > 0;

		public bool IncidentFlag => nctsHeader.EnRouteIncidents.Count > 0;

		public string TraderIdentificationNumber => CachedValueHelper.GetValue(ref traderIdentificationNumber, () => nctsHeader.DestinationTrader.Organisation.GetIdentificationNumber());
		CachedValue<string> traderIdentificationNumber;

		public string TraderCommunicationLanguage => CachedValueHelper.GetValue(ref traderCommunicationLanguage, () => nctsHeader.BH_CommunicationLanguage);
		CachedValue<string> traderCommunicationLanguage;

		public IReadOnlyCollection<IAuthorization> Authorisations => authorisations ?? (authorisations = nctsHeader.CusAuthorizationUsages
			.OrderBy(cau => cau.AGC_Code)
			.ThenBy(cau => cau.AGC_Number)
			.Select((cau, index) => new AuthorizationProvider(cau, index + 1)).ToArray());
		IReadOnlyCollection<IAuthorization> authorisations;

		public IConsignmentType01 Consignment => consignment ?? (consignment = new ConsignmentType01Provider(nctsHeader));
		IConsignmentType01 consignment;

		public string Discharge => nctsHeader.ArrivalMovementHeader.BM_DischargeType;

		public string VoletPageNumber => Discharge.IsNullOrEmpty() ? string.Empty : nctsHeader.ArrivalMovementHeader.BM_CarnetTotalPages.ToString();

		public override string MessageType => Constants.MessageTypes.CC007C;
	}
}
