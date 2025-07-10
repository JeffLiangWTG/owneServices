using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.CH.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.Business;

public class CusLineTariffDetail : AutoCHCusLineTariffDetail, ISupportMultipleResourceStringData
{
	public new class Schema : AutoCHCusLineTariffDetail.Schema
	{
		public const string Description = nameof(CusLineTariffDetail.Description);
		public const int BZ_Qty_DecimalPlaces_ADT = 3;
		public const int BZ_Qty_DecimalPlaces_FEE = 1;
	}

	public CusLineTariffDetail(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	[ReadOnly(true)]
	[ResourceStringData("Enterprise.Customs.CH.Business.CusLineTariffDetail|BZ_TaxType|ADT", Caption = "Type", FullDescription = "Additional Tax Type", MultipleKey = UniversalReferenceConstants.RateTypes.AdditionalTaxes)]
	public override ZString BZ_TaxType
	{
		get => base.BZ_TaxType;
		set
		{
			var oldValue = BZ_TaxType;
			base.BZ_TaxType = value;
			if (!IsCopying && oldValue != BZ_TaxType)
			{
				SetAdditionalTaxQuantity();
				SetAdditionalTaxBaseValue();
			}
		}
	}

	[List(nameof(Lookups) + "." + nameof(CusLineTariffDetailLookups.TariffList))]
	[ResourceStringData("Enterprise.Customs.CH.Business.CusLineTariffDetail|BZ_Tariff|ADT", Caption = "Code", FullDescription = "Additional Tax Code", MultipleKey = RateTypes.AdditionalTaxes)]
	[ResourceStringData("Enterprise.Customs.CH.Business.CusLineTariffDetail|BZ_Tariff|FEE", Caption = "Type", FullDescription = "Fee Type", MultipleKey = RateTypes.AdditionalFees)]
	public override ZString BZ_Tariff
	{
		get => base.BZ_Tariff;
		set
		{
			var oldValue = BZ_Tariff;
			base.BZ_Tariff = value;
			if (!IsCopying && oldValue != BZ_Tariff)
			{
				DefaultValueFromTariff();
				SetDefaultValuesForBZ_Qty1();
				CalculateAdditionalFeeValue();
				CalculateAdditionalTaxValue();
				if (!IsValidationSuspended)
				{
					Validation.ValidateBZ_ManualRate();
				}
			}
		}
	}

	public ZString AssessmentCode => UniversalTariff?.GetAttribute(TariffAttributes.AssessmentCode)?.ZZ3_Value ?? ZString.Empty;

	[ReadOnlyMember(nameof(BZ_Qty1_ReadOnly))]
	[ResourceStringData("Enterprise.Customs.CH.Business.CusLineTariffDetail|BZ_Qty1|ADT", Caption = "Quantity", FullDescription = "Additional Tax Quantity", MultipleKey = RateTypes.AdditionalTaxes)]
	[ResourceStringData("Enterprise.Customs.CH.Business.CusLineTariffDetail|BZ_Qty1|FEE", Caption = "Quantity", FullDescription = "Fee Quantity", MultipleKey = RateTypes.AdditionalFees)]
	[DecimalPlaces(nameof(BZ_Qty1_DecimalPlaces))]
	public override ZDecimal BZ_Qty1
	{
		get => base.BZ_Qty1;
		set
		{
			var oldValue = BZ_Qty1;
			base.BZ_Qty1 = value;
			if (!IsCopying && oldValue != BZ_Qty1)
			{
				CalculateAdditionalFeeValue();
				CalculateAdditionalTaxValue();
			}
		}
	}

	public bool BZ_Qty1_ReadOnly => IsAdditionalTax && BZ_UQ1.IsEmpty;

	public int BZ_Qty1_DecimalPlaces => IsAdditionalFee ? Schema.BZ_Qty_DecimalPlaces_FEE : Schema.BZ_Qty_DecimalPlaces_ADT;

	[ReadOnly(true)]
	[ResourceStringData("Enterprise.Customs.CH.Business.CusLineTariffDetail|BZ_UQ1", Caption = "UOM", FullDescription = "Unit Of Measurement")]
	public override ZString BZ_UQ1
	{
		get => base.BZ_UQ1;
		set
		{
			var oldValue = BZ_UQ1;
			base.BZ_UQ1 = value;
			if (!IsCopying && oldValue != BZ_UQ1)
			{
				CalculateAdditionalTaxValue();
			}
			if (!IsValidationSuspended)
			{
				Validation.ValidateBZ_Qty1();
			}
		}
	}

	[ReadOnlyMember(nameof(BZ_AlcoholPercentage_ReadOnly))]
	[ResourceStringData("Enterprise.Customs.CH.Business.CusLineTariffDetail|BZ_AlcoholPercentage", Caption = "Alcohol Percentage", FullDescription = "Alcohol Percentage for Spirit Tax")]
	[DecimalPlaces(1)]
	public override ZDecimal BZ_AlcoholPercentage
	{
		get => base.BZ_AlcoholPercentage;
		set
		{
			var oldValue = BZ_AlcoholPercentage;
			base.BZ_AlcoholPercentage = value;
			if (!IsCopying && oldValue != BZ_AlcoholPercentage)
			{
				SetAdditionalTaxQuantity();
			}
		}
	}

	public bool BZ_AlcoholPercentage_ReadOnly => IsAdditionalTax && BZ_TaxType != AdditionalTaxesTypes.Spirits;

	[ReadOnly(true)]
	[ResourceStringData("Enterprise.Customs.CH.Business.CusLineTariffDetail|BZ_BaseValue", Caption = "Base Value", FullDescription = "Value for ad-valorem tax (ex. 660 additional tax)")]
	public override ZDecimal BZ_BaseValue
	{
		get => base.BZ_BaseValue;
		set => base.BZ_BaseValue = value;
	}

	[ReadOnlyMember(nameof(BZ_ManualRate_ReadOnly))]
	[ResourceStringData("Enterprise.Customs.CH.Business.CusLineTariffDetail|BZ_ManualRate|ADT", Caption = "Manual Rate", FullDescription = "Manual Rate (ex. Tobacco additional tax rate)", MultipleKey = RateTypes.AdditionalTaxes)]
	[ResourceStringData("Enterprise.Customs.CH.Business.CusLineTariffDetail|BZ_ManualRate|FEE", Caption = "Rate", FullDescription = "Fee Rate", MultipleKey = RateTypes.AdditionalFees)]
	public override ZDecimal BZ_ManualRate
	{
		get => base.BZ_ManualRate;
		set
		{
			var oldValue = BZ_ManualRate;
			base.BZ_ManualRate = value;
			if (!IsCopying && oldValue != BZ_ManualRate)
			{
				CalculateAdditionalTaxValue();
				CalculateAdditionalFeeValue();
			}
		}
	}

	public bool BZ_ManualRate_ReadOnly => IsAdditionalTax && !(IsAdditionalTaxApplied && (IsAdditionalTaxRateFormulaEmpty() || IsForcedManualRate));

	[ReadOnly(true)]
	[ResourceStringData("Enterprise.Customs.CH.Business.CusLineTariffDetail|BZ_Value|ADT", Caption = "Tax Amount", FullDescription = "Calculated Additional Tax Amount", MultipleKey = RateTypes.AdditionalTaxes)]
	[ResourceStringData("Enterprise.Customs.CH.Business.CusLineTariffDetail|BZ_Value|FEE", Caption = "Fee Amount", FullDescription = "Calculated Additional Fee Amount", MultipleKey = RateTypes.AdditionalFees)]
	public override ZDecimal BZ_Value
	{
		get => base.BZ_Value;
		set => base.BZ_Value = value;
	}

	public bool IsAdditionalFee => BZ_Type == RateTypes.AdditionalFees;

	public bool IsAdditionalTax => BZ_Type == RateTypes.AdditionalTaxes;

	public bool IsAdditionalTaxApplied => !BZ_Tariff.IsEmpty && !BZ_Tariff.EndsWith("-000");

	public bool IsSOTAAdditionalTax => IsAdditionalTax && (BZ_Tariff == UniversalReferenceConstants.AdditionalTaxesTariffs.Tariff465002 || BZ_Tariff == UniversalReferenceConstants.AdditionalTaxesTariffs.Tariff465202);

	public bool IsPreventionAdditionalTax => IsAdditionalTax && (BZ_Tariff == UniversalReferenceConstants.AdditionalTaxesTariffs.Tariff470002 || BZ_Tariff == UniversalReferenceConstants.AdditionalTaxesTariffs.Tariff470202);

	public bool IsQuantityBasedAdditionalTax => IsAdditionalTax && BZ_Tariff.StartsWith(AdditionalTaxesTariffs.TariffType_QuantityBased);

	public bool IsForcedManualRate => AdditionalTaxesTariffs.IsForcedManualRate(BZ_Tariff);

	[ResourceStringData("Enterprise.Customs.CH.Business.CusLineTariffDetail|Description", Caption = "Description", FullDescription = "Fee Type Description")]
	public ZString Description => Lookups.TariffList.GetDescriptionFromCode(BZ_Tariff);

	public ZPropertyInfo DescriptionInfo => GetZPropertyInfo(nameof(Description));

	public new CusLineTariffDetailLookups Lookups => (CusLineTariffDetailLookups)base.Lookups;

	public new JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)base.InvoiceLine;

