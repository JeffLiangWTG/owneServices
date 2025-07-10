using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.GUI.Testing
{
	[TestedType(typeof(JPAFRForm))]
	class JPAFRFormTest : ZFormBasherTest
	{
		public void TestFormCaption()
		{
			var header = Factory.New<JPAFRHeader>();
			using (var form = new JPAFRForm(header))
			{
				form.Show();
				AssertContains(header.HumanReadableName, form.FormCaption);
				form.FireSaveButton();
				AssertContains(header.HumanReadableName, form.FormCaption);
			}
		}

		public void TestSelectAndShowBill()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();

			using (var form = new JPAFRForm(header))
			{
				form.Show();
				Application.DoEvents();

				var billsUserControl = GetControl<JPAFRBillsUserControl>(form, "jpafrBillsUserControl");
				var grid = GetControl<JPAFRBillsUserControl, ZGrid>(billsUserControl, "BillsGrid");

				AssertEquals("hidden to begin with", false, grid.Visible);

				form.SelectAndShowBill(bill1.PK);
				AssertEquals("should be shown now", true, grid.Visible);
				AssertEquals("should have selected bill1", bill1, grid.ListManager.GetCurrent());

				form.SelectAndShowBill(bill2.PK);
				AssertEquals("should have selected bill2", bill2, grid.ListManager.GetCurrent());
			}
		}

		public void TestDifferentControlsForDifferentJobTypes()
		{
			var testHeader = Factory.New<JPAFRHeader>();
			testHeader.JPH_IsShippingLineEntry = false;
			using (var form = new JPAFRForm(testHeader))
			{
				form.Show();
				var jpAFRSplitContainer = GetControl<SplitContainer>(form, "JPAFRSplitContainer");
				AssertNotNull(jpAFRSplitContainer);
				AssertNotNull(GetControl<SailingUserControl>(form, "sailingUserControl"));

				var jPH_DischargePortSuffixTextBox = form.Controls.Find("JPH_DischargePortSuffixTextBox", true).FirstOrDefault() as ZTextBox;
				AssertNotNull(jPH_DischargePortSuffixTextBox);
				AssertEquals(false, jPH_DischargePortSuffixTextBox.Visible);

				var jPH_MasterBillNumberTextBox = form.Controls.Find("JPH_MasterBillNumberTextBox", true).FirstOrDefault() as ZTextBox;
				AssertNotNull(jPH_MasterBillNumberTextBox);
				AssertEquals(true, jPH_MasterBillNumberTextBox.Visible);

				AssertEquals(true, jpAFRSplitContainer.Panel1Collapsed);

				var mainTabControl = form.Controls.Find("MainTabControl", true).FirstOrDefault() as ZTabControl;
				mainTabControl.SelectedIndex = 1;
				var billsTabControl = mainTabControl.SelectedTab.Controls.Find("BillDetailsTabControl", true).FirstOrDefault() as ZTabControl;
				billsTabControl.SelectedIndex = 0;
				var containerOperaterCodeTextBox = form.Controls.Find("ContainerOperaterCodeTextBox", true).FirstOrDefault() as ZTextBox;
				AssertNotNull(containerOperaterCodeTextBox);
				AssertEquals(false, containerOperaterCodeTextBox.Visible);

				var isMasterBillCheckBox = form.Controls.Find("IsMasterBillCheckBox", true).FirstOrDefault() as ZCheckBox;
				AssertNotNull(isMasterBillCheckBox);
				AssertEquals(false, isMasterBillCheckBox.Visible);

				billsTabControl.SelectedIndex = 2;
				var jPB_Calc_GeneralCustomsTransitApprovalNumberTextBox = form.Controls.Find("JPB_Calc_GeneralCustomsTransitApprovalNumberTextBox", true).FirstOrDefault() as ZTextBox;
				AssertNotNull(jPB_Calc_GeneralCustomsTransitApprovalNumberTextBox);
				AssertEquals(false, jPB_Calc_GeneralCustomsTransitApprovalNumberTextBox.Visible);

				var containersGrid = form.Controls.Find("ContainersGrid", true).FirstOrDefault() as ZGrid;
				AssertNotNull(containersGrid);
				AssertEquals("JPAFRNVOCCContainers", containersGrid.ColumnLayoutContext);
				AssertEquals(6, containersGrid.ColumnStyles.Count);
				AssertEquals(false, containersGrid.ColumnStyles.OfType<ZGridColumnInfo>().Any(a => a.ColumnName == "JPC_TypeOfService"));
				AssertEquals(false, containersGrid.ColumnStyles.OfType<ZGridColumnInfo>().Any(a => a.ColumnName == "JPC_VanningType"));
				AssertEquals(false, containersGrid.ColumnStyles.OfType<ZGridColumnInfo>().Any(a => a.ColumnName == "JPC_CCCApplicationId"));
				AssertEquals(false, containersGrid.ColumnStyles.OfType<ZGridColumnInfo>().Any(a => a.ColumnName == "JPC_SearchExclusionIdForCheckBox"));
			}

			testHeader.JPH_IsShippingLineEntry = true;
			using (var form = new JPAFRForm(testHeader))
			{
				form.Show();
				var jpAFRSplitContainer = GetControl<SplitContainer>(form, "JPAFRSplitContainer");
				AssertNotNull(jpAFRSplitContainer);
				AssertNotNull(GetControl<SailingUserControl>(form, "sailingUserControl"));

				var jPH_DischargePortSuffixTextBox = form.Controls.Find("JPH_DischargePortSuffixTextBox", true).FirstOrDefault() as ZTextBox;
				AssertNotNull(jPH_DischargePortSuffixTextBox);
				AssertEquals(true, jPH_DischargePortSuffixTextBox.Visible);

				var jPH_MasterBillNumberTextBox = form.Controls.Find("JPH_MasterBillNumberTextBox", true).FirstOrDefault() as ZTextBox;
				AssertNotNull(jPH_MasterBillNumberTextBox);
				AssertEquals(false, jPH_MasterBillNumberTextBox.Visible);

				AssertEquals(false, jpAFRSplitContainer.Panel1Collapsed);

				var mainTabControl = form.Controls.Find("MainTabControl", true).FirstOrDefault() as ZTabControl;
				mainTabControl.SelectedIndex = 1;
				var billsTabControl = mainTabControl.SelectedTab.Controls.Find("BillDetailsTabControl", true).FirstOrDefault() as ZTabControl;
				billsTabControl.SelectedIndex = 0;
				var containerOperaterCodeTextBox = form.Controls.Find("ContainerOperaterCodeTextBox", true).FirstOrDefault() as ZTextBox;
				AssertNotNull(containerOperaterCodeTextBox);
				AssertEquals(true, containerOperaterCodeTextBox.Visible);

				var isMasterBillCheckBox = form.Controls.Find("IsMasterBillCheckBox", true).FirstOrDefault() as ZCheckBox;
				AssertNotNull(isMasterBillCheckBox);
				AssertEquals(true, isMasterBillCheckBox.Visible);

				billsTabControl.SelectedIndex = 2;
				var jPB_Calc_GeneralCustomsTransitApprovalNumberTextBox = form.Controls.Find("JPB_Calc_GeneralCustomsTransitApprovalNumberTextBox", true).FirstOrDefault() as ZTextBox;
				AssertNotNull(jPB_Calc_GeneralCustomsTransitApprovalNumberTextBox);
				AssertEquals(true, jPB_Calc_GeneralCustomsTransitApprovalNumberTextBox.Visible);

				var containersGrid = form.Controls.Find("ContainersGrid", true).FirstOrDefault() as ZGrid;
				AssertNotNull(containersGrid);
				AssertEquals("JPAFRVOCCContainers", containersGrid.ColumnLayoutContext);
				AssertEquals(10, containersGrid.ColumnStyles.Count);
				AssertEquals(true, containersGrid.ColumnStyles.OfType<ZGridColumnInfo>().Any(a => a.ColumnName == "JPC_TypeOfService"));
				AssertEquals(true, containersGrid.ColumnStyles.OfType<ZGridColumnInfo>().Any(a => a.ColumnName == "JPC_VanningType"));
				AssertEquals(true, containersGrid.ColumnStyles.OfType<ZGridColumnInfo>().Any(a => a.ColumnName == "JPC_CCCApplicationId"));
				AssertEquals(true, containersGrid.ColumnStyles.OfType<ZGridColumnInfo>().Any(a => a.ColumnName == "JPC_SearchExclusionIdForCheckBox"));
			}
		}

		public void TestImportFromSailingMenuItem_NVOCC()
		{
			var header = Factory.New<JPAFRHeader>();
			var voyage = Factory.New<JobVoyage>();
			var origin = voyage.Origins.AddNew();
			var destination = voyage.Destinations.AddNew();
			var sailing = Factory.New<JobSailing>();
			sailing.JX_JB = destination.PK;
			sailing.JX_JA = origin.PK;
			using (var form = new JPAFRForm(header))
			{
				form.Show();
				var actionMenuItem = form.Menu.MenuItems.FindByText("AFR");
				var importFromSailingMenuItem = actionMenuItem.MenuItems.FindByText(ImportBillsFromSailingMenuItem);
				AssertNull(importFromSailingMenuItem);
			}
		}

		public void TestImportFromSailingMenuItem_VOCC()
		{
			const string noSailingSelectedMessage = "There is no Sailing Schedule to import Bills Of Lading.";
			var header = Factory.New<JPAFRHeader>();
			header.JPH_IsShippingLineEntry = true;
			var voyage = Factory.New<JobVoyage>();
			var origin = voyage.Origins.AddNew();
			var destination = voyage.Destinations.AddNew();
			var sailing = Factory.New<JobSailing>();
			sailing.JX_JB = destination.PK;
			sailing.JX_JA = origin.PK;
			using (var form = new JPAFRForm(header))
			{
				form.Show();
				var actionMenuItem = form.Menu.MenuItems.FindByText("AFR");
				var importFromSailingMenuItem = actionMenuItem.MenuItems.FindByText(ImportBillsFromSailingMenuItem);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				importFromSailingMenuItem.PerformClick();
				AssertEquals(noSailingSelectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				header.ChangeSailing(sailing.PK);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				importFromSailingMenuItem.PerformClick();
				AssertNotEquals(noSailingSelectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		T GetControl<T>(JPAFRForm form, string name)
			where T : Control
		{
			return GetControl<JPAFRForm, T>(form, name);
		}

		T GetControl<P, T>(P parent, string name)
			where P : Control
			where T : Control
		{
			return (T)typeof(P).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(parent);
		}

		protected override Form GetFormToBashCore()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill = header.Bills.AddNew();
			bill.Containers.AddNew();
			Factory.Save();
			var result = new JPAFRForm(header);
			result.ControllerID = ControllerIDs.Customs.JP.AFR;
			return result;
		}

		const string ImportBillsFromSailingMenuItem = "Import Bills From Sailing Schedule";
	}
}
