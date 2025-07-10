using System.Collections.Immutable;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.IT.Business.UniversalReferenceConstants;
using EURefCusRateCodes = Enterprise.Customs.EU.Business.UniversalReferenceConstants.RefCusRateCodes;
using EURefCusRateTypes = Enterprise.Customs.Business.UniversalReferenceConstants.RefCusRateTypes;

namespace Enterprise.Customs.IT.Business.Declaration;

public sealed class CusEntryLineFee : EU.Business.Declaration.CusEntryLineFee, Integration.Customs.IT.ICusEntryLineFee, IFee
{
	public CusEntryLineFee(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new CusEntryLineFeeValidation Validation => (CusEntryLineFeeValidation)base.Validation;

	protected override Customs.Business.CusEntryLineFeeValidation GetNewValidation() => new CusEntryLineFeeValidation(this);

	public new CusEntryLineFeeLookups Lookups => (CusEntryLineFeeLookups)base.Lookups;

	protected override Customs.Business.CusEntryLineFeeLookups GetNewLookups() => new CusEntryLineFeeLookups(this);

	public new CusEntryLine EntryLine => (CusEntryLine)base.EntryLine;

	protected override bool IncludeForVatCalculationCore => IsVatable() && (EntryLine.Declaration.IsImport || CF_MethodOfPayment != DutyMethodOfPayment.SecurityDepositDeferredPaymentR);

	public ZBool IsDuty => CF_ChargeType.StartsWith(RefCusRateCodes.DutyChargeTypeStartingCode);

	public ZBool IsSanMarinoDuty => CF_ChargeType.StartsWith(RefCusRateCodes.DutyForSanMarinoChargeTypeStartingCode);

	public ZBool IsTemporaryAntiDumping => CF_ChargeType == RefCusRateCodes.TemporaryAntiDumpingDuty;

	public ZBool IsTemporaryCountervailing => CF_ChargeType == RefCusRateCodes.TemportaryCountervailingDuty;

	public bool IsVat => CF_ChargeType == EURefCusRateCodes.Vat || CF_ChargeType == RefCusRateCodes.ItalianCustomsVatCode;

	public bool IsVatExemption => CF_ChargeType == RefCusRateCodes.ItalianCustomsVatExemptionCode406 || CF_ChargeType == RefCusRateCodes.ItalianCustomsVatExemptionCode407;

	public bool IsPortTax => Factory.GetValue(ref isPortTaxCached, () =>
	{
		var valuationDate = EntryLine?.Header?.EffectiveValuationDate ?? ZDateTime.Empty;
		var portTaxRateCodesLoader = new PortTaxRateCodesLoader(Factory, valuationDate);
		var portTaxRateCodeResolver = new PortTaxRateCodeResolver(portTaxRateCodesLoader);
		return portTaxRateCodeResolver.IsPortTaxRateCode(CF_ChargeType);
	});

	CachedProperty<bool> isPortTaxCached;

	public bool IsCarTax => CF_ChargeType == RefCusRateCodes.CarTax423;

	public bool IsMiscellaneousContingentRevenueConcerningTax => CF_ChargeType == RefCusRateCodes.MiscellaneousContingentRevenueConcerningTax430;

	public bool IsRecoveryOfCourtCostsTax => CF_ChargeType == RefCusRateCodes.RecoveryOfCourtCostsTax445;

	protected override ZString GetDefaultMethodOfPaymentValue()
	{
		var procedureAttributeBasedCalculator = ProcedureAttributeBasedMethodOfPaymentCalculator.GetNew(this);
		var defaultValueFromProcedureAttribute = procedureAttributeBasedCalculator.GetDefaultValue();
		if (!defaultValueFromProcedureAttribute.IsEmpty)
		{
			return defaultValueFromProcedureAttribute;
		}

		if (IsAntiDumpingOrCountervailingDuty)
		{
			return DutyMethodOfPayment.SecurityDepositDeferredPaymentR;
		}

		var declaration = EntryLine?.Declaration;
		if (declaration == null)
		{
			return DutyMethodOfPayment.DeferredPaymentVatProcedureG;
		}

		if (declaration.JE_DefermentAccountNumber.IsEmpty)
		{
			return DutyMethodOfPayment.ImmediatePaymentInCashA;
		}

		if (declaration.IsDefermentAccountNumberEqualToDatCode())
		{
			return declaration.IsImport
				? DutyMethodOfPayment.OthersD
				: DutyMethodOfPayment.AgentGeneralGuaranteeAccountT;
		}

		if (IsDuty || IsSanMarinoDuty)
		{
			return declaration.IsUCC6
				? DutyMethodOfPayment.DeferredPaymentE
				: DutyMethodOfPayment.DeferredPaymentCustomsProcedureF;
		}

		return DutyMethodOfPayment.DeferredPaymentVatProcedureG;
	}

	protected override bool GetIsNationalIndirectTaxationFee() => ChargeTypeIsContainedInRateCollection(new ZString[] { EURefCusRateTypes.SecurityDeposit });

	protected override bool AllowNegativeAmountCore => IsVatExemption;

	public override ZString CF_RateOverrideReasonCode
	{
		get => base.CF_RateOverrideReasonCode;
		set
		{
			var oldValue = base.CF_RateOverrideReasonCode;
			base.CF_RateOverrideReasonCode = value;
			if (oldValue != value)
			{
				SetBaseValueToZeroIfApplicable();
			}
		}
	}

	public override bool TotalAmountReadOnly
		=> IsActionExclude
			? !CF_ChargeType.IsEmpty
			: base.TotalAmountReadOnly;

	[ReadOnlyMember(nameof(IsActionBlankOrIsActionExcludeWithChargeType))]
	public override ZString CF_ChargeType { get => base.CF_ChargeType; set => base.CF_ChargeType = value; }

	[ReadOnlyMember(nameof(IsActionBlankOrIsActionExcludeWithChargeType))]
	public override ZString CF_MethodOfCalculation => base.CF_MethodOfCalculation;

	[ReadOnlyMember(nameof(IsActionBlankOrIsActionExcludeWithChargeType))]
	public override ZDecimal CF_Rate => base.CF_Rate;

	[ReadOnlyMember(nameof(IsActionBlankOrIsActionExcludeWithChargeType))]
	public override ZString CF_MethodOfPayment { get => base.CF_MethodOfPayment; set => base.CF_MethodOfPayment = value; }

	[ReadOnlyMember(nameof(IsActionBlankOrIsActionExcludeWithChargeType))]
	public override ZDecimal CF_BaseValue => base.CF_BaseValue;

	#region Implementation

	bool IsVatable() => !ChargeTypeIsContainedInRateCollection(notNationalVatableRateTypes.ToArray());

	ZString GetDefaultDataGroupingCode() => EntryLine.Declaration?.GetDefaultDataGroupingCode() ?? ZString.Empty;

	void SetBaseValueToZeroIfApplicable()
	{
		if (IsActionExclude)
		{
			CF_BaseValue = 0;
		}
	}

	readonly ImmutableArray<ZString> notNationalVatableRateTypes = new ZString[]
	{
		EURefCusRateTypes.Interest,
		RefCusRateTypes.MiscellaneousNotVatableImportExport,
		RefCusRateTypes.MiscellaneousNotVatable,
	}.ToImmutableArray();

	bool ChargeTypeIsContainedInRateCollection(ZString[] rateTypes)
	{
		var dataGrouping = GetDefaultDataGroupingCode();
		if (!dataGrouping.IsEmpty)
		{
			var rateCodes = Factory.GetCachedRatesByType(dataGrouping, rateTypes.ToArray()).Select(x => x.ZY1_RateCode);
			return rateCodes.Contains(CF_ChargeType);
		}
		return false;
	}

	bool IsActionBlankOrIsActionExcludeWithChargeType => IsActionBlank || (IsActionExclude && !CF_ChargeType.IsEmpty);

	#endregion

	#region IFee Members

	ZString IFee.ChargeType => CF_ChargeType;

	ZDecimal IFee.BaseValue => Utilities.Round(CF_BaseValue, CF_BaseValueDecimalPlaces);

	ZDecimal IFee.Rate => CF_Rate;

	ZString IFee.MethodOfCalculation => CF_MethodOfCalculation;

	ZDecimal IFee.Amount => CF_ChargeAmount;

	ZString IFee.MethodOfPayment => CF_MethodOfPayment;

	#endregion

	protected override IFeeRounder GetNewChargeAmountRounder() => new TwoDigitCustomChargeAmountRounder();

	protected override EU.Business.Declaration.ChargeAmountRefresher GetNewChargeAmountRefresher() => new ChargeAmountRefresher(this);

	internal bool IsActionExclude => CF_RateOverrideReasonCode == RateOverrideReasonList.Codes.Exclude;

	ZBool IsAntiDumpingOrCountervailingDuty => IsTemporaryAntiDumping || IsTemporaryCountervailing;
}
