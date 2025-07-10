using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	partial class OrganisationsUserControl
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
			this.DeclarationConsigneeAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.PowerOfAttorneyTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.InspectionWitnessDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.InspectionWitnessCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ExternalBrokerDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.ExternalBrokerCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AirCargoAgentDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.AirCargoAgentNACCSCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AirCargoAgentLocationCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ExternalBrokerGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.InspectionWitnessGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AirCargoAgentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CarrierGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CarrierCodeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ShippingOrAirLineOrganisationGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ForwarderGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ForwarderDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.ForwarderTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AttorneyForCustomsProcedureGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AttorneyForCustomsProcedureDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.DeclarationConsignorAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DeclarationConsigneeAddressControl.SuspendLayout();
			this.InspectionWitnessDocAddressControl.SuspendLayout();
			this.ExternalBrokerDocAddressControl.SuspendLayout();
			this.AirCargoAgentDocAddressControl.SuspendLayout();
			this.ExternalBrokerGroupBox.SuspendLayout();
			this.InspectionWitnessGroupBox.SuspendLayout();
			this.AirCargoAgentGroupBox.SuspendLayout();
			this.CarrierGroupBox.SuspendLayout();
			this.CarrierCodeCodeFindBox.SuspendLayout();
			this.ShippingOrAirLineOrganisationGuidFindBox.SuspendLayout();
			this.ForwarderGroupBox.SuspendLayout();
			this.ForwarderDocAddressControl.SuspendLayout();
			this.AttorneyForCustomsProcedureGroupBox.SuspendLayout();
			this.AttorneyForCustomsProcedureDocAddressControl.SuspendLayout();
			this.DeclarationConsignorAddressControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.JP.Business.JobDeclaration);
			// 
			// DeclarationConsigneeAddressControl
			// 
			this.DeclarationConsigneeAddressControl.AddressValidationProcessCmdKey = null;
			this.DeclarationConsigneeAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeclarationConsigneeAddressControl, "DeclarationConsigneeAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).DeclarationConsigneeAddress)));
			this.DeclarationConsigneeAddressControl.BindToOrganisations = "Lookups+ConsigneeList";
			this.DeclarationConsigneeAddressControl.CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("D9FEBC9B-4577-4047-8BF2-A5817BC11DA5", "Consignee Address");
			this.DeclarationConsigneeAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
			this.DeclarationConsigneeAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 338, true);
			this.DeclarationConsigneeAddressControl.Name = "DeclarationConsigneeAddressControl";
			this.DeclarationConsigneeAddressControl.ReadOnly = false;
			this.DeclarationConsigneeAddressControl.SingleLineNoGroupBoxPanelWidth = 320;
			this.DeclarationConsigneeAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.DeclarationConsigneeAddressControl.TabIndex = 1;
			this.DeclarationConsigneeAddressControl.ValidationJustForced = false;
			// 
			// PowerOfAttorneyTextBox
			// 
			this.BindingSource.SetBindingMember(this.PowerOfAttorneyTextBox, "JE_ACP_POA");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).JE_ACP_POA)));
			this.PowerOfAttorneyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 41, true);
			this.PowerOfAttorneyTextBox.Name = "PowerOfAttorneyTextBox";
			this.PowerOfAttorneyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.PowerOfAttorneyTextBox.TabIndex = 3;
			// 
			// InspectionWitnessDocAddressControl
			// 
			this.InspectionWitnessDocAddressControl.AddressValidationProcessCmdKey = null;
			this.InspectionWitnessDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InspectionWitnessDocAddressControl, "InspectionWitness");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).InspectionWitness)));
			this.InspectionWitnessDocAddressControl.BindToOrganisations = "Lookups.InspectionWitnessOrganisations";
			this.InspectionWitnessDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
			this.InspectionWitnessDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 16, true);
			this.InspectionWitnessDocAddressControl.Name = "InspectionWitnessDocAddressControl";
			this.InspectionWitnessDocAddressControl.ReadOnly = false;
			this.InspectionWitnessDocAddressControl.SingleLineNoGroupBoxPanelWidth = 293;
			this.InspectionWitnessDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.InspectionWitnessDocAddressControl.TabIndex = 0;
			this.InspectionWitnessDocAddressControl.ValidationJustForced = false;
			// 
			// InspectionWitnessCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.InspectionWitnessCodeTextBox, "InspectionWitnessCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).InspectionWitnessCode)));
			this.InspectionWitnessCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(201, 40, true);
			this.InspectionWitnessCodeTextBox.Name = "InspectionWitnessCodeTextBox";
			this.InspectionWitnessCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.InspectionWitnessCodeTextBox.TabIndex = 1;
			// 
			// ExternalBrokerDocAddressControl
			// 
			this.ExternalBrokerDocAddressControl.AddressValidationProcessCmdKey = null;
			this.ExternalBrokerDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExternalBrokerDocAddressControl, "ExternalBrokerAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).ExternalBrokerAddress)));
			this.ExternalBrokerDocAddressControl.BindToOrganisations = "Lookups.InspectionWitnessOrganisations";
			this.ExternalBrokerDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
			this.ExternalBrokerDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 14, true);
			this.ExternalBrokerDocAddressControl.Name = "ExternalBrokerDocAddressControl";
			this.ExternalBrokerDocAddressControl.ReadOnly = false;
			this.ExternalBrokerDocAddressControl.SingleLineNoGroupBoxPanelWidth = 293;
			this.ExternalBrokerDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.ExternalBrokerDocAddressControl.TabIndex = 0;
			this.ExternalBrokerDocAddressControl.ValidationJustForced = false;
			// 
			// ExternalBrokerCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExternalBrokerCodeTextBox, "ExternalBrokerCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).ExternalBrokerCode)));
			this.ExternalBrokerCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(201, 38, true);
			this.ExternalBrokerCodeTextBox.Name = "ExternalBrokerCodeTextBox";
			this.ExternalBrokerCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.ExternalBrokerCodeTextBox.TabIndex = 1;
			// 
			// AirCargoAgentDocAddressControl
			// 
			this.AirCargoAgentDocAddressControl.AddressValidationProcessCmdKey = null;
			this.AirCargoAgentDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AirCargoAgentDocAddressControl, "AirCargoAgent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).AirCargoAgent)));
			this.AirCargoAgentDocAddressControl.BindToOrganisations = "Lookups.InspectionWitnessOrganisations";
			this.AirCargoAgentDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
			this.AirCargoAgentDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 17, true);
			this.AirCargoAgentDocAddressControl.Name = "AirCargoAgentDocAddressControl";
			this.AirCargoAgentDocAddressControl.ReadOnly = false;
			this.AirCargoAgentDocAddressControl.SingleLineNoGroupBoxPanelWidth = 293;
			this.AirCargoAgentDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.AirCargoAgentDocAddressControl.TabIndex = 6;
			this.AirCargoAgentDocAddressControl.ValidationJustForced = false;
			// 
			// AirCargoAgentNACCSCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.AirCargoAgentNACCSCodeTextBox, "AirCargoAgentNACCSCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).AirCargoAgentNACCSCode)));
			this.AirCargoAgentNACCSCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(201, 41, true);
			this.AirCargoAgentNACCSCodeTextBox.Name = "AirCargoAgentNACCSCodeTextBox";
			this.AirCargoAgentNACCSCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.AirCargoAgentNACCSCodeTextBox.TabIndex = 7;
			// 
			// AirCargoAgentLocationCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.AirCargoAgentLocationCodeTextBox, "AirCargoAgentLocationCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).AirCargoAgentLocationCode)));
			this.AirCargoAgentLocationCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(201, 67, true);
			this.AirCargoAgentLocationCodeTextBox.Name = "AirCargoAgentLocationCodeTextBox";
			this.AirCargoAgentLocationCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.AirCargoAgentLocationCodeTextBox.TabIndex = 8;
			// 
			// ExternalBrokerGroupBox
			// 
			this.ExternalBrokerGroupBox.CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("5f5dd0bc-5411-42ed-ae9f-bb8c3370933b", "External Broker");
			this.ExternalBrokerGroupBox.Controls.Add(this.ExternalBrokerDocAddressControl);
			this.ExternalBrokerGroupBox.Controls.Add(this.ExternalBrokerCodeTextBox);
			this.ExternalBrokerGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 176, true);
			this.ExternalBrokerGroupBox.Name = "ExternalBrokerGroupBox";
			this.ExternalBrokerGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(422, 62, true);
			this.ExternalBrokerGroupBox.TabIndex = 5;
			this.ExternalBrokerGroupBox.TabStop = false;
			// 
			// InspectionWitnessGroupBox
			// 
			this.InspectionWitnessGroupBox.CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("ad2c9049-cff5-4644-96c2-76da39fedc3d", "Inspection Witness");
			this.InspectionWitnessGroupBox.Controls.Add(this.InspectionWitnessDocAddressControl);
			this.InspectionWitnessGroupBox.Controls.Add(this.InspectionWitnessCodeTextBox);
			this.InspectionWitnessGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 244, true);
			this.InspectionWitnessGroupBox.Name = "InspectionWitnessGroupBox";
			this.InspectionWitnessGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(422, 62, true);
			this.InspectionWitnessGroupBox.TabIndex = 10;
			this.InspectionWitnessGroupBox.TabStop = false;
			// 
			// AirCargoAgentGroupBox
			// 
			this.AirCargoAgentGroupBox.CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("71c7fbf3-f8c9-4209-8f3d-6d44f9fc0589", "Air Cargo Agent");
			this.AirCargoAgentGroupBox.Controls.Add(this.AirCargoAgentDocAddressControl);
			this.AirCargoAgentGroupBox.Controls.Add(this.AirCargoAgentLocationCodeTextBox);
			this.AirCargoAgentGroupBox.Controls.Add(this.AirCargoAgentNACCSCodeTextBox);
			this.AirCargoAgentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 450, true);
			this.AirCargoAgentGroupBox.Name = "AirCargoAgentGroupBox";
			this.AirCargoAgentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(422, 90, true);
			this.AirCargoAgentGroupBox.TabIndex = 11;
			this.AirCargoAgentGroupBox.TabStop = false;
			// 
			// CarrierGroupBox
			// 
			this.CarrierGroupBox.CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("a6ee8591-3d8c-4ce1-afd4-c9061beae494", "Carrier");
			this.CarrierGroupBox.Controls.Add(this.CarrierCodeCodeFindBox);
			this.CarrierGroupBox.Controls.Add(this.ShippingOrAirLineOrganisationGuidFindBox);
			this.CarrierGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 36, true);
			this.CarrierGroupBox.Name = "CarrierGroupBox";
			this.CarrierGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 66, true);
			this.CarrierGroupBox.TabIndex = 12;
			this.CarrierGroupBox.TabStop = false;
			// 
			// CarrierCodeCodeFindBox
			// 
			this.CarrierCodeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CarrierCodeCodeFindBox, "JE_CarrierCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).JE_CarrierCode)));
			this.CarrierCodeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 41, true);
			this.CarrierCodeCodeFindBox.Name = "CarrierCodeCodeFindBox";
			this.CarrierCodeCodeFindBox.ParentType = null;
			this.CarrierCodeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.CarrierCodeCodeFindBox.TabIndex = 3;
			// 
			// ShippingOrAirLineOrganisationGuidFindBox
			// 
			this.ShippingOrAirLineOrganisationGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShippingOrAirLineOrganisationGuidFindBox, "JE_OH_ShippingLine");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).JE_OH_ShippingLine)));
			this.ShippingOrAirLineOrganisationGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 15, true);
			this.ShippingOrAirLineOrganisationGuidFindBox.Name = "ShippingOrAirLineOrganisationGuidFindBox";
			this.ShippingOrAirLineOrganisationGuidFindBox.ParentType = null;
			this.ShippingOrAirLineOrganisationGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.ShippingOrAirLineOrganisationGuidFindBox.TabIndex = 2;
			// 
			// ForwarderGroupBox
			// 
			this.ForwarderGroupBox.CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("5f5dd0bc-5411-42ed-ae9f-bb8c3370933b", "Forwarder");
			this.ForwarderGroupBox.Controls.Add(this.ForwarderDocAddressControl);
			this.ForwarderGroupBox.Controls.Add(this.ForwarderTextBox);
			this.ForwarderGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 108, true);
			this.ForwarderGroupBox.Name = "ForwarderGroupBox";
			this.ForwarderGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(422, 62, true);
			this.ForwarderGroupBox.TabIndex = 13;
			this.ForwarderGroupBox.TabStop = false;
			// 
			// ForwarderDocAddressControl
			// 
			this.ForwarderDocAddressControl.AddressValidationProcessCmdKey = null;
			this.ForwarderDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ForwarderDocAddressControl, "ForwarderAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).ForwarderAddress)));
			this.ForwarderDocAddressControl.BindToOrganisations = "Lookups.InspectionWitnessOrganisations";
			this.ForwarderDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
			this.ForwarderDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 14, true);
			this.ForwarderDocAddressControl.Name = "ForwarderDocAddressControl";
			this.ForwarderDocAddressControl.ReadOnly = false;
			this.ForwarderDocAddressControl.SingleLineNoGroupBoxPanelWidth = 293;
			this.ForwarderDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.ForwarderDocAddressControl.TabIndex = 0;
			this.ForwarderDocAddressControl.ValidationJustForced = false;
			// 
			// ForwarderTextBox
			// 
			this.BindingSource.SetBindingMember(this.ForwarderTextBox, "ForwarderCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).ForwarderCode)));
			this.ForwarderTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(201, 38, true);
			this.ForwarderTextBox.Name = "ForwarderTextBox";
			this.ForwarderTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.ForwarderTextBox.TabIndex = 1;
			// 
			// AttorneyForCustomsProcedureGroupBox
			// 
			this.AttorneyForCustomsProcedureGroupBox.CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("ad2c9049-cff5-4644-96c2-76da39fedc3d", "Attorney for Customs Procedures (ACP)");
			this.AttorneyForCustomsProcedureGroupBox.Controls.Add(this.AttorneyForCustomsProcedureDocAddressControl);
			this.AttorneyForCustomsProcedureGroupBox.Controls.Add(this.PowerOfAttorneyTextBox);
			this.AttorneyForCustomsProcedureGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 382, true);
			this.AttorneyForCustomsProcedureGroupBox.Name = "AttorneyForCustomsProcedureGroupBox";
			this.AttorneyForCustomsProcedureGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(422, 62, true);
			this.AttorneyForCustomsProcedureGroupBox.TabIndex = 14;
			this.AttorneyForCustomsProcedureGroupBox.TabStop = false;
			// 
			// AttorneyForCustomsProcedureDocAddressControl
			// 
			this.AttorneyForCustomsProcedureDocAddressControl.AddressValidationProcessCmdKey = null;
			this.AttorneyForCustomsProcedureDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AttorneyForCustomsProcedureDocAddressControl, "AttorneyForCustomsProceduresAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).AttorneyForCustomsProceduresAddress)));
			this.AttorneyForCustomsProcedureDocAddressControl.BindToOrganisations = "Lookups.RepresentativeList";
			this.AttorneyForCustomsProcedureDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
			this.AttorneyForCustomsProcedureDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 16, true);
			this.AttorneyForCustomsProcedureDocAddressControl.Name = "AttorneyForCustomsProcedureDocAddressControl";
			this.AttorneyForCustomsProcedureDocAddressControl.ReadOnly = false;
			this.AttorneyForCustomsProcedureDocAddressControl.SingleLineNoGroupBoxPanelWidth = 293;
			this.AttorneyForCustomsProcedureDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.AttorneyForCustomsProcedureDocAddressControl.TabIndex = 0;
			this.AttorneyForCustomsProcedureDocAddressControl.ValidationJustForced = false;
			// 
			// DeclarationConsignorAddressControl
			// 
			this.DeclarationConsignorAddressControl.AddressValidationProcessCmdKey = null;
			this.DeclarationConsignorAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeclarationConsignorAddressControl, "DeclarationConsignorAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).DeclarationConsignorAddress)));
			this.DeclarationConsignorAddressControl.BindToOrganisations = "Lookups+ConsigneeList";
			this.DeclarationConsignorAddressControl.CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("FAF37994-60C9-45BE-8748-DED3973ECA4F", "Consignor Address");
			this.DeclarationConsignorAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
			this.DeclarationConsignorAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 312, true);
			this.DeclarationConsignorAddressControl.Name = "DeclarationConsignorAddressControl";
			this.DeclarationConsignorAddressControl.ReadOnly = false;
			this.DeclarationConsignorAddressControl.SingleLineNoGroupBoxPanelWidth = 320;
			this.DeclarationConsignorAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.DeclarationConsignorAddressControl.TabIndex = 15;
			this.DeclarationConsignorAddressControl.ValidationJustForced = false;
			// 
			// OrganisationsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DeclarationConsignorAddressControl);
			this.Controls.Add(this.AttorneyForCustomsProcedureGroupBox);
			this.Controls.Add(this.ForwarderGroupBox);
			this.Controls.Add(this.CarrierGroupBox);
			this.Controls.Add(this.AirCargoAgentGroupBox);
			this.Controls.Add(this.InspectionWitnessGroupBox);
			this.Controls.Add(this.ExternalBrokerGroupBox);
			this.Controls.Add(this.DeclarationConsigneeAddressControl);
			this.Name = "OrganisationsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(443, 590, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DeclarationConsigneeAddressControl.ResumeLayout(true);
			this.DeclarationConsigneeAddressControl.PerformLayout();
			this.InspectionWitnessDocAddressControl.ResumeLayout(true);
			this.InspectionWitnessDocAddressControl.PerformLayout();
			this.ExternalBrokerDocAddressControl.ResumeLayout(true);
			this.ExternalBrokerDocAddressControl.PerformLayout();
			this.AirCargoAgentDocAddressControl.ResumeLayout(true);
			this.AirCargoAgentDocAddressControl.PerformLayout();
			this.ExternalBrokerGroupBox.ResumeLayout(false);
			this.ExternalBrokerGroupBox.PerformLayout();
			this.InspectionWitnessGroupBox.ResumeLayout(false);
			this.InspectionWitnessGroupBox.PerformLayout();
			this.AirCargoAgentGroupBox.ResumeLayout(false);
			this.AirCargoAgentGroupBox.PerformLayout();
			this.CarrierGroupBox.ResumeLayout(false);
			this.CarrierGroupBox.PerformLayout();
			this.CarrierCodeCodeFindBox.ResumeLayout(true);
			this.CarrierCodeCodeFindBox.PerformLayout();
			this.ShippingOrAirLineOrganisationGuidFindBox.ResumeLayout(true);
			this.ShippingOrAirLineOrganisationGuidFindBox.PerformLayout();
			this.ForwarderGroupBox.ResumeLayout(false);
			this.ForwarderGroupBox.PerformLayout();
			this.ForwarderDocAddressControl.ResumeLayout(true);
			this.ForwarderDocAddressControl.PerformLayout();
			this.AttorneyForCustomsProcedureGroupBox.ResumeLayout(false);
			this.AttorneyForCustomsProcedureGroupBox.PerformLayout();
			this.AttorneyForCustomsProcedureDocAddressControl.ResumeLayout(true);
			this.AttorneyForCustomsProcedureDocAddressControl.PerformLayout();
			this.DeclarationConsignorAddressControl.ResumeLayout(true);
			this.DeclarationConsignorAddressControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal MasterFiles.GUI.ZDocAddressControl DeclarationConsigneeAddressControl;
		internal ZTextBox PowerOfAttorneyTextBox;
		internal ZDocAddressControl InspectionWitnessDocAddressControl;
		internal ZTextBox InspectionWitnessCodeTextBox;
		internal ZDocAddressControl ExternalBrokerDocAddressControl;
		internal ZTextBox ExternalBrokerCodeTextBox;
		internal ZDocAddressControl AirCargoAgentDocAddressControl;
		internal ZTextBox AirCargoAgentNACCSCodeTextBox;
		internal ZTextBox AirCargoAgentLocationCodeTextBox;
		private ZGroupBox ExternalBrokerGroupBox;
		private ZGroupBox InspectionWitnessGroupBox;
		private ZGroupBox AirCargoAgentGroupBox;
		private ZGroupBox CarrierGroupBox;
		private ZCodeFindBox CarrierCodeCodeFindBox;
		internal ZGuidFindBox ShippingOrAirLineOrganisationGuidFindBox;
		private ZGroupBox ForwarderGroupBox;
		internal ZDocAddressControl ForwarderDocAddressControl;
		internal ZTextBox ForwarderTextBox;
		private ZGroupBox AttorneyForCustomsProcedureGroupBox;
		internal ZDocAddressControl AttorneyForCustomsProcedureDocAddressControl;
		internal ZDocAddressControl DeclarationConsignorAddressControl;
	}
}
