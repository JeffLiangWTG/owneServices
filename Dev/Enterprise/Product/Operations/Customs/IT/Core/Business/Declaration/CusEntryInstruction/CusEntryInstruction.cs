using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.Business.Declaration;

[SystemDefinedValues]
public partial class CusEntryInstruction : AutoCusEntryInstruction
	, Integration.Customs.IT.ICusEntryInstruction
	, IProcedureCodeProvider
	, IElectronicFolderSupporter
{
	public CusEntryInstruction(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new class Schema : AutoCusEntryInstruction.Schema
	{
		public new const int CEI_SubStyleMaxLength = 1;
		public const string Incoterm = "Incoterm";
		public const string ValuationCode = "ValuationCode";
		public const string Currency = "Currency";
		public const string FinancialAndBankingDataLine1 = "FinancialAndBankingDataLine1";
		public const int FinancialAndBankingDataLine1MaxLenght = 60;
		public const string FinancialAndBankingDataLine2 = "FinancialAndBankingDataLine2";
		public const int FinancialAndBankingDataLine2MaxLenght = 60;
		public const string PreviousInvoiceCurrencyExRate = "PreviousInvoiceCurrencyExRate";
		public const string ClearanceByEntryLine = "ClearanceByEntryLine";
		public const string ElectronicDocuments = "ElectronicDocuments";
		public new const int CEI_ProcedureMaxLength = 2;
	}

	public static class GenAddOnColumnConstants
	{
		public const string UseElectronicDocumentsColumnName = "IT_UseElectronicDocuments";
	}

	public new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;

	[ResourceStringData("Enterprise.Customs.IT.Business.Declaration.CusEntryInstruction|ElectronicDocuments", Caption = "Electronic Documents Upload Required?")]
	public ZBool ElectronicDocuments
	{
		get => this.GetSystemDefinedValue<ZBool>(GenAddOnColumnConstants.UseElectronicDocumentsColumnName);
		set
		{
			var oldValue = ElectronicDocuments;
			if (oldValue != value)
			{
				this.SetSystemDefinedValue(GenAddOnColumnConstants.UseElectronicDocumentsColumnName, value);
				ElectronicDocumentsInfo.RefreshBinding(oldValue);
			}
		}
	}

	public ZPropertyInfo ElectronicDocumentsInfo => GetZPropertyInfo(Schema.ElectronicDocuments);

	[MaxLength(Schema.CEI_SubStyleMaxLength)]
	[ReadOnlyMember(nameof(EntryIsInAmendingStatus))]
	public override ZString CEI_SubStyle
	{
		get => base.CEI_SubStyle;
		set
		{
			var oldValue = CEI_SubStyle;
			base.CEI_SubStyle = value;
			if (!IsCopying && oldValue != CEI_SubStyle)
			{
				DefaultDateForDutyIfApplicable();
				ClearSimplifiedDecAcceptanceDate();
			}
		}
	}

	public override ZString CEI_Style
	{
		get => base.CEI_Style;
		set
		{
			var oldValue = CEI_Style;
			base.CEI_Style = value;
			if (!IsCopying && oldValue != CEI_Style)
			{
				SetDefaultSubStyleIfRequired();
				DefaultDateForDutyIfStyleIsNotEmpty();
				ClearSimplifiedDecAcceptanceDate();
				RemoveFeesIfNotAllowed();
			}
		}
	}

	[ResourceStringData("Enterprise.Customs.IT.Business.Declaration.CusEntryInstruction|ZG_TempProcLimitDate",
		Caption = "Temporary Procedure Limit Date",
		ShortCaption = "Limit Date",
		MediumCaption = "Temp. Procedure Limit Date")]
	public override ZDateTime ZG_TempProcLimitDate
	{
		get => base.ZG_TempProcLimitDate;
		set => base.ZG_TempProcLimitDate = value;
	}

	public ZBool TempProcLimitDateRequired
	{
		get
		{
			{
				return HasIntoTemporaryExportProcedure || HasIntoTemporaryImportProcedure || HasIntoInwardProcessingProcedure || HasIntoOutwardProcessingProcedure;
			}
		}
	}

	[MaxLength(Schema.CEI_ProcedureMaxLength)]
	[ResourceStringData("Enterprise.Customs.IT.Business.Declaration.CusEntryInstruction|CEI_Procedure", Caption = "Procedure Code", ShortCaption = "PC")]
	[ReadOnlyMember(nameof(EntryIsInAmendingStatus))]
	public override ZString CEI_Procedure
	{
		get => base.CEI_Procedure;
		set
		{
			var oldValue = CEI_Procedure;
			base.CEI_Procedure = value;
			if (!IsCopying && oldValue != CEI_Procedure)
			{
				JobDeclaration?.MarkAsNeedingValidation();
				RefreshInvoiceLineBindings();
			}
		}
	}

	[ResourceStringData("Enterprise.Customs.IT.Business.Declaration.CusEntryInstruction|CEI_DateForDuty", Caption = "Acceptance Date")]
	public override ZDateTime CEI_DateForDuty
	{
		get => base.CEI_DateForDuty;
		set
		{
			var oldValue = CEI_DateForDuty;
			base.CEI_DateForDuty = value;
			if (!IsCopying && oldValue != CEI_DateForDuty && JobDeclaration != null)
			{
				JobDeclaration.Invoices.ForEach(x => x.MarkAsNeedingValidation());
			}
		}
	}

	public override OrgAddress WarehouseFor27 => base.Warehouse2;

	[ResourceStringData("Enterprise.Customs.IT.Business.Declaration.CusEntryInstruction|WarehouseIDFor27", Caption = "[49] To Warehouse")]
	public override ZString WarehouseIDFor27 => ToWarehouseCode;

	public ZPropertyInfo WarehouseIDFor27Info => GetZPropertyInfo(nameof(WarehouseIDFor27));

	[ResourceStringData("Enterprise.Customs.IT.Business.Declaration.CusEntryInstruction|FromWarehouseCode", Caption = "[49] From Warehouse")]
	public override ZString FromWarehouseCode => base.FromWarehouseCode;

	void RefreshInvoiceLineBindings()
	{
		foreach (var invoiceLine in InvoiceLines)
		{
			invoiceLine.JI_CEIInfo.RefreshBinding();
		}
	}

	public new CusEntryInstructionLookups Lookups => (CusEntryInstructionLookups)base.Lookups;

	protected override Customs.Business.CusEntryInstructionLookups GetNewLookups()
	{
		return new CusEntryInstructionLookups(this);
	}

	protected override Customs.Business.CusEntryInstructionValidation GetNewValidation() => new CusEntryInstructionValidation(this);
	protected override EU.Business.Declaration.AddInfoCusEntryInstruction GetNewAddInfo() => new AddInfoCusEntryInstruction(this);
	public new AddInfoCusEntryInstructionValidation AddInfoValidation => (AddInfoCusEntryInstructionValidation)AddInfo.Validation;
	public new AddInfoCusEntryInstructionLookups AddInfoLookups => (AddInfoCusEntryInstructionLookups)AddInfo.Lookups;
	public new CusEntryInstructionValidation Validation => (CusEntryInstructionValidation)base.Validation;
	public new CusEntryHeader EntryHeader => (CusEntryHeader)base.EntryHeader;

	public bool AllRelatedInvoicesHaveSameIncoTerms => Factory.GetValue(ref allRelatedInvoicesHaveSameIncoTermsCache, () => AllRelatedInvoicesHaveSamePropertyValue(x => x.IncoTerm));
	CachedProperty<bool> allRelatedInvoicesHaveSameIncoTermsCache;

	public bool AllRelatedInvoicesHaveSameValuationCode => Factory.GetValue(ref allRelatedInvoicesHaveSameValuationCodeCache, () => AllRelatedInvoicesHaveSamePropertyValue(x => x.JZ_ValuationCode));
	CachedProperty<bool> allRelatedInvoicesHaveSameValuationCodeCache;

	public bool AllRelatedInvoicesHaveSameCurrency => Factory.GetValue(ref allRelatedInvoicesHaveSameCurrencyCache, () => AllRelatedInvoicesHaveSamePropertyValue(x => x.JZ_RX_NKInvoice_Currency));
	CachedProperty<bool> allRelatedInvoicesHaveSameCurrencyCache;

	public bool AllRelatedInvoicesHaveSameIncoTermPlace => Factory.GetValue(ref allRelatedInvoicesHaveSameIncoTermPlaceCache, () => AllRelatedInvoicesHaveSamePropertyValue(x => x.JZ_IncoTermPlace));
	CachedProperty<bool> allRelatedInvoicesHaveSameIncoTermPlaceCache;

	public bool AllRelatedInvoicesHaveSameAgreedPlaceCode => Factory.GetValue(ref allRelatedInvoicesHaveSameAgreedPlaceCodeCache, () => AllRelatedInvoicesHaveSamePropertyValue(x => x.ZG_AgreedPlaceCode));
	CachedProperty<bool> allRelatedInvoicesHaveSameAgreedPlaceCodeCache;

	public bool AllRelatedInvoicesHaveSameDeliveryTerms => Factory.GetValue(ref allRelatedInvoicesHaveSameDeliveryTermsCache, () => AllRelatedInvoicesHaveSamePropertyValue(x => x.JZ_AdditionalTerms));
	CachedProperty<bool> allRelatedInvoicesHaveSameDeliveryTermsCache;

	public new IEnumerable<JobComInvoiceHeader> Invoices => base.Invoices.Cast<JobComInvoiceHeader>();

	[ResourceStringData("Enterprise.Customs.IT.Business.Declaration.CusEntryInstruction|Incoterm", Caption = "Incoterm", ShortCaption = "Incoterm")]
	public ZString Incoterm => GetValueFromInvoiceHeadersOrEmpty(x => x.JZ_IncoTerm, AllRelatedInvoicesHaveSameIncoTerms);
	public ZPropertyInfo IncotermInfo => GetZPropertyInfo(Schema.Incoterm);

	[ResourceStringData("Enterprise.Customs.IT.Business.Declaration.CusEntryInstruction|ValuationCode", Caption = "Tran. Nature", ShortCaption = "Tran. Nature")]
	public ZString ValuationCode => GetValueFromInvoiceHeadersOrEmpty(x => x.JZ_ValuationCode, AllRelatedInvoicesHaveSameValuationCode);
	public ZPropertyInfo ValuationCodeInfo => GetZPropertyInfo(Schema.ValuationCode);

	[ResourceStringData("Enterprise.Customs.IT.Business.Declaration.CusEntryInstruction|Currency", Caption = "Currency", ShortCaption = "Currency")]
	public ZString Currency => GetValueFromInvoiceHeadersOrEmpty(x => x.JZ_RX_NKInvoice_Currency, AllRelatedInvoicesHaveSameCurrency);
	public ZPropertyInfo CurrencyInfo => GetZPropertyInfo(Schema.Currency);

	public ZBool HasIntoWarehouseProcedureOnAnyInvoiceLine => base.HasIntoWarehouseProcedure;

	public new ZBool HasIntoWarehouseProcedure => RandomProcedure?.IsIntoWarehouse ?? ZBool.False;
	public ZBool HasReimportProcedure => RandomProcedure?.IsReimportProcedure ?? ZBool.False;

	public new ZBool HasIntoTemporaryExportProcedure => RandomProcedure?.IsIntoTemporaryExportProcedure ?? ZBool.False;
	public new ZBool HasIntoTemporaryImportProcedure => RandomProcedure?.IsIntoTemporaryImportProcedure ?? ZBool.False;
	public new ZBool HasIntoOutwardProcessingProcedure => RandomProcedure?.IsIntoOutwardProcessing ?? ZBool.False;
	public new ZBool HasIntoInwardProcessingProcedure => RandomProcedure?.IsIntoInwardProcessing ?? ZBool.False;

	public ZBool HasAtLeastOneAuthorizationUsage(ZString code)
	{
		var dict = hasAtLeastOneAuthorizationUsageCachedProperty ??= new Dictionary<ZString, CachedProperty<ZBool>>();
		var calculatedProperty = dict.GetOrAdd(code, () => default);
		return Factory.GetValue(ref calculatedProperty, () => GetHasAtLeastOneAuthorizationUsage(code));
	}
	Dictionary<ZString, CachedProperty<ZBool>> hasAtLeastOneAuthorizationUsageCachedProperty;
	bool GetHasAtLeastOneAuthorizationUsage(ZString code) => CusAuthorizationUsages
		.Select(x => x.AGC_Code)
		.Contains(code);

	public ZBool HasAtLeastOneSupportingDocument(ZString code)
	{
		var dict = hasAtLeastOneSupportingDocumentCachedProperty ??= new Dictionary<ZString, CachedProperty<ZBool>>();
		var calculatedProperty = dict.GetOrAdd(code, () => default);
		return Factory.GetValue(ref calculatedProperty, () => GetHasAtLeastOneSupportingDocument(code));
	}
	Dictionary<ZString, CachedProperty<ZBool>> hasAtLeastOneSupportingDocumentCachedProperty;
	bool GetHasAtLeastOneSupportingDocument(ZString code) => InvoiceLines
		.Cast<JobComInvoiceLine>()
		.SelectMany(x => x.SupportingDocuments.Cast<SupportingDocument>())
		.Select(x => x.CSI_Code)
		.Contains(code);

	public RefCusIntoProcedureWrapper RandomProcedure => GetRandomProcedure();
	RefCusIntoProcedureWrapper randomProcedure;

	RefCusIntoProcedureWrapper GetRandomProcedure()
	{
		if (randomProcedure == null || randomProcedure.ProcedureCode != CEI_Procedure)
		{
			randomProcedure = RefCusIntoProcedureWrapper.GetNewIfProcedureCodeIsValid(this, Factory);
		}
		return randomProcedure;
	}

	[MaxLength(Schema.FinancialAndBankingDataLine1MaxLenght)]
	public ZString FinancialAndBankingDataLine1
	{
		get => this.GetSystemDefinedValue<ZString>(Schema.FinancialAndBankingDataLine1);
		set
		{
			var oldValue = FinancialAndBankingDataLine1;
			if (oldValue != value)
			{
				CheckMaximumLength(FinancialAndBankingDataLine1Info, value);
				this.SetSystemDefinedValue(Schema.FinancialAndBankingDataLine1, value);
				FinancialAndBankingDataLine1Info.RefreshBinding(oldValue);
				if (!IsValidationSuspended)
				{
					Validation.ValidateFinancialAndBankingDataLine1();
				}
			}
		}
	}

	public ZPropertyInfo FinancialAndBankingDataLine1Info => GetZPropertyInfo(Schema.FinancialAndBankingDataLine1);

	[MaxLength(Schema.FinancialAndBankingDataLine2MaxLenght)]
	public ZString FinancialAndBankingDataLine2
	{
		get => this.GetSystemDefinedValue<ZString>(Schema.FinancialAndBankingDataLine2);
		set
		{
			var oldValue = FinancialAndBankingDataLine2;
			if (oldValue != value)
			{
				CheckMaximumLength(FinancialAndBankingDataLine2Info, value);
				this.SetSystemDefinedValue(Schema.FinancialAndBankingDataLine2, value);
				FinancialAndBankingDataLine2Info.RefreshBinding(oldValue);
				if (!IsValidationSuspended)
				{
					Validation.ValidateFinancialAndBankingDataLine2();
				}
			}
		}
	}

	public ZPropertyInfo FinancialAndBankingDataLine2Info => GetZPropertyInfo(Schema.FinancialAndBankingDataLine2);

	[ResourceStringData("Enterprise.Customs.IT.Business.Declaration.CusEntryInstruction|ZG_UseDeclarationOfIntent", Caption = "Declaration of Intent", ShortCaption = "Decl. of Intent")]
	[ReadOnlyMember(nameof(ZG_UseDeclarationOfIntentReadOnly))]
	public override ZBool ZG_UseDeclarationOfIntent
	{
		get => base.ZG_UseDeclarationOfIntent;
		set
		{
			var oldValue = ZG_UseDeclarationOfIntent;
			base.ZG_UseDeclarationOfIntent = value;
			if (!IsCopying && oldValue != ZG_UseDeclarationOfIntent)
			{
				DefaultSupportingDocument01DI();
			}
		}
	}

	public ZBool IsPreliminaryDeclarationUnderCodeA => CEI_SubStyle == ITEntrySubStyleList.Codes.PreliminaryStandardDeclarationD;
	public ZBool IsSimplifiedDeclaration => CEI_Style == SADDeclarationTypeList.Codes.DichiarazioneSemplificata;

	ZBool ZG_UseDeclarationOfIntentReadOnly => JobDeclaration?.DeclarationOfIntentAuthorisation == null;

	[ResourceStringData("Enterprise.Customs.IT.Business.Declaration.CusEntryInstruction|CEI_OA_Warehouse2", Caption = "[49] To Warehouse")]
	public override ZGuid CEI_OA_Warehouse2
	{
		get => base.CEI_OA_Warehouse2;
		set
		{
			var oldValue = CEI_OA_Warehouse2;
			base.CEI_OA_Warehouse2 = value;
			var newValue = CEI_OA_Warehouse2;
			if (newValue != oldValue)
			{
				if (!IsCopying)
				{
					InvoiceLines.Cast<JobComInvoiceLine>().ToList().ForEach(x => x.SupportingDocumentsManager.AddCustomsDecisionsSupportingDocumentIfApplicable());
				}

				UpdateWarehouseTypeAndId(oldValue, newValue, warehouseTypeSetter: v => ZG_ToWarehouseType = v, warehouseIdSetter: v => ZG_ToWarehouseID = v, shouldUpdateAuthorizationUsages: IsImportH2);
			}
		}
	}

	[ResourceStringData("Enterprise.Customs.IT.Business.Declaration.CusEntryInstruction|CEI_OA_Warehouse", Caption = "[49] From Warehouse")]
	public override ZGuid CEI_OA_Warehouse
	{
		get => base.CEI_OA_Warehouse;
		set
		{
			var oldValue = base.CEI_OA_Warehouse;
			base.CEI_OA_Warehouse = value;
			var newValue = CEI_OA_Warehouse;
			if (oldValue != newValue)
			{
				UpdateWarehouseTypeAndId(oldValue, newValue, warehouseTypeSetter: v => ZG_FromWarehouseType = v, warehouseIdSetter: v => ZG_FromWarehouseID = v, shouldUpdateAuthorizationUsages: false);
			}
		}
	}

	void DefaultSupportingDocument01DI()
	{
		JobDeclaration?.DeclarationOfIntentRefresher.DefaultSupportingDocument01DI(this);
	}

	ZString GetValueFromInvoiceHeadersOrEmpty(Func<JobComInvoiceHeader, ZString> selector, ZBool allInvoicesHaveSameValue)
	{
		if (!HasIntoWarehouseProcedure && allInvoicesHaveSameValue)
		{
			return Invoices.Select(selector).Distinct().FirstOrDefault();
		}
		else
		{
			return ZString.Empty;
		}
	}

	ZBool AllRelatedInvoicesHaveSamePropertyValue(Func<JobComInvoiceHeader, ZString> propertySelector) => Invoices.Select(propertySelector).Distinct().Count() == 1;

	void DefaultDateForDutyIfStyleIsNotEmpty()
	{
		if (!CEI_Style.IsEmpty)
		{
			DefaultDateForDutyIfApplicable();
		}
	}

	void DefaultDateForDutyIfApplicable()
	{
		if (CEI_DateForDuty.IsEmpty && (!IsPreliminaryDeclarationUnderCodeA || IsUCC6AndIsExport))
		{
			CEI_DateForDuty = ZDate.Today;
		}
	}

	[ResourceStringData("Enterprise.Customs.IT.Business.Declaration.CusEntryInstruction|ZG_ParticipantType", Caption = "Participants")]
	public override ZString ZG_ParticipantType
	{
		get => base.ZG_ParticipantType;
		set
		{
			var oldValue = ZG_ParticipantType;
			base.ZG_ParticipantType = value;
			if (!IsCopying && oldValue != ZG_ParticipantType)
			{
				ResetOrDefaultPreviousInvoiceFields();
			}
		}
	}

	void ResetOrDefaultPreviousInvoiceFields()
	{
		if (!IsTriangulationOrJointDeclaration)
		{
			ZG_PreviousInvoiceAmount = ZDecimal.Zero;
			ZG_PreviousInvoiceCurrency = ZString.Empty;
		}
		else if (ZG_PreviousInvoiceCurrency.IsEmpty)
		{
			ZG_PreviousInvoiceCurrency = LocalCurrencyCode;
		}
	}

	public ZBool IsTriangulationOrJointDeclaration
	{
		get
		{
			var result = ZBool.False;
			switch (ZG_ParticipantType)
			{
				case ParticipantTypeList.Codes.Triangulation:
				case ParticipantTypeList.Codes.JointDeclarationManySuppliersForTheSameEntryLine:
					result = ZBool.True;
					break;
			}
			return result;
		}
	}

	public ZBool IsBuyersConsol => ZG_ParticipantType == ParticipantTypeList.Codes.BuyersConsolManySuppliersOneImporter;

	[ResourceStringData("Enterprise.Customs.IT.Business.Declaration.CusEntryInstruction|ZG_PreviousInvoiceAmount", Caption = "Amount")]
	public override ZDecimal ZG_PreviousInvoiceAmount { get => base.ZG_PreviousInvoiceAmount; set => base.ZG_PreviousInvoiceAmount = value; }

	public override ZString ZG_PreviousInvoiceCurrency
	{
		get => base.ZG_PreviousInvoiceCurrency; set
		{
			var oldValue = ZG_PreviousInvoiceCurrency;
			base.ZG_PreviousInvoiceCurrency = value;
			if (!IsCopying && oldValue != ZG_PreviousInvoiceCurrency)
			{
				SetPreviousInvoiceCurrencyExRate();
			}
		}
	}

	RefCurrency PreviousInvoiceCurrency => Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, ZG_PreviousInvoiceCurrency);

	ZString LocalCurrencyCode => JobDeclaration?.LocalCurrencyCode ?? Core.Constants.CurrencyCodes.Italy;

	void SetPreviousInvoiceCurrencyExRate()
	{
		ZDecimal exchangeRate;
		var previousInvoiceCurrency = PreviousInvoiceCurrency;
		if (previousInvoiceCurrency == null)
		{
			exchangeRate = 0m;
		}
		else if (previousInvoiceCurrency.RX_Code == LocalCurrencyCode)
		{
			exchangeRate = 1m;
		}
		else
		{
			exchangeRate = CurrencyConverter.GetExchangeRate(previousInvoiceCurrency);
		}

		PreviousInvoiceCurrencyExRate = exchangeRate;
	}

	[DecimalPlaces(6)]
	[ReadOnlyMember(nameof(PreviousInvoiceCurrencyExRateReadOnly))]
	[ResourceStringData("Enterprise.Customs.IT.Business.Declaration.CusEntryInstruction|PreviousInvoiceCurrencyExRate", Caption = "Exchange Rate")]
	public ZDecimal PreviousInvoiceCurrencyExRate
	{
		get => previousInvoiceCurrencyExRate;
		set => SetNonPersistentPropertyValue(PreviousInvoiceCurrencyExRateInfo, ref previousInvoiceCurrencyExRate, value);
	}
	ZDecimal previousInvoiceCurrencyExRate;

	public ZPropertyInfo PreviousInvoiceCurrencyExRateInfo => GetZPropertyInfo(Schema.PreviousInvoiceCurrencyExRate);

	ZBool PreviousInvoiceCurrencyExRateReadOnly => ZBool.True;

	#region IProcedureCodeProvider

	ZString IProcedureCodeProvider.ProcedureCode => CEI_Procedure;

	ZString IProcedureCodeProvider.CountryCode => CountryCode;

	#endregion

	#region IElectronicFolderSupporter Members

	ZBool IElectronicFolderSupporter.UseElectronicFolder => ElectronicDocuments;

	#endregion

	[ResourceStringData("Enterprise.Customs.IT.Business.Declaration.CusEntryInstruction|ClearanceByEntryLine", Caption = "Clearance by Entry Line", FullDescription = "Request clearance by entry line")]
	[ReadOnlyMember(nameof(EntryIsInAmendingStatus))]
	public ZBool ClearanceByEntryLine
	{
		get => this.GetSystemDefinedValue<ZBool>(Schema.ClearanceByEntryLine);
		set
		{
			var oldValue = ClearanceByEntryLine;
			if (oldValue != value)
			{
				this.SetSystemDefinedValue(Schema.ClearanceByEntryLine, value);
				ClearanceByEntryLineInfo.RefreshBinding(oldValue);
			}
		}
	}

	public ZPropertyInfo ClearanceByEntryLineInfo => GetZPropertyInfo(Schema.ClearanceByEntryLine);

	bool EntryIsInAmendingStatus => EntryHeader?.IsInAmendingStatus ?? false;

	#region SimpliedDecAcceptanceDate

	[ResourceStringData("Enterprise.Customs.IT.Business.Declaration.CusEntryInstruction|ZG_SimplifiedDecAcceptanceDate",
		FullDescription = "Simplified Declaration Acceptance Date",
		Caption = "Sim. Decl. Accept. Date",
		ShortCaption = "Sim. Dec. Acc. Date",
		MediumCaption = "Sim. Decl. Accept. Date")]
	public override ZDateTime ZG_SimplifiedDecAcceptanceDate
	{
		get => base.ZG_SimplifiedDecAcceptanceDate;
		set => base.ZG_SimplifiedDecAcceptanceDate = value;
	}

	public bool CanSetSimplifiedDecAcceptanceDate => Factory.GetValue(ref canSetSimplifiedDecAcceptanceDateCachedProperty, () => GetCanSetSimplifiedDecAcceptanceDate());
	CachedProperty<bool> canSetSimplifiedDecAcceptanceDateCachedProperty;

	bool GetCanSetSimplifiedDecAcceptanceDate()
	{
		return IsUCC6ExportAndC2Style ||
		  (!IsUCC6ExportAndC1Style && subStylesAllowingSimplifiedDecAcceptanceDate.Contains(CEI_SubStyle));
	}

	bool IsUCC6ExportAndC1Style => IsUCC6AndIsExport && CEI_Style == ExportUCC6DeclarationTypeList.Codes.DichiarazioneSemplificataC1;

	bool IsUCC6ExportAndC2Style => IsUCC6AndIsExport && CEI_Style == ExportUCC6DeclarationTypeList.Codes.NotificaDiPresentazioneDelleMerciC2;

	#endregion

	#region ZG_PresentationStartDate

	[ResourceStringData("Enterprise.Customs.IT.Business.Declaration.CusEntryInstruction|ZG_PresentationStartDate",
		FullDescription = "Date and Time of Presentation of the Goods",
		Caption = "Date/Time Pres. Goods",
		ShortCaption = "Pres. Goods",
		MediumCaption = "Date/Time Pres. Goods")]
	public override ZDateTime ZG_PresentationStartDate { get => base.ZG_PresentationStartDate; set => base.ZG_PresentationStartDate = value; }

	#endregion

	public new EU.Business.ICusAuthorizationUsageCollection<CusAuthorizationUsage, CusEntryInstruction> CusAuthorizationUsages => (EU.Business.ICusAuthorizationUsageCollection<CusAuthorizationUsage, CusEntryInstruction>)base.CusAuthorizationUsages;
	protected override EU.Business.ICusAuthorizationUsageCollection<EU.Business.CusAuthorizationUsage, EU.Business.Declaration.CusEntryInstruction> GetCusAuthorizationUsages() => new EU.Business.CusAuthorizationUsageCollection<CusAuthorizationUsage, CusEntryInstruction>(this, Factory);

	public new GuaranteeForEntryInstructionCollection Guarantees => (GuaranteeForEntryInstructionCollection)base.Guarantees;
	protected override EU.Business.Declaration.GuaranteeForEntryInstructionCollection GetNewGuaranteeForEntryInstructionCollection() => new GuaranteeForEntryInstructionCollection(this);

	protected override ICusSupplyChainActorReferenceCollection<EU.Business.Declaration.CusSupplyChainActorReference> GetNewCusSupplyChainActorReferenceCollection() => new CusSupplyChainActorReferenceCollection<CusSupplyChainActorReference>(this);
	protected override Type SupplyChainActorType => typeof(CusSupplyChainActorReference);

	public new SupportingDocumentCollection SupportingDocuments => (SupportingDocumentCollection)base.SupportingDocuments;
	protected override EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentCollection CreateNewSupportingDocumentCollection() => new SupportingDocumentCollection(this);

	public new PreviousDocumentCollection PreviousDocuments => (PreviousDocumentCollection)base.PreviousDocuments;
	protected override EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentCollection CreateNewPreviousDocumentCollection() => new PreviousDocumentCollection(this);

	void UpdateWarehouseTypeAndId(ZGuid oldWarehouseAddressId, ZGuid newWarehouseAddressId, Action<ZString> warehouseTypeSetter, Action<ZString> warehouseIdSetter, bool shouldUpdateAuthorizationUsages)
	{
		Argument.NotNull(warehouseTypeSetter, nameof(warehouseTypeSetter));
		Argument.NotNull(warehouseIdSetter, nameof(warehouseIdSetter));

		if (shouldUpdateAuthorizationUsages)
		{
			DeleteAllAuthorizationUsagesForWarehouse(oldWarehouseAddressId);
		}

		if (newWarehouseAddressId.IsEmpty || !newWarehouseAddressId.IsValid || !IsUCC6)
		{
			warehouseTypeSetter(ZString.Empty);
			warehouseIdSetter(ZString.Empty);
			return;
		}

		var authorisationHeaders = GetAuthorisationHeaders(newWarehouseAddressId);
		if (authorisationHeaders == null || authorisationHeaders.Count != 1)
		{
			warehouseTypeSetter(ZString.Empty);
			warehouseIdSetter(ZString.Empty);
			return;
		}

		var authorizationHeader = authorisationHeaders.Single();
		warehouseIdSetter(authorizationHeader.CPH_Number);

		var warehouseType = authorizationHeaderTypeToWarehouseTypeMap[authorizationHeader.CPH_Type];
		warehouseTypeSetter(warehouseType);

		if (shouldUpdateAuthorizationUsages)
		{
			AddUsageForAuthorizationIfNotExists(authorizationHeader);
		}
	}

	#region UpdateToWarehouseAuthorizationForH2

	void AddUsageForAuthorizationIfNotExists(CusAuthorisationHeader auth)
	{
		var usages = GetUsagesForAuthorization(auth);
		if (!usages.Any())
		{
			AddUsageForAuthorization(auth);
		}
	}

	void AddUsageForAuthorization(CusAuthorisationHeader auth)
	{
		if (auth == null)
		{
			return;
		}

		var usage = CusAuthorizationUsages.AddNew();
		usage.AGC_Code = auth.CPH_Type;
		usage.AGC_OH_Owner = auth.CPH_OH_PermitHolder;
		usage.AGC_Number = auth.CPH_Number;
	}

	void DeleteAllAuthorizationUsagesForWarehouse(ZGuid warehouseAddressId)
	{
		var auth = GetUniqueAuthorizationForWarehouse(warehouseAddressId);
		if (auth != null)
		{
			GetUsagesForAuthorization(auth).DeleteAll();
		}
	}

	CusAuthorisationHeader GetUniqueAuthorizationForWarehouse(ZGuid warehouseAddressId)
	{
		if (warehouseAddressId.IsEmpty || !warehouseAddressId.IsValid)
		{
			return null;
		}

		var authorisationHeaders = GetAuthorisationHeaders(warehouseAddressId);
		return authorisationHeaders?.Count == 1 ? authorisationHeaders.First() : null;
	}

	IEnumerable<CusAuthorizationUsage> GetUsagesForAuthorization(CusAuthorisationHeader auth) => auth is null ? [] : CusAuthorizationUsages.Where(usage => usage.IsForAuthorisationHeader(auth));

	#endregion

	void SetDefaultSubStyleIfRequired()
	{
		if (CEI_SubStyle.IsEmpty && declarationTypesNeedsSubStyleDefaulting.Contains(CEI_Style))
		{
			CEI_SubStyle = ITEntrySubStyleList.Codes.StandardDeclarationA;
		}
	}

	void ClearSimplifiedDecAcceptanceDate()
	{
		if (!CanSetSimplifiedDecAcceptanceDate)
		{
			ZG_SimplifiedDecAcceptanceDate = ZDateTime.Empty;
		}
	}

	internal bool IsDutiesAndFeeCalculationAllowed()
	{
		if (!IsUCC6AndIsExport || CEI_Style.IsEmpty)
		{
			return true;
		}

		return !declarationTypesWithNoDutyOfFeeCalculationRequirement.Contains(CEI_Style);
	}

	void RemoveFeesIfNotAllowed()
	{
		if (IsDutiesAndFeeCalculationAllowed() || EntryHeader == null)
		{
			return;
		}

		foreach (var cusEntryLine in EntryHeader.MergedLines)
		{
			cusEntryLine.Fees?.RemoveAndDeleteAll();
		}
	}

	internal IReadOnlyCollection<CusAuthorisationHeader> GetAuthorisationHeaders(ZGuid addressId)
	{
		var date = CEI_DateForDuty;
		var key = $"IT_GetAuthorisationHeaders|{date.ToISO8601ShortDateString()}|{addressId.ToStringKey()}";
		var allowedKeys = authorizationHeaderTypeToWarehouseTypeMap.Keys.ToArray();
		return Factory.GetCachedValue(key, () => CusAuthorisationHeader.Loader.GetAuthorisationsForAddresses(Factory, Core.Constants.CountryCodes.Italy, allowedKeys, date, new[] { addressId }));
	}

	internal void RemoveAuthorizationUsageRecordsWithTypeIfExists(string authorizationCode)
	{
		Argument.NotNullOrEmpty(authorizationCode, nameof(authorizationCode));

		var cusAuthorizationUsageCollection = CusAuthorizationUsages;
		var matchingAuthorizationUsages = cusAuthorizationUsageCollection.Where(c => c.AGC_Code == authorizationCode).ToArray();
		matchingAuthorizationUsages.ForEach(cusAuthorizationUsageCollection.RemoveAndDelete);
	}

	internal CusAuthorizationUsage AddAuthorizationUsage(string authorizationCode, OrgHeader owner, string referenceNumber)
	{
		Argument.NotNullOrEmpty(authorizationCode, nameof(authorizationCode));

		var authorizationUsage = CusAuthorizationUsages.AddNew();
		authorizationUsage.AGC_Code = authorizationCode;
		authorizationUsage.AGC_Number = referenceNumber;
		authorizationUsage.AGC_OH_Owner = owner?.PK ?? ZGuid.Empty;
		return authorizationUsage;
	}

	public bool RequireGuaranteeNumberInReference2 => IsImport && !CEI_Style.IsEmpty
		&& (CEI_Style == ImportUCC6DeclarationTypeList.Codes.RegimeSpecialeAmmissioneTemporaneaH3 || CEI_Style == ImportUCC6DeclarationTypeList.Codes.RegimeSpecialePerfezionamentoAttivoH4);

	readonly ImmutableArray<string> declarationTypesWithNoDutyOfFeeCalculationRequirement = new[]
	{
		ExportUCC6DeclarationTypeList.Codes.DichiarazionePerTerritoriFiscaliSpecialiB4,
		ExportUCC6DeclarationTypeList.Codes.DichiarazioneSemplificataC1,
		ExportUCC6DeclarationTypeList.Codes.NotificaDiPresentazioneDelleMerciC2,
	}.ToImmutableArray();

	bool IsUCC6AndIsExport => JobDeclaration?.IsUCC6AndIsExport ?? false;

	bool IsUCC6 => JobDeclaration?.IsUCC6 ?? false;

	bool IsImport => JobDeclaration?.IsImport ?? false;

	bool IsImportH2 => IsImport && CEI_Style == ImportUCC6DeclarationTypeList.Codes.RegimeSpecialeDepositoDoganaleH2;

	readonly ImmutableArray<string> declarationTypesNeedsSubStyleDefaulting = new string[]
	{
		ImportUCC6DeclarationTypeList.Codes.ImmissioneLiberaPraticaH1,
		ImportUCC6DeclarationTypeList.Codes.RegimeSpecialeDepositoDoganaleH2,
		ImportUCC6DeclarationTypeList.Codes.RegimeSpecialeAmmissioneTemporaneaH3,
		ImportUCC6DeclarationTypeList.Codes.RegimeSpecialePerfezionamentoAttivoH4,
		SADDeclarationTypeList.Codes.ProceduraOrdinariaCODogana,
		SADDeclarationTypeList.Codes.ProceduraOrdinariaCOLuogo,
		SADDeclarationTypeList.Codes.DichiarazioneSemplificata
	}.ToImmutableArray();

	readonly ImmutableArray<string> subStylesAllowingSimplifiedDecAcceptanceDate = new[]
	{
		ITEntrySubStyleList.Codes.SupplementaryDeclarationX,
		ITEntrySubStyleList.Codes.SupplementaryDeclarationY,
		ITEntrySubStyleList.Codes.SupplementaryDeclarationZ,
	}.ToImmutableArray();

	readonly ImmutableDictionary<ZString, ZString> authorizationHeaderTypeToWarehouseTypeMap = new Dictionary<ZString, ZString>
	{
		{ CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, WarehouseTypeList.Codes.CW1 },
		{ CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2, WarehouseTypeList.Codes.CW2 },
		{ CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, WarehouseTypeList.Codes.CWP },
	}.ToImmutableDictionary();
}
