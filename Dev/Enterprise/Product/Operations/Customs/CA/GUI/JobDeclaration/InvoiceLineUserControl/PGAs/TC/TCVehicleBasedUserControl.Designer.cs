namespace Enterprise.Customs.CA.GUI
{
	partial class TCVehicleBasedUserControl
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
			this.TCSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.CommodityGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ManufactureYearDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ManufactureMonthDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ModelYearDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.VINNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.StateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OriginCountryFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.VehicleClassDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ImporterDeclarationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.MakeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ManufacturerLetterCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.TitleStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.VehicleStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ODOReadingTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.StatementLabelCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.AssemblerNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ModelTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ManufacturerAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.VehicleConditionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.BottomSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.ChassisGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ChassisYearDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ClassisMakeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ChassisManufacturerNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ChassisModelTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LPCOsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LPCOsGridUserControl = new Enterprise.Customs.CA.GUI.LPCOGridUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.TCSplitContainer)).BeginInit();
			this.TCSplitContainer.Panel1.SuspendLayout();
			this.TCSplitContainer.Panel2.SuspendLayout();
			this.TCSplitContainer.SuspendLayout();
			this.CommodityGroupBox.SuspendLayout();
			this.ManufactureYearDropEdit.SuspendLayout();
			this.ManufactureMonthDropEdit.SuspendLayout();
			this.ModelYearDropEdit.SuspendLayout();
			this.OriginCountryFindBox.SuspendLayout();
			this.VehicleClassDropEdit.SuspendLayout();
			this.TitleStatusDropEdit.SuspendLayout();
			this.VehicleStatusDropEdit.SuspendLayout();
			this.ManufacturerAddressControl.SuspendLayout();
			this.VehicleConditionDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BottomSplitContainer)).BeginInit();
			this.BottomSplitContainer.Panel1.SuspendLayout();
			this.BottomSplitContainer.Panel2.SuspendLayout();
			this.BottomSplitContainer.SuspendLayout();
			this.ChassisGroupBox.SuspendLayout();
			this.ChassisYearDropEdit.SuspendLayout();
			this.LPCOsGroupBox.SuspendLayout();
			this.LPCOsGridUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.TCPGAHeader);
			// 
			// TCSplitContainer
			// 
			this.TCSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TCSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.TCSplitContainer.IsSplitterFixed = true;
			this.TCSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TCSplitContainer.Name = "TCSplitContainer";
			this.TCSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// TCSplitContainer.Panel1
			// 
			this.TCSplitContainer.Panel1.Controls.Add(this.CommodityGroupBox);
			this.TCSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1084, 621, true);
			this.TCSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(0);
			// 
			// TCSplitContainer.Panel2
			// 
			this.TCSplitContainer.Panel2.Controls.Add(this.BottomSplitContainer);
			this.TCSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(204);
			this.TCSplitContainer.TabIndex = 1;
			// 
			// CommodityGroupBox
			// 
			this.CommodityGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("9a7988c3-0855-48e7-9cc1-646c183ed4a3", "Commodity Details");
			this.CommodityGroupBox.Controls.Add(this.ManufactureYearDropEdit);
			this.CommodityGroupBox.Controls.Add(this.ManufactureMonthDropEdit);
			this.CommodityGroupBox.Controls.Add(this.ModelYearDropEdit);
			this.CommodityGroupBox.Controls.Add(this.VINNumberTextBox);
			this.CommodityGroupBox.Controls.Add(this.StateButton);
			this.CommodityGroupBox.Controls.Add(this.OriginCountryFindBox);
			this.CommodityGroupBox.Controls.Add(this.VehicleClassDropEdit);
			this.CommodityGroupBox.Controls.Add(this.ImporterDeclarationCheckBox);
			this.CommodityGroupBox.Controls.Add(this.MakeTextBox);
			this.CommodityGroupBox.Controls.Add(this.ManufacturerLetterCheckBox);
			this.CommodityGroupBox.Controls.Add(this.TitleStatusDropEdit);
			this.CommodityGroupBox.Controls.Add(this.VehicleStatusDropEdit);
			this.CommodityGroupBox.Controls.Add(this.ODOReadingTextBox);
			this.CommodityGroupBox.Controls.Add(this.StatementLabelCheckBox);
			this.CommodityGroupBox.Controls.Add(this.AssemblerNameTextBox);
			this.CommodityGroupBox.Controls.Add(this.ModelTextBox);
			this.CommodityGroupBox.Controls.Add(this.ManufacturerAddressControl);
			this.CommodityGroupBox.Controls.Add(this.VehicleConditionDropEdit);
			this.CommodityGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CommodityGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CommodityGroupBox.Name = "CommodityGroupBox";
			this.CommodityGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1084, 204, true);
			this.CommodityGroupBox.TabIndex = 1;
			this.CommodityGroupBox.TabStop = false;
			// 
			// ManufactureYearDropEdit
			// 
			this.ManufactureYearDropEdit.AllowDrop = true;
			this.ManufactureYearDropEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ManufactureYearDropEdit, "CA_ManufactureYear");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.TCPGAHeader)(null)).CA_ManufactureYear)));
			this.ManufactureYearDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("0a6fd215-c968-44ec-b4fe-4e668fc93f2e", "Manuf. Date", "Manufacture Year", "");
			this.ManufactureYearDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(790, 45, true);
			this.ManufactureYearDropEdit.Name = "ManufactureYearDropEdit";
			this.ManufactureYearDropEdit.ShowDescriptionBox = false;
			this.ManufactureYearDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.ManufactureYearDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.ManufactureYearDropEdit.TabIndex = 16;
			this.ManufactureYearDropEdit.Visible = false;
			// 
			// ManufactureMonthDropEdit
			// 
			this.ManufactureMonthDropEdit.AllowDrop = true;
			this.ManufactureMonthDropEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ManufactureMonthDropEdit, "CA_ManufactureMonth");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.TCPGAHeader)(null)).CA_ManufactureMonth)));
			this.ManufactureMonthDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("275c59ce-1412-4afb-a7bf-cec6f78a3c47", "Month");
			this.ManufactureMonthDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(900, 45, true);
			this.ManufactureMonthDropEdit.Name = "ManufactureMonthDropEdit";
			this.ManufactureMonthDropEdit.ShowDescriptionBox = false;
			this.ManufactureMonthDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.ManufactureMonthDropEdit.TabIndex = 17;
			this.ManufactureMonthDropEdit.Visible = false;
			// 
			// ModelYearDropEdit
			// 
			this.ModelYearDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ModelYearDropEdit, "InvoiceLine.CA_ModelYear");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.TCPGAHeader)(null)).InvoiceLine.CA_ModelYear)));
			this.ModelYearDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ea55a7fc-6e4d-482e-9071-18ebba5334e4", "Model Year");
			this.ModelYearDropEdit.BindToList = "InvoiceLine.AddInfoLookups.ModelYearList";
			this.ModelYearDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(790, 19, true);
			this.ModelYearDropEdit.Name = "ModelYearDropEdit";
			this.ModelYearDropEdit.ShowDescriptionBox = false;
			this.ModelYearDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.ModelYearDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.ModelYearDropEdit.TabIndex = 15;
			this.ModelYearDropEdit.Visible = false;
			// 
			// VINNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.VINNumberTextBox, "InvoiceLine.CA_VINNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.TCPGAHeader)(null)).InvoiceLine.CA_VINNumber)));
			this.VINNumberTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("f4e09949-8146-4a10-9787-67a97e54209e", "VIN");
			this.VINNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(472, 19, true);
			this.VINNumberTextBox.Name = "VINNumberTextBox";
			this.VINNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.VINNumberTextBox.TabIndex = 8;
			this.VINNumberTextBox.Visible = false;
			// 
			// StateButton
			// 
			this.StateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(930, 173, true);
			this.StateButton.Name = "StateButton";
			this.StateButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.StateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(74, 23, true);
			this.StateButton.TabIndex = 22;
			this.StateButton.Text = "Stmt Text";
			this.StateButton.ToolTipCaption = null;
			this.StateButton.UseVisualStyleBackColor = true;
			this.StateButton.Visible = false;
			this.StateButton.Click += new System.EventHandler(this.StateButton_Click);
			// 
			// OriginCountryFindBox
			// 
			this.OriginCountryFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OriginCountryFindBox, "RN_NKCountryOfOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.TCPGAHeader)(null)).RN_NKCountryOfOrigin)));
			this.OriginCountryFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("8dcb2c6f-ff7b-4da1-b9c2-cc55e1c45e5d", "Origin Country/region");
			this.OriginCountryFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(472, 123, true);
			this.OriginCountryFindBox.Name = "OriginCountryFindBox";
			this.OriginCountryFindBox.ShouldResize = true;
			this.OriginCountryFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.OriginCountryFindBox.TabIndex = 12;
			this.OriginCountryFindBox.Visible = false;
			// 
			// VehicleClassDropEdit
			// 
			this.VehicleClassDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VehicleClassDropEdit, "CA_ProductClass");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.TCPGAHeader)(null)).CA_ProductClass)));
			this.VehicleClassDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("08f4b825-11cb-49f9-8a0d-4f2e1290df7f", "Vehicle Class");
			this.VehicleClassDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 19, true);
			this.VehicleClassDropEdit.Name = "VehicleClassDropEdit";
			this.VehicleClassDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.VehicleClassDropEdit.TabIndex = 1;
			this.VehicleClassDropEdit.Visible = false;
			// 
			// ImporterDeclarationCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ImporterDeclarationCheckBox, "IsVPRImporterDeclared");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.Customs.CA.Business.TCPGAHeader)(null)).IsVPRImporterDeclared)));
			this.ImporterDeclarationCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("7bcb0334-e276-4228-94de-4c4439dc9511", "Importer Statement");
			this.ImporterDeclarationCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ImporterDeclarationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(790, 174, true);
			this.ImporterDeclarationCheckBox.Name = "ImporterDeclarationCheckBox";
			this.ImporterDeclarationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(134, 23, true);
			this.ImporterDeclarationCheckBox.TabIndex = 21;
			this.ImporterDeclarationCheckBox.UseVisualStyleBackColor = true;
			this.ImporterDeclarationCheckBox.Visible = false;
			// 
			// MakeTextBox
			// 
			this.BindingSource.SetBindingMember(this.MakeTextBox, "JI_BrandName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.TCPGAHeader)(null)).JI_BrandName)));
			this.MakeTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("7fb55a96-515b-47ec-99d4-c7c7f86cc351", "Make");
			this.MakeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(472, 45, true);
			this.MakeTextBox.Name = "MakeTextBox";
			this.MakeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.MakeTextBox.TabIndex = 9;
			this.MakeTextBox.Visible = false;
			// 
			// ManufacturerLetterCheckBox
			// 
			this.ManufacturerLetterCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ManufacturerLetterCheckBox, "ManufacturerLetterAttached");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.TCPGAHeader)(null)).ManufacturerLetterAttached)));
			this.ManufacturerLetterCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("4ca01838-4698-41b1-bd42-edcba60384f9", "Manuf. Letter of Compliance Label Attached", "Manufacturer Letter of Compliance Label Attached", "");
			this.ManufacturerLetterCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ManufacturerLetterCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(790, 151, true);
			this.ManufacturerLetterCheckBox.Name = "ManufacturerLetterCheckBox";
			this.ManufacturerLetterCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 17, true);
			this.ManufacturerLetterCheckBox.TabIndex = 20;
			this.ManufacturerLetterCheckBox.UseVisualStyleBackColor = true;
			this.ManufacturerLetterCheckBox.Visible = false;
			// 
			// TitleStatusDropEdit
			// 
			this.TitleStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TitleStatusDropEdit, "CA_TitleStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.TCPGAHeader)(null)).CA_TitleStatus)));
			this.TitleStatusDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("928c54af-7bb6-4898-94df-253baabd0789", "Title Status");
			this.TitleStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 123, true);
			this.TitleStatusDropEdit.Name = "TitleStatusDropEdit";
			this.TitleStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.TitleStatusDropEdit.TabIndex = 5;
			this.TitleStatusDropEdit.Visible = false;
			// 
			// VehicleStatusDropEdit
			// 
			this.VehicleStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VehicleStatusDropEdit, "CA_VehicleStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.TCPGAHeader)(null)).CA_VehicleStatus)));
			this.VehicleStatusDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("8258d792-c5a1-421b-952b-19465eba9fc5", "Vehicle Status");
			this.VehicleStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(790, 97, true);
			this.VehicleStatusDropEdit.Name = "VehicleStatusDropEdit";
			this.VehicleStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(195, 20, true);
			this.VehicleStatusDropEdit.TabIndex = 18;
			this.VehicleStatusDropEdit.Visible = false;
			// 
			// ODOReadingTextBox
			// 
			this.BindingSource.SetBindingMember(this.ODOReadingTextBox, "CA_ODOReading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.TCPGAHeader)(null)).CA_ODOReading)));
			this.ODOReadingTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("36fa7d1d-f305-4431-ba67-bdfbc59da499", "ODO Reading");
			this.ODOReadingTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(472, 97, true);
			this.ODOReadingTextBox.Name = "ODOReadingTextBox";
			this.ODOReadingTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.ODOReadingTextBox.TabIndex = 11;
			this.ODOReadingTextBox.Visible = false;
			// 
			// StatementLabelCheckBox
			// 
			this.StatementLabelCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.StatementLabelCheckBox, "StatementLabelAttached");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.TCPGAHeader)(null)).StatementLabelAttached)));
			this.StatementLabelCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("5a9f28ef-97fe-4e21-93b3-84f2a4a893a6", "Statement of Compliance Label Attached");
			this.StatementLabelCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.StatementLabelCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(790, 126, true);
			this.StatementLabelCheckBox.Name = "StatementLabelCheckBox";
			this.StatementLabelCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(219, 17, true);
			this.StatementLabelCheckBox.TabIndex = 19;
			this.StatementLabelCheckBox.UseVisualStyleBackColor = true;
			this.StatementLabelCheckBox.Visible = false;
			// 
			// AssemblerNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.AssemblerNameTextBox, "CA_AssemblerName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.TCPGAHeader)(null)).CA_AssemblerName)));
			this.AssemblerNameTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("57069e2a-8854-4559-8d7f-b8843ec250f2", "Assembler", "Assembler Name", "");
			this.AssemblerNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 71, true);
			this.AssemblerNameTextBox.Name = "AssemblerNameTextBox";
			this.AssemblerNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.AssemblerNameTextBox.TabIndex = 3;
			this.AssemblerNameTextBox.Visible = false;
			// 
			// ModelTextBox
			// 
			this.BindingSource.SetBindingMember(this.ModelTextBox, "JI_Model");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.TCPGAHeader)(null)).JI_Model)));
			this.ModelTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("e8b2a863-6a6e-4def-af1b-98d5b47747de", "Model");
			this.ModelTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(472, 71, true);
			this.ModelTextBox.Name = "ModelTextBox";
			this.ModelTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.ModelTextBox.TabIndex = 10;
			this.ModelTextBox.Visible = false;
			// 
			// ManufacturerAddressControl
			// 
			this.ManufacturerAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ManufacturerAddressControl, "OA_Manufacturer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CA.Business.TCPGAHeader)(null)).OA_Manufacturer)));
			this.ManufacturerAddressControl.BindToOrgList = "RequirementsParent.ManufacturersLookup";
			this.ManufacturerAddressControl.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ff83515b-607e-4557-a796-b032329ec505", "Manuf.", "Manufacturer", "");
			this.ManufacturerAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 45, true);
			this.ManufacturerAddressControl.Name = "ManufacturerAddressControl";
			this.ManufacturerAddressControl.PopupCaption = "";
			this.ManufacturerAddressControl.ReadOnly = false;
			this.ManufacturerAddressControl.ShowAddress = false;
			this.ManufacturerAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.ManufacturerAddressControl.TabIndex = 2;
			this.ManufacturerAddressControl.Visible = false;
			// 
			// VehicleConditionDropEdit
			// 
			this.VehicleConditionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VehicleConditionDropEdit, "CA_VehicleCondition");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.TCPGAHeader)(null)).CA_VehicleCondition)));
			this.VehicleConditionDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("de8b0494-538d-41c1-a8ee-e31eff40bfea", "Vehicle Condition");
			this.VehicleConditionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 97, true);
			this.VehicleConditionDropEdit.Name = "VehicleConditionDropEdit";
			this.VehicleConditionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.VehicleConditionDropEdit.TabIndex = 4;
			this.VehicleConditionDropEdit.Visible = false;
			// 
			// BottomSplitContainer
			// 
			this.BottomSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BottomSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.BottomSplitContainer.IsSplitterFixed = true;
			this.BottomSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BottomSplitContainer.Name = "BottomSplitContainer";
			this.BottomSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// BottomSplitContainer.Panel1
			// 
			this.BottomSplitContainer.Panel1.Controls.Add(this.ChassisGroupBox);
			// 
			// BottomSplitContainer.Panel2
			// 
			this.BottomSplitContainer.Panel2.Controls.Add(this.LPCOsGroupBox);
			this.BottomSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1084, 413, true);
			this.BottomSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(43);
			this.BottomSplitContainer.TabIndex = 0;
			// 
			// ChassisGroupBox
			// 
			this.ChassisGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ChassisGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("b04af929-cdc8-40ba-8e47-1ff104cf5db1", "Chassis Details");
			this.ChassisGroupBox.Controls.Add(this.ChassisYearDropEdit);
			this.ChassisGroupBox.Controls.Add(this.ClassisMakeTextBox);
			this.ChassisGroupBox.Controls.Add(this.ChassisManufacturerNameTextBox);
			this.ChassisGroupBox.Controls.Add(this.ChassisModelTextBox);
			this.ChassisGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ChassisGroupBox.Name = "ChassisGroupBox";
			this.ChassisGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1084, 43, true);
			this.ChassisGroupBox.TabIndex = 2;
			this.ChassisGroupBox.TabStop = false;
			// 
			// ChassisYearDropEdit
			// 
			this.ChassisYearDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ChassisYearDropEdit, "CA_ChassisYear");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.TCPGAHeader)(null)).CA_ChassisYear)));
			this.ChassisYearDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("1f6670e7-1005-43b6-a185-b87f00eef460", "Year");
			this.ChassisYearDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(930, 14, true);
			this.ChassisYearDropEdit.Name = "ChassisYearDropEdit";
			this.ChassisYearDropEdit.ShowDescriptionBox = false;
			this.ChassisYearDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.ChassisYearDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.ChassisYearDropEdit.TabIndex = 16;
			// 
			// ClassisMakeTextBox
			// 
			this.BindingSource.SetBindingMember(this.ClassisMakeTextBox, "CA_ChassisMake");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.TCPGAHeader)(null)).CA_ChassisMake)));
			this.ClassisMakeTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("fd87f773-0768-42b3-b0ad-fcbdd72e1fde", "Make");
			this.ClassisMakeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(472, 14, true);
			this.ClassisMakeTextBox.Name = "ClassisMakeTextBox";
			this.ClassisMakeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 20, true);
			this.ClassisMakeTextBox.TabIndex = 13;
			// 
			// ChassisManufacturerNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.ChassisManufacturerNameTextBox, "CA_ChassisManufacturerName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.TCPGAHeader)(null)).CA_ChassisManufacturerName)));
			this.ChassisManufacturerNameTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("989293df-ea02-4ff8-9755-9663c08d29e6", "Manuf.", "Manufacturer", "");
			this.ChassisManufacturerNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 14, true);
			this.ChassisManufacturerNameTextBox.Name = "ChassisManufacturerNameTextBox";
			this.ChassisManufacturerNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.ChassisManufacturerNameTextBox.TabIndex = 12;
			// 
			// ChassisModelTextBox
			// 
			this.BindingSource.SetBindingMember(this.ChassisModelTextBox, "CA_ChassisModel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.TCPGAHeader)(null)).CA_ChassisModel)));
			this.ChassisModelTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("adde3113-f6a7-48a0-a723-06d07457a15a", "Model");
			this.ChassisModelTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(706, 14, true);
			this.ChassisModelTextBox.Name = "ChassisModelTextBox";
			this.ChassisModelTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(186, 20, true);
			this.ChassisModelTextBox.TabIndex = 14;
			// 
			// LPCOsGroupBox
			// 
			this.LPCOsGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("e1f82bb4-eaae-4baf-877f-da5ae9c9538e", "LPCOs");
			this.LPCOsGroupBox.Controls.Add(this.LPCOsGridUserControl);
			this.LPCOsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LPCOsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LPCOsGroupBox.Name = "LPCOsGroupBox";
			this.LPCOsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1084, 366, true);
			this.LPCOsGroupBox.TabIndex = 3;
			this.LPCOsGroupBox.TabStop = false;
			// 
			// LPCOsGridUserControl
			// 
			this.LPCOsGridUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LPCOsGridUserControl, "LPCOViews");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.CA.Business.LPCOViewCollection)(((Enterprise.Customs.CA.Business.TCPGAHeader)(null)).LPCOViews)));
			this.LPCOsGridUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LPCOsGridUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.LPCOsGridUserControl.Name = "LPCOsGridUserControl";
			this.LPCOsGridUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1078, 347, true);
			this.LPCOsGridUserControl.TabIndex = 1;
			// 
			// TCVehicleBasedUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TCSplitContainer);
			this.Name = "TCVehicleBasedUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1084, 621, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TCSplitContainer.Panel1.ResumeLayout(false);
			this.TCSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.TCSplitContainer)).EndInit();
			this.TCSplitContainer.ResumeLayout(false);
			this.TCSplitContainer.PerformLayout();
			this.CommodityGroupBox.ResumeLayout(false);
			this.CommodityGroupBox.PerformLayout();
			this.ManufactureYearDropEdit.ResumeLayout(true);
			this.ManufactureYearDropEdit.PerformLayout();
			this.ManufactureMonthDropEdit.ResumeLayout(true);
			this.ManufactureMonthDropEdit.PerformLayout();
			this.ModelYearDropEdit.ResumeLayout(true);
			this.ModelYearDropEdit.PerformLayout();
			this.OriginCountryFindBox.ResumeLayout(true);
			this.OriginCountryFindBox.PerformLayout();
			this.VehicleClassDropEdit.ResumeLayout(true);
			this.VehicleClassDropEdit.PerformLayout();
			this.TitleStatusDropEdit.ResumeLayout(true);
			this.TitleStatusDropEdit.PerformLayout();
			this.VehicleStatusDropEdit.ResumeLayout(true);
			this.VehicleStatusDropEdit.PerformLayout();
			this.ManufacturerAddressControl.ResumeLayout(true);
			this.ManufacturerAddressControl.PerformLayout();
			this.VehicleConditionDropEdit.ResumeLayout(true);
			this.VehicleConditionDropEdit.PerformLayout();
			this.BottomSplitContainer.Panel1.ResumeLayout(false);
			this.BottomSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.BottomSplitContainer)).EndInit();
			this.BottomSplitContainer.ResumeLayout(false);
			this.BottomSplitContainer.PerformLayout();
			this.ChassisGroupBox.ResumeLayout(false);
			this.ChassisGroupBox.PerformLayout();
			this.ChassisYearDropEdit.ResumeLayout(true);
			this.ChassisYearDropEdit.PerformLayout();
			this.LPCOsGroupBox.ResumeLayout(false);
			this.LPCOsGroupBox.PerformLayout();
			this.LPCOsGridUserControl.ResumeLayout(true);
			this.LPCOsGridUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected CargoWise.Windows.UI.KSplitContainer TCSplitContainer;
		protected ZArchitecture.GUI.ZGroupBox CommodityGroupBox;
		protected ZArchitecture.GUI.ZDropEdit TitleStatusDropEdit;
		protected ZArchitecture.GUI.ZAddressControl ManufacturerAddressControl;
		protected ZArchitecture.ZTextBox AssemblerNameTextBox;
		protected ZArchitecture.GUI.ZDropEdit VehicleConditionDropEdit;
		protected ZArchitecture.GUI.ZDropEdit VehicleClassDropEdit;
		protected ZArchitecture.ZTextBox VINNumberTextBox;
		protected ZArchitecture.GUI.ZCodeFindBox OriginCountryFindBox;
		protected ZArchitecture.ZTextBox MakeTextBox;
		protected ZArchitecture.ZTextBox ODOReadingTextBox;
		protected ZArchitecture.ZTextBox ModelTextBox;
		protected ZArchitecture.GUI.ZDropEdit VehicleStatusDropEdit;
		protected ZArchitecture.GUI.ZCheckBox ImporterDeclarationCheckBox;
		protected ZArchitecture.GUI.ZGroupBox ChassisGroupBox;
		protected ZArchitecture.ZTextBox ClassisMakeTextBox;
		protected ZArchitecture.ZTextBox ChassisManufacturerNameTextBox;
		protected ZArchitecture.ZTextBox ChassisModelTextBox;
		protected ZArchitecture.GUI.ZGroupBox LPCOsGroupBox;
		internal LPCOGridUserControl LPCOsGridUserControl;
		protected ZArchitecture.GUI.ZButton StateButton;
		protected ZArchitecture.GUI.ZCheckBox ManufacturerLetterCheckBox;
		protected ZArchitecture.GUI.ZCheckBox StatementLabelCheckBox;
		protected CargoWise.Windows.UI.KSplitContainer BottomSplitContainer;
		protected ZArchitecture.GUI.ZDropEdit ChassisYearDropEdit;
		protected ZArchitecture.GUI.ZDropEdit ModelYearDropEdit;
		protected ZArchitecture.GUI.ZDropEdit ManufactureYearDropEdit;
		protected ZArchitecture.GUI.ZDropEdit ManufactureMonthDropEdit;
	}
}
