using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public partial class CreateOrAttachLoadListForm
	{
		protected override void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.wizardButtonPanel = new CargoWise.Windows.UI.KPanel();
			this.cancelFormButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.backButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.nextButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.wziardPagePanel = new CargoWise.Windows.UI.KPanel();
			this.mainTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.loadListTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.vesselJournyNameLabel = new Enterprise.ZArchitecture.ZLabel();
			this.vesselCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.jK_JX_JB_E_ARVDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zLabel9 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel8 = new Enterprise.ZArchitecture.ZLabel();
			this.jK_JX_JA_E_DEPDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.jK_JX_JV_VoyageFlightTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.voyageFlightJournyNoLabel = new Enterprise.ZArchitecture.ZLabel();
			this.jK_JX_JB_RL_NKPortOfDischargeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.jK_JX_JA_RL_NKPortOfLoadingCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.zLabel4 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel3 = new Enterprise.ZArchitecture.ZLabel();
			this.oceanBillLabel = new Enterprise.ZArchitecture.ZLabel();
			this.jK_OceanBillBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.clientLabel = new Enterprise.ZArchitecture.ZLabel();
			this.jK_OH_ForwarderBoundOrganisationFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
			this.containersTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.zGrid1 = new Enterprise.ZArchitecture.ZGrid();
			this.currentContainerPanel = new CargoWise.Windows.UI.KPanel();
			this.transportModeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.jC_TransportModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.jC_RCGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.zLabel6 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.jC_ContainerNumBoundTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.shipmentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.zGrid2 = new Enterprise.ZArchitecture.ZGrid();
			this.shipmentDetailsPanel = new CargoWise.Windows.UI.KPanel();
			this.jS_GoodsDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel5 = new Enterprise.ZArchitecture.ZLabel();
			this.consigneeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.consigneePKBoundOrganisationFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
			this.transportModeBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zLabel16 = new Enterprise.ZArchitecture.ZLabel();
			this.jS_HouseBillTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.houseBillLabel = new Enterprise.ZArchitecture.ZLabel();
			this.marksAndNumbersTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.marksAndNumbersLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			this.wizardButtonPanel.SuspendLayout();
			this.wziardPagePanel.SuspendLayout();
			this.mainTabControl.SuspendLayout();
			this.loadListTabPage.SuspendLayout();
			this.containersTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).BeginInit();
			this.currentContainerPanel.SuspendLayout();
			this.shipmentsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGrid2)).BeginInit();
			this.shipmentDetailsPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 317, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 24, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(277);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.MinWidth = 0;
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(0);
			// 
			// WizardButtonPanel
			// 
			this.wizardButtonPanel.Controls.Add(this.cancelFormButton);
			this.wizardButtonPanel.Controls.Add(this.backButton);
			this.wizardButtonPanel.Controls.Add(this.nextButton);
			this.wizardButtonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.wizardButtonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 277, true);
			this.wizardButtonPanel.Name = "WizardButtonPanel";
			this.wizardButtonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 40, true);
			this.wizardButtonPanel.TabIndex = 2;
			// 
			// CancelFormButton
			// 
			this.cancelFormButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelFormButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelFormButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(280, 8, true);
			this.cancelFormButton.Name = "CancelFormButton";
			this.cancelFormButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.cancelFormButton.TabIndex = 2;
			this.cancelFormButton.Text = "Cancel";
			// 
			// BackButton
			// 
			this.backButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.backButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 8, true);
			this.backButton.Name = "BackButton";
			this.backButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.backButton.TabIndex = 0;
			this.backButton.Text = "< &Back";
			this.backButton.Click += new System.EventHandler(this.BackButton_Click);
			// 
			// NextButton
			// 
			this.nextButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.nextButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(184, 8, true);
			this.nextButton.Name = "NextButton";
			this.nextButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.nextButton.TabIndex = 1;
			this.nextButton.Text = "&Next >";
			this.nextButton.Click += new System.EventHandler(this.NextButton_Click);
			// 
			// WziardPagePanel
			// 
			this.wziardPagePanel.Controls.Add(this.mainTabControl);
			this.wziardPagePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.wziardPagePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.wziardPagePanel.Name = "WziardPagePanel";
			this.wziardPagePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 277, true);
			this.wziardPagePanel.TabIndex = 2;
			// 
			// MainTabControl
			// 
			this.mainTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.mainTabControl.Controls.Add(this.loadListTabPage);
			this.mainTabControl.Controls.Add(this.containersTabPage);
			this.mainTabControl.Controls.Add(this.shipmentsTabPage);
			this.mainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.mainTabControl.Name = "MainTabControl";
			this.mainTabControl.SelectedIndex = 0;
			this.mainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 277, true);
			this.mainTabControl.TabIndex = 0;
			this.mainTabControl.SelectedIndexChanged += new System.EventHandler(this.MainTabControl_SelectedIndexChanged);
			// 
			// LoadListTabPage
			// 
			this.loadListTabPage.CheckForNotifications = true;
			this.loadListTabPage.Controls.Add(this.vesselJournyNameLabel);
			this.loadListTabPage.Controls.Add(this.vesselCodeFindBox);
			this.loadListTabPage.Controls.Add(this.jK_JX_JB_E_ARVDateEdit);
			this.loadListTabPage.Controls.Add(this.zLabel9);
			this.loadListTabPage.Controls.Add(this.zLabel8);
			this.loadListTabPage.Controls.Add(this.jK_JX_JA_E_DEPDateEdit);
			this.loadListTabPage.Controls.Add(this.jK_JX_JV_VoyageFlightTextBox);
			this.loadListTabPage.Controls.Add(this.voyageFlightJournyNoLabel);
			this.loadListTabPage.Controls.Add(this.jK_JX_JB_RL_NKPortOfDischargeCodeFindBox);
			this.loadListTabPage.Controls.Add(this.jK_JX_JA_RL_NKPortOfLoadingCodeFindBox);
			this.loadListTabPage.Controls.Add(this.zLabel4);
			this.loadListTabPage.Controls.Add(this.zLabel3);
			this.loadListTabPage.Controls.Add(this.oceanBillLabel);
			this.loadListTabPage.Controls.Add(this.jK_OceanBillBoundTextBox);
			this.loadListTabPage.Controls.Add(this.clientLabel);
			this.loadListTabPage.Controls.Add(this.jK_OH_ForwarderBoundOrganisationFindBox);
			this.loadListTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 24, true);
			this.loadListTabPage.Name = "LoadListTabPage";
			this.loadListTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 249, true);
			this.loadListTabPage.TabIndex = 0;
			this.loadListTabPage.Text = "Load List";
			// 
			// VesselJournyNameLabel
			// 
			this.vesselJournyNameLabel.AutoSize = true;
			this.vesselJournyNameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 136, true);
			this.vesselJournyNameLabel.Name = "VesselJournyNameLabel";
			this.vesselJournyNameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 13, true);
			this.vesselJournyNameLabel.TabIndex = 8;
			this.vesselJournyNameLabel.Text = "Vessel:";
			// 
			// VesselCodeFindBox
			// 
			this.vesselCodeFindBox.BindTo = "MainTransport+JW_Vessel";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).MainTransport.JW_VesselInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).MainTransport.JW_Vessel)));
			this.vesselCodeFindBox.BindToList = "RefVessel_List";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).RefVessel_List)));
			this.vesselCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(67, 136, true);
			this.vesselCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefVessel;
			this.vesselCodeFindBox.Name = "VesselCodeFindBox";
			this.vesselCodeFindBox.PreBoundMaxLength = 35;
			this.vesselCodeFindBox.ShowDescriptionBox = false;
			this.vesselCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.vesselCodeFindBox.TabIndex = 10;
			// 
			// JK_JX_JB_E_ARVDateEdit
			// 
			this.jK_JX_JB_E_ARVDateEdit.AutoCompleteMonthThreshold = 1;
			this.jK_JX_JB_E_ARVDateEdit.AutoCompleteYear = true;
			this.jK_JX_JB_E_ARVDateEdit.BindTo = "MainTransport+JW_ETA";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).MainTransport.JW_ETA)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).MainTransport.JW_ETAInfo)));
			this.jK_JX_JB_E_ARVDateEdit.IsFixedReadOnly = false;
			this.jK_JX_JB_E_ARVDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(227, 184, true);
			this.jK_JX_JB_E_ARVDateEdit.Name = "JK_JX_JB_E_ARVDateEdit";
			this.jK_JX_JB_E_ARVDateEdit.TabIndex = 16;
			// 
			// zLabel9
			// 
			this.zLabel9.AutoSize = true;
			this.zLabel9.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(191, 184, true);
			this.zLabel9.Name = "zLabel9";
			this.zLabel9.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 13, true);
			this.zLabel9.TabIndex = 15;
			this.zLabel9.Text = "ETA:";
			// 
			// zLabel8
			// 
			this.zLabel8.AutoSize = true;
			this.zLabel8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(31, 184, true);
			this.zLabel8.Name = "zLabel8";
			this.zLabel8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 13, true);
			this.zLabel8.TabIndex = 13;
			this.zLabel8.Text = "ETD:";
			// 
			// JK_JX_JA_E_DEPDateEdit
			// 
			this.jK_JX_JA_E_DEPDateEdit.AutoCompleteMonthThreshold = 1;
			this.jK_JX_JA_E_DEPDateEdit.AutoCompleteYear = true;
			this.jK_JX_JA_E_DEPDateEdit.BindTo = "MainTransport+JW_ETD";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).MainTransport.JW_ETD)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).MainTransport.JW_ETDInfo)));
			this.jK_JX_JA_E_DEPDateEdit.IsFixedReadOnly = false;
			this.jK_JX_JA_E_DEPDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(67, 184, true);
			this.jK_JX_JA_E_DEPDateEdit.Name = "JK_JX_JA_E_DEPDateEdit";
			this.jK_JX_JA_E_DEPDateEdit.TabIndex = 14;
			// 
			// JK_JX_JV_VoyageFlightTextBox
			// 
			this.jK_JX_JV_VoyageFlightTextBox.BindTo = "MainTransport+JW_VoyageFlight";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).MainTransport.JW_VoyageFlightInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).MainTransport.JW_VoyageFlight)));
			this.jK_JX_JV_VoyageFlightTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(67, 160, true);
			this.jK_JX_JV_VoyageFlightTextBox.Name = "JK_JX_JV_VoyageFlightTextBox";
			this.jK_JX_JV_VoyageFlightTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.jK_JX_JV_VoyageFlightTextBox.TabIndex = 12;
			// 
			// VoyageFlightJournyNoLabel
			// 
			this.voyageFlightJournyNoLabel.AutoSize = true;
			this.voyageFlightJournyNoLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 160, true);
			this.voyageFlightJournyNoLabel.Name = "VoyageFlightJournyNoLabel";
			this.voyageFlightJournyNoLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 13, true);
			this.voyageFlightJournyNoLabel.TabIndex = 11;
			this.voyageFlightJournyNoLabel.Text = "Voyage:";
			// 
			// JK_JX_JB_RL_NKPortOfDischargeCodeFindBox
			// 
			this.jK_JX_JB_RL_NKPortOfDischargeCodeFindBox.BindTo = "JK_RL_NKDischargePort";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).JK_RL_NKDischargePortInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).JK_RL_NKDischargePort)));
			this.jK_JX_JB_RL_NKPortOfDischargeCodeFindBox.BindToList = "RefUNLOCO_List";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).RefUNLOCO_List)));
			this.jK_JX_JB_RL_NKPortOfDischargeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(67, 112, true);
			this.jK_JX_JB_RL_NKPortOfDischargeCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.jK_JX_JB_RL_NKPortOfDischargeCodeFindBox.Name = "JK_JX_JB_RL_NKPortOfDischargeCodeFindBox";
			this.jK_JX_JB_RL_NKPortOfDischargeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.jK_JX_JB_RL_NKPortOfDischargeCodeFindBox.TabIndex = 7;
			// 
			// JK_JX_JA_RL_NKPortOfLoadingCodeFindBox
			// 
			this.jK_JX_JA_RL_NKPortOfLoadingCodeFindBox.BindTo = "JK_RL_NKLoadPort";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).JK_RL_NKLoadPortInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).JK_RL_NKLoadPort)));
			this.jK_JX_JA_RL_NKPortOfLoadingCodeFindBox.BindToList = "RefUNLOCO_List";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).RefUNLOCO_List)));
			this.jK_JX_JA_RL_NKPortOfLoadingCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(67, 88, true);
			this.jK_JX_JA_RL_NKPortOfLoadingCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.jK_JX_JA_RL_NKPortOfLoadingCodeFindBox.Name = "JK_JX_JA_RL_NKPortOfLoadingCodeFindBox";
			this.jK_JX_JA_RL_NKPortOfLoadingCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.jK_JX_JA_RL_NKPortOfLoadingCodeFindBox.TabIndex = 5;
			// 
			// zLabel4
			// 
			this.zLabel4.AutoSize = true;
			this.zLabel4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 88, true);
			this.zLabel4.Name = "zLabel4";
			this.zLabel4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 13, true);
			this.zLabel4.TabIndex = 4;
			this.zLabel4.Text = "Loading:";
			// 
			// zLabel3
			// 
			this.zLabel3.AutoSize = true;
			this.zLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 112, true);
			this.zLabel3.Name = "zLabel3";
			this.zLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(58, 13, true);
			this.zLabel3.TabIndex = 6;
			this.zLabel3.Text = "Discharge:";
			// 
			// OceanBillLabel
			// 
			this.oceanBillLabel.AutoSize = true;
			this.oceanBillLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 32, true);
			this.oceanBillLabel.Name = "OceanBillLabel";
			this.oceanBillLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 13, true);
			this.oceanBillLabel.TabIndex = 2;
			this.oceanBillLabel.Text = "Ocean Bill:";
			// 
			// JK_OceanBillBoundTextBox
			// 
			this.jK_OceanBillBoundTextBox.BindTo = "JK_MasterBillNum";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).JK_MasterBillNumInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).JK_MasterBillNum)));
			this.jK_OceanBillBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(67, 32, true);
			this.jK_OceanBillBoundTextBox.Name = "JK_OceanBillBoundTextBox";
			this.jK_OceanBillBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.jK_OceanBillBoundTextBox.TabIndex = 3;
			// 
			// ClientLabel
			// 
			this.clientLabel.AutoSize = true;
			this.clientLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(23, 8, true);
			this.clientLabel.Name = "ClientLabel";
			this.clientLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(38, 13, true);
			this.clientLabel.TabIndex = 0;
			this.clientLabel.Text = "Client:";
			// 
			// JK_OH_ForwarderBoundOrganisationFindBox
			// 
			this.jK_OH_ForwarderBoundOrganisationFindBox.BindTo = "JK_OH_Forwarder";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZGuid)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).JK_OH_Forwarder)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).JK_OH_ForwarderInfo)));
			this.jK_OH_ForwarderBoundOrganisationFindBox.BindToList = "Forwarder_List";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Forwarder_List)));
			this.jK_OH_ForwarderBoundOrganisationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(67, 8, true);
			this.jK_OH_ForwarderBoundOrganisationFindBox.Name = "JK_OH_ForwarderBoundOrganisationFindBox";
			this.jK_OH_ForwarderBoundOrganisationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.jK_OH_ForwarderBoundOrganisationFindBox.TabIndex = 1;
			// 
			// ContainersTabPage
			// 
			this.containersTabPage.CheckForNotifications = true;
			this.containersTabPage.Controls.Add(this.zGrid1);
			this.containersTabPage.Controls.Add(this.currentContainerPanel);
			this.containersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 24, true);
			this.containersTabPage.Name = "ContainersTabPage";
			this.containersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 249, true);
			this.containersTabPage.TabIndex = 1;
			this.containersTabPage.Text = "Containers";
			// 
			// zGrid1
			// 
			this.zGrid1.AllowNavigation = false;
			this.zGrid1.BindTo = "Containers";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Containers)));
			this.zGrid1.CaptionVisible = false;
			zDropEditColumnStyleInfo1.BindToList = "JC_ContainerMode_List";
			zDropEditColumnStyleInfo1.Caption = "Container Mode";
			zDropEditColumnStyleInfo1.ColumnName = "JC_ContainerMode";
			zTextBoxColumnStyleInfo1.Caption = "Container Num";
			zTextBoxColumnStyleInfo1.ColumnName = "JC_ContainerNum";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zGuidFindBoxColumnStyleInfo1.BindToList = "RefContainer_List";
			zGuidFindBoxColumnStyleInfo1.Caption = "Type";
			zGuidFindBoxColumnStyleInfo1.ColumnName = "JC_RC";
			zGuidFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefContainer;
			this.zGrid1.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.zGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid1.LayoutKey = "zGrid1";
			this.zGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGrid1.Name = "zGrid1";
			this.zGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 149, true);
			this.zGrid1.TabIndex = 0;
			// Compile time check for the above grid columns. If any of these lines fail, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Freight.CFS.Business.CFSContainer)(((object)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Containers)))).JC_ContainerMode_List)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Freight.CFS.Business.CFSContainer)(((object)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Containers)))).JC_ContainerModeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Freight.CFS.Business.CFSContainer)(((object)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Containers)))).JC_ContainerMode)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Freight.CFS.Business.CFSContainer)(((object)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Containers)))).JC_ContainerNumInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Freight.CFS.Business.CFSContainer)(((object)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Containers)))).JC_ContainerNum)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Freight.CFS.Business.CFSContainer)(((object)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Containers)))).RefContainer_List)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZGuid)(((Enterprise.Freight.CFS.Business.CFSContainer)(((object)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Containers)))).JC_RC)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Freight.CFS.Business.CFSContainer)(((object)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Containers)))).JC_RCInfo)));
			// 
			// CurrentContainerPanel
			// 
			this.currentContainerPanel.Controls.Add(this.transportModeLabel);
			this.currentContainerPanel.Controls.Add(this.jC_TransportModeDropEdit);
			this.currentContainerPanel.Controls.Add(this.jC_RCGuidFindBox);
			this.currentContainerPanel.Controls.Add(this.zLabel6);
			this.currentContainerPanel.Controls.Add(this.zLabel1);
			this.currentContainerPanel.Controls.Add(this.jC_ContainerNumBoundTextBox1);
			this.currentContainerPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.currentContainerPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 149, true);
			this.currentContainerPanel.Name = "CurrentContainerPanel";
			this.currentContainerPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 100, true);
			this.currentContainerPanel.TabIndex = 1;
			// 
			// TransportModeLabel
			// 
			this.transportModeLabel.AutoSize = true;
			this.transportModeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(19, 8, true);
			this.transportModeLabel.Name = "TransportModeLabel";
			this.transportModeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(37, 13, true);
			this.transportModeLabel.TabIndex = 0;
			this.transportModeLabel.Text = "Mode:";
			// 
			// JC_TransportModeDropEdit
			// 
			this.jC_TransportModeDropEdit.BindTo = "Containers.JC_TransportMode";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Freight.CFS.Business.CFSContainer)(((object)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Containers)))).JC_TransportModeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Freight.CFS.Business.CFSContainer)(((object)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Containers)))).JC_TransportMode)));
			this.jC_TransportModeDropEdit.BindToList = "Containers.JC_TransportMode_List";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Freight.CFS.Business.CFSContainer)(((object)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Containers)))).JC_TransportMode_List)));
			this.jC_TransportModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(62, 8, true);
			this.jC_TransportModeDropEdit.MaxItemsToShowInDropDown = 20;
			this.jC_TransportModeDropEdit.Name = "JC_TransportModeDropEdit";
			this.jC_TransportModeDropEdit.PreBoundMaxLength = 3;
			this.jC_TransportModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 20, true);
			this.jC_TransportModeDropEdit.TabIndex = 1;
			// 
			// JC_RCGuidFindBox
			// 
			this.jC_RCGuidFindBox.BindTo = "Containers.JC_RC";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZGuid)(((Enterprise.Freight.CFS.Business.CFSContainer)(((object)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Containers)))).JC_RC)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Freight.CFS.Business.CFSContainer)(((object)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Containers)))).JC_RCInfo)));
			this.jC_RCGuidFindBox.BindToList = "Containers.RefContainer_List";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Freight.CFS.Business.CFSContainer)(((object)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Containers)))).RefContainer_List)));
			this.jC_RCGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(62, 56, true);
			this.jC_RCGuidFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefContainer;
			this.jC_RCGuidFindBox.Name = "JC_RCGuidFindBox";
			this.jC_RCGuidFindBox.PreBoundMaxLength = 4;
			this.jC_RCGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 20, true);
			this.jC_RCGuidFindBox.TabIndex = 5;
			// 
			// zLabel6
			// 
			this.zLabel6.AutoSize = true;
			this.zLabel6.BindTo = null;
			this.zLabel6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(21, 56, true);
			this.zLabel6.Name = "zLabel6";
			this.zLabel6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(35, 13, true);
			this.zLabel6.TabIndex = 4;
			this.zLabel6.Text = "Type:";
			// 
			// zLabel1
			// 
			this.zLabel1.AutoSize = true;
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 32, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 13, true);
			this.zLabel1.TabIndex = 2;
			this.zLabel1.Text = "Number:";
			// 
			// JC_ContainerNumBoundTextBox1
			// 
			this.jC_ContainerNumBoundTextBox1.BindTo = "Containers.JC_ContainerNum";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Freight.CFS.Business.CFSContainer)(((object)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Containers)))).JC_ContainerNumInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Freight.CFS.Business.CFSContainer)(((object)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Containers)))).JC_ContainerNum)));
			this.jC_ContainerNumBoundTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(62, 32, true);
			this.jC_ContainerNumBoundTextBox1.Name = "JC_ContainerNumBoundTextBox1";
			this.jC_ContainerNumBoundTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 20, true);
			this.jC_ContainerNumBoundTextBox1.TabIndex = 3;
			// 
			// ShipmentsTabPage
			// 
			this.shipmentsTabPage.CheckForNotifications = true;
			this.shipmentsTabPage.Controls.Add(this.zGrid2);
			this.shipmentsTabPage.Controls.Add(this.shipmentDetailsPanel);
			this.shipmentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 24, true);
			this.shipmentsTabPage.Name = "ShipmentsTabPage";
			this.shipmentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 249, true);
			this.shipmentsTabPage.TabIndex = 2;
			this.shipmentsTabPage.Text = "Shipments";
			// 
			// zGrid2
			// 
			this.zGrid2.AllowNavigation = false;
			this.zGrid2.BindTo = "Shipments";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Shipments)));
			this.zGrid2.CaptionVisible = false;
			zDropEditColumnStyleInfo2.BindToList = "Lookups.JS_PackingMode_List";
			zDropEditColumnStyleInfo2.Caption = "Mode";
			zDropEditColumnStyleInfo2.ColumnName = "JS_PackingMode";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo2.Caption = "House Bill";
			zTextBoxColumnStyleInfo2.ColumnName = "JS_HouseBill";
			zOrganisationFindBoxColumnStyleInfo1.BindToList = "Lookups.ConsigneeForwarder_List";
			zOrganisationFindBoxColumnStyleInfo1.Caption = "Consignee";
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "ConsigneePK";
			zTextBoxColumnStyleInfo3.Caption = "Description";
			zTextBoxColumnStyleInfo3.ColumnName = "JS_GoodsDescription";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			this.zGrid2.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.zGrid2.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.zGrid2.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.zGrid2.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.zGrid2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGrid2.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid2.LayoutKey = "zGrid2";
			this.zGrid2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGrid2.Name = "zGrid2";
			this.zGrid2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 89, true);
			this.zGrid2.TabIndex = 0;
			// Compile time check for the above grid columns. If any of these lines fail, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Freight.CFS.Business.CFSShipment)(((object)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Shipments)))).Lookups.JS_PackingMode_List)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Freight.CFS.Business.CFSShipment)(((object)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Shipments)))).JS_PackingModeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Freight.CFS.Business.CFSShipment)(((object)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Shipments)))).JS_PackingMode)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Freight.CFS.Business.CFSShipment)(((object)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Shipments)))).JS_HouseBillInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Freight.CFS.Business.CFSShipment)(((object)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Shipments)))).JS_HouseBill)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Freight.CFS.Business.CFSShipment)(((object)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Shipments)))).Lookups.ConsigneeForwarder_List)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZGuid)(((Enterprise.Freight.CFS.Business.CFSShipment)(((object)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Shipments)))).ConsigneePK)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Freight.CFS.Business.CFSShipment)(((object)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Shipments)))).ConsigneePKInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Freight.CFS.Business.CFSShipment)(((object)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Shipments)))).JS_GoodsDescriptionInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Freight.CFS.Business.CFSShipment)(((object)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Shipments)))).JS_GoodsDescription)));
			// 
			// ShipmentDetailsPanel
			// 
			this.shipmentDetailsPanel.Controls.Add(this.marksAndNumbersTextBox);
			this.shipmentDetailsPanel.Controls.Add(this.marksAndNumbersLabel);
			this.shipmentDetailsPanel.Controls.Add(this.jS_GoodsDescriptionTextBox);
			this.shipmentDetailsPanel.Controls.Add(this.zLabel5);
			this.shipmentDetailsPanel.Controls.Add(this.consigneeLabel);
			this.shipmentDetailsPanel.Controls.Add(this.consigneePKBoundOrganisationFindBox);
			this.shipmentDetailsPanel.Controls.Add(this.transportModeBoundDropEdit);
			this.shipmentDetailsPanel.Controls.Add(this.zLabel16);
			this.shipmentDetailsPanel.Controls.Add(this.jS_HouseBillTextBox);
			this.shipmentDetailsPanel.Controls.Add(this.houseBillLabel);
			this.shipmentDetailsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.shipmentDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 89, true);
			this.shipmentDetailsPanel.Name = "ShipmentDetailsPanel";
			this.shipmentDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 160, true);
			this.shipmentDetailsPanel.TabIndex = 1;
			// 
			// JS_GoodsDescriptionTextBox
			// 
			this.jS_GoodsDescriptionTextBox.BindTo = "Shipments.JS_GoodsDescription";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Freight.CFS.Business.CFSShipment)(((object)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Shipments)))).JS_GoodsDescriptionInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Freight.CFS.Business.CFSShipment)(((object)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Shipments)))).JS_GoodsDescription)));
			this.jS_GoodsDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 56, true);
			this.jS_GoodsDescriptionTextBox.Multiline = true;
			this.jS_GoodsDescriptionTextBox.Name = "JS_GoodsDescriptionTextBox";
			this.jS_GoodsDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 40, true);
			this.jS_GoodsDescriptionTextBox.TabIndex = 7;
			// 
			// zLabel5
			// 
			this.zLabel5.AutoSize = true;
			this.zLabel5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 56, true);
			this.zLabel5.Name = "zLabel5";
			this.zLabel5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 13, true);
			this.zLabel5.TabIndex = 6;
			this.zLabel5.Text = "Description:";
			// 
			// ConsigneeLabel
			// 
			this.consigneeLabel.AutoSize = true;
			this.consigneeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 32, true);
			this.consigneeLabel.Name = "ConsigneeLabel";
			this.consigneeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(61, 13, true);
			this.consigneeLabel.TabIndex = 4;
			this.consigneeLabel.Text = "Consignee:";
			// 
			// ConsigneePKBoundOrganisationFindBox
			// 
			this.consigneePKBoundOrganisationFindBox.BindTo = "Shipments.ConsigneePK";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZGuid)(((Enterprise.Freight.CFS.Business.CFSShipment)(((object)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Shipments)))).ConsigneePK)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Freight.CFS.Business.CFSShipment)(((object)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Shipments)))).ConsigneePKInfo)));
			this.consigneePKBoundOrganisationFindBox.BindToList = "Shipments.Lookups.ConsigneeForwarder_List";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Freight.CFS.Business.CFSShipment)(((object)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Shipments)))).Lookups.ConsigneeForwarder_List)));
			this.consigneePKBoundOrganisationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 32, true);
			this.consigneePKBoundOrganisationFindBox.Name = "ConsigneePKBoundOrganisationFindBox";
			this.consigneePKBoundOrganisationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.consigneePKBoundOrganisationFindBox.TabIndex = 5;
			// 
			// TransportModeBoundDropEdit
			// 
			this.transportModeBoundDropEdit.BindTo = "Shipments.JS_TransportMode";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Freight.CFS.Business.CFSShipment)(((object)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Shipments)))).JS_TransportModeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Freight.CFS.Business.CFSShipment)(((object)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Shipments)))).JS_TransportMode)));
			this.transportModeBoundDropEdit.BindToList = "Shipments.Lookups.JS_TransportMode_List";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Freight.CFS.Business.CFSShipment)(((object)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Shipments)))).Lookups.JS_TransportMode_List)));
			this.transportModeBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 8, true);
			this.transportModeBoundDropEdit.Name = "TransportModeBoundDropEdit";
			this.transportModeBoundDropEdit.PreBoundMaxLength = 3;
			this.transportModeBoundDropEdit.ShowDescriptionBox = false;
			this.transportModeBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 20, true);
			this.transportModeBoundDropEdit.TabIndex = 1;
			// 
			// zLabel16
			// 
			this.zLabel16.AutoSize = true;
			this.zLabel16.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.zLabel16.Name = "zLabel16";
			this.zLabel16.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(58, 13, true);
			this.zLabel16.TabIndex = 0;
			this.zLabel16.Text = "Transport:";
			// 
			// JS_HouseBillTextBox
			// 
			this.jS_HouseBillTextBox.BindTo = "Shipments.JS_HouseBill";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Freight.CFS.Business.CFSShipment)(((object)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Shipments)))).JS_HouseBillInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Freight.CFS.Business.CFSShipment)(((object)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Shipments)))).JS_HouseBill)));
			this.jS_HouseBillTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(216, 8, true);
			this.jS_HouseBillTextBox.Name = "JS_HouseBillTextBox";
			this.jS_HouseBillTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.jS_HouseBillTextBox.TabIndex = 3;
			// 
			// HouseBillLabel
			// 
			this.houseBillLabel.AutoSize = true;
			this.houseBillLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 8, true);
			this.houseBillLabel.Name = "HouseBillLabel";
			this.houseBillLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 13, true);
			this.houseBillLabel.TabIndex = 2;
			this.houseBillLabel.Text = "House Bill:";
			// 
			// MarksAndNumbersTextBox
			// 
			this.marksAndNumbersTextBox.BindTo = "Shipments.JS_MarksAndNumbers";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Freight.CFS.Business.CFSShipment)(((object)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Shipments)))).JS_MarksAndNumbersInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Freight.CFS.Business.CFSShipment)(((object)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Shipments)))).JS_MarksAndNumbers)));
			this.marksAndNumbersTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 102, true);
			this.marksAndNumbersTextBox.Multiline = true;
			this.marksAndNumbersTextBox.Name = "MarksAndNumbersTextBox";
			this.marksAndNumbersTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 40, true);
			this.marksAndNumbersTextBox.TabIndex = 9;
			// 
			// MarksAndNumbersLabel
			// 
			this.marksAndNumbersLabel.AutoSize = true;
			this.marksAndNumbersLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 102, true);
			this.marksAndNumbersLabel.Name = "MarksAndNumbersLabel";
			this.marksAndNumbersLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 26, true);
			this.marksAndNumbersLabel.TabIndex = 8;
			this.marksAndNumbersLabel.Text = "Marks and\r\nNumbers:";
			// 
			// CreateOrAttachLoadListForm
			// 
			this.AcceptButton = this.nextButton;
			this.CancelButton = this.cancelFormButton;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 341, true);
			this.Controls.Add(this.wziardPagePanel);
			this.Controls.Add(this.wizardButtonPanel);
			this.DataSourceAssemblyName = "Enterprise.Freight.CFS.Business";
			this.DataSourceTypeName = "Enterprise.Freight.CFS.Business.CFSLoadListConsol";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(376, 368, true);
			this.Name = "CreateOrAttachLoadListForm";
			this.Text = "Create Load List";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.wizardButtonPanel, 0);
			this.Controls.SetChildIndex(this.wziardPagePanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			this.wizardButtonPanel.ResumeLayout(false);
			this.wziardPagePanel.ResumeLayout(false);
			this.mainTabControl.ResumeLayout(false);
			this.loadListTabPage.ResumeLayout(false);
			this.loadListTabPage.PerformLayout();
			this.containersTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).EndInit();
			this.currentContainerPanel.ResumeLayout(false);
			this.currentContainerPanel.PerformLayout();
			this.shipmentsTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.zGrid2)).EndInit();
			this.shipmentDetailsPanel.ResumeLayout(false);
			this.shipmentDetailsPanel.PerformLayout();
			this.ResumeLayout(false);
		}

		CargoWise.Windows.UI.KPanel wizardButtonPanel;
		ZButton cancelFormButton;
		internal ZButton backButton;
		internal ZButton nextButton;
		CargoWise.Windows.UI.KPanel wziardPagePanel;
		internal ZTemplateTabControl mainTabControl;
		ZTabPage loadListTabPage;
		MasterFiles.GUI.ZOrganisationFindBox jK_OH_ForwarderBoundOrganisationFindBox;
		ZLabel clientLabel;
		ZTextBox jK_OceanBillBoundTextBox;
		ZLabel oceanBillLabel;
		ZCodeFindBox jK_JX_JB_RL_NKPortOfDischargeCodeFindBox;
		ZCodeFindBox jK_JX_JA_RL_NKPortOfLoadingCodeFindBox;
		ZLabel zLabel4;
		ZLabel zLabel3;
		ZLabel vesselJournyNameLabel;
		ZCodeFindBox vesselCodeFindBox;
		ZDateEdit jK_JX_JB_E_ARVDateEdit;
		ZLabel zLabel9;
		ZLabel zLabel8;
		ZDateEdit jK_JX_JA_E_DEPDateEdit;
		ZTextBox jK_JX_JV_VoyageFlightTextBox;
		ZLabel voyageFlightJournyNoLabel;
		ZTabPage containersTabPage;
		CargoWise.Windows.UI.KPanel currentContainerPanel;
		ZGrid zGrid1;
		ZTextBox jC_ContainerNumBoundTextBox1;
		ZLabel zLabel1;
		ZGuidFindBox jC_RCGuidFindBox;
		ZLabel zLabel6;
		ZLabel transportModeLabel;
		ZDropEdit jC_TransportModeDropEdit;
		ZTabPage shipmentsTabPage;
		CargoWise.Windows.UI.KPanel shipmentDetailsPanel;
		ZGrid zGrid2;
		ZDropEdit transportModeBoundDropEdit;
		ZLabel zLabel16;
		ZTextBox jS_HouseBillTextBox;
		ZLabel houseBillLabel;
		ZLabel consigneeLabel;
		ZTextBox jS_GoodsDescriptionTextBox;
		ZLabel zLabel5;
		MasterFiles.GUI.ZOrganisationFindBox consigneePKBoundOrganisationFindBox;
		ZTextBox marksAndNumbersTextBox;
		ZLabel marksAndNumbersLabel;
		System.ComponentModel.IContainer components = null;
	}
}
