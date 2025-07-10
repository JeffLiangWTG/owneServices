using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.PlugIn;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Test.Forms.FormStrategies
{
	public class ZFormPlugInStrategyTest : TestCaseWithDummy
	{
		public void TestInsertPlugInMenuItems()
		{
			using (var testForm = new PlugInTestForm(Dummy))
			{
				testForm.Show();
				Application.DoEvents();
				UserIdleWorker.Flush();

				var lastDummyMenuItemIndex = testForm.Menu.MenuItems.Count - 2; // skip help menu
				AssertEquals("PlugIn3-Menu2", testForm.Menu.MenuItems[lastDummyMenuItemIndex].Text);
				AssertEquals("PlugIn3", testForm.Menu.MenuItems[lastDummyMenuItemIndex - 1].Text);
				AssertEquals("PlugIn1", testForm.Menu.MenuItems[lastDummyMenuItemIndex - 2].Text);

				AssertEquals("PlugIn2", testForm.Menu.MenuItems[2].MenuItems[0].Text);
			}
		}

		[ExpectNoExceptions]
		public void TestHidingFormWithPlugins()
		{
			using (var testForm = new PlugInTestForm(Dummy))
			{
				testForm.Show();
				Application.DoEvents();
				UserIdleWorker.Flush();
				testForm.Hide();
			}
		}

		public void TestKeepOriginalPositionWhenGetNewTopMenuInPlugInIsExistingMenu()
		{
			using (var testForm = new PlugInTestForm(Dummy))
			{
				var oldActionMenuIndex = testForm.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName].Index;

				testForm.PlugIns.Add(DummyControllerIDs.DummyControllerWithGetNewTopMenuInPlugInIsExistingMenu);

				testForm.Show();
				Application.DoEvents();
				UserIdleWorker.Flush();

				AssertEquals("Existing menu should keep the original position", oldActionMenuIndex, testForm.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName].Index);
				AssertNotNull(testForm.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName].MenuItems.FindByText("DummyPluginWithGetNewTopMenuInPlugInIsExistingMenu"));
			}
		}

		#region Implementation

		class PlugInTestForm : ZTestForm
		{
			public PlugInTestForm(IBusiness bO)
				: base(bO)
			{
				PlugIns.Add(DummyControllerIDs.Dummy1);
				PlugIns.Add(DummyControllerIDs.Dummy2);
				PlugIns.Add(DummyControllerIDs.Dummy3);

				DummyMenuItem = new ZMenuItem("DummyMenuItem");
				MainMenu.MenuItems.Add(2, DummyMenuItem);
			}

			readonly MenuItem DummyMenuItem;

			protected override Menu GetMenuForPlugInCore(ControllerID controllerID)
			{
				if (controllerID == DummyControllerIDs.Dummy2)
				{
					return DummyMenuItem;
				}

				return base.GetMenuForPlugInCore(controllerID);
			}
		}

		#endregion
	}
}
