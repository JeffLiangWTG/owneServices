using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Application.Testing;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing
{
	class SelectReportItemMenuItemCreatorTest : TestCaseWithFactory
	{
		public void TestCreate()
		{
			using (var provider = new ReportsGridUserControlProviderForTesting())
			{
				var selectReportItemMenuItemCreator = new SelectReportItemMenuItemCreator(provider);
				var menuItem = selectReportItemMenuItemCreator.Create();
				AssertEquals("&Select/Edit Report Items", menuItem.Text);
			}
		}

		public void TestSelectReportItemMemuItem_OnClick()
		{
			var exitControlMenuProviders = new KeyObjectHandleDictionaryObject
			{
				{ "LV", new TestObjectHandle(new ExitControlMenuProviderForTest()) }
			};

			using (ObjectFactory.Substitute("ExitControlMenuProviders", exitControlMenuProviders))
			{
				var company = Factory.New<GlbCompany>();
				company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Latvia;
				var exitHeder = Factory.New<CusExitHeader>();
				exitHeder.CXH_GC_Company = company.PK;
				var report1 = exitHeder.CusExitReports.AddNew();
				var report2 = exitHeder.CusExitReports.AddNew();

				using (var form = new ExitControlForm(exitHeder))
				{
					form.Show();

					var exitControlUserControl = form.ExitControlUserControl;
					var exitControlTabControl = exitControlUserControl.ExitControlTabControl;
					exitControlTabControl.SelectedTab = exitControlUserControl.ReportsTabPage;

					var reportsTabUserControl = exitControlUserControl.ReportsTabUserControl;
					var grid = reportsTabUserControl.ReportsGrid;
					grid.Select(0);
					grid.Select(1);
					var selectReportItemsMenuItem = grid.ContextMenu.MenuItems.FindByText("&Select/Edit Report Items");
					UnitTestUserNotification.Instance.ClearMessages();
					selectReportItemsMenuItem.PerformClick();
					AssertEquals("Multi selection is not allowed.", UnitTestUserNotification.Instance.LastMessage.Text);

					grid.UnSelectAll();
					UnitTestUserNotification.Instance.ClearMessages();
					selectReportItemsMenuItem.PerformClick();
					AssertEquals("Please select an Exit Report first.", UnitTestUserNotification.Instance.LastMessage.Text);

					grid.Select(0);
					UnitTestUserNotification.Instance.ClearMessages();
					selectReportItemsMenuItem.PerformClick();
					AssertEquals("'Entry/Consignment' must have a valid value before selecting Report Items.", UnitTestUserNotification.Instance.LastMessage.Text);

					var consignment1 = exitHeder.CusExitConsignments.AddNew();
					var consignmentItem1 = consignment1.CusExitConsignmentItems.AddNew();
					consignmentItem1.CusExitConsignmentPackagePivots.AddNew();
					report1.CER_CXC_Consignment = consignment1.PK;
					var consignment2 = exitHeder.CusExitConsignments.AddNew();
					var consignmentItem2 = consignment2.CusExitConsignmentItems.AddNew();
					consignmentItem2.CusExitConsignmentPackagePivots.AddNew();
					report2.CER_CXC_Consignment = consignment1.PK;
					UnitTestUserNotification.Instance.ClearMessages();
					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.SetDelegateToCallOnFormShown(obj =>
					{
						AssertType<ExitReportCreationSelectionForm>(obj);
					});
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
					selectReportItemsMenuItem.PerformClick();
					AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}
	}
}
