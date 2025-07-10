using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ES.ExitControl.Business;
using Enterprise.Customs.EU.ExitControl.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.ES.ExitControl.GUI.Testing
{
	class ViewOnCustomsWebsiteMenuItemCreatorTest : TestCaseWithFactory
	{
		public void TestCreate()
		{
			var exitHeader = Factory.New<CusExitReport>();
			using (var provider = new EU.ExitControl.GUI.Testing.ReportsGridUserControlProviderForTesting())
			{
				var viewOnCustomsWebsiteMenuItemCreator = new ViewOnCustomsWebsiteMenuItemCreator(provider);
				var menuItem = viewOnCustomsWebsiteMenuItemCreator.Create();
				AssertEquals("View on Customs Website", menuItem.Text);
			}
		}

		[RequiresSTA]
		public void TestViewOnCustomsWebsite_Click_Validations()
		{
			var exitHeader = Factory.NewWithValidTestData<CusExitHeader>();

			var consignment1 = exitHeader.CusExitConsignments.AddNew();
			consignment1.CXC_MovementReference = "AAA";
			var report1 = exitHeader.CusExitReports.AddNew();
			report1.CER_CXC_Consignment = consignment1.PK;
			var consignment2 = exitHeader.CusExitConsignments.AddNew();
			consignment2.CXC_MovementReference = "BBB";
			var report2 = exitHeader.CusExitReports.AddNew();
			report2.CER_CXC_Consignment = consignment2.PK;

			Factory.Save();

			using (var form = new ExitControlForm(exitHeader))
			{
				form.Show();

				ZFormModaliser.ShowDialogsInTest = false;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				var reportsTabUserControl = GetReportsTabUserControl(form);
				var grid = reportsTabUserControl.FindSingle<ZArchitecture.ZGrid>("ReportsGrid");

				var requestInboxNotifEntryMenuItem = grid.ContextMenu.MenuItems.FindByText("View on Customs Website");

				grid.Select();
				grid.Focus();

				requestInboxNotifEntryMenuItem.PerformClick();
				AssertEquals("Needs to select a row", "Please select a row first", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[RequiresSTA]
		public void TestViewOnCustomsWebsite_Click()
		{
			var expectedMRN = "AH3RRRRRRNNNNNNNN";
			var expectedUrl = "https://www1.agenciatributaria.gob.es/wlpl/ADEX-JDIT/AesDetalleDecLlegada?wMrn=" + expectedMRN;

			var exitHeader = Factory.New<CusExitHeader>();
			var exitReport = exitHeader.CusExitReports.AddNew();
			var consignment = exitReport.Header.CusExitConsignments.AddNew();
			consignment.CXC_MovementReference = expectedMRN;
			exitReport.CER_CXC_Consignment = consignment.PK;
			exitReport.CER_MessageStatus = "ACC";

			using (var form = new ExitControlForm(exitHeader))
			{
				WebUrlLauncher.ClearLastUrlLaunched();
				form.Show();

				ZFormModaliser.ShowDialogsInTest = false;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				var reportsTabUserControl = GetReportsTabUserControl(form);
				var grid = reportsTabUserControl.FindSingle<ZArchitecture.ZGrid>("ReportsGrid");

				var requestInboxNotifEntryMenuItem = grid.ContextMenu.MenuItems.FindByText("View on Customs Website");

				grid.SelectAllElements();

				requestInboxNotifEntryMenuItem.PerformClick();
				AssertEquals("The correct url has been launched", expectedUrl, WebUrlLauncher.LastUrlLaunched);
			}
		}

		ReportsTabUserControl GetReportsTabUserControl(ExitControlForm form)
		{
			var exitControlUserControl = form.FindSingle<ZUserControl>("ExitControlUserControl");
			var exitControlTabControl = exitControlUserControl.FindSingle<ZTabControl>("ExitControlTabControl");
			exitControlTabControl.SelectTab("ReportsTabPage");

			return (ReportsTabUserControl)exitControlUserControl.FindSingle<ZUserControl>("ReportsTabUserControl");
		}
	}
}
