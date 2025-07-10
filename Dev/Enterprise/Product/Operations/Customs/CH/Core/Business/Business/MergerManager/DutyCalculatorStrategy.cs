using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Universal;
using static Enterprise.Customs.CH.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.Business;

public class DutyCalculatorStrategy : Customs.Business.DutyCalculatorStrategy
{
	public DutyCalculatorStrategy(JobDeclaration declaration) : base(declaration)
	{
	}

	protected override bool ShouldCalculateDuties => true;

	protected override void CalculateDutiesForRates(Customs.Business.CusEntryLine entryLine, EntryLineUniversalRate entryLineUniversalData, IEnumerable<RateView> applicableRates)
	{
		var invoiceLine = (entryLine as CusEntryLine)?.RandomLine;

		foreach (var rate in applicableRates)
		{
			if (invoiceLine?.JI_RateOverride ?? false)
			{
				CalculateDutiesForOverriddenRate(entryLine, invoiceLine, entryLineUniversalData, rate);
			}
			else
			{
				CalculateDutiesForRate(entryLine, entryLineUniversalData, rate);
			}
		}
	}

	void CalculateDutiesForOverriddenRate(Customs.Business.CusEntryLine entryLine, JobComInvoiceLine invoiceLine, EntryLineUniversalRate entryLineUniversalData, RateView rate)
	{
		entryLineUniversalData.CustomsValueFormula = rate?.CusRateType?.ZZR_CustomsValueFormula ?? ZString.Empty;
		var rateFormula = OverrideRate(rate.ZZ2_RateFormula, invoiceLine.JI_OverriddenRate.ToString());
		var dutyAmount = Calculate(entryLine, entryLineUniversalData, rateFormula);
		entryLineUniversalData.CountrySpecificValueList.AddNewKeyOrAccumulateValue(GetEntryLineFeeType(rate), dutyAmount);
	}

	ZString OverrideRate(ZString rateFormula, ZString overriddenRate)
	{
		var formulaParts = rateFormula.Trim().Split(' ');

		foreach (var part in formulaParts)
		{
			if (decimal.TryParse(part, out var value))
			{
				return rateFormula.Replace(part, overriddenRate);
			}
		}

		return rateFormula;
	}

	protected override void UpdateEntryLineFees(Customs.Business.CusEntryLine entryLine, EntryLineUniversalRate entryLineUniversalData)
	{
		var invoiceLine = (entryLine as CusEntryLine)?.RandomLine;

		foreach (KeyValuePair<string, decimal> countrySpecificValue in entryLineUniversalData.CountrySpecificValueList)
		{
			if (countrySpecificValue.Value != 0m)
			{
				var addedOrUpdatedFee = entryLine.Fees.AddOrUpdate(countrySpecificValue.Key, countrySpecificValue.Value);

				addedOrUpdatedFee.CF_RateOverrideReasonCode = invoiceLine.JI_RateOverride ? RateOverrideReasonCode.Overridden : ZString.Empty;
				addedOrUpdatedFee.CF_Rate = invoiceLine.JI_RateOverride ? invoiceLine.JI_OverriddenRate : GetRateFromCalculationFormula();
				addedOrUpdatedFee.CF_BaseValue = GetBaseValueFromUOMValueList();
			}
		}

		ZDecimal GetRateFromCalculationFormula()
		{
			var formulaParts = invoiceLine.UniversalDutyRate.ZZ2_RateFormula.Trim().Split(' ');

			foreach (var part in formulaParts)
			{
				if (decimal.TryParse(part, out var value))
				{
					return value;
				}
			}

			return ZDecimal.Zero;
		}

		ZDecimal GetBaseValueFromUOMValueList()
		{
			var formulaParts = invoiceLine.UniversalDutyRate.ZZ2_RateFormula.Trim().Split(' ');
			var uom = ZString.Empty;

			foreach (var part in formulaParts)
			{
				if (part.StartsWith("[") && part.EndsWith("]"))
				{
					uom = part.SubstringSafe(1, part.Length - 2);
				}
			}

			if (!uom.IsEmpty)
			{
				return entryLineUniversalData.UnitOfMeasureValueList.GetValue(uom);
			}

			return entryLineUniversalData.CustomsValue;
		}
	}
}
