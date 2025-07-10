namespace Enterprise.Customs.AU.ExportManifest.GUI
{
	partial class BaseHeaderDetails
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.RecalculateTotalsButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CTOFindBox = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.ContingencyCANTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ContingencyCANLabel = new Enterprise.ZArchitecture.ZLabel();
			this.VesselCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CTOLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PortOfDestinationCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.PortOfDestinationLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TotalContainerCountLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TotalPackageCountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.VoyageFlightNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.VesselIDMasterAirWaybillLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PortOfDepartureCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CountryOfDestainationLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ModeOfTransportLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TotalContainerCountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DateOfDepartureLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CANTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DateOfDepartureDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.VoyageFlightNoLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TotalEmptyContainerCountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CountryOfDestinationCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.TotalEmptyContainerCountLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ModeOfTransportDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PortOfDepartureLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TotalPackageCountLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CANLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PackDepotDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.LloydsIMOTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LloydsLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CTOFindBox.SuspendLayout();
			this.VesselCodeFindBox.SuspendLayout();
			this.PortOfDestinationCodeFindBox.SuspendLayout();
			this.PortOfDepartureCodeFindBox.SuspendLayout();
			this.DateOfDepartureDateEdit.SuspendLayout();
			this.CountryOfDestinationCodeFindBox.SuspendLayout();
			this.ModeOfTransportDropEdit.SuspendLayout();
			this.PackDepotDocAddressControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestHeader);
			// 
			// RecalculateTotalsButton
			// 
			this.RecalculateTotalsButton.IsCaptionOverridden = true;
			this.RecalculateTotalsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(411, 82, true);
			this.RecalculateTotalsButton.Name = "RecalculateTotalsButton";
			this.RecalculateTotalsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 23, true);
			this.RecalculateTotalsButton.TabIndex = 7;
			this.RecalculateTotalsButton.Text = "Calc. Totals";
			this.RecalculateTotalsButton.ToolTipCaption = null;
			this.RecalculateTotalsButton.UseVisualStyleBackColor = true;
			this.RecalculateTotalsButton.Click += new System.EventHandler(this.RecalculateTotalsButton_Click);
			// 
			// CTOFindBox
			// 
			this.CTOFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CTOFindBox, "ED_OA_CTOAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestHeader)(null)).ED_OA_CTOAddress)));
			this.CTOFindBox.BindToOrgList = "Lookups+OrgHeaders";
			this.CTOFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 84, true);
			this.CTOFindBox.Name = "CTOFindBox";
			this.CTOFindBox.PopupCaption = "";
			this.CTOFindBox.ShowAddress = false;
			this.CTOFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(377, 17, true);
			this.CTOFindBox.TabIndex = 3;
			// 
			// ContingencyCANTextBox
			// 
			this.BindingSource.SetBindingMember(this.ContingencyCANTextBox, "ED_CCAN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestHeader)(null)).ED_CCAN)));
			this.ContingencyCANTextBox.CaptionResourceString = null;
			this.ContingencyCANTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(627, 161, true);
			this.ContingencyCANTextBox.Name = "ContingencyCANTextBox";
			this.ContingencyCANTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(172, 17, true);
			this.ContingencyCANTextBox.TabIndex = 14;
			// 
			// ContingencyCANLabel
			// 
			this.ContingencyCANLabel.AutoSize = true;
			this.ContingencyCANLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ContingencyCANLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(499, 161, true);
			this.ContingencyCANLabel.Name = "ContingencyCANLabel";
			this.ContingencyCANLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(93, 13, true);
			this.ContingencyCANLabel.TabIndex = 70;
			this.ContingencyCANLabel.Text = "Contingency CRN:";
			// 
			// VesselCodeFindBox
			// 
			this.VesselCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VesselCodeFindBox, "ED_VesselName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestHeader)(null)).ED_VesselName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestHeader)(null)).Lookups.Vessels)));
			this.VesselCodeFindBox.BindToList = "Lookups.Vessels";
			this.VesselCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(627, 6, true);
			this.VesselCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefVessel;
			this.VesselCodeFindBox.Name = "VesselCodeFindBox";
			this.VesselCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.VesselCodeFindBox.ParentType = null;
			this.VesselCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(172, 17, true);
			this.VesselCodeFindBox.TabIndex = 8;
			// 
			// CTOLabel
			// 
			this.CTOLabel.AutoSize = true;
			this.CTOLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CTOLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 87, true);
			this.CTOLabel.Name = "CTOLabel";
			this.CTOLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 13, true);
			this.CTOLabel.TabIndex = 69;
			this.CTOLabel.Text = "CTO:";
			// 
			// PortOfDestinationCodeFindBox
			// 
			this.PortOfDestinationCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PortOfDestinationCodeFindBox, "ED_RL_NKPortOfDestination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestHeader)(null)).ED_RL_NKPortOfDestination)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestHeader)(null)).Lookups.PortOfDestinations)));
			this.PortOfDestinationCodeFindBox.BindToList = "Lookups.PortOfDestinations";
			this.PortOfDestinationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(627, 84, true);
			this.PortOfDestinationCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.PortOfDestinationCodeFindBox.Name = "PortOfDestinationCodeFindBox";
			this.PortOfDestinationCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.PortOfDestinationCodeFindBox.ParentType = null;
			this.PortOfDestinationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(172, 17, true);
			this.PortOfDestinationCodeFindBox.TabIndex = 15;
			// 
			// PortOfDestinationLabel
			// 
			this.PortOfDestinationLabel.AutoSize = true;
			this.PortOfDestinationLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.PortOfDestinationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(499, 87, true);
			this.PortOfDestinationLabel.Name = "PortOfDestinationLabel";
			this.PortOfDestinationLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 13, true);
			this.PortOfDestinationLabel.TabIndex = 68;
			this.PortOfDestinationLabel.Text = "Port of Destination:";
			// 
			// TotalContainerCountLabel
			// 
			this.TotalContainerCountLabel.AutoSize = true;
			this.TotalContainerCountLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.TotalContainerCountLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(259, 36, true);
			this.TotalContainerCountLabel.Name = "TotalContainerCountLabel";
			this.TotalContainerCountLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(131, 13, true);
			this.TotalContainerCountLabel.TabIndex = 60;
			this.TotalContainerCountLabel.Text = "Total Full Container Count:";
			// 
			// TotalPackageCountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalPackageCountCalcEdit, "ED_NoOfPacks");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestHeader)(null)).ED_NoOfPacks)));
			this.TotalPackageCountCalcEdit.CaptionResourceString = null;
			this.TotalPackageCountCalcEdit.DecimalPlaces = 2;
			this.TotalPackageCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(411, 6, true);
			this.TotalPackageCountCalcEdit.Name = "TotalPackageCountCalcEdit";
			this.TotalPackageCountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 17, true);
			this.TotalPackageCountCalcEdit.TabIndex = 4;
			this.TotalPackageCountCalcEdit.Text = "0";
			this.TotalPackageCountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// VoyageFlightNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.VoyageFlightNoTextBox, "ED_VoyageNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestHeader)(null)).ED_VoyageNumber)));
			this.VoyageFlightNoTextBox.CaptionResourceString = null;
			this.VoyageFlightNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(627, 58, true);
			this.VoyageFlightNoTextBox.Name = "VoyageFlightNoTextBox";
			this.VoyageFlightNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(172, 17, true);
			this.VoyageFlightNoTextBox.TabIndex = 10;
			// 
			// VesselIDMasterAirWaybillLabel
			// 
			this.VesselIDMasterAirWaybillLabel.AutoSize = true;
			this.VesselIDMasterAirWaybillLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.VesselIDMasterAirWaybillLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(499, 9, true);
			this.VesselIDMasterAirWaybillLabel.Name = "VesselIDMasterAirWaybillLabel";
			this.VesselIDMasterAirWaybillLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 13, true);
			this.VesselIDMasterAirWaybillLabel.TabIndex = 59;
			this.VesselIDMasterAirWaybillLabel.Text = "Vessel:";
			// 
			// PortOfDepartureCodeFindBox
			// 
			this.PortOfDepartureCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PortOfDepartureCodeFindBox, "ED_RL_NKPortOfDeparture");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestHeader)(null)).ED_RL_NKPortOfDeparture)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestHeader)(null)).Lookups.PortOfDepartures)));
			this.PortOfDepartureCodeFindBox.BindToList = "Lookups.PortOfDepartures";
			this.PortOfDepartureCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(627, 109, true);
			this.PortOfDepartureCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.PortOfDepartureCodeFindBox.Name = "PortOfDepartureCodeFindBox";
			this.PortOfDepartureCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.PortOfDepartureCodeFindBox.ParentType = null;
			this.PortOfDepartureCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(172, 17, true);
			this.PortOfDepartureCodeFindBox.TabIndex = 16;
			// 
			// CountryOfDestainationLabel
			// 
			this.CountryOfDestainationLabel.AutoSize = true;
			this.CountryOfDestainationLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CountryOfDestainationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(499, 139, true);
			this.CountryOfDestainationLabel.Name = "CountryOfDestainationLabel";
			this.CountryOfDestainationLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 13, true);
			this.CountryOfDestainationLabel.TabIndex = 51;
			this.CountryOfDestainationLabel.Text = "Ctry/Rgn. of Destination:";
			// 
			// ModeOfTransportLabel
			// 
			this.ModeOfTransportLabel.AutoSize = true;
			this.ModeOfTransportLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ModeOfTransportLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 36, true);
			this.ModeOfTransportLabel.Name = "ModeOfTransportLabel";
			this.ModeOfTransportLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 13, true);
			this.ModeOfTransportLabel.TabIndex = 47;
			this.ModeOfTransportLabel.Text = "Mode of Transport:";
			// 
			// TotalContainerCountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalContainerCountCalcEdit, "ED_NoOfContainer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestHeader)(null)).ED_NoOfContainer)));
			this.TotalContainerCountCalcEdit.CaptionResourceString = null;
			this.TotalContainerCountCalcEdit.DecimalPlaces = 2;
			this.TotalContainerCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(411, 32, true);
			this.TotalContainerCountCalcEdit.Name = "TotalContainerCountCalcEdit";
			this.TotalContainerCountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 17, true);
			this.TotalContainerCountCalcEdit.TabIndex = 5;
			this.TotalContainerCountCalcEdit.Text = "0";
			this.TotalContainerCountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DateOfDepartureLabel
			// 
			this.DateOfDepartureLabel.AutoSize = true;
			this.DateOfDepartureLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.DateOfDepartureLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 61, true);
			this.DateOfDepartureLabel.Name = "DateOfDepartureLabel";
			this.DateOfDepartureLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 13, true);
			this.DateOfDepartureLabel.TabIndex = 54;
			this.DateOfDepartureLabel.Text = "Date of Departure:";
			// 
			// CANTextBox
			// 
			this.BindingSource.SetBindingMember(this.CANTextBox, "ED_CAN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestHeader)(null)).ED_CAN)));
			this.CANTextBox.CaptionResourceString = null;
			this.CANTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 6, true);
			this.CANTextBox.Name = "CANTextBox";
			this.CANTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 17, true);
			this.CANTextBox.TabIndex = 0;
			// 
			// DateOfDepartureDateEdit
			// 
			this.DateOfDepartureDateEdit.AllowDrop = true;
			this.DateOfDepartureDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.DateOfDepartureDateEdit, "ED_DepartureDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestHeader)(null)).ED_DepartureDate)));
			this.DateOfDepartureDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.DateOfDepartureDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 58, true);
			this.DateOfDepartureDateEdit.Name = "DateOfDepartureDateEdit";
			this.DateOfDepartureDateEdit.TabIndex = 2;
			// 
			// VoyageFlightNoLabel
			// 
			this.VoyageFlightNoLabel.AutoSize = true;
			this.VoyageFlightNoLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.VoyageFlightNoLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(499, 61, true);
			this.VoyageFlightNoLabel.Name = "VoyageFlightNoLabel";
			this.VoyageFlightNoLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(61, 13, true);
			this.VoyageFlightNoLabel.TabIndex = 57;
			this.VoyageFlightNoLabel.Text = "Voyage No:";
			// 
			// TotalEmptyContainerCountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalEmptyContainerCountCalcEdit, "ED_NoOfEmptyContainers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestHeader)(null)).ED_NoOfEmptyContainers)));
			this.TotalEmptyContainerCountCalcEdit.CaptionResourceString = null;
			this.TotalEmptyContainerCountCalcEdit.DecimalPlaces = 2;
			this.TotalEmptyContainerCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(411, 58, true);
			this.TotalEmptyContainerCountCalcEdit.Name = "TotalEmptyContainerCountCalcEdit";
			this.TotalEmptyContainerCountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 17, true);
			this.TotalEmptyContainerCountCalcEdit.TabIndex = 6;
			this.TotalEmptyContainerCountCalcEdit.Text = "0";
			this.TotalEmptyContainerCountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CountryOfDestinationCodeFindBox
			// 
			this.CountryOfDestinationCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryOfDestinationCodeFindBox, "ED_RN_NKCountryOfDestination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestHeader)(null)).ED_RN_NKCountryOfDestination)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestHeader)(null)).Lookups.CountryOfDestinations)));
			this.CountryOfDestinationCodeFindBox.BindToList = "Lookups.CountryOfDestinations";
			this.CountryOfDestinationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(627, 135, true);
			this.CountryOfDestinationCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			this.CountryOfDestinationCodeFindBox.Name = "CountryOfDestinationCodeFindBox";
			this.CountryOfDestinationCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CountryOfDestinationCodeFindBox.ParentType = null;
			this.CountryOfDestinationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(172, 17, true);
			this.CountryOfDestinationCodeFindBox.TabIndex = 17;
			// 
			// TotalEmptyContainerCountLabel
			// 
			this.TotalEmptyContainerCountLabel.AutoSize = true;
			this.TotalEmptyContainerCountLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.TotalEmptyContainerCountLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(259, 61, true);
			this.TotalEmptyContainerCountLabel.Name = "TotalEmptyContainerCountLabel";
			this.TotalEmptyContainerCountLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 13, true);
			this.TotalEmptyContainerCountLabel.TabIndex = 62;
			this.TotalEmptyContainerCountLabel.Text = "Total Empty Container Count:";
			// 
			// ModeOfTransportDropEdit
			// 
			this.ModeOfTransportDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ModeOfTransportDropEdit, "ED_TransportMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestHeader)(null)).ED_TransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestHeader)(null)).Lookups.ModeOfTransportList)));
			this.ModeOfTransportDropEdit.BindToList = "Lookups.ModeOfTransportList";
			this.ModeOfTransportDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 32, true);
			this.ModeOfTransportDropEdit.Name = "ModeOfTransportDropEdit";
			this.ModeOfTransportDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 17, true);
			this.ModeOfTransportDropEdit.TabIndex = 1;
			// 
			// PortOfDepartureLabel
			// 
			this.PortOfDepartureLabel.AutoSize = true;
			this.PortOfDepartureLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.PortOfDepartureLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(499, 111, true);
			this.PortOfDepartureLabel.Name = "PortOfDepartureLabel";
			this.PortOfDepartureLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(91, 13, true);
			this.PortOfDepartureLabel.TabIndex = 49;
			this.PortOfDepartureLabel.Text = "Port of Departure:";
			// 
			// TotalPackageCountLabel
			// 
			this.TotalPackageCountLabel.AutoSize = true;
			this.TotalPackageCountLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.TotalPackageCountLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(259, 9, true);
			this.TotalPackageCountLabel.Name = "TotalPackageCountLabel";
			this.TotalPackageCountLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(107, 13, true);
			this.TotalPackageCountLabel.TabIndex = 64;
			this.TotalPackageCountLabel.Text = "Total Package Count:";
			// 
			// CANLabel
			// 
			this.CANLabel.AutoSize = true;
			this.CANLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CANLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 9, true);
			this.CANLabel.Name = "CANLabel";
			this.CANLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(31, 13, true);
			this.CANLabel.TabIndex = 66;
			this.CANLabel.Text = "CAN:";
			// 
			// PackDepotDocAddressControl
			// 
			this.PackDepotDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PackDepotDocAddressControl, "PackDepotAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestHeader)(null)).PackDepotAddress)));
			this.PackDepotDocAddressControl.BindToOrganisations = "OrgHeaderList";
			this.PackDepotDocAddressControl.Text = "Pack Depot";
			this.PackDepotDocAddressControl.CaptionResourceString = null;
			this.PackDepotDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.HideOverrideAndTabs;
			this.PackDepotDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 111, true);
			this.PackDepotDocAddressControl.Name = "PackDepotDocAddressControl";
			this.PackDepotDocAddressControl.ReadOnly = false;
			this.PackDepotDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.PackDepotDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 68, true);
			this.PackDepotDocAddressControl.TabIndex = 72;
			this.PackDepotDocAddressControl.ValidationJustForced = false;
			// 
			// LloydsIMOTextBox
			// 
			this.BindingSource.SetBindingMember(this.LloydsIMOTextBox, "ED_LloydsIMO");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestHeader)(null)).ED_LloydsIMO)));
			this.LloydsIMOTextBox.CaptionResourceString = null;
			this.LloydsIMOTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(627, 32, true);
			this.LloydsIMOTextBox.Name = "LloydsIMOTextBox";
			this.LloydsIMOTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(172, 17, true);
			this.LloydsIMOTextBox.TabIndex = 9;
			// 
			// LloydsLabel
			// 
			this.LloydsLabel.AutoSize = true;
			this.LloydsLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.LloydsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(499, 35, true);
			this.LloydsLabel.Name = "LloydsLabel";
			this.LloydsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(61, 13, true);
			this.LloydsLabel.TabIndex = 74;
			this.LloydsLabel.Text = "Lloyds/IMO:";
			// 
			// BaseHeaderDetails
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.LloydsIMOTextBox);
			this.Controls.Add(this.LloydsLabel);
			this.Controls.Add(this.PackDepotDocAddressControl);
			this.Controls.Add(this.RecalculateTotalsButton);
			this.Controls.Add(this.CTOFindBox);
			this.Controls.Add(this.ContingencyCANTextBox);
			this.Controls.Add(this.ContingencyCANLabel);
			this.Controls.Add(this.VesselCodeFindBox);
			this.Controls.Add(this.CTOLabel);
			this.Controls.Add(this.PortOfDestinationCodeFindBox);
			this.Controls.Add(this.PortOfDestinationLabel);
			this.Controls.Add(this.TotalContainerCountLabel);
			this.Controls.Add(this.TotalPackageCountCalcEdit);
			this.Controls.Add(this.VoyageFlightNoTextBox);
			this.Controls.Add(this.VesselIDMasterAirWaybillLabel);
			this.Controls.Add(this.PortOfDepartureCodeFindBox);
			this.Controls.Add(this.CountryOfDestainationLabel);
			this.Controls.Add(this.ModeOfTransportLabel);
			this.Controls.Add(this.TotalContainerCountCalcEdit);
			this.Controls.Add(this.DateOfDepartureLabel);
			this.Controls.Add(this.CANTextBox);
			this.Controls.Add(this.DateOfDepartureDateEdit);
			this.Controls.Add(this.VoyageFlightNoLabel);
			this.Controls.Add(this.TotalEmptyContainerCountCalcEdit);
			this.Controls.Add(this.CountryOfDestinationCodeFindBox);
			this.Controls.Add(this.TotalEmptyContainerCountLabel);
			this.Controls.Add(this.ModeOfTransportDropEdit);
			this.Controls.Add(this.PortOfDepartureLabel);
			this.Controls.Add(this.TotalPackageCountLabel);
			this.Controls.Add(this.CANLabel);
			this.Name = "BaseHeaderDetails";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(813, 183, true);
			this.Click += new System.EventHandler(this.RecalculateTotalsButton_Click);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CTOFindBox.ResumeLayout(true);
			this.CTOFindBox.PerformLayout();
			this.VesselCodeFindBox.ResumeLayout(true);
			this.VesselCodeFindBox.PerformLayout();
			this.PortOfDestinationCodeFindBox.ResumeLayout(true);
			this.PortOfDestinationCodeFindBox.PerformLayout();
			this.PortOfDepartureCodeFindBox.ResumeLayout(true);
			this.PortOfDepartureCodeFindBox.PerformLayout();
			this.DateOfDepartureDateEdit.ResumeLayout(true);
			this.DateOfDepartureDateEdit.PerformLayout();
			this.CountryOfDestinationCodeFindBox.ResumeLayout(true);
			this.CountryOfDestinationCodeFindBox.PerformLayout();
			this.ModeOfTransportDropEdit.ResumeLayout(true);
			this.ModeOfTransportDropEdit.PerformLayout();
			this.PackDepotDocAddressControl.ResumeLayout(true);
			this.PackDepotDocAddressControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected internal Enterprise.ZArchitecture.GUI.ZButton RecalculateTotalsButton;
		protected Enterprise.ZArchitecture.GUI.ZAddressControl CTOFindBox;
		protected Enterprise.ZArchitecture.ZTextBox ContingencyCANTextBox;
		protected Enterprise.ZArchitecture.ZLabel ContingencyCANLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZCodeFindBox VesselCodeFindBox;
		protected Enterprise.ZArchitecture.ZLabel CTOLabel;
		protected Enterprise.ZArchitecture.GUI.ZCodeFindBox PortOfDestinationCodeFindBox;
		protected Enterprise.ZArchitecture.ZLabel PortOfDestinationLabel;
		protected internal Enterprise.ZArchitecture.ZLabel TotalContainerCountLabel;
		protected Enterprise.ZArchitecture.ZCalcEdit TotalPackageCountCalcEdit;
		protected internal Enterprise.ZArchitecture.ZTextBox VoyageFlightNoTextBox;
		protected internal Enterprise.ZArchitecture.ZLabel VesselIDMasterAirWaybillLabel;
		protected Enterprise.ZArchitecture.GUI.ZCodeFindBox PortOfDepartureCodeFindBox;
		protected Enterprise.ZArchitecture.ZLabel CountryOfDestainationLabel;
		protected Enterprise.ZArchitecture.ZLabel ModeOfTransportLabel;
		protected internal Enterprise.ZArchitecture.ZCalcEdit TotalContainerCountCalcEdit;
		protected Enterprise.ZArchitecture.ZLabel DateOfDepartureLabel;
		protected Enterprise.ZArchitecture.ZTextBox CANTextBox;
		protected Enterprise.ZArchitecture.GUI.ZDateEdit DateOfDepartureDateEdit;
		protected internal Enterprise.ZArchitecture.ZLabel VoyageFlightNoLabel;
		protected internal Enterprise.ZArchitecture.ZCalcEdit TotalEmptyContainerCountCalcEdit;
		protected Enterprise.ZArchitecture.GUI.ZCodeFindBox CountryOfDestinationCodeFindBox;
		protected internal Enterprise.ZArchitecture.ZLabel TotalEmptyContainerCountLabel;
		protected Enterprise.ZArchitecture.ZLabel PortOfDepartureLabel;
		protected Enterprise.ZArchitecture.ZLabel TotalPackageCountLabel;
		protected Enterprise.ZArchitecture.ZLabel CANLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZDropEdit ModeOfTransportDropEdit;
		protected Enterprise.MasterFiles.GUI.ZDocAddressControl PackDepotDocAddressControl;
		protected internal Enterprise.ZArchitecture.ZTextBox LloydsIMOTextBox;
		protected internal Enterprise.ZArchitecture.ZLabel LloydsLabel;
	}
}
