using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AE.Manifest.Business;

public class AsycudaManifestHeader : ASYCUDA.Business.AsycudaManifestHeader
	, Integration.Customs.ASYCUDA.AEManifest.IAsycudaManifestHeader, ISupportingDocObject
{
	public AsycudaManifestHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new class Schema : ASYCUDA.Business.AsycudaManifestHeader.Schema
	{
		public const int AMA_CustomsOriginPortMaxLength = 5;
		public new const int AMA_CarrierCodeMaxLength = 3;
	}

	[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.CustomsOriginPortList))]
	[MaxLength(Schema.AMA_CustomsOriginPortMaxLength)]
	[ResourceStringData("Enterprise.Customs.AE.Manifest.Business.AsycudaManifestHeader|AMA_CustomsOriginPort", Caption = "Last Foreign Port")]
	public override ZString AMA_CustomsOriginPort { get => base.AMA_CustomsOriginPort; set => base.AMA_CustomsOriginPort = value; }

	[MaxLength(Schema.AMA_CarrierCodeMaxLength)]
	public override ZString AMA_CarrierCode { get => base.AMA_CarrierCode; set => base.AMA_CarrierCode = value; }

	public override ZGuid AMA_OA_Carrier
	{
		get => base.AMA_OA_Carrier;
		set
		{
			var oldValue = AMA_OA_Carrier;
			base.AMA_OA_Carrier = value;
			if (AMA_OA_Carrier != oldValue && !IsCopying && AMA_CarrierCode.IsEmpty)
			{
				AMA_CarrierCode = Carrier?.Header?.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.CarrierCode, Core.Constants.CountryCodes.UnitedArabEmirates) ?? ZString.Empty;
			}
		}
	}

	[ResourceStringData("Enterprise.Customs.AE.Manifest.Business.AsycudaManifestHeader|AMA_OA_ShippingAgent", Caption = "Default Freight Forwarder", ShortCaption = "Default Frt. Forwarder")]
	public override ZGuid AMA_OA_ShippingAgent { get => base.AMA_OA_ShippingAgent; set => base.AMA_OA_ShippingAgent = value; }

	public override ZString MessageStatus => AMA_MessageStatus.IsEmpty ? CombineBillStatuses(bill => bill.ABL_MessageStatus, StatusCodeMultiple) : AMA_MessageStatus;

	[ResourceStringData("Enterprise.Customs.AE.Manifest.Business.AsycudaManifestHeader|CarrierMPCI", Caption = "MPCI ID")]
	public ZString CarrierMPCI => Carrier?.Header?.CustomsCodes.GetCustomsRegNo(OrgCusCode.UnitedArabEmiratesCodeTypes.MPCINumber, Core.Constants.CountryCodes.UnitedArabEmirates) ?? ZString.Empty;

	public ZPropertyInfo CarrierMPCIInfo => GetZPropertyInfo(nameof(CarrierMPCI));

	[ResourceStringData("Enterprise.Customs.AE.Manifest.Business.AsycudaManifestHeader|ShippingAgentMPCI", Caption = "MPCI ID")]
	public ZString ShippingAgentMPCI => ShippingAgent?.Header?.CustomsCodes.GetCustomsRegNo(OrgCusCode.UnitedArabEmiratesCodeTypes.MPCINumber, Core.Constants.CountryCodes.UnitedArabEmirates) ?? ZString.Empty;

	public ZPropertyInfo ShippingAgentMPCIInfo => GetZPropertyInfo(nameof(ShippingAgentMPCI));

	public new AsycudaManifestHeaderLookups Lookups => (AsycudaManifestHeaderLookups)base.Lookups;

	protected override ManifestBase.AsycudaManifestHeaderLookups GetNewLookups() => new AsycudaManifestHeaderLookups(this);

	public new AsycudaBill MasterBill => (AsycudaBill)base.MasterBill;

	public new AsycudaBillCollection Bills => (AsycudaBillCollection)base.Bills;

	protected override ManifestBase.IAsycudaBillCollection<ManifestBase.AsycudaBill, ManifestBase.AsycudaManifestHeader> CreateNewAsycudaBillCollection()
	{
		return new AsycudaBillCollection(this);
	}

	public new AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader> Containers => (AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>)base.Containers;

	protected override IAsycudaContainerCollection<ManifestBase.AsycudaContainer, ManifestBase.AsycudaManifestHeader> CreateNewAsycudaContainerCollection()
	{
		return new AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>(this);
	}

	protected override Type GetBillTypeCore() => typeof(AsycudaBill);

	protected override Type GetContainerTypeCore() => typeof(AsycudaContainer);

	protected override ZString GetDefaultCountryCode() => Core.Constants.CountryCodes.UnitedArabEmirates;

	protected override ASYCUDA.Business.MessageChooser GetNewMessageChooserCore(IEnumerable<ISelectionItem> items, string messageType, bool showStatus)
	{
		return new MessageChooser(this, items, showStatus);
	}

	protected override ManifestBase.AsycudaManifestHeaderValidation GetNewValidation() => new AsycudaManifestHeaderValidation(this);

	public override ManifestBase.AsycudaPackPackedItemPivotCollection.RelationshipType PackedItemRelationship => ManifestBase.AsycudaPackPackedItemPivotCollection.RelationshipType.One;

	public override bool ShowPackedItems => false;

	public ZString CountryCode => GetDefaultCountryCode();

	protected override ZString GetDataGroupingCore() => Core.Constants.Customs.Universal.RefDataGrouping.Codes.UAEManifest;

	public override BaseMessageSendingNotificationHelper GetMessageSendingNotificationHelper()
	{
		return new AEMessageSendingNotificationHelper(this);
	}

	public Customs.Business.SupportingDocSendingObject GetSupportingDocSendingObject()
	{
		return new SupportingDocSendingObject(this);
	}

	protected override BusinessObjectSynchroniser GetConsolSynchronizerCore(ForwardingConsol source) => new AsycudaManifestHeaderSynchroniser(this, source);
}
