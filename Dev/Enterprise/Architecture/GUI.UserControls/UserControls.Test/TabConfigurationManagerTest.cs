using System;
using System.Data;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class TabConfigurationManagerTest : TestCaseWithFactory
	{
		public void TestVisibility_ChecksPersistenceOrFormCustomisationSettings()
		{
			using (var form = new DummyForm(Factory.New<DummyWithWorkflow>()))
			{
				form.Show();
				var page = (ZTabPage)form.MainTabControl.TabPages[0];

				var manager = new TabConfigurationManager(form.MainMenu, form.MainTabControl);
				var settings = new FormCustomisationSettingsMock();
				manager.FormCustomisationSettings = settings;

				form.HasTabVisiblePersisted = true;
				form.RetrieveTabPageVisibleOverride = true;
				manager.RefreshTabVisibility();
				AssertEquals(true, form.RetrieveTabPageVisibleCalled);
				AssertEquals(true, settings.IsElementVisibleCalled);

				form.RetrieveTabPageVisibleCalled = false;
				settings.IsElementVisibleCalled = false;

				form.RetrieveTabPageVisibleOverride = false;
				manager.RefreshTabVisibility();
				AssertEquals(true, form.RetrieveTabPageVisibleCalled);
				AssertEquals(false, settings.IsElementVisibleCalled);

				form.RetrieveTabPageVisibleCalled = false;

				form.HasTabVisiblePersisted = false;
				manager.RefreshTabVisibility();
				AssertEquals(false, form.RetrieveTabPageVisibleCalled);
				AssertEquals(true, settings.IsElementVisibleCalled);
			}
		}

		public void TestVisiblePersistence()
		{
			DummyWithWorkflow bizo = Factory.New<DummyWithWorkflow>();
			using (DummyForm form = new DummyForm(bizo))
			{
				form.Show();
				ZTabPage page = (ZTabPage)form.MainTabControl.TabPages[0];

				TabConfigurationManager manager = new TabConfigurationManager(form.MainMenu, form.MainTabControl);
				FormCustomisationSettingsMock settings = new FormCustomisationSettingsMock();
				manager.FormCustomisationSettings = settings;
				manager.Enabled = true;

				MenuItem viewMenu = form.MainMenu.MenuItems.FindByText("View");

				page.TabVisible = true;
				PopupMenu(viewMenu);

				AssertEquals("Precondition", false, form.StoreTabVisibleCalled);
				viewMenu.MenuItems[0].PerformClick();
				AssertEquals("Called", true, form.StoreTabVisibleCalled);
			}
		}

		public void TestVisibilitySetIfNoFormSettings()
		{
			DummyWithWorkflow bizo = Factory.New<DummyWithWorkflow>();
			using (DummyForm form = new DummyForm(bizo))
			{
				form.Show();
				ZTabPage page = (ZTabPage)form.MainTabControl.TabPages[0];

				TabConfigurationManager manager = new TabConfigurationManager(form.MainMenu, form.MainTabControl);
				manager.Enabled = true;

				MenuItem viewMenu = form.MainMenu.MenuItems.FindByText("View");

				page.TabVisible = true;
				PopupMenu(viewMenu);

				AssertEquals("Precondition", false, form.StoreTabVisibleCalled);
				viewMenu.MenuItems[0].PerformClick();
				AssertEquals("Called", true, form.StoreTabVisibleCalled);
			}
		}

		public void TestSetVisible_WithNotifications()
		{
			DummyWithWorkflow bizo = Factory.New<DummyWithWorkflow>();
			using (DummyForm form = new DummyForm(bizo))
			{
				form.Show();

				ZTextBox textBox = new ZTextBox();
				textBox.BindTo = DummyBizoSchema.Z0_VarCharMax.Name;
				ZTabPage page = (ZTabPage)form.MainTabControl.TabPages[0];
				page.Controls.Add(textBox);

				TabConfigurationManager manager = new TabConfigurationManager(form.MainMenu, form.MainTabControl);
				FormCustomisationSettingsMock settings = new FormCustomisationSettingsMock();
				manager.FormCustomisationSettings = settings;
				manager.Enabled = true;

				MenuItem viewMenu = form.MainMenu.MenuItems.FindByText("View");

				page.TabVisible = true;
				PopupMenu(viewMenu);

				viewMenu.MenuItems[0].PerformClick();
				AssertEquals(false, page.TabVisible);

				viewMenu.MenuItems[0].PerformClick();
				AssertEquals(true, page.TabVisible);

				NotificationBroadcaster.Instance.BroadcastNotificationsChange(NotificationType.Error, textBox);
				viewMenu.MenuItems[0].PerformClick();
				AssertEquals("Stays visible as error exists", true, page.TabVisible);

				NotificationBroadcaster.Instance.BroadcastNotificationsChange(NotificationType.Warning, textBox);
				viewMenu.MenuItems[0].PerformClick();
				AssertEquals("Now invisible as only warning", false, page.TabVisible);

				viewMenu.MenuItems[0].PerformClick();
				AssertEquals(true, page.TabVisible);
				NotificationBroadcaster.Instance.BroadcastNotificationsChange(NotificationType.Error, textBox);
				viewMenu.MenuItems[0].PerformClick();
				AssertEquals("Stays visible as message error exists", true, page.TabVisible);
			}
		}

		public void TestViewMenuCreationAndTabVisibility()
		{
			DummyWithWorkflow bizo = Factory.New<DummyWithWorkflow>();
			using (DummyForm form = new DummyForm(bizo))
			{
				form.Show();

				TabConfigurationManager manager = new TabConfigurationManager(form.MainMenu, form.MainTabControl);
				FormCustomisationSettingsMock settings = new FormCustomisationSettingsMock();
				manager.FormCustomisationSettings = settings;
				manager.Enabled = true;

				MenuItem viewMenu = form.MainMenu.MenuItems.FindByText("View");
				AssertEquals("Not loaded until menu popup", 1, viewMenu.MenuItems.Count);

				PopupMenu(viewMenu);
				AssertEquals(3, viewMenu.MenuItems.Count);

				AssertEquals("CRAP Text", viewMenu.MenuItems[0].Text);
				AssertEquals("RIGHTTAB Text", viewMenu.MenuItems[1].Text);
				AssertEquals("BLAH Text", viewMenu.MenuItems[2].Text);

				settings.VisibleElementName = "NADDA";
				manager.RefreshTabVisibility();
				AssertEquals(0, form.MainTabControl.TabPages.Count);
				PopupMenu(viewMenu);
				AssertEquals(false, viewMenu.MenuItems[0].Checked);
				AssertEquals(false, viewMenu.MenuItems[1].Checked);
				AssertEquals(false, viewMenu.MenuItems[2].Checked);

				settings.VisibleElementName = "RIGHTTAB";
				manager.RefreshTabVisibility();
				AssertEquals(1, form.MainTabControl.TabPages.Count);
				PopupMenu(viewMenu);
				AssertEquals(false, viewMenu.MenuItems[0].Checked);
				AssertEquals(true, viewMenu.MenuItems[1].Checked);
				AssertEquals(false, viewMenu.MenuItems[2].Checked);

				PopupMenu(viewMenu);
				AssertEquals(3, viewMenu.MenuItems.Count);

				ZTabPage page = new ZTabPage();
				page.Name = "NEW";
				page.Text = "NEW Text";
				form.MainTabControl.TabPages.Add(page);
				PopupMenu(viewMenu);
				AssertEquals("Menu re-built on every popup", 4, viewMenu.MenuItems.Count);
			}
		}

		public void TestViewMenuCreationAndTabVisibility_NoSettings()
		{
			DummyWithWorkflow bizo = Factory.New<DummyWithWorkflow>();
			using (DummyForm form = new DummyForm(bizo))
			{
				form.Show();

				TabConfigurationManager manager = new TabConfigurationManager(form.MainMenu, form.MainTabControl);
				manager.Enabled = true;

				MenuItem viewMenu = form.MainMenu.MenuItems.FindByText("View");
				AssertEquals("Not loaded until menu popup", 1, viewMenu.MenuItems.Count);

				PopupMenu(viewMenu);
				AssertEquals(3, viewMenu.MenuItems.Count);

				AssertEquals("CRAP Text", viewMenu.MenuItems[0].Text);
				AssertEquals("RIGHTTAB Text", viewMenu.MenuItems[1].Text);
				AssertEquals("BLAH Text", viewMenu.MenuItems[2].Text);
			}
		}

		[NUnit.Framework.ExpectNoExceptions]
		public void TestDoesNotRequireMenu()
		{
			using (DummyForm form = new DummyForm(Factory.New<DummyWithWorkflow>()))
			{
				form.Show();
				TabConfigurationManager manager = new TabConfigurationManager(null, form.MainTabControl);
			}
		}

		public void TestVisibilityOverride_Workflow()
		{
			using (DummyForm form = new DummyForm(Factory.New<DummyWithWorkflow>()))
			{
				WorkflowVisibilityOverridableTab page = new WorkflowVisibilityOverridableTab();
				page.Name = "magictab";
				page.Text = "magictab text";

				form.Show();

				form.MainTabControl.TabPages.Add(page);

				TabConfigurationManager manager = new TabConfigurationManager(null, form.MainTabControl);
				FormCustomisationSettingsMock settings = new FormCustomisationSettingsMock();
				manager.FormCustomisationSettings = settings;
				manager.Enabled = true;

				settings.VisibleElementName = "magictab";
				manager.RefreshTabVisibility();
				AssertEquals(true, page.TabVisible);

				page.VisiblilityOverride = false;

				manager.RefreshTabVisibility();
				AssertEquals(false, page.TabVisible);
			}
		}

		public void TestVisibilityOverride_Menu()
		{
			using (var form = new DummyForm(Factory.New<DummyWithWorkflow>()))
			{
				form.Show();

				var page = new WorkflowVisibilityOverridableTab();
				page.Name = "magictab";
				page.Text = "magictab text";

				page.VisiblilityOverride = false;

				form.MainTabControl.TabPages.Clear();
				form.MainTabControl.TabPages.Add(page);

				var tabConfigurationManager = new TabConfigurationManager(form.MainMenu, form.MainTabControl);
				tabConfigurationManager.Enabled = true;

				MenuItem viewMenu = form.MainMenu.MenuItems.FindByText("View");
				AssertEquals("Not loaded until menu popup", 1, viewMenu.MenuItems.Count);

				PopupMenu(viewMenu);

				var magicTabMenuItem = viewMenu.MenuItems.FindByText("magictab text");

				AssertNotNull("menu item text shows magic tab", magicTabMenuItem);
				AssertEquals("menu item is not checked", false, magicTabMenuItem.Checked);
				AssertEquals("tab is invisible", false, page.TabVisible);

				magicTabMenuItem.PerformClick();

				AssertEquals("menu item is not checked", false, magicTabMenuItem.Checked);
				AssertEquals("tab is still invisible", false, page.TabVisible);
			}
		}
		public void TestViewMenuCreationAndTabVisibility_TabRelevant()
		{
			DummyWithWorkflow bizo = Factory.New<DummyWithWorkflow>();
			using (DummyForm form = new DummyForm(bizo))
			{
				form.Show();
				var tabPage1 = (ZTabPage)form.MainTabControl.TabPages[0];
				var tabPage2 = (ZTabPage)form.MainTabControl.TabPages[1];

				TabConfigurationManager manager = new TabConfigurationManager(form.MainMenu, form.MainTabControl);
				FormCustomisationSettingsMock settings = new FormCustomisationSettingsMock();
				manager.FormCustomisationSettings = settings;
				manager.Enabled = true;

				tabPage1.TabRelevant = false;
				tabPage2.TabRelevant = false;

				MenuItem viewMenu = form.MainMenu.MenuItems.FindByText("View");
				AssertEquals("Not loaded until menu popup", 1, viewMenu.MenuItems.Count);

				PopupMenu(viewMenu);
				AssertEquals(1, viewMenu.MenuItems.Count);
				AssertEquals("BLAH Text", viewMenu.MenuItems[0].Text);

				settings.VisibleElementName = "CRAP";
				manager.RefreshTabVisibility();
				AssertEquals(0, form.MainTabControl.TabPages.Count);
				PopupMenu(viewMenu);
				AssertEquals(1, viewMenu.MenuItems.Count);
				AssertEquals("BLAH Text", viewMenu.MenuItems[0].Text);

				tabPage1.TabRelevant = true;
				tabPage2.TabRelevant = true;
				AssertEquals(1, form.MainTabControl.TabPages.Count);
				Assert("tabPage1 should be shown", tabPage1.TabVisible);
				Assert("tabPage1 should NOT be shown", !tabPage2.TabVisible);
				PopupMenu(viewMenu);
				AssertEquals(3, viewMenu.MenuItems.Count);

				AssertEquals("CRAP Text", viewMenu.MenuItems[0].Text);
				AssertEquals("RIGHTTAB Text", viewMenu.MenuItems[1].Text);
				AssertEquals("BLAH Text", viewMenu.MenuItems[2].Text);

				AssertEquals(true, viewMenu.MenuItems[0].Checked);
				AssertEquals(false, viewMenu.MenuItems[1].Checked);
				AssertEquals(false, viewMenu.MenuItems[2].Checked);
			}
		}

		#region Implementation

		void PopupMenu(MenuItem menuItem)
		{
			typeof(MenuItem).InvokeMember("OnPopup", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, menuItem, new object[] { EventArgs.Empty });
		}

		public class FormCustomisationSettingsMock : IFormCustomisationSettings
		{
			public bool AllFieldsAreOnDefaultTabs()
			{
				return false;
			}

			public string[] CustomisableTabPageNames
			{
				get { return new string[] { "TAB1", "TAB2" }; }
			}

			public TabPlacement GetTabPlacement(string elementName)
			{
				return new TabPlacement();
			}

			public bool? IsElementVisible(string elementName, ElementType elementType)
			{
				IsElementVisibleCalled = true;
				return VisibleElementName == elementName;
			}

			public string VisibleElementName;
			public bool IsElementVisibleCalled;
		}

		class DummyWithWorkflow : DummyBusinessObject, IWorkflowProviderCore
		{
			public DummyWithWorkflow(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public IColumnValueRanker GetTemplateSelectionCriteria()
			{
				return new ColumnValueRanker();
			}

			public ZString WorkflowType
			{
				get { return "XXX"; }
			}

			#endregion
		}

		class DummyForm : ZForm, ITabVisibilityDeciderPersistence
		{
			public DummyForm(DummyBusinessObject bizo)
				: base(bizo)
			{
			}

			protected override void OnLoad(EventArgs e)
			{
				base.OnLoad(e);

				TabControl = new ZTabControl();
				Controls.Add(TabControl);

				ZTabPage page = new ZTabPage();
				page.Name = "CRAP";
				page.Text = "CRAP Text";
				MainTabControl.TabPages.Add(page);

				page = new ZTabPage();
				page.Name = "RIGHTTAB";
				page.Text = "RIGHTTAB Text";
				MainTabControl.TabPages.Add(page);

				page = new ZTabPage();
				page.Name = "BLAH";
				page.Text = "BLAH Text";
				MainTabControl.TabPages.Add(page);
			}

			ZTabControl TabControl;

			public new MainMenu MainMenu
			{
				get { return base.MainMenu; }
			}

			public ZTabControl MainTabControl
			{
				get { return base.TopLevelTabControl; }
			}

			#region ITabVisibilityDeciderPersistence Members

			public bool HasTabVisiblePersisted { get; set; }

			public bool RetrieveTabPageVisible(ZTabPage page)
			{
				RetrieveTabPageVisibleCalled = true;
				return RetrieveTabPageVisibleOverride;
			}

			public bool RetrieveTabPageVisibleCalled;
			public bool RetrieveTabPageVisibleOverride;

			public void StoreTabVisible(ZTabPage page)
			{
				StoreTabVisibleCalled = true;
			}

			public bool StoreTabVisibleCalled;

			#endregion
		}

		class WorkflowVisibilityOverridableTab : ZTabPage, ITabVisibilityOverride
		{
			public bool? VisiblilityOverride { get; set; }

			bool? ITabVisibilityOverride.IsTabVisible
			{
				get { return VisiblilityOverride; }
			}
		}
	}
}
