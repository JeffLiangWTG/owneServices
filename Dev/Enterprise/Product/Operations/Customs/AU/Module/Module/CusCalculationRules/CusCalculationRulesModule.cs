using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AU.Module
{
	public class CusCalculationRulesModule : Customs.Module.CusCalculationRulesModule
	{
		protected override FilterBusinessObject GetNewFilterBusinessObject() => new CusCalculationRulesFilterBusinessObject();

		protected override IBusinessObjectCollection GetNewGridCollection() => new Business.CusCalculationRuleCollection<CusCalculationRule>(Factory);
	}
}
