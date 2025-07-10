using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Module.Testing
{
	[TestedType(typeof(CusCalculationRulesController))]
	sealed class CusCalculationRulesControllerTest : Customs.Module.Testing.CusCalculationRulesControllerTest
	{
		public override void TestDeleteForm()
		{
			// DeleteForm is empty now
			Assert(true);
		}

		public override void TestViewForm()
		{
			// ViewForm is empty now
			Assert(true);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Customs.CusCalculationRules;
		}
	}
}
