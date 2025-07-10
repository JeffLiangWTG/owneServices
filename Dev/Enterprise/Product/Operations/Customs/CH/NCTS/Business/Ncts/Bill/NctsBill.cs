using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NctsBill : EU.NCTS.Business.NctsBill, ICusCodeDataTypeSupporter
{
	public NctsBill(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new NctsHeader Header => (NctsHeader)base.Header;

	protected override Type MovementDetailType => typeof(CusInBondMoveDetail);

	public new CusInBondMoveDetail MovementDetail => base.MovementDetail as CusInBondMoveDetail;

	public new INctsDepartureCargoDescCollection<NctsDepartureCargoDesc> GoodsItems => (INctsDepartureCargoDescCollection<NctsDepartureCargoDesc>)base.GoodsItems;

	protected override INctsDepartureCargoDescCollection<EU.NCTS.Business.NctsDepartureCargoDesc> GetNewGoodsItems() => new NctsDepartureCargoDescCollection<NctsDepartureCargoDesc>(this);

	public new INctsArrivalCargoDescCollection<NctsArrivalCargoDesc> ArrivalGoodsItems => (INctsArrivalCargoDescCollection<NctsArrivalCargoDesc>)base.ArrivalGoodsItems;

	protected override INctsArrivalCargoDescCollection<EU.NCTS.Business.NctsArrivalCargoDesc> GetNewArrivalGoodsItems() => new NctsArrivalCargoDescCollection<NctsArrivalCargoDesc>(this);

	public new ICusSupplyChainActorReferenceCollection<CusSupplyChainActorReference> CusSupplyChainActorReferences => (ICusSupplyChainActorReferenceCollection<CusSupplyChainActorReference>)base.CusSupplyChainActorReferences;
	protected override ICusSupplyChainActorReferenceCollection<EU.NCTS.Business.CusSupplyChainActorReference> GetCusSupplyChainActorReferencesCore() => new CusSupplyChainActorReferenceCollection<CusSupplyChainActorReference>(this);
	protected override Type SupplyChainActorType => typeof(CusSupplyChainActorReference);

	public new NctsBillValidation Validation => (NctsBillValidation)base.Validation;

	protected override CusInBondBillValidation GetNewValidation() => new NctsBillValidation(this);

	public new INctsSupportingDocumentCollection<NctsSupportingDocument> SupportingDocuments => (NctsSupportingDocumentCollection<NctsSupportingDocument>)base.SupportingDocuments;
	protected override INctsSupportingDocumentCollection<EU.NCTS.Business.NctsSupportingDocument> GetSupportingDocuments()
	{
		var supportingDocuments = new NctsSupportingDocumentCollection<NctsSupportingDocument>(this);
		supportingDocuments.SetReadOnlyIncludingChildren(IsPhase5Arrival);
		return supportingDocuments;
	}

	public new INctsBillAdditionalDocumentCollection<NctsBillAdditionalDocument> AdditionalDocuments => (NctsBillAdditionalDocumentCollection<NctsBillAdditionalDocument>)base.AdditionalDocuments;
	protected override INctsBillAdditionalDocumentCollection<EU.NCTS.Business.NctsBillAdditionalDocument> GetAdditionalDocuments()
	{
		var additionalDocuments = new NctsBillAdditionalDocumentCollection<NctsBillAdditionalDocument>(this);
		additionalDocuments.SetReadOnlyIncludingChildren(IsPhase5Arrival);
		return additionalDocuments;
	}

	protected override ICommonPreviousDocumentCollection<EU.NCTS.Business.CommonPreviousDocument> GetPreviousDocuments()
	{
		var previousDocuments = new CommonPreviousDocumentCollection<CommonPreviousDocument>(this);
		previousDocuments.SetReadOnlyIncludingChildren(IsPhase5Arrival);
		return previousDocuments;
	}

	[LightValidationTestExempt]
	public override ZGuid B0_BH { get => base.B0_BH; set => base.B0_BH = value; }

	public IDictionary<ZString, Type> GetCusCodeDataTypes()
	{
		return new Dictionary<ZString, Type>()
		{
			{ CH.Business.CusCodeDataTypeList.Codes.UnloadingRemarks, typeof(HouseConsignmentDifferences) },
		};
	}

	IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
	{
		yield return new CusCodeDataTypeSupporterFetchStrategy(this);
		yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
	}

	protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
	{
		var cusSupportingInfoTypes = base.GetCusSupportingInfoTypes();
		cusSupportingInfoTypes[CusSupportingInfoTypeList.Codes.AdditionalInfo] = typeof(NctsBillAdditionalDocument);
		cusSupportingInfoTypes[CusSupportingInfoTypeList.Codes.SupportingDocument] = typeof(NctsSupportingDocument);
		cusSupportingInfoTypes[CusSupportingInfoTypeList.Codes.PreviousDocument] = typeof(CommonPreviousDocument);
		return cusSupportingInfoTypes;
	}

	#region UnloadingRemark
	[ChildEditable(true)]
	internal HouseConsignmentDifferencesCollection HouseConsignmentDifferences
	{
		get
		{
			if (houseConsignmentDifferences == null)
			{
				houseConsignmentDifferences = new HouseConsignmentDifferencesCollection(this);
				houseConsignmentDifferences.Load();
				RegisterEditableChildObject(houseConsignmentDifferences);
			}
			return houseConsignmentDifferences;
		}
	}
	HouseConsignmentDifferencesCollection houseConsignmentDifferences;

	internal HouseConsignmentDifferences HouseConsignmentDifference => houseConsignmentDifference != null && !houseConsignmentDifference.IsDeleted ? houseConsignmentDifference : (houseConsignmentDifference = HouseConsignmentDifferences.FindOrCreate());
	HouseConsignmentDifferences houseConsignmentDifference;

	public HouseConsignmentDifferencesLookups HouseConsignmentDifferenceLookups => HouseConsignmentDifference.Lookups;

	[ResourceStringData("Enterprise.Customs.CH.NCTS.Business.NctsBill|UnloadingRemarkCode", Caption = "Unloading Code")]
	[List(nameof(HouseConsignmentDifferenceLookups) + "." + nameof(HouseConsignmentDifferencesLookups.CY_CodeList))]
	public ZString UnloadingRemarkCode
	{
		get => HouseConsignmentDifference.CY_Code;
		set
		{
			HouseConsignmentDifference.CY_Code = value;
			UnloadingRemarkCodeInfo.RefreshBinding();
		}
	}

	public ZPropertyInfo UnloadingRemarkCodeInfo => GetWrappedZPropertyInfo(nameof(UnloadingRemarkCode), _ => HouseConsignmentDifference.CY_CodeInfo);

	[ResourceStringData("Enterprise.Customs.CH.NCTS.Business.NctsBill|UnloadingRemarkText", Caption = "Unloading Remarks")]
	public ZString UnloadingRemarkText
	{
		get => HouseConsignmentDifference.CY_Data;
		set
		{
			HouseConsignmentDifference.CY_Data = value;
			UnloadingRemarkTextInfo.RefreshBinding();
		}
	}

	public ZPropertyInfo UnloadingRemarkTextInfo => GetWrappedZPropertyInfo(nameof(UnloadingRemarkText), _ => HouseConsignmentDifference.CY_DataInfo);

	internal void ClearUnloadingRemarks()
	{
		UnloadingRemarkCode = ZString.Empty;
		UnloadingRemarkText = ZString.Empty;
	}
	#endregion

	protected override bool B0_TransportPaymentMethodReadOnly => Header.IsDepartureMovement && !Header.MovementHeader.BM_MethodOfPayment.IsEmpty;

	protected override void ValidateConsignee(JobDocAddressValidation validation)
	{
		base.ValidateConsignee(validation);
		PassarValidation.CheckNS30132(validation.Parent.OrganisationPKInfo, this);

		if (ValidationDecider is NctsBillDeparturePhase5ValidationDecider validationDecider && validationDecider.IsRuleNS30018Active)
		{
			PassarValidation.CheckNS30018(validation.Parent.OrganisationPKInfo, this);
		}
	}

	protected override bool ShouldSetArrivalTransportInfosReadOnlyCore => true;

	public bool HasRequiredPreviousDocument => Factory.GetCached(ref hasRequiredPreviousDocument, () => PreviousDocuments.Any(x => IsRequiredPreviousDocumentType(x.CSI_Code)));
	public CachedProperty<bool> hasRequiredPreviousDocument;

	static bool IsRequiredPreviousDocumentType(ZString documentType) => documentType.ToString() switch
	{
		"SNOT" or
		"SWEB" or
		"SZVE" or
		"STRE" or
		"SAUZ" or
		"STAB" or
		"SZVA" or
		"NTRV" or
		"SZWA" => true,
		_ => false
	};
}
