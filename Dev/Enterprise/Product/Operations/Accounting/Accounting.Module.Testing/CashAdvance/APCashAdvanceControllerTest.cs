using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(APCashAdvanceController))]
	class APCashAdvanceControllerTest : CashAdvanceControllerTest
	{
		public void TestModuleID_ShouldEqualAPCashAdvance()
		{
			var controller = new APCashAdvanceController();
			AssertEquals("ModuleID", ModuleIDs.APCashAdvance, controller.ModuleID);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.APCashAdvance;
		}
	}
}