	public ZString TaxTariffKey => IsAdditionalTax ? BZ_Tariff.SubstringSafe(4, 3) : ZString.Empty;

	protected override Customs.Business.CusLineTariffDetailLookups GetNewLookups() => new CusLineTariffDetailLookups(this);

	protected override Customs.Business.CusLineTariffDetailValidation GetNewValidation() => new CusLineTariffDetailValidation(this);

	IReadOnlyList<string> ISupportMultipleResourceStringData.MultipleKeysToUse => new string[] { BZ_Type };

	internal void CalculateAdditionalFeeValue()
	{
		if (IsAdditionalFee)
		{
			var value = BZ_Qty1 * BZ_ManualRate;
			if (BZ_Tariff == FeeRateCodes.CustomsReliefControlTax && value < MinimumReliefFeeValue)
			{
				value = MinimumReliefFeeValue;
			}
			BZ_Value = value;
		}
	}

	readonly static ZDecimal MinimumReliefFeeValue = 7m;

	void DefaultValueFromTariff()
	{
		if (IsAdditionalTax)
		{
			fAdditionalTaxRateFormula = null;
			SetAdditionalTaxUQ();
			if (BZ_ManualRate_ReadOnly)
			{
				BZ_ManualRate = 0;
			}
		}
	}

	void SetAdditionalTaxUQ()
	{
		var tariffUOM = UniversalTariff?.GetSpecificUOM(UOMTypeList.Codes.CU1) ?? ZString.Empty;

		if (tariffUOM.IsEmpty && BZ_TaxType == AdditionalTaxesTypes.Tobacco)
		{
			var tariffCodeGroup = InvoiceLine.TariffCodeGroup;
			if (tariffCodeGroup == ImportTariffCodeGroups.Cigarets)
			{
				tariffUOM = SwissCustomsConstants.MeasurementUnits.UnitOfOneThousandUOM;
			}
			else if (tariffCodeGroup == ImportTariffCodeGroups.SmokingTobacco || tariffCodeGroup == ImportTariffCodeGroups.ProductsContainingTobacco)
			{
				tariffUOM = SwissCustomsConstants.MeasurementUnits.GrossWeightUOM;
			}
		}
		BZ_UQ1 = tariffUOM;
	}

