using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common;
using Enterprise.Customs.ManifestBase;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

[DependentBusinessObject(typeof(TemporaryStorageHeader), nameof(TemporaryStorageHeader.Bills))]
public sealed class TemporaryStorageBill : EU.Business.CusTempStorage.TemporaryStorageBill, Integration.Customs.IT.ITemporaryStorageBill
{
	public TemporaryStorageBill(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new class Schema : EU.Business.CusTempStorage.TemporaryStorageBill.Schema
	{
		public const int GrossWeightDecimalPlaces = 6;
		public const int NetWeightDecimalPlaces = 6;
		public const int CustomsQty2DecimalPlaces = 6;
	}

	public new TemporaryStorageHeader Header => (TemporaryStorageHeader)base.Header;

	protected override AsycudaBillValidation GetNewValidation() => new TemporaryStorageBillValidation(this);

	public new TemporaryStorageBillValidation Validation => (TemporaryStorageBillValidation)base.Validation;

	[ReadOnly(true)]
	[ResourceStringData("3AE006F7-2C14-476C-B096-02E0947E0A26", Caption = "LRN", MediumCaption = "LRN", ShortCaption = "LRN")]
	public ZString Lrn => LoadCusEntryNum(ref lrnCusEntryNumber, CusEntryNumberTypes.Standard.LocalReferenceNumber)?.CE_EntryNum ?? ZString.Empty;

	CusEntryNumber lrnCusEntryNumber;

	[ReadOnly(true)]
	[ResourceStringData("94278635-8795-49F9-92CE-3A52F7ED77D2", Caption = "MRN", MediumCaption = "MRN", ShortCaption = "MRN")]
	public ZString Mrn => LoadCusEntryNum(ref mrnCusEntryNumber, CusEntryNumberTypes.Standard.MovementReferenceNumber)?.CE_EntryNum ?? ZString.Empty;

	public ZDateTime RegistrationDate => LoadCusEntryNum(ref mrnCusEntryNumber, CusEntryNumberTypes.Standard.MovementReferenceNumber)?.CE_IssueDate ?? ZDateTime.Empty;

	CusEntryNumber mrnCusEntryNumber;

	public new EU.Business.CusTempStorage.ITemporaryStoragePackedItemCollection<TemporaryStoragePackedItem, TemporaryStorageBill> PackedItems =>
		(EU.Business.CusTempStorage.ITemporaryStoragePackedItemCollection<TemporaryStoragePackedItem, TemporaryStorageBill>)base.PackedItems;

	protected override Type GetPackedItemTypeCore() => typeof(TemporaryStoragePackedItem);

	protected override IAsycudaBillPackedItemCollection<AsycudaPackedItem, AsycudaBill> CreateNewAsycudaBillPackedItemCollection()
	{
		return new EU.Business.CusTempStorage.TemporaryStoragePackedItemCollection<TemporaryStoragePackedItem, TemporaryStorageBill>(this);
	}

	CusEntryNumber LoadCusEntryNum(ref CusEntryNumber entryNum, ZString entryType)
	{
		if (entryNum == null || entryNum.IsDeleted)
		{
			entryNum = CusEntryNumber.Load(this, entryType, GetCountryCode());
		}
		return entryNum;
	}

	[ReadOnly(true), DecimalPlaces(Schema.GrossWeightDecimalPlaces)]
	[ResourceStringData("D7F315D0-C10C-4174-BEBB-C333DEF17E35", Caption = "Gross Weight in KG", MediumCaption = "Gross Weight", ShortCaption = "Gross Wgt.", FullDescription = "Bill Total Gross Weight in KG")]
	public ZDecimal GrossWeightInKG => Factory.GetValue(ref grossWeightInKGCached, () => PackedItems.Sum(x => x.GrossWeightInKG));
	CachedProperty<ZDecimal> grossWeightInKGCached;

	[ReadOnly(true), DecimalPlaces(Schema.NetWeightDecimalPlaces)]
	[ResourceStringData("E1B48729-F4C3-4C8A-91AA-926D5B4A2A79", Caption = "Net Weight in KG", MediumCaption = "Net Weight", ShortCaption = "Net Wgt.", FullDescription = "Bill Total Net Weight in KG")]
	public ZDecimal NetWeightInKG => Factory.GetValue(ref netWeightInKGCached, () => PackedItems.Sum(x => x.NetWeightInKG));
	CachedProperty<ZDecimal> netWeightInKGCached;

	[ReadOnly(true), DecimalPlaces(Schema.CustomsQty2DecimalPlaces)]
	[ResourceStringData("B9E5D72C-1B1D-470B-AC5F-8FA38C1C2E8D", Caption = "Supp. Quantity", MediumCaption = "Supp. Quantity", ShortCaption = "Supp. Qty.", FullDescription = "Bill Total Supplementary Quantity")]
	public ZDecimal SuppQuantity
	{
		get
		{
			return Factory.GetValue(ref suppQuantityCached, () =>
			{
				if (PackedItems.Any() && PackedItems.All(x => x.API_CustomsUQ2 == PackedItems[0].API_CustomsUQ2))
				{
					return PackedItems.Sum(x => x.API_CustomsQty2);
				}
				return ZDecimal.Zero;
			});
		}
	}

	protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
	{
		var dictionary = base.GetCusSupportingInfoTypes();
		dictionary[Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument] = typeof(TemporaryStoragePreviousDocument);
		return dictionary;
	}

	public new EU.Business.CusTempStorage.ITemporaryStoragePreviousDocumentCollection<TemporaryStoragePreviousDocument> PreviousDocuments =>
		(EU.Business.CusTempStorage.ITemporaryStoragePreviousDocumentCollection<TemporaryStoragePreviousDocument>)base.PreviousDocuments;

	protected override EU.Business.CusTempStorage.ITemporaryStoragePreviousDocumentCollection<EU.Business.CusTempStorage.TemporaryStoragePreviousDocument> CreateNewPreviousDocumentCollection() => new EU.Business.CusTempStorage.TemporaryStoragePreviousDocumentCollection<TemporaryStoragePreviousDocument>(this);

	protected override Type GetPackTypeCore() => typeof(TemporaryStoragePack);

	CachedProperty<ZDecimal> suppQuantityCached;

	public override bool CanDelete => Mrn.IsEmpty;

	public override MultilingualString ReasonForNotAbleToDelete
		=> ResString.GetMultilingualString("1EBD4D89-DDB0-41FA-833C-825037570DDB", "It is not allowed to remove a bill with the MRN filled.");

	[ReadOnlyMember(nameof(IsCustomsStatusAMG))]
	public override ZString ABL_GoodsDescription
	{
		get => base.ABL_GoodsDescription;
		set => base.ABL_GoodsDescription = value;
	}

	bool IsCustomsStatusAMG => Header?.IsCustomsStatusAMG ?? ZBool.False;

	protected override bool IsSupplyChainActorReferenceCollectionReadOnly => base.IsSupplyChainActorReferenceCollectionReadOnly || IsCustomsStatusAMG;

	protected override bool IsUCRNumberReadOnly => base.IsUCRNumberReadOnly || IsCustomsStatusAMG;

	public CusTempStorageRegHeader RegisterHeader => Factory.GetValue(ref cachedRegisterHeader, () =>
	{
		if (Header.AMA_JobReference.IsEmpty || Mrn.IsEmpty)
		{
			return null;
		}

		var query = new ZQuery(CusTempStorageRegHeaderSchema.SRH_InternalReference, Header.AMA_JobReference);
		query.AddToFilter(CusTempStorageRegHeaderSchema.SRH_PreviousReference, Mrn);
		return Factory.LoadTop1<CusTempStorageRegHeader>(query);
	});

	CachedProperty<CusTempStorageRegHeader> cachedRegisterHeader;
}
