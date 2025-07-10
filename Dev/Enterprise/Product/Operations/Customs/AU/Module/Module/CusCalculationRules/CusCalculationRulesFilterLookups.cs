using Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.Customs.AU.Module
{
	public class CusCalculationRulesFilterLookups : Customs.Module.CusCalculationRulesFilterLookups
	{
		public CusCalculationRulesFilterLookups(Customs.Module.CusCalculationRulesFilterBusinessObject filterBizObj) : base(filterBizObj)
		{
		}

		protected override Business.CusCalculationRule CusCalculationRule => cusCalculationRule ?? (cusCalculationRule = Factory.GetNull<CusCalculationRule>());
		CusCalculationRule cusCalculationRule;
	}
}
