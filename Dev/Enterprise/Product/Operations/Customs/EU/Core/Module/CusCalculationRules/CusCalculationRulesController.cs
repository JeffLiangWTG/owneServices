using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Module
{
	public class CusCalculationRulesController : Customs.Module.CusCalculationRulesController
	{
		public override Type TypeOfTopLevelBusinessObject => typeof(CusCalculationRule);

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new CusCalculationRuleForm(businessEntity as CusCalculationRule);
		}
	}
}
