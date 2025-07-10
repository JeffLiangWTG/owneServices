using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	[SystemDefinedValues]
	public partial class JobDeclaration : BaseJobDeclaration, Integration.Customs.AsycudaCustoms.IJobDeclaration
	{
		public JobDeclaration(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoJobDeclaration.Schema
		{
			public new const int JE_CustomsOfficeMaxLength = 4;
			public new const int JE_ManifestNumberMaxLength = 28;
			public const int JE_DeclarationTypeMaxLength = 3;
			public const string JE_DeclarationType = "JE_DeclarationType";
		}

		public override ZGuid JE_GB
		{
			get { return base.JE_GB; }
			set
			{
				var oldValue = JE_GB;
				base.JE_GB = value;
				if (oldValue != JE_GB)
				{
					applicationBusinessProvider?.InvalidateCache();
				}
			}
		}

		[ResourceStringData("59399536-CEE1-44C5-AAB9-697E0261E5D1", Caption = "Duty Payer")]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.DutyPayers))]
		public override ZGuid JE_OH_DutyPayer { get => base.JE_OH_DutyPayer; set => base.JE_OH_DutyPayer = value; }

		[ResourceStringData("{e080d999-33f3-46f6-8e54-3d8a8483e4d3}", Caption = "Manifest Number")]
		[MaxLength(Schema.JE_ManifestNumberMaxLength)]
		public override ZString JE_ManifestNumber { get => base.JE_ManifestNumber; set => base.JE_ManifestNumber = value; }

		[ResourceStringData("{6AED9CC7-9A71-4F77-B160-2E6AE26A4A7D}", Caption = "Origin")]
		public override ZString JE_GoodsOrigin { get => base.JE_GoodsOrigin; set => base.JE_GoodsOrigin = value; }

		[ResourceStringData("EE7FA65C-8C69-4E7A-857A-B08D9B245F89", Caption = "Payment Method")]
		public override ZString JE_PaymentMethod { get => base.JE_PaymentMethod; set => base.JE_PaymentMethod = value; }

		[ResourceStringData("DEF030D9-683C-4D8B-8EE8-3ABE607E91DA", Caption = "Inland M.O.T")]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.TransportTypeList))]
		public override ZString JE_TransportModeInland { get => base.JE_TransportModeInland; set => base.JE_TransportModeInland = value; }

		[ResourceStringData("7A491E9B-8427-4A5F-99BD-3E5EDC997C34", Caption = "Representative")]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.RepresentativeList))]
		public override ZGuid JE_OA_Representative { get => base.JE_OA_Representative; set => base.JE_OA_Representative = value; }

		[ResourceStringData("6499724F-8276-449E-8250-572E465FA545", Caption = "Declarant")]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.DeclarantOfficeList))]
		public override ZGuid JE_OA_DeclarantAddress { get => base.JE_OA_DeclarantAddress; set => base.JE_OA_DeclarantAddress = value; }

		public bool IsRiskManagementEnabled => Extensions.IsRiskManagementEnabled(DefaultDataGroupingCore, Factory);

		public bool HasInwardInvoiceLine => Factory.GetValue(ref hasInwardInvoiceLineCached, () => InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.IsIntoWarehouseWarehousing));
		CachedProperty<bool> hasInwardInvoiceLineCached;

		public bool HasOutwardInvoiceLine => Factory.GetValue(ref hasOutwardInvoiceLineCached, () => InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.IsOutOfWarehouseWarehousing));
		CachedProperty<bool> hasOutwardInvoiceLineCached;

		protected override Customs.Business.MergeManager GetMergeManager() => new MergeManager(this);

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new JobDeclarationFetchStrategy(this);

		public BaseApplicationBusinessProvider ApplicationBusinessProvider => (applicationBusinessProvider ?? (applicationBusinessProvider = new RecalculableCachedValue<BaseApplicationBusinessProvider>(() => BaseApplicationBusinessProvider.GetApplicationBusinessProvider(Factory, CountryCode)))).Value;
		RecalculableCachedValue<BaseApplicationBusinessProvider> applicationBusinessProvider;

		protected override DocumentSupporter CreateNewDocumentSupporter() => new JobDeclarationDocumentSupporter(this);

		protected override DeclarationDocManagerInfo GetNewDocManagerInfo() => new Business.DeclarationDocManagerInfo(this);

		[ResourceStringData("C07030B1-C85C-40F7-8179-3060FFA2442F", Caption = "Payment Acct. No.")]
		[MaxLength(JobDeclaration.Schema.JE_DefermentAccountNumberMaxLength)]
		public override ZString JE_DefermentAccountNumber { get => base.JE_DefermentAccountNumber; set => base.JE_DefermentAccountNumber = value; }

		#region Implementation

		#region protected override

		public override ZGuid JE_OH_Supplier
		{
			get => base.JE_OH_Supplier;
			set
			{
				var oldValue = JE_OH_Supplier;
				base.JE_OH_Supplier = value;
				if (!IsCopying && oldValue != JE_OH_Supplier)
				{
					CustomsEntryHeaders.MarkAsNeedingValidation();
				}
			}
		}

		public override ZBool BondedWarehouseEditable => false;

		protected override bool IsCustomsHeaderAmendmentATotalReplacement => false;
		protected override bool IsCustomsLineAmendmentATotalReplacement => false;

		protected override bool HasSplitEntriesCore => false;

		public override bool IsInvoicesRequiredToBeInSameCurrency => true;

		public override bool IsInvoicesRequiredToBeInSameIncoTerm => true;

		public override ZBool IsImport => base.IsImport || IsExWarehouse;

		public override ZDateTime DateOfValuation
		{
			get
			{
				var valuationDate = JE_ValuationDate;
				return valuationDate.IsValid && !valuationDate.IsEmpty ? valuationDate : CachedTodaysDate;
			}
		}

		[ResourceStringData("D218AC55-B2B0-4768-A23D-2721672F6D4B", Caption = "Valuation Date")]
		public override ZDate JE_ValuationDate { get => base.JE_ValuationDate; set => base.JE_ValuationDate = value; }

		public override ZString JE_MessageType
		{
			get => base.JE_MessageType;
			set
			{
				var oldValue = base.JE_MessageType;
				base.JE_MessageType = value;
				if (!IsCopying && JE_MessageType != oldValue && !IsMessageTypeChangeProcessSuspended && !IsDataChangeSuspendedByFakeDeclaration)
				{
					RefreshIncotermAndChargeFactory();
					DefaultValuationDate();
				}
			}
		}

		public override bool IsMessageTypeChangeAnError => IsMergeDone;

		public override ZDateTime JE_DateOfArrival
		{
			get => base.JE_DateOfArrival;
			set
			{
				var oldValue = JE_DateOfArrival;
				base.JE_DateOfArrival = value;
				if (!IsCopying && JE_DateOfArrival != oldValue && !IsSettingDefaultValues)
				{
					DefaultValuationDate();
				}
			}
		}

		public override ZDateTime JE_ExportDate
		{
			get => base.JE_ExportDate;
			set
			{
				var oldValue = JE_ExportDate;
				base.JE_ExportDate = value;
				if (!IsCopying && JE_ExportDate != oldValue && !IsSettingDefaultValues)
				{
					DefaultValuationDate();
				}
			}
		}

		void DefaultValuationDate()
		{
			var config = Configuration;
			if (IsExport)
			{
				if (config?.ZZC_DefaultExportValuationDate.Equals(ValuationDateDefaultTypeList.Codes.DOE) ?? false)
				{
					JE_ValuationDate = JE_ExportDate.Date;
				}
			}
			else
			{
				var defaultImportValuationDate = config?.ZZC_DefaultImportValuationDate ?? ZString.Empty;
				if (defaultImportValuationDate.Equals(ValuationDateDefaultTypeList.Codes.DOE))
				{
					JE_ValuationDate = JE_ExportDate.Date;
				}
				else if (defaultImportValuationDate.Equals(ValuationDateDefaultTypeList.Codes.DOA))
				{
					JE_ValuationDate = JE_DateOfArrival.Date;
				}
			}
		}

		ZZRefCusConfiguration Configuration => (configuration ?? (configuration = new CachedValue<ZZRefCusConfiguration>(() => ZZRefCusConfiguration.Get(Company)))).Value;
		CachedValue<ZZRefCusConfiguration> configuration;

		#region Bonded Warehouse

		protected override bool IsAutoUpdateBondedWarehouseEnabledCore => true;

		protected override bool IsInwardBondedWarehousingEnabledCore => HasInwardInvoiceLine;

		protected override bool IsOutwardBondedWarehousingEnabledCore => HasOutwardInvoiceLine;

		protected override bool SupportsBondedWarehousingCore => false;

		protected override bool SupportMultipleWarehouseEntryCore => true;

		protected override bool ShouldUpdateOutwardLinesWithInventoryDetailsCore => IsInventorySelectionEnabled;

		protected override bool IsInventorySelectionEnabledCore => IsExWarehouse || IsExport;

		protected override DeclarationInventorySelectionHeader GetNewInventorySelectionHeader() => new InventorySelectionHeader(this);

		public bool IsBondedWarehouseAutomationOn => Factory.GetValue(ref isBondedWarehouseAutomationOn,
			() => ClientIsBondedWarehousing || CustomsEntryInstructions.Any<CusEntryInstruction>(c => c.WarehouseIsBondedWarehousing || c.Warehouse2IsBondedWarehousing));
		CachedProperty<bool> isBondedWarehouseAutomationOn;

		#endregion

		#endregion

		#region CusEntryInstruction

		protected override Customs.Business.EntryInstructionProvider GetCustomsEntryInstructionProviderCore() => new EntryInstructionProvider(this);

		public CodeDescriptionPairList DeclarationTypeList => CusEntryInstruction.Lookups.StyleList;

		[List(nameof(DeclarationTypeList))]
		[MaxLength(Schema.JE_DeclarationTypeMaxLength)]
		[ResourceStringData("Enterprise.Customs.AsycudaCustoms.Business|JE_DeclarationType", Caption = "Declaration Type", ShortCaption = "Dec Type")]
		public ZString JE_DeclarationType
		{
			get
			{
				return CusEntryInstruction?.CEI_Style ?? ZString.Empty;
			}
			set
			{
				var oldValue = JE_DeclarationType;
				CheckMaximumLength(JE_DeclarationTypeInfo, value);
				CusEntryInstruction.CEI_Style = value;
				if (!IsCopying && oldValue != JE_DeclarationType)
				{
					if (!IsValidationSuspended)
					{
						Validation.ValidateJE_DeclarationType();
					}
				}
				JE_DeclarationTypeInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo JE_DeclarationTypeInfo => GetZPropertyInfo(Schema.JE_DeclarationType);

		public CusEntryInstruction CusEntryInstruction => GetCusEntryInstruction();

		protected CusEntryInstruction GetCusEntryInstruction()
		{
			if (!IsDeleted && (cusEntryInstruction == null || cusEntryInstruction.IsDeleted))
			{
				cusEntryInstruction = CustomsEntryInstructionProvider.CustomsEntryInstructions.OfType<CusEntryInstruction>().OrderBy(x => x.PK).FirstOrDefault();
				if (cusEntryInstruction == null)
				{
					cusEntryInstruction = (CusEntryInstruction)CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				}
				if (cusEntryInstruction != null)
				{
					RegisterEditableChildObject(cusEntryInstruction);
					if (!IsPersistent)
					{
						cusEntryInstruction.MakeNonPersistent(); // Otherwise, the declaration-faker thing for standalone invoices would leave a CEI without a JE, which cannot be saved
					}
				}
			}
			return cusEntryInstruction;
		}
		CusEntryInstruction cusEntryInstruction;

		public new CusEntryInstructionCollection<CusEntryInstruction> CustomsEntryInstructions => (CusEntryInstructionCollection<CusEntryInstruction>)base.CustomsEntryInstructions;

		#endregion

		protected override string GetIApportionInvoiceHolderCountryContextCore() => GetIncoTermFactoryFromCompanyConfiguration();

		string GetIncoTermFactoryFromCompanyConfiguration() => Configuration is ZZRefCusConfiguration config
			? IsExport
				? config.ZZC_CustomsValueCodeForExport + config.ZZC_CustomsValueCodeForExport
				: config.ZZC_CustomsValueCode + config.ZZC_VATValueCode
			: CustomsValueCodeList.Codes.FOB + CustomsValueCodeList.Codes.CIF;

		[ResourceStringData("Enterprise.Customs.AsycudaCustoms.Business|JE_CustomsOffice", Caption = "Customs Office", ShortCaption = "Office")]
		[MaxLength(nameof(JE_CustomsOffice_MaxLength))]
		public override ZString JE_CustomsOffice
		{
			get { return base.JE_CustomsOffice; }
			set { base.JE_CustomsOffice = value; }
		}

		protected int JE_CustomsOffice_MaxLength => RefCusCodeType.GetMaxLength(Factory, CountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Schema.JE_CustomsOfficeMaxLength, ZString.Empty);

		public void AllocateAllBGMReferences()
		{
			var emptyBGMReferenceEntryHeaders = ActiveEntryHeaders.Cast<CusEntryHeader>().Where(x => x.NeedAllocateBGMReference).ToList();
			var nextBGMReferenceCounter = GetBGMReferenceCounter(emptyBGMReferenceEntryHeaders.Count);

			if (nextBGMReferenceCounter >= 0)
			{
				PopulateJE_DeclarationReferenceIfNeeded();
				var reference = JE_DeclarationReference + "/";

				foreach (var entryheader in emptyBGMReferenceEntryHeaders)
				{
					entryheader.CH_BGMReference = new ZString(reference + nextBGMReferenceCounter++).Right(Customs.Business.AutoCusEntryHeader.Schema.CH_BGMReferenceMaxLength);
				}
			}
		}

		int GetBGMReferenceCounter(int incrementStep)
		{
			return BGMReferenceCounterProvider.Instance.Value.GetBGMReferenceCounter(PK, incrementStep);
		}

		public const string BGMReferenceCounterString = "BGMReferenceCounter";

		#region LocalCurrencyCode

		protected override ZString LocalCurrencyCodeCore
		{
			get
			{
				var country = Branch?.Company?.Country.Code ?? GlbCompany.CurrentCompany.Country.Code;
				var attribute = new RefCusCodeListAttribute.Loader(Factory).Load(
					Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping,
					ZDate.Today,
					Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Asycuda,
					country,
					UniversalReferenceConstants.RefCusCodeListAttributes.Name.CustomsCurrency).FirstOrDefault();
				return attribute?.ZZE_Value ?? ZString.Empty;
			}
		}

		#endregion

		protected override Customs.Business.JobDeclarationDeepCloneStrategy GetTemplateCopyStrategy(BusinessObjectFactory alternateFactory, CloneType cloneType)
		{
			return new JobDeclarationDeepCloneStrategy(this, cloneType, alternateFactory);
		}

		protected override ZBool IsReciprocalRatesCore => ApplicationBusinessProvider?.IsReciprocalRates(this) ?? base.IsReciprocalRatesCore;

		protected override bool ShouldLogEventIfJE_EntryStatusChanged => false;

		#endregion
	}
}
