using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.ModulePlugIn.Testing
{
	sealed class ZModulePluginTest : TestCaseWithFactory
	{
		public void TestGetActionMenuItemsToAddDefaultBehaviour()
		{
			using (ZModulePlugin plugin = new ModulePluginWithDefaultBehaviour())
			{
				var items = plugin.GetActionMenuItemToAdd();
				AssertEquals("return null (no menu item) by default", null, items);
			}
		}

		public void TestGetButtonGridMenuItemsToAddDefaultBehaviour()
		{
			using (ZModulePlugin plugin = new ModulePluginWithDefaultBehaviour())
			{
				var items = plugin.GetActionMenuItemToAdd();
				AssertEquals("return null (no menu item) by default", null, items);
			}
		}

		public void TestRequiresMultiSelectDefault()
		{
			using (ZModulePlugin plugin = new ModulePluginWithDefaultBehaviour())
			{
				AssertEquals("return false, multi-select not required by default", false, plugin.RequiresMultiSelect);
			}
		}

		public class ModulePluginWithDefaultBehaviour : ZModulePlugin { }
	}
}
