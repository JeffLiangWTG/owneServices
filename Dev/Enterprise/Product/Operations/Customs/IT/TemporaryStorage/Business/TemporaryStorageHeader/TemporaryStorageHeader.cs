using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.TemporaryStorage.Business.CodeDescriptionPairLists;
using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

public sealed class TemporaryStorageHeader : EU.Business.CusTempStorage.TemporaryStorageHeader, Integration.Customs.IT.ITemporaryStorageHeader, ICustomsLinkedObjectAdapterProvider
{
	public TemporaryStorageHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new TemporaryStorageBill MasterBill => (TemporaryStorageBill)base.MasterBill;

	[ResourceStringData("4472100D-3450-4837-AEC6-6A8B6283535E", Caption = "Account")]
	[List(nameof(Lookups) + "." + nameof(TemporaryStorageHeaderLookups.CustomsProfileList))]
	[MaxLength(20)]
	public override ZString AMA_CustomsProfile { get => base.AMA_CustomsProfile; set => base.AMA_CustomsProfile = value; }

	public override ZGuid AMA_OA_Declarant
	{
		get => base.AMA_OA_Declarant;
		set
		{
			var oldValue = base.AMA_OA_Declarant;
			if (!IsCopying && oldValue != value)
			{
				base.AMA_OA_Declarant = value;
				DefaultCustomsProfile();
			}
		}
	}

	protected override ZAddress GetNewAMA_OA_Declarant_ZAddress()
	{
		var result = base.GetNewAMA_OA_Declarant_ZAddress();
		result.DefaultAddressType = AddressType.OFC;
		return result;
	}

	public override ZGuid AMA_OA_Representative
	{
		get => base.AMA_OA_Representative;
		set
		{
			var oldValue = base.AMA_OA_Representative;
			if (!IsCopying && oldValue != value)
			{
				base.AMA_OA_Representative = value;
				DefaultCustomsProfile();
			}
		}
	}

	protected override ZAddress GetNewAMA_OA_Representative_ZAddress()
	{
		var result = base.GetNewAMA_OA_Representative_ZAddress();
		result.DefaultAddressType = AddressType.OFC;
		return result;
	}

	[ReadOnlyMember(nameof(IsCustomsStatusAMG))]
	public override ZString AMA_CustomsOffice { get => base.AMA_CustomsOffice; set => base.AMA_CustomsOffice = value; }

	[ReadOnlyMember(nameof(IsCustomsStatusAMG))]
	public override ZString AMA_TransportMode { get => base.AMA_TransportMode; set => base.AMA_TransportMode = value; }

	[ReadOnlyMember(nameof(IsCustomsStatusAMG))]
	public override ZString ArrivalTransportMeansCode { get => base.ArrivalTransportMeansCode; set => base.ArrivalTransportMeansCode = value; }

	[ReadOnlyMember(nameof(IsCustomsStatusAMG))]
	[ResourceStringData("71f88d1b-e1e4-43f3-9340-886766d05c5a", Caption = "Transport Type")]
	public override ZString TransportType { get => base.TransportType; set => base.TransportType = value; }

	[ResourceStringData("B2404C32-BFD3-4B7E-BE31-C5C04C9770E0", Caption = "Representative Qualification", MediumCaption = "Representative Qualification", ShortCaption = "Repres. Qual.")]
	[List(nameof(Lookups) + "." + nameof(TemporaryStorageHeaderLookups.RepresentativeQualificationList))]
	public override ZString AMA_AgentType { get => base.AMA_AgentType; set => base.AMA_AgentType = value; }

	protected override void DefaultAuthorizationProperties(object sender, EventArgs e)
	{
		// NOTE: This is a temporary fix
	}

	[ChildEditable]
	public new EU.Business.CusTempStorage.ITemporaryStorageBillCollection<TemporaryStorageBill, TemporaryStorageHeader> Bills => (EU.Business.CusTempStorage.ITemporaryStorageBillCollection<TemporaryStorageBill, TemporaryStorageHeader>)base.Bills;

