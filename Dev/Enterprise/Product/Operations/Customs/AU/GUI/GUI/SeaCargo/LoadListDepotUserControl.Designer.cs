using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public partial class LoadListDepotUserControl
	{
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.loadListTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.depotTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.splitter1 = new CargoWise.Windows.UI.KSplitter();
			this.houseBillsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zGrid1 = new Enterprise.ZArchitecture.ZGrid();
			this.containersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.containersGrid = new Enterprise.ZArchitecture.ZGrid();
			this.panel1 = new CargoWise.Windows.UI.KPanel();
			this.voyageBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.voyageLabel = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.lloydsNumberBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.vesselBountTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.masterBillLabel = new Enterprise.ZArchitecture.ZLabel();
			this.oceanBillBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.messagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.messagesUserControl = new Enterprise.Customs.AU.SeaCargo.GUI.SCDMessageUserControl();
			this.loadListTabControl.SuspendLayout();
			this.depotTabPage.SuspendLayout();
			this.houseBillsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).BeginInit();
			this.containersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.containersGrid)).BeginInit();
			this.panel1.SuspendLayout();
			this.messagesTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// LoadListTabControl
			// 
			this.loadListTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.loadListTabControl.Controls.Add(this.depotTabPage);
			this.loadListTabControl.Controls.Add(this.messagesTabPage);
			this.loadListTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.loadListTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.loadListTabControl.Name = "LoadListTabControl";
			this.loadListTabControl.SelectedIndex = 0;
			this.loadListTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(896, 600, true);
			this.loadListTabControl.TabIndex = 0;
			// 
			// DepotTabPage
			// 
			this.depotTabPage.CheckForNotifications = true;
			this.depotTabPage.Controls.Add(this.splitter1);
			this.depotTabPage.Controls.Add(this.houseBillsGroupBox);
			this.depotTabPage.Controls.Add(this.containersGroupBox);
			this.depotTabPage.Controls.Add(this.panel1);
			this.depotTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.depotTabPage.Name = "DepotTabPage";
			this.depotTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(888, 573, true);
			this.depotTabPage.TabIndex = 0;
			this.depotTabPage.Text = "Customs Status";
			// 
			// splitter1
			// 
			this.splitter1.Dock = System.Windows.Forms.DockStyle.Top;
			this.splitter1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 232, true);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(888, 3, true);
			this.splitter1.TabIndex = 4;
			this.splitter1.TabStop = false;
			// 
			// HouseBillsGroupBox
			// 
			this.houseBillsGroupBox.Controls.Add(this.zGrid1);
			this.houseBillsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.houseBillsGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.houseBillsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 232, true);
			this.houseBillsGroupBox.Name = "HouseBillsGroupBox";
			this.houseBillsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(888, 341, true);
			this.houseBillsGroupBox.TabIndex = 3;
			this.houseBillsGroupBox.TabStop = false;
			this.houseBillsGroupBox.Text = "House Bills";
			// 
			// zGrid1
			// 
			this.zGrid1.AllowNavigation = false;
			this.zGrid1.BindTo = "Shipments";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.SeaCargoDepotLoadList)(null)).Shipments)));
			this.zGrid1.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "House Bill";
			zTextBoxColumnStyleInfo1.ColumnName = "HouseBill";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo2.Caption = "Message State Text";
			zTextBoxColumnStyleInfo2.ColumnName = "MessageStateText";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.zGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGrid1.EnableToolTips = false;
			this.zGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid1.LayoutKey = "ContainersGrid";
			this.zGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.zGrid1.Name = "zGrid1";
			this.zGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(882, 322, true);
			this.zGrid1.TabIndex = 1;
			// Compile time check for the above grid columns. If any of these lines fail, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.SeaCargoDepotShipment)(((object)(((Enterprise.Customs.AU.Declaration.Business.SeaCargoDepotLoadList)(null)).Shipments)))).HouseBillInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.SeaCargoDepotShipment)(((object)(((Enterprise.Customs.AU.Declaration.Business.SeaCargoDepotLoadList)(null)).Shipments)))).HouseBill)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.SeaCargoDepotShipment)(((object)(((Enterprise.Customs.AU.Declaration.Business.SeaCargoDepotLoadList)(null)).Shipments)))).MessageStateTextInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.SeaCargoDepotShipment)(((object)(((Enterprise.Customs.AU.Declaration.Business.SeaCargoDepotLoadList)(null)).Shipments)))).MessageStateText)));
			// 
			// ContainersGroupBox
			// 
			this.containersGroupBox.Controls.Add(this.containersGrid);
			this.containersGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.containersGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.containersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 32, true);
			this.containersGroupBox.Name = "ContainersGroupBox";
			this.containersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(888, 200, true);
			this.containersGroupBox.TabIndex = 2;
			this.containersGroupBox.TabStop = false;
			this.containersGroupBox.Text = "Containers";
			// 
			// ContainersGrid
			// 
			this.containersGrid.AllowNavigation = false;
			this.containersGrid.BindTo = "Containers";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.SeaCargoDepotLoadList)(null)).Containers)));
			this.containersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo3.Caption = "Container";
			zTextBoxColumnStyleInfo3.ColumnName = "ContainerNumber";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo4.Caption = "Status";
			zTextBoxColumnStyleInfo4.ColumnName = "MessageStateText";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
			zGuidFindBoxColumnStyleInfo1.BindToList = "ClientList";
			zGuidFindBoxColumnStyleInfo1.Caption = "Client";
			zGuidFindBoxColumnStyleInfo1.ColumnName = "ResponsibleParty";
			zTextBoxColumnStyleInfo5.Caption = "CMP";
			zTextBoxColumnStyleInfo5.ColumnName = "ResponsiblePartyCMP";
			this.containersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.containersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.containersGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.containersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.containersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.containersGrid.EnableToolTips = false;
			this.containersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.containersGrid.LayoutKey = "ContainersGrid";
			this.containersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.containersGrid.Name = "ContainersGrid";
			this.containersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(882, 181, true);
			this.containersGrid.TabIndex = 1;
			// Compile time check for the above grid columns. If any of these lines fail, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.SeaCargoDepotContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.SeaCargoDepotLoadList)(null)).Containers)))).ContainerNumberInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.SeaCargoDepotContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.SeaCargoDepotLoadList)(null)).Containers)))).ContainerNumber)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.SeaCargoDepotContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.SeaCargoDepotLoadList)(null)).Containers)))).MessageStateTextInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.SeaCargoDepotContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.SeaCargoDepotLoadList)(null)).Containers)))).MessageStateText)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZGuid)(((Enterprise.Customs.AU.Declaration.Business.SeaCargoDepotContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.SeaCargoDepotLoadList)(null)).Containers)))).ResponsibleParty)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.SeaCargoDepotContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.SeaCargoDepotLoadList)(null)).Containers)))).ResponsiblePartyInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.SeaCargoDepotContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.SeaCargoDepotLoadList)(null)).Containers)))).ClientList)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.SeaCargoDepotContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.SeaCargoDepotLoadList)(null)).Containers)))).ResponsiblePartyCMPInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.SeaCargoDepotContainer)(((object)(((Enterprise.Customs.AU.Declaration.Business.SeaCargoDepotLoadList)(null)).Containers)))).ResponsiblePartyCMP)));
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.voyageBoundTextBox);
			this.panel1.Controls.Add(this.voyageLabel);
			this.panel1.Controls.Add(this.zLabel2);
			this.panel1.Controls.Add(this.lloydsNumberBoundTextBox);
			this.panel1.Controls.Add(this.zLabel1);
			this.panel1.Controls.Add(this.vesselBountTextBox);
			this.panel1.Controls.Add(this.masterBillLabel);
			this.panel1.Controls.Add(this.oceanBillBoundTextBox);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.panel1.Name = "panel1";
			this.panel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(888, 32, true);
			this.panel1.TabIndex = 3;
			// 
			// VoyageBoundTextBox
			// 
			this.voyageBoundTextBox.BindTo = "Voyage";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.SeaCargoDepotLoadList)(null)).VoyageInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.SeaCargoDepotLoadList)(null)).Voyage)));
			this.voyageBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(680, 6, true);
			this.voyageBoundTextBox.Name = "VoyageBoundTextBox";
			this.voyageBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 20, true);
			this.voyageBoundTextBox.TabIndex = 8;
			this.voyageBoundTextBox.Text = "ZTEXTBOX1";
			// 
			// VoyageLabel
			// 
			this.voyageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(632, 8, true);
			this.voyageLabel.Name = "VoyageLabel";
			this.voyageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 16, true);
			this.voyageLabel.TabIndex = 7;
			this.voyageLabel.Text = "Voyage:";
			// 
			// zLabel2
			// 
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 8, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 16, true);
			this.zLabel2.TabIndex = 5;
			this.zLabel2.Text = "Lloyds:";
			// 
			// LloydsNumberBoundTextBox
			// 
			this.lloydsNumberBoundTextBox.BindTo = "Lloyds";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.SeaCargoDepotLoadList)(null)).LloydsInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.SeaCargoDepotLoadList)(null)).Lloyds)));
			this.lloydsNumberBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(466, 6, true);
			this.lloydsNumberBoundTextBox.Name = "LloydsNumberBoundTextBox";
			this.lloydsNumberBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 20, true);
			this.lloydsNumberBoundTextBox.TabIndex = 4;
			this.lloydsNumberBoundTextBox.Text = "ZTEXTBOX1";
			// 
			// zLabel1
			// 
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(224, 8, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 16, true);
			this.zLabel1.TabIndex = 3;
			this.zLabel1.Text = "Vessel:";
			// 
			// VesselBountTextBox
			// 
			this.vesselBountTextBox.BindTo = "Vessel";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.SeaCargoDepotLoadList)(null)).VesselInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.SeaCargoDepotLoadList)(null)).Vessel)));
			this.vesselBountTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(272, 6, true);
			this.vesselBountTextBox.Name = "VesselBountTextBox";
			this.vesselBountTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.vesselBountTextBox.TabIndex = 2;
			this.vesselBountTextBox.Text = "ZTEXTBOX1";
			// 
			// MasterBillLabel
			// 
			this.masterBillLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.masterBillLabel.Name = "MasterBillLabel";
			this.masterBillLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 16, true);
			this.masterBillLabel.TabIndex = 1;
			this.masterBillLabel.Text = "Ocean Bill:";
			// 
			// OceanBillBoundTextBox
			// 
			this.oceanBillBoundTextBox.BindTo = "OceanBill";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.SeaCargoDepotLoadList)(null)).OceanBillInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.SeaCargoDepotLoadList)(null)).OceanBill)));
			this.oceanBillBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 6, true);
			this.oceanBillBoundTextBox.Name = "OceanBillBoundTextBox";
			this.oceanBillBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.oceanBillBoundTextBox.TabIndex = 0;
			this.oceanBillBoundTextBox.Text = "ZTEXTBOX1";
			// 
			// MessagesTabPage
			// 
			this.messagesTabPage.CheckForNotifications = true;
			this.messagesTabPage.Controls.Add(this.messagesUserControl);
			this.messagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.messagesTabPage.Name = "MessagesTabPage";
			this.messagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(888, 573, true);
			this.messagesTabPage.TabIndex = 1;
			this.messagesTabPage.Text = "Messages";
			// 
			// MessagesUserControl
			// 
			this.messagesUserControl.DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
			this.messagesUserControl.DataSourceTypeName = "Enterprise.Customs.AU.Declaration.Business.SeaCargoDepotContainer";
			this.messagesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.messagesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.messagesUserControl.Name = "MessagesUserControl";
			this.messagesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(888, 573, true);
			this.messagesUserControl.TabIndex = 0;
			// 
			// LoadListDepotUserControl
			// 
			this.CaptionRenderingEnabled = false;
			this.Controls.Add(this.loadListTabControl);
			this.DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
			this.DataSourceTypeName = "Enterprise.Customs.AU.Declaration.Business.SeaCargoDepotLoadList";
			this.Name = "LoadListDepotUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(896, 600, true);
			this.loadListTabControl.ResumeLayout(false);
			this.depotTabPage.ResumeLayout(false);
			this.houseBillsGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).EndInit();
			this.containersGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.containersGrid)).EndInit();
			this.panel1.ResumeLayout(false);
			this.messagesTabPage.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		private ZTemplateTabControl loadListTabControl;
		private ZTabPage depotTabPage;
		private ZTabPage messagesTabPage;
		private SCDMessageUserControl messagesUserControl;
		private ZArchitecture.ZGrid zGrid1;
		private ZGroupBox containersGroupBox;
		private ZArchitecture.ZGrid containersGrid;
		private ZGroupBox houseBillsGroupBox;
		private CargoWise.Windows.UI.KSplitter splitter1;
		private CargoWise.Windows.UI.KPanel panel1;
		private ZArchitecture.ZLabel masterBillLabel;
		private ZArchitecture.ZLabel zLabel1;
		private ZArchitecture.ZTextBox vesselBountTextBox;
		private ZArchitecture.ZLabel zLabel2;
		private ZArchitecture.ZLabel voyageLabel;
		private ZArchitecture.ZTextBox oceanBillBoundTextBox;
		private ZArchitecture.ZTextBox lloydsNumberBoundTextBox;
		private ZArchitecture.ZTextBox voyageBoundTextBox;
	}
}
