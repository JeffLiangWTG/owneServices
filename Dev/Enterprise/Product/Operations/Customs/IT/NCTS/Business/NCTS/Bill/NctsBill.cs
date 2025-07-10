using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsBill : EU.NCTS.Business.NctsBill
{
	public NctsBill(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	protected override CusInBondBillValidation GetNewValidation() => new NctsBillValidation(this);

	public new NctsBillValidation Validation => (NctsBillValidation)base.Validation;

	public new NctsHeader Header => (NctsHeader)base.Header;

	public new NctsBillLookups Lookups => (NctsBillLookups)base.Lookups;

	protected override CusInBondBillLookups GetNewLookups() => new NctsBillLookups(this);

	public new INctsSupportingDocumentCollection<NctsSupportingDocument> SupportingDocuments => (INctsSupportingDocumentCollection<NctsSupportingDocument>)base.SupportingDocuments;

	protected override INctsSupportingDocumentCollection<EU.NCTS.Business.NctsSupportingDocument> GetSupportingDocuments() => new NctsSupportingDocumentCollection<NctsSupportingDocument>(this);

	public new INctsDepartureCargoDescCollection<NctsDepartureCargoDesc> GoodsItems => (INctsDepartureCargoDescCollection<NctsDepartureCargoDesc>)base.GoodsItems;

	public int TransportTypeAtDepartureParsed => int.TryParse(TransportTypeAtDeparture, out var result) ? result : default;

	protected override INctsDepartureCargoDescCollection<EU.NCTS.Business.NctsDepartureCargoDesc> GetNewGoodsItems() => new NctsDepartureCargoDescCollection(this);

	protected override ICommonPreviousDocumentCollection<CommonPreviousDocument> GetPreviousDocuments() => new CommonPreviousDocumentCollection<CommonPreviousDocument>(this);

	protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
	{
		var cusSupportingInfoTypes = base.GetCusSupportingInfoTypes();
		cusSupportingInfoTypes[CusSupportingInfoTypeList.Codes.PreviousDocument] = typeof(CommonPreviousDocument);
		cusSupportingInfoTypes[CusSupportingInfoTypeList.Codes.SupportingDocument] = typeof(NctsSupportingDocument);
		return cusSupportingInfoTypes;
	}

	public override ZGuid B0_BH
	{
		get => base.B0_BH;
		set
		{
			var oldValue = B0_BH;
			base.B0_BH = value;
			if (!IsCopying && oldValue != B0_BH)
			{
				GoodsItems.ForEach(x => x.Fees.MarkAsNeedingValidation());
			}
		}
	}

	[ResourceStringData("B839F07C-D7DB-41E3-9CE4-2BA1D2107E71", Caption = "Status", FullDescription = "House Consignment Status")]
	[List(nameof(Lookups) + "." + nameof(Lookups.StatusList))]
	[ReadOnly(true)]
	public override ZString B0_BillStatus { get => base.B0_BillStatus; set => base.B0_BillStatus = value; }

	protected override bool IsTransportDepartureReadOnlyCore => base.IsTransportDepartureReadOnlyCore || ParentHasDepartureTransportMeans();

	bool ParentHasDepartureTransportMeans()
	{
		return Header.MovementHeader is NctsDepartureMovementHeader departureMovement
			&& departureMovement.GetDepartureTransportMeansProperties().Any(x => !x.Value.IsEmpty);
	}

	internal void WipeDepartureTransportMeansIfNeeded()
	{
		if (IsTransportDepartureReadOnly)
		{
			VesselNameAtDeparture = ZString.Empty;
			VesselCountryAtDeparture = ZString.Empty;
			TransportTypeAtDeparture = ZString.Empty;
			TransportAtDeparture = ZString.Empty;
			TransportCountryAtDeparture = ZString.Empty;
			Trailer1IDAtDeparture = ZString.Empty;
			Trailer1NationalityAtDeparture = ZString.Empty;
			Trailer2IDAtDeparture = ZString.Empty;
			Trailer2NationalityAtDeparture = ZString.Empty;
			AircraftIDAtDeparture = ZString.Empty;
			TransportDepartureAdditionalWagonNumbers.RemoveAndDeleteAll();
		}
	}

	internal void RefreshDepartureTransportMeansBindings()
	{
		VesselNameAtDepartureInfo.RefreshBinding();
		VesselCountryAtDepartureInfo.RefreshBinding();
		TransportTypeAtDepartureInfo.RefreshBinding();
		TransportAtDepartureInfo.RefreshBinding();
		TransportCountryAtDepartureInfo.RefreshBinding();
		Trailer1IDAtDepartureInfo.RefreshBinding();
		Trailer1NationalityAtDepartureInfo.RefreshBinding();
		Trailer2IDAtDepartureInfo.RefreshBinding();
		Trailer2NationalityAtDepartureInfo.RefreshBinding();
		AircraftIDAtDepartureInfo.RefreshBinding();
	}

	public bool IsPhaseInAmendment => Header.MovementHeader.BM_Phase == NctsMovementHeaderTransactionStatusList.Codes.Amendment;

	public void SetAsCustomsDeletionRequest()
	{
		B0_BillStatus = NctsDeletionStatusList.Codes.DeletionRequest;
	}

	public void ClearCustomsDeletionStatus()
	{
		B0_BillStatus = ZString.Empty;
	}
	public bool IsCustomsStatusDeleted => B0_BillStatus == NctsDeletionStatusList.Codes.Deleted;

	public bool IsCustomsStatusDeletionRequested => B0_BillStatus == NctsDeletionStatusList.Codes.DeletionRequest;

	protected override void RunPreSaveValidationCore()
	{
		base.RunPreSaveValidationCore();
		Consignee.Validation.ValidateOrganisationPK();
	}

	#region ICanDelete

	public override bool CanDelete
	{
		get { return base.CanDelete && !IsPhaseInAmendment; }
	}

	public override MultilingualString ReasonForNotAbleToDelete => IsPhaseInAmendment
		? ResString.GetMultilingualString("FE8B4E7F-EDCA-41BD-A1B0-06CFC8A52940", "It is not possible to delete a House Consignment in Amendment phase. You can request a deletion of House Consignment setting its Status to DLR (deletion request).")
		: base.ReasonForNotAbleToDelete;

	#endregion
}