	public static IReadOnlyCollection<string> R148List => new string[] { AssessmentCodeValues.PerPiece, AssessmentCodeValues.PerKgNetMass, "14", "15", "16", AssessmentCodeValues.PerHectolitre, "18", "19", "20", "21", AssessmentCodeValues.Per1000Pieces, AssessmentCodeValues.Per1000LitresAt15DegreesCelsius };

	void SetDefaultValuesForBZ_Qty1()
	{
		if (BZ_Qty1.IsEmpty && Parent is JobComInvoiceLine invoiceLine)
		{
			if (!AdditionalTaxesTypes.ExcludedTaxTypes.Contains(BZ_TaxType.ToString()))
			{
				if (AssessmentCode == AssessmentCodeValues.Per100kgGrossMass)
				{
					BZ_Qty1 = invoiceLine.JI_CustomsQuantity;
				}
				else if (AssessmentCode == AssessmentCodeValues.Per1000kgNetMass)
				{
					BZ_Qty1 = invoiceLine.JI_CustomsSecondQuantity;
				}
				else if (R148List.Contains(AssessmentCode.ToString()))
				{
					BZ_Qty1 = invoiceLine.JI_CustomsThirdQuantity;
				}
			}
			else if (BZ_TaxType == AdditionalTaxesTypes.Beer)
			{
				BZ_Qty1 = invoiceLine.JI_CustomsThirdQuantity / 100;
			}
		}
	}

