using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal;
using CusEntryLine = Enterprise.Customs.EU.Business.Declaration.CusEntryLine;
using DutyCalculatorStrategy = Enterprise.Customs.EU.Business.Declaration.DutyCalculatorStrategy;

namespace Enterprise.Customs.GB.DocumentWrappers
{
	public class DutyCalculatorStrategySimulator : DutyCalculatorStrategy
	{
		public DutyCalculatorStrategySimulator(JobDeclaration declaration, CusEntryLine entryLine) : base(declaration)
		{
			this.entryLine = entryLine;
		}

		public List<(IDutyCalculationIntermediateResult result, string code)> CalculateEntryLineFeesForTaxEstimator()
		{
			results = new List<(IDutyCalculationIntermediateResult, string)>();
			CalculateEntryLineFees(entryLine);
			return results;
		}

		protected override bool ShouldCalculateSystemFeeForThisCode(CusEntryLine entryLine, string rateCode)
		{
			return false;
		}

		protected override void AddNewEntryLineFee(CusEntryLine entryLine, ZString rateCode, ZString overrideReasonCode, IDutyCalculationIntermediateResult calculatedFee, RateView rateForCalculation = null)
		{
			results.Add((calculatedFee, rateCode));
		}

		readonly CusEntryLine entryLine;

		List<(IDutyCalculationIntermediateResult, string)> results;
	}
}
