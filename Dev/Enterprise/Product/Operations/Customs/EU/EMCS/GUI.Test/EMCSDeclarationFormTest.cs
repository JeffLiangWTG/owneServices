using System;
using System.Reflection;
using System.Windows.Forms;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.GUI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.GUI.Testing
{
	[TestedType(typeof(EMCSDeclarationForm))]
	class EMCSDeclarationFormTest : ZFormBasherTest
	{
		public void TestMessagingMenu()
		{
			using (var form = new EMCSDeclarationForm(declaration))
			{
				form.Show();
				AssertEquals(typeof(EMCSMenu), form.Menu.MenuItems.FindByText("EMCS").GetType());
			}

			declaration.JE_IsCancelled = true;
			using (var form = new EMCSDeclarationForm(declaration))
			{
				form.Show();
				AssertEquals(typeof(EDIMenuStub), form.Menu.MenuItems.FindByText("EMCS").GetType());
			}
		}

		public void TestViewMenuItems()
		{
			using (var form = new EMCSDeclarationForm(declaration))
			{
				form.Show();

				var viewMenu = form.Menu.MenuItems.FindByText("View");
				//This forces the MenuItems to load as they are Dynamic
				typeof(MenuItem).InvokeMember("OnPopup", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, viewMenu, new object[] { EventArgs.Empty });
				CombineAssertions(() =>
				{
					AssertNotNull("EMCS", viewMenu.MenuItems.FindByText("EMCS"));
					AssertNotNull("Inv. Lines", viewMenu.MenuItems.FindByText("Inv. Lines"));
					AssertNotNull("Workflow && Tracking", viewMenu.MenuItems.FindByText("Workflow && Tracking"));
					AssertNotNull("Custom", viewMenu.MenuItems.FindByText("Custom"));
					AssertNotNull("Billing", viewMenu.MenuItems.FindByText("Billing"));
					AssertNotNull("Doc Data", viewMenu.MenuItems.FindByText("Doc Data"));
					AssertNotNull("eDocs", viewMenu.MenuItems.FindByText("eDocs"));
					AssertNotNull("Notes", viewMenu.MenuItems.FindByText("Notes"));
					AssertNotNull("Logs", viewMenu.MenuItems.FindByText("Logs"));
				});
			}
		}

		public void TestCustomMenuUnCheckedAndTabPage()
		{
			using (var form = new EMCSDeclarationForm(declaration))
			{
				form.Show();

				var tabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				var viewMenu = form.Menu.MenuItems.FindByText("View");
				typeof(MenuItem).InvokeMember("OnPopup", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, viewMenu, new object[] { EventArgs.Empty });

				var customsMenuItem = viewMenu.MenuItems.FindByText("Custom");
				customsMenuItem.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals("Unchecked", false, customsMenuItem.Checked);
					AssertNull("The Custom tab disappears", tabControl.GetTabPageByNameOrText("Custom"));
				});
			}
		}

		public void TestCustomMenuCheckedAndTabPage()
		{
			using (var form = new EMCSDeclarationForm(declaration))
			{
				form.Show();

				var tabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				var viewMenu = form.Menu.MenuItems.FindByText("View");
				typeof(MenuItem).InvokeMember("OnPopup", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, viewMenu, new object[] { EventArgs.Empty });

				var customsMenuItem = viewMenu.MenuItems.FindByText("Custom");
				CombineAssertions(() =>
				{
					AssertEquals("Checked", true, customsMenuItem.Checked);
					var workflowTab = tabControl.GetTabPageByNameOrText("WorkflowTabPage");
					AssertEquals("The Custom tab should appear after Workflow && Tracking", tabControl.TabPages.IndexOf(workflowTab) + 1, tabControl.TabPages.IndexOf(tabControl.GetTabPageByNameOrText("Custom")));
				});
			}
		}

		public void TestFormCaption()
		{
			using (var form = new EMCSDeclarationForm(declaration))
			{
				form.Show();
				AssertEquals("EMCS Declaration", form.FormCaption);

				Factory.Save();
				AssertEquals("EMCS Declaration - " + declaration.JE_DeclarationReference, form.FormCaption);
			}
		}

		public void TestScreenMenuAndTabPage()
		{
			using (var form = new EMCSDeclarationForm(declaration))
			{
				form.Show();

				var actionMenu = form.Menu.MenuItems.FindByText("Actions");
				AssertNotNull(actionMenu);
				AssertNotNull(actionMenu.MenuItems.FindByText("View Compliance Status"));

				var brokerageUserControl = (EMCSCustomsBrokerageUserControl)form.Controls.Find("EMCSCustomsBrokerageUserControl", true)[0];
				var tabControl = (ZTemplateTabControl)brokerageUserControl.Controls.Find("MainTabControl", true)[0];
				var logsTabPage = brokerageUserControl.FindSingle<ZTabPage>("EventTabPage");
				tabControl.SelectedTab = logsTabPage;
				var logsMainTab = (ZTabControl)logsTabPage.Controls.Find("MainTabControl", true)[0];
				AssertEquals(2, logsMainTab.TabPages.Count);
				AssertEquals("Denied Party Screening Logs", logsMainTab.TabPages[1].Text);
			}
		}

		public void TestPlugIns()
		{
			using (var form = new EMCSDeclarationForm(declaration))
			{
				AssertNotNull(ControllerIDs.JobInvoicing.Name, form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing));
				AssertNotNull(ControllerIDs.eDocsPlugIn.Name, form.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn));
				AssertNotNull(ControllerIDs.DocDataPlugIn.Name, form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn));
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new EMCSDeclarationForm(declaration)
			{
				ControllerID = ControllerIDs.Customs.EU.EMCS
			};
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<EMCSJobDeclaration>();
			Env.Registry.ExternalBorderComplianceTool = ExternalBorderComplianceToolList.Codes.None;
		}

		protected override void TearDown()
		{
			Env.Registry.ExternalBorderComplianceTool = ExternalBorderComplianceToolList.Codes.BorderWiseWeb;
			base.TearDown();
		}

		EMCSJobDeclaration declaration;
	}
}