	public void CalculateAdditionalTaxValue()
	{
		if (!IsAdditionalTax)
		{
			return;
		}

		if (IsAdditionalTaxRateFormulaEmpty() || IsForcedManualRate)
		{
			BZ_Value = BZ_ManualRate * BZ_Qty1;

			if (AdditionalTaxesTariffs.IsTobaccoTariff450002Or450202(BZ_Tariff))
			{
				BZ_Value /= 1000;
			}
		}
		else
		{
			BZ_Value = DutyCalculatorHelper.Calculate(UniversalRateCalcData, AdditionalTaxRateFormula, 2, ZString.Empty, ZDecimal.Zero, false, false);
		}
	}

	public void SetAdditionalTaxQuantity()
	{
		if (!IsAdditionalTax)
		{
			return;
		}

		if (BZ_TaxType == AdditionalTaxesTypes.CitesFlora)
		{
			BZ_Qty1 = 1;
		}
		else if (BZ_TaxType == AdditionalTaxesTypes.Spirits)
		{
			BZ_Qty1 = InvoiceLine.JI_CustomsThirdQuantity * BZ_AlcoholPercentage / 100;
		}
		else if (BZ_Qty1_ReadOnly)
		{
			BZ_Qty1 = 0;
		}
	}

	public void SetAdditionalTaxBaseValue()
	{
		if (IsAdditionalTax && BZ_TaxType == AdditionalTaxesTypes.MotorVehicle)
		{
			BZ_BaseValue = UniversalRateCalcData.ValueForDuty + UniversalRateCalcData.CountrySpecificValueList[UniversalReferenceConstants.FormulaPlaceholder.DutyCalculation];
		}
	}

	CusLineTariffDetailRateCalcData UniversalRateCalcData => Factory.GetCached(ref universalRateCalcData, () => new CusLineTariffDetailRateCalcData(this));
	CachedProperty<CusLineTariffDetailRateCalcData> universalRateCalcData;

	ZString AdditionalTaxRateFormula => fAdditionalTaxRateFormula ?? (fAdditionalTaxRateFormula = UniversalTariff?.GetApplicableRate(RateSelectionCriteria)?.ZZ2_RateFormula ?? ZString.Empty);
	string fAdditionalTaxRateFormula;

	public bool IsAdditionalTaxRateFormulaEmpty() => (AdditionalTaxRateFormula.IsEmpty || AdditionalTaxRateFormula == FormulaPlaceholder.FreeRateFormula) && UniversalTariff?.UnitsOfMeasure?.Count == 0;

	public bool ShouldCalculateAdditionalTaxValueAfterMerge => AdditionalTaxRateFormula.Contains(FormulaPlaceholder.DutyCalculation);

	public IZZRateSelectionCriteria RateSelectionCriteria => (rateSelectionCriteria ?? (rateSelectionCriteria = new CachedProperty<IZZRateSelectionCriteria>(base.Factory, GetRateSelectionCriteriaCore))).Value;
	CachedProperty<IZZRateSelectionCriteria> rateSelectionCriteria;

	IZZRateSelectionCriteria GetRateSelectionCriteriaCore() => new AdditionalTaxSelectionCriteria(this);
}
