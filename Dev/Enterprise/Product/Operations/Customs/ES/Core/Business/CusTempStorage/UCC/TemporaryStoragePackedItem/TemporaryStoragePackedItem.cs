using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common;
using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.ES.Business.CusTempStorage;

[SystemDefinedValues]
public class TemporaryStoragePackedItem : EU.Business.CusTempStorage.TemporaryStoragePackedItem
{
	public TemporaryStoragePackedItem(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new class Schema : EU.Business.CusTempStorage.TemporaryStoragePackedItem.Schema
	{
		public const string IsMissing = "IsMissing";
	}

	protected override bool CalculateAndRefreshAllDutyAmountFromTariffRatesCore => !Bill?.Header?.IsMessageTypeTSMAndIsUnionGoods ?? true;

	[ResourceStringData("62BCA841-C8EB-4076-AFB2-FDBA444E9104", Caption = "Missing", MediumCaption = "Missing", ShortCaption = "Missing", FullDescription = "Is Missing")]
	public ZBool IsMissing
	{
		get => API_PackStatus == "MIS";
		set
		{
			var oldValue = IsMissing;
			if (oldValue != value)
			{
				API_PackStatus = value ? "MIS" : ZString.Empty;
				IsMissingInfo.RefreshBinding(oldValue);
			}
		}
	}

	public ZPropertyInfo IsMissingInfo => GetZPropertyInfo(Schema.IsMissing);

	#region GenAddOn
	public static class GenAddOnColumnConstants
	{
		public const string PresentationDateColumnName = "PresentationDateOrigin";
	}
	#endregion

	[ResourceStringData("13007FBC-9AAF-44E0-8A45-108052434EDC", Caption = "Presentation Date", MediumCaption = "Presentation Date", ShortCaption = "Presentation Date", FullDescription = "Date of Presentation")]
	public ZDateTime PresentationDate
	{
		get => this.GetSystemDefinedValue<ZDateTime>(GenAddOnColumnConstants.PresentationDateColumnName);
		set
		{
			var oldValue = PresentationDate;
			if (value != oldValue)
			{
				this.SetSystemDefinedValue(GenAddOnColumnConstants.PresentationDateColumnName, value);
				PresentationDateInfo.RefreshBinding(oldValue);
			}
		}
	}

	public ZPropertyInfo PresentationDateInfo => GetZPropertyInfo(nameof(PresentationDate));

	[MaxLength(CusEntryNumber.Schema.CE_EntryNumMaxLength)]
	[ResourceStringData("6550958F-A314-45E3-A4BD-0ED04A324C07", Caption = "UCR", MediumCaption = "UCR", ShortCaption = "UCR", FullDescription = "UCR Number")]
	public ZString UCR
	{
		get => UCREntryNumber.CE_EntryNum;
		set
		{
			if (UCR != value)
			{
				UCREntryNumber.CE_EntryNum = value;
				RegisterEditableChildObject(ucrEntryNumber);
				UCRInfo.RefreshBinding();
			}
		}
	}

	public ZPropertyInfo UCRInfo => GetZPropertyInfo(nameof(UCR));

	CusEntryNumber UCREntryNumber
	{
		get
		{
			if (ucrEntryNumber == null || ucrEntryNumber.IsDeleted)
			{
				ucrEntryNumber = CusEntryNumber.LoadOrCreate<CusEntryNumber>(this, CusEntryNumberTypes.Standard.UniqueConsignementReference, CountryCode);
				ucrEntryNumber.CE_EntryNumInfo.ValueChanged += delegate
				{ MarkAsNeedingValidation(); };
				ucrEntryNumber.CE_IssueDateInfo.ValueChanged += delegate
				{ MarkAsNeedingValidation(); };
			}
			return ucrEntryNumber;
		}
	}
	CusEntryNumber ucrEntryNumber;

	public ZDecimal GrossWeightInKG => new ZWeight(API_GrossWeight, API_GrossWeightUQ).InKilogramsSafe;

	public ZInt TotalPackageQuantity => TemporaryStorageLinkPackages.Where(p => p.IsLinked).Sum(p => p.PackQty);

	public new TemporaryStorageBill Bill => (TemporaryStorageBill)base.Bill;

	public new TemporaryStoragePackedItemValidation Validation => (TemporaryStoragePackedItemValidation)base.Validation;

	protected override AsycudaPackedItemValidation GetNewValidation() => new TemporaryStoragePackedItemValidation(this);

	[ChildEditable]
	public new EU.Business.CusTempStorage.TemporaryStorageAdditionalInfoCollection<TemporaryStorageAdditionalInfo> AdditionalInfos =>
		(EU.Business.CusTempStorage.TemporaryStorageAdditionalInfoCollection<TemporaryStorageAdditionalInfo>)base.AdditionalInfos;

	protected override EU.Business.CusTempStorage.ITemporaryStorageAdditionalInfoCollection<EU.Business.CusTempStorage.TemporaryStorageAdditionalInfo> CreateNewAdditionalInfoCollection() =>
		new EU.Business.CusTempStorage.TemporaryStorageAdditionalInfoCollection<TemporaryStorageAdditionalInfo>(this);

	public new EU.Business.CusTempStorage.TemporaryStoragePreviousDocumentCollection<TemporaryStoragePreviousDocument> PreviousDocuments
		=> (EU.Business.CusTempStorage.TemporaryStoragePreviousDocumentCollection<TemporaryStoragePreviousDocument>)base.PreviousDocuments;

	protected override EU.Business.CusTempStorage.ITemporaryStoragePreviousDocumentCollection<EU.Business.CusTempStorage.TemporaryStoragePreviousDocument> CreateNewPreviousDocumentCollection()
=> new EU.Business.CusTempStorage.TemporaryStoragePreviousDocumentCollection<TemporaryStoragePreviousDocument>(this);

	protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
	{
		var dictionary = base.GetCusSupportingInfoTypes();
		dictionary[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo] = typeof(TemporaryStorageAdditionalInfo);
		dictionary[Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument] = typeof(TemporaryStoragePreviousDocument);
		return dictionary;
	}

	#region Cloning and copying

	protected override EU.Business.CusTempStorage.TemporaryStorageHeaderCloneStrategy GetTemporaryStorageHeaderCloneStrategy(BusinessObject bizObjToClone) => new TemporaryStorageHeaderCloneStrategy(bizObjToClone);

	protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
	{
		var newPackedItem = (TemporaryStoragePackedItem)base.CloneInternal(args);
		newPackedItem.UCR = UCR;

		SupplementaryCodes.ForEach(sp => newPackedItem.AdditionalSupplementaryCodes.AddNew(sp.CY_Code));
		newPackedItem.RefreshAllDutyAmountsFromTariffRates();

		return newPackedItem;
	}

	#endregion
}
