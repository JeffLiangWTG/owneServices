using System;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using NCTS5ArrivalCustomsStatusList = Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists.NCTS5ArrivalCustomsStatusList;
using NCTS5DepartureCustomsStatusList = Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists.NCTS5DepartureCustomsStatusList;

namespace Enterprise.Customs.BE.NCTS.Business;

public class NctsHeader : EU.NCTS.Business.NctsHeader
	, Integration.Customs.BE.ICusInBondHeader
{
	public NctsHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();

		var userLanguage = GlbStaff.CurrentUser.GS_WorkingLanguage.Left(2);
		if (Lookups.CommunicationLanguageList.ContainsCode(userLanguage))
		{
			BH_CommunicationLanguage = userLanguage;
		}
	}

	public new EU.NCTS.Business.EnRouteIncidentCollection EnRouteIncidents => base.EnRouteIncidents;

	protected override EU.NCTS.Business.EnRouteIncidentCollection GetEnRouteIncidents() => new EU.NCTS.Business.EnRouteIncidentCollection(this);

	public new EU.NCTS.Business.INctsBillCollection<NctsBill> Bills => (EU.NCTS.Business.INctsBillCollection<NctsBill>)base.Bills;
	protected override EU.NCTS.Business.INctsBillCollection<EU.NCTS.Business.NctsBill> GetNewBillCollection() => new EU.NCTS.Business.NctsBillCollection<NctsBill>(this);
	protected override Type BillTypeCore => typeof(NctsBill);

	public new NctsDepartureMovementHeader MovementHeader => (NctsDepartureMovementHeader)base.MovementHeader;

	public new NctsArrivalMovementHeader ArrivalMovementHeader => (NctsArrivalMovementHeader)base.ArrivalMovementHeader;

	public new EU.NCTS.Business.INctsGuaranteeCollection<NctsGuarantee> Guarantees => (EU.NCTS.Business.INctsGuaranteeCollection<NctsGuarantee>)base.Guarantees;

	public new EU.NCTS.Business.NctsEuOfficeCode EnquiryCustomsOffice => EU.NCTS.Business.NctsEuOfficeCode.LoadOrCreate<EU.NCTS.Business.NctsEuOfficeCode>(this, OfficeCodes_NCTS.Codes.NCTSOfficeOfEnquiry);

	protected override EU.NCTS.Business.INctsGuaranteeCollection<EU.NCTS.Business.NctsGuarantee> GetGuaranteesForNonPhase5Departure() => new EU.NCTS.Business.NctsGuaranteeCollection<NctsGuarantee>(this);

	protected override CusInBondHeaderLookups GetNewLookups() => new NctsHeaderLookups(this);

	public CusEntryNumber CorrelationIdentifierEntryNumber
	{
		get
		{
			if (correlationIdentifierEntryNumber == null || correlationIdentifierEntryNumber.IsDeleted)
			{
				EU.NCTS.Business.NctsCommonMovementHeader movementHeader = IsArrivalMovement ? ArrivalMovementHeader : MovementHeader;
				correlationIdentifierEntryNumber = CusEntryNumber.LoadOrCreate(movementHeader, CusEntryNumberTypes.EU.CorrelationIdentifier, CountryCode);
				RegisterEditableChildObject(correlationIdentifierEntryNumber);
			}

			return correlationIdentifierEntryNumber;
		}
	}
	CusEntryNumber correlationIdentifierEntryNumber;

	[ChildEditable]
	public new EU.NCTS.Business.ICommonPreviousDocumentCollection<CommonPreviousDocument> PreviousDocuments => (EU.NCTS.Business.ICommonPreviousDocumentCollection<CommonPreviousDocument>)base.PreviousDocuments;

	public new NctsHeaderPhase5Validation Validation => (NctsHeaderPhase5Validation)base.Validation;

	protected override CusInBondHeaderValidation GetNewPhase5Validation() => new NctsHeaderPhase5Validation(this);

	protected override EU.NCTS.Business.ICommonPreviousDocumentCollection<EU.NCTS.Business.CommonPreviousDocument> GetPreviousDocuments() => new EU.NCTS.Business.CommonPreviousDocumentCollection<CommonPreviousDocument>(this);

	protected override Type AdditionalInfoType => typeof(NctsAdditionalInfo);
	protected override Type PreviousDocumentType => typeof(CommonPreviousDocument);

	public bool HasLogWith(ZString eventCode, ZString reference) => Logs.HasLogWith(log => log.SL_SE_NKEvent.EqualsIgnoringCase(eventCode) && log.SL_Reference.EqualsIgnoringCase(reference));

	protected override void CloneAdditionalDocuments(EU.NCTS.Business.NctsHeader newHeader)
	{
		var filteredAdditionalInfos = AdditionalDocuments
			.Where(info => !(info.CSI_Code.In(new ZString[] { Constants.AdditionalDocumentTypes.CBRNumber, Constants.AdditionalDocumentTypes._4009 })
			&& info.IsAnAdditionalReference));

		foreach (var additionalInfo in filteredAdditionalInfos)
		{
			var newInfo = (NctsAdditionalInfo)new EU.NCTS.Business.NctsDeepCloneStrategy(additionalInfo, newHeader.PK).Clone();
			newHeader.AdditionalDocuments.Add(newInfo);
		}
	}

	#region ICancellable

	public override string CanCancel()
	{
		var cannotCancelDueToCustomsStatus = false;
		var customsStatus = string.Empty;

		if (IsPhase5Arrival)
		{
			customsStatus = ArrivalMovementHeader.BM_CustomsStatus;
			switch (customsStatus)
			{
				case NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease:
				case NCTS5ArrivalCustomsStatusList.Codes.ClosedPartialRelease:
				case NCTS5ArrivalCustomsStatusList.Codes.DiscrepancyResolutionPartialRelease:
				case NCTS5ArrivalCustomsStatusList.Codes.DiscrepancyResolutionNoRelease:
				case NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks:
				case NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted:
					cannotCancelDueToCustomsStatus = true;
					break;
			}
		}
		else if (IsPhase5Departure)
		{
			customsStatus = MovementHeader.BM_CustomsStatus;
			switch (customsStatus)
			{
				case NCTS5DepartureCustomsStatusList.Codes.Acknowledged:
				case NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested:
				case NCTS5DepartureCustomsStatusList.Codes.Cancelled:
				case NCTS5DepartureCustomsStatusList.Codes.CancellationRequestedByCustoms:
				case NCTS5DepartureCustomsStatusList.Codes.DecisionToControl:
				case NCTS5DepartureCustomsStatusList.Codes.AdditionalDocumentsRequest:
				case NCTS5DepartureCustomsStatusList.Codes.IntentionToControl:
				case NCTS5DepartureCustomsStatusList.Codes.DiscrepanciesAtDestination:
				case NCTS5DepartureCustomsStatusList.Codes.UnderEnquiry:
				case NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid:
				case NCTS5DepartureCustomsStatusList.Codes.IncidentRegistered:
				case NCTS5DepartureCustomsStatusList.Codes.MrnAllocated:
				case NCTS5DepartureCustomsStatusList.Codes.NotReleasedForTransit:
				case NCTS5DepartureCustomsStatusList.Codes.PreLodged:
				case NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit:
				case NCTS5DepartureCustomsStatusList.Codes.RequestForAdvice:
				case NCTS5DepartureCustomsStatusList.Codes.ReleaseRequestHasBeenRequested:
				case NCTS5DepartureCustomsStatusList.Codes.UnderRecoveryProcedure:
				case NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed:
					cannotCancelDueToCustomsStatus = true;
					break;
			}
		}

		return cannotCancelDueToCustomsStatus ? Res.GetString("55D89749-4EC6-4CF4-B0E7-F2A722692AFA", "Cannot deactivate when Customs Status is {0}.", customsStatus) : base.CanCancel();
	}

	#endregion

	protected override EU.NCTS.Business.NctsHeaderDocumentSupporter GetNewDocumentSupporter() => new NctsHeaderDocumentSupporter(this);
}