	protected override EU.Business.CusTempStorage.ITemporaryStorageBillCollection<EU.Business.CusTempStorage.TemporaryStorageBill, EU.Business.CusTempStorage.TemporaryStorageHeader> CreateNewTemporaryStorageBillCollection()
		=> new EU.Business.CusTempStorage.TemporaryStorageBillCollection<TemporaryStorageBill, TemporaryStorageHeader>(this);

	public IReadOnlyCollection<TemporaryStoragePackedItem> TemporaryStoragePackedItems => Factory.GetCached(ref _temporaryStoragePackedItemsCache, () => [.. Bills.SelectMany(x => x.PackedItems)]);
	CachedProperty<IReadOnlyCollection<TemporaryStoragePackedItem>> _temporaryStoragePackedItemsCache;

	public HashSet<ZString> DuplicatedBillNumbers => Factory.GetCached(ref _duplicatedBillNumbersCache, GetDuplicatedBillNumber);
	CachedProperty<HashSet<ZString>> _duplicatedBillNumbersCache;

	HashSet<ZString> GetDuplicatedBillNumber()
	{
		var duplicates = Bills
			.GroupBy(bill => bill.ABL_BillNumber)
			.Where(group => group.Count() > 1)
			.Select(group => group.Key)
			.ToHashSet();

		return duplicates;
	}

	protected override Type GetBillTypeCore() => typeof(TemporaryStorageBill);

	protected override Type GoodsLocationTypeCore => typeof(CusGoodsLocation);

	public new CusGoodsLocation GoodsLocation => (CusGoodsLocation)base.GoodsLocation;

	public new TemporaryStorageHeaderLookups Lookups => (TemporaryStorageHeaderLookups)base.Lookups;

	protected override AsycudaManifestHeaderLookups GetNewLookups() => new TemporaryStorageHeaderLookups(this);

	public new TemporaryStorageHeaderValidation Validation => (TemporaryStorageHeaderValidation)base.Validation;

	protected override AsycudaManifestHeaderValidation GetNewValidation() => new TemporaryStorageHeaderValidation(this);

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();

		DefaultCustomsProfile();

