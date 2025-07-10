using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class AUCustomsMultiPackPerContainerControlTest : TestCaseWithFactory
	{
		public void TestGridId()
		{
			using (var control = new PackingUserControl())
			{
				AssertEquals("GridLayoutio4uPJ92QiJBDALlyBv5Jw==", control.PackingDetailsGrid.GridId);
			}
		}

		public void TestHouseBillGroupBoxText()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			using (ZAUCustomsDeclarationForm testForm = new ZAUCustomsDeclarationForm(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.PackingTabPage;
				PackingUserControl packingControl = testForm.CustomsBrokerageUserControl.Packing as PackingUserControl;
				AssertEquals("Group box text", "Master and House Bills", packingControl.HouseBillsGroupBox.Text);
			}
		}

		public void TestHouseBillGrid()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			using (ZAUCustomsDeclarationForm testForm = new ZAUCustomsDeclarationForm(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.PackingTabPage;
				PackingUserControl packingControl = testForm.CustomsBrokerageUserControl.Packing as PackingUserControl;
				AssertEquals("HouseBill", true, packingControl.HouseBillsGrid.ColumnStyles[1].ToString().StartsWith(CusDecHouseBillSchema.Constants.CU_BillNum));
			}
		}

		public void TestPartShipConsignmentRefNoVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ZAUCustomsDeclarationForm testForm = new ZAUCustomsDeclarationForm(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.PackingTabPage;
				var packingControl = testForm.CustomsBrokerageUserControl.Packing as PackingUserControl;
				AssertNull("Should not be visible for sea", packingControl.HouseBillsGrid.Columns[Bill.Schema.CU_fPartShipConsignmentReference]);
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.DeclarationTabPage;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.PackingTabPage;
				AssertNotNull("Should be visible for air", packingControl.HouseBillsGrid.Columns[Bill.Schema.CU_fPartShipConsignmentReference]);
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.DeclarationTabPage;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.ExWarehouse;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.PackingTabPage;
				AssertNull("Should not be visible for ex-warehouse", packingControl.HouseBillsGrid.Columns[Bill.Schema.CU_fPartShipConsignmentReference]);
			}
		}

		public void TestControlVisibility()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			using (ZAUCustomsDeclarationForm testForm = new ZAUCustomsDeclarationForm(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_ApplicationCode = "CMR";
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.PackingTabPage;
				PackingUserControl packingControl = testForm.CustomsBrokerageUserControl.Packing as PackingUserControl;
				AssertEquals("Export Label Not Visible", false, packingControl.ExportLabel.Visible);
				AssertEquals("House Bills are visible", true, packingControl.HouseBillsGroupBox.Visible);
				AssertEquals("Packing details are visible", true, packingControl.PackingDetailsGroupBox.Visible);
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.DeclarationTabPage;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Other;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.ContainerTabPage;
				AssertEquals(false, testForm.CustomsBrokerageUserControl.PackingTabPage.TabVisible);
				packingControl = testForm.CustomsBrokerageUserControl.Packing as PackingUserControl;
				AssertEquals("Export Label Not Visible", false, packingControl.ExportLabel.Visible);
				AssertEquals("Export Label Dock Style", DockStyle.Fill, packingControl.ExportLabel.Dock);
				AssertEquals("House Bills are visible", false, packingControl.HouseBillsGroupBox.Visible);
				AssertEquals("House Label Dock Style", DockStyle.Fill, packingControl.HouseBillPanel.Dock);
				AssertEquals("Packing details are not visible", false, packingControl.PackingDetailsGroupBox.Visible);
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.DeclarationTabPage;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.ContainerTabPage;
				AssertEquals(false, testForm.CustomsBrokerageUserControl.PackingTabPage.TabVisible);
				packingControl = testForm.CustomsBrokerageUserControl.Packing as PackingUserControl;
				AssertEquals("Export Label Not Visible", false, packingControl.ExportLabel.Visible);
				AssertEquals("Export Label Dock Style", DockStyle.Fill, packingControl.ExportLabel.Dock);
				AssertEquals("House Bills are visible", false, packingControl.HouseBillsGroupBox.Visible);
				AssertEquals("House Label Dock Style", DockStyle.Fill, packingControl.HouseBillPanel.Dock);
				AssertEquals("Packing details are not visible", false, packingControl.PackingDetailsGroupBox.Visible);
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.DeclarationTabPage;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.ExWarehouse;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_ApplicationCode = "CMR";
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.ContainerTabPage;
				AssertEquals(false, testForm.CustomsBrokerageUserControl.PackingTabPage.TabVisible);
				packingControl = testForm.CustomsBrokerageUserControl.Packing as PackingUserControl;
				AssertEquals("Export Label is Visible", false, packingControl.ExportLabel.Visible);
				AssertEquals("Export Label Dock Style", DockStyle.Fill, packingControl.ExportLabel.Dock);
				AssertEquals("House Bills are not visible", false, packingControl.HouseBillsGroupBox.Visible);
				AssertEquals("House Label Dock Style", DockStyle.Fill, packingControl.HouseBillPanel.Dock);
				AssertEquals("Packing details are not visible", false, packingControl.PackingDetailsGroupBox.Visible);
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.DeclarationTabPage;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_ApplicationCode = "CMR";
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.PackingTabPage;
				AssertEquals(true, testForm.CustomsBrokerageUserControl.PackingTabPage.TabVisible);
				packingControl = testForm.CustomsBrokerageUserControl.Packing as PackingUserControl;
				AssertEquals("Export Label Not Visible", false, packingControl.ExportLabel.Visible);
				AssertEquals("Export Label Dock Style", DockStyle.Fill, packingControl.ExportLabel.Dock);
				AssertEquals("House Bills are visible", true, packingControl.HouseBillsGroupBox.Visible);
				AssertEquals("House Label Dock Style", DockStyle.Top, packingControl.HouseBillPanel.Dock);
				AssertEquals("Packing details are visible", true, packingControl.PackingDetailsGroupBox.Visible);
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.DeclarationTabPage;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearance;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_ApplicationCode = "CMR";
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.ContainerTabPage;
				AssertEquals(false, testForm.CustomsBrokerageUserControl.PackingTabPage.TabVisible);
				packingControl = testForm.CustomsBrokerageUserControl.Packing as PackingUserControl;
				AssertEquals("Export Label Not Visible", false, packingControl.ExportLabel.Visible);
				AssertEquals("Export Label Dock Style", DockStyle.Fill, packingControl.ExportLabel.Dock);
				AssertEquals("House Bills are not visible", false, packingControl.HouseBillsGroupBox.Visible);
				AssertEquals("House Label Dock Style", DockStyle.Fill, packingControl.HouseBillPanel.Dock);
				AssertEquals("Packing details are not visible", false, packingControl.PackingDetailsGroupBox.Visible);
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.DeclarationTabPage;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearanceWithLines;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_ApplicationCode = "CMR";
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.ContainerTabPage;
				AssertEquals(false, testForm.CustomsBrokerageUserControl.PackingTabPage.TabVisible);
				packingControl = testForm.CustomsBrokerageUserControl.Packing as PackingUserControl;
				AssertEquals("Export Label Not Visible", false, packingControl.ExportLabel.Visible);
				AssertEquals("Export Label Dock Style", DockStyle.Fill, packingControl.ExportLabel.Dock);
				AssertEquals("House Bills are not visible", false, packingControl.HouseBillsGroupBox.Visible);
				AssertEquals("House Label Dock Style", DockStyle.Fill, packingControl.HouseBillPanel.Dock);
				AssertEquals("Packing details are not visible", false, packingControl.PackingDetailsGroupBox.Visible);
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.DeclarationTabPage;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Drawback;
				AssertEquals(false, testForm.CustomsBrokerageUserControl.PackingTabPage.TabVisible);
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.ContainerTabPage;
				packingControl = testForm.CustomsBrokerageUserControl.Packing as PackingUserControl;
				AssertEquals("Export Label Not Visible", false, packingControl.ExportLabel.Visible);
				AssertEquals("Export Label Dock Style", DockStyle.Fill, packingControl.ExportLabel.Dock);
				AssertEquals("House Bills are not visible", false, packingControl.HouseBillsGroupBox.Visible);
				AssertEquals("House Label Dock Style", DockStyle.Fill, packingControl.HouseBillPanel.Dock);
				AssertEquals("Packing details are not visible", false, packingControl.PackingDetailsGroupBox.Visible);
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.DeclarationTabPage;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Refund;
				AssertEquals(false, testForm.CustomsBrokerageUserControl.PackingTabPage.TabVisible);
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.ContainerTabPage;
				packingControl = testForm.CustomsBrokerageUserControl.Packing as PackingUserControl;
				AssertEquals("Export Label Not Visible", false, packingControl.ExportLabel.Visible);
				AssertEquals("Export Label Dock Style", DockStyle.Fill, packingControl.ExportLabel.Dock);
				AssertEquals("House Bills are not visible", false, packingControl.HouseBillsGroupBox.Visible);
				AssertEquals("House Label Dock Style", DockStyle.Fill, packingControl.HouseBillPanel.Dock);
				AssertEquals("Packing details are not visible", false, packingControl.PackingDetailsGroupBox.Visible);
			}
		}

		public void TestHouseBillLabels()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			using (ZAUCustomsDeclarationForm testForm = new ZAUCustomsDeclarationForm(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.PackingTabPage;
				PackingUserControl packingControl = testForm.CustomsBrokerageUserControl.Packing as PackingUserControl;
				AssertEquals("House bill labels", "Master and House Bills", packingControl.HouseBillsGroupBox.Text);
				AssertEquals("House Bill GridColumns", 6, packingControl.HouseBillsGrid.Columns.Count);
				ZGridColumn column = packingControl.HouseBillsGrid.Columns[CusDecHouseBillSchema.Constants.CU_BillNum];
				AssertEquals("Caption on House bill column", "Bill Num", column.ColumnStyle.HeaderText);
				column = packingControl.PackingDetailsGrid.Columns[Customs.Business.BasePackage.Schema.CW_HouseBill];
				AssertEquals("Caption on House bill column on Packing grid", "Linked Bill(Lowest Bill)", column.ColumnStyle.HeaderText);
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.PackingTabPage;
				packingControl = testForm.CustomsBrokerageUserControl.Packing as PackingUserControl;
				AssertEquals("House bill labels", "Master and House Bills", packingControl.HouseBillsGroupBox.Text);
				AssertEquals("House Bill Grids Columns", 7, packingControl.HouseBillsGrid.Columns.Count);
				column = packingControl.HouseBillsGrid.Columns[CusDecHouseBillSchema.Constants.CU_BillNum];
				AssertEquals("Caption on House bill column", "Bill Num", column.ColumnStyle.HeaderText);
				column = packingControl.PackingDetailsGrid.Columns[Customs.Business.BasePackage.Schema.CW_HouseBill];
				AssertEquals("Caption on House bill column on Packing grid", "Linked Bill(Lowest Bill)", column.ColumnStyle.HeaderText);
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.DeclarationTabPage;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Mail;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.PackingTabPage;
				packingControl = testForm.CustomsBrokerageUserControl.Packing as PackingUserControl;
				AssertEquals("House bill labels", "Parcel Post Numbers", packingControl.HouseBillsGroupBox.Text);
				AssertEquals("House Bill Grid columns", 4, packingControl.HouseBillsGrid.Columns.Count);
				column = packingControl.HouseBillsGrid.Columns[CusDecHouseBillSchema.Constants.CU_BillNum];
				AssertEquals("Caption on House bill column", "Parcel Post Number", column.ColumnStyle.HeaderText);
				column = packingControl.PackingDetailsGrid.Columns[Customs.Business.BasePackage.Schema.CW_HouseBill];
				AssertEquals("Caption on House bill column on Packing grid", "Parcel Post Number", column.ColumnStyle.HeaderText);
			}
		}
	}
}
