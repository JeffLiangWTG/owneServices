#if NETFRAMEWORK
using CargoWise.Common;
#elif NET
using DisposableAction = CargoWise.Common.DisposableAction;
#endif
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IT.Business.UniversalReference;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using IEnumerableExtensions = CargoWise.Common.IEnumerableExtensions;

namespace Enterprise.Customs.IT.Business.Declaration;

public partial class JobComInvoiceLine : AutoJobComInvoiceLine
	, Integration.Customs.IT.IJobComInvoiceLine
	, IAdditionalBusinessObjectFetchStrategyProvider
{
	public JobComInvoiceLine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new class Schema : EU.Business.Declaration.JobComInvoiceLine.Schema
	{
		public const string Remarks = "Remarks";
		public const int RemarksMaxLength = 300;
	}

	protected override Type InvoiceHeaderType => typeof(JobComInvoiceHeader);

	protected override JobComInvoiceLineValidation GetNewValidation()
	{
		CommonJobComInvoiceLineValidation validation;
		if (IsImport)
		{
			validation = new ImportJobComInvoiceLineValidation(this);
		}
		else if (IsExport)
		{
			validation = new ExportJobComInvoiceLineValidation(this);
		}
		else
		{
			validation = new CommonJobComInvoiceLineValidation(this);
		}
		return validation;
	}

	#region Properties

	public override ZGuid JI_CEI
	{
		get => base.JI_CEI;
		set
		{
			var oldValue = JI_CEI;
			base.JI_CEI = value;
			if (!IsCopying && oldValue != JI_CEI)
			{
				DefaultSupportingDocument01DI();
				SupportingDocumentsManager.AddCustomsDecisionsSupportingDocumentIfApplicable();
			}
		}
	}

	public override ZGuid JI_JZ
	{
		get => base.JI_JZ;
		set
		{
			var oldValue = JI_JZ;
			base.JI_JZ = value;
			if (!IsCopying && oldValue != JI_JZ)
			{
				SetDefaultValuationCodeIfRequired();
			}
		}
	}

	void SetDefaultValuationCodeIfRequired()
	{
		if (JI_ValuationCode.IsEmpty && (Declaration?.IsUCC6AndIsImport ?? false))
		{
			JI_ValuationCode = MasterFiles.Business.Customs.EU.ValuationMethodList.Codes._1;
		}
	}

	public void DefaultSupportingDocument01DI()
	{
		var entryInstruction = EntryInstruction;
		if (entryInstruction != null)
		{
			Declaration?.DeclarationOfIntentRefresher.DefaultSupportingDocument01DI(entryInstruction);
		}
	}

	[ResourceStringData("Enterprise.Customs.IT.Business.Declaration.ITJobComInvoiceLine|ZG_SteelType", Caption = "Steel Type")]
	public override ZString ZG_SteelType
	{
		get => base.ZG_SteelType;
		set => base.ZG_SteelType = value;
	}

	[ReadOnlyMember(nameof(PortTaxRateReadOnly))]
	[ResourceStringData("Enterprise.Customs.IT.Business.Declaration.ITJobComInvoiceLine|ZG_PortTaxRate", Caption = "Port Tax Rate")]
	public override ZString ZG_PortTaxRate
	{
		get => base.ZG_PortTaxRate;
		set
		{
			var oldValue = ZG_PortTaxRate;
			base.ZG_PortTaxRate = value;
			if (!IsCopying && oldValue != ZG_PortTaxRate)
			{
				RefreshPortTaxSupportingDocument();
			}
		}
	}

	internal void RefreshPortTaxSupportingDocument()
	{
		var invoiceLineWrapper = new JobComInvoiceLineSupportingDocumentsWithHarbourRateProvider(this);
		var supportingDocumentRefresher = new PortTaxSupportingDocumentRefresher(invoiceLineWrapper);
		supportingDocumentRefresher.RefreshDocument();
	}

	public ZBool PortTaxRateReadOnly => !(Declaration?.NeedsPortTax ?? ZBool.False);

	public ZBool IsTemporaryProcedure => CusProcedure?.IsIntoTemporaryProcedure() ?? ZBool.False;

	public override ZString JI_Tariff
	{
		get => base.JI_Tariff;
		set
		{
			if (!SetterSuspender.IsSetterSuspended(Schema.JI_Tariff))
			{
				var oldValue = JI_Tariff;
				base.JI_Tariff = value;
				if (!IsCopying && JI_Tariff != oldValue)
				{
					DefaultSupplementaryQuantityUOMFromTariffIfNeeded();
					DefaultThirdQuantityUOMFromTariffIfNeeded();
				}
			}
		}
	}

	void DefaultSupplementaryQuantityUOMFromTariffIfNeeded()
	{
		if (!IsUOMDefaultingSuspended)
		{
			var availableUnitsOfMeasure = Lookups.SupplementaryQuantityUOMs;
			JI_CustomsSecondUnitQty = availableUnitsOfMeasure.Count() == 1 ? availableUnitsOfMeasure.Single().Left(Schema.JI_CustomsSecondUnitQtyMaxLength) : ZString.Empty;
		}
	}

	void DefaultThirdQuantityUOMFromTariffIfNeeded()
	{
		if (!IsUOMDefaultingSuspended)
		{
			JI_CustomsThirdUnitQty = SuggestedCustomsThirdQuantityUOM;
		}
	}

	protected override bool SetSecondQuantityFromEdiTariffsOwnRecord => false;

	protected override void SetSupplementaryCodeFromVatApplicabilitiesWhenUsingSingleVatField()
	{
		var vatApplicability = GetEffectiveVATApplicabilities().FirstOrDefault(x => x.ZX5_ZZF_NKTaxOrFeeCode == JI_ZZF_NKTaxType);
		if (vatApplicability != null)
		{
			new AdditionalSupplementaryCodeSetter(this).SetSupplementaryCodeFromVatApplicabilities(vatApplicability.ZX5_AdditionalCode);
		}
	}

	[ResourceStringData("84F3D91D-BC56-46AF-9C6A-C0462EB05708", Caption = "IVA")]
	public override ZString JI_ZZF_NKTaxType
	{
		get => base.JI_ZZF_NKTaxType;
		set
		{
			using (SuspendTaxTypeDefaulting())
			{
				var oldValue = JI_ZZF_NKTaxType;
				base.JI_ZZF_NKTaxType = value;
				if (!IsCopying && oldValue != JI_ZZF_NKTaxType)
				{
					SetSupplementaryCodeFromVatApplicabilitiesWhenUsingSingleVatField();
				}
			}
		}
	}

	public override ZDecimal JI_CustomsQuantity
	{
		get => base.JI_CustomsQuantity;
		set
		{
			var isUCC6AndIsExport = Declaration?.IsUCC6AndIsExport ?? false;
			var isTransitionPeriodAES30 = Declaration?.IsTransitionPeriodAES30 ?? false;

			if (isUCC6AndIsExport && isTransitionPeriodAES30)
			{
				base.JI_CustomsQuantity = value.Round(JobComInvoiceLineSchema.JI_NetWeight.Scale);
			}
			else
			{
				base.JI_CustomsQuantity = value;
			}
		}
	}

	[ResourceStringData("1EA6E3CC-6399-40DC-AA45-56AA3B0292B3", Caption = "[44] Third Qty (10YY)", MediumCaption = "Third Qty (10YY)", ShortCaption = "Third Qty")]
	public override ZDecimal JI_CustomsThirdQuantity
	{
		get => base.JI_CustomsThirdQuantity;
		set => base.JI_CustomsThirdQuantity = value;
	}

	[ResourceStringData("1F9D9473-EF3A-47AD-95A2-DDE1D60F4346", Caption = "Third Quantity Unit", MediumCaption = "Third Qty Unit", ShortCaption = "UQ")]
	public override ZString JI_CustomsThirdUnitQty
	{
		get => base.JI_CustomsThirdUnitQty;
		set => base.JI_CustomsThirdUnitQty = value;
	}

	[ResourceStringData("AED7DE1C-3BBE-4D62-B46A-DDFE1E482A3C", Caption = "Fourth Qty", FullDescription = "Fourth Quantity")]
	public override ZDecimal JI_CustomsFourthQuantity { get => base.JI_CustomsFourthQuantity; set => base.JI_CustomsFourthQuantity = value; }

	[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CountryOfExportList))]
	[ResourceStringData("01D3B2BD-63C0-457D-92F2-544F9FBE7F31", Caption = "Country of Export")]
	public override ZString JI_RN_NKCountryOfExport { get => base.JI_RN_NKCountryOfExport; set => base.JI_RN_NKCountryOfExport = value; }

	[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.SortedInvoiceList))]
	[ResourceStringData("86B41EA1-F466-4FA9-8F12-B1D76AFF45AA", Caption = "Invoice Number")]
	public override ZString JI_Calc_Invoice { get => base.JI_Calc_Invoice; set => base.JI_Calc_Invoice = value; }

	#endregion

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		ZG_SteelType = SteelTypeList.Codes._0;
	}

	protected override Customs.Business.InvoiceLinePackageValidation GetNewLinkPackValidationCore(BaseCusLinkPackage linkPackage) => new InvoiceLinePackageValidation(linkPackage, this);

	public override ZDateTime EffectiveAssessmentDate
	{
		get
		{
			var result = EntryInstruction?.CEI_DateForDuty ?? base.EffectiveAssessmentDate;
			return result.IsValid ? result : ZDateTime.Today;
		}
	}

	public ZString SuggestedCustomsThirdQuantityUOM
	{
		get
		{
			var thirdQuantityUom = ZString.Empty;
			if (UniversalDutyRate != null)
			{
				thirdQuantityUom = Factory.GetCachedValue(FormattableString.Invariant($"UniversalTariff_ThirdQuantityUOMs_{JI_Tariff}"), () =>
				{
					return UniversalReferenceHelper.GetThirdQuantityUOM(UniversalDutyRate);
				});
			}
			return thirdQuantityUom;
		}
	}

	public override ZString JI_Procedure
	{
		get => base.JI_Procedure;
		set
		{
			var oldValue = JI_Procedure;
			base.JI_Procedure = value;
			if (!IsCopying && JI_Procedure != oldValue)
			{
				SupportingDocumentsManager.AddCustomsDecisionsSupportingDocumentIfApplicable();
			}
		}
	}

	public override ZInt MaxNumberOfAdditionalProcedureCode => 99;

	public override string ProcedureMustBeEnteredForAdditionalProceduresSelectionErrorMessage => ValidationCaptions.InvoiceLine.ProcedureMustBeEnteredForAdditionalProceduresSelection;

	[ResourceStringData("A9F460AC-4D53-497F-83BF-9D338148D631", Caption = "Description")]
	[MaxLength(Schema.RemarksMaxLength)]
	public ZString Remarks
	{
		get => InvoiceLineRemark.Remarks;
		set
		{
			CheckMaximumLength(RemarksInfo, value);
			InvoiceLineRemark.Remarks = value;
			RemarksInfo.RefreshBinding();
			if (!IsValidationSuspended)
			{
				Validation.ValidateRemarks();
			}
		}
	}

	public ZPropertyInfo RemarksInfo => GetZPropertyInfo(Schema.Remarks);

	InvoiceLineRemark InvoiceLineRemark => invoiceLineRemark ?? (invoiceLineRemark = new InvoiceLineRemark(this));
	InvoiceLineRemark invoiceLineRemark;

	public ZString EffectiveRemarks => Factory.GetValue(ref effectiveRemarksCached, () => Remarks.Replace("\r\n", " ").Trim());
	CachedProperty<ZString> effectiveRemarksCached;

	public override void UpdateDetailsFromPivotOnPartChangeCore()
	{
		base.UpdateDetailsFromPivotOnPartChangeCore();

		var pivot = Pivot as CusClassPartPivot;
		if (pivot != null)
		{
			UpdatePortTaxRateIfNecessary(pivot);
		}
	}

	void UpdatePortTaxRateIfNecessary(CusClassPartPivot pivot)
	{
		if (Declaration.NeedsPortTax && ZG_PortTaxRate.IsEmpty)
		{
			var taxes = pivot.Taxes.Cast<ITTaxOnlyForPivot>().Select(x => x.Data);
			var portTaxRate = taxes.FirstOrDefault(x => x.IsPortTax)?.G4_PortTaxRate ?? ZString.Empty;
			if (!portTaxRate.IsEmpty)
			{
				ZG_PortTaxRate = portTaxRate;
			}
		}
	}

	public ZString VatRateDescription => AppliedTaxAndFee != null ? (ZString)ValidationCaptions.InvoiceLine.GetVatRateDescription(AppliedTaxAndFee.ZZF_Value) : ZString.Empty;

	public IEnumerable<ZString> GetEffectiveSupplementaryCodesRelatedToVATApplicabilities() => GetEffectiveVATApplicabilities().Select(x => x.ZX5_AdditionalCode);

	public void AddMissingSupportingDocuments(IEnumerable<MissingSupportingDocument> missingSupportingDocuments)
	{
		if (missingSupportingDocuments != null)
		{
			var actualSupportingDocuments = ActualSupportingDocumentCodeCollection;
			var missingSupportingDocumentsDistinctedByCode = missingSupportingDocuments.DistinctBy(x => x.Code);
			foreach (var missingSupportingDocument in missingSupportingDocumentsDistinctedByCode)
			{
				var missingSupportingDocumentCode = missingSupportingDocument.Code.SubstringSafe(0, 6);
				if (!actualSupportingDocuments.Any(supportingDocument => supportingDocument.CSI_Code == missingSupportingDocumentCode))
				{
					SupportingDocuments.AddNew().CSI_Code = missingSupportingDocumentCode;
				}
			}
		}
	}

	public IEnumerable<SupportingDocument> ActualSupportingDocumentCodeCollection => Factory.GetCached(ref actualSupportingDocumentCodeCollectionCached, GetActualSupportingDocumentCodeCollection);
	CachedProperty<IEnumerable<SupportingDocument>> actualSupportingDocumentCodeCollectionCached;

	IEnumerable<SupportingDocument> GetActualSupportingDocumentCodeCollection()
	{
		var actualSupportingDocuments = new List<SupportingDocument>();

		actualSupportingDocuments.AddRange(GetSupportingDocuments(SupportingDocuments));
		actualSupportingDocuments.AddRange(GetSupportingDocuments(Declaration?.SupportingDocuments));
		actualSupportingDocuments.AddRange(GetSupportingDocuments(InvoiceHeader?.SupportingDocuments));
		actualSupportingDocuments.AddRange(GetSupportingDocuments(EntryInstruction?.SupportingDocuments));

		return actualSupportingDocuments;

		IEnumerable<SupportingDocument> GetSupportingDocuments(SupportingDocumentCollection supportingDocuments)
			=> supportingDocuments?.Cast<SupportingDocument>() ?? Enumerable.Empty<SupportingDocument>();
	}

	public bool HasSameConditionSelectionCriteria(IZZConditionSelectionCriteria conditionSelectionCriteria)
	{
		return ConditionSelectionCriterias != null && ConditionSelectionCriterias.Length == 1 && conditionSelectionCriteria != null
				&& ConditionSelectionCriterias[0].DataGrouping == conditionSelectionCriteria.DataGrouping
				&& ConditionSelectionCriterias[0].TradeGroupCountry == conditionSelectionCriteria.TradeGroupCountry
				&& ConditionSelectionCriterias[0].PrimaryPreference == conditionSelectionCriteria.PrimaryPreference
				&& IEnumerableExtensions.ContainsSameElementsInAnyOrder(ConditionSelectionCriterias[0].AdditionalCodes, conditionSelectionCriteria.AdditionalCodes)
				&& ConditionSelectionCriterias[0].ConcessionOrder == conditionSelectionCriteria.ConcessionOrder;
	}

	protected override ZBool ShouldCheckMissingPreviousDocumentsCore => !(EntryInstruction?.IsPreliminaryDeclarationUnderCodeA ?? ZBool.False);

	public InvoiceLineSupportingDocumentsManager SupportingDocumentsManager => supportingDocumentsManager ?? (supportingDocumentsManager = new InvoiceLineSupportingDocumentsManager(this));
	InvoiceLineSupportingDocumentsManager supportingDocumentsManager;

	#region TaxType Defaulting Suspender

	public bool IsTaxTypeDefaultingSuspended => taxTypeDefaultingSuspenderIndex > 0;
	int taxTypeDefaultingSuspenderIndex;

