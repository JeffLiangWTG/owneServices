namespace Enterprise.Customs.ASYCUDA.GUI
{
	partial class ExampleControlTemplate
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
			this.VoyageFlightTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MastersNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ManifestNumberFromMasterBillTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ContainerModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.VehicleRegistrationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.VesselCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.IssueDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.EstDepartureDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.EstArrivalDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.TransportModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PortOfLoadingCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.PortOfDischargeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CustomsDischargePortCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CustomsLoadPortCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.DischargeTerminalAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.DeconsolidateAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.CustomsOfficeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ManifestTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PortOfFirstArrivalCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ShippingAgentAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.CarrierCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NatureDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RadioCallSignTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MasterBOLTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AgentTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CarrierAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.ConveyanceCountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.BuyersConsolidationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ContainerModeDropEdit.SuspendLayout();
			this.VesselCodeFindBox.SuspendLayout();
			this.IssueDateDateEdit.SuspendLayout();
			this.EstDepartureDateEdit.SuspendLayout();
			this.EstArrivalDateEdit.SuspendLayout();
			this.TransportModeDropEdit.SuspendLayout();
			this.PortOfLoadingCodeFindBox.SuspendLayout();
			this.PortOfDischargeCodeFindBox.SuspendLayout();
			this.CustomsDischargePortCodeFindBox.SuspendLayout();
			this.CustomsLoadPortCodeFindBox.SuspendLayout();
			this.DischargeTerminalAddressControl.SuspendLayout();
			this.DeconsolidateAddressControl.SuspendLayout();
			this.CustomsOfficeDropEdit.SuspendLayout();
			this.ManifestTypeDropEdit.SuspendLayout();
			this.PortOfFirstArrivalCodeFindBox.SuspendLayout();
			this.ShippingAgentAddressControl.SuspendLayout();
			this.NatureDropEdit.SuspendLayout();
			this.AgentTypeDropEdit.SuspendLayout();
			this.CarrierAddressControl.SuspendLayout();
			this.ConveyanceCountryCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader);
			// 
			// VoyageFlightTextBox
			// 
			this.BindingSource.SetBindingMember(this.VoyageFlightTextBox, "AMA_Voyage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).AMA_Voyage)));
			this.VoyageFlightTextBox.CaptionResourceString = null;
			this.VoyageFlightTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 100, true);
			this.VoyageFlightTextBox.Name = "VoyageFlightTextBox";
			this.VoyageFlightTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.VoyageFlightTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.VoyageFlightTextBox.TabIndex = 7;
			// 
			// MastersNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.MastersNameTextBox, "AMA_MasterInformation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).AMA_MasterInformation)));
			this.MastersNameTextBox.CaptionResourceString = null;
			this.MastersNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(307, 127, true);
			this.MastersNameTextBox.Name = "MastersNameTextBox";
			this.MastersNameTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.MastersNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(126, 20, true);
			this.MastersNameTextBox.TabIndex = 11;
			// 
			// ManifestNumberFromMasterBillTextBox
			// 
			this.BindingSource.SetBindingMember(this.ManifestNumberFromMasterBillTextBox, "AMA_MasterBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).AMA_MasterBill)));
			this.ManifestNumberFromMasterBillTextBox.CaptionResourceString = null;
			this.ManifestNumberFromMasterBillTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 247, true);
			this.ManifestNumberFromMasterBillTextBox.Name = "ManifestNumberFromMasterBillTextBox";
			this.ManifestNumberFromMasterBillTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.ManifestNumberFromMasterBillTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.ManifestNumberFromMasterBillTextBox.TabIndex = 20;
			// 
			// ContainerModeDropEdit
			// 
			this.ContainerModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ContainerModeDropEdit, "AMA_ContainerMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).AMA_ContainerMode)));
			this.ContainerModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(329, 27, true);
			this.ContainerModeDropEdit.Name = "ContainerModeDropEdit";
			this.ContainerModeDropEdit.ShouldResizeByMaxLength = true;
			this.ContainerModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.ContainerModeDropEdit.TabIndex = 3;
			// 
			// VehicleRegistrationTextBox
			// 
			this.BindingSource.SetBindingMember(this.VehicleRegistrationTextBox, "AMA_VehicleRegistration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).AMA_VehicleRegistration)));
			this.VehicleRegistrationTextBox.CaptionResourceString = null;
			this.VehicleRegistrationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 75, true);
			this.VehicleRegistrationTextBox.Name = "VehicleRegistrationTextBox";
			this.VehicleRegistrationTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.VehicleRegistrationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.VehicleRegistrationTextBox.TabIndex = 7;
			// 
			// VesselCodeFindBox
			// 
			this.VesselCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VesselCodeFindBox, "AMA_VesselName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).AMA_VesselName)));
			this.VesselCodeFindBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.VesselCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(199, 75, true);
			this.VesselCodeFindBox.Name = "VesselCodeFindBox";
			this.VesselCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.VesselCodeFindBox.ParentType = null;
			this.VesselCodeFindBox.PreBoundMaxLength = 35;
			this.VesselCodeFindBox.ShowDescriptionBox = false;
			this.VesselCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.VesselCodeFindBox.TabIndex = 8;
			// 
			// IssueDateDateEdit
			// 
			this.IssueDateDateEdit.AllowDrop = true;
			this.IssueDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.IssueDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.IssueDateDateEdit, "AMA_MasterBillIssueDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).AMA_MasterBillIssueDate)));
			this.IssueDateDateEdit.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("194EA40B-DE67-4B9C-BBC9-DF6C25D54934", "Issue Date");
			this.IssueDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(348, 247, true);
			this.IssueDateDateEdit.Name = "IssueDateDateEdit";
			this.IssueDateDateEdit.TabIndex = 21;
			// 
			// EstDepartureDateEdit
			// 
			this.EstDepartureDateEdit.AllowDrop = true;
			this.EstDepartureDateEdit.AutoCompleteMonthThreshold = 1;
			this.EstDepartureDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.EstDepartureDateEdit, "AMA_E_DEP");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).AMA_E_DEP)));
			this.EstDepartureDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(348, 151, true);
			this.EstDepartureDateEdit.Name = "EstDepartureDateEdit";
			this.EstDepartureDateEdit.TabIndex = 14;
			// 
			// EstArrivalDateEdit
			// 
			this.EstArrivalDateEdit.AllowDrop = true;
			this.EstArrivalDateEdit.AutoCompleteMonthThreshold = 1;
			this.EstArrivalDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.EstArrivalDateEdit, "AMA_E_ARV");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).AMA_E_ARV)));
			this.EstArrivalDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.EstArrivalDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(348, 199, true);
			this.EstArrivalDateEdit.Name = "EstArrivalDateEdit";
			this.EstArrivalDateEdit.TabIndex = 17;
			// 
			// TransportModeDropEdit
			// 
			this.TransportModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportModeDropEdit, "AMA_TransportMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).AMA_TransportMode)));
			this.TransportModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 27, true);
			this.TransportModeDropEdit.Name = "TransportModeDropEdit";
			this.TransportModeDropEdit.PreBoundMaxLength = 3;
			this.TransportModeDropEdit.ShouldResizeByMaxLength = true;
			this.TransportModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.TransportModeDropEdit.TabIndex = 2;
			// 
			// PortOfLoadingCodeFindBox
			// 
			this.PortOfLoadingCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PortOfLoadingCodeFindBox, "AMA_RL_NKPortOfLoading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).AMA_RL_NKPortOfLoading)));
			this.PortOfLoadingCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 151, true);
			this.PortOfLoadingCodeFindBox.Name = "PortOfLoadingCodeFindBox";
			this.PortOfLoadingCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.PortOfLoadingCodeFindBox.ParentType = null;
			this.PortOfLoadingCodeFindBox.PreBoundMaxLength = 5;
			this.PortOfLoadingCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.PortOfLoadingCodeFindBox.TabIndex = 12;
			// 
			// PortOfDischargeCodeFindBox
			// 
			this.PortOfDischargeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PortOfDischargeCodeFindBox, "AMA_RL_NKPortOfDischarge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).AMA_RL_NKPortOfDischarge)));
			this.PortOfDischargeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 199, true);
			this.PortOfDischargeCodeFindBox.Name = "PortOfDischargeCodeFindBox";
			this.PortOfDischargeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.PortOfDischargeCodeFindBox.ParentType = null;
			this.PortOfDischargeCodeFindBox.PreBoundMaxLength = 5;
			this.PortOfDischargeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.PortOfDischargeCodeFindBox.TabIndex = 15;
			// 
			// CustomsDischargePortCodeFindBox
			// 
			this.CustomsDischargePortCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsDischargePortCodeFindBox, "AMA_CustomsDischargePort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).AMA_CustomsDischargePort)));
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.CustomsDischargePortCodeFindBox, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CustomsDischargePortCodeFindBox, false);
			this.CustomsDischargePortCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(193, 199, true);
			this.CustomsDischargePortCodeFindBox.Name = "CustomsDischargePortCodeFindBox";
			this.CustomsDischargePortCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CustomsDischargePortCodeFindBox.ParentType = null;
			this.CustomsDischargePortCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.CustomsDischargePortCodeFindBox.TabIndex = 16;
			// 
			// CustomsLoadPortCodeFindBox
			// 
			this.CustomsLoadPortCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsLoadPortCodeFindBox, "AMA_CustomsLoadPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).AMA_CustomsLoadPort)));
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.CustomsLoadPortCodeFindBox, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CustomsLoadPortCodeFindBox, false);
			this.CustomsLoadPortCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(193, 151, true);
			this.CustomsLoadPortCodeFindBox.Name = "CustomsLoadPortCodeFindBox";
			this.CustomsLoadPortCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CustomsLoadPortCodeFindBox.ParentType = null;
			this.CustomsLoadPortCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.CustomsLoadPortCodeFindBox.TabIndex = 13;
			// 
			// DischargeTerminalAddressControl
			// 
			this.DischargeTerminalAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DischargeTerminalAddressControl, "AMA_OA_DischargeTerminalAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).AMA_OA_DischargeTerminalAddress)));
			this.DischargeTerminalAddressControl.BindToOrgList = "Lookups.Organisations";
			this.DischargeTerminalAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(522, 319, true);
			this.DischargeTerminalAddressControl.Name = "DischargeTerminalAddressControl";
			this.DischargeTerminalAddressControl.PopupCaption = "";
			this.DischargeTerminalAddressControl.ReadOnly = false;
			this.DischargeTerminalAddressControl.ShowAddress = false;
			this.DischargeTerminalAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.DischargeTerminalAddressControl.TabIndex = 25;
			// 
			// DeconsolidateAddressControl
			// 
			this.DeconsolidateAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeconsolidateAddressControl, "AMA_OA_DeconsolidateAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).AMA_OA_DeconsolidateAddress)));
			this.DeconsolidateAddressControl.BindToOrgList = "Lookups.Organisations";
			this.DeconsolidateAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 319, true);
			this.DeconsolidateAddressControl.Name = "DeconsolidateAddressControl";
			this.DeconsolidateAddressControl.PopupCaption = "";
			this.DeconsolidateAddressControl.ReadOnly = false;
			this.DeconsolidateAddressControl.ShowAddress = false;
			this.DeconsolidateAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.DeconsolidateAddressControl.TabIndex = 24;
			// 
			// CustomsOfficeDropEdit
			// 
			this.CustomsOfficeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsOfficeDropEdit, "AMA_CustomsOffice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).AMA_CustomsOffice)));
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.CustomsOfficeDropEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
			this.CustomsOfficeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 271, true);
			this.CustomsOfficeDropEdit.Name = "CustomsOfficeDropEdit";
			this.CustomsOfficeDropEdit.ShouldResizeByMaxLength = true;
			this.CustomsOfficeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 20, true);
			this.CustomsOfficeDropEdit.TabIndex = 22;
			// 
			// ManifestTypeDropEdit
			// 
			this.ManifestTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ManifestTypeDropEdit, "AMA_ManifestType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).AMA_ManifestType)));
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.ManifestTypeDropEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
			this.ManifestTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 3, true);
			this.ManifestTypeDropEdit.Name = "ManifestTypeDropEdit";
			this.ManifestTypeDropEdit.ShouldResizeByMaxLength = true;
			this.ManifestTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(217, 20, true);
			this.ManifestTypeDropEdit.TabIndex = 0;
			// 
			// PortOfFirstArrivalCodeFindBox
			// 
			this.PortOfFirstArrivalCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PortOfFirstArrivalCodeFindBox, "AMA_RL_NKPortOfFirstArrival");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).AMA_RL_NKPortOfFirstArrival)));
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.PortOfFirstArrivalCodeFindBox, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
			this.PortOfFirstArrivalCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 175, true);
			this.PortOfFirstArrivalCodeFindBox.Name = "PortOfFirstArrivalCodeFindBox";
			this.PortOfFirstArrivalCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.PortOfFirstArrivalCodeFindBox.ParentType = null;
			this.PortOfFirstArrivalCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 20, true);
			this.PortOfFirstArrivalCodeFindBox.TabIndex = 15;
			// 
			// ShippingAgentAddressControl
			// 
			this.ShippingAgentAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShippingAgentAddressControl, "AMA_OA_ShippingAgent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).AMA_OA_ShippingAgent)));
			this.ShippingAgentAddressControl.BindToOrgList = "Lookups.Organisations";
			this.ShippingAgentAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 295, true);
			this.ShippingAgentAddressControl.Name = "ShippingAgentAddressControl";
			this.ShippingAgentAddressControl.PopupCaption = "";
			this.ShippingAgentAddressControl.ReadOnly = false;
			this.ShippingAgentAddressControl.ShowAddress = false;
			this.ShippingAgentAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.ShippingAgentAddressControl.TabIndex = 23;
			// 
			// CarrierCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.CarrierCodeTextBox, "AMA_CarrierCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).AMA_CarrierCode)));
			this.CarrierCodeTextBox.CaptionResourceString = null;
			this.CarrierCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(522, 223, true);
			this.CarrierCodeTextBox.Name = "CarrierCodeTextBox";
			this.CarrierCodeTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.CarrierCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(162, 20, true);
			this.CarrierCodeTextBox.TabIndex = 19;
			// 
			// NatureDropEdit
			// 
			this.NatureDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NatureDropEdit, "AMA_Nature");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).AMA_Nature)));
			this.NatureDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(522, 3, true);
			this.NatureDropEdit.Name = "NatureDropEdit";
			this.NatureDropEdit.ShouldResizeByMaxLength = true;
			this.NatureDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(162, 20, true);
			this.NatureDropEdit.TabIndex = 1;
			// 
			// RadioCallSignTextBox
			// 
			this.BindingSource.SetBindingMember(this.RadioCallSignTextBox, "AMA_RadioCallSign");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).AMA_RadioCallSign)));
			this.RadioCallSignTextBox.CaptionResourceString = null;
			this.RadioCallSignTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(522, 75, true);
			this.RadioCallSignTextBox.Name = "RadioCallSignTextBox";
			this.RadioCallSignTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.RadioCallSignTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.RadioCallSignTextBox.TabIndex = 9;
			// 
			// MasterBOLTextBox
			// 
			this.BindingSource.SetBindingMember(this.MasterBOLTextBox, "MasterBOL");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).MasterBOL)));
			this.MasterBOLTextBox.CaptionResourceString = null;
			this.MasterBOLTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(228, 51, true);
			this.MasterBOLTextBox.Name = "MasterBOLTextBox";
			this.MasterBOLTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.MasterBOLTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(123, 20, true);
			this.MasterBOLTextBox.TabIndex = 5;
			// 
			// AgentTypeDropEdit
			// 
			this.AgentTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AgentTypeDropEdit, "AMA_AgentType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).AMA_AgentType)));
			this.AgentTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 51, true);
			this.AgentTypeDropEdit.Name = "AgentTypeDropEdit";
			this.AgentTypeDropEdit.PreBoundMaxLength = 3;
			this.AgentTypeDropEdit.ShouldResizeByMaxLength = true;
			this.AgentTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.AgentTypeDropEdit.TabIndex = 4;
			// 
			// CarrierAddressControl
			// 
			this.CarrierAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CarrierAddressControl, "AMA_OA_Carrier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).AMA_OA_Carrier)));
			this.CarrierAddressControl.BindToOrgList = "Lookups+CarrierList";
			this.LabelCaptionRenderProvider.SetLabelTop(this.CarrierAddressControl, 0);
			this.CarrierAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 223, true);
			this.CarrierAddressControl.Name = "CarrierAddressControl";
			this.CarrierAddressControl.PopupCaption = "";
			this.CarrierAddressControl.ReadOnly = false;
			this.CarrierAddressControl.ShowAddress = false;
			this.CarrierAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.CarrierAddressControl.TabIndex = 18;
			// 
			// ConveyanceCountryCodeFindBox
			// 
			this.ConveyanceCountryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConveyanceCountryCodeFindBox, "AMA_RN_NKConveyanceNationality");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).AMA_RN_NKConveyanceNationality)));
			this.ConveyanceCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 127, true);
			this.ConveyanceCountryCodeFindBox.Name = "ConveyanceCountryCodeFindBox";
			this.ConveyanceCountryCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ConveyanceCountryCodeFindBox.ParentType = null;
			this.ConveyanceCountryCodeFindBox.PreBoundMaxLength = 2;
			this.ConveyanceCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.ConveyanceCountryCodeFindBox.TabIndex = 10;
			// 
			// BuyersConsolidationCheckBox
			// 
			this.BindingSource.SetBindingMember(this.BuyersConsolidationCheckBox, "AMA_IsBuyersConsolidation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).AMA_IsBuyersConsolidation)));
			this.BuyersConsolidationCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.BuyersConsolidationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(354, 48, true);
			this.BuyersConsolidationCheckBox.Name = "BuyersConsolidationCheckBox";
			this.BuyersConsolidationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 17, true);
			this.BuyersConsolidationCheckBox.TabIndex = 6;
			// 
			// ExampleControlTemplate
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.EstArrivalDateEdit);
			this.Controls.Add(this.CustomsDischargePortCodeFindBox);
			this.Controls.Add(this.CustomsLoadPortCodeFindBox);
			this.Controls.Add(this.DischargeTerminalAddressControl);
			this.Controls.Add(this.DeconsolidateAddressControl);
			this.Controls.Add(this.CustomsOfficeDropEdit);
			this.Controls.Add(this.ManifestTypeDropEdit);
			this.Controls.Add(this.PortOfFirstArrivalCodeFindBox);
			this.Controls.Add(this.ShippingAgentAddressControl);
			this.Controls.Add(this.CarrierCodeTextBox);
			this.Controls.Add(this.NatureDropEdit);
			this.Controls.Add(this.RadioCallSignTextBox);
			this.Controls.Add(this.MasterBOLTextBox);
			this.Controls.Add(this.AgentTypeDropEdit);
			this.Controls.Add(this.CarrierAddressControl);
			this.Controls.Add(this.ConveyanceCountryCodeFindBox);
			this.Controls.Add(this.TransportModeDropEdit);
			this.Controls.Add(this.PortOfDischargeCodeFindBox);
			this.Controls.Add(this.VoyageFlightTextBox);
			this.Controls.Add(this.PortOfLoadingCodeFindBox);
			this.Controls.Add(this.ManifestNumberFromMasterBillTextBox);
			this.Controls.Add(this.MastersNameTextBox);
			this.Controls.Add(this.EstDepartureDateEdit);
			this.Controls.Add(this.IssueDateDateEdit);
			this.Controls.Add(this.ContainerModeDropEdit);
			this.Controls.Add(this.BuyersConsolidationCheckBox);
			this.Controls.Add(this.VehicleRegistrationTextBox);
			this.Controls.Add(this.VesselCodeFindBox);
			this.Name = "ExampleControlTemplate";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(917, 410, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ContainerModeDropEdit.ResumeLayout(true);
			this.ContainerModeDropEdit.PerformLayout();
			this.VesselCodeFindBox.ResumeLayout(true);
			this.VesselCodeFindBox.PerformLayout();
			this.IssueDateDateEdit.ResumeLayout(true);
			this.IssueDateDateEdit.PerformLayout();
			this.EstDepartureDateEdit.ResumeLayout(true);
			this.EstDepartureDateEdit.PerformLayout();
			this.EstArrivalDateEdit.ResumeLayout(true);
			this.EstArrivalDateEdit.PerformLayout();
			this.TransportModeDropEdit.ResumeLayout(true);
			this.TransportModeDropEdit.PerformLayout();
			this.PortOfLoadingCodeFindBox.ResumeLayout(true);
			this.PortOfLoadingCodeFindBox.PerformLayout();
			this.PortOfDischargeCodeFindBox.ResumeLayout(true);
			this.PortOfDischargeCodeFindBox.PerformLayout();
			this.CustomsDischargePortCodeFindBox.ResumeLayout(true);
			this.CustomsDischargePortCodeFindBox.PerformLayout();
			this.CustomsLoadPortCodeFindBox.ResumeLayout(true);
			this.CustomsLoadPortCodeFindBox.PerformLayout();
			this.DischargeTerminalAddressControl.ResumeLayout(true);
			this.DischargeTerminalAddressControl.PerformLayout();
			this.DeconsolidateAddressControl.ResumeLayout(true);
			this.DeconsolidateAddressControl.PerformLayout();
			this.CustomsOfficeDropEdit.ResumeLayout(true);
			this.CustomsOfficeDropEdit.PerformLayout();
			this.ManifestTypeDropEdit.ResumeLayout(true);
			this.ManifestTypeDropEdit.PerformLayout();
			this.PortOfFirstArrivalCodeFindBox.ResumeLayout(true);
			this.PortOfFirstArrivalCodeFindBox.PerformLayout();
			this.ShippingAgentAddressControl.ResumeLayout(true);
			this.ShippingAgentAddressControl.PerformLayout();
			this.NatureDropEdit.ResumeLayout(true);
			this.NatureDropEdit.PerformLayout();
			this.AgentTypeDropEdit.ResumeLayout(true);
			this.AgentTypeDropEdit.PerformLayout();
			this.CarrierAddressControl.ResumeLayout(true);
			this.CarrierAddressControl.PerformLayout();
			this.ConveyanceCountryCodeFindBox.ResumeLayout(true);
			this.ConveyanceCountryCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox ConveyanceCountryCodeFindBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit AgentTypeDropEdit;
		internal Enterprise.ZArchitecture.ZTextBox VoyageFlightTextBox;
		internal Enterprise.ZArchitecture.ZTextBox MastersNameTextBox;
		internal Enterprise.ZArchitecture.ZTextBox VehicleRegistrationTextBox;
		internal Enterprise.ZArchitecture.ZTextBox ManifestNumberFromMasterBillTextBox;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox VesselCodeFindBox;
		internal Enterprise.ZArchitecture.GUI.ZDateEdit IssueDateDateEdit;
		internal Enterprise.ZArchitecture.GUI.ZDateEdit EstDepartureDateEdit;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit TransportModeDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit ContainerModeDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox PortOfLoadingCodeFindBox;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox PortOfDischargeCodeFindBox;
		internal Enterprise.ZArchitecture.GUI.ZCheckBox BuyersConsolidationCheckBox;
		internal Enterprise.ZArchitecture.GUI.ZAddressControl CarrierAddressControl;
		internal Enterprise.ZArchitecture.ZTextBox MasterBOLTextBox;
		internal Enterprise.ZArchitecture.ZTextBox RadioCallSignTextBox;
		internal Enterprise.ZArchitecture.GUI.ZAddressControl DischargeTerminalAddressControl;
		internal Enterprise.ZArchitecture.GUI.ZAddressControl DeconsolidateAddressControl;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit CustomsOfficeDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit ManifestTypeDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox PortOfFirstArrivalCodeFindBox;
		internal Enterprise.ZArchitecture.GUI.ZAddressControl ShippingAgentAddressControl;
		internal Enterprise.ZArchitecture.ZTextBox CarrierCodeTextBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit NatureDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZDateEdit EstArrivalDateEdit;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox CustomsLoadPortCodeFindBox;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox CustomsDischargePortCodeFindBox;
	}
}
