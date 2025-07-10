using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business
{
	public static class InsuranceRuleCalculation
	{
		public static string GetFormulaFromRates(CusCalculationRuleRateCollection rates)
		{
			rates.Sort(nameof(CusCalculationRuleRate.ValueFrom));
			var collectionCount = rates.Count;
			if (collectionCount > 0)
			{
				var previousFormula = GetFormulaFromOneRate(rates[0]);
				var currentFormula = previousFormula;
				for (var i = 1; i < collectionCount; i++)
				{
					var rate = rates[i];
					var rateString = GetFormulaFromOneRate(rate);
					currentFormula = $"IF(VFD >= {rate.ValueFrom}, {rateString}, {previousFormula})";
					previousFormula = currentFormula;
				}

				return currentFormula;
			}
			else
			{
				return string.Empty;
			}

			string GetFormulaFromOneRate(CusCalculationRuleRate rate)
			{
				return rate.Uplift.IsEmpty ? $"{rate.FlatRate}" : $"{rate.Uplift / 100:0.#######}*VFD";
			}
		}

		public static CusCalculationRuleRateCollection LoadRatesFromFormula(CusCalculationRule rule, BusinessObjectFactory factory)
		{
			var formulaToParse = rule.CCR_Formula;
			var calculationRuleRateCollection = new CusCalculationRuleRateCollection(factory);

			if (string.IsNullOrWhiteSpace(formulaToParse))
			{
				var rate = calculationRuleRateCollection.AddNew();
				rate.IsFirstRate = true;
				calculationRuleRateCollection.HasChanges = true; // So that new CusCalculationRule will have CCR_Formula = "0" when saved
				return calculationRuleRateCollection;
			}

			try
			{
				while (!string.IsNullOrWhiteSpace(formulaToParse))
				{
					formulaToParse = formulaToParse.Trim();
					if (formulaToParse.StartsWith("IF"))
					{
						SegmentingIntegratedFormula(formulaToParse, out var currentValueFrom, out var currentFormulaValue, out var childFormula);
						formulaToParse = childFormula;
						var rate = calculationRuleRateCollection.AddNew();
						ParseSingleFormula(rate, currentFormulaValue, currentValueFrom);
					}
					else
					{
						formulaToParse = formulaToParse.TrimEnd(')');
						var rate = calculationRuleRateCollection.AddNew();
						ParseSingleFormula(rate, formulaToParse);
						break;
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				calculationRuleRateCollection.RemoveAndDeleteAll();
			}

			calculationRuleRateCollection.Sort(nameof(CusCalculationRuleRate.ValueFrom));
			var rateWithZeroValueFrom = calculationRuleRateCollection.Cast<CusCalculationRuleRate>().FirstOrDefault(x => x.ValueFrom == 0);
			if (rateWithZeroValueFrom != null)
			{
				rateWithZeroValueFrom.IsFirstRate = true;
			}
			return calculationRuleRateCollection;
		}

		static void SegmentingIntegratedFormula(string formula, out string currentValueFrom, out string currentFormulaValue, out string childFormula)
		{
			var formulaArray = formula.Split(new char[] { ',' }, 3);
			currentValueFrom = formulaArray[0];
			currentFormulaValue = formulaArray[1];
			childFormula = formulaArray[2];
		}

		static void ParseSingleFormula(CusCalculationRuleRate rate, string singleFormula, string valueFrom = "")
		{
			using (rate.SuspendSettingHasChanges())
			{
				rate.ValueFrom = string.IsNullOrWhiteSpace(valueFrom) ? 0 : ZDecimal.Parse(valueFrom.Split('=')[1].Trim());
				if (singleFormula.Contains("*"))
				{
					rate.Uplift = ZDecimal.Parse(singleFormula.Split('*')[0].Trim()) * 100;
					rate.Validation.ValidateFlatRate();
				}
				else
				{
					rate.FlatRate = ZDecimal.Parse(singleFormula.Trim());
					rate.Validation.ValidateUplift();
				}
			}
		}
	}
}
