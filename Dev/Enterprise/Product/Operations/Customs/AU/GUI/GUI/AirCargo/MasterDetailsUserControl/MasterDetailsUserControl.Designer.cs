using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.AirCargo.GUI
{
	public partial class MasterDetailsUserControl
	{
		private void InitializeComponent()
		{
			this.masterGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.departureDateLabel = new Enterprise.ZArchitecture.ZLabel();
			this.departureDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.AltPartShipModelCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.cFSAddressLabel = new Enterprise.ZArchitecture.ZLabel();
			this.depotAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.coLoadMasterTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.coLoadMasterLabel = new Enterprise.ZArchitecture.ZLabel();
			this.firstArrivalDateLabel = new Enterprise.ZArchitecture.ZLabel();
			this.firstArrivalDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.folioTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.folioLabel = new Enterprise.ZArchitecture.ZLabel();
			this.masterBillControl = new Enterprise.ZArchitecture.ZMasterBillControl();
			this.flightNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.masterBillLabel = new Enterprise.ZArchitecture.ZLabel();
			this.arrivalDateLabel = new Enterprise.ZArchitecture.ZLabel();
			this.portOfLoadingLabel = new Enterprise.ZArchitecture.ZLabel();
			this.portOfDischargeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.flightNumberLabel = new Enterprise.ZArchitecture.ZLabel();
			this.arivalDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.portOfLoadingCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.portOfDischargeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.branchLabel = new Enterprise.ZArchitecture.ZLabel();
			this.branchGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.masterGroupBox.SuspendLayout();
			this.departureDateEdit.SuspendLayout();
			this.depotAddressControl.SuspendLayout();
			this.firstArrivalDateDateEdit.SuspendLayout();
			this.masterBillControl.SuspendLayout();
			this.arivalDateDateEdit.SuspendLayout();
			this.portOfLoadingCodeFindBox.SuspendLayout();
			this.portOfDischargeCodeFindBox.SuspendLayout();
			this.branchGuidFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.CusHAWB);
			// 
			// MasterGroupBox
			// 
			this.masterGroupBox.Controls.Add(this.departureDateLabel);
			this.masterGroupBox.Controls.Add(this.departureDateEdit);
			this.masterGroupBox.Controls.Add(this.AltPartShipModelCheckBox);
			this.masterGroupBox.Controls.Add(this.cFSAddressLabel);
			this.masterGroupBox.Controls.Add(this.depotAddressControl);
			this.masterGroupBox.Controls.Add(this.coLoadMasterTextBox);
			this.masterGroupBox.Controls.Add(this.coLoadMasterLabel);
			this.masterGroupBox.Controls.Add(this.firstArrivalDateLabel);
			this.masterGroupBox.Controls.Add(this.firstArrivalDateDateEdit);
			this.masterGroupBox.Controls.Add(this.folioTextBox);
			this.masterGroupBox.Controls.Add(this.folioLabel);
			this.masterGroupBox.Controls.Add(this.masterBillControl);
			this.masterGroupBox.Controls.Add(this.flightNumberTextBox);
			this.masterGroupBox.Controls.Add(this.masterBillLabel);
			this.masterGroupBox.Controls.Add(this.arrivalDateLabel);
			this.masterGroupBox.Controls.Add(this.portOfLoadingLabel);
			this.masterGroupBox.Controls.Add(this.portOfDischargeLabel);
			this.masterGroupBox.Controls.Add(this.flightNumberLabel);
			this.masterGroupBox.Controls.Add(this.arivalDateDateEdit);
			this.masterGroupBox.Controls.Add(this.portOfLoadingCodeFindBox);
			this.masterGroupBox.Controls.Add(this.portOfDischargeCodeFindBox);
			this.masterGroupBox.Controls.Add(this.branchLabel);
			this.masterGroupBox.Controls.Add(this.branchGuidFindBox);
			this.masterGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.masterGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.masterGroupBox.Name = "MasterGroupBox";
			this.masterGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(973, 113, true);
			this.masterGroupBox.TabIndex = 0;
			this.masterGroupBox.TabStop = false;
			this.masterGroupBox.Text = "Master Details";
			// 
			// DepartureDateLabel
			// 
			this.departureDateLabel.AutoSize = true;
			this.departureDateLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(218, 60, true);
			this.departureDateLabel.Name = "DepartureDateLabel";
			this.departureDateLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(54, 13, true);
			this.departureDateLabel.TabIndex = 42;
			this.departureDateLabel.Text = "Departure";
			// 
			// DepartureDateEdit
			// 
			this.departureDateEdit.AllowDrop = true;
			this.departureDateEdit.AutoCompleteMonthThreshold = 1;
			this.departureDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.departureDateEdit, "MAWB.CM_DepartureDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(null)).MAWB.CM_DepartureDate)));
			this.departureDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(304, 58, true);
			this.departureDateEdit.Name = "DepartureDateEdit";
			this.departureDateEdit.TabIndex = 5;
			// 
			// AltPartShipModelCheckBox
			// 
			this.AltPartShipModelCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AltPartShipModelCheckBox, "MAWB.CM_fUseAltPartShipModel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(null)).MAWB.CM_fUseAltPartShipModel)));
			this.AltPartShipModelCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AltPartShipModelCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(781, 85, true);
			this.AltPartShipModelCheckBox.Name = "AltPartShipModelCheckBox";
			this.AltPartShipModelCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 17, true);
			this.AltPartShipModelCheckBox.TabIndex = 11;
			this.AltPartShipModelCheckBox.Text = "Generate Consignment References";
			// 
			// CFSAddressLabel
			// 
			this.cFSAddressLabel.AutoSize = true;
			this.cFSAddressLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 86, true);
			this.cFSAddressLabel.Name = "CFSAddressLabel";
			this.cFSAddressLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 13, true);
			this.cFSAddressLabel.TabIndex = 40;
			this.cFSAddressLabel.Text = "CFS Address:";
			// 
			// DepotAddressControl
			// 
			this.depotAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.depotAddressControl, "MAWB.CM_OA_UnpackDepotAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(null)).MAWB.CM_OA_UnpackDepotAddress)));
			this.depotAddressControl.BindToOrgList = "MAWB.Lookups+UnpackDepotOrganisations";
			this.depotAddressControl.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("d940476e-db97-4ab0-8a69-86c376df5dcc", "CFS", "CFS Address", "");
			this.depotAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 84, true);
			this.depotAddressControl.Name = "DepotAddressControl";
			this.depotAddressControl.PopupCaption = "";
			this.depotAddressControl.ShowAddress = false;
			this.depotAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.depotAddressControl.TabIndex = 9;
			// 
			// CoLoadMasterTextBox
			// 
			this.BindingSource.SetBindingMember(this.coLoadMasterTextBox, "MAWB.CM_MasterHouseBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(null)).MAWB.CM_MasterHouseBill)));
			this.coLoadMasterTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(512, 58, true);
			this.coLoadMasterTextBox.Name = "CoLoadMasterTextBox";
			this.coLoadMasterTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.coLoadMasterTextBox.TabIndex = 8;
			// 
			// CoLoadMasterLabel
			// 
			this.coLoadMasterLabel.AutoSize = true;
			this.coLoadMasterLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 60, true);
			this.coLoadMasterLabel.Name = "CoLoadMasterLabel";
			this.coLoadMasterLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 13, true);
			this.coLoadMasterLabel.TabIndex = 38;
			this.coLoadMasterLabel.Text = "Co-Load:";
			// 
			// FirstArrivalDateLabel
			// 
			this.firstArrivalDateLabel.AutoSize = true;
			this.firstArrivalDateLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(216, 37, true);
			this.firstArrivalDateLabel.Name = "FirstArrivalDateLabel";
			this.firstArrivalDateLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 13, true);
			this.firstArrivalDateLabel.TabIndex = 36;
			this.firstArrivalDateLabel.Text = "1st Arrival:";
			// 
			// FirstArrivalDateDateEdit
			// 
			this.firstArrivalDateDateEdit.AllowDrop = true;
			this.firstArrivalDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.firstArrivalDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.firstArrivalDateDateEdit, "MAWB+CM_DateOfFirstArrival");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(null)).MAWB.CM_DateOfFirstArrival)));
			this.firstArrivalDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(304, 35, true);
			this.firstArrivalDateDateEdit.Name = "FirstArrivalDateDateEdit";
			this.firstArrivalDateDateEdit.TabIndex = 4;
			// 
			// FolioTextBox
			// 
			this.BindingSource.SetBindingMember(this.folioTextBox, "MAWB.CM_Folio");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(null)).MAWB.CM_Folio)));
			this.folioTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 58, true);
			this.folioTextBox.Name = "FolioTextBox";
			this.folioTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.folioTextBox.TabIndex = 2;
			// 
			// FolioLabel
			// 
			this.folioLabel.AutoSize = true;
			this.folioLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 60, true);
			this.folioLabel.Name = "FolioLabel";
			this.folioLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 13, true);
			this.folioLabel.TabIndex = 33;
			this.folioLabel.Text = "Folio:";
			// 
			// MasterBillControl
			// 
			this.masterBillControl.AllowAlphaInMAWP = false;
			this.masterBillControl.AllowDrop = true;
			this.masterBillControl.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.masterBillControl, "MAWB.CM_MAWB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(null)).MAWB.CM_MAWB)));
			this.masterBillControl.FormattedMasterBill = "";
			this.masterBillControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 13, true);
			this.masterBillControl.Name = "MasterBillControl";
			this.masterBillControl.ReadOnly = false;
			this.masterBillControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.masterBillControl.TabIndex = 0;
			// 
			// FlightNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.flightNumberTextBox, "MAWB.CM_FlightNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(null)).MAWB.CM_FlightNo)));
			this.flightNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 35, true);
			this.flightNumberTextBox.Name = "FlightNumberTextBox";
			this.flightNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.flightNumberTextBox.TabIndex = 1;
			// 
			// MasterBillLabel
			// 
			this.masterBillLabel.AutoSize = true;
			this.masterBillLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 15, true);
			this.masterBillLabel.Name = "MasterBillLabel";
			this.masterBillLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 13, true);
			this.masterBillLabel.TabIndex = 24;
			this.masterBillLabel.Text = "Master:";
			// 
			// ArrivalDateLabel
			// 
			this.arrivalDateLabel.AutoSize = true;
			this.arrivalDateLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(216, 15, true);
			this.arrivalDateLabel.Name = "ArrivalDateLabel";
			this.arrivalDateLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(39, 13, true);
			this.arrivalDateLabel.TabIndex = 28;
			this.arrivalDateLabel.Text = "Arrival:";
			// 
			// PortOfLoadingLabel
			// 
			this.portOfLoadingLabel.AutoSize = true;
			this.portOfLoadingLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 15, true);
			this.portOfLoadingLabel.Name = "PortOfLoadingLabel";
			this.portOfLoadingLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 13, true);
			this.portOfLoadingLabel.TabIndex = 29;
			this.portOfLoadingLabel.Text = "Loading:";
			// 
			// PortOfDischargeLabel
			// 
			this.portOfDischargeLabel.AutoSize = true;
			this.portOfDischargeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 37, true);
			this.portOfDischargeLabel.Name = "PortOfDischargeLabel";
			this.portOfDischargeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(58, 13, true);
			this.portOfDischargeLabel.TabIndex = 32;
			this.portOfDischargeLabel.Text = "Discharge:";
			// 
			// FlightNumberLabel
			// 
			this.flightNumberLabel.AutoSize = true;
			this.flightNumberLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 37, true);
			this.flightNumberLabel.Name = "FlightNumberLabel";
			this.flightNumberLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(35, 13, true);
			this.flightNumberLabel.TabIndex = 26;
			this.flightNumberLabel.Text = "Flight:";
			// 
			// ArivalDateDateEdit
			// 
			this.arivalDateDateEdit.AllowDrop = true;
			this.arivalDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.arivalDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.arivalDateDateEdit, "MAWB.CM_ArrivalDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(null)).MAWB.CM_ArrivalDate)));
			this.arivalDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(304, 13, true);
			this.arivalDateDateEdit.Name = "ArivalDateDateEdit";
			this.arivalDateDateEdit.TabIndex = 3;
			// 
			// PortOfLoadingCodeFindBox
			// 
			this.portOfLoadingCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.portOfLoadingCodeFindBox, "MAWB.CM_RL_NKLoadPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(null)).MAWB.CM_RL_NKLoadPort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(null)).MAWB.Lookups.PortOfLoadingList)));
			this.portOfLoadingCodeFindBox.BindToList = "MAWB+Lookups+PortOfLoadingList";
			this.portOfLoadingCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(512, 13, true);
			this.portOfLoadingCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.portOfLoadingCodeFindBox.Name = "PortOfLoadingCodeFindBox";
			this.portOfLoadingCodeFindBox.PreBoundMaxLength = 5;
			this.portOfLoadingCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.portOfLoadingCodeFindBox.TabIndex = 6;
			// 
			// PortOfDischargeCodeFindBox
			// 
			this.portOfDischargeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.portOfDischargeCodeFindBox, "MAWB.CM_RL_NKDischargePort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(null)).MAWB.CM_RL_NKDischargePort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(null)).MAWB.Lookups.PortOfDischargeList)));
			this.portOfDischargeCodeFindBox.BindToList = "MAWB+Lookups+PortOfDischargeList";
			this.portOfDischargeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(512, 35, true);
			this.portOfDischargeCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.portOfDischargeCodeFindBox.Name = "PortOfDischargeCodeFindBox";
			this.portOfDischargeCodeFindBox.PreBoundMaxLength = 5;
			this.portOfDischargeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.portOfDischargeCodeFindBox.TabIndex = 7;
			// 
			// branchLabel
			// 
			this.branchLabel.AutoSize = true;
			this.branchLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 86, true);
			this.branchLabel.Name = "branchLabel";
			this.branchLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 13, true);
			this.branchLabel.TabIndex = 17;
			this.branchLabel.Text = "Branch Code:";
			// 
			// BranchGuidFindBox
			// 
			this.branchGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.branchGuidFindBox, "MAWB.CM_GB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(null)).MAWB.CM_GB)));
			this.branchGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(512, 83, true);
			this.branchGuidFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbBranch;
			this.branchGuidFindBox.Name = "BranchGuidFindBox";
			this.branchGuidFindBox.PreBoundMaxLength = 3;
			this.branchGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.branchGuidFindBox.TabIndex = 10;
			// 
			// MasterDetailsUserControl
			// 
			this.Controls.Add(this.masterGroupBox);
			this.Name = "MasterDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(973, 113, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.masterGroupBox.ResumeLayout(false);
			this.masterGroupBox.PerformLayout();
			this.departureDateEdit.ResumeLayout(true);
			this.departureDateEdit.PerformLayout();
			this.depotAddressControl.ResumeLayout(true);
			this.depotAddressControl.PerformLayout();
			this.firstArrivalDateDateEdit.ResumeLayout(true);
			this.firstArrivalDateDateEdit.PerformLayout();
			this.masterBillControl.ResumeLayout(true);
			this.masterBillControl.PerformLayout();
			this.arivalDateDateEdit.ResumeLayout(true);
			this.arivalDateDateEdit.PerformLayout();
			this.portOfLoadingCodeFindBox.ResumeLayout(true);
			this.portOfLoadingCodeFindBox.PerformLayout();
			this.portOfDischargeCodeFindBox.ResumeLayout(true);
			this.portOfDischargeCodeFindBox.PerformLayout();
			this.branchGuidFindBox.ResumeLayout(true);
			this.branchGuidFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private ZGroupBox masterGroupBox;
		private ZArchitecture.ZLabel firstArrivalDateLabel;
		private ZDateEdit firstArrivalDateDateEdit;
		private ZArchitecture.ZTextBox folioTextBox;
		private ZArchitecture.ZLabel folioLabel;
		private ZArchitecture.ZMasterBillControl masterBillControl;
		private ZArchitecture.ZTextBox flightNumberTextBox;
		private ZArchitecture.ZLabel masterBillLabel;
		private ZArchitecture.ZLabel arrivalDateLabel;
		private ZArchitecture.ZLabel portOfLoadingLabel;
		private ZArchitecture.ZLabel portOfDischargeLabel;
		private ZArchitecture.ZLabel flightNumberLabel;
		private ZDateEdit arivalDateDateEdit;
		private ZCodeFindBox portOfLoadingCodeFindBox;
		private ZCodeFindBox portOfDischargeCodeFindBox;
		private ZArchitecture.ZTextBox coLoadMasterTextBox;
		private ZArchitecture.ZLabel coLoadMasterLabel;
		private ZGuidFindBox branchGuidFindBox;
		private ZArchitecture.ZLabel branchLabel;
		private ZArchitecture.ZLabel cFSAddressLabel;
		private ZAddressControl depotAddressControl;
		protected internal ZCheckBox AltPartShipModelCheckBox;
		private ZArchitecture.ZLabel departureDateLabel;
		private ZDateEdit departureDateEdit;
	}
}
