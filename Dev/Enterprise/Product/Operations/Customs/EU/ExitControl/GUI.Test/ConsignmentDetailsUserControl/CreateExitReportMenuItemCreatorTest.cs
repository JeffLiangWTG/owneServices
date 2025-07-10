using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Application.Testing;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing
{
	class CreateExitReportMenuItemCreatorTest : TestCaseWithFactory
	{
		public void TestCreate()
		{
			var header = Factory.New<CusExitHeader>();
			using (var gridProvider = new ConsignmentsGridUserControlProviderForTesting(header))
			{
				var createExitReportMenuItemCreator = new CreateExitReportMenuItemCreator(gridProvider);
				var menuItem = createExitReportMenuItemCreator.Create();
				CombineAssertions(() =>
				{
					AssertEquals("&Create Exit Report", menuItem.Text);
					AssertEquals("CreateExitReportMenuItem", menuItem.Name);
				});
			}
		}

		public void TestCreateExitReport_Click()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.EuropeanUnion;
			var exitHeader = Factory.New<CusExitHeader>();
			exitHeader.CXH_GC_Company = company.PK;
			var consignment1 = exitHeader.CusExitConsignments.AddNew();
			consignment1.CXC_MovementReference = "MRN1";
			var consignment2 = exitHeader.CusExitConsignments.AddNew();
			consignment2.CXC_MovementReference = "MRN2";

			var exitControlMenuProviders = new KeyObjectHandleDictionaryObject
			{
				{ "EU", new TestObjectHandle(new ExitControlMenuProviderForTest()) }
			};

			using (ObjectFactory.Substitute("ExitControlMenuProviders", exitControlMenuProviders))
			using (var form = new ExitControlForm(exitHeader))
			{
				form.Show();

				var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				mainTabControl.SelectTab("MainTabPage");

				var exitControlUserControl = form.ExitControlUserControl;
				var exitControlTabControl = exitControlUserControl.ExitControlTabControl;
				exitControlTabControl.SelectTab(nameof(ExitControlUserControl.ConsignmentsTabPage));

				var consignmentsTabUserControl = exitControlUserControl.ConsignmentsTabUserControl;
				var consignmentItemsTabPage = consignmentsTabUserControl.ConsignmentItemsTabPage;
				consignmentsTabUserControl.ConsignmentTabControl.SelectTab(consignmentItemsTabPage);
				var consignmentsGridUserControl = consignmentsTabUserControl.ConsignmentsSplitContainer.Panel1.FindSingle<ConsignmentsGridUserControl>("ConsignmentsGridUserControl");
				var grid = consignmentsGridUserControl.ConsignmentsGrid;
				var contextMenu = grid.ContextMenu;
				var createExitReportMenuItem = contextMenu.MenuItems.Cast<MenuItem>().Single(x => x.Name == "CreateExitReportMenuItem");

				UnitTestUserNotification.Instance.ClearMessages();
				createExitReportMenuItem.PerformClick();
				AssertEquals("Please select a consignment first.", UnitTestUserNotification.Instance.LastMessage.Text);

				grid.Select(0);
				grid.Select(1);
				UnitTestUserNotification.Instance.ClearMessages();
				createExitReportMenuItem.PerformClick();
				AssertEquals("multiple consignments now possible", null, UnitTestUserNotification.Instance.LastMessage.Text);

				grid.UnSelectAll();
				grid.Select(0);
				UnitTestUserNotification.Instance.ClearMessages();
				createExitReportMenuItem.PerformClick();
				AssertEquals("A consignment should have at least one item.", UnitTestUserNotification.Instance.LastMessage.Text);

				var consignmentItem1 = consignment1.CusExitConsignmentItems.AddNew();
				var consignmentItem2 = consignment1.CusExitConsignmentItems.AddNew();
				var consignmentPackagePivot1 = consignmentItem1.CusExitConsignmentPackagePivots.AddNew();
				var package1 = consignmentPackagePivot1.Package;
				UnitTestUserNotification.Instance.ClearMessages();
				createExitReportMenuItem.PerformClick();
				AssertEquals("All items should have at least one package.", UnitTestUserNotification.Instance.LastMessage.Text);

				var consignmentPackagePivot2 = consignmentItem2.CusExitConsignmentPackagePivots.AddNew();
				var package2 = consignmentPackagePivot2.Package;
				UnitTestUserNotification.Instance.ClearMessages();
				createExitReportMenuItem.PerformClick();
				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormShown(obj =>
				{
					var dialog = (ExitReportCreationSelectionForm)obj;
					var exitConsignment = dialog.BusinessEntity;
					foreach (var item in exitConsignment.CusExitConsignmentItems)
					{
						item.CCI_Calc_ShouldReportItem = true;
						item.CCI_Calc_ReportGrossMass = 50m;
						item.CCI_Calc_ReportNetMass = 30m;
						foreach (var pivot in item.CusExitConsignmentPackagePivots)
						{
							var package = pivot.Package;
							if (package != null)
							{
								package.CXP_Calc_ShouldReportItem = true;
								package.CXP_Calc_ReportQuantity = 2;
							}
						}
					}
					dialog.FindSingle<ZButton>("OKButton").PerformClick();
				});
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				createExitReportMenuItem.PerformClick();
				AssertEquals("Report is created (multiple: 2, single: 1)", 3, exitHeader.CusExitReports.Count);
				AssertEquals("Report's MRN", "MRN1", exitHeader.CusExitReports[0].Consignment.CXC_MovementReference);
			}
		}

		public void TestCreateExitReport_Click_MultipleSelection_NoWarning()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.EuropeanUnion;
			var exitHeader = Factory.New<CusExitHeader>();
			exitHeader.CXH_GC_Company = company.PK;
			var consignment1 = exitHeader.CusExitConsignments.AddNew();
			consignment1.CXC_MovementReference = "MRN1";
			var consignment2 = exitHeader.CusExitConsignments.AddNew();
			consignment2.CXC_MovementReference = "MRN2";
			var consignment3 = exitHeader.CusExitConsignments.AddNew();
			consignment3.CXC_MovementReference = "MRN3";

			var exitControlMenuProviders = new KeyObjectHandleDictionaryObject
			{
				{ "EU", new TestObjectHandle(new ExitControlMenuProviderForTest()) }
			};
			using (ObjectFactory.Substitute("ExitControlMenuProviders", exitControlMenuProviders))
			using (var form = new ExitControlForm(exitHeader))
			{
				form.Show();

				var grid = GetConsignmentsGrid(form);
				var contextMenu = grid.ContextMenu;
				var createExitReportMenuItem = contextMenu.MenuItems.Cast<MenuItem>()
					.FirstOrDefault(x => x.Name == "CreateExitReportMenuItem");

				grid.Select(0);
				grid.Select(1);
				UnitTestUserNotification.Instance.ClearMessages();
				createExitReportMenuItem.PerformClick();
				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Exit reports count", 2, exitHeader.CusExitReports.Count);
				AssertContainsExactElementsInAnyOrder(new[] { "MRN1", "MRN2" },
					exitHeader.CusExitReports.Select(x => x.Consignment.CXC_MovementReference));
			}
		}

		public void TestCreateExitReport_Click_MultipleSelection_Warning()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.EuropeanUnion;
			var exitHeader = Factory.New<CusExitHeader>();
			exitHeader.CXH_GC_Company = company.PK;
			var consignment1 = exitHeader.CusExitConsignments.AddNew();
			consignment1.CXC_MovementReference = "MRN1";
			var consignment2 = exitHeader.CusExitConsignments.AddNew();
			consignment2.CXC_MovementReference = "MRN2";
			var consignment3 = exitHeader.CusExitConsignments.AddNew();
			consignment3.CXC_MovementReference = "MRN3";
			var consignment4 = exitHeader.CusExitConsignments.AddNew();
			consignment4.CXC_MovementReference = "MRN4";

			_ = consignment1.CusExitConsignmentItems.AddNew();
			_ = consignment4.CusExitConsignmentItems.AddNew();

			var exitControlMenuProviders = new KeyObjectHandleDictionaryObject
			{
				{ "EU", new TestObjectHandle(new ExitControlMenuProviderForTest()) }
			};
			using (ObjectFactory.Substitute("ExitControlMenuProviders", exitControlMenuProviders))
			using (var form = new ExitControlForm(exitHeader))
			{
				form.Show();

				var grid = GetConsignmentsGrid(form);
				var contextMenu = grid.ContextMenu;
				var createExitReportMenuItem = contextMenu.MenuItems.Cast<MenuItem>()
					.FirstOrDefault(x => x.Name == "CreateExitReportMenuItem");

				grid.Select(0);
				grid.Select(1);
				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				createExitReportMenuItem.PerformClick();
				AssertEquals("For the following consignments a report will not be created due to existing items and/or packing details: MRN1",
					UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Cancelled. Reports are not created", 0, exitHeader.CusExitReports.Count);

				grid.Select(0);
				grid.Select(1);
				grid.Select(3);
				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				createExitReportMenuItem.PerformClick();
				AssertEquals("For the following consignments a report will not be created due to existing items and/or packing details: MRN1, MRN4",
					UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Cancelled. Reports are not created", 0, exitHeader.CusExitReports.Count);

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				createExitReportMenuItem.PerformClick();
				AssertEquals("For the following consignments a report will not be created due to existing items and/or packing details: MRN1, MRN4",
					UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Report is created for suitable consignments", 1, exitHeader.CusExitReports.Count);
				AssertEquals("Report's MRN", "MRN2", exitHeader.CusExitReports[0].Consignment.CXC_MovementReference);
			}
		}

		static ZGrid GetConsignmentsGrid(ExitControlForm form)
		{
			var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
			mainTabControl.SelectTab("MainTabPage");

			var exitControlUserControl = form.ExitControlUserControl;
			var exitControlTabControl = exitControlUserControl.ExitControlTabControl;
			exitControlTabControl.SelectTab(nameof(ExitControlUserControl.ConsignmentsTabPage));

			var consignmentsTabUserControl = exitControlUserControl.ConsignmentsTabUserControl;
			var consignmentItemsTabPage = consignmentsTabUserControl.ConsignmentItemsTabPage;
			consignmentsTabUserControl.ConsignmentTabControl.SelectTab(consignmentItemsTabPage);
			var consignmentsGridUserControl = consignmentsTabUserControl.ConsignmentsSplitContainer.Panel1.FindSingle<ConsignmentsGridUserControl>("ConsignmentsGridUserControl");
			return consignmentsGridUserControl.ConsignmentsGrid;
		}
	}

	sealed class ExitControlMenuProviderForTest : IExitControlMenuProvider
	{
		IExitControlMainMenuProvider IExitControlMenuProvider.GetExitControlMainMenuProvider(CusExitHeader header) => new ExitControlMainMenuProvider(header);
		IConsignmentsGridUserControlMenuProvider IExitControlMenuProvider.GetConsignmentsGridUserControlMenuProvider(IConsignmentsGridUserControlProvider provider) => new ConsignmentsGridUserControlMenuProvider(provider);
		IReportsGridUserControlMenuProvider IExitControlMenuProvider.GetReportsGridUserControlMenuProvider(IReportsGridUserControlProvider provider) => new ReportsGridUserControlMenuProvider(provider);
	}
}
