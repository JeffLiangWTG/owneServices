namespace Enterprise.Customs.GB.GUI.Wizards
{
	partial class EidrWizardForm
	{
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
            this.CreateEidrButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.InvoiceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.DateOfImportDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.AdditionalReferenceNumbersTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.WarehouseAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
            this.DescriptionOfGoodsTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.NumberOfPackagesCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.TypeOfPackagesDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.CustomsValueInGBPCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.NetMassInKGCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.DeclarantAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
            this.TransportModeFindBox = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.CountryOfOriginFindBox = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.EIDRTypeFindBox = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.SupplementaryDeclarationDueDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.CpcCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.DateOfImportDateEdit.SuspendLayout();
            this.WarehouseAddressControl.SuspendLayout();
            this.TypeOfPackagesDropEdit.SuspendLayout();
            this.DeclarantAddressControl.SuspendLayout();
            this.TransportModeFindBox.SuspendLayout();
            this.CountryOfOriginFindBox.SuspendLayout();
            this.EIDRTypeFindBox.SuspendLayout();
            this.SupplementaryDeclarationDueDateDateEdit.SuspendLayout();
            this.CpcCodeFindBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 565, true);
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(612, 22, true);
            this.MainStatusBar.TabIndex = 13;
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.Business.Wizards.EIDR.EidrWizard);
            // 
            // CreateEidrButton
            // 
            this.CreateEidrButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CreateEidrButton.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("3f34f24f-d4e9-4017-9efe-41f7457882e4", "Create EIDR");
            this.CreateEidrButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.CreateEidrButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(222, 371, true);
            this.CreateEidrButton.Name = "CreateEidrButton";
            this.CreateEidrButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 23, true);
            this.CreateEidrButton.TabIndex = 30;
            this.CreateEidrButton.ToolTipCaption = null;
            this.CreateEidrButton.UseVisualStyleBackColor = true;
            this.CreateEidrButton.Click += new System.EventHandler(this.ButtonCreateEidr_Click);
            // 
            // InvoiceNumberTextBox
            // 
            this.InvoiceNumberTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.InvoiceNumberTextBox, "InvoiceNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Business.Wizards.EIDR.EidrWizard)(null)).InvoiceNumber)));
            this.InvoiceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(222, 37, true);
            this.InvoiceNumberTextBox.Name = "InvoiceNumberTextBox";
            this.InvoiceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 20, true);
            this.InvoiceNumberTextBox.TabIndex = 16;
            // 
            // DateOfImportDateEdit
            // 
            this.DateOfImportDateEdit.AllowDrop = true;
            this.DateOfImportDateEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.DateOfImportDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.DateOfImportDateEdit, "DateOfImport");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Business.Wizards.EIDR.EidrWizard)(null)).DateOfImport)));
            this.DateOfImportDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(222, 58, true);
            this.DateOfImportDateEdit.Name = "DateOfImportDateEdit";
            this.DateOfImportDateEdit.TabIndex = 17;
            // 
            // AdditionalReferenceNumbersTextBox
            // 
            this.AdditionalReferenceNumbersTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.AdditionalReferenceNumbersTextBox, "AdditionalReferenceNumbers");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Business.Wizards.EIDR.EidrWizard)(null)).AdditionalReferenceNumbers)));
            this.AdditionalReferenceNumbersTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(222, 81, true);
            this.AdditionalReferenceNumbersTextBox.Name = "AdditionalReferenceNumbersTextBox";
            this.AdditionalReferenceNumbersTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(369, 20, true);
            this.AdditionalReferenceNumbersTextBox.TabIndex = 18;
            // 
            // WarehouseAddressControl
            // 
            this.WarehouseAddressControl.AllowDrop = true;
            this.WarehouseAddressControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.WarehouseAddressControl, "WarehouseOrgAddressPk");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.GB.Business.Wizards.EIDR.EidrWizard)(null)).WarehouseOrgAddressPk)));
            this.WarehouseAddressControl.BindToOrgList = "Lookups+WarehouseList";
            this.WarehouseAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(222, 103, true);
            this.WarehouseAddressControl.Name = "WarehouseAddressControl";
            this.WarehouseAddressControl.PopupCaption = "";
            this.WarehouseAddressControl.ShowAddress = false;
            this.WarehouseAddressControl.ShowOrganisationName = true;
            this.WarehouseAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 20, true);
            this.WarehouseAddressControl.TabIndex = 19;
            // 
            // DescriptionOfGoodsTextBox
            // 
            this.DescriptionOfGoodsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.DescriptionOfGoodsTextBox, "DescriptionOfGoods");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Business.Wizards.EIDR.EidrWizard)(null)).DescriptionOfGoods)));
            this.DescriptionOfGoodsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(222, 125, true);
            this.DescriptionOfGoodsTextBox.Name = "DescriptionOfGoodsTextBox";
            this.DescriptionOfGoodsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(369, 20, true);
            this.DescriptionOfGoodsTextBox.TabIndex = 20;
            // 
            // NumberOfPackagesCalcEdit
            // 
            this.NumberOfPackagesCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.NumberOfPackagesCalcEdit, "NumberOfPackages");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.GB.Business.Wizards.EIDR.EidrWizard)(null)).NumberOfPackages)));
            this.NumberOfPackagesCalcEdit.DecimalPlaces = 2;
            this.NumberOfPackagesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(222, 170, true);
            this.NumberOfPackagesCalcEdit.Name = "NumberOfPackagesCalcEdit";
            this.NumberOfPackagesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(58, 20, true);
            this.NumberOfPackagesCalcEdit.TabIndex = 22;
            this.NumberOfPackagesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // TypeOfPackagesDropEdit
            // 
            this.TypeOfPackagesDropEdit.AllowDrop = true;
            this.TypeOfPackagesDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.TypeOfPackagesDropEdit, "TypeOfPackages");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Business.Wizards.EIDR.EidrWizard)(null)).TypeOfPackages)));
            this.TypeOfPackagesDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(222, 193, true);
            this.TypeOfPackagesDropEdit.Name = "TypeOfPackagesDropEdit";
            this.TypeOfPackagesDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
            this.TypeOfPackagesDropEdit.TabIndex = 23;
            // 
            // CustomsValueInGBPCalcEdit
            // 
            this.CustomsValueInGBPCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.CustomsValueInGBPCalcEdit, "CustomsValueInGBP");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.GB.Business.Wizards.EIDR.EidrWizard)(null)).CustomsValueInGBP)));
            this.CustomsValueInGBPCalcEdit.DecimalPlaces = 2;
            this.CustomsValueInGBPCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(222, 147, true);
            this.CustomsValueInGBPCalcEdit.Name = "CustomsValueInGBPCalcEdit";
            this.CustomsValueInGBPCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(58, 20, true);
            this.CustomsValueInGBPCalcEdit.TabIndex = 21;
            this.CustomsValueInGBPCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // NetMassInKGCalcEdit
            // 
            this.NetMassInKGCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.NetMassInKGCalcEdit, "NetMassInKG");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.GB.Business.Wizards.EIDR.EidrWizard)(null)).NetMassInKG)));
            this.NetMassInKGCalcEdit.DecimalPlaces = 2;
            this.NetMassInKGCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(222, 214, true);
            this.NetMassInKGCalcEdit.Name = "NetMassInKGCalcEdit";
            this.NetMassInKGCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(58, 20, true);
            this.NetMassInKGCalcEdit.TabIndex = 24;
            this.NetMassInKGCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // DeclarantAddressControl
            // 
            this.DeclarantAddressControl.AllowDrop = true;
            this.DeclarantAddressControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.DeclarantAddressControl, "DeclarantOrgAddressPk");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.GB.Business.Wizards.EIDR.EidrWizard)(null)).DeclarantOrgAddressPk)));
            this.DeclarantAddressControl.BindToOrgList = "Lookups+DeclarantList";
            this.DeclarantAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(222, 237, true);
            this.DeclarantAddressControl.Name = "DeclarantAddressControl";
            this.DeclarantAddressControl.PopupCaption = "";
            this.DeclarantAddressControl.ShowAddress = false;
            this.DeclarantAddressControl.ShowOrganisationName = true;
            this.DeclarantAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 20, true);
            this.DeclarantAddressControl.TabIndex = 25;
            // 
            // TransportModeFindBox
            // 
            this.TransportModeFindBox.AllowDrop = true;
            this.TransportModeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.TransportModeFindBox, "TransportMode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Business.Wizards.EIDR.EidrWizard)(null)).TransportMode)));
            this.TransportModeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(222, 259, true);
            this.TransportModeFindBox.Name = "TransportModeFindBox";
            this.TransportModeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
            this.TransportModeFindBox.TabIndex = 26;
            // 
            // CountryOfOriginFindBox
            // 
            this.CountryOfOriginFindBox.AllowDrop = true;
            this.CountryOfOriginFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.CountryOfOriginFindBox, "CountryOfOrigin");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Business.Wizards.EIDR.EidrWizard)(null)).CountryOfOrigin)));
            this.CountryOfOriginFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(222, 282, true);
            this.CountryOfOriginFindBox.Name = "CountryOfOriginFindBox";
            this.CountryOfOriginFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
            this.CountryOfOriginFindBox.TabIndex = 27;
            // 
            // EIDRTypeFindBox
            // 
            this.EIDRTypeFindBox.AllowDrop = true;
            this.EIDRTypeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.EIDRTypeFindBox, "EIDRType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Business.Wizards.EIDR.EidrWizard)(null)).EIDRType)));
            this.EIDRTypeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(222, 305, true);
            this.EIDRTypeFindBox.Name = "EIDRTypeFindBox";
            this.EIDRTypeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
            this.EIDRTypeFindBox.TabIndex = 28;
            // 
            // SupplementaryDeclarationDueDateDateEdit
            // 
            this.SupplementaryDeclarationDueDateDateEdit.AllowDrop = true;
            this.SupplementaryDeclarationDueDateDateEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.SupplementaryDeclarationDueDateDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.SupplementaryDeclarationDueDateDateEdit, "SupplementaryDeclarationDueDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Business.Wizards.EIDR.EidrWizard)(null)).SupplementaryDeclarationDueDate)));
            this.SupplementaryDeclarationDueDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(222, 326, true);
            this.SupplementaryDeclarationDueDateDateEdit.Name = "SupplementaryDeclarationDueDateDateEdit";
            this.SupplementaryDeclarationDueDateDateEdit.TabIndex = 29;
            // 
            // CpcCodeFindBox
            // 
            this.CpcCodeFindBox.AllowDrop = true;
            this.CpcCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.CpcCodeFindBox, "CPC");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Business.Wizards.EIDR.EidrWizard)(null)).CPC)));
            this.CpcCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(222, 14, true);
            this.CpcCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusProcedure;
            this.CpcCodeFindBox.Name = "CpcCodeFindBox";
            this.CpcCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.CpcCodeFindBox.ParentType = null;
            this.CpcCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(369, 20, true);
            this.CpcCodeFindBox.TabIndex = 15;
            // 
            // EidrWizardForm
            // 
            this.CaptionRenderingEnabled = true;
            this.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("4fd8d290-8087-41cc-a884-9e012b5e4b5a", "EIDR Wizard");
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(612, 587, true);
            this.Controls.Add(this.CpcCodeFindBox);
            this.Controls.Add(this.SupplementaryDeclarationDueDateDateEdit);
            this.Controls.Add(this.EIDRTypeFindBox);
            this.Controls.Add(this.CountryOfOriginFindBox);
            this.Controls.Add(this.TransportModeFindBox);
            this.Controls.Add(this.DeclarantAddressControl);
            this.Controls.Add(this.NetMassInKGCalcEdit);
            this.Controls.Add(this.CustomsValueInGBPCalcEdit);
            this.Controls.Add(this.NumberOfPackagesCalcEdit);
            this.Controls.Add(this.TypeOfPackagesDropEdit);
            this.Controls.Add(this.DescriptionOfGoodsTextBox);
            this.Controls.Add(this.WarehouseAddressControl);
            this.Controls.Add(this.AdditionalReferenceNumbersTextBox);
            this.Controls.Add(this.DateOfImportDateEdit);
            this.Controls.Add(this.InvoiceNumberTextBox);
            this.Controls.Add(this.CreateEidrButton);
            this.DataSourceType = typeof(Enterprise.Customs.GB.Business.Wizards.EIDR.EidrWizard);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "EidrWizardForm";
            this.Controls.SetChildIndex(this.CreateEidrButton, 0);
            this.Controls.SetChildIndex(this.InvoiceNumberTextBox, 0);
            this.Controls.SetChildIndex(this.DateOfImportDateEdit, 0);
            this.Controls.SetChildIndex(this.AdditionalReferenceNumbersTextBox, 0);
            this.Controls.SetChildIndex(this.WarehouseAddressControl, 0);
            this.Controls.SetChildIndex(this.DescriptionOfGoodsTextBox, 0);
            this.Controls.SetChildIndex(this.TypeOfPackagesDropEdit, 0);
            this.Controls.SetChildIndex(this.NumberOfPackagesCalcEdit, 0);
            this.Controls.SetChildIndex(this.CustomsValueInGBPCalcEdit, 0);
            this.Controls.SetChildIndex(this.NetMassInKGCalcEdit, 0);
            this.Controls.SetChildIndex(this.DeclarantAddressControl, 0);
            this.Controls.SetChildIndex(this.TransportModeFindBox, 0);
            this.Controls.SetChildIndex(this.CountryOfOriginFindBox, 0);
            this.Controls.SetChildIndex(this.EIDRTypeFindBox, 0);
            this.Controls.SetChildIndex(this.SupplementaryDeclarationDueDateDateEdit, 0);
            this.Controls.SetChildIndex(this.CpcCodeFindBox, 0);
            this.Controls.SetChildIndex(this.MainStatusBar, 0);
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.DateOfImportDateEdit.ResumeLayout(true);
            this.DateOfImportDateEdit.PerformLayout();
            this.WarehouseAddressControl.ResumeLayout(true);
            this.WarehouseAddressControl.PerformLayout();
            this.TypeOfPackagesDropEdit.ResumeLayout(true);
            this.TypeOfPackagesDropEdit.PerformLayout();
            this.DeclarantAddressControl.ResumeLayout(true);
            this.DeclarantAddressControl.PerformLayout();
            this.TransportModeFindBox.ResumeLayout(true);
            this.TransportModeFindBox.PerformLayout();
            this.CountryOfOriginFindBox.ResumeLayout(true);
            this.CountryOfOriginFindBox.PerformLayout();
            this.EIDRTypeFindBox.ResumeLayout(true);
            this.EIDRTypeFindBox.PerformLayout();
            this.SupplementaryDeclarationDueDateDateEdit.ResumeLayout(true);
            this.SupplementaryDeclarationDueDateDateEdit.PerformLayout();
            this.CpcCodeFindBox.ResumeLayout(true);
            this.CpcCodeFindBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox InvoiceNumberTextBox;
		private ZArchitecture.GUI.ZDateEdit DateOfImportDateEdit;
		private ZArchitecture.ZTextBox AdditionalReferenceNumbersTextBox;
		private ZArchitecture.GUI.ZAddressControl WarehouseAddressControl;
		private ZArchitecture.ZTextBox DescriptionOfGoodsTextBox;
		private ZArchitecture.ZCalcEdit NumberOfPackagesCalcEdit;
		private ZArchitecture.GUI.ZDropEdit TypeOfPackagesDropEdit;
		private ZArchitecture.ZCalcEdit CustomsValueInGBPCalcEdit;
		private ZArchitecture.ZCalcEdit NetMassInKGCalcEdit;
		private ZArchitecture.GUI.ZAddressControl DeclarantAddressControl;
		private ZArchitecture.GUI.ZDropEdit TransportModeFindBox;
		private ZArchitecture.GUI.ZDropEdit CountryOfOriginFindBox;
		private ZArchitecture.GUI.ZDropEdit EIDRTypeFindBox;
		private ZArchitecture.GUI.ZDateEdit SupplementaryDeclarationDueDateDateEdit;
		private ZArchitecture.GUI.ZButton CreateEidrButton;
		private ZArchitecture.GUI.ZCodeFindBox CpcCodeFindBox;
	}
}
