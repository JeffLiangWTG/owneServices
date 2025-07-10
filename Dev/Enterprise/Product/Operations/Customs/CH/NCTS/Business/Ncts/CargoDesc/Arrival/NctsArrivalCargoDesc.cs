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
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NctsArrivalCargoDesc : EU.NCTS.Business.NctsArrivalCargoDesc, ICusCodeDataTypeSupporter
	, Integration.Customs.CH.IArrivalCargoDesc
{
	public NctsArrivalCargoDesc(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new NctsHeader Header => (NctsHeader)base.Header;

	protected override TariffFormatter GetNewTariffFormatter() => new TariffFormatterCH();

	public new NctsUnloadedCargoDesc UnloadedGoodsItem => (NctsUnloadedCargoDesc)base.UnloadedGoodsItem;

	public new NctsArrivalCargoDescValidation Validation => (NctsArrivalCargoDescValidation)base.Validation;

	protected override CusInBondCargoDescValidation GetNewValidation() => new NctsArrivalCargoDescValidation(this);

	protected override CusInBondCargoDescLookups GetNewLookups() => new NctsArrivalCargoDescLookups(this);

	public new NctsAdditionalInfoCollection<NctsAdditionalInfo> AdditionalInfos => (NctsAdditionalInfoCollection<NctsAdditionalInfo>)base.AdditionalInfos;
	protected override INctsAdditionalInfoCollection<EU.NCTS.Business.NctsAdditionalInfo> GetNctsAdditionalInfoCollection() => new NctsAdditionalInfoCollection<NctsAdditionalInfo>(this);
	protected override Type AdditionalInfoType => typeof(NctsAdditionalInfo);

	public new NctsPreviousDocumentCollection<NctsPreviousDocument> PreviousDocuments => (NctsPreviousDocumentCollection<NctsPreviousDocument>)base.PreviousDocuments;
	protected override INctsPreviousDocumentCollection<EU.NCTS.Business.NctsPreviousDocument> GetPreviousDocuments() => new NctsPreviousDocumentCollection<NctsPreviousDocument>(this);

	public new NctsPackageCollection<NctsPackage, NctsCommonCargoDesc> Packages => (NctsPackageCollection)base.Packages;

	protected override INctsPackageCollection<EU.NCTS.Business.NctsPackage, NctsCommonCargoDesc> GetNctsPackageCollection() => new NctsPackageCollection(this);

	public IDictionary<ZString, Type> GetCusCodeDataTypes()
	{
		return new Dictionary<ZString, Type>()
		{
			{ CH.Business.CusCodeDataTypeList.Codes.UnloadingRemarks, typeof(GoodsItemDifferencesDetails) },
		};
	}

	protected override Type PreviousDocumentType => typeof(NctsPreviousDocument);

	IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
	{
		yield return new CusCodeDataTypeSupporterFetchStrategy(this);
		yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
	}

	protected override bool ShouldSetSupportingDocumentsReadOnly => true;
	protected override bool ShouldSetAdditionalInfosReadOnly => true;

	protected override JobDocAddressRequirement GetJobDocAddressRequirement(DocAddressType docAddressType)
	{
		var requirement = new JobDocAddressRequirement(docAddressType);
		requirement.ValidateState = validation => { };
		return requirement;
	}

	protected override bool IsUnloadedCommodityCodeRequiredCore => false;

	[ChildEditable(true)]
	internal GoodsItemDifferencesDetailsCollection GoodsItemDifferencesDetails
	{
		get
		{
			if (goodsItemDifferencesDetails == null)
			{
				goodsItemDifferencesDetails = new GoodsItemDifferencesDetailsCollection(this);
				goodsItemDifferencesDetails.Load();
				RegisterEditableChildObject(goodsItemDifferencesDetails);
			}
			return goodsItemDifferencesDetails;
		}
	}
	GoodsItemDifferencesDetailsCollection goodsItemDifferencesDetails;

	internal GoodsItemDifferencesDetails GoodsItemDifferencesDetail => goodsItemDifferencesDetail != null && !goodsItemDifferencesDetail.IsDeleted ? goodsItemDifferencesDetail : (goodsItemDifferencesDetail = GoodsItemDifferencesDetails.FindOrCreate());
	GoodsItemDifferencesDetails goodsItemDifferencesDetail;

	public GoodsItemDifferencesDetailsLookups GoodsItemDifferencesDetailLookups => GoodsItemDifferencesDetail.Lookups;

	[LightValidationTestExempt]
	public override ZGuid BY_ParentID { get => base.BY_ParentID; set => base.BY_ParentID = value; }

	[LightValidationTestExempt]
	public override ZString BY_ParentTableCode { get => base.BY_ParentTableCode; set => base.BY_ParentTableCode = value; }

	public override ZString BY_UnloadedState
	{
		get => base.BY_UnloadedState;
		set
		{
			var oldValue = BY_UnloadedState;
			base.BY_UnloadedState = value;
			if (!IsCopying && oldValue != BY_UnloadedState)
			{
				GoodsItemDifferencesDetails.MarkAsNeedingValidation();
				if (BY_UnloadedState == EU.NCTS.Business.NctsUnloadedStateList.Codes.DEC)
				{
					ClearUnloadingRemarks();
				}
			}
		}
	}

	[ResourceStringData("Enterprise.Customs.CH.NCTS.Business.NctsArrivalAndUnloadingCargoDesc|UnloadingRemarkCode", Caption = "Unloading Code")]
	[List(nameof(GoodsItemDifferencesDetailLookups) + "." + nameof(GoodsItemDifferencesDetailsLookups.CY_CodeList))]
	[ReadOnlyMember(nameof(IsUnloadingReadOnly))]
	public ZString UnloadingRemarkCode
	{
		get => GoodsItemDifferencesDetail.CY_Code;
		set
		{
			GoodsItemDifferencesDetail.CY_Code = value;
			UnloadingRemarkCodeInfo.RefreshBinding();
		}
	}

	public ZPropertyInfo UnloadingRemarkCodeInfo => GetWrappedZPropertyInfo(nameof(UnloadingRemarkCode), _ => GoodsItemDifferencesDetail.CY_CodeInfo);

	[ResourceStringData("Enterprise.Customs.CH.NCTS.Business.NctsArrivalAndUnloadingCargoDesc|UnloadingRemarkText", Caption = "Unloading Remarks")]
	[ReadOnlyMember(nameof(IsUnloadingReadOnly))]
	public ZString UnloadingRemarkText
	{
		get => GoodsItemDifferencesDetail.CY_Data;
		set
		{
			GoodsItemDifferencesDetail.CY_Data = value;
			UnloadingRemarkTextInfo.RefreshBinding();
		}
	}

	public ZPropertyInfo UnloadingRemarkTextInfo => GetWrappedZPropertyInfo(nameof(UnloadingRemarkText), _ => GoodsItemDifferencesDetail.CY_DataInfo);

	void ClearUnloadingRemarks()
	{
		UnloadingRemarkCode = ZString.Empty;
		UnloadingRemarkText = ZString.Empty;
	}

	bool IsUnloadingReadOnly => Header.ArrivalMovementHeader.IsUnloadingRemarksReadOnly || BY_UnloadedState == NctsUnloadedStateList.Codes.DEC;

	protected override Type NctsUnloadedCargoDescType => typeof(NctsUnloadedCargoDesc);

	public bool IsDIFWithDifferences => Factory.GetCached(ref isDIFWithDifferences, () =>
	{
		return BY_UnloadedState == NctsUnloadedStateList.Codes.DIF
			&& (UnloadedGoodsItem is NctsUnloadedCargoDesc unloadedGoodsItem
				&& (InventoryDataProviderHelper.IsDifferent(BY_HarmonisedTariff, unloadedGoodsItem.BY_HarmonisedTariff)
				 || InventoryDataProviderHelper.IsDifferent(BY_CusC4Number, unloadedGoodsItem.BY_CusC4Number)
				 || InventoryDataProviderHelper.IsDifferent(BY_Description, unloadedGoodsItem.BY_Description)
				 || InventoryDataProviderHelper.IsDifferent(BY_GrossWeight, unloadedGoodsItem.BY_GrossWeight)
				 || InventoryDataProviderHelper.IsDifferent(BY_NetWeight, unloadedGoodsItem.BY_NetWeight)));
	});
	CachedProperty<bool> isDIFWithDifferences;

	public bool IsDIFWithDifferencesIncludingPackages => Factory.GetCached(ref isDIFWithDifferencesIncludingPackages, () => BY_UnloadedState == NctsUnloadedStateList.Codes.DIF && (IsDIFWithDifferences || IsAnyPackageDIFWithDifferences));
	CachedProperty<bool> isDIFWithDifferencesIncludingPackages;

	public bool IsAllPackageDEC => Factory.GetCached(ref isAllPackageDEC, () => Packages.Cast<NctsPackage>().All(x => x.B5_TypeOfDifference == NctsUnloadedStateList.Codes.DEC));
	CachedProperty<bool> isAllPackageDEC;

	public bool IsAllPackageMIS => Factory.GetCached(ref isAllPackageMIS, () => Packages.Cast<NctsPackage>().Any() && Packages.Cast<NctsPackage>().All(x => x.B5_TypeOfDifference == NctsUnloadedStateList.Codes.MIS));
	CachedProperty<bool> isAllPackageMIS;

	public bool IsAnyPackageDIF => Factory.GetCached(ref isAnyPackageDIF, () => Packages.Cast<NctsPackage>().Any(x => x.B5_TypeOfDifference == NctsUnloadedStateList.Codes.DIF));
	CachedProperty<bool> isAnyPackageDIF;

	public bool IsAnyPackageDIFWithDifferences => Factory.GetCached(ref isAnyPackageDIFWithDifferences, () => Packages.Cast<NctsPackage>().Any(x => x.IsDIFWithDifferences));
	CachedProperty<bool> isAnyPackageDIFWithDifferences;

	public bool IsUnloadedStateMIS => Factory.GetCached(ref isUnloadedStateMIS, () => BY_UnloadedState == NctsUnloadedStateList.Codes.MIS);
	CachedProperty<bool> isUnloadedStateMIS;
}