		AMA_AgentType = ZString.Empty;
		AMA_Calc_HasHouseConsignment = true;
	}

	protected override IDictionary<ZString, Type> GetCusSupportingInfoTypesCore()
	{
		var dictionary = base.GetCusSupportingInfoTypesCore();
		dictionary[Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument] = typeof(TemporaryStoragePreviousDocument);
		return dictionary;
	}

	public new EU.Business.CusTempStorage.ITemporaryStoragePreviousDocumentCollection<TemporaryStoragePreviousDocument> PreviousDocuments =>
		(EU.Business.CusTempStorage.ITemporaryStoragePreviousDocumentCollection<TemporaryStoragePreviousDocument>)base.PreviousDocuments;

	protected override EU.Business.CusTempStorage.ITemporaryStoragePreviousDocumentCollection<EU.Business.CusTempStorage.TemporaryStoragePreviousDocument> CreateNewPreviousDocumentCollection() => new EU.Business.CusTempStorage.TemporaryStoragePreviousDocumentCollection<TemporaryStoragePreviousDocument>(this);

	void DefaultCustomsProfile()
	{
		var customsProfileList = Lookups.CustomsProfileList;
		if (customsProfileList.Count == 1)
		{
			AMA_CustomsProfile = customsProfileList[0].Code;
		}
	}

	protected override Type ArrivalTransportMeansTypeCore => typeof(ArrivalTransportMeans);

	protected override ZString GetCusGoodsLocationProviderKeyCore() => Core.Constants.CountryCodes.Italy + GoodsLocationProviderApplications.Codes.PresentationNotificationAndTemporaryStorage;

	public new IAsycudaContainerCollection<TemporaryStorageContainer, TemporaryStorageHeader> Containers => (IAsycudaContainerCollection<TemporaryStorageContainer, TemporaryStorageHeader>)base.Containers;

	protected override IAsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader> CreateNewAsycudaContainerCollection() => new AsycudaContainerCollection<TemporaryStorageContainer, TemporaryStorageHeader>(this);

	protected override Type GetContainerTypeCore() => typeof(TemporaryStorageContainer);

	public ISadCustomsLinkedObjectAdapter GetSadCustomsLinkedObjectAdapter()
	{
		throw new NotImplementedException();
	}

	public ISingleWindowCustomsLinkedObjectAdapter GetNewSingleWindowCustomsLinkedObjectAdapter()
	{
		throw new NotImplementedException();
	}

	public IXmlCustomsLinkedObjectAdapter GetNewXmlCustomsLinkedObjectAdapter()
	{
		return new TemporaryStorageCustomsLinkedObjectAdapter(this);
	}

	public new ITEDIMessageCollection Messages => (ITEDIMessageCollection)base.Messages;
	protected override Messaging.Business.EDIMessageCollection CreateNewEDIMessageCollection() => new ITEDIMessageCollection(this);

	protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
	{
		var newHeader = (TemporaryStorageHeader)base.CloneInternal(args);

		newHeader.TransportType = TransportType;
		newHeader.PresentationCustomsOffice = PresentationCustomsOffice;

		var newGoodsLocation = newHeader.GoodsLocation;
		var goodsLocation = GoodsLocation;
		newGoodsLocation.CGL_Qualifier = goodsLocation.CGL_Qualifier;
		newGoodsLocation.CGL_Type = goodsLocation.CGL_Type;
		newGoodsLocation.AdditionalIdentifier = goodsLocation.AdditionalIdentifier;

		var newAddress = newGoodsLocation.Address;
		var address = goodsLocation.Address;
		newAddress.IdentificationHolderPK = address.IdentificationHolderPK;
		newAddress.AuthorisationNumber = address.AuthorisationNumber;

		return newHeader;
	}

	protected override void OnCustomsStatusChangedCore(ZString oldValue, ZString newValue)
	{
		base.OnCustomsStatusChangedCore(oldValue, newValue);
		if (IsCustomsStatusAMG)
		{
			Containers.SetCountedReadOnlyIncludingChildren(true);

			foreach (var bill in Bills)
			{
				bill.SupplyChainActors.SetCountedReadOnlyIncludingChildren(true);

				foreach (var packedItem in bill.PackedItems)
				{
					packedItem.AdditionalInfos.SetCountedReadOnlyIncludingChildren(true);
					packedItem.SupportingDocuments.SetCountedReadOnlyIncludingChildren(true);
					packedItem.SupplyChainActors.SetCountedReadOnlyIncludingChildren(true);
				}
			}
		}
	}

	public bool IsCustomsStatusAMG => CustomsStatus == PNTSCustomsStatusList.Codes.Amending;

	public void SetCustomsStatusAsRegistered(ZDateTime acceptanceDate)
	{
		var allBillsHaveMrn = Bills.All(bill => !bill.Mrn.IsEmpty);

		CustomsStatus = allBillsHaveMrn
			? PNTSCustomsStatusList.Codes.FullyActivated
			: PNTSCustomsStatusList.Codes.PartialActivated;

		if (!allBillsHaveMrn)
		{
			AMA_MessageStatus = ZString.Empty;
		}

		if (!acceptanceDate.IsEmpty && (CustomsStatusDate.IsEmpty || acceptanceDate > CustomsStatusDate))
		{
			CustomsStatusDate = acceptanceDate;
		}
	}

	protected override void OnFactorySaving()
	{
		base.OnFactorySaving();
		if (CustomsStatus == PNTSCustomsStatusList.Codes.PartialActivated && Bills.HasChanges)
		{
			SetCustomsStatusAsRegistered(ZDateTime.Empty);
		}
	}
}