#if DEBUG

	public
#endif

	IDisposable SuspendTaxTypeDefaulting()
	{
		taxTypeDefaultingSuspenderIndex++;
		return new DisposableAction(() => taxTypeDefaultingSuspenderIndex--);
	}

	#endregion

	[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.ItalyStateList))]
	[ResourceStringData("Enterprise.Customs.IT.Business.Declaration.ITJobComInvoiceLine|JI_StateOrRegionOfOrigin", Caption = "Origin State")]
	public override ZString JI_StateOrRegionOfOrigin
	{
		get => base.JI_StateOrRegionOfOrigin;
		set => base.JI_StateOrRegionOfOrigin = value;
	}

	#region SetDefaultValuesForOrigin

	public void SetDefaultValuesForOrigin()
	{
		if (ShouldSetDefaultValuesForOrigin)
		{
			var declarationCountry = Declaration.SupplierDocumentaryAddress.Country;
			if (declarationCountry != null)
			{
				JI_CountryOfOrigin = declarationCountry.Code;

				if (JI_CountryOfOrigin.Equals(Core.Constants.CountryCodes.Italy))
				{
					var originState = Declaration.SupplierDocumentaryAddress.E2_State;
					JI_StateOrRegionOfOrigin = originState.Substring(0, 3);
				}
			}
		}
	}

	bool ShouldSetDefaultValuesForOrigin => Declaration != null && Declaration.IsExport() && Declaration.SupplierDocumentaryAddress != null;

	#endregion

	#region AdditionalSupplementaryCodeSetter

	class AdditionalSupplementaryCodeSetter
	{
		public AdditionalSupplementaryCodeSetter(JobComInvoiceLine invoiceLine)
		{
			this.invoiceLine = invoiceLine;
		}

		readonly JobComInvoiceLine invoiceLine;

		IEnumerable<SupplementaryCode> AdditionalSupplementaryCodes => invoiceLine.AdditionalSupplementaryCodes.Cast<SupplementaryCode>().OrderBy(x => x.CY_Order);

		public void SetSupplementaryCodeFromVatApplicabilities(ZString additionalCode)
		{
			var availablePosition = GetAvailablePosition();
			var hasNoAviablePosition = availablePosition == 0;

			if (SupplementaryCodeHelper.IsQVatSupplementaryCode(additionalCode) && hasNoAviablePosition)
			{
				invoiceLine.AdditionalSupplementaryCodes.AddNew().CY_Code = additionalCode;
			}
			else
			{
				ZPropertyInfo targetSupplementaryCodeInfo;
				switch (availablePosition)
				{
					case 1:
						targetSupplementaryCodeInfo = invoiceLine.JI_SupplementaryCode1Info;
						break;

					case 2:
						targetSupplementaryCodeInfo = invoiceLine.JI_SupplementaryCode2Info;
						break;

					default:
						targetSupplementaryCodeInfo = AdditionalSupplementaryCodes.FirstOrDefault(x => x.CY_Order == availablePosition)?.CY_CodeInfo;
						break;
				}

				if (targetSupplementaryCodeInfo != null)
				{
					targetSupplementaryCodeInfo.Value = additionalCode;
					targetSupplementaryCodeInfo.RefreshBinding();
				}
			}
		}

		int GetAvailablePosition()
		{
			var orderedAdditionalCodes = new List<ZString>();
			orderedAdditionalCodes.Add(invoiceLine.JI_SupplementaryCode1);
			orderedAdditionalCodes.Add(invoiceLine.JI_SupplementaryCode2);
			orderedAdditionalCodes.AddRange(AdditionalSupplementaryCodes.OrderBy(x => x.CY_Order).Select(x => x.CY_Code));

			const int NoAvailablePositionIndexValue = -1;

			var availableAdditionalCodeIndex = IEnumerableExtensions.IndexOf(orderedAdditionalCodes, supplementaryCode => SupplementaryCodeHelper.IsQVatSupplementaryCode(supplementaryCode));
			if (availableAdditionalCodeIndex == NoAvailablePositionIndexValue)
			{
				availableAdditionalCodeIndex = IEnumerableExtensions.IndexOf(orderedAdditionalCodes, supplementaryCode => supplementaryCode.IsEmpty);
			}

			return (availableAdditionalCodeIndex > NoAvailablePositionIndexValue ? availableAdditionalCodeIndex : NoAvailablePositionIndexValue) + 1;
		}
	}

	#endregion

	#region Freight Charges

	public ZDecimal JI_Calc_ExtraEUFreightChargesAmount
	{
		get
		{
			if (cachedJI_Calc_ExtraEUFreightChargesAmount == null)
			{
				cachedJI_Calc_ExtraEUFreightChargesAmount = new CachedProperty<ZDecimal>(Factory, () => ValuationCalculator.GetExtraEUFreightChargesAmount());
			}
			return cachedJI_Calc_ExtraEUFreightChargesAmount.Value;
		}
	}
	CachedProperty<ZDecimal> cachedJI_Calc_ExtraEUFreightChargesAmount;

	public ZDecimal JI_Calc_EUFreightChargesAmount
	{
		get
		{
			if (cachedJI_Calc_EUFreightChargesAmount == null)
			{
				cachedJI_Calc_EUFreightChargesAmount = new CachedProperty<ZDecimal>(Factory, () => ValuationCalculator.GetEUFreightChargesAmount());
			}
			return cachedJI_Calc_EUFreightChargesAmount.Value;
		}
	}
	CachedProperty<ZDecimal> cachedJI_Calc_EUFreightChargesAmount;

	public ZDecimal JI_Calc_DomesticFreightChargesAmount
	{
		get
		{
			if (cachedJI_Calc_DomesticFreightChargesAmount == null)
			{
				cachedJI_Calc_DomesticFreightChargesAmount = new CachedProperty<ZDecimal>(Factory, () => ValuationCalculator.GetDomesticFreightChargesAmount());
			}
			return cachedJI_Calc_DomesticFreightChargesAmount.Value;
		}
	}
	CachedProperty<ZDecimal> cachedJI_Calc_DomesticFreightChargesAmount;

	#endregion

	public bool IsNonTurkishImportWithTurkishDispatch => Factory.GetCached(ref isNonTurkishImportWithTurkishDispatch, GetIsNonTurkishImportWithTurkishDispatch);
	CachedProperty<bool> isNonTurkishImportWithTurkishDispatch;

	bool GetIsNonTurkishImportWithTurkishDispatch()
	{
		return IsImport
			&& JI_CountryOfOrigin != Core.Constants.CountryCodes.Turkey
			&& Declaration is JobDeclaration declaration
			&& declaration.JE_GoodsOrigin == Core.Constants.CountryCodes.Turkey;
	}

	public bool IsEligibleForTurkeyCustomsDutyExempt => Factory.GetCached(ref isEligibleForTurkeyCustomsDutyExempt, GetIsEligibleForTurkeyCustomsDutyExempt);
	CachedProperty<bool> isEligibleForTurkeyCustomsDutyExempt;

	bool GetIsEligibleForTurkeyCustomsDutyExempt()
	{
		return IsNonTurkishImportWithTurkishDispatch
			&& JI_PrimaryPreference == UniversalReferenceConstants.RefCusPreferences.NonImpositionOfCustomsDuties;
	}

	protected override IZZRateSelectionCriteria GetDutyRateSelectionCriteriaCore()
	{
		return IsEligibleForTurkeyCustomsDutyExempt
			? new TurkeyDutyRateSelectionCriteria(this)
			: base.GetDutyRateSelectionCriteriaCore();
	}

	protected override IEnumerable<IZZRateSelectionCriteria> GetNationalRateSelectionCriteriaCore()
	{
		return RefCusRateType.Loader.GetRateTypesByDataGrouping(Factory, Core.Constants.CountryCodes.Italy)
			.Select(x => new RateSelectionCriteria<JobComInvoiceLine>(this, x.ZZR_RateType, ZString.Empty))
			.ToImmutableArray();
	}

	protected override void SetDefaultTaxOrFeeCode()
	{
		if (IsImport)
		{
			base.SetDefaultTaxOrFeeCode();
		}
	}

	public void EnableOrDisableAdditionalInfosMaxCountValidation() => EnableOrDisableAdditionalInfosMaxCountValidation(AdditionalInfos);

	void EnableOrDisableAdditionalInfosMaxCountValidation(AdditionalInfoCollection additionalInfos)
	{
		additionalInfos.MaxCountValidationDisable();
		var declaration = Declaration;
		if (declaration == null || declaration.IsUCC6AndIsExport)
		{
			return;
		}

		if (declaration.IsExport)
		{
			const int collectionMaxCountForEXP = 1;
			additionalInfos.EnableMaxCountValidation(collectionMaxCountForEXP, warnAtHalfway: false, CargoWise.ComponentModel.NotificationType.Error, ValidationCaptions.AdditionalInfo.OnlyOneLineOfAdditionalInfoIsAllowed);
		}
		else if (declaration.IsImport)
		{
			const int collectionMaxCountForIMP = 99;
			additionalInfos.EnableMaxCountValidation(collectionMaxCountForIMP, warnAtHalfway: false, CargoWise.ComponentModel.NotificationType.Error, ValidationCaptions.AdditionalInfo.Only99LinesOfAdditionalInfoAreAllowed);
		}
	}

	protected override List<EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo> EffectiveAdditionalInfosCore()
	{
		var result = new List<EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo>();
		result.AddRange(AdditionalInfos);
		result.AddRange(InvoiceHeader?.AdditionalInfos ?? Enumerable.Empty<EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo>());
		return result;
	}

	public new ICusAuthorizationUsageCollection<CusAuthorizationUsage, JobComInvoiceLine> CusAuthorizationUsages => (CusAuthorizationUsageCollection<CusAuthorizationUsage, JobComInvoiceLine>)base.CusAuthorizationUsages;
	protected override ICusAuthorizationUsageCollection<EU.Business.CusAuthorizationUsage, EU.Business.Declaration.JobComInvoiceLine> GetCusAuthorizationUsages() => new CusAuthorizationUsageCollection<CusAuthorizationUsage, JobComInvoiceLine>(this, Factory);

	public new AdditionalProcedureCodeCollection AdditionalProcedureCodes => (AdditionalProcedureCodeCollection)base.AdditionalProcedureCodes;
	protected override EU.Business.AdditionalProcedureCodeCollection GetAdditionalProcedureCodeCollection() => new AdditionalProcedureCodeCollection(this);

	protected override ZBool IsBondedWhsQuantityVisibleCore => IsIntoOrOutOfRegimeProcedure || (CusProcedure?.IsIntoVATWarehouse() ?? false);

	protected override ZBool IsPreviousEntryNumberVisibleCore => HasOutOfRegimeProcedure;

	protected override ZValidation GetBuyerJobDocAddressAdditionalValidation(JobDocAddress buyerJobDocAddress) => GetInvoiceLineTraderJobDocAddressValidation(buyerJobDocAddress);

	protected override ZValidation GetSellerJobDocAddressAdditionalValidation(JobDocAddress sellerJobDocAddress) => GetInvoiceLineTraderJobDocAddressValidation(sellerJobDocAddress);

	ZValidation GetInvoiceLineTraderJobDocAddressValidation(JobDocAddress traderJobDocAddress) => (Declaration?.IsImport ?? false) ? new InvoiceLineTraderJobDocAddressValidation(traderJobDocAddress, Declaration) : null;
}
