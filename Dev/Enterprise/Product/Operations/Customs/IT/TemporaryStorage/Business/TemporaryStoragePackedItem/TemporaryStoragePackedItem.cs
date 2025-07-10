using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.ManifestBase;
using Enterprise.ZArchitecture;
using static Enterprise.Core.Constants;
using ITLinkPackageCollection = Enterprise.Customs.IT.TemporaryStorage.Business.TemporaryStorageLinkPackageCollection<Enterprise.Customs.IT.TemporaryStorage.Business.TemporaryStorageLinkPackage>;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

public class TemporaryStoragePackedItem : EU.Business.CusTempStorage.TemporaryStoragePackedItem
{
	public new class Schema : EU.Business.CusTempStorage.TemporaryStoragePackedItem.Schema
	{
		public new const int API_GrossWeightDecimalPlaces = 3;
		public const int API_CustomsQty2DecimalPlaces = 6;
	}

	public TemporaryStoragePackedItem(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new TemporaryStorageSupportingDocumentCollection<TemporaryStorageSupportingDocument> SupportingDocuments => (TemporaryStorageSupportingDocumentCollection<TemporaryStorageSupportingDocument>)base.SupportingDocuments;

	protected override ITemporaryStorageSupportingDocumentCollection<EU.Business.CusTempStorage.TemporaryStorageSupportingDocument> CreateNewSupportingDocumentCollection()
	{
		var result = new TemporaryStorageSupportingDocumentCollection<TemporaryStorageSupportingDocument>(this);
		if (IsCustomsStatusAMG)
		{
			result.SetReadOnlyIncludingChildren(true);
		}
		return result;
	}

	public new ITemporaryStoragePreviousDocumentCollection<TemporaryStoragePreviousDocument> PreviousDocuments => (ITemporaryStoragePreviousDocumentCollection<TemporaryStoragePreviousDocument>)base.PreviousDocuments;

	protected override ITemporaryStoragePreviousDocumentCollection<EU.Business.CusTempStorage.TemporaryStoragePreviousDocument> CreateNewPreviousDocumentCollection() => new TemporaryStoragePreviousDocumentCollection<TemporaryStoragePreviousDocument>(this);

	protected override int API_GrossWeightDecimalPlaces => Schema.API_GrossWeightDecimalPlaces;

	[ReadOnlyMember(nameof(IsCustomsStatusAMG))]
	[DecimalPlaces(Schema.API_NetWeightDecimalPlaces)]
	public override ZDecimal API_NetWeight { get => base.API_NetWeight; set => base.API_NetWeight = value; }

	[ReadOnlyMember(nameof(IsCustomsStatusAMG))]
	public override ZString API_ChemicalSubstanceCode { get => base.API_ChemicalSubstanceCode; set => base.API_ChemicalSubstanceCode = value; }

	[ReadOnlyMember(nameof(IsCustomsStatusAMG))]
	[ResourceStringData("1541642D-AA79-438C-A401-F7556FA9AFE6", Caption = "Supplementary Quantity", MediumCaption = "Supplementary Qty", ShortCaption = "Sup. Qty", FullDescription = "Supplementary Additional Quantity for Liability Amount Calculation")]
	[DecimalPlaces(Schema.API_CustomsQty2DecimalPlaces)]
	public override ZDecimal API_CustomsQty2 { get => base.API_CustomsQty2; set => base.API_CustomsQty2 = value; }

	[ResourceStringData("DEA39EAA-777C-4E5C-BD51-3936C81B1328", Caption = "Supplementary Unit Quantity", MediumCaption = "Supplementary Unit Qty", ShortCaption = "Sup. Unit Qty", FullDescription = "Supplementary Additional Unit for Liability Amount Calculation")]
	public override ZString API_CustomsUQ2 { get => base.API_CustomsUQ2; set => base.API_CustomsUQ2 = value; }

	public ZDecimal GrossWeightInKG => new ZWeight(API_GrossWeight, API_GrossWeightUQ).InKilogramsSafe;

	public ZDecimal NetWeightInKG => new ZWeight(API_NetWeight, API_NetWeightUQ).InKilogramsSafe;

	public new TemporaryStorageBill Bill => (TemporaryStorageBill)base.Bill;

	[ReadOnly(true)]
	[ResourceStringData("DBFE5725-621B-4752-8988-8D15D7954B3A", Caption = "Registration No.", MediumCaption = "Registration No.", ShortCaption = "Registration No.")]
	public ZString RegistrationNo => LoadCusEntryNum(ref regCusEntryNumber, CusEntryNumberTypes.EU.CustomsRegistry)?.CE_EntryNum ?? ZString.Empty;

	[ReadOnly(true)]
	[ResourceStringData("9995BB3D-93B2-4D1D-A1F0-22312BB08881", Caption = "Release Date", MediumCaption = "Release Date", ShortCaption = "Release Date")]
	public ZDateTime ReleaseDate => LoadCusEntryNum(ref regCusEntryNumber, CusEntryNumberTypes.EU.CustomsRegistry)?.CE_IssueDate ?? ZDateTime.Empty;

	public ZPropertyInfo ReleaseDateInfo => GetZPropertyInfo(nameof(ReleaseDate));

	protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
	{
		var dictionary = base.GetCusSupportingInfoTypes();
		dictionary[Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument] = typeof(TemporaryStorageSupportingDocument);
		dictionary[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo] = typeof(TemporaryStorageAdditionalInfo);
		dictionary[Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument] = typeof(TemporaryStoragePreviousDocument);
		return dictionary;
	}

	public new TemporaryStorageAdditionalInfoCollection<TemporaryStorageAdditionalInfo> AdditionalInfos => (TemporaryStorageAdditionalInfoCollection<TemporaryStorageAdditionalInfo>)base.AdditionalInfos;

	protected override ITemporaryStorageAdditionalInfoCollection<EU.Business.CusTempStorage.TemporaryStorageAdditionalInfo> CreateNewAdditionalInfoCollection()
	{
		var result = new TemporaryStorageAdditionalInfoCollection<TemporaryStorageAdditionalInfo>(this);
		if (IsCustomsStatusAMG)
		{
			result.SetReadOnlyIncludingChildren(true);
		}
		return result;
	}

	protected override ICusSupplyChainActorReferenceCollection<CusSupplyChainActorReference> CreateNewSupplyChainActors()
	{
		var result = new CusSupplyChainActorReferenceCollection<CusSupplyChainActorReference>(this);
		if (IsCustomsStatusAMG)
		{
			result.SetReadOnlyIncludingChildren(true);
		}
		return result;
	}

	protected override AsycudaPackedItemValidation GetNewValidation() => new TemporaryStoragePackedItemValidation(this);

	protected override ITemporaryStorageLinkPackageCollection<EU.Business.CusTempStorage.TemporaryStorageLinkPackage> GetNewTemporaryStorageLinkPackagesCore() => new ITLinkPackageCollection(this);

	protected override bool IsFormattedTariffReadOnly => IsCustomsStatusAMG;

	bool IsCustomsStatusAMG => Bill?.Header?.IsCustomsStatusAMG ?? ZBool.False;

	CusEntryNumber LoadCusEntryNum(ref CusEntryNumber entryNum, ZString entryType)
	{
		if (entryNum == null || entryNum.IsDeleted)
		{
			entryNum = CusEntryNumber.Load(this, entryType, CountryCodes.Italy);
		}
		return entryNum;
	}

	CusEntryNumber regCusEntryNumber;
}
