using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Module
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
