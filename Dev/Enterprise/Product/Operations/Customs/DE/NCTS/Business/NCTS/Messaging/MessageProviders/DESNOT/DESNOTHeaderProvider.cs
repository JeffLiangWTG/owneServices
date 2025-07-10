using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class DESNOTHeaderProvider : NCTSHeaderProvider, IDESNOTHeader
	{
		public DESNOTHeaderProvider(NctsHeader nctsHeader) : base(nctsHeader)
		{
			movementHeader = nctsHeader.ArrivalMovementHeader;
		}
		readonly NctsArrivalMovementHeader movementHeader;

		public string AuthorisationNumber => CachedValueHelper.GetValue(ref authorisationNumber, () =>
		{
			var result = GlbBranch.CurrentBranch.OrgProxy?.GetCustomsRegNo(GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber) ?? ZString.Empty;
			if (result.IsEmpty)
			{
				result = EORIHelper.GetSenderDetailsFromRegistry().Bin;
			}
			return result;
		});
		CachedValue<string> authorisationNumber;

		public string MRN => nctsHeader.ArrivalMrnFromUser;

		public DateTime ArrivalNotificationDateAndTime
		{
			get
			{
				var arrivalDateAndTime = movementHeader.BM_ArrivalDate.ToUniversalBranchTime(movementHeader.Factory).SafeDateTime();
				arrivalDateAndTime = arrivalDateAndTime.AddMilliseconds(-arrivalDateAndTime.Millisecond);
				return arrivalDateAndTime;
			}
		}

		public bool IncidentFlag => nctsHeader.BH_ExportFlag == EventFlagList.Codes.Yes;

		public IReadOnlyCollection<INCTSAuthorisation> Authorisations =>
			authorisations ?? (authorisations = nctsHeader.CusAuthorizationUsages
				.Select(NCTSAuthorisationProvider.NewOrNull)
				.ToArray());

		IReadOnlyCollection<INCTSAuthorisation> authorisations;

		public string CustomsOfficeOfDestinationActualReferenceNumber => movementHeader.DestinationCustomsOfficeCodeForArrival;

		public INCTSPartyIDContact TraderAtDestination => traderAtDestination ?? (traderAtDestination = NCTSPartyIDContactProvider.NewOrNull(nctsHeader.DestinationTrader, GlbStaff.CurrentUser));
		INCTSPartyIDContact traderAtDestination;

		public string LocationOfGoodsAdditionalIdentifier => movementHeader.GoodsLocation.CGL_AdditionalIdentifier;

		public INCTSContactPerson LocationOfGoodsContact => locationOfGoodsContact ?? (locationOfGoodsContact = NCTSContactPersonProvider.NewOrNull(movementHeader.GoodsLocation.Address));
		INCTSContactPerson locationOfGoodsContact;

		public IReadOnlyCollection<INCTSIncident> Incidents => incidents ?? (incidents = IncidentFlag ? nctsHeader.EnRouteIncidents.Select(NCTSIncidentProvider.NewOrNull).ToArray() : Array.Empty<INCTSIncident>());
		IReadOnlyCollection<INCTSIncident> incidents;
	}
}
