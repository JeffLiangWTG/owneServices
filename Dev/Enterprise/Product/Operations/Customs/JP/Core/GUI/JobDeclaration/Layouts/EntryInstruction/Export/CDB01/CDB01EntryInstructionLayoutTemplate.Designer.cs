namespace Enterprise.Customs.JP.GUI
{
	partial class CDB01EntryInstructionLayoutTemplate
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.DateForDutyDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.CDB01CargoTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CDB01PermitNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CDB01MoveInUserControl = new Enterprise.Customs.JP.GUI.CDB01MoveInUserControl();
			this.CDB01BillNumberUserControl = new Enterprise.Customs.JP.GUI.CDB01BillNumberUserControl();
			this.FinalDestinationCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.FinalDestinationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CarrierGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CarrierCodeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ShippingOrAirLineOrganisationGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ForwarderGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ForWarderNACCSCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AirCargoAgentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AirCargoAgentDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.AirCargoAgentNACCSCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AirCargoAgentLocationCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ExternalBrokerGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ExternalBrokerDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.ExternalBrokerCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PortOfLoadingCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.PortOfLoadingTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MAWBTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PortOfLoadingPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.FinalDestinationPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ForwarderDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DateForDutyDateEdit.SuspendLayout();
			this.CDB01CargoTypeDropEdit.SuspendLayout();
			this.CDB01MoveInUserControl.SuspendLayout();
			this.CDB01BillNumberUserControl.SuspendLayout();
			this.FinalDestinationCodeFindBox.SuspendLayout();
			this.CarrierGroupBox.SuspendLayout();
			this.CarrierCodeCodeFindBox.SuspendLayout();
			this.ShippingOrAirLineOrganisationGuidFindBox.SuspendLayout();
			this.ForwarderGroupBox.SuspendLayout();
			this.AirCargoAgentGroupBox.SuspendLayout();
			this.AirCargoAgentDocAddressControl.SuspendLayout();
			this.ExternalBrokerGroupBox.SuspendLayout();
			this.ExternalBrokerDocAddressControl.SuspendLayout();
			this.PortOfLoadingCodeFindBox.SuspendLayout();
			this.PortOfLoadingPanel.SuspendLayout();
			this.FinalDestinationPanel.SuspendLayout();
			this.ForwarderDocAddressControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.JP.Business.CusEntryInstruction);
			// 
			// DateForDutyDateEdit
			// 
			this.DateForDutyDateEdit.AllowDrop = true;
			this.DateForDutyDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.DateForDutyDateEdit, "CEI_DateForDuty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).CEI_DateForDuty)));
			this.DateForDutyDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 36, true);
			this.DateForDutyDateEdit.Name = "DateForDutyDateEdit";
			this.DateForDutyDateEdit.TabIndex = 1;
			// 
			// CDB01CargoTypeDropEdit
			// 
			this.CDB01CargoTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CDB01CargoTypeDropEdit, "CEI_CDB01CargoType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).CEI_CDB01CargoType)));
			this.CDB01CargoTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 62, true);
			this.CDB01CargoTypeDropEdit.Name = "CDB01CargoTypeDropEdit";
			this.CDB01CargoTypeDropEdit.PreBoundMaxLength = 1;
			this.CDB01CargoTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.CDB01CargoTypeDropEdit.TabIndex = 3;
			// 
			// CDB01PermitNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.CDB01PermitNumberTextBox, "CEI_CDB01PermitNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).CEI_CDB01PermitNo)));
			this.CDB01PermitNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 88, true);
			this.CDB01PermitNumberTextBox.Name = "CDB01PermitNumberTextBox";
			this.CDB01PermitNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.CDB01PermitNumberTextBox.TabIndex = 4;
			// 
			// CDB01MoveInUserControl
			// 
			this.CDB01MoveInUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CDB01MoveInUserControl, ".");
			this.CDB01MoveInUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 114, true);
			this.CDB01MoveInUserControl.Name = "CDB01MoveInUserControl";
			this.CDB01MoveInUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(359, 188, true);
			this.CDB01MoveInUserControl.TabIndex = 5;
			// 
			// CDB01BillNumberUserControl
			// 
			this.CDB01BillNumberUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CDB01BillNumberUserControl, ".");
			this.CDB01BillNumberUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 4, true);
			this.CDB01BillNumberUserControl.Name = "CDB01BillNumberUserControl";
			this.CDB01BillNumberUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 26, true);
			this.CDB01BillNumberUserControl.TabIndex = 0;
			// 
			// FinalDestinationCodeFindBox
			// 
			this.FinalDestinationCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FinalDestinationCodeFindBox, "JobDeclaration.JE_RL_NKFinalDestination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).JobDeclaration.JE_RL_NKFinalDestination)));
			this.FinalDestinationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FinalDestinationCodeFindBox.Name = "FinalDestinationCodeFindBox";
			this.FinalDestinationCodeFindBox.ParentType = null;
			this.FinalDestinationCodeFindBox.PreBoundMaxLength = 5;
			this.FinalDestinationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 20, true);
			this.FinalDestinationCodeFindBox.TabIndex = 14;
			// 
			// FinalDestinationTextBox
			// 
			this.BindingSource.SetBindingMember(this.FinalDestinationTextBox, "JobDeclaration.FinalDestinationIATACode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).JobDeclaration.FinalDestinationIATACode)));
			this.FinalDestinationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 0, true);
			this.FinalDestinationTextBox.Name = "FinalDestinationTextBox";
			this.FinalDestinationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.FinalDestinationTextBox.TabIndex = 15;
			// 
			// CarrierGroupBox
			// 
			this.CarrierGroupBox.CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("666a22ca-949e-40ee-8b70-690ca4ae99a6", "Carrier");
			this.CarrierGroupBox.Controls.Add(this.CarrierCodeCodeFindBox);
			this.CarrierGroupBox.Controls.Add(this.ShippingOrAirLineOrganisationGuidFindBox);
			this.CarrierGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(404, 357, true);
			this.CarrierGroupBox.Name = "CarrierGroupBox";
			this.CarrierGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(403, 74, true);
			this.CarrierGroupBox.TabIndex = 19;
			this.CarrierGroupBox.TabStop = false;
			// 
			// CarrierCodeCodeFindBox
			// 
			this.CarrierCodeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CarrierCodeCodeFindBox, "JobDeclaration.JE_CarrierCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).JobDeclaration.JE_CarrierCode)));
			this.CarrierCodeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 45, true);
			this.CarrierCodeCodeFindBox.Name = "CarrierCodeCodeFindBox";
			this.CarrierCodeCodeFindBox.ParentType = null;
			this.CarrierCodeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.CarrierCodeCodeFindBox.TabIndex = 1;
			// 
			// ShippingOrAirLineOrganisationGuidFindBox
			// 
			this.ShippingOrAirLineOrganisationGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShippingOrAirLineOrganisationGuidFindBox, "JobDeclaration.JE_OH_ShippingLine");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).JobDeclaration.JE_OH_ShippingLine)));
			this.ShippingOrAirLineOrganisationGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 19, true);
			this.ShippingOrAirLineOrganisationGuidFindBox.Name = "ShippingOrAirLineOrganisationGuidFindBox";
			this.ShippingOrAirLineOrganisationGuidFindBox.ParentType = null;
			this.ShippingOrAirLineOrganisationGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.ShippingOrAirLineOrganisationGuidFindBox.TabIndex = 0;
			// 
			// ForwarderGroupBox
			// 
			this.ForwarderGroupBox.CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("8c6979f4-4cea-40dc-b0f4-2d5dd5fd1475", "Forwarder");
			this.ForwarderGroupBox.Controls.Add(this.ForwarderDocAddressControl);
			this.ForwarderGroupBox.Controls.Add(this.ForWarderNACCSCodeTextBox);
			this.ForwarderGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(404, 274, true);
			this.ForwarderGroupBox.Name = "ForwarderGroupBox";
			this.ForwarderGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(403, 77, true);
			this.ForwarderGroupBox.TabIndex = 18;
			this.ForwarderGroupBox.TabStop = false;
			// 
			// ForWarderNACCSCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.ForWarderNACCSCodeTextBox, "JobDeclaration.ForwarderCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).JobDeclaration.ForwarderCode)));
			this.ForWarderNACCSCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 45, true);
			this.ForWarderNACCSCodeTextBox.Name = "ForWarderNACCSCodeTextBox";
			this.ForWarderNACCSCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.ForWarderNACCSCodeTextBox.TabIndex = 1;
			// 
			// AirCargoAgentGroupBox
			// 
			this.AirCargoAgentGroupBox.CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("defd9d64-5645-4ab8-830c-1675cf97b203", "Air Cargo Agent");
			this.AirCargoAgentGroupBox.Controls.Add(this.AirCargoAgentDocAddressControl);
			this.AirCargoAgentGroupBox.Controls.Add(this.AirCargoAgentNACCSCodeTextBox);
			this.AirCargoAgentGroupBox.Controls.Add(this.AirCargoAgentLocationCodeTextBox);
			this.AirCargoAgentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(404, 168, true);
			this.AirCargoAgentGroupBox.Name = "AirCargoAgentGroupBox";
			this.AirCargoAgentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(403, 100, true);
			this.AirCargoAgentGroupBox.TabIndex = 17;
			this.AirCargoAgentGroupBox.TabStop = false;
			// 
			// AirCargoAgentDocAddressControl
			// 
			this.AirCargoAgentDocAddressControl.AddressValidationProcessCmdKey = null;
			this.AirCargoAgentDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AirCargoAgentDocAddressControl, "JobDeclaration.AirCargoAgent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).JobDeclaration.AirCargoAgent)));
			this.AirCargoAgentDocAddressControl.BindToOrganisations = "JobDeclaration.Lookups.InspectionWitnessOrganisations";
			this.AirCargoAgentDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
			this.AirCargoAgentDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 19, true);
			this.AirCargoAgentDocAddressControl.Name = "AirCargoAgentDocAddressControl";
			this.AirCargoAgentDocAddressControl.ReadOnly = false;
			this.AirCargoAgentDocAddressControl.SingleLineNoGroupBoxPanelWidth = 293;
			this.AirCargoAgentDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.AirCargoAgentDocAddressControl.TabIndex = 0;
			this.AirCargoAgentDocAddressControl.ValidationJustForced = false;
			// 
			// AirCargoAgentNACCSCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.AirCargoAgentNACCSCodeTextBox, "JobDeclaration.AirCargoAgentNACCSCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).JobDeclaration.AirCargoAgentNACCSCode)));
			this.AirCargoAgentNACCSCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 45, true);
			this.AirCargoAgentNACCSCodeTextBox.Name = "AirCargoAgentNACCSCodeTextBox";
			this.AirCargoAgentNACCSCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.AirCargoAgentNACCSCodeTextBox.TabIndex = 1;
			// 
			// AirCargoAgentLocationCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.AirCargoAgentLocationCodeTextBox, "JobDeclaration.AirCargoAgentLocationCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).JobDeclaration.AirCargoAgentLocationCode)));
			this.AirCargoAgentLocationCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 71, true);
			this.AirCargoAgentLocationCodeTextBox.Name = "AirCargoAgentLocationCodeTextBox";
			this.AirCargoAgentLocationCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.AirCargoAgentLocationCodeTextBox.TabIndex = 2;
			// 
			// ExternalBrokerGroupBox
			// 
			this.ExternalBrokerGroupBox.CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("b43dfc1d-c084-4418-97a9-77bb0b6e761e", "External Broker");
			this.ExternalBrokerGroupBox.Controls.Add(this.ExternalBrokerDocAddressControl);
			this.ExternalBrokerGroupBox.Controls.Add(this.ExternalBrokerCodeTextBox);
			this.ExternalBrokerGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(404, 88, true);
			this.ExternalBrokerGroupBox.Name = "ExternalBrokerGroupBox";
			this.ExternalBrokerGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(403, 74, true);
			this.ExternalBrokerGroupBox.TabIndex = 16;
			this.ExternalBrokerGroupBox.TabStop = false;
			// 
			// ExternalBrokerDocAddressControl
			// 
			this.ExternalBrokerDocAddressControl.AddressValidationProcessCmdKey = null;
			this.ExternalBrokerDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExternalBrokerDocAddressControl, "JobDeclaration.ExternalBrokerAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).JobDeclaration.ExternalBrokerAddress)));
			this.ExternalBrokerDocAddressControl.BindToOrganisations = "JobDeclaration.Lookups.InspectionWitnessOrganisations";
			this.ExternalBrokerDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
			this.ExternalBrokerDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 19, true);
			this.ExternalBrokerDocAddressControl.Name = "ExternalBrokerDocAddressControl";
			this.ExternalBrokerDocAddressControl.ReadOnly = false;
			this.ExternalBrokerDocAddressControl.SingleLineNoGroupBoxPanelWidth = 293;
			this.ExternalBrokerDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.ExternalBrokerDocAddressControl.TabIndex = 0;
			this.ExternalBrokerDocAddressControl.ValidationJustForced = false;
			// 
			// ExternalBrokerCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExternalBrokerCodeTextBox, "JobDeclaration.ExternalBrokerCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).JobDeclaration.ExternalBrokerCode)));
			this.ExternalBrokerCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 45, true);
			this.ExternalBrokerCodeTextBox.Name = "ExternalBrokerCodeTextBox";
			this.ExternalBrokerCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.ExternalBrokerCodeTextBox.TabIndex = 1;
			// 
			// PortOfLoadingCodeFindBox
			// 
			this.PortOfLoadingCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PortOfLoadingCodeFindBox, "JobDeclaration.JE_RL_NKPortOfLoading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).JobDeclaration.JE_RL_NKPortOfLoading)));
			this.PortOfLoadingCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PortOfLoadingCodeFindBox.Name = "PortOfLoadingCodeFindBox";
			this.PortOfLoadingCodeFindBox.ParentType = null;
			this.PortOfLoadingCodeFindBox.PreBoundMaxLength = 5;
			this.PortOfLoadingCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 20, true);
			this.PortOfLoadingCodeFindBox.TabIndex = 12;
			// 
			// PortOfLoadingTextBox
			// 
			this.BindingSource.SetBindingMember(this.PortOfLoadingTextBox, "JobDeclaration.PortOfLoadingIATACode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).JobDeclaration.PortOfLoadingIATACode)));
			this.PortOfLoadingTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 0, true);
			this.PortOfLoadingTextBox.Name = "PortOfLoadingTextBox";
			this.PortOfLoadingTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.PortOfLoadingTextBox.TabIndex = 13;
			// 
			// MAWBTextBox
			// 
			this.BindingSource.SetBindingMember(this.MAWBTextBox, "JobDeclaration.JE_MasterBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).JobDeclaration.JE_MasterBill)));
			this.MAWBTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(491, 10, true);
			this.MAWBTextBox.Name = "MAWBTextBox";
			this.MAWBTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 20, true);
			this.MAWBTextBox.TabIndex = 11;
			// 
			// PortOfLoadingPanel
			// 
			this.PortOfLoadingPanel.Controls.Add(this.PortOfLoadingCodeFindBox);
			this.PortOfLoadingPanel.Controls.Add(this.PortOfLoadingTextBox);
			this.PortOfLoadingPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(491, 36, true);
			this.PortOfLoadingPanel.Name = "PortOfLoadingPanel";
			this.PortOfLoadingPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 20, true);
			this.PortOfLoadingPanel.TabIndex = 20;
			// 
			// FinalDestinationPanel
			// 
			this.FinalDestinationPanel.Controls.Add(this.FinalDestinationCodeFindBox);
			this.FinalDestinationPanel.Controls.Add(this.FinalDestinationTextBox);
			this.FinalDestinationPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(491, 62, true);
			this.FinalDestinationPanel.Name = "FinalDestinationPanel";
			this.FinalDestinationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 20, true);
			this.FinalDestinationPanel.TabIndex = 21;
			// 
			// ForwarderDocAddressControl
			// 
			this.ForwarderDocAddressControl.AddressValidationProcessCmdKey = null;
			this.ForwarderDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ForwarderDocAddressControl, "JobDeclaration.ForwarderAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).JobDeclaration.ForwarderAddress)));
			this.ForwarderDocAddressControl.BindToOrganisations = "JobDeclaration.Lookups.InspectionWitnessOrganisations";
			this.ForwarderDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
			this.ForwarderDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 18, true);
			this.ForwarderDocAddressControl.Name = "ForwarderDocAddressControl";
			this.ForwarderDocAddressControl.ReadOnly = false;
			this.ForwarderDocAddressControl.SingleLineNoGroupBoxPanelWidth = 293;
			this.ForwarderDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.ForwarderDocAddressControl.TabIndex = 2;
			this.ForwarderDocAddressControl.ValidationJustForced = false;
			// 
			// CDB01EntryInstructionLayoutTemplate
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.FinalDestinationPanel);
			this.Controls.Add(this.PortOfLoadingPanel);
			this.Controls.Add(this.CarrierGroupBox);
			this.Controls.Add(this.ForwarderGroupBox);
			this.Controls.Add(this.AirCargoAgentGroupBox);
			this.Controls.Add(this.ExternalBrokerGroupBox);
			this.Controls.Add(this.MAWBTextBox);
			this.Controls.Add(this.CDB01BillNumberUserControl);
			this.Controls.Add(this.CDB01MoveInUserControl);
			this.Controls.Add(this.CDB01PermitNumberTextBox);
			this.Controls.Add(this.CDB01CargoTypeDropEdit);
			this.Controls.Add(this.DateForDutyDateEdit);
			this.Name = "CDB01EntryInstructionLayoutTemplate";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(869, 462, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DateForDutyDateEdit.ResumeLayout(true);
			this.DateForDutyDateEdit.PerformLayout();
			this.CDB01CargoTypeDropEdit.ResumeLayout(true);
			this.CDB01CargoTypeDropEdit.PerformLayout();
			this.CDB01MoveInUserControl.ResumeLayout(true);
			this.CDB01MoveInUserControl.PerformLayout();
			this.CDB01BillNumberUserControl.ResumeLayout(true);
			this.CDB01BillNumberUserControl.PerformLayout();
			this.FinalDestinationCodeFindBox.ResumeLayout(true);
			this.FinalDestinationCodeFindBox.PerformLayout();
			this.CarrierGroupBox.ResumeLayout(false);
			this.CarrierGroupBox.PerformLayout();
			this.CarrierCodeCodeFindBox.ResumeLayout(true);
			this.CarrierCodeCodeFindBox.PerformLayout();
			this.ShippingOrAirLineOrganisationGuidFindBox.ResumeLayout(true);
			this.ShippingOrAirLineOrganisationGuidFindBox.PerformLayout();
			this.ForwarderGroupBox.ResumeLayout(false);
			this.ForwarderGroupBox.PerformLayout();
			this.AirCargoAgentGroupBox.ResumeLayout(false);
			this.AirCargoAgentGroupBox.PerformLayout();
			this.AirCargoAgentDocAddressControl.ResumeLayout(true);
			this.AirCargoAgentDocAddressControl.PerformLayout();
			this.ExternalBrokerGroupBox.ResumeLayout(false);
			this.ExternalBrokerGroupBox.PerformLayout();
			this.ExternalBrokerDocAddressControl.ResumeLayout(true);
			this.ExternalBrokerDocAddressControl.PerformLayout();
			this.PortOfLoadingCodeFindBox.ResumeLayout(true);
			this.PortOfLoadingCodeFindBox.PerformLayout();
			this.PortOfLoadingPanel.ResumeLayout(false);
			this.PortOfLoadingPanel.PerformLayout();
			this.FinalDestinationPanel.ResumeLayout(false);
			this.FinalDestinationPanel.PerformLayout();
			this.ForwarderDocAddressControl.ResumeLayout(true);
			this.ForwarderDocAddressControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private ZArchitecture.GUI.ZDateEdit DateForDutyDateEdit;
		private ZArchitecture.GUI.ZDropEdit CDB01CargoTypeDropEdit;
		private ZArchitecture.ZTextBox CDB01PermitNumberTextBox;
		private CDB01MoveInUserControl CDB01MoveInUserControl;
		private CDB01BillNumberUserControl CDB01BillNumberUserControl;
		private ZArchitecture.GUI.ZCodeFindBox FinalDestinationCodeFindBox;
		private ZArchitecture.ZTextBox FinalDestinationTextBox;
		private ZArchitecture.GUI.ZGroupBox CarrierGroupBox;
		private ZArchitecture.GUI.ZCodeFindBox CarrierCodeCodeFindBox;
		internal ZArchitecture.GUI.ZGuidFindBox ShippingOrAirLineOrganisationGuidFindBox;
		private ZArchitecture.GUI.ZGroupBox ForwarderGroupBox;
		internal ZArchitecture.ZTextBox ForWarderNACCSCodeTextBox;
		private ZArchitecture.GUI.ZGroupBox AirCargoAgentGroupBox;
		internal MasterFiles.GUI.ZDocAddressControl AirCargoAgentDocAddressControl;
		internal ZArchitecture.ZTextBox AirCargoAgentNACCSCodeTextBox;
		internal ZArchitecture.ZTextBox AirCargoAgentLocationCodeTextBox;
		private ZArchitecture.GUI.ZGroupBox ExternalBrokerGroupBox;
		internal MasterFiles.GUI.ZDocAddressControl ExternalBrokerDocAddressControl;
		internal ZArchitecture.ZTextBox ExternalBrokerCodeTextBox;
		private ZArchitecture.GUI.ZCodeFindBox PortOfLoadingCodeFindBox;
		private ZArchitecture.ZTextBox PortOfLoadingTextBox;
		private ZArchitecture.ZTextBox MAWBTextBox;
		private ZArchitecture.GUI.ZPanel PortOfLoadingPanel;
		private ZArchitecture.GUI.ZPanel FinalDestinationPanel;
		internal MasterFiles.GUI.ZDocAddressControl ForwarderDocAddressControl;
	}
}
