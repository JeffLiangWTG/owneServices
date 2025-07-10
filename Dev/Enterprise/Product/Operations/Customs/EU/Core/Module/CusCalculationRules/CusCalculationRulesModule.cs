using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.Module
{
	public class CusCalculationRulesModule : Customs.Module.CusCalculationRulesModule
	{
		protected override FilterBusinessObject GetNewFilterBusinessObject() => new CusCalculationRulesFilterBusinessObject();

		protected override IBusinessObjectCollection GetNewGridCollection() => new Customs.Business.CusCalculationRuleCollection<CusCalculationRule>(Factory);
	}
}
