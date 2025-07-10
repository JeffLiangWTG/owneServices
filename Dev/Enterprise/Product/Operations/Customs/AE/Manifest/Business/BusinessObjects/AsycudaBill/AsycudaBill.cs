using System;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.AE.Business;
using Enterprise.Customs.Universal.Helper;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AE.Manifest.Business;

[CodeProperty(nameof(AsycudaBill.ABL_BillNumber)), DescriptionProperty(nameof(AsycudaBill.ABL_BillNumber))]
public class AsycudaBill : ASYCUDA.Business.AsycudaBill
	, IMessageAttachee
{
	public AsycudaBill(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new class Schema : ASYCUDA.Business.AsycudaBill.Schema
	{
		public const string Negotiable = nameof(AsycudaBill.Negotiable);
		public const string ABL_SplitBill = nameof(AsycudaBill.ABL_SplitBill);
		public const string Payer = nameof(AsycudaBill.Payer);
		public const string ABL_SplitBillNumber = nameof(AsycudaBill.ABL_SplitBillNumber);
		public const int NegotiableMaxLength = 1;
		public const int PayerMaxLength = 70;
	}

	public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;

	[ResourceStringData("AEAsycudaBill|ABL_CargoType", Caption = "Cargo Type", ShortCaption = "Type")]
	[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.AECargoTypeList))]
	public override ZString ABL_CargoType
	{
		get => base.ABL_CargoType;
		set => base.ABL_CargoType = value;
	}

	[ResourceStringData("AEAsycudaBill|ABL_SpecialCargoCode", Caption = "Service Requirement", MediumCaption = "Service Req.", ShortCaption = "Serv. Req.")]
	[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.ServiceRequirementCodeList))]
	public override ZString ABL_SpecialCargoCode
	{
		get => base.ABL_SpecialCargoCode;
		set => base.ABL_SpecialCargoCode = value;
	}

	[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.CustomsOriginPortList))]
	public override ZString ABL_CustomsOriginPort
	{
		get => base.ABL_CustomsOriginPort;
		set => base.ABL_CustomsOriginPort = value;
	}

	protected override bool ABL_BillStatus_ReadOnly => true;

	[ResourceStringData("AEAsycudaBill|ABL_FreightValue", Caption = "Goods Value")]
	public override ZDecimal ABL_FreightValue
	{
		get => base.ABL_FreightValue;
		set => base.ABL_FreightValue = value;
	}

	[ResourceStringData("AEAsycudaBill|ABL_RX_NKFreightValueCurrency", Caption = "Goods Currency")]
	public override ZString ABL_RX_NKFreightValueCurrency
	{
		get => base.ABL_RX_NKFreightValueCurrency;
		set => base.ABL_RX_NKFreightValueCurrency = value;
	}

	[ResourceStringData("f955d611-e547-481a-841c-edc7c6c2fc64", Caption = "Sender Reference")]
	[ReadOnly(true)]
	public override ZString ABL_SenderReference
	{
		get => base.ABL_SenderReference;
		set => base.ABL_SenderReference = value;
	}

	[ResourceStringData("AEAsycudaBill|Negotiable", Caption = "Negotiable?")]
	[MaxLength(Schema.NegotiableMaxLength)]
	[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.NegotiableList))]
	public ZString Negotiable
	{
		get => this.GetSystemDefinedValue<ZString>(Schema.Negotiable);
		set
		{
			var oldValue = Negotiable;
			CheckMaximumLength(NegotiableInfo, value);
			this.SetSystemDefinedValue(Schema.Negotiable, value);
			NegotiableInfo.RefreshBinding(oldValue);
			if (!IsCopying && oldValue != value)
			{
				Payer = ZString.Empty;
			}
			if (!IsValidationSuspended)
			{
				(Validation as AsycudaBillValidationForRegularBill)?.ValidateNegotiable();
			}
		}
	}

	public ZPropertyInfo NegotiableInfo => GetZPropertyInfo(nameof(Negotiable));

	[ResourceStringData("AEAsycudaBill|Payer", Caption = "Payer")]
	[MaxLength(Schema.PayerMaxLength)]
	[ReadOnlyMember(nameof(Payer_ReadOnly))]
	public ZString Payer
	{
		get => this.GetSystemDefinedValue<ZString>(Schema.Payer);
		set
		{
			var oldValue = Payer;
			CheckMaximumLength(PayerInfo, value);
			this.SetSystemDefinedValue(Schema.Payer, value);
			PayerInfo.RefreshBinding(oldValue);
			if (!IsValidationSuspended)
			{
				(Validation as AsycudaBillValidationForRegularBill)?.ValidatePayer();
			}
		}
	}

	public ZPropertyInfo PayerInfo => GetZPropertyInfo(nameof(Payer));

	public ZBool Payer_ReadOnly => !Negotiable.Equals(NegotiableList.Codes.Yes);

	[ResourceStringData("AEAsycudaBill|ABL_SplitBill", Caption = "Split Bill")]
	public ZBool ABL_SplitBill
	{
		get => this.GetSystemDefinedValue<ZBool>(Schema.ABL_SplitBill);
		set
		{
			var oldValue = ABL_SplitBill;
			this.SetSystemDefinedValue(Schema.ABL_SplitBill, value);
			if (!IsCopying && !value)
			{
				ABL_SplitBillNumber = ZString.Empty;
			}
			ABL_SplitBillInfo.RefreshBinding(oldValue);
		}
	}
	public ZPropertyInfo ABL_SplitBillInfo => GetZPropertyInfo(nameof(ABL_SplitBill));

	[ResourceStringData("AEAsycudaBill|ABL_SplitBillNumber", Caption = "Split Bill Number")]
	[MaxLength(34)]
	[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.SplitBills))]
	public ZString ABL_SplitBillNumber
	{
		get => this.GetSystemDefinedValue<ZString>(Schema.ABL_SplitBillNumber);
		set
		{
			var oldValue = ABL_SplitBillNumber;
			CheckMaximumLength(ABL_SplitBillNumberInfo, value);
			this.SetSystemDefinedValue(Schema.ABL_SplitBillNumber, value);
			ABL_SplitBillNumberInfo.RefreshBinding(oldValue);
			if (!IsValidationSuspended)
			{
				(Validation as AsycudaBillValidationForRegularBill)?.ValidateABL_SplitBillNumber();
			}
		}
	}

	public ZPropertyInfo ABL_SplitBillNumberInfo => GetZPropertyInfo(nameof(ABL_SplitBillNumber));

	[ResourceStringData("AEAsycudaBill|ABL_OA_Forwarder", Caption = "Freight Forwarder")]
	public override ZGuid ABL_OA_Forwarder { get => base.ABL_OA_Forwarder; set => base.ABL_OA_Forwarder = value; }

	[ResourceStringData("AEAsycudaBill|ForwarderMPCI", Caption = "FF MPCI ID")]
	public ZString ForwarderMPCI => Forwarder?.Header?.CustomsCodes.GetCustomsRegNo(OrgCusCode.UnitedArabEmiratesCodeTypes.MPCINumber, Core.Constants.CountryCodes.UnitedArabEmirates) ?? ZString.Empty;

	public ZPropertyInfo ForwarderMPCIInfo => GetZPropertyInfo(nameof(ForwarderMPCI));

	public override ZString ABL_BolType
	{
		get => base.ABL_BolType;
		set
		{
			var oldValue = ABL_BolType;
			base.ABL_BolType = value;
			if (!IsCopying && oldValue != ABL_BolType && ABL_BolType == Core.Constants.ShipmentTypes.CoLoadMaster && ABL_OA_Forwarder.IsEmpty)
			{
				ABL_OA_Forwarder = Header?.AMA_OA_ShippingAgent ?? ZGuid.Empty;
			}
		}
	}

	public new AsycudaBillLookups Lookups => (AsycudaBillLookups)base.Lookups;

	protected override ManifestBase.AsycudaBillLookups GetNewLookups() => new AsycudaBillLookups(this);

	protected override ManifestBase.AsycudaBillValidation GetNewValidationForRegularBill() => new AsycudaBillValidationForRegularBill(this);

	protected override ManifestBase.AsycudaBillValidation GetNewValidationForMasterChild() => new AsycudaBillValidationForMasterChild(this);

	public new ASYCUDA.Business.AsycudaPackCollection<AsycudaPack, AsycudaBill> Packs => (ASYCUDA.Business.AsycudaPackCollection<AsycudaPack, AsycudaBill>)base.Packs;

	public bool IsTSS => ABL_ShipmentType == ShipmentTypeList.Codes.Transhipment28;

	protected override ManifestBase.IAsycudaPackCollection<ManifestBase.AsycudaPack, ManifestBase.AsycudaBill> CreateNewAsycudaPackCollection()
	{
		return new ASYCUDA.Business.AsycudaPackCollection<AsycudaPack, AsycudaBill>(this);
	}

	protected override ZAddress GetNewABL_OA_DeliveryAgent_ZAddress()
	{
		var result = base.GetNewABL_OA_DeliveryAgent_ZAddress();
		result.DefaultAddressType = AddressType.DLV;
		return result;
	}

	protected override ZAddress GetNewABL_OA_ContainerAgent_ZAddress()
	{
		var result = base.GetNewABL_OA_ContainerAgent_ZAddress();
		result.DefaultAddressType = AddressType.DLV;
		return result;
	}

	protected override Type GetPackTypeCore() => typeof(AsycudaPack);

	protected override Type GetPackedItemTypeCore() => typeof(AsycudaPackedItem);

	public override void OnSaving()
	{
		base.OnSaving();
		PopulateABL_SenderReferenceIfRequired();
	}

	void PopulateABL_SenderReferenceIfRequired()
	{
		PopulateNumberPropertyIfRequired(ABL_SenderReferenceInfo, x => (ZString)new ReferenceNumberGenerator(Factory).GenerateDocumentReferenceNumber());
	}

	#region IMessageAttachee

	ZString IMessageAttachee.EntryStatus
	{
		get => ABL_BillStatus;
		set
		{
			ABL_BillStatus = value;
			if (Header is AsycudaManifestHeader manifestHeader)
			{
				manifestHeader.RegistrationStatus = manifestHeader.MessagingProvider?.GetMostSevereValueCustomsStatus(manifestHeader) ?? ZString.Empty;
				manifestHeader.RegistrationStatusInfo.RefreshBinding();
			}
		}
	}

	ZString IMessageAttachee.MessageStatus
	{
		get => ABL_MessageStatus;
		set
		{
			ABL_MessageStatus = value;
			if (Header is AsycudaManifestHeader manifestHeader)
			{
				manifestHeader.AMA_MessageStatus = manifestHeader.MessagingProvider?.GetMostSevereValueMessageStatus(manifestHeader) ?? ZString.Empty;
				manifestHeader.MessageStatusInfo.RefreshBinding();
			}
		}
	}

	ZString IMessageAttachee.DocumentIdentifier => ABL_SenderReference;

	IBusinessObjectCollection IMessageAttachee.Messages => Messages;

	#endregion
}

