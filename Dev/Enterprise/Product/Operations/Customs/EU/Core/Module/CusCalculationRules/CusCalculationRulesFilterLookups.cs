using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.Module
{
	public class CusCalculationRulesFilterLookups : Customs.Module.CusCalculationRulesFilterLookups
	{
		public CusCalculationRulesFilterLookups(Customs.Module.CusCalculationRulesFilterBusinessObject filterBizObj) : base(filterBizObj)
		{
		}

		protected override Customs.Business.CusCalculationRule CusCalculationRule => cusCalculationRule ?? (cusCalculationRule = Factory.GetNull<CusCalculationRule>());
		CusCalculationRule cusCalculationRule;
	}
}
