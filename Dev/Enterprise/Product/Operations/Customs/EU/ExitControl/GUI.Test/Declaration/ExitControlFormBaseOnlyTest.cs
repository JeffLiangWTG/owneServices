using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Application.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing;

[TestedType(typeof(ExitControlForm))]
sealed class ExitControlFormBaseOnlyTest : ExitControlFormAbstractTest<CusExitHeader>
{
	public void TestAuditPluginLoaded()
	{
		using var form = new ExitControlForm(exitHeader);
		AssertNotNull("AuditPlugin", form.PlugIns.IsPlugInAvailable(ZArchitecture.Modules.ControllerIDs.Audit));
	}

	public void TestDocDataPlugIn()
	{
		using var form = new ExitControlForm(exitHeader);
		AssertNotNull("DocDataPlugIn", form.PlugIns.GetPlugIn(ZArchitecture.Modules.ControllerIDs.DocDataPlugIn));
	}

	public void TestFormCaption()
	{
		using (var form = new ExitControlForm(exitHeader))
		{
			form.Show();
			AssertEquals("Exit Control 456", form.FormCaption);
		}
	}

	public void TestIExitControlMainMenuSupporterImplemented()
	{
		var exitControlMenuProviders = new KeyObjectHandleDictionaryObject
		{
			{ "LV", new TestObjectHandle(new ExitControlMenuProviderForTest()) }
		};

		using (ObjectFactory.Substitute("ExitControlMenuProviders", exitControlMenuProviders))
		{
			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Latvia;
			var exitHeader = Factory.New<CusExitHeader>();
			exitHeader.CXH_GC_Company = company.PK;
			exitHeader.CusExitConsignments.AddNew();
			exitHeader.CusExitReports.AddNew();

			using (var form = new ExitControlForm(exitHeader))
			{
				form.Show();
				var exitControlUserControl = form.ExitControlUserControl;
				var exitControlTabControl = exitControlUserControl.ExitControlTabControl;
				exitControlTabControl.SelectedTab = exitControlUserControl.ConsignmentsTabPage;
				exitControlTabControl.SelectedTab = exitControlUserControl.ReportsTabPage;
				var exitControlMenuItems = form.Menu.MenuItems.FindByText("E&xit Control").MenuItems;
				CombineAssertions("Exit Control menu items", () =>
				{
					AssertEquals(4, exitControlMenuItems.Count);
					AssertEquals("&Create Exit Report", exitControlMenuItems[0].Text);
					AssertEquals("&Select/Edit Report Items", exitControlMenuItems[1].Text);
					AssertEquals("-", exitControlMenuItems[2].Text);
					AssertEquals("Send to Customs", exitControlMenuItems[3].Text);
				});
			}
		}
	}

	public void TestWorkflowTabAdded()
	{
		using (var form = new ExitControlForm(exitHeader))
		{
			form.Show();
			var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
			var workflowTabPage = form.WorkflowTabPage;
			AssertEquals("MainTabControl contains WorkflowTabPage", true, mainTabControl.Controls.Contains(workflowTabPage));
		}
	}

	public void TestContainersOrEquipmentsAndSealsUserControl()
	{
		using (var form = new ExitControlForm(exitHeader))
		{
			form.Show();
			var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
			var mainTabPage = mainTabControl.GetTabPage("MainTabPage");
			mainTabControl.SelectedTab = mainTabPage;
			var exitControlUserControl = form.ExitControlUserControl;
			var exitControlTabControl = exitControlUserControl.ExitControlTabControl;
			var containersOrEquipmentsTabPage = exitControlUserControl.ContainersOrEquipmentsTabPage;
			exitControlTabControl.SelectedTab = containersOrEquipmentsTabPage;
			var containersOrEquipmentsAndSealsUserControl = containersOrEquipmentsTabPage.FindSingle<ContainersOrEquipmentsAndSealsUserControl>();
			AssertEquals("ContainersOrEquipmentsAndSealsUserControl", containersOrEquipmentsAndSealsUserControl.Name);
		}
	}

	public void TestExitControlTabUserControl()
	{
		using (var form = new ExitControlForm(exitHeader))
		{
			form.Show();
			var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
			var mainTabPage = mainTabControl.GetTabPage("MainTabPage");
			var detailsTabUserControl = form.ExitControlUserControl.DetailsTabUserControl;
			AssertEquals("In MainTabPage", true, mainTabPage.Contains(detailsTabUserControl));
			AssertEquals("MainTabPage Caption", "Exit Control", mainTabPage.CaptionResourceString.Caption);
		}
	}

	public void TestMenuItems()
	{
		using (var form = new ExitControlForm(exitHeader))
		{
			form.Show();

			var menuItems = form.Menu.MenuItems;
			var menuItem = menuItems.FindByText("E&xit Control");

			CombineAssertions(() =>
			{
				AssertSequencesEqual("All menu items", new[] { "&File", "&Edit", "Actio&ns", "E&xit Control", "&Documents", "&Help" }, menuItems.Cast<MenuItem>().Select(x => x.Text));
				AssertType<ExitControlMenuItem>("ExitControl menu item", menuItem);
			});
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		exitHeader = Factory.New<CusExitHeader>();
		exitHeader.CXH_JobReference = "456";
	}
	CusExitHeader exitHeader;
}
