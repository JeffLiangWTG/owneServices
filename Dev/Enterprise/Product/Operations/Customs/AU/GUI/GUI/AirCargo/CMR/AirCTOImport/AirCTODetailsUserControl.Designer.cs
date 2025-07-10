using Enterprise.Messaging.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.AU.AirCargo.GUI
{
	public partial class AirCTOUserDetailsControl
	{
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.CM_RL_NKPortOfFirstArrivalCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ctoHouseDetailsUserControl = new Enterprise.Customs.AU.AirCargo.GUI.CTOHouseDetailsUserControl();
			this.messagesTabPage = new Enterprise.ZArchitecture.GUI.ZDetailsTabPage();
			this.messagesUserControl = new Enterprise.Messaging.GUI.EDIMessageUserControl();
			this.cTOAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.cTOIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.cTOAddressLabel = new Enterprise.ZArchitecture.ZLabel();
			this.cTOEstablishmentIDLabel = new Enterprise.ZArchitecture.ZLabel();
			this.Routing3CodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.Routing2CodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.RoutingCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.routingLabel = new Enterprise.ZArchitecture.ZLabel();
			this.responsiblePartyIDOrganisationFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
			this.responsiblePartyLabel = new Enterprise.ZArchitecture.ZLabel();
			this.responsiblePartyTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.masterBillsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.departureDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.departureDateLabel = new Enterprise.ZArchitecture.ZLabel();
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
			this.CM_RL_NKPortOfFirstArrivalCodeFindBox.SuspendLayout();
			this.ctoHouseDetailsUserControl.SuspendLayout();
			this.messagesTabPage.SuspendLayout();
			this.messagesUserControl.SuspendLayout();
			this.cTOAddressControl.SuspendLayout();
			this.Routing3CodeFindBox.SuspendLayout();
			this.Routing2CodeFindBox.SuspendLayout();
			this.RoutingCodeFindBox.SuspendLayout();
			this.responsiblePartyIDOrganisationFindBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.masterBillsGrid)).BeginInit();
			this.masterBillsGrid.SuspendLayout();
			this.departureDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// UpperPanel
			// 
			this.UpperPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(960, 88, true);
			// 
			// HouseBillsPanel
			// 
			this.HouseBillsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 88, true);
			this.HouseBillsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(960, 544, true);
			// 
			// HouseBillsGroupBox
			// 
			this.HouseBillsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(952, 517, true);
			this.HouseBillsGroupBox.Text = "Master Bills";
			// 
			// FlightNumberTextBox
			// 
			this.FlightNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 16, true);
			this.FlightNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 20, true);
			this.FlightNumberTextBox.TabIndex = 0;
			// 
			// ArrivalDateLabel
			// 
			this.ArrivalDateLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(223, 20, true);
			this.ArrivalDateLabel.Text = "ETA:";
			// 
			// CM_RL_NKPortOfDischargeLabel
			// 
			this.CM_RL_NKPortOfDischargeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 44, true);
			this.CM_RL_NKPortOfDischargeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 13, true);
			this.CM_RL_NKPortOfDischargeLabel.Text = "Port of Discharge:";
			// 
			// FlightNumberLabel
			// 
			this.FlightNumberLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 20, true);
			// 
			// ArivalDateDateEdit
			// 
			this.ArivalDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(319, 16, true);
			this.ArivalDateDateEdit.TabIndex = 1;
			// 
			// PortOfDischargeCodeFindBox
			// 
			this.PortOfDischargeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 40, true);
			this.PortOfDischargeCodeFindBox.ShowDescriptionBox = false;
			this.PortOfDischargeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			// 
			// MasterGroupBox
			// 
			this.MasterGroupBox.Controls.Add(this.departureDateLabel);
			this.MasterGroupBox.Controls.Add(this.departureDateEdit);
			this.MasterGroupBox.Controls.Add(this.responsiblePartyIDOrganisationFindBox);
			this.MasterGroupBox.Controls.Add(this.responsiblePartyLabel);
			this.MasterGroupBox.Controls.Add(this.responsiblePartyTextBox);
			this.MasterGroupBox.Controls.Add(this.routingLabel);
			this.MasterGroupBox.Controls.Add(this.cTOEstablishmentIDLabel);
			this.MasterGroupBox.Controls.Add(this.cTOAddressLabel);
			this.MasterGroupBox.Controls.Add(this.cTOIDTextBox);
			this.MasterGroupBox.Controls.Add(this.cTOAddressControl);
			this.MasterGroupBox.Controls.Add(this.CM_RL_NKPortOfFirstArrivalCodeFindBox);
			this.MasterGroupBox.Controls.Add(this.zLabel1);
			this.MasterGroupBox.Controls.Add(this.Routing3CodeFindBox);
			this.MasterGroupBox.Controls.Add(this.Routing2CodeFindBox);
			this.MasterGroupBox.Controls.Add(this.RoutingCodeFindBox);
			this.MasterGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.MasterGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(960, 88, true);
			this.MasterGroupBox.Controls.SetChildIndex(this.RoutingCodeFindBox, 0);
			this.MasterGroupBox.Controls.SetChildIndex(this.Routing2CodeFindBox, 0);
			this.MasterGroupBox.Controls.SetChildIndex(this.Routing3CodeFindBox, 0);
			this.MasterGroupBox.Controls.SetChildIndex(this.CM_RL_NKPortOfDischargeLabel, 0);
			this.MasterGroupBox.Controls.SetChildIndex(this.zLabel1, 0);
			this.MasterGroupBox.Controls.SetChildIndex(this.PortOfDischargeCodeFindBox, 0);
			this.MasterGroupBox.Controls.SetChildIndex(this.ArivalDateDateEdit, 0);
			this.MasterGroupBox.Controls.SetChildIndex(this.FlightNumberLabel, 0);
			this.MasterGroupBox.Controls.SetChildIndex(this.ArrivalDateLabel, 0);
			this.MasterGroupBox.Controls.SetChildIndex(this.FlightNumberTextBox, 0);
			this.MasterGroupBox.Controls.SetChildIndex(this.CM_RL_NKPortOfFirstArrivalCodeFindBox, 0);
			this.MasterGroupBox.Controls.SetChildIndex(this.cTOAddressControl, 0);
			this.MasterGroupBox.Controls.SetChildIndex(this.cTOIDTextBox, 0);
			this.MasterGroupBox.Controls.SetChildIndex(this.cTOAddressLabel, 0);
			this.MasterGroupBox.Controls.SetChildIndex(this.cTOEstablishmentIDLabel, 0);
			this.MasterGroupBox.Controls.SetChildIndex(this.routingLabel, 0);
			this.MasterGroupBox.Controls.SetChildIndex(this.responsiblePartyTextBox, 0);
			this.MasterGroupBox.Controls.SetChildIndex(this.responsiblePartyLabel, 0);
			this.MasterGroupBox.Controls.SetChildIndex(this.responsiblePartyIDOrganisationFindBox, 0);
			this.MasterGroupBox.Controls.SetChildIndex(this.departureDateEdit, 0);
			this.MasterGroupBox.Controls.SetChildIndex(this.departureDateLabel, 0);
			// 
			// MasterTabControl
			// 
			this.MasterTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(960, 544, true);
			// 
			// HouseBillsTabPage
			// 
			this.houseBillsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(952, 517, true);
			this.houseBillsTabPage.Text = "Master Bills";
			// 
			// houseBillsSplitContainer
			// 
			// 
			// houseBillsSplitContainer.Panel1
			// 
			this.houseBillsSplitContainer.Panel1.Controls.Add(this.masterBillsGrid);
			this.houseBillsSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(946, 498, true);
			this.houseBillsSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(212);
			// 
			// HAWBTabControl
			// 
			this.HAWBTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(946, 282, true);
			// 
			// HouseDetailsTabPage
			// 
			this.HouseDetailsTabPage.Controls.Add(this.ctoHouseDetailsUserControl);
			this.HouseDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(938, 255, true);
			this.HouseDetailsTabPage.Text = "Master Bill Details";
			this.HouseDetailsTabPage.Controls.SetChildIndex(this.airCagoHouseBillPartiesUserControl, 0);
			this.HouseDetailsTabPage.Controls.SetChildIndex(this.ctoHouseDetailsUserControl, 0);
			// 
			// MiscInfoTabControl
			// 
			this.airCagoHouseBillPartiesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(642, 3, true);
			this.airCagoHouseBillPartiesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(380, 265, true);
			this.airCagoHouseBillPartiesUserControl.TabIndex = 1;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.CTOCusMAWB);
			// 
			// zLabel1
			// 
			this.zLabel1.AutoSize = true;
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(223, 44, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 13, true);
			this.zLabel1.TabIndex = 5;
			this.zLabel1.Text = "Port of 1st Arrival:";
			// 
			// CM_RL_NKPortOfFirstArrivalCodeFindBox
			// 
			this.CM_RL_NKPortOfFirstArrivalCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CM_RL_NKPortOfFirstArrivalCodeFindBox, "CM_RL_NKFirstArrivalPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CTOCusMAWB)(null)).CM_RL_NKFirstArrivalPort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CTOCusMAWB)(null)).Lookups.PortOfDischargeList)));
			this.CM_RL_NKPortOfFirstArrivalCodeFindBox.BindToList = "Lookups+PortOfDischargeList";
			this.CM_RL_NKPortOfFirstArrivalCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(319, 40, true);
			this.CM_RL_NKPortOfFirstArrivalCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.CM_RL_NKPortOfFirstArrivalCodeFindBox.Name = "CM_RL_NKPortOfFirstArrivalCodeFindBox";
			this.CM_RL_NKPortOfFirstArrivalCodeFindBox.PreBoundMaxLength = 5;
			this.CM_RL_NKPortOfFirstArrivalCodeFindBox.ShowDescriptionBox = false;
			this.CM_RL_NKPortOfFirstArrivalCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.CM_RL_NKPortOfFirstArrivalCodeFindBox.TabIndex = 4;
			// 
			// ctoHouseDetailsUserControl
			// 
			this.ctoHouseDetailsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ctoHouseDetailsUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.ctoHouseDetailsUserControl.HAWB = null;
			this.ctoHouseDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 3, true);
			this.ctoHouseDetailsUserControl.Name = "ctoHouseDetailsUserControl";
			this.ctoHouseDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(636, 264, true);
			this.ctoHouseDetailsUserControl.TabIndex = 0;
			// 
			// MessagesTabPage
			// 
			this.messagesTabPage.AdditionalText = "Details";
			this.messagesTabPage.Controls.Add(this.messagesUserControl);
			this.messagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.messagesTabPage.Name = "MessagesTabPage";
			this.messagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(930, 237, true);
			this.messagesTabPage.TabIndex = 1;
			this.messagesTabPage.Text = "MasterBill Messages";
			// 
			// MessagesUserControl
			// 
			this.messagesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.messagesUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Messaging.Business.IEDIMessageCollectionProvider)(((Enterprise.Customs.AU.Declaration.Business.CTOCusMAWB)(null)))));
			this.messagesUserControl.BindPrepend = "";
			this.messagesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.messagesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.messagesUserControl.Name = "MessagesUserControl";
			this.messagesUserControl.ShowChangingBlueMessageHeading = false;
			this.messagesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(930, 237, true);
			this.messagesUserControl.TabIndex = 0;
			// 
			// CTOAddressControl
			// 
			this.cTOAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.cTOAddressControl, "FakeFlightOuturnUnderbond+C4_OA_DestinationAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.AU.Declaration.Business.CTOCusMAWB)(null)).FakeFlightOuturnUnderbond.C4_OA_DestinationAddress)));
			this.cTOAddressControl.BindToOrgList = "FakeFlightOuturnUnderbond+Lookups+AirCTOs";
			this.cTOAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 63, true);
			this.cTOAddressControl.Name = "CTOAddressControl";
			this.cTOAddressControl.PopupCaption = "";
			this.cTOAddressControl.ShowAddress = false;
			this.cTOAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.cTOAddressControl.TabIndex = 6;
			// 
			// CTOIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.cTOIDTextBox, "FakeFlightOuturnUnderbond+C4_DestinationPremiseID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CTOCusMAWB)(null)).FakeFlightOuturnUnderbond.C4_DestinationPremiseID)));
			this.cTOIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(490, 63, true);
			this.cTOIDTextBox.Name = "CTOIDTextBox";
			this.cTOIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 20, true);
			this.cTOIDTextBox.TabIndex = 7;
			// 
			// CTOAddressLabel
			// 
			this.cTOAddressLabel.AutoSize = true;
			this.cTOAddressLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 67, true);
			this.cTOAddressLabel.Name = "CTOAddressLabel";
			this.cTOAddressLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 13, true);
			this.cTOAddressLabel.TabIndex = 10;
			this.cTOAddressLabel.Text = "CTO Address:";
			// 
			// CTOEstablishmentIDLabel
			// 
			this.cTOEstablishmentIDLabel.AutoSize = true;
			this.cTOEstablishmentIDLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(423, 67, true);
			this.cTOEstablishmentIDLabel.Name = "CTOEstablishmentIDLabel";
			this.cTOEstablishmentIDLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(61, 13, true);
			this.cTOEstablishmentIDLabel.TabIndex = 11;
			this.cTOEstablishmentIDLabel.Text = "Premise ID:";
			// 
			// Routing3CodeFindBox
			// 
			this.Routing3CodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.Routing3CodeFindBox, "CM_RL_NKRoutePort3");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CTOCusMAWB)(null)).CM_RL_NKRoutePort3)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CTOCusMAWB)(null)).Lookups.PortOfDischargeList)));
			this.Routing3CodeFindBox.BindToList = "Lookups+PortOfDischargeList";
			this.Routing3CodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(716, 64, true);
			this.Routing3CodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.Routing3CodeFindBox.Name = "Routing3CodeFindBox";
			this.Routing3CodeFindBox.PreBoundMaxLength = 5;
			this.Routing3CodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.Routing3CodeFindBox.TabIndex = 10;
			// 
			// Routing2CodeFindBox
			// 
			this.Routing2CodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.Routing2CodeFindBox, "CM_RL_NKRoutePort2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CTOCusMAWB)(null)).CM_RL_NKRoutePort2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CTOCusMAWB)(null)).Lookups.PortOfDischargeList)));
			this.Routing2CodeFindBox.BindToList = "Lookups+PortOfDischargeList";
			this.Routing2CodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(716, 44, true);
			this.Routing2CodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.Routing2CodeFindBox.Name = "Routing2CodeFindBox";
			this.Routing2CodeFindBox.PreBoundMaxLength = 5;
			this.Routing2CodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.Routing2CodeFindBox.TabIndex = 9;
			// 
			// RoutingCodeFindBox
			// 
			this.RoutingCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RoutingCodeFindBox, "CM_RL_NKRoutePort1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CTOCusMAWB)(null)).CM_RL_NKRoutePort1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CTOCusMAWB)(null)).Lookups.PortOfDischargeList)));
			this.RoutingCodeFindBox.BindToList = "Lookups+PortOfDischargeList";
			this.RoutingCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(716, 24, true);
			this.RoutingCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.RoutingCodeFindBox.Name = "RoutingCodeFindBox";
			this.RoutingCodeFindBox.PreBoundMaxLength = 5;
			this.RoutingCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.RoutingCodeFindBox.TabIndex = 8;
			// 
			// RoutingLabel
			// 
			this.routingLabel.IsFontBold = true;
			this.routingLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(716, 8, true);
			this.routingLabel.Name = "RoutingLabel";
			this.routingLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 16, true);
			this.routingLabel.TabIndex = 18;
			this.routingLabel.Text = "Routing";
			// 
			// ResponsiblePartyIDOrganisationFindBox
			// 
			this.responsiblePartyIDOrganisationFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.responsiblePartyIDOrganisationFindBox, "CM_OH_ResponsibleParty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.AU.Declaration.Business.CTOCusMAWB)(null)).CM_OH_ResponsibleParty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CTOCusMAWB)(null)).Lookups.ResponsibleParties)));
			this.responsiblePartyIDOrganisationFindBox.BindToList = "Lookups+ResponsibleParties";
			this.responsiblePartyIDOrganisationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(490, 16, true);
			this.responsiblePartyIDOrganisationFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.responsiblePartyIDOrganisationFindBox.Name = "ResponsiblePartyIDOrganisationFindBox";
			this.responsiblePartyIDOrganisationFindBox.PopupCaption = null;
			this.responsiblePartyIDOrganisationFindBox.ShowDescriptionBox = false;
			this.responsiblePartyIDOrganisationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.responsiblePartyIDOrganisationFindBox.TabIndex = 2;
			// 
			// ResponsiblePartyLabel
			// 
			this.responsiblePartyLabel.AutoSize = true;
			this.responsiblePartyLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(423, 20, true);
			this.responsiblePartyLabel.Name = "ResponsiblePartyLabel";
			this.responsiblePartyLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 13, true);
			this.responsiblePartyLabel.TabIndex = 19;
			this.responsiblePartyLabel.Text = "Resp. Id:";
			// 
			// ResponsiblePartyTextBox
			// 
			this.BindingSource.SetBindingMember(this.responsiblePartyTextBox, "CM_ResponsiblePartyID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CTOCusMAWB)(null)).CM_ResponsiblePartyID)));
			this.responsiblePartyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(588, 16, true);
			this.responsiblePartyTextBox.Name = "ResponsiblePartyTextBox";
			this.responsiblePartyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 20, true);
			this.responsiblePartyTextBox.TabIndex = 3;
			// 
			// MasterBillsGrid
			// 
			this.masterBillsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.masterBillsGrid, "ChildBills");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CTOCusMAWB)(null)).ChildBills)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CTOCusMAWB)(null)).ChildBills)).SyncRoot)).CS_MessageReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CTOCusMAWB)(null)).ChildBills)).SyncRoot)).CS_HAWB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CTOCusMAWB)(null)).ChildBills)).SyncRoot)).CS_RL_NKLoadPort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CTOCusMAWB)(null)).ChildBills)).SyncRoot)).Lookups.OriginList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CTOCusMAWB)(null)).ChildBills)).SyncRoot)).CS_RL_NKOrigin)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CTOCusMAWB)(null)).ChildBills)).SyncRoot)).Lookups.OriginList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CTOCusMAWB)(null)).ChildBills)).SyncRoot)).CS_RL_NKDestination)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CTOCusMAWB)(null)).ChildBills)).SyncRoot)).Lookups.DestinationList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CTOCusMAWB)(null)).ChildBills)).SyncRoot)).CS_Weight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CTOCusMAWB)(null)).ChildBills)).SyncRoot)).CS_WeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CTOCusMAWB)(null)).ChildBills)).SyncRoot)).Lookups.UnitOfWeightList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CTOCusMAWB)(null)).ChildBills)).SyncRoot)).CS_GoodsValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CTOCusMAWB)(null)).ChildBills)).SyncRoot)).CS_RX_NKGoodsCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CTOCusMAWB)(null)).ChildBills)).SyncRoot)).Lookups.CurrencyList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CTOCusMAWB)(null)).ChildBills)).SyncRoot)).CS_GoodsDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CTOCusMAWB)(null)).ChildBills)).SyncRoot)).CS_IsMasterHouse)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CTOCusMAWB)(null)).ChildBills)).SyncRoot)).CS_PiecesManifested)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CTOCusMAWB)(null)).ChildBills)).SyncRoot)).CS_FreightPrepaidCollect)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CTOCusMAWB)(null)).ChildBills)).SyncRoot)).Lookups.PrepaidCollectList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CTOCusMAWB)(null)).ChildBills)).SyncRoot)).CS_WarehouseLocation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CTOCusMAWB)(null)).ChildBills)).SyncRoot)).CS_FolioReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CTOCusMAWB)(null)).ChildBills)).SyncRoot)).CS_ShipmentType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CTOCusMAWB)(null)).ChildBills)).SyncRoot)).Lookups.ShipmentTypeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CTOCusMAWB)(null)).ChildBills)).SyncRoot)).CS_IsPersonalEffects)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CTOCusMAWB)(null)).ChildBills)).SyncRoot)).CS_IsSelfAssessedClearance)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CTOCusMAWB)(null)).ChildBills)).SyncRoot)).CMRCargoStatus.Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CTOCusMAWB)(null)).ChildBills)).SyncRoot)).CMRMessageStatus.Description)));
			this.masterBillsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "Ref. No";
			zTextBoxColumnStyleInfo1.ColumnName = "CS_MessageReference";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.Caption = "MAWB";
			zTextBoxColumnStyleInfo2.ColumnName = "CS_HAWB";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo1.BindToList = "Lookups.OriginList";
			zCodeFindBoxColumnStyleInfo1.Caption = "Load";
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CS_RL_NKLoadPort";
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo2.BindToList = "Lookups.OriginList";
			zCodeFindBoxColumnStyleInfo2.Caption = "Origin";
			zCodeFindBoxColumnStyleInfo2.ColumnName = "CS_RL_NKOrigin";
			zCodeFindBoxColumnStyleInfo2.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo3.BindToList = "Lookups.DestinationList";
			zCodeFindBoxColumnStyleInfo3.Caption = "Destination";
			zCodeFindBoxColumnStyleInfo3.ColumnName = "CS_RL_NKDestination";
			zCodeFindBoxColumnStyleInfo3.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			zCodeFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = "Weight";
			zCalcEditColumnStyleInfo1.ColumnName = "CS_Weight";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.BindToList = "Lookups.UnitOfWeightList";
			zDropEditColumnStyleInfo1.Caption = "Wgt UQ";
			zDropEditColumnStyleInfo1.ColumnName = "CS_WeightUQ";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.Caption = "Goods Value";
			zCalcEditColumnStyleInfo2.ColumnName = "CS_GoodsValue";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo4.BindToList = "Lookups.CurrencyList";
			zCodeFindBoxColumnStyleInfo4.Caption = "Curr";
			zCodeFindBoxColumnStyleInfo4.ColumnName = "CS_RX_NKGoodsCurrency";
			zCodeFindBoxColumnStyleInfo4.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			zCodeFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.Caption = "Goods Desc";
			zTextBoxColumnStyleInfo3.ColumnName = "CS_GoodsDescription";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.Caption = "Consolidation?";
			zCheckBoxColumnStyleInfo1.ColumnName = "CS_IsMasterHouse";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.Caption = "Pieces";
			zCalcEditColumnStyleInfo3.ColumnName = "CS_PiecesManifested";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.BindToList = "Lookups.PrepaidCollectList";
			zDropEditColumnStyleInfo2.Caption = "Prepaid/Collect";
			zDropEditColumnStyleInfo2.ColumnName = "CS_FreightPrepaidCollect";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.Caption = "Warehouse Location";
			zTextBoxColumnStyleInfo4.ColumnName = "CS_WarehouseLocation";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.Caption = "Folio Reference";
			zTextBoxColumnStyleInfo5.ColumnName = "CS_FolioReference";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo3.BindToList = "Lookups.ShipmentTypeList";
			zDropEditColumnStyleInfo3.Caption = "Shipment Type";
			zDropEditColumnStyleInfo3.ColumnName = "CS_ShipmentType";
			zDropEditColumnStyleInfo3.IsVisible = false;
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo2.Caption = "P/E";
			zCheckBoxColumnStyleInfo2.ColumnName = "CS_IsPersonalEffects";
			zCheckBoxColumnStyleInfo2.ToolTip = "Personal Effects";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCheckBoxColumnStyleInfo3.Caption = "SAC";
			zCheckBoxColumnStyleInfo3.ColumnName = "CS_IsSelfAssessedClearance";
			zCheckBoxColumnStyleInfo3.ToolTip = "Self Assessed Clearance";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo6.Caption = "Customs Status";
			zTextBoxColumnStyleInfo6.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo6.ColumnName = "CMRCargoStatus+Description";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo7.Caption = "Message Status";
			zTextBoxColumnStyleInfo7.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo7.ColumnName = "CMRMessageStatus+Description";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.masterBillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.masterBillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.masterBillsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.masterBillsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.masterBillsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo3);
			this.masterBillsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.masterBillsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.masterBillsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.masterBillsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo4);
			this.masterBillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.masterBillsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.masterBillsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.masterBillsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.masterBillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.masterBillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.masterBillsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.masterBillsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.masterBillsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.masterBillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.masterBillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.masterBillsGrid.CopySelectedRowsAllowed = true;
			this.masterBillsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.masterBillsGrid.GridId = "47afbc32-f2ce-4e46-aa02-dcf097f0a1d2";
			this.masterBillsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.masterBillsGrid.LayoutKey = "MasterBillsGrid";
			this.masterBillsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.masterBillsGrid.Name = "MasterBillsGrid";
			this.masterBillsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(946, 212, true);
			this.masterBillsGrid.TabIndex = 1;
			// 
			// DepartureDateEdit
			// 
			this.departureDateEdit.AllowDrop = true;
			this.departureDateEdit.AutoCompleteMonthThreshold = 1;
			this.departureDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.departureDateEdit, "CM_DepartureDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.CTOCusMAWB)(null)).CM_DepartureDate)));
			this.departureDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(490, 40, true);
			this.departureDateEdit.Name = "DepartureDateEdit";
			this.departureDateEdit.TabIndex = 5;
			// 
			// DepartureDateLabel
			// 
			this.departureDateLabel.AutoSize = true;
			this.departureDateLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(423, 44, true);
			this.departureDateLabel.Name = "DepartureDateLabel";
			this.departureDateLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(54, 13, true);
			this.departureDateLabel.TabIndex = 21;
			this.departureDateLabel.Text = "Departure";
			// 
			// AirCTOUserDetailsControl
			// 
			this.Name = "AirCTOUserDetailsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(960, 632, true);
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
			this.CM_RL_NKPortOfFirstArrivalCodeFindBox.ResumeLayout(true);
			this.CM_RL_NKPortOfFirstArrivalCodeFindBox.PerformLayout();
			this.ctoHouseDetailsUserControl.ResumeLayout(true);
			this.ctoHouseDetailsUserControl.PerformLayout();
			this.messagesTabPage.ResumeLayout(false);
			this.messagesTabPage.PerformLayout();
			this.messagesUserControl.ResumeLayout(true);
			this.messagesUserControl.PerformLayout();
			this.cTOAddressControl.ResumeLayout(true);
			this.cTOAddressControl.PerformLayout();
			this.Routing3CodeFindBox.ResumeLayout(true);
			this.Routing3CodeFindBox.PerformLayout();
			this.Routing2CodeFindBox.ResumeLayout(true);
			this.Routing2CodeFindBox.PerformLayout();
			this.RoutingCodeFindBox.ResumeLayout(true);
			this.RoutingCodeFindBox.PerformLayout();
			this.responsiblePartyIDOrganisationFindBox.ResumeLayout(true);
			this.responsiblePartyIDOrganisationFindBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.masterBillsGrid)).EndInit();
			this.masterBillsGrid.ResumeLayout(false);
			this.masterBillsGrid.PerformLayout();
			this.departureDateEdit.ResumeLayout(true);
			this.departureDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private ZLabel zLabel1;
		public ZCodeFindBox CM_RL_NKPortOfFirstArrivalCodeFindBox;
		public CTOHouseDetailsUserControl ctoHouseDetailsUserControl;
		private ZDetailsTabPage messagesTabPage;
		internal EDIMessageUserControl messagesUserControl;
		private ZAddressControl cTOAddressControl;
		private ZTextBox cTOIDTextBox;
		private ZLabel cTOAddressLabel;
		private ZLabel cTOEstablishmentIDLabel;
		public ZCodeFindBox Routing3CodeFindBox;
		public ZCodeFindBox Routing2CodeFindBox;
		public ZCodeFindBox RoutingCodeFindBox;
		private ZLabel routingLabel;
		private MasterFiles.GUI.ZOrganisationFindBox responsiblePartyIDOrganisationFindBox;
		protected ZLabel responsiblePartyLabel;
		protected ZTextBox responsiblePartyTextBox;
		internal ZGrid masterBillsGrid;
		private ZLabel departureDateLabel;
		private ZDateEdit departureDateEdit;
	}
}
