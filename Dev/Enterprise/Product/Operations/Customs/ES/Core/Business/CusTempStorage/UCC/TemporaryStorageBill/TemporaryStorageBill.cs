using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.CusTempStorage;

[DependentBusinessObject(typeof(TemporaryStorageHeader), nameof(TemporaryStorageHeader.Bills))]
public class TemporaryStorageBill : EU.Business.CusTempStorage.TemporaryStorageBill, Integration.Customs.ES.ITemporaryStorageBill
{
	public TemporaryStorageBill(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new TemporaryStorageHeader Header => (TemporaryStorageHeader)base.Header;

	bool IsMessageSent => Header?.IsSent ?? false;

	[ReadOnlyMember(nameof(IsMessageSent))]
	[ResourceStringData("7761EDD5-14F9-42C9-A7DA-4352FD2C208B", Caption = "Transport Document Type", MediumCaption = "Transport Document Type", ShortCaption = "Transp. Doc Type", FullDescription = "Type of Transport Document")]
	public override ZString TypeOfBillDocument { get => base.TypeOfBillDocument; set => base.TypeOfBillDocument = value; }

	[ReadOnlyMember(nameof(IsMessageSent))]
	[ResourceStringData("164D301E-4E6C-4CD1-AC2F-A3A2EB51AF5E", Caption = "Transport Document", MediumCaption = "Transport Document", ShortCaption = "Transp. Doc", FullDescription = "Transport Document Number")]
	public override ZString ABL_BillNumber { get => base.ABL_BillNumber; set => base.ABL_BillNumber = value; }

	protected override ManifestBase.AsycudaBillValidation GetNewValidation() => new TemporaryStorageBillValidation(this);

	public new TemporaryStorageBillValidation Validation => (TemporaryStorageBillValidation)base.Validation;

	public new EU.Business.CusTempStorage.TemporaryStorageAdditionalInfoCollection<TemporaryStorageAdditionalInfo> AdditionalInfos
		=> (EU.Business.CusTempStorage.TemporaryStorageAdditionalInfoCollection<TemporaryStorageAdditionalInfo>)base.AdditionalInfos;

	protected override EU.Business.CusTempStorage.ITemporaryStorageAdditionalInfoCollection<EU.Business.CusTempStorage.TemporaryStorageAdditionalInfo> CreateNewAdditionalInfoCollection()
		=> new EU.Business.CusTempStorage.TemporaryStorageAdditionalInfoCollection<TemporaryStorageAdditionalInfo>(this);

	public new EU.Business.CusTempStorage.TemporaryStoragePreviousDocumentCollection<TemporaryStoragePreviousDocument> PreviousDocuments
		=> (EU.Business.CusTempStorage.TemporaryStoragePreviousDocumentCollection<TemporaryStoragePreviousDocument>)base.PreviousDocuments;

	protected override EU.Business.CusTempStorage.ITemporaryStoragePreviousDocumentCollection<EU.Business.CusTempStorage.TemporaryStoragePreviousDocument> CreateNewPreviousDocumentCollection()
		=> new EU.Business.CusTempStorage.TemporaryStoragePreviousDocumentCollection<TemporaryStoragePreviousDocument>(this);

	public new TemporaryStoragePackedItemCollection PackedItems
		=> (TemporaryStoragePackedItemCollection)base.PackedItems;

	protected override ManifestBase.IAsycudaBillPackedItemCollection<ManifestBase.AsycudaPackedItem, ManifestBase.AsycudaBill> CreateNewAsycudaBillPackedItemCollection()
		=> new TemporaryStoragePackedItemCollection(this);

	protected override Type GetPackedItemTypeCore() => typeof(TemporaryStoragePackedItem);

	public const string ChildMocCode = "MOC";

	protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
	{
		var result = base.GetCusSupportingInfoTypes();
		result[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo] = typeof(TemporaryStorageAdditionalInfo);
		result[Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument] = typeof(TemporaryStoragePreviousDocument);
		return result;
	}

	protected override ZString GetRegNoForPartyCore(OrgAddress address, ZString[] regNoTypes) => OrgHeaderExtension.GetIDCode(address?.Header);

	#region Cloning and copying

	protected override EU.Business.CusTempStorage.TemporaryStorageHeaderCloneStrategy GetTemporaryStorageHeaderCloneStrategy(BusinessObject bizObjToClone) => new TemporaryStorageHeaderCloneStrategy(bizObjToClone);

	protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
	{
		var newBill = (TemporaryStorageBill)base.CloneInternal(args);
		newBill.TypeOfBillDocument = TypeOfBillDocument;

		return newBill;
	}

	#endregion
}
