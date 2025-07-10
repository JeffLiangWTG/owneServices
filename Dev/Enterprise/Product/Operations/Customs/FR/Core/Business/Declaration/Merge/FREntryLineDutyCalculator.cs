using CargoWise.Types;
using Enterprise.Customs.DutyCalculator;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.FR.Business.Declaration
{
	class FREntryLineDutyCalculator : EntryLineDutyCalculator
	{
		public FREntryLineDutyCalculator(EU.Business.Declaration.CusEntryLine entity, RateCalculationVisitorMode rateCalculationVisitorMode)
			: base(entity, rateCalculationVisitorMode)
		{
		}

		protected override ZString GetConvertedFormulaForCleanUpFormula(RateView rateForCalculation)
		{
			var result = rateForCalculation.ZZ2_RateFormula;
			if (result == UniversalReferenceConstants.RefCusRateFormula.Precalcule)
			{
				result = "0";
			}
			return result;
		}
	}
}
