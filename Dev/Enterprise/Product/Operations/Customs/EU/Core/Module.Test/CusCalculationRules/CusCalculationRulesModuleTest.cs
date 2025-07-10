using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Module.Testing
{
	[TestedType(typeof(CusCalculationRulesModule))]
	sealed class CusCalculationRulesModuleTest : Customs.Module.Testing.CusCalculationRulesModuleTest
	{
		public override void TestGetNewFilterBusinessObject()
		{
			using (var module = new CusCalculationRulesModuleForTest())
			{
				AssertType<CusCalculationRulesFilterBusinessObject>(module.GetNewFilterBusinessObject());
			}
		}

		public override void TestGetNewController()
		{
			using (var module = new CusCalculationRulesModuleForTest())
			{
				AssertType<CusCalculationRulesController>(module.GetNewController());
			}
		}

		public override void TestGetNewGridCollection()
		{
			using (var module = new CusCalculationRulesModuleForTest())
			{
				AssertType<CusCalculationRuleCollection<Business.CusCalculationRule>>(module.GetNewGridCollection());
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.CusCalculationRules;

		sealed class CusCalculationRulesModuleForTest : CusCalculationRulesModule
		{
			public new IFilterControl GetNewFilterControl() => base.GetNewFilterControl();
			public new FilterBusinessObject GetNewFilterBusinessObject() => base.GetNewFilterBusinessObject();
			public new IBusinessObjectCollection GetNewGridCollection() => base.GetNewGridCollection();
		}
	}
}
