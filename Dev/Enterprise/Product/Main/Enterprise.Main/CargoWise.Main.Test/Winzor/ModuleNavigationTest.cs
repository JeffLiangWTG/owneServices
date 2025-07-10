using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Core.Modules;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using MainFormForTest = Enterprise.Startup.Testing.MainFormTestCase.TestMainForm;

namespace CargoWise.Main.Navigation.Test;
internal class ModuleNavigationTest : TestCaseWithFactory
{
	public void TestOpenModuleInNewWindow_AppBar()
	{
		TestCaseHelper.ClearTable(StmLinkSchema.Constants.TableName);
		var testModuleId = DummyModuleIDs.Dummy;

		using (var form = new TestMainFormWithUnReadItems())
		{
			form.Show();
			System.Windows.Forms.Application.DoEvents();
			var nav = form.nav;

			var shortCutTriggered = false;
			var testMenuItem = new ZToolStripMenuItem()
			{
				ShortcutKeys = Keys.Control | Keys.Shift | Keys.S
			};

			testMenuItem.Click += (sender, args) =>
			{
				shortCutTriggered = true;
			};
			nav.navigationViewModel.PopupMenus.Add(testMenuItem.ToPopupMenu());

			nav.ModuleOpener = form;

			nav.LoadModuleTree(ModuleTree.Tree);
			var mainFormModule = new MainFormModule(testModuleId);

			Assertion.AssertNoExceptionThrown("Null should have been handled.", () => nav.OpenModuleInNewWindow(mainFormModule, null));

			nav.newModuleFormForTest.Width = 10;
			nav.newModuleFormForTest.Height = 10;

			AssertEquals("Min Width is 1366", ControlDpiScalingHelper.ScaleToCurrentDpiX(1366), nav.newModuleFormForTest.Width);
			AssertEquals("Min Height is 730", ControlDpiScalingHelper.ScaleToCurrentDpiY(730), nav.newModuleFormForTest.Height);

			var keys = Keys.Control | Keys.Shift | Keys.S;
			nav.newModuleFormForTest.InvokeProcessKeyEvent("KeyDown", keys.ToString(), keys, false);
			AssertEquals("shortCutTriggered should be true", true, shortCutTriggered);

			nav.newModuleFormForTest.Dispose();
		}
	}

	public void TestAppBarHostShouldContainChildren()
	{
		TestCaseHelper.ClearTable(StmLinkSchema.Constants.TableName);
		var testModuleId = DummyModuleIDs.Dummy;

		using (var form = new TestMainFormWithUnReadItems())
		{
			form.Show();
			System.Windows.Forms.Application.DoEvents();
			var nav = form.nav;
			nav.ModuleOpener = form;

			nav.LoadModuleTree(ModuleTree.Tree);
			var mainFormModule = new MainFormModule(testModuleId);

			Assertion.AssertNoExceptionThrown("Null should have been handled.", () => nav.OpenModuleInNewWindow(mainFormModule, null));

			var host = nav.newModuleFormForTest.Find(c => c.Name == "NextAppBarHost").FirstOrDefault();
			Assert(host != null);
			var appBar = nav.appBarForTest;
			Assert(appBar != null);
			host.GetNextControl(appBar, false);
			nav.newModuleFormForTest.Dispose();
		}
	}

	public void TestOpenModuleInNewWindow_AppBar_Shortcuts()
	{
		TestCaseHelper.ClearTable(StmLinkSchema.Constants.TableName);
		var testModuleId = DummyModuleIDs.Dummy;

		using (var form = new TestMainFormWithUnReadItems())
		{
			form.Show();
			System.Windows.Forms.Application.DoEvents();
			var nav = form.nav;

			var shortCutTriggered = false;
			var keys = Keys.Control | Keys.Shift | Keys.S;

			var testMenuItem = new ZToolStripMenuItem()
			{
				ShortcutKeys = keys
			};

			testMenuItem.Click += (sender, args) =>
			{
				shortCutTriggered = true;
			};

			nav.navigationViewModel.PopupMenus.Add(testMenuItem.ToPopupMenu());

			nav.ModuleOpener = form;

			nav.LoadModuleTree(ModuleTree.Tree);
			var mainFormModule = new MainFormModule(testModuleId);

			Assertion.AssertNoExceptionThrown("Null should have been handled.", () => nav.OpenModuleInNewWindow(mainFormModule, null));

			nav.newModuleFormForTest.InvokeProcessKeyEvent("KeyDown", keys.ToString(), keys, false);
			AssertEquals("shortCutTriggered should be true", true, shortCutTriggered);

			nav.newModuleFormForTest.Dispose();
		}
	}
	public void TestShowModuleUrlHandlerOnFormInitializedActionShouldNotBeNull()
	{
		using (var form = new TestMainFormWithUnReadItems())
		{
			Assert(ShowModuleUrlHandler.Instance.OnFormInitialized != null);
		}
	}

	class TestMainFormWithUnReadItems : MainFormForTest
	{
		public readonly NextNavigation nav = new();

		public TestMainFormWithUnReadItems()
		{
			Controls.Add(nav);
		}
	}
}
