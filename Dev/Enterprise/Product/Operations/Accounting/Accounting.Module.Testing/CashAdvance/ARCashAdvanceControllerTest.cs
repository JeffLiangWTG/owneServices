using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(ARCashAdvanceController))]
	class ARCashAdvanceControllerTest : CashAdvanceControllerTest
	{
		public void TestModuleID()
		{
			var controller = new ARCashAdvanceController();
			AssertEquals("ModuleID", ModuleIDs.ARCashAdvance, controller.ModuleID);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ARCashAdvance;
		}
	}
}
