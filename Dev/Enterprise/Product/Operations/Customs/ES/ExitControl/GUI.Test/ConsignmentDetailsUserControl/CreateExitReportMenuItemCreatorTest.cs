using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.ES.ExitControl.Business;
using Enterprise.Customs.EU.ExitControl.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.ExitControl.GUI.Testing
{
	class CreateExitReportMenuItemCreatorTest : TestCaseWithFactory
	{
		public void TestCreateExitReport_Click()
		{
			CombineAssertions(() =>
			{
				var exitHeder = Factory.New<CusExitHeader>();
				var consignment1 = exitHeder.CusExitConsignments.AddNew();
				var consignment2 = exitHeder.CusExitConsignments.AddNew();

				using (var form = new ExitControlForm(exitHeder))
				{
					form.Show();

					var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
					mainTabControl.SelectTab("MainTabPage");

					var exitControlUserControl = form.FindSingle<ZUserControl>("ExitControlUserControl");
					var exitControlTabControl = exitControlUserControl.FindSingle<ZTabControl>("ExitControlTabControl");
					exitControlTabControl.SelectTab("ConsignmentsTabPage");

					var consignmentsTabUserControl = exitControlUserControl.FindSingle<ZUserControl>("ConsignmentsTabUserControl");
					var consignmentTabControl = consignmentsTabUserControl.FindSingle<ZTabControl>("ConsignmentTabControl");
					consignmentTabControl.SelectTab("ConsignmentItemsTabPage");
					var consignmentsSplitContainer = consignmentsTabUserControl.FindSingle<KSplitContainer>("ConsignmentsSplitContainer");
					var consignmentsGridUserControl = consignmentsSplitContainer.Panel1.FindSingle<ConsignmentsGridUserControl>("ConsignmentsGridUserControl");
					var grid = consignmentsGridUserControl.FindSingle<ZGrid>("ConsignmentsGrid");
					grid.Select(0);
					grid.Select(1);
					var contextMenu = grid.ContextMenu;
					var createExitReportMenuItem = contextMenu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Name == "CreateExitReportMenuItem");
					UnitTestUserNotification.Instance.ClearMessages();
					createExitReportMenuItem.PerformClick();
					AssertEquals("multiple consignments now possible", null, UnitTestUserNotification.Instance.LastMessage.Text);

					var consignmentItem1 = consignment1.CusExitConsignmentItems.AddNew();
					consignmentItem1.CCI_LineNumber = 1;
					consignmentItem1.CCI_DiscrepancyStatus = ZString.Empty;
					consignmentItem1.CCI_Calc_ReportGrossMass = 50m;
					consignmentItem1.CCI_Calc_ReportNetMass = 25m;

					var supdoc1 = consignmentItem1.AdditionalInfos.AddNew();
					supdoc1.CSI_ItemNumber = 1;
					supdoc1.CSI_Code = "9001";
					supdoc1.CSI_ReferenceNumber = "REF1";
					supdoc1.CSI_SubType = "TRA";
					supdoc1.CSI_Status = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;

					var consignmentPackagePivot1 = consignmentItem1.CusExitConsignmentPackagePivots.AddNew();
					var package1 = consignmentPackagePivot1.Package;
					package1.CXP_MarksAndNumbersStatus = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;
					package1.CXP_Calc_ReportQuantity = 3;

					grid.UnSelectAll();
					grid.Select(0);
					UnitTestUserNotification.Instance.ClearMessages();
					createExitReportMenuItem.PerformClick();
					AssertEquals("Multiple reports created", 2, exitHeder.CusExitReports.Count);
					AssertEquals("One reportItem created", 1, exitHeder.CusExitReports[0].CusExitReportItems.Count);

					var reportItem1 = (CusExitReportItem)package1.CusExitReportItems[0];
					AssertEquals("reportItem1.ERI_CCI_ConsignmentItem", consignmentItem1.PK, reportItem1.ERI_CCI_ConsignmentItem);
					AssertEquals("reportItem1.ERI_GrossMass", 50m, reportItem1.ERI_GrossMass);
					AssertEquals("reportItem1.ERI_NetMass", 25m, reportItem1.ERI_NetMass);
					AssertEquals("reportItem1.ERI_Quantity", 3, reportItem1.ERI_Quantity);
					AssertEquals("reportItem1 is associated correctly to package1", package1.PK, reportItem1.ERI_CXP_Package);

					AssertEquals("reportItem1.AdditionalInfos is 1", 1, reportItem1.AdditionalInfos.Count);
					var report1AddInfo = reportItem1.AdditionalInfos[0];
					AssertEquals("report1AddInfo with correct data, CSI_ItemNumber", 1, report1AddInfo.CSI_ItemNumber);
					AssertEquals("report1AddInfo with correct data, CSI_Code", "9001", report1AddInfo.CSI_Code);
					AssertEquals("report1AddInfo with correct data, CSI_ReferenceNumber", "REF1", report1AddInfo.CSI_ReferenceNumber);
					AssertEquals("report1AddInfo with correct data, CSI_SubType", "TRA", report1AddInfo.CSI_SubType);
					AssertEquals("report1AddInfo with correct data, CSI_Status", "DIF", report1AddInfo.CSI_Status);
				}
			});
		}
	}
}
