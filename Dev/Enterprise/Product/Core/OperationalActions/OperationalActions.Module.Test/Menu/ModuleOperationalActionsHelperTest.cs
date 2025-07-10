using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Module.Testing
{
	sealed class ModuleOperationalActionsHelperTest : TestCase
	{
		public void TestShouldAddOperationalActionsIntoGridActionsMenu()
		{
			var helper = new ModuleOperationalActionsHelperForTest();

			using (var control = new ZFilterStripControl())
			using (var buttonGrid = new ZModuleButtonGrid())
			{
				AssertEquals("Should not add Operational Actions menu for ZFilterStripControl, ", false, helper.ShouldAddOperationalActionsIntoGridActionsMenu_Exposed(control));
				AssertEquals("Should not add Operational Actions menu for ZModuleButtonGrid", false, helper.ShouldAddOperationalActionsIntoGridActionsMenu_Exposed(buttonGrid));
			}
		}

		class ModuleOperationalActionsHelperForTest : ModuleOperationalActionsHelper
		{
			public bool ShouldAddOperationalActionsIntoGridActionsMenu_Exposed(Control parent) => ShouldAddOperationalActionsIntoGridActionsMenu(parent);
		}
	}
}
