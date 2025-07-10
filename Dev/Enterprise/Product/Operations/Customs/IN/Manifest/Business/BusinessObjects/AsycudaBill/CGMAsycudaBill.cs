using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.IN.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.IN.Manifest.Business;

public sealed class CGMAsycudaBill : AsycudaBill, IDataVersionLoggingSupported
{
	public CGMAsycudaBill(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new class Schema : ASYCUDA.Business.AsycudaBill.Schema
	{
		public const string BondNumber = nameof(CGMAsycudaBill.BondNumber);
		public const string MLOCode = nameof(CGMAsycudaBill.MLOCode);
		public const string CarrierCode = nameof(CGMAsycudaBill.CarrierCode);
		public const string MessageStatusDescription = nameof(CGMAsycudaBill.MessageStatusDescription);

		public const int BondNumberMaxLength = 10;
		public const int SubLineNumberMaxLength = 4;
		public const int ItemTypeMaxLength = 2;
		public const int ShipmentTypeMaxLength = 1;
		public const int AirPortCodeLength = 3;
		public const int BillNumberMaxLength = 20;
		public const int MarksAndNumbersMaxLength = 300;
		public const int CustomsFinalDestinationPortMaxLength = 10;
	}

	public new CGMAsycudaManifestHeader Header => (CGMAsycudaManifestHeader)base.Header;

	protected override ManifestBase.AsycudaBillValidation GetNewValidationForMasterChild() => new CGMAsycudaBillValidationForMasterChild(this);

	protected override ManifestBase.AsycudaBillValidation GetNewValidationForRegularBill() => new CGMAsycudaBillValidationForRegularBill(this);

	public new CGMAsycudaBillLookups Lookups => (CGMAsycudaBillLookups)base.Lookups;

	protected override ManifestBase.AsycudaBillLookups GetNewLookups() => new CGMAsycudaBillLookups(this);

	[ResourceStringData("Enterprise.Customs.IN.Manifest.Business.AsycudaBill|ABL_BillIssueDate|SEA", Caption = "HBL Date", MediumCaption = "HBL Dt.", ShortCaption = "Dt.", MultipleKey = AsycudaManifestHeader.CaptionKeySea)]
	[ResourceStringData("Enterprise.Customs.IN.Manifest.Business.AsycudaBill|ABL_BillIssueDate|AIR", Caption = "HAWB Date", MediumCaption = "HAWB Dt.", ShortCaption = "Dt.", MultipleKey = AsycudaManifestHeader.CaptionKeyAir)]
	public override ZDate ABL_BillIssueDate => base.ABL_BillIssueDate;

	[ResourceStringData("Enterprise.Customs.IN.Manifest.Business.AsycudaBill|ABL_SpecialCargoCode|SEA", Caption = "Item Type", MultipleKey = AsycudaManifestHeader.CaptionKeySea)]
	[ResourceStringData("Enterprise.Customs.IN.Manifest.Business.AsycudaBill|ABL_SpecialCargoCode|AIR", Caption = "Shipment Type", MediumCaption = "Shipment Type", ShortCaption = "Ship. Type", MultipleKey = AsycudaManifestHeader.CaptionKeyAir)]
	[List((nameof(Lookups) + "." + nameof(CGMAsycudaBillLookups.SpecialCargoCodeList)))]
	[MaxLength(nameof(ABL_SpecialCargoCodeMaxLength))]
	public override ZString ABL_SpecialCargoCode => base.ABL_SpecialCargoCode;

	int ABL_SpecialCargoCodeMaxLength => IsSea ? Schema.ItemTypeMaxLength : Schema.ShipmentTypeMaxLength;

	[ResourceStringData("41F3289A-43AF-4B7E-8313-8B8C20A984A6", Caption = "Cargo Movement", MediumCaption = "Cargo Mov.", ShortCaption = "Cargo Mov.")]
	[MaxLength(2)]
	public override ZString ABL_CargoStatus => base.ABL_CargoStatus;

	[ResourceStringData("1CD5830B-FB05-4BFF-898F-AB52B88FF95A", Caption = "Sub Line Number", MediumCaption = "Sub-Line No.", ShortCaption = "S-Line No.")]
	[MaxLength(Schema.SubLineNumberMaxLength)]
	public override ZString ABL_CarrierReference { get => base.ABL_CarrierReference; }

	[ResourceStringData("Enterprise.Customs.IN.Manifest.Business.AsycudaBill|ABL_RL_NKOrigin|AIR", Caption = "Port Of Origin", MediumCaption = "Origin Port", ShortCaption = "Origin", MultipleKey = AsycudaManifestHeader.CaptionKeyAir)]
	public override ZString ABL_RL_NKOrigin => base.ABL_RL_NKOrigin;

	[ResourceStringData("Enterprise.Customs.IN.Manifest.Business.AsycudaBill|ABL_ManifestQty|AIR", Caption = "Packages", MediumCaption = "Packages", ShortCaption = "Pack.", MultipleKey = AsycudaManifestHeader.CaptionKeyAir)]
	public override ZInt ABL_ManifestQty => base.ABL_ManifestQty;

	[ResourceStringData("Enterprise.Customs.IN.Manifest.Business.AsycudaBill|ABL_GrossWeight|AIR", Caption = "Gross Weight", MediumCaption = "Gross Wt.", ShortCaption = "Gross Wt.", MultipleKey = AsycudaManifestHeader.CaptionKeyAir)]
	public override ZDecimal ABL_GrossWeight => base.ABL_GrossWeight;

	public override ZString ABL_GrossWeightUQ
	{
		get => base.ABL_GrossWeightUQ;
		set
		{
			var oldValue = ABL_GrossWeightUQ;
			base.ABL_GrossWeightUQ = value;
			if (!IsCopying && oldValue != ABL_GrossWeightUQ)
			{
				Packs.Cast<CGMAsycudaPack>().ForEach(pack => pack.APA_WeightUQ = ABL_GrossWeightUQ);
			}
		}
	}

	[ResourceStringData("Enterprise.Customs.IN.Manifest.Business.AsycudaBill|ABL_Volume|AIR", Caption = "Volume", MediumCaption = "Volume", ShortCaption = "Vol.", MultipleKey = AsycudaManifestHeader.CaptionKeyAir)]
	public override ZDecimal ABL_Volume => base.ABL_Volume;

	[ResourceStringData("Enterprise.Customs.IN.Manifest.Business.AsycudaBill|ABL_BillNumber|SEA", Caption = "HBL Number", MediumCaption = "HBL Num.", ShortCaption = "HBL", MultipleKey = AsycudaManifestHeader.CaptionKeySea)]
	[ResourceStringData("Enterprise.Customs.IN.Manifest.Business.AsycudaBill|ABL_BillNumber|AIR", Caption = "HAWB Number", MediumCaption = "HAWB Num.", ShortCaption = "HAWB", MultipleKey = AsycudaManifestHeader.CaptionKeyAir)]
	[MaxLength(Schema.BillNumberMaxLength)]
	public override ZString ABL_BillNumber => base.ABL_BillNumber;

	public override bool ABL_BillNumber_ReadOnly => IsAir && !ABL_MessageStatus.IsEmpty;

	[ResourceStringData("F3F8D3BC-DB90-47C0-8FFD-3A8DC18C977B", Caption = "Importer", MediumCaption = "Importer", ShortCaption = "Imp.")]
	public override ZGuid ABL_OA_Buyer
	{
		get => base.ABL_OA_Buyer;
		set
		{
			var oldValue = base.ABL_OA_Buyer;
			base.ABL_OA_Buyer = value;
			if (!IsCopying && oldValue != ABL_OA_Buyer)
			{
				DefaultConsigneeIfEmpty();
			}
		}
	}

	[ResourceStringData("CE6443A7-A782-4031-BC00-5730B1E84496", Caption = "Importer Name")]
	public override ZString ABL_BuyerName { get => base.ABL_BuyerName; set => base.ABL_BuyerName = value; }

	[ResourceStringData("68A231D3-BA0A-46D7-83E8-2F031A1CD14F", Caption = "Importer Street 1")]
	public override ZString ABL_BuyerStreet1 { get => base.ABL_BuyerStreet1; set => base.ABL_BuyerStreet1 = value; }

	[ResourceStringData("A0A57EE7-938B-461F-9DC7-E0BBD4B199D3", Caption = "Importer Street 2")]
	public override ZString ABL_BuyerStreet2 { get => base.ABL_BuyerStreet2; set => base.ABL_BuyerStreet2 = value; }

	[ResourceStringData("7D1DE789-4655-4719-866E-0A9B043764BE", Caption = "Importer City")]
	public override ZString ABL_BuyerCity { get => base.ABL_BuyerCity; set => base.ABL_BuyerCity = value; }

	[ResourceStringData("81316DB8-EE9A-47C2-BDC1-C99432F77946", Caption = "Importer State")]
	public override ZString ABL_BuyerState { get => base.ABL_BuyerState; set => base.ABL_BuyerState = value; }

	[ResourceStringData("D0B09EBC-72C1-48DA-BF97-F7DB78946241", Caption = "Importer Postcode")]
	public override ZString ABL_BuyerPostcode { get => base.ABL_BuyerPostcode; set => base.ABL_BuyerPostcode = value; }

	[ResourceStringData("F540B3B1-E906-4D77-8E3A-43BC0774A020", Caption = "Importer Phone")]
	public override ZString ABL_BuyerPhone { get => base.ABL_BuyerPhone; set => base.ABL_BuyerPhone = value; }

	[ResourceStringData("44DEE9E3-BBF7-4192-B0EE-6A7076497288", Caption = "Importer Country / Region")]
	public override ZString ABL_RN_NKBuyerCountry { get => base.ABL_RN_NKBuyerCountry; set => base.ABL_RN_NKBuyerCountry = value; }

	[MaxLength(Schema.MarksAndNumbersMaxLength)]
	public override ZString ABL_MarksAndNumbers { get => base.ABL_MarksAndNumbers; set => base.ABL_MarksAndNumbers = value; }

	[ResourceStringData("40002480-005B-40D2-B137-AE8570979DCB", Caption = "Action", MediumCaption = "Action", ShortCaption = "Act.")]
	public override ZString ABL_BillStatus { get => base.ABL_BillStatus; set => base.ABL_BillStatus = value; }

	protected override bool ABL_BillStatus_ReadOnly => !IsChildMasterBill && Header != null
		&& (Header.ActionInDatabaseIsDelete	|| IsActionFixedToFresh || IsActionFixedToSupplementary);

	internal bool IsActionFixedToFresh => ABL_MessageStatus.IsEmpty && Header is AsycudaManifestHeader header
		&& header.MessageStatus.IsEmpty && header.RegistrationStatus.IsEmpty;

	internal bool IsActionFixedToSupplementary => ABL_MessageStatus.IsEmpty && Header is AsycudaManifestHeader header
		&& header.MessageStatus == MessageStatusList.Codes.MessageAccepted && header.RegistrationStatus == RegistrationStatusList.Codes.ManifestRegistered;

	public ZBool IsDeleteAction => ABL_BillStatus == BillActionList.Codes.Delete;

	[ResourceStringData("33813094-AA63-4B71-8A73-ED51636EE0F0", Caption = "Status")]
	[ReadOnly(true)]
	public ZString MessageStatusDescription => Lookups.MessageStatusList.GetDescriptionFromCode(ABL_MessageStatus);

	public ZPropertyInfo MessageStatusDescriptionInfo => GetZPropertyInfo(Schema.MessageStatusDescription);

	void DefaultConsigneeIfEmpty()
	{
		var buyerAddressPK = ABL_OA_Buyer;
		if (!buyerAddressPK.IsEmpty && ABL_OA_Consignee.IsEmpty)
		{
			ABL_OA_Consignee = buyerAddressPK;
		}
	}

	public ZString UnoCode => UNDGSubstance?.DG_UNNO ?? ZString.Empty;

	public ZString ImcoCode => UNDGSubstance?.DG_Class ?? ZString.Empty;

	[ResourceStringData("379CA102-819D-4CB9-940A-2162AFD48033", Caption = "Bond Number", MediumCaption = "Bond No.", ShortCaption = "Bond No.")]
	[MaxLength(Schema.BondNumberMaxLength)]
	public ZString BondNumber
	{
		get => this.GetSystemDefinedValue<ZString>(Schema.BondNumber);
		set
		{
			var oldValue = BondNumber;
			CheckMaximumLength(BondNumberInfo, value);
			this.SetSystemDefinedValue(Schema.BondNumber, value);
			if (!IsValidationSuspended && Validation is CGMAsycudaBillValidationForRegularBill validationForRegularBill)
			{
				validationForRegularBill.ValidateBondNumber();
			}
			BondNumberInfo.RefreshBinding(oldValue);
		}
	}

	public ZPropertyInfo BondNumberInfo => GetZPropertyInfo(Schema.BondNumber);

	public ZString PortOfOriginCode
	{
		get
		{
			if (IsAir)
			{
				var iataCode = Origin?.RL_IATA ?? ZString.Empty;
				return iataCode.IsEmpty ? ABL_RL_NKOrigin.Right(Schema.AirPortCodeLength) : iataCode;
			}
			return ABL_RL_NKOrigin;
		}
	}

	public ZString PortOfFinalDestinationCode
	{
		get
		{
			if (IsAir)
			{
				var iataCode = FinalDestination?.RL_IATA ?? ZString.Empty;
				return iataCode.IsEmpty ? ABL_RL_NKFinalDestination.Right(Schema.AirPortCodeLength) : iataCode;
			}
			return ABL_RL_NKFinalDestination;
		}
	}

	[ResourceStringData("5D29D3AE-E93A-43EE-993B-EFAC857C4525", Caption = "Nature Of Cargo", MediumCaption = "Nat. Of Cargo", ShortCaption = "N.O.C.")]
	[List((nameof(Lookups) + "." + nameof(CGMAsycudaBillLookups.NatureOfCargoList)))]
	public override ZString ABL_ContainerMode { get => base.ABL_ContainerMode; set => base.ABL_ContainerMode = value; }

	public bool IsContainerModeCorCP()
	{
		var containerMode = ABL_ContainerMode;
		return containerMode == NatureOfCargoList.Codes.C || containerMode == NatureOfCargoList.Codes.CP;
	}

	[ResourceStringData("D0F482B5-D2B0-4B79-842C-81CB22B0300A", Caption = "Bond Holder", MediumCaption = "Bond Holder", ShortCaption = "Holder")]
	public override ZGuid ABL_OH_BondHolder { get => base.ABL_OH_BondHolder; set => base.ABL_OH_BondHolder = value; }

	[ResourceStringData("DC09B9F0-A738-4DE1-A693-EFF371F49CB8", Caption = "MLO Code", MediumCaption = "MLO Code", ShortCaption = "MLO")]
	public ZString MLOCode => BondHolder?.CustomsCodes.GetCustomsRegNoMatchingCountryAndCodes(CountryCodes.India, OrgCusCode.CodeTypes.CarrierCode) ?? ZString.Empty;

	public ZPropertyInfo MLOCodeInfo => GetZPropertyInfo(Schema.MLOCode);

	[List((nameof(Lookups) + "." + nameof(CGMAsycudaBillLookups.DestinationCodeList)))]
	[ResourceStringData("3C34BA1B-20EC-4DE8-9151-9F899096BAAD", Caption = "Final Destination")]
	public override ZString ABL_LocationInformation
	{
		get => base.ABL_LocationInformation;
		set
		{
			base.ABL_LocationInformation = value;
			if (!IsValidationSuspended && Validation is CGMAsycudaBillValidationForRegularBill validationForRegularBill)
			{
				validationForRegularBill.ValidateABL_CustomsFinalDestinationPort();
			}
		}
	}

	public bool FinalDestinationIsCustomsHouse => ABL_LocationInformation == DestinationCodeList.Codes.CUS;

	public bool FinalDestinationIsCFS => ABL_LocationInformation == DestinationCodeList.Codes.CFS;

	[List((nameof(Lookups) + "." + nameof(CGMAsycudaBillLookups.CustomsFinalDestinationPortList)))]
	[MaxLength(Schema.CustomsFinalDestinationPortMaxLength)]
	public override ZString ABL_CustomsFinalDestinationPort { get => base.ABL_CustomsFinalDestinationPort; set => base.ABL_CustomsFinalDestinationPort = value; }

	[List(nameof(Lookups) + "." + nameof(CGMAsycudaBillLookups.InlandTransportModeList))]
	[ResourceStringData("67650EE0-1A15-4F42-802D-C573583F8DCA", Caption = "Mode Of Transport", MediumCaption = "M.O.T", ShortCaption = "M.O.T")]
	public override ZString ABL_InlandTransportMode { get => base.ABL_InlandTransportMode; set => base.ABL_InlandTransportMode = value; }

	[ResourceStringData("608064D6-B4A5-4307-91ED-00EE61C25D5A", Caption = "Carrier Code")]
	public ZString CarrierCode => IsTranshipment
									? LocalTransportCarrier?.CustomsCodes.GetCustomsRegNoMatchingCountryAndCodes(CountryCodes.India, OrgCusCode.CodeTypes.CarrierCode) ?? ZString.Empty
									: ZString.Empty;

	public ZPropertyInfo CarrierCodeInfo => GetZPropertyInfo(Schema.CarrierCode);

	public new IAsycudaPackCollection<CGMAsycudaPack, CGMAsycudaBill> Packs => (IAsycudaPackCollection<CGMAsycudaPack, CGMAsycudaBill>)base.Packs;

	protected override ManifestBase.IAsycudaPackCollection<ManifestBase.AsycudaPack, ManifestBase.AsycudaBill> CreateNewAsycudaPackCollection()
	{
		return new CGMAsycudaPackCollection(this);
	}

	protected override Type GetPackTypeCore() => typeof(CGMAsycudaPack);

	UNDGSubstance UNDGSubstance => Factory.GetValue(ref uNDGSubstance, () => Packs.Cast<CGMAsycudaPack>().FirstOrDefault()?.UNDGs.Cast<UNDGDataItem>().FirstOrDefault()?.UNDGSubstance);
	CachedProperty<UNDGSubstance> uNDGSubstance;

	public bool IsTranshipment => ABL_CargoStatus == CargoMovementList.Codes.TranshipmentCargo || ABL_CargoStatus == CargoMovementList.Codes.TranshipmentToICDSMTP;

	public ResourceStringData LocalTransportCarrierCaption => ABL_InlandTransportMode.ToString() switch
	{
		ModeOfTransportList.Codes.Train => Res.GetData("IN.Manifest.Business.CGMAsycudaBill|ABL_OH_LocalTransportCarrier|T", "Rail", "Rail Opt.", "Rail Operator"),
		ModeOfTransportList.Codes.Road => Res.GetData("IN.Manifest.Business.CGMAsycudaBill|ABL_OH_LocalTransportCarrier|R", "Transporter"),
		_ => Res.GetData("IN.Manifest.Business.CGMAsycudaBill|ABL_OH_LocalTransportCarrier", "Carrier"),
	};

	protected override ZBool ShouldSynchronisePaymentType() => ZBool.True;

	#region IDataVersionLoggingSupported
	
	bool IDataVersionLoggingSupported.IsDataVersionsAutoLogged => true;

	DataVersionLogValueFormatter IDataVersionLoggingSupported.DataVersionLogValueFormatter => this.GetDefaultDataVersionLogFormatter();

	#endregion

	public override void OnSaving()
	{
		if (IsChildMasterBill)
		{
			if (IsDeleteAction)
			{
				Header?.Bills.AsEnumerable().ForEach(x => x.ABL_BillStatus = BillActionList.Codes.Delete);
			}
		}
		else if (IsAir && this.AnyPropertyHasChanges(requireBillStatusChangeMembersForAir))
		{
			if (RequiresAmendment())
			{
				ABL_BillStatus = BillActionList.Codes.Amendment;
			}
			else if(RequiresSupplementary())
			{
				ABL_MessageStatus = string.Empty;
				ABL_BillStatus = BillActionList.Codes.Supplementary;
			}
		}
		base.OnSaving();
	}

	bool RequiresAmendment() => !ABL_MessageStatusInfo.HasChanges
		&& ABL_MessageStatus == BillMessageStatusList.Codes.Accepted
		&& (ABL_BillStatus == BillActionList.Codes.Fresh || ABL_BillStatus == BillActionList.Codes.Supplementary);

	bool RequiresSupplementary() => Header is CGMAsycudaManifestHeader header
		&& header.AMA_MessageStatus == MessageStatusList.Codes.MessageAccepted
		&& header.RegistrationStatus == RegistrationStatusList.Codes.ManifestRegistered
		&& ABL_MessageStatus == BillMessageStatusList.Codes.Error;

	readonly string[] requireBillStatusChangeMembersForAir = new[]
	{
		Schema.ABL_BillNumber,
		Schema.ABL_BillIssueDate,
		Schema.ABL_RL_NKOrigin,
		Schema.ABL_RL_NKFinalDestination,
		Schema.ABL_SpecialCargoCode,
		Schema.ABL_ManifestQty,
		Schema.ABL_ManifestUQ,
		Schema.ABL_GrossWeight,
		Schema.ABL_GrossWeightUQ,
		Schema.ABL_GoodsDescription
	};
}
