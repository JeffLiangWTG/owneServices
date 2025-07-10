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
using Enterprise.Customs.IN.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IN.Manifest.Business;

public sealed class CGMAsycudaManifestHeader : AsycudaManifestHeader
	, Integration.Customs.ASYCUDA.INManifest.ICGMAsycudaManifestHeader
	, ICusEntryNumberParent
{
	public CGMAsycudaManifestHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new class Schema : ASYCUDA.Business.AsycudaManifestHeader.Schema
	{
		public const string ImportGeneralManifestNumber = "ImportGeneralManifestNumber";
		public const string ImportGeneralManifestDate = "ImportGeneralManifestDate";
		public const string GrossWeight = "GrossWeight";
		public const string GrossWeightUQ = "GrossWeightUQ";
		public const string ManifestQty = "ManifestQty";
		public const string ManifestUQ = "ManifestUQ";
		public const string StatusOverride = "StatusOverride";
		public const string Action = "Action";

		public const int ImportGeneralManifestNumberMaxLength = 7;
		public const int LineNumberMaxLength = 4;
		public const int MasterBillMaxLength = 20;
		public const int GrossWeightPrecision = 9;
		public const int GrossWeightDecimal = 3;
		public const int ActionMaxLength = 1;
	}

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		AMA_ManifestType = INManifestTypes.Codes.CGM;
		AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
		AMA_TransportMode = Lookups.TransportModeList.DefaultCode;
		if (IsAir)
		{
			MasterBill.ABL_SpecialCargoCode = ShipmentTypeList.Codes.Total;
		}
	}

	[List(nameof(Lookups) + "." + nameof(CGMAsycudaManifestHeaderLookups.ManifestUQList))]
	public ZString ManifestUQ
	{
		get { return MasterBill.ABL_ManifestUQ; }
		set
		{
			var oldValue = ManifestUQ;
			MasterBill.ABL_ManifestUQ = value;
			ManifestUQInfo.RefreshBinding(oldValue);
		}
	}

	public ZPropertyInfo ManifestUQInfo => GetWrappedZPropertyInfo(Schema.ManifestUQ, x => MasterBill.ABL_ManifestUQInfo);

	[ResourceStringData("Enterprise.Customs.IN.Manifest.Business.AsycudaManifestHeader|ManifestQty", Caption = "Total Packages", MediumCaption = "Packages", ShortCaption = "Packs.", MultipleKey = CaptionKeyAir)]
	public ZInt ManifestQty
	{
		get { return MasterBill.ABL_ManifestQty; }
		set
		{
			var oldValue = ManifestQty;
			MasterBill.ABL_ManifestQty = value;
			ManifestQtyInfo.RefreshBinding(oldValue);
		}
	}

	public ZPropertyInfo ManifestQtyInfo => GetWrappedZPropertyInfo(Schema.ManifestQty, x => MasterBill.ABL_ManifestQtyInfo);

	[List(nameof(Lookups) + "." + nameof(CGMAsycudaManifestHeaderLookups.WeightUQList))]
	public ZString GrossWeightUQ
	{
		get { return MasterBill.ABL_GrossWeightUQ; }
		set
		{
			var oldValue = GrossWeightUQ;
			MasterBill.ABL_GrossWeightUQ = value;
			GrossWeightUQInfo.RefreshBinding(oldValue);
		}
	}

	public ZPropertyInfo GrossWeightUQInfo => GetWrappedZPropertyInfo(Schema.GrossWeightUQ, x => MasterBill.ABL_GrossWeightUQInfo);

	[MeasureUnit(Schema.GrossWeightUQ, MeasureUnitType.Weight)]
	[ResourceStringData("Enterprise.Customs.IN.Manifest.Business.AsycudaManifestHeader|GrossWeight", Caption = "Total Gross Weight", MediumCaption = "Gross Weight", ShortCaption = "Gr. Wt.", MultipleKey = CaptionKeyAir)]
	public ZDecimal GrossWeight
	{
		get { return MasterBill.ABL_GrossWeight; }
		set
		{
			var oldValue = GrossWeight;
			MasterBill.ABL_GrossWeight = value;
			GrossWeightInfo.RefreshBinding(oldValue);
		}
	}

	public ZPropertyInfo GrossWeightInfo => GetWrappedZPropertyInfo(Schema.GrossWeight, x => MasterBill.ABL_GrossWeightInfo);

	[MaxLength(Schema.ActionMaxLength)]
	[ReadOnlyMember(nameof(Action_ReadOnly))]
	[List(nameof(Lookups) + "." + nameof(CGMAsycudaManifestHeaderLookups.ActionList))]
	[ResourceStringData("EEDC997C-54A2-4205-85BA-38418400CF10", Caption = "Action", MediumCaption = "Action", ShortCaption = "Act.")]
	public ZString Action
	{
		get => MasterBill.ABL_BillStatus;
		set
		{
			var oldValue = Action;
			MasterBill.ABL_BillStatus = value;
			if (!IsCopying && oldValue != Action)
			{
				if (Globals.IsUserInteractive && IsDeleteAction)
				{
					Globals.Message.ShowInformation(Res.GetString("69A65117-A9FD-4A26-966F-0363E4EDAD6C", "Action = D - Delete at Header level will also set all house bills Action to D - Delete."));
				}
			}
			ActionInfo.RefreshBinding(oldValue);
		}
	}

	public ZPropertyInfo ActionInfo => GetWrappedZPropertyInfo(Schema.Action, x => MasterBill.ABL_BillStatusInfo);

	internal bool Action_ReadOnly => (AMA_MessageStatus.IsEmpty && RegistrationStatus.IsEmpty) || ActionInDatabaseIsDelete;

	bool IsDeleteAction => MasterBill.IsDeleteAction;

	internal bool ActionInDatabaseIsDelete => IsInDatabase && (ZString)ActionInfo.OriginalValue == ManifestMessageTypeList.Codes.Delete;

	[MaxLength(Schema.ImportGeneralManifestNumberMaxLength)]
	[ResourceStringData("F7A8C158-7CBB-4A1A-96ED-C671A2BF6E1E", Caption = "IGM Number", MediumCaption = "IGM No.", ShortCaption = "IGM No.")]
	public ZString ImportGeneralManifestNumber
	{
		get
		{
			return ImportGeneralManifestEntryNumber?.CE_EntryNum ?? ZString.Empty;
		}
		set
		{
			CheckMaximumLength(ImportGeneralManifestNumberInfo, value);
			LoadOrCreateIGMEntryNumber().CE_EntryNum = value;
			ImportGeneralManifestNumberInfo.RefreshBinding();

			if (!IsValidationSuspended)
			{
				Validation.ValidateImportGeneralManifestNumber();
			}
		}
	}

	public ZPropertyInfo ImportGeneralManifestNumberInfo => GetZPropertyInfo(Schema.ImportGeneralManifestNumber);

	[ResourceStringData("9D5D911D-CC67-4E37-9833-38AC8DBDF766", Caption = "IGM Date", MediumCaption = "IGM Date", ShortCaption = "IGM Dt.")]
	public ZDate ImportGeneralManifestDate
	{
		get
		{
			return ImportGeneralManifestEntryNumber?.CE_IssueDate.Date ?? ZDate.Empty;
		}
		set
		{
			LoadOrCreateIGMEntryNumber().CE_IssueDate = value;
			ImportGeneralManifestDateInfo.RefreshBinding();
		}
	}

	public ZPropertyInfo ImportGeneralManifestDateInfo => GetZPropertyInfo(Schema.ImportGeneralManifestDate);

	[ResourceStringData("Enterprise.Customs.IN.Manifest.Business.AsycudaManifestHeader|StatusOverride", Caption = "Override", FullDescription = "Override Message Status and Customs Status")]
	public ZBool StatusOverride
	{
		get => statusOverride;
		set
		{
			SetNonPersistentPropertyValue(StatusOverrideInfo, ref statusOverride, value);

			if (!StatusOverride)
			{
				RollbackChangesOnStatus();
			}
		}
	}
	ZBool statusOverride;

	public ZPropertyInfo StatusOverrideInfo => GetZPropertyInfo(Schema.StatusOverride);

	[ResourceStringData("33B3B6ED-9753-4FBA-977B-1BC42C42F60B", Caption = "Port Of Destination", MediumCaption = "Destination", ShortCaption = "Destination")]
	public override ZString AMA_RL_NKFinalDestination
	{
		get => base.AMA_RL_NKFinalDestination;
		set
		{
			var oldValue = AMA_RL_NKFinalDestination;
			base.AMA_RL_NKFinalDestination = value;
			if (!IsCopying && oldValue != AMA_RL_NKFinalDestination && IsAir)
			{
				PopulateAMA_CustomsOffice();
			}
		}
	}

	void PopulateAMA_CustomsOffice()
	{
		if (AMA_CustomsOffice.IsEmpty && !AMA_RL_NKFinalDestination.IsEmpty)
		{
			var code = $"{AMA_RL_NKFinalDestination}4";
			if (IsCustomsOfficeCodeValid(code))
			{
				AMA_CustomsOffice = code;
			}
		}
	}

	bool IsCustomsOfficeCodeValid(string code)
	{
		return Factory.GetCachedValue($"Enterprise.Customs.IN.Manifest.Business.CGMAsycudaManifestHeader|IsCustomsOfficeCodeValid|{code}", () =>
		{
			var filter = UniversalReferenceDataHelper.GetCustomsOfficeCollection(Factory, isAir: true).CompleteFilter;
			filter.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_Code, code);
			return Factory.ExistsInDatabase(ZZRefCusCodeListCombinedSchema.Constants.TableName, filter);
		});
	}

	[ResourceStringData("Enterprise.Customs.IN.Manifest.Business.AsycudaManifestHeader|AMA_RL_NKOrigin|SEA", Caption = "Port Of Shipment", MediumCaption = "Port Of Shp.", ShortCaption = "Port Of Shp.", MultipleKey = CaptionKeySea)]
	[ResourceStringData("Enterprise.Customs.IN.Manifest.Business.AsycudaManifestHeader|AMA_RL_NKOrigin|AIR", Caption = "Port of Origin", MediumCaption = "Origin", ShortCaption = "Origin", MultipleKey = CaptionKeyAir)]
	public override ZString AMA_RL_NKOrigin => base.AMA_RL_NKOrigin;

	[ResourceStringData("FF4CA2BB-72CD-4F27-9103-52E37F12DF7E", Caption = "Port Of Destination", MediumCaption = "Port Of Dest.", ShortCaption = "Port Of Dest.")]
	public override ZString AMA_CustomsDischargePort => base.AMA_CustomsDischargePort;

	[ResourceStringData("Enterprise.Customs.IN.Manifest.Business.AsycudaManifestHeader|AMA_ContainerMode|SEA", Caption = "Nature Of Cargo", MediumCaption = "Cargo", ShortCaption = "Cargo", MultipleKey = CaptionKeySea)]
	[ResourceStringData("Enterprise.Customs.IN.Manifest.Business.AsycudaManifestHeader|AMA_ContainerMode|AIR", Caption = "Container Mode", MultipleKey = CaptionKeyAir)]
	public override ZString AMA_ContainerMode => base.AMA_ContainerMode;

	[ResourceStringData("Enterprise.Customs.IN.Manifest.Business.AsycudaManifestHeader|AMA_MasterBillIssueDate|SEA", Caption = "BOL Date", MediumCaption = "BOL Dt.", ShortCaption = "BL Dt.", MultipleKey = CaptionKeySea)]
	[ResourceStringData("Enterprise.Customs.IN.Manifest.Business.AsycudaManifestHeader|AMA_MasterBillIssueDate|AIR", Caption = "MAWB Date", MediumCaption = "MAWB Dt.", ShortCaption = "MAWB Dt.", MultipleKey = CaptionKeyAir)]
	public override ZDate AMA_MasterBillIssueDate => base.AMA_MasterBillIssueDate;

	[ResourceStringData("E64E513F-91E5-454B-A844-E64368763844", Caption = "Line Number", MediumCaption = "Line No.", ShortCaption = "Line No.")]
	[MaxLength(Schema.LineNumberMaxLength)]
	public override ZString AMA_CarrierReference => base.AMA_CarrierReference;

	[ResourceStringData("7D49948E-A641-4DD9-B82C-BBE5CDBD5B58", Caption = "Cargo Description", MediumCaption = "Cargo Desc.", ShortCaption = "Desc.")]
	public override ZString AMA_GoodsDescription => base.AMA_GoodsDescription;

	[MaxLength(Schema.MasterBillMaxLength)]
	public override ZString AMA_MasterBill { get => base.AMA_MasterBill; set => base.AMA_MasterBill = value; }

	public override ZString AMA_TransportMode
	{
		get => base.AMA_TransportMode;
		set
		{
			var oldValue = AMA_TransportMode;
			base.AMA_TransportMode = value;
			if (!IsCopying && oldValue != AMA_TransportMode)
			{
				if (IsAir)
				{
					if (GrossWeightUQ.IsEmpty)
					{
						GrossWeightUQ = Core.Constants.Weight.Kilograms;
					}
					if (ManifestUQ.IsEmpty)
					{
						ManifestUQ = Core.Constants.PkgUnit.Package;
					}
					if (AMA_MessageStatus.IsEmpty && RegistrationStatus.IsEmpty)
					{
						Action = ManifestMessageTypeList.Codes.Fresh;
					}

					PopulateAMA_CustomsOffice();
				}
			}
		}
	}

	public override ZString AMA_MessageStatus
	{
		get => base.AMA_MessageStatus;
		set
		{
			var oldValue = AMA_MessageStatus;
			base.AMA_MessageStatus = value;
			if (!IsCopying && oldValue != AMA_MessageStatus)
			{
				Bills.MarkAsNeedingValidation();
			}
		}
	}

	protected override ZString CalculateStatusAfterSending(ZString messageType)
	{
		return messageType.ToString() switch
		{
			ManifestMessageTypeList.Codes.Amendment => RegistrationStatusList.Codes.ManifestIsUnderAmendment,
			ManifestMessageTypeList.Codes.Delete => RegistrationStatusList.Codes.ManifestDeleteRequisition,
			_ => string.Empty
		};
	}

	readonly string[] readOnlyMembersAfterAcceptedAtCustomsForSea = new[]
	{
		Schema.AMA_LloydsNumber,
		Schema.AMA_RadioCallSign,
		Schema.AMA_Voyage,
		Schema.AMA_CustomsOffice
	};

	readonly string[] readOnlyMembersAfterAcceptedAtCustomsForAir = new[]
	{
		Schema.AMA_MasterBill,
		Schema.AMA_CustomsOffice
	};

	readonly string[] readOnlyMembersAfterFirstMessageSentForAir = new[]
	{
		Schema.AMA_TransportMode,
		Schema.AMA_ManifestType,
		Schema.AMA_Nature
	};

	protected override bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
	{
		bool readOnlyAfterAcceptedAtCustoms() => !RegistrationStatus.IsEmpty
			&& (IsSea && readOnlyMembersAfterAcceptedAtCustomsForSea.Contains(property.Name)
			 || IsAir && readOnlyMembersAfterAcceptedAtCustomsForAir.Contains(property.Name));

		bool readOnlyAfterFirstMessageSent() => Messages.Count > 0
			&& (IsAir && readOnlyMembersAfterFirstMessageSentForAir.Contains(property.Name));

		return readOnlyAfterAcceptedAtCustoms() || readOnlyAfterFirstMessageSent() || base.GetShouldPropertiesBeReadOnly(property);
	}

	public new CGMAsycudaManifestHeaderValidation Validation => (CGMAsycudaManifestHeaderValidation)base.Validation;

	protected override AsycudaManifestHeaderValidation GetNewValidation() => new CGMAsycudaManifestHeaderValidation(this);

	protected override ASYCUDA.Business.ZZDatabaseValidationHelper GetNewZZValidationHelper() => new CGMZZDatabaseValidationHelper(this);

	public new CGMAsycudaBill MasterBill => (CGMAsycudaBill)base.MasterBill;

	public new CGMAsycudaBillCollection Bills => (CGMAsycudaBillCollection)base.Bills;

	protected override IAsycudaBillCollection<ManifestBase.AsycudaBill, ManifestBase.AsycudaManifestHeader> CreateNewAsycudaBillCollection() => new CGMAsycudaBillCollection(this);

	protected override Type GetBillTypeCore() => typeof(CGMAsycudaBill);

	public new CGMAsycudaManifestHeaderLookups Lookups => (CGMAsycudaManifestHeaderLookups)base.Lookups;

	protected override AsycudaManifestHeaderLookups GetNewLookups() => new CGMAsycudaManifestHeaderLookups(this);

	protected override Type GetContainerTypeCore() => typeof(CGMAsycudaContainer);

	public new CGMAsycudaContainerCollection Containers => (CGMAsycudaContainerCollection)base.Containers;

	protected override IAsycudaContainerCollection<AsycudaContainer, ManifestBase.AsycudaManifestHeader> CreateNewAsycudaContainerCollection() => new CGMAsycudaContainerCollection(this);

	protected override ASYCUDA.Business.AsycudaManifestHeaderDocWrapper GetDocWrapperCore()
	{
		if (IsAir)
		{
			return new CGMAirAsycudaManifestHeaderDocWrapper(this);
		}
		else if (IsSea)
		{
			return new CGMSeaAsycudaManifestHeaderDocWrapper(this);
		}
		else
		{
			return base.GetDocWrapperCore();
		}
	}

	protected override Customs.Business.BusinessObjectSynchroniser GetConsolSynchronizerCore(Freight.Forwarding.Business.ForwardingConsol source)
	{
		return new CGMAsycudaManifestHeaderSynchroniser(this, source);
	}

	protected override bool AMA_MessageStatus_ReadOnly => !StatusOverride;

	protected override bool RegistrationDetails_ReadOnly => !StatusOverride;

	public override void OnSaved(bool saveSucceeded)
	{
		base.OnSaved(saveSucceeded);
		if (saveSucceeded)
		{
			StatusOverride = false;
		}
	}

	#region ImportGeneralManifestEntryNumber

	CusEntryNumber ImportGeneralManifestEntryNumber
	{
		get
		{
			if (importGeneralManifestEntryNumber == null || importGeneralManifestEntryNumber.IsDeleted)
			{
				importGeneralManifestEntryNumber = CusEntryNumber.Load(this, CusEntryNumberTypes.Indian.ImportGeneralManifest, AMA_RN_NKCountry);
				RegisterEditableChildObject(importGeneralManifestEntryNumber);
			}
			return importGeneralManifestEntryNumber;
		}
	}
	CusEntryNumber importGeneralManifestEntryNumber;

	CusEntryNumber LoadOrCreateIGMEntryNumber()
	{
		var entryNumber = ImportGeneralManifestEntryNumber;
		if (entryNumber == null)
		{
			entryNumber = CusEntryNumber.LoadOrCreate(this, CusEntryNumberTypes.Indian.ImportGeneralManifest, AMA_RN_NKCountry);
			RegisterEditableChildObject(importGeneralManifestEntryNumber);
		}
		return entryNumber;
	}

	#endregion

	#region ICusEntryNumberParent

	string ICusEntryNumberParent.EntryNumberChangedCallStack => ZString.Empty;

	bool ICusEntryNumberParent.CanBeChangedOrDeleted(CusEntryNumber entryNumber, out string errMsg)
	{
		errMsg = string.Empty;
		return true;
	}

	void ICusEntryNumberParent.EntryNumberChanged(ZString oldValue, ZString newValue)
	{
	}

	#endregion

	public IReadOnlyList<CGMAsycudaPack> PacksWithContainer => Factory.GetCached(ref cachedPacksWithContainer, GetPacksWithContainer);
	CachedProperty<CGMAsycudaPack[]> cachedPacksWithContainer;

	CGMAsycudaPack[] GetPacksWithContainer()
	{
		return Bills.Cast<CGMAsycudaBill>().OrderBy(x => x.ABL_SequenceNumber).SelectMany(bill => bill.Packs.Cast<CGMAsycudaPack>().Where(pack => pack.Container is not null)).ToArray();
	}

	public override void OnSaving()
	{
		if (IsAir && !IsDeleteAction && IsInDatabase && RequiresAmendment())
		{
			Action = ManifestMessageTypeList.Codes.Amendment;
		}
		if (ImportGeneralManifestEntryNumber != null && ImportGeneralManifestNumber.IsEmpty && ImportGeneralManifestDate.IsEmpty)
		{
			ImportGeneralManifestEntryNumber.Delete();
		}
		if (RegistrationEntryNumber != null && !RegistrationEntryNumber.IsInDatabase && !RegistrationStatus.IsEmpty)
		{
			RegistrationEntryNumber.CE_EntryNum = AMA_MasterBill;
		}
		base.OnSaving();
	}

	readonly string[] requireAmendmentHeaderMembersAfterRegisteredForAir = new[]
	{
		Schema.AMA_Voyage,
		Schema.AMA_E_ARV,
		Schema.AMA_MasterBillIssueDate,
		Schema.AMA_RL_NKOrigin,
		Schema.AMA_RL_NKFinalDestination,
		Schema.AMA_GoodsDescription,
		Schema.ManifestQty,
		Schema.GrossWeight
	};

	readonly string[] requireAmendmentMasterBillMembersAfterRegisteredForAir = new[]
	{
		AsycudaBill.Schema.ABL_SpecialCargoCode
	};

	readonly string[] requireAmendmentEntryNumberMembersAfterRegisteredForAir = new[]
	{
		CusEntryNumber.Schema.CE_EntryNum,
		CusEntryNumber.Schema.CE_IssueDate
	};

	bool RequiresAmendment()
	{
		return (RegistrationStatus == RegistrationStatusList.Codes.ManifestRegistered || RegistrationStatus == RegistrationStatusList.Codes.ManifestIsUnderAmendment)
				&& (this.AnyPropertyHasChanges(requireAmendmentHeaderMembersAfterRegisteredForAir)
					|| MasterBill.AnyPropertyHasChanges(requireAmendmentMasterBillMembersAfterRegisteredForAir)
					|| ImportGeneralManifestEntryNumber.AnyPropertyHasChanges(requireAmendmentEntryNumberMembersAfterRegisteredForAir));
	}
}
