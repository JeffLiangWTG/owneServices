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
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.IN;
using Enterprise.Customs.Common.Shared;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IN.Business;

public class CusEntryInstruction : AutoINCusEntryInstruction
	, Integration.Customs.IN.ICusEntryInstruction
	, ISupportMultipleResourceStringData
	, ICusSupportingInfoWithSerialNoParent
	, IDateOfValuationProvider
{
	public CusEntryInstruction(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new class Schema : AutoINCusEntryInstruction.Schema
	{
		public const string LocalReferenceNumber = nameof(CusEntryInstruction.LocalReferenceNumber);
		public const string LocalReferenceNumberDate = nameof(CusEntryInstruction.LocalReferenceNumberDate);
		public const string RBIWaiverNumber = nameof(CusEntryInstruction.RBIWaiverNumber);
		public const string RBIWaiverDate = nameof(CusEntryInstruction.RBIWaiverDate);
		public const string ShippingBillNumberOverride = nameof(CusEntryInstruction.ShippingBillNumberOverride);
		public const string ShippingBillNumber = nameof(CusEntryInstruction.ShippingBillNumber);
		public const string ShippingBillDate = nameof(CusEntryInstruction.ShippingBillDate);
		public const string GrossWeight = nameof(CusEntryInstruction.GrossWeight);
		public const string NetWeight = nameof(CusEntryInstruction.NetWeight);
		public const string NumberOfPackagesUQ = nameof(CusEntryInstruction.NumberOfPackagesUQ);
		public const string LoosePackagesUQ = nameof(CusEntryInstruction.LoosePackagesUQ);
		public const string MessageStatus = nameof(CusEntryInstruction.MessageStatus);
		public const string CustomsStatus = nameof(CusEntryInstruction.CustomsStatus);
		public const string StatusOverride = nameof(CusEntryInstruction.StatusOverride);

		public const int RBIWaiverNumberMaxLength = 20;
		public const int SBNumberMaxLength = 7;
	}

	public new CusEntryHeader EntryHeader => (CusEntryHeader)base.EntryHeader;

	[ResourceStringData("Enterprise.Customs.IN.Business.CusEntryInstruction|LocalReferenceNumber", Caption = "Local Reference Number", MediumCaption = "LRN", ShortCaption = "LRN")]
	public ZString LocalReferenceNumber => EntryHeader?.CH_BGMReference ?? ZString.Empty;

	public ZPropertyInfo LocalReferenceNumberInfo => GetZPropertyInfo(Schema.LocalReferenceNumber);

	[ResourceStringData("Enterprise.Customs.IN.Business.CusEntryInstruction|LocalReferenceNumberDate", Caption = "LRN Date", MediumCaption = "Date", ShortCaption = "Date")]
	public ZDateTime LocalReferenceNumberDate => EntryHeader?.CreateTime ?? ZDateTime.Empty;

	public ZPropertyInfo LocalReferenceNumberDateInfo => GetZPropertyInfo(Schema.LocalReferenceNumberDate);

	[MaxLength(Customs.Business.CusEntryHeader.Schema.CH_StatusMaxLength)]
	[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.MessageStatusList))]
	[ReadOnlyMember(nameof(MessageStatus_ReadOnly))]
	[ResourceStringData("Enterprise.Customs.IN.Business.CusEntryInstruction|MessageStatus", Caption = "Message Status", MediumCaption = "Message", ShortCaption = "Message")]
	public ZString MessageStatus
	{
		get
		{
			return EntryHeader?.CH_Status ?? ZString.Empty;
		}
		set
		{
			if (EntryHeader is CusEntryHeader entryHeader)
			{
				CheckMaximumLength(MessageStatusInfo, value);
				entryHeader.CH_Status = value;
				MessageStatusInfo.RefreshBinding();
			}
		}
	}

	public ZPropertyInfo MessageStatusInfo => EntryHeader == null ? GetZPropertyInfo(Schema.MessageStatus) : GetWrappedZPropertyInfo(Schema.MessageStatus, x => EntryHeader.CH_StatusInfo);

	public bool MessageStatus_ReadOnly => !StatusOverride;

	[MaxLength(Customs.Business.CusEntryHeader.Schema.CH_EntryStatusMaxLength)]
	[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.CustomsStatusList))]
	[ReadOnlyMember(nameof(CustomsStatus_ReadOnly))]
	[ResourceStringData("Enterprise.Customs.IN.Business.CusEntryInstruction|CustomsStatus", Caption = "Customs Status", MediumCaption = "Customs", ShortCaption = "Customs")]
	public ZString CustomsStatus
	{
		get
		{
			return EntryHeader?.CH_EntryStatus ?? ZString.Empty;
		}
		set
		{
			if (EntryHeader is CusEntryHeader entryHeader)
			{
				CheckMaximumLength(CustomsStatusInfo, value);
				entryHeader.CH_EntryStatus = value;
				CustomsStatusInfo.RefreshBinding();
			}
		}
	}

	public ZPropertyInfo CustomsStatusInfo => EntryHeader == null ? GetZPropertyInfo(Schema.CustomsStatus) : GetWrappedZPropertyInfo(Schema.CustomsStatus, x => EntryHeader.CH_EntryStatusInfo);

	public bool CustomsStatus_ReadOnly => !StatusOverride;

	[ReadOnlyMember(nameof(StatusOverride_ReadOnly))]
	[ResourceStringData("Enterprise.Customs.IN.Business.CusEntryInstruction|StatusOverride", Caption = "Override", FullDescription = "Override Message Status and Customs Status")]
	public ZBool StatusOverride
	{
		get => statusOverride;
		set
		{
			SetNonPersistentPropertyValue(StatusOverrideInfo, ref statusOverride, value);

			if (EntryHeader is CusEntryHeader entryHeader && !StatusOverride)
			{
				entryHeader.RollbackChangesOnStatus();
			}

			MessageStatusInfo.RefreshBinding();
			CustomsStatusInfo.RefreshBinding();
		}
	}
	ZBool statusOverride;

	public ZPropertyInfo StatusOverrideInfo => GetZPropertyInfo(Schema.StatusOverride);

	public bool StatusOverride_ReadOnly => EntryHeader is null;

	[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.PackageUQList))]
	public ZString NumberOfPackagesUQ => Core.Constants.PkgUnit.Package;

	public ZPropertyInfo NumberOfPackagesUQInfo => GetZPropertyInfo(Schema.NumberOfPackagesUQ);

	[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.PackageUQList))]
	public ZString LoosePackagesUQ => Core.Constants.PkgUnit.Package;

	public ZPropertyInfo LoosePackagesUQInfo => GetZPropertyInfo(Schema.LoosePackagesUQ);

	public new JobDeclaration JobDeclaration => base.JobDeclaration as JobDeclaration;

	public new CusEntryInstructionLookups Lookups => (CusEntryInstructionLookups)base.Lookups;

	protected override Customs.Business.CusEntryInstructionLookups GetNewLookups() => new CusEntryInstructionLookups(this);

	public new CusEntryInstructionValidation Validation => (CusEntryInstructionValidation)base.Validation;

	protected override Customs.Business.CusEntryInstructionValidation GetNewValidation() => new CusEntryInstructionValidation(this);

	protected override ICusContainerOnEntryInstructionCollection<Customs.Business.CusContainerOnEntryInstruction> GetNewCusContainerOnEntryInstructionCollection()
	{
		return new CusContainerOnEntryInstructionCollection<CusContainerOnEntryInstruction>(this);
	}

	public IReadOnlyList<CusContainerOnEntryInstruction> Containers => ContainersForInstructionForBindingOnly.Cast<CusContainerOnEntryInstruction>().Where(x => x.IsForEntry).OrderBy(x => x.Container?.CO_ContainerNumber ?? ZString.Empty).ToList();

	public bool IsExport => JobDeclaration?.IsExport ?? false;

	IReadOnlyList<string> ISupportMultipleResourceStringData.MultipleKeysToUse => new string[] { JobDeclaration?.JE_MessageType ?? ZString.Empty };

	[ResourceStringData("Enterprise.Customs.IN.Business.CusEntryInstruction|CEI_NumberOfPackages", Caption = "Total No. Of Packages", MediumCaption = "Total Pack", ShortCaption = "Tot. Pk.", MultipleKey = SharedJobMessageTypeList.Codes.Export)]
	public override ZInt CEI_NumberOfPackages { get => base.CEI_NumberOfPackages; set => base.CEI_NumberOfPackages = value; }

	[ResourceStringData("Enterprise.Customs.IN.Business.CusEntryInstruction|CEI_LoosePackages", Caption = "Loose Packages", MediumCaption = "Loose Pack", ShortCaption = "Loose Pack", MultipleKey = SharedJobMessageTypeList.Codes.Export)]
	public override ZInt CEI_LoosePackages { get => base.CEI_LoosePackages; set => base.CEI_LoosePackages = value; }

	[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.WeightUQList))]
	[ResourceStringData("Enterprise.Customs.IN.Business.CusEntryInstruction|CEI_WeightUQ", Caption = "UOM", MultipleKey = SharedJobMessageTypeList.Codes.Export)]
	public override ZString CEI_WeightUQ { get => base.CEI_WeightUQ; set => base.CEI_WeightUQ = value; }

	[MaxLength(2)]
	[ResourceStringData("Enterprise.Customs.IN.Business.CusEntryInstruction|CEI_TotalContainer", Caption = "Total Container", MediumCaption = "Total Cont.", ShortCaption = "Tot. Cont.", MultipleKey = SharedJobMessageTypeList.Codes.Export)]
	public override ZInt CEI_TotalContainer { get => base.CEI_TotalContainer; set => base.CEI_TotalContainer = value; }

	[ResourceStringData("Enterprise.Customs.IN.Business.CusEntryInstruction|GrossWeight", Caption = "Gross Weight", MediumCaption = "Gr. Weight", ShortCaption = "Gr. Wt.", MultipleKey = SharedJobMessageTypeList.Codes.Export)]
	public ZDecimal GrossWeight => Factory.GetValue(ref grossWeightCached, () => InvoiceLines.Sum(item => Core.Constants.Weight.ConvertSafe(item.JI_Weight, item.JI_WeightUQ, CEI_WeightUQ, applyDefaultRounding: false)));
	CachedProperty<ZDecimal> grossWeightCached;

	public ZPropertyInfo GrossWeightInfo => GetZPropertyInfo(Schema.GrossWeight);

	[ResourceStringData("Enterprise.Customs.IN.Business.CusEntryInstruction|NetWeight", Caption = "Net Weight", MediumCaption = "Net Weight", ShortCaption = "Net. Wt.", MultipleKey = SharedJobMessageTypeList.Codes.Export)]
	public ZDecimal NetWeight => Factory.GetValue(ref netWeightCached, () => InvoiceLines.Sum(item => Core.Constants.Weight.ConvertSafe(item.JI_NetWeight, item.JI_NetWeightUQ, CEI_WeightUQ, applyDefaultRounding: false)));
	CachedProperty<ZDecimal> netWeightCached;

	public ZPropertyInfo NetWeightInfo => GetZPropertyInfo(Schema.NetWeight);

	[MaxLength(4)]
	[ResourceStringData("Enterprise.Customs.IN.Business.CusEntryInstruction|CEI_Style", Caption = "Declaration Type", MediumCaption = "Dec. Type", ShortCaption = "Dec. Type", MultipleKey = SharedJobMessageTypeList.Codes.Export)]
	public override ZString CEI_Style
	{
		get => base.CEI_Style;
		set
		{
			var oldValue = CEI_Style;
			base.CEI_Style = value;
			if (!IsCopying && oldValue != CEI_Style)
			{
				ClearSubStyleIfNotRequired();
			}
		}
	}

	[MaxLength(2)]
	[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.EntrySubStyleList))]
	[ResourceStringData("Enterprise.Customs.IN.Business.CusEntryInstruction|CEI_SubStyle", Caption = "NFEI Category", MediumCaption = "NFEI", ShortCaption = "NFEI", MultipleKey = SharedJobMessageTypeList.Codes.Export)]
	[ReadOnlyMember(nameof(CEI_SubStyle_ReadOnly))]
	public override ZString CEI_SubStyle { get => base.CEI_SubStyle; set => base.CEI_SubStyle = value; }

	public ZBool CEI_SubStyle_ReadOnly => IsExport && !IsNFEICategoryRequired;

	void ClearSubStyleIfNotRequired()
	{
		if (CEI_SubStyle_ReadOnly)
		{
			CEI_SubStyle = ZString.Empty;
		}
	}

	ZBool IsNFEICategoryRequired => CEI_Style == DeclarationTypeList.Codes.NoForeignExchangeInvolved;

	[MaxLength(Schema.RBIWaiverNumberMaxLength)]
	[ResourceStringData("Enterprise.Customs.IN.Business.CusEntryInstruction|RBIWaiverNumber", Caption = "RBI Waiver No.", MediumCaption = "Waiver No.", ShortCaption = "W. No.")]
	public ZString RBIWaiverNumber
	{
		get
		{
			return RBIWaiverEntryNumber?.CE_EntryNum ?? ZString.Empty;
		}
		set
		{
			CheckMaximumLength(RBIWaiverNumberInfo, value);
			LoadOrCreateRBIWaiverEntryNumber().CE_EntryNum = value;
			RBIWaiverNumberInfo.RefreshBinding();
			if (!IsValidationSuspended)
			{
				Validation.ValidateRBIWaiverNumber();
			}
		}
	}

	public ZPropertyInfo RBIWaiverNumberInfo => GetZPropertyInfo(Schema.RBIWaiverNumber);

	[ResourceStringData("Enterprise.Customs.IN.Business.CusEntryInstruction|RBIWaiverDate", Caption = "RBI Waiver Date", MediumCaption = "Waiver Date", ShortCaption = "Date")]
	public ZDate RBIWaiverDate
	{
		get
		{
			return RBIWaiverEntryNumber?.CE_IssueDate.Date ?? ZDate.Empty;
		}
		set
		{
			LoadOrCreateRBIWaiverEntryNumber().CE_IssueDate = value;
			RBIWaiverDateInfo.RefreshBinding();
			if (!IsValidationSuspended)
			{
				Validation.ValidateRBIWaiverDate();
			}
		}
	}

	public ZPropertyInfo RBIWaiverDateInfo => GetZPropertyInfo(Schema.RBIWaiverDate);

	CusEntryNumber RBIWaiverEntryNumber
	{
		get
		{
			if (fRBIWaiverEntryNumber == null || fRBIWaiverEntryNumber.IsDeleted)
			{
				fRBIWaiverEntryNumber = CusEntryNumber.Load(this, CusEntryNumberTypes.Indian.ReservedBankOfIndia, Core.Constants.CountryCodes.India);
				RegisterEditableChildObject(fRBIWaiverEntryNumber);
			}
			return fRBIWaiverEntryNumber;
		}
	}
	CusEntryNumber fRBIWaiverEntryNumber;

	CusEntryNumber LoadOrCreateRBIWaiverEntryNumber()
	{
		if (RBIWaiverEntryNumber == null)
		{
			fRBIWaiverEntryNumber = CusEntryNumber.LoadOrCreate(this, CusEntryNumberTypes.Indian.ReservedBankOfIndia, Core.Constants.CountryCodes.India);
			RegisterEditableChildObject(fRBIWaiverEntryNumber);
		}
		return fRBIWaiverEntryNumber;
	}

	#region Shipping Bill

	[MaxLength(Schema.SBNumberMaxLength)]
	[ReadOnlyMember(nameof(ShippingBillNumber_ReadOnly))]
	[ResourceStringData("Enterprise.Customs.IN.Business.CusEntryInstruction|ShippingBillNumber", Caption = "Shipping Bill No.", MediumCaption = "SB No.", ShortCaption = "SB No.")]
	public ZString ShippingBillNumber
	{
		get => ShippingBillEntryNumber?.CE_EntryNum ?? ZString.Empty;
		set
		{
			var oldValue = ShippingBillNumber;
			CheckMaximumLength(ShippingBillNumberInfo, value);
			LoadOrCreateShippingBillEntryNumber().CE_EntryNum = value;
			ShippingBillNumberInfo.RefreshBinding(oldValue);
			if (!IsValidationSuspended)
			{
				Validation.ValidateShippingBillNumber();
			}
		}
	}

	public ZPropertyInfo ShippingBillNumberInfo => GetZPropertyInfo(Schema.ShippingBillNumber);

	[ReadOnlyMember(nameof(ShippingBillDate_ReadOnly))]
	[ResourceStringData("Enterprise.Customs.IN.Business.CusEntryInstruction|ShippingBillDate", Caption = "Shipping Bill Date", MediumCaption = "SB Date", ShortCaption = "SB Dt.")]
	public ZDateTime ShippingBillDate
	{
		get => ShippingBillEntryNumber?.CE_IssueDate ?? ZDateTime.Empty;
		set
		{
			var oldValue = ShippingBillDate;
			LoadOrCreateShippingBillEntryNumber().CE_IssueDate = value;
			ShippingBillDateInfo.RefreshBinding(oldValue);
			if (!IsValidationSuspended)
			{
				Validation.ValidateShippingBillDate();
			}
		}
	}

	public ZPropertyInfo ShippingBillDateInfo => GetZPropertyInfo(Schema.ShippingBillDate);

	[ResourceStringData("Enterprise.Customs.IN.Business.CusEntryInstruction|ShippingBillNumberOverride", Caption = "Override", FullDescription = "Override Shipping Bill")]
	public ZBool ShippingBillNumberOverride
	{
		get => shippingBillNumberOverride;
		set
		{
			var oldValue = ShippingBillNumberOverride;
			SetNonPersistentPropertyValue(ShippingBillNumberOverrideInfo, ref shippingBillNumberOverride, value);
			ShippingBillNumberOverrideInfo.RefreshBinding(oldValue);

			var entryNumber = ShippingBillEntryNumber;
			if (!ShippingBillNumberOverride && entryNumber != null && !IsCopying)
			{
				var entryNumberIsInDatabase = entryNumber.IsInDatabase;
				ShippingBillNumber = entryNumberIsInDatabase ? (ZString)entryNumber.CE_EntryNumInfo.OriginalValue : ZString.Empty;
				ShippingBillDate = entryNumberIsInDatabase ? (ZDateTime)entryNumber.CE_IssueDateInfo.OriginalValue : ZDateTime.Empty;
			}
		}
	}
	ZBool shippingBillNumberOverride;

	public ZPropertyInfo ShippingBillNumberOverrideInfo => GetZPropertyInfo(Schema.ShippingBillNumberOverride);

	bool ShippingBillNumber_ReadOnly => !ShippingBillNumberOverride;

	bool ShippingBillDate_ReadOnly => !ShippingBillNumberOverride;

	CusEntryNumber ShippingBillEntryNumber
	{
		get
		{
			if (shippingBillEntryNumber == null || shippingBillEntryNumber.IsDeleted)
			{
				shippingBillEntryNumber = CusEntryNumber.Load(this, CusEntryNumberTypes.Indian.ShippingBill, Core.Constants.CountryCodes.India);
				RegisterEditableChildObject(shippingBillEntryNumber);
			}
			return shippingBillEntryNumber;
		}
	}

	CusEntryNumber shippingBillEntryNumber;

	CusEntryNumber LoadOrCreateShippingBillEntryNumber()
	{
		if (ShippingBillEntryNumber == null)
		{
			shippingBillEntryNumber = CusEntryNumber.LoadOrCreate(this, CusEntryNumberTypes.Indian.ShippingBill, Core.Constants.CountryCodes.India);
			RegisterEditableChildObject(shippingBillEntryNumber);
		}
		return shippingBillEntryNumber;
	}
	#endregion

	protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new CusEntryInstructionFetchStrategy(this);

	[UniversalCopyCollectionEntity(CusSupportingInfoSchema.Constants.TableName, CusSupportingInfoSchema.Constants.CSI_ParentTableCode)]
	[ChildEditable(true)]
	public SupportingDocumentCollection SupportingDocuments
	{
		get
		{
			if (fSupportingDocument == null)
			{
				fSupportingDocument = new SupportingDocumentCollection(this);
				fSupportingDocument.Load();
				RegisterEditableChildObject(fSupportingDocument);
			}
			return fSupportingDocument;
		}
	}

	SupportingDocumentCollection fSupportingDocument;

	public HugeSequenceNumberGenerator SupportingDocumentLineNumberGenerator => fSupportingDocumentLineNumberGenerator ??= new HugeSequenceNumberGenerator(() => SupportingDocuments);
	HugeSequenceNumberGenerator fSupportingDocumentLineNumberGenerator;

	[UniversalCopyCollectionEntity(CusSupportingInfoSchema.Constants.TableName, CusSupportingInfoSchema.Constants.CSI_ParentTableCode)]
	[ChildEditable(true)]
	public SWControlCollection SWControls
	{
		get
		{
			if (fSWControls == null)
			{
				fSWControls = new SWControlCollection(this);
				fSWControls.Load();
				RegisterEditableChildObject(fSWControls);
			}
			return fSWControls;
		}
	}

	SWControlCollection fSWControls;

	public HugeSequenceNumberGenerator SWControlsLineNumberGenerator => fSWControlsLineNumberGenerator ??= new HugeSequenceNumberGenerator(() => SWControls);
	HugeSequenceNumberGenerator fSWControlsLineNumberGenerator;

	public override void Delete()
	{
		using (this.GetLineNumberSuspenders())
		{
			if (!IsDeleted)
			{
				this.DeleteChildren<CusEntryNumber>(CusEntryNumSchema.CE_ParentID);
			}
			base.Delete();
		}
	}

	#region ICusSupportingInfoTypeSupporter

	IDictionary<ZString, Type> Integration.Customs.ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes() => new Dictionary<ZString, Type>
	{
		{ CusSupportingInfoTypeList.Codes.SupportingDocument, typeof(SupportingDocument) },
		{ CusSupportingInfoTypeList.Codes.SingleWindowControl, typeof(SWControl) }
	};

	IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
	{
		yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
	}

	#endregion

	ZDateTime IDateOfValuationProvider.DateOfValuation => JobDeclaration?.DateOfValuation ?? ZDateTime.Today;

	protected override void OnFactorySaving()
	{
		if (RBIWaiverEntryNumber != null && RBIWaiverEntryNumber.HasChanges && RBIWaiverNumber.IsEmpty && RBIWaiverDate.IsEmpty)
		{
			RBIWaiverEntryNumber.Delete();
		}
		if (ShippingBillEntryNumber != null && ShippingBillEntryNumber.HasChanges && ShippingBillNumber.IsEmpty && ShippingBillDate.IsEmpty)
		{
			ShippingBillEntryNumber.Delete();
		}
		base.OnFactorySaving();
	}

	protected override void OnFactorySaved(bool saveSucceeded)
	{
		if (saveSucceeded)
		{
			if (ShippingBillNumberOverride)
			{
				shippingBillNumberOverride = false;
			}

			if (StatusOverride)
			{
				StatusOverride = false;
			}
		}
		base.OnFactorySaved(saveSucceeded);
	}

	HugeSequenceNumberGenerator ICusSupportingInfoWithSerialNoParent.GetSequenceNumberGenerator(string type)
	{
		return type switch
		{
			CusSupportingInfoTypeList.Codes.SupportingDocument => SupportingDocumentLineNumberGenerator,
			CusSupportingInfoTypeList.Codes.SingleWindowControl => SWControlsLineNumberGenerator,
			_ => null
		};
	}
}
