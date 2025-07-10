using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DutyCalculator;

namespace Enterprise.Customs.FR.Business.Declaration
{
	sealed class StatisticalValueFeeCalculator : EU.Business.Declaration.IExtraFeeCalculator
	{
		public StatisticalValueFeeCalculator(CusEntryLine entryLine)
		{
			this.entryLine = Argument.NotNull(entryLine, nameof(entryLine));
		}
		readonly CusEntryLine entryLine;

		public ZString RateCode => UniversalReferenceConstants.RefCusRateCodes.StatisticalValueBasis; // to do : change the code here for the one wanted.

		public IEnumerable<IDutyCalculationIntermediateResult> CalculateExtraFees()
		{
			var rate = 1m;
			var baseValue = entryLine.CL_Calc_StatisticalBasisExcludingSTACharge;
			yield return new DutyCalculationIntermediateResult(baseValue * rate, rate, baseValue, Customs.Business.UniversalReferenceConstants.MethodOfCalculation.Percentage);
		}
	}
}
