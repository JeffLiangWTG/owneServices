using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZHelpMenuTest : TestCaseWithFactory
	{
		public void TestClientSpecificMenuItem()
		{
			using (ClientHookLoader.Instance.OverrideClientHookForTest(new TestClientHook()))
			using (var form = new ZForm())
			{
				form.Menu.MenuItems.Add(HelpMenu);
				Assert("Should not have MyClientHook menu", HelpMenu.MenuItems[0].Text != "MyClientHook Extensions");

				HelpMenu.FirePopup();
				Assert("Should have MyClientHook menu", HelpMenu.MenuItems[0].Text == "MyClientHook Extensions");
			}
		}

		class TestClientHook : ClientHook
		{
			public override string ClientDisplayName
			{
				get { return "MyClientHook"; }
			}

			public override string HelpWebPage
			{
				get { return "http://www.help.com"; }
			}

			public override Clients Client
			{
				get { return Clients.BTI; }
			}
		}

		public void TestZKeyStatusMenuItem()
		{
			using (var form = new ZForm())
			using (var menu = new ZHelpMenu(form))
			{
				form.Menu.MenuItems.Add(menu);
				var found = false;
				foreach (MenuItem menuItem in menu.MenuItems)
				{
					if (menuItem is ZMenuItem && menuItem.Text.Equals("&Key Status"))
					{
						found = true;
						var keyStatusForm = ZApplication.GetOpenForms().OfType<ZKeyStatusForm>().SingleOrDefault(x => x.Text == "Key Status Form");
						try
						{
							AssertNull(keyStatusForm);
							menuItem.PerformClick();

							keyStatusForm = ZApplication.GetOpenForms().OfType<ZKeyStatusForm>().SingleOrDefault(x => x.Text == "Key Status Form");
							AssertNotNull(keyStatusForm);
						}
						finally
						{
							keyStatusForm?.Close();
						}
						break;
					}
				}
				Assert("Should have ZKeyStatusForm", found);
			}
		}

		public void TestTrainingModeMenuItem()
		{
			using (var form = new ZForm())
			{
				form.Menu.MenuItems.Add(HelpMenu);
				var found = false;
				foreach (MenuItem menuItem in HelpMenu.MenuItems)
				{
					if (menuItem is ZTrainingModeMenuItem)
					{
						found = true;

						HelpMenu.FirePopup();
						var item = (ZTrainingModeMenuItem)menuItem;
						AssertEquals(EnvProxy.Instance.Registry.TraningModeEnabled, item.Checked);

						var initial = EnvProxy.Instance.Registry.TraningModeEnabled;
						item.PerformClick();
						AssertNotEquals(initial, EnvProxy.Instance.Registry.TraningModeEnabled);
#if WINZOR
						AssertEquals(EnvProxy.Instance.Registry.TraningModeEnabled, item.Checked);
#endif
						HelpMenu.FirePopup();
						AssertEquals(EnvProxy.Instance.Registry.TraningModeEnabled, item.Checked);
						item.PerformClick();
						AssertEquals(initial, EnvProxy.Instance.Registry.TraningModeEnabled);

						break;
					}
				}
				Assert("Should have ZTrainingModeMenuItem", found);
			}
		}

		public void TestTrainingModeMenuItem_ToolStripVersion()
		{
			using (var form = new ZForm())
			{
				var toolStripMenuItem = new ZToolStripMenuItem();
				ZTrainingModeMenuItem.AddTrainingModeMenuItem(toolStripMenuItem);
				var found = false;
				foreach (var item in toolStripMenuItem.DropDownItems.OfType<ZToolStripMenuItem>())
				{
					if (item.Text == "&Training Mode")
					{
						found = true;

						toolStripMenuItem.ShowDropDown();
						AssertEquals(EnvProxy.Instance.Registry.TraningModeEnabled, item.Checked);

						var initial = EnvProxy.Instance.Registry.TraningModeEnabled;
						item.PerformClick();
						AssertNotEquals(initial, EnvProxy.Instance.Registry.TraningModeEnabled);

						toolStripMenuItem.ShowDropDown();
						AssertEquals(EnvProxy.Instance.Registry.TraningModeEnabled, item.Checked);
						item.PerformClick();
						AssertEquals(initial, EnvProxy.Instance.Registry.TraningModeEnabled);

						break;
					}
				}

				toolStripMenuItem.Dispose();
				Assert("Should have ZTrainingModeMenuItem", found);
			}
		}

		public void TestServiceRequestMenuItem()
		{
			using (var form = new ZForm())
			using (var menu = new ZHelpMenu(form))
			{
				form.Menu.MenuItems.Add(menu);
				var found = false;
				foreach (MenuItem menuItem in menu.MenuItems)
				{
					if (menuItem is ServiceRequestMenuItem)
					{
						MenuClickPendingTracker.InstanceCount = 0;
						found = true;
						menuItem.PerformClick();

						AssertEquals("MenuClickPendingTracker.InstanceCount", 2, MenuClickPendingTracker.InstanceCount);
						AssertEquals("MenuClickPendingTracker.Count", 0, MenuClickPendingTracker.Count);

						break;
					}
				}
				Assert("Should have ServiceRequestMenuItem", found);
			}
		}

		public void TestServiceRequestMenuItem_ToolStripVersion()
		{
			using (var form = new ZForm())
			{
				var toolStripMenuItem = new ZToolStripMenuItem();
				ServiceRequestMenuItem.AddServiceRequestMenuItem(toolStripMenuItem, form);

				var found = false;
				foreach (var menuItem in toolStripMenuItem.DropDownItems.OfType<ZToolStripMenuItem>())
				{
					if (menuItem.Text.Contains("eRequest"))
					{
						MenuClickPendingTracker.InstanceCount = 0;
						found = true;
						menuItem.PerformClick();

						AssertEquals("MenuClickPendingTracker.InstanceCount", 3, MenuClickPendingTracker.InstanceCount);
						AssertEquals("MenuClickPendingTracker.Count", 0, MenuClickPendingTracker.Count);

						break;
					}
				}

				toolStripMenuItem.Dispose();
				Assert("Should have ServiceRequestMenuItem", found);
			}
		}

		public void TestHotKeyHelpMenuItem()
		{
			using (var form = new ZForm())
			using (var menu = new ZHelpMenu(form))
			{
				form.Menu.MenuItems.Add(menu);
				var found = false;
				foreach (MenuItem menuItem in menu.MenuItems)
				{
					if (menuItem is ZMenuItem && menuItem.Text.Equals("Hot Key Help"))
					{
						MenuClickPendingTracker.InstanceCount = 0;
						found = true;
						var shortcutForm = ZApplication.GetOpenForms().OfType<ZForm>().SingleOrDefault(x => x.Text == "Available Shortcuts");
						AssertNull(shortcutForm);
						menuItem.PerformClick();

						shortcutForm = ZApplication.GetOpenForms().OfType<ZForm>().SingleOrDefault(x => x.Text == "Available Shortcuts");
						AssertNotNull(shortcutForm);
						shortcutForm.Dispose();
						break;
					}
				}
				Assert("Should have HotKeyHelpMenuItem", found);
			}
		}

		public void TestBorderWiseLoginMenuItem()
		{
			using (var form = new ZForm())
			using (var menu = new ZHelpMenu(form))
			{
				form.Menu.MenuItems.Add(menu);

				AssertNotNull(menu.MenuItems.FindByText(ZHelpMenu.BorderWiseLogin));
			}
		}

		public void TestUpdateNotesMenuItem()
		{
			using (var form = new ZForm())
			using (var menu = new ZHelpMenu(form))
			{
				form.Menu.MenuItems.Add(menu);

				var wisetechAcademySubpage = menu.MenuItems.FindByText(ZHelpMenu.WiseTechAcademyName);

				AssertNotNull(wisetechAcademySubpage.MenuItems.FindByText(ZHelpMenu.ReleaseNotesName));
			}
		}

		#region Setup

		protected override void SetUp()
		{
			base.SetUp();
			HelpMenu = new ZHelpMenuTestClass();
		}

		protected override void TearDown()
		{
			HelpMenu.Dispose();
			base.TearDown();
		}

		ZHelpMenuTestClass HelpMenu;

		class ZHelpMenuTestClass : ZHelpMenu
		{
			public ZHelpMenuTestClass()
				: base(null)
			{
			}

			public void FirePopup()
			{
				OnPopup(EventArgs.Empty);
			}
		}

		#endregion
	}
}
