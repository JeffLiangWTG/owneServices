using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.AU.AirCargo.GUI
{
	public partial class CMRACAStandAloneUserControl
	{
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.houseDetails = new Enterprise.Customs.AU.AirCargo.GUI.CMRHouseDetailsUserControl();
			this.firstArrivalPortFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.responsiblePartyLabel = new Enterprise.ZArchitecture.ZLabel();
			this.responsiblePartyTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.underbondStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.underbondStatusLabel = new Enterprise.ZArchitecture.ZLabel();
			this.responsiblePartyIDOrganisationFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
			this.deferredScheduleDateTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.firstArrivalPortLabel = new Enterprise.ZArchitecture.ZLabel();
			this.masterCarstMessagesTab = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.historyGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MessagesGrid = new Enterprise.Messaging.GUI.MessageZGrid();
			this.splitter1 = new CargoWise.Windows.UI.KSplitter();
			this.messageTextGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MessageTextTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.deconsolidatorAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.deconsolidatorAddressLabel = new Enterprise.ZArchitecture.ZLabel();
			this.AltPartShipModelCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.depatureDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.depatureLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MasterBillControl.SuspendLayout();
			this.PortOfLoadingCodeFindBox.SuspendLayout();
			this.AirCargoStandAloneHouseMessageUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.HouseBillsModuleButtonGrid)).BeginInit();
			this.HouseBillsModuleButtonGrid.SuspendLayout();
			this.FirstArrivalDateEdit.SuspendLayout();
			this.BranchGuidFindBox.SuspendLayout();
			this.UpperPanel.SuspendLayout();
			this.HouseBillsPanel.SuspendLayout();
			this.HouseBillsGroupBox.SuspendLayout();
			this.ArivalDateDateEdit.SuspendLayout();
			this.PortOfDischargeCodeFindBox.SuspendLayout();
			this.MasterGroupBox.SuspendLayout();
			this.MasterTabControl.SuspendLayout();
			this.houseBillsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.houseBillsSplitContainer)).BeginInit();
			this.houseBillsSplitContainer.Panel1.SuspendLayout();
			this.houseBillsSplitContainer.Panel2.SuspendLayout();
			this.houseBillsSplitContainer.SuspendLayout();
			this.HAWBTabControl.SuspendLayout();
			this.HouseDetailsTabPage.SuspendLayout();
			this.airCagoHouseBillPartiesUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.houseDetails.SuspendLayout();
			this.firstArrivalPortFindBox.SuspendLayout();
			this.responsiblePartyIDOrganisationFindBox.SuspendLayout();
			this.masterCarstMessagesTab.SuspendLayout();
			this.historyGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessagesGrid)).BeginInit();
			this.MessagesGrid.SuspendLayout();
			this.messageTextGroupBox.SuspendLayout();
			this.deconsolidatorAddressControl.SuspendLayout();
			this.depatureDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MasterBillControl
			// 
			this.MasterBillControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(56, 16, true);
			// 
			// MasterBillLabel
			// 
			this.MasterBillLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 18, true);
			// 
			// PortOfLoadingCodeFindBox
			// 
			this.PortOfLoadingCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(385, 15, true);
			this.PortOfLoadingCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(157, 20, true);
			this.PortOfLoadingCodeFindBox.TabIndex = 7;
			// 
			// AirCargoStandAloneHouseMessageUserControl
			// 
			this.AirCargoStandAloneHouseMessageUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(930, 250, true);
			// 
			// FolioTextBox
			// 
			this.FolioTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(893, 15, true);
			this.FolioTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 20, true);
			this.FolioTextBox.TabIndex = 15;
			// 
			// FolioLabel
			// 
			this.FolioLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(825, 16, true);
			this.FolioLabel.TabIndex = 14;
			// 
			// HouseBillsModuleButtonGrid
			// 
			this.HouseBillsModuleButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1037, 131, true);
			// 
			// PortOfLoadingLabel
			// 
			this.PortOfLoadingLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(318, 18, true);
			this.PortOfLoadingLabel.TabIndex = 4;
			// 
			// FirstArrivalDateEdit
			// 
			this.FirstArrivalDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(385, 91, true);
			this.FirstArrivalDateEdit.TabIndex = 10;
			this.FirstArrivalDateEdit.Visible = false;
			// 
			// FirstArrivalDateLabel
			// 
			this.FirstArrivalDateLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(323, 94, true);
			this.FirstArrivalDateLabel.Visible = false;
			// 
			// CoLoadMasterTextBox
			// 
			this.CoLoadMasterTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(225, 15, true);
			this.CoLoadMasterTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 20, true);
			this.CoLoadMasterTextBox.TabIndex = 4;
			// 
			// CoLoadMasterLabel
			// 
			this.CoLoadMasterLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 18, true);
			this.CoLoadMasterLabel.TabIndex = 2;
			// 
			// BranchGuidFindBox
			// 
			this.BranchGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(56, 66, true);
			this.BranchGuidFindBox.ShowDescriptionBox = false;
			this.BranchGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 20, true);
			this.BranchGuidFindBox.TabIndex = 2;
			// 
			// branchLabel
			// 
			this.branchLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 69, true);
			this.branchLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(44, 13, true);
			this.branchLabel.Text = "Branch:";
			// 
			// UpperPanel
			// 
			this.UpperPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1051, 124, true);
			// 
			// HouseBillsPanel
			// 
			this.HouseBillsPanel.Controls.Add(this.deferredScheduleDateTextBox);
			this.HouseBillsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 124, true);
			this.HouseBillsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1051, 530, true);
			this.HouseBillsPanel.Controls.SetChildIndex(this.MasterTabControl, 0);
			this.HouseBillsPanel.Controls.SetChildIndex(this.deferredScheduleDateTextBox, 0);
			// 
			// HouseBillsGroupBox
			// 
			this.HouseBillsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1043, 503, true);
			// 
			// FlightNumberTextBox
			// 
			this.FlightNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(56, 40, true);
			// 
			// ArrivalDateLabel
			// 
			this.ArrivalDateLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 44, true);
			this.ArrivalDateLabel.TabIndex = 10;
			// 
			// CM_RL_NKPortOfDischargeLabel
			// 
			this.CM_RL_NKPortOfDischargeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(318, 44, true);
			this.CM_RL_NKPortOfDischargeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(58, 13, true);
			this.CM_RL_NKPortOfDischargeLabel.TabIndex = 12;
			this.CM_RL_NKPortOfDischargeLabel.Text = "Discharge:";
			// 
			// FlightNumberLabel
			// 
			this.FlightNumberLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 44, true);
			this.FlightNumberLabel.TabIndex = 8;
			// 
			// ArivalDateDateEdit
			// 
			this.ArivalDateDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ArivalDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(202, 41, true);
			this.ArivalDateDateEdit.TabIndex = 5;
			// 
			// PortOfDischargeCodeFindBox
			// 
			this.PortOfDischargeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(385, 41, true);
			this.PortOfDischargeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(157, 20, true);
			this.PortOfDischargeCodeFindBox.TabIndex = 8;
			// 
			// MasterGroupBox
			// 
			this.MasterGroupBox.Controls.Add(this.depatureLabel);
			this.MasterGroupBox.Controls.Add(this.depatureDateEdit);
			this.MasterGroupBox.Controls.Add(this.AltPartShipModelCheckBox);
			this.MasterGroupBox.Controls.Add(this.deconsolidatorAddressLabel);
			this.MasterGroupBox.Controls.Add(this.firstArrivalPortLabel);
			this.MasterGroupBox.Controls.Add(this.firstArrivalPortFindBox);
			this.MasterGroupBox.Controls.Add(this.deconsolidatorAddressControl);
			this.MasterGroupBox.Controls.Add(this.responsiblePartyIDOrganisationFindBox);
			this.MasterGroupBox.Controls.Add(this.underbondStatusLabel);
			this.MasterGroupBox.Controls.Add(this.underbondStatusTextBox);
			this.MasterGroupBox.Controls.Add(this.responsiblePartyLabel);
			this.MasterGroupBox.Controls.Add(this.responsiblePartyTextBox);
			this.MasterGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MasterGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1051, 124, true);
			this.MasterGroupBox.Controls.SetChildIndex(this.branchLabel, 0);
			this.MasterGroupBox.Controls.SetChildIndex(this.BranchGuidFindBox, 0);
			this.MasterGroupBox.Controls.SetChildIndex(this.responsiblePartyTextBox, 0);
			this.MasterGroupBox.Controls.SetChildIndex(this.CM_RL_NKPortOfDischargeLabel, 0);
			this.MasterGroupBox.Controls.SetChildIndex(this.MasterBillLabel, 0);
			this.MasterGroupBox.Controls.SetChildIndex(this.PortOfDischargeCodeFindBox, 0);
			this.MasterGroupBox.Controls.SetChildIndex(this.ArivalDateDateEdit, 0);
			this.MasterGroupBox.Controls.SetChildIndex(this.FlightNumberLabel, 0);
			this.MasterGroupBox.Controls.SetChildIndex(this.ArrivalDateLabel, 0);
			this.MasterGroupBox.Controls.SetChildIndex(this.FlightNumberTextBox, 0);
			this.MasterGroupBox.Controls.SetChildIndex(this.MasterBillControl, 0);
			this.MasterGroupBox.Controls.SetChildIndex(this.PortOfLoadingCodeFindBox, 0);
			this.MasterGroupBox.Controls.SetChildIndex(this.PortOfLoadingLabel, 0);
			this.MasterGroupBox.Controls.SetChildIndex(this.FolioLabel, 0);
			this.MasterGroupBox.Controls.SetChildIndex(this.FolioTextBox, 0);
			this.MasterGroupBox.Controls.SetChildIndex(this.FirstArrivalDateLabel, 0);
			this.MasterGroupBox.Controls.SetChildIndex(this.FirstArrivalDateEdit, 0);
			this.MasterGroupBox.Controls.SetChildIndex(this.CoLoadMasterLabel, 0);
			this.MasterGroupBox.Controls.SetChildIndex(this.CoLoadMasterTextBox, 0);
			this.MasterGroupBox.Controls.SetChildIndex(this.responsiblePartyLabel, 0);
			this.MasterGroupBox.Controls.SetChildIndex(this.underbondStatusTextBox, 0);
			this.MasterGroupBox.Controls.SetChildIndex(this.underbondStatusLabel, 0);
			this.MasterGroupBox.Controls.SetChildIndex(this.responsiblePartyIDOrganisationFindBox, 0);
			this.MasterGroupBox.Controls.SetChildIndex(this.deconsolidatorAddressControl, 0);
			this.MasterGroupBox.Controls.SetChildIndex(this.firstArrivalPortFindBox, 0);
			this.MasterGroupBox.Controls.SetChildIndex(this.firstArrivalPortLabel, 0);
			this.MasterGroupBox.Controls.SetChildIndex(this.deconsolidatorAddressLabel, 0);
			this.MasterGroupBox.Controls.SetChildIndex(this.AltPartShipModelCheckBox, 0);
			this.MasterGroupBox.Controls.SetChildIndex(this.depatureDateEdit, 0);
			this.MasterGroupBox.Controls.SetChildIndex(this.depatureLabel, 0);
			// 
			// MasterTabControl
			// 
			this.MasterTabControl.Controls.Add(this.masterCarstMessagesTab);
			this.MasterTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1051, 530, true);
			this.MasterTabControl.Controls.SetChildIndex(this.masterCarstMessagesTab, 0);
			this.MasterTabControl.Controls.SetChildIndex(this.houseBillsTabPage, 0);
			// 
			// HouseBillsTabPage
			// 
			this.houseBillsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1043, 503, true);
			// 
			// houseBillsSplitContainer
			// 
			this.houseBillsSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1037, 484, true);
			this.houseBillsSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(160);
			// 
			// HAWBTabControl
			// 
			this.HAWBTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1037, 320, true);
			// 
			// HouseDetailsTabPage
			// 
			this.HouseDetailsTabPage.Controls.Add(this.houseDetails);
			this.HouseDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1029, 293, true);
			this.HouseDetailsTabPage.Controls.SetChildIndex(this.airCagoHouseBillPartiesUserControl, 0);
			this.HouseDetailsTabPage.Controls.SetChildIndex(this.houseDetails, 0);
			// 
			// airCagoHouseBillPartiesUserControl
			// 
			this.airCagoHouseBillPartiesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(642, 3, true);
			this.airCagoHouseBillPartiesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(380, 265, true);
			this.airCagoHouseBillPartiesUserControl.TabIndex = 1;
			// 
			// HouseDetails
			// 
			this.houseDetails.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.houseDetails, "FilteredChildBills");
			this.houseDetails.HAWB = null;
			this.houseDetails.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 3, true);
			this.houseDetails.Name = "HouseDetails";
			this.houseDetails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(632, 265, true);
			this.houseDetails.TabIndex = 0;
			// 
			// FirstArrivalPortFindBox
			// 
			this.firstArrivalPortFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.firstArrivalPortFindBox, "CM_RL_NKFirstArrivalPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).CM_RL_NKFirstArrivalPort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).Lookups.PortOfDischargeList)));
			this.firstArrivalPortFindBox.BindToList = "Lookups+PortOfDischargeList";
			this.firstArrivalPortFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(385, 66, true);
			this.firstArrivalPortFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.firstArrivalPortFindBox.Name = "FirstArrivalPortFindBox";
			this.firstArrivalPortFindBox.PopupCaption = null;
			this.firstArrivalPortFindBox.PreBoundMaxLength = 5;
			this.firstArrivalPortFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(157, 20, true);
			this.firstArrivalPortFindBox.TabIndex = 9;
			// 
			// ResponsiblePartyLabel
			// 
			this.responsiblePartyLabel.AutoSize = true;
			this.responsiblePartyLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(553, 18, true);
			this.responsiblePartyLabel.Name = "ResponsiblePartyLabel";
			this.responsiblePartyLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 13, true);
			this.responsiblePartyLabel.TabIndex = 6;
			this.responsiblePartyLabel.Text = "Resp. Id:";
			// 
			// ResponsiblePartyTextBox
			// 
			this.BindingSource.SetBindingMember(this.responsiblePartyTextBox, "CM_ResponsiblePartyID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).CM_ResponsiblePartyID)));
			this.responsiblePartyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(710, 15, true);
			this.responsiblePartyTextBox.Name = "ResponsiblePartyTextBox";
			this.responsiblePartyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 20, true);
			this.responsiblePartyTextBox.TabIndex = 12;
			// 
			// UnderbondStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.underbondStatusTextBox, "UnderbondStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).UnderbondStatus)));
			this.underbondStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(605, 41, true);
			this.underbondStatusTextBox.Name = "UnderbondStatusTextBox";
			this.underbondStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(202, 20, true);
			this.underbondStatusTextBox.TabIndex = 13;
			// 
			// UnderbondStatusLabel
			// 
			this.underbondStatusLabel.AutoSize = true;
			this.underbondStatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(553, 44, true);
			this.underbondStatusLabel.Name = "UnderbondStatusLabel";
			this.underbondStatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(34, 13, true);
			this.underbondStatusLabel.TabIndex = 21;
			this.underbondStatusLabel.Text = "UBM:";
			// 
			// ResponsiblePartyIDOrganisationFindBox
			// 
			this.responsiblePartyIDOrganisationFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.responsiblePartyIDOrganisationFindBox, "CM_OH_ResponsibleParty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).CM_OH_ResponsibleParty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).Lookups.ResponsibleParties)));
			this.responsiblePartyIDOrganisationFindBox.BindToList = "Lookups+ResponsibleParties";
			this.responsiblePartyIDOrganisationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(605, 15, true);
			this.responsiblePartyIDOrganisationFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.responsiblePartyIDOrganisationFindBox.Name = "ResponsiblePartyIDOrganisationFindBox";
			this.responsiblePartyIDOrganisationFindBox.PopupCaption = null;
			this.responsiblePartyIDOrganisationFindBox.ShowDescriptionBox = false;
			this.responsiblePartyIDOrganisationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.responsiblePartyIDOrganisationFindBox.TabIndex = 11;
			// 
			// DeferredScheduleDateTextBox
			// 
			this.deferredScheduleDateTextBox.BackColor = System.Drawing.Color.LightSalmon;
			this.BindingSource.SetBindingMember(this.deferredScheduleDateTextBox, "DeferredScheduledDateForDisplayInCanberraTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).DeferredScheduledDateForDisplayInCanberraTime)));
			this.deferredScheduleDateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(607, 1, true);
			this.deferredScheduleDateTextBox.Name = "DeferredScheduleDateTextBox";
			this.deferredScheduleDateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 20, true);
			this.deferredScheduleDateTextBox.TabIndex = 0;
			// 
			// FirstArrivalPortLabel
			// 
			this.firstArrivalPortLabel.AutoSize = true;
			this.firstArrivalPortLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(318, 69, true);
			this.firstArrivalPortLabel.Name = "FirstArrivalPortLabel";
			this.firstArrivalPortLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(61, 13, true);
			this.firstArrivalPortLabel.TabIndex = 22;
			this.firstArrivalPortLabel.Text = "First Arrival:";
			// 
			// MasterCarstMessagesTab
			// 
			this.masterCarstMessagesTab.Controls.Add(this.historyGroupBox);
			this.masterCarstMessagesTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.masterCarstMessagesTab.Name = "MasterCarstMessagesTab";
			this.masterCarstMessagesTab.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.masterCarstMessagesTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(944, 588, true);
			this.masterCarstMessagesTab.TabIndex = 1;
			this.masterCarstMessagesTab.Text = "Master CARST Messages";
			this.masterCarstMessagesTab.UseVisualStyleBackColor = true;
			// 
			// HistoryGroupBox
			// 
			this.historyGroupBox.Controls.Add(this.MessagesGrid);
			this.historyGroupBox.Controls.Add(this.splitter1);
			this.historyGroupBox.Controls.Add(this.messageTextGroupBox);
			this.historyGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.historyGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.historyGroupBox.Name = "HistoryGroupBox";
			this.historyGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(938, 582, true);
			this.historyGroupBox.TabIndex = 2;
			this.historyGroupBox.TabStop = false;
			this.historyGroupBox.Text = "Messages";
			// 
			// MessagesGrid
			// 
			this.MessagesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.MessagesGrid, "Messages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).Messages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).Messages)).SyncRoot)).EM_MessageNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).Messages)).SyncRoot)).EM_MessageType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).Messages)).SyncRoot)).EM_MessageSubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).Messages)).SyncRoot)).EM_MessageDateTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).Messages)).SyncRoot)).EM_SystemCreateTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).Messages)).SyncRoot)).EM_InterchangeNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).Messages)).SyncRoot)).EM_DateTimeInterchangeSent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).Messages)).SyncRoot)).EM_User)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).Messages)).SyncRoot)).EM_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).Messages)).SyncRoot)).EM_ReceiveTransmit)));
			this.MessagesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "Message No";
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "EM_MessageNum";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.ToolTip = "Message Number";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.Caption = "Message Type";
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "EM_MessageType";
			zTextBoxColumnStyleInfo2.ToolTip = "Message Type";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.Caption = "Message SubType";
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "EM_MessageSubType";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.ToolTip = "Message Sub Type";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.Caption = "Message Time";
			zDateEditColumnStyleInfo1.ColumnName = "EM_MessageDateTime";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.Caption = "Create Time (UTC)";
			zDateEditColumnStyleInfo2.ColumnName = "EM_SystemCreateTimeUtc";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.Caption = "Interchange No";
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.ColumnName = "EM_InterchangeNumber";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.ToolTip = "Interchange No in which this message was contained.";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo3.Caption = "Interchange Sent";
			zDateEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDateEditColumnStyleInfo3.ColumnName = "EM_DateTimeInterchangeSent";
			zDateEditColumnStyleInfo3.IsReadOnly = true;
			zDateEditColumnStyleInfo3.ToolTip = "It is when messages are actually sent to or received from Customs.";
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo5.Caption = "Sender";
			zTextBoxColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo5.ColumnName = "EM_User";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.ToolTip = "It is the person who created this message.";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo6.Caption = "Status";
			zTextBoxColumnStyleInfo6.ColumnName = "EM_Status";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.Caption = "Direction";
			zTextBoxColumnStyleInfo7.ColumnName = "EM_ReceiveTransmit";
			zTextBoxColumnStyleInfo7.ToolTip = "TRX: sent to Customs RCV: received from Customs";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.MessagesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.MessagesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.MessagesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.MessagesGrid.CopySelectedRowsAllowed = true;
			this.MessagesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessagesGrid.GridId = "b8947049-b97b-4e14-a549-b63fc2c9a03a";
			this.MessagesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MessagesGrid.LayoutKey = "zGrid1";
			this.MessagesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.MessagesGrid.Name = "MessagesGrid";
			this.MessagesGrid.ReadOnly = true;
			this.MessagesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(631, 563, true);
			this.MessagesGrid.TabIndex = 2;
			// 
			// splitter1
			// 
			this.splitter1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.splitter1.Dock = System.Windows.Forms.DockStyle.Right;
			this.splitter1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(634, 16, true);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(5, 563, true);
			this.splitter1.TabIndex = 3;
			this.splitter1.TabStop = false;
			// 
			// MessageTextGroupBox
			// 
			this.messageTextGroupBox.Controls.Add(this.MessageTextTextBox);
			this.messageTextGroupBox.Dock = System.Windows.Forms.DockStyle.Right;
			this.messageTextGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(639, 16, true);
			this.messageTextGroupBox.Name = "MessageTextGroupBox";
			this.messageTextGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 563, true);
			this.messageTextGroupBox.TabIndex = 2;
			this.messageTextGroupBox.TabStop = false;
			this.messageTextGroupBox.Text = "Message Text";
			// 
			// MessageTextTextBox
			// 
			this.MessageTextTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.MessageTextTextBox, "Messages.EM_FormattedMessageText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).Messages)).SyncRoot)).EM_FormattedMessageText)));
			this.MessageTextTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.MessageTextTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.MessageTextTextBox.Multiline = true;
			this.MessageTextTextBox.Name = "MessageTextTextBox";
			this.MessageTextTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.MessageTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 544, true);
			this.MessageTextTextBox.TabIndex = 0;
			// 
			// DeconsolidatorAddressControl
			// 
			this.deconsolidatorAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.deconsolidatorAddressControl, "CM_OA_UnpackDepotAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).CM_OA_UnpackDepotAddress)));
			this.deconsolidatorAddressControl.BindToOrgList = "Lookups+UnpackDepotOrganisations";
			this.deconsolidatorAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(605, 66, true);
			this.deconsolidatorAddressControl.Name = "DeconsolidatorAddressControl";
			this.deconsolidatorAddressControl.PopupCaption = "";
			this.deconsolidatorAddressControl.ShowAddress = false;
			this.deconsolidatorAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.deconsolidatorAddressControl.TabIndex = 14;
			// 
			// DeconsolidatorAddressLabel
			// 
			this.deconsolidatorAddressLabel.AutoSize = true;
			this.deconsolidatorAddressLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(557, 69, true);
			this.deconsolidatorAddressLabel.Name = "DeconsolidatorAddressLabel";
			this.deconsolidatorAddressLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 13, true);
			this.deconsolidatorAddressLabel.TabIndex = 23;
			this.deconsolidatorAddressLabel.Text = "CFS:";
			// 
			// AltPartShipModelCheckBox
			// 
			this.AltPartShipModelCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AltPartShipModelCheckBox, "CM_fUseAltPartShipModel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).CM_fUseAltPartShipModel)));
			this.AltPartShipModelCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AltPartShipModelCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(56, 93, true);
			this.AltPartShipModelCheckBox.Name = "AltPartShipModelCheckBox";
			this.AltPartShipModelCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 17, true);
			this.AltPartShipModelCheckBox.TabIndex = 3;
			this.AltPartShipModelCheckBox.Text = "Generate Consignment References";
			// 
			// DepatureDateEdit
			// 
			this.depatureDateEdit.AllowDrop = true;
			this.depatureDateEdit.AutoCompleteMonthThreshold = 1;
			this.depatureDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.depatureDateEdit, "CM_DepartureDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).CM_DepartureDate)));
			this.depatureDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(225, 66, true);
			this.depatureDateEdit.Name = "DepatureDateEdit";
			this.depatureDateEdit.TabIndex = 6;
			// 
			// depatureLabel
			// 
			this.depatureLabel.AutoSize = true;
			this.depatureLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 69, true);
			this.depatureLabel.Name = "depatureLabel";
			this.depatureLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 13, true);
			this.depatureLabel.TabIndex = 25;
			this.depatureLabel.Text = "Departure:";
			// 
			// CMRACAStandAloneUserControl
			// 
			this.Name = "CMRACAStandAloneUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1051, 727, true);
			this.MasterBillControl.ResumeLayout(true);
			this.MasterBillControl.PerformLayout();
			this.PortOfLoadingCodeFindBox.ResumeLayout(true);
			this.PortOfLoadingCodeFindBox.PerformLayout();
			this.AirCargoStandAloneHouseMessageUserControl.ResumeLayout(true);
			this.AirCargoStandAloneHouseMessageUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.HouseBillsModuleButtonGrid)).EndInit();
			this.HouseBillsModuleButtonGrid.ResumeLayout(false);
			this.HouseBillsModuleButtonGrid.PerformLayout();
			this.FirstArrivalDateEdit.ResumeLayout(true);
			this.FirstArrivalDateEdit.PerformLayout();
			this.BranchGuidFindBox.ResumeLayout(true);
			this.BranchGuidFindBox.PerformLayout();
			this.UpperPanel.ResumeLayout(false);
			this.UpperPanel.PerformLayout();
			this.HouseBillsPanel.ResumeLayout(false);
			this.HouseBillsPanel.PerformLayout();
			this.HouseBillsGroupBox.ResumeLayout(false);
			this.HouseBillsGroupBox.PerformLayout();
			this.ArivalDateDateEdit.ResumeLayout(true);
			this.ArivalDateDateEdit.PerformLayout();
			this.PortOfDischargeCodeFindBox.ResumeLayout(true);
			this.PortOfDischargeCodeFindBox.PerformLayout();
			this.MasterGroupBox.ResumeLayout(false);
			this.MasterGroupBox.PerformLayout();
			this.MasterTabControl.ResumeLayout(false);
			this.MasterTabControl.PerformLayout();
			this.houseBillsTabPage.ResumeLayout(false);
			this.houseBillsTabPage.PerformLayout();
			this.houseBillsSplitContainer.Panel1.ResumeLayout(false);
			this.houseBillsSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.houseBillsSplitContainer)).EndInit();
			this.houseBillsSplitContainer.ResumeLayout(false);
			this.houseBillsSplitContainer.PerformLayout();
			this.HAWBTabControl.ResumeLayout(false);
			this.HAWBTabControl.PerformLayout();
			this.HouseDetailsTabPage.ResumeLayout(false);
			this.HouseDetailsTabPage.PerformLayout();
			this.airCagoHouseBillPartiesUserControl.ResumeLayout(false);
			this.airCagoHouseBillPartiesUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.houseDetails.ResumeLayout(true);
			this.houseDetails.PerformLayout();
			this.firstArrivalPortFindBox.ResumeLayout(true);
			this.firstArrivalPortFindBox.PerformLayout();
			this.responsiblePartyIDOrganisationFindBox.ResumeLayout(true);
			this.responsiblePartyIDOrganisationFindBox.PerformLayout();
			this.masterCarstMessagesTab.ResumeLayout(false);
			this.masterCarstMessagesTab.PerformLayout();
			this.historyGroupBox.ResumeLayout(false);
			this.historyGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessagesGrid)).EndInit();
			this.MessagesGrid.ResumeLayout(false);
			this.MessagesGrid.PerformLayout();
			this.messageTextGroupBox.ResumeLayout(false);
			this.messageTextGroupBox.PerformLayout();
			this.deconsolidatorAddressControl.ResumeLayout(true);
			this.deconsolidatorAddressControl.PerformLayout();
			this.depatureDateEdit.ResumeLayout(true);
			this.depatureDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		protected ZTextBox responsiblePartyTextBox;
		protected ZLabel responsiblePartyLabel;
		private ZCodeFindBox firstArrivalPortFindBox;
		internal CMRHouseDetailsUserControl houseDetails;
		protected ZTextBox underbondStatusTextBox;
		protected ZLabel underbondStatusLabel;
		private MasterFiles.GUI.ZOrganisationFindBox responsiblePartyIDOrganisationFindBox;
		private ZLabel firstArrivalPortLabel;
		private ZTabPage masterCarstMessagesTab;
		private ZGroupBox historyGroupBox;
		public Messaging.GUI.MessageZGrid MessagesGrid;
		private CargoWise.Windows.UI.KSplitter splitter1;
		private ZGroupBox messageTextGroupBox;
		public ZTextBox MessageTextTextBox;
		internal ZTextBox deferredScheduleDateTextBox;
		private ZAddressControl deconsolidatorAddressControl;
		private ZLabel deconsolidatorAddressLabel;
		private ZLabel depatureLabel;
		private ZDateEdit depatureDateEdit;
		protected internal ZCheckBox AltPartShipModelCheckBox;
	}
}
