namespace Enterprise.Customs.GB.GUI.Wizards
{
	partial class CdsFsdWizardForm
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
            Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            this.CreateCdsFsdButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.NumberOfYDeclarationsSubmittedCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.NumberOfYDeclarationsDueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.NumberOfZDeclarationsSubmittedCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.NumberOfZDeclarationsDueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.ImporterFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
            this.DeclarantAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
            this.AuthorisationHolderAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
            this.AuthorisationTypeFindBox = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.DueDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.lateDeclarationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.LateChildGridPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.LateChildGrid = new Enterprise.ZArchitecture.ZGrid();
            this.countTimelyEntriesButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.countLateEntriesButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.startOfPeriodDate = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.FindOnlyForDueDateCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.FindForImporterLinkedOrgCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.FindOnlyForAuthHolderCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.FindOnlyForAuthTypeCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.ImporterFindBox.SuspendLayout();
            this.DeclarantAddressControl.SuspendLayout();
            this.AuthorisationHolderAddressControl.SuspendLayout();
            this.AuthorisationTypeFindBox.SuspendLayout();
            this.DueDateDateEdit.SuspendLayout();
            this.lateDeclarationGroupBox.SuspendLayout();
            this.LateChildGridPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LateChildGrid)).BeginInit();
            this.LateChildGrid.SuspendLayout();
            this.startOfPeriodDate.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 543, true);
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(619, 22, true);
            this.MainStatusBar.TabIndex = 13;
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.CDS.CDSFinalSupplementaryDeclarationHelper);
            // 
            // CreateCdsFsdButton
            // 
            this.CreateCdsFsdButton.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("9d35ae90-6dc5-4a9e-b928-b1d1e8695456", "Create CDS FSD");
            this.CreateCdsFsdButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.CreateCdsFsdButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(231, 510, true);
            this.CreateCdsFsdButton.Name = "CreateCdsFsdButton";
            this.CreateCdsFsdButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 23, true);
            this.CreateCdsFsdButton.TabIndex = 25;
            this.CreateCdsFsdButton.ToolTipCaption = null;
            this.CreateCdsFsdButton.UseVisualStyleBackColor = true;
            this.CreateCdsFsdButton.Click += new System.EventHandler(this.ButtonCreateEidr_Click);
            // 
            // NumberOfYDeclarationsSubmittedCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.NumberOfYDeclarationsSubmittedCalcEdit, "NumberOfTypeYDeclarationsSubmitted");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.GB.CDS.CDSFinalSupplementaryDeclarationHelper)(null)).NumberOfTypeYDeclarationsSubmitted)));
            this.NumberOfYDeclarationsSubmittedCalcEdit.DecimalPlaces = 0;
            this.NumberOfYDeclarationsSubmittedCalcEdit.Decimals = 0;
            this.NumberOfYDeclarationsSubmittedCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(231, 252, true);
            this.NumberOfYDeclarationsSubmittedCalcEdit.Name = "NumberOfYDeclarationsSubmittedCalcEdit";
            this.NumberOfYDeclarationsSubmittedCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(58, 20, true);
            this.NumberOfYDeclarationsSubmittedCalcEdit.TabIndex = 12;
            this.NumberOfYDeclarationsSubmittedCalcEdit.Text = "0";
            this.NumberOfYDeclarationsSubmittedCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.NumberOfYDeclarationsSubmittedCalcEdit.TrackDisposedAccess = true;
            // 
            // NumberOfYDeclarationsDueCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.NumberOfYDeclarationsDueCalcEdit, "NumberOfTypeYDeclarationsDue");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.GB.CDS.CDSFinalSupplementaryDeclarationHelper)(null)).NumberOfTypeYDeclarationsDue)));
            this.NumberOfYDeclarationsDueCalcEdit.DecimalPlaces = 0;
            this.NumberOfYDeclarationsDueCalcEdit.Decimals = 0;
            this.NumberOfYDeclarationsDueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(231, 278, true);
            this.NumberOfYDeclarationsDueCalcEdit.Name = "NumberOfYDeclarationsDueCalcEdit";
            this.NumberOfYDeclarationsDueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(58, 20, true);
            this.NumberOfYDeclarationsDueCalcEdit.TabIndex = 14;
            this.NumberOfYDeclarationsDueCalcEdit.Text = "0";
            this.NumberOfYDeclarationsDueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.NumberOfYDeclarationsDueCalcEdit.TrackDisposedAccess = true;
            // 
            // NumberOfZDeclarationsSubmittedCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.NumberOfZDeclarationsSubmittedCalcEdit, "NumberOfTypeZDeclarationsSubmitted");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.GB.CDS.CDSFinalSupplementaryDeclarationHelper)(null)).NumberOfTypeZDeclarationsSubmitted)));
            this.NumberOfZDeclarationsSubmittedCalcEdit.DecimalPlaces = 0;
            this.NumberOfZDeclarationsSubmittedCalcEdit.Decimals = 0;
            this.NumberOfZDeclarationsSubmittedCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(231, 304, true);
            this.NumberOfZDeclarationsSubmittedCalcEdit.Name = "NumberOfZDeclarationsSubmittedCalcEdit";
            this.NumberOfZDeclarationsSubmittedCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(58, 20, true);
            this.NumberOfZDeclarationsSubmittedCalcEdit.TabIndex = 16;
            this.NumberOfZDeclarationsSubmittedCalcEdit.Text = "0";
            this.NumberOfZDeclarationsSubmittedCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.NumberOfZDeclarationsSubmittedCalcEdit.TrackDisposedAccess = true;
            // 
            // NumberOfZDeclarationsDueCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.NumberOfZDeclarationsDueCalcEdit, "NumberOfTypeZDeclarationsDue");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.GB.CDS.CDSFinalSupplementaryDeclarationHelper)(null)).NumberOfTypeZDeclarationsDue)));
            this.NumberOfZDeclarationsDueCalcEdit.DecimalPlaces = 0;
            this.NumberOfZDeclarationsDueCalcEdit.Decimals = 0;
            this.NumberOfZDeclarationsDueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(231, 330, true);
            this.NumberOfZDeclarationsDueCalcEdit.Name = "NumberOfZDeclarationsDueCalcEdit";
            this.NumberOfZDeclarationsDueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(58, 20, true);
            this.NumberOfZDeclarationsDueCalcEdit.TabIndex = 18;
            this.NumberOfZDeclarationsDueCalcEdit.Text = "0";
            this.NumberOfZDeclarationsDueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.NumberOfZDeclarationsDueCalcEdit.TrackDisposedAccess = true;
            // 
            // ImporterFindBox
            // 
            this.ImporterFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ImporterFindBox, "ImporterPK");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.GB.CDS.CDSFinalSupplementaryDeclarationHelper)(null)).ImporterPK)));
            this.ImporterFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(231, 70, true);
            this.ImporterFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
            this.ImporterFindBox.Name = "ImporterFindBox";
            this.ImporterFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.ImporterFindBox.ParentType = null;
            this.ImporterFindBox.ShowDescriptionBox = false;
            this.ImporterFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
            this.ImporterFindBox.TabIndex = 5;
            // 
            // DeclarantAddressControl
            // 
            this.DeclarantAddressControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.DeclarantAddressControl, "DeclarantAddressPK");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.GB.CDS.CDSFinalSupplementaryDeclarationHelper)(null)).DeclarantAddressPK)));
            this.DeclarantAddressControl.BindToOrgList = "Lookups+DeclarantList";
            this.DeclarantAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(231, 96, true);
            this.DeclarantAddressControl.Name = "DeclarantAddressControl";
            this.DeclarantAddressControl.PopupCaption = "";
            this.DeclarantAddressControl.ShowAddress = false;
            this.DeclarantAddressControl.ShowOrganisationName = true;
            this.DeclarantAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 20, true);
            this.DeclarantAddressControl.TabIndex = 6;
            // 
            // AuthorisationHolderAddressControl
            // 
            this.AuthorisationHolderAddressControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.AuthorisationHolderAddressControl, "AuthorisationHolderAddressPK");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.GB.CDS.CDSFinalSupplementaryDeclarationHelper)(null)).AuthorisationHolderAddressPK)));
            this.AuthorisationHolderAddressControl.BindToOrgList = "Lookups+AuthorisationHolderList";
            this.AuthorisationHolderAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(231, 122, true);
            this.AuthorisationHolderAddressControl.Name = "AuthorisationHolderAddressControl";
            this.AuthorisationHolderAddressControl.PopupCaption = "";
            this.AuthorisationHolderAddressControl.ShowAddress = false;
            this.AuthorisationHolderAddressControl.ShowOrganisationName = true;
            this.AuthorisationHolderAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 20, true);
            this.AuthorisationHolderAddressControl.TabIndex = 8;
            // 
            // AuthorisationTypeFindBox
            // 
            this.AuthorisationTypeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.AuthorisationTypeFindBox, "AuthorisationType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.CDS.CDSFinalSupplementaryDeclarationHelper)(null)).AuthorisationType)));
            this.AuthorisationTypeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(231, 200, true);
            this.AuthorisationTypeFindBox.Name = "AuthorisationTypeFindBox";
            this.AuthorisationTypeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
            this.AuthorisationTypeFindBox.TabIndex = 11;
            // 
            // DueDateDateEdit
            // 
            this.DueDateDateEdit.AllowDrop = true;
            this.DueDateDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.DueDateDateEdit, "DueDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.CDS.CDSFinalSupplementaryDeclarationHelper)(null)).DueDate)));
            this.DueDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(231, 18, true);
            this.DueDateDateEdit.Name = "DueDateDateEdit";
            this.DueDateDateEdit.TabIndex = 2;
            // 
            // lateDeclarationGroupBox
            // 
            this.lateDeclarationGroupBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("7d88a824-cc3c-4fca-83b6-3af76f7584a3", "Late Declarations");
            this.lateDeclarationGroupBox.Controls.Add(this.LateChildGridPanel);
            this.lateDeclarationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(57, 356, true);
            this.lateDeclarationGroupBox.Name = "lateDeclarationGroupBox";
            this.lateDeclarationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(456, 146, true);
            this.lateDeclarationGroupBox.TabIndex = 20;
            this.lateDeclarationGroupBox.TabStop = false;
            // 
            // LateChildGridPanel
            // 
            this.LateChildGridPanel.Controls.Add(this.LateChildGrid);
            this.LateChildGridPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 23, true);
            this.LateChildGridPanel.Name = "LateChildGridPanel";
            this.LateChildGridPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(408, 100, true);
            this.LateChildGridPanel.TabIndex = 24;
            // 
            // LateChildGrid
            // 
            this.LateChildGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.LateChildGrid, "CDSFinalSupplementaryDeclarationHelperLateChildCollection");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.GB.CDS.CDSFinalSupplementaryDeclarationHelper)(null)).CDSFinalSupplementaryDeclarationHelperLateChildCollection)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.GB.CDS.CDSFinalSupplementaryDeclarationHelperLateChild)(((System.Collections.IList)(((Enterprise.Customs.GB.CDS.CDSFinalSupplementaryDeclarationHelper)(null)).CDSFinalSupplementaryDeclarationHelperLateChildCollection)).SyncRoot)).DueDate)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.GB.CDS.CDSFinalSupplementaryDeclarationHelperLateChild)(((System.Collections.IList)(((Enterprise.Customs.GB.CDS.CDSFinalSupplementaryDeclarationHelper)(null)).CDSFinalSupplementaryDeclarationHelperLateChildCollection)).SyncRoot)).NumberOfTypeZDeclarations)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.GB.CDS.CDSFinalSupplementaryDeclarationHelperLateChild)(((System.Collections.IList)(((Enterprise.Customs.GB.CDS.CDSFinalSupplementaryDeclarationHelper)(null)).CDSFinalSupplementaryDeclarationHelperLateChildCollection)).SyncRoot)).NumberOfTypeYDeclarations)));
            this.LateChildGrid.CaptionVisible = false;
            zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("5cbc4225-dca8-4e11-8a57-471e039aa35c", "Due Date");
            zDateEditColumnStyleInfo1.ColumnName = "DueDate";
            zDateEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("1aaf32fb-6d73-4426-a2ba-f62a24a0cf1b", "No.of Z Declarations");
            zCalcEditColumnStyleInfo1.ColumnName = "NumberOfTypeZDeclarations";
            zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("deb200a5-c73e-4735-8aed-4f4f45a784b7", "No.of Y Declarations");
            zCalcEditColumnStyleInfo2.ColumnName = "NumberOfTypeYDeclarations";
            zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            this.LateChildGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
            this.LateChildGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
            this.LateChildGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
            this.LateChildGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LateChildGrid.GridId = "2bf9d473-38e3-4eca-a827-57f92058c280";
            this.LateChildGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.LateChildGrid.LayoutKey = "LateChildGrid";
            this.LateChildGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.LateChildGrid.Name = "LateChildGrid";
            this.LateChildGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(408, 100, true);
            this.LateChildGrid.TabIndex = 20;
            // 
            // countTimelyEntriesButton
            // 
            this.countTimelyEntriesButton.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("af29a6b3-3969-42c6-9ec4-09476c67708c", "Count Timely Entries");
            this.countTimelyEntriesButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(332, 264, true);
            this.countTimelyEntriesButton.Name = "countTimelyEntriesButton";
            this.countTimelyEntriesButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 23, true);
            this.countTimelyEntriesButton.TabIndex = 26;
            this.countTimelyEntriesButton.TabStop = false;
            this.countTimelyEntriesButton.ToolTipCaption = null;
            this.countTimelyEntriesButton.UseVisualStyleBackColor = true;
            this.countTimelyEntriesButton.Click += new System.EventHandler(this.countTimelyEntriesButton_Click);
            // 
            // countLateEntriesButton
            // 
            this.countLateEntriesButton.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("eccf6007-ae7d-4b22-81a4-459037d11bcf", "Count Late Entries");
            this.countLateEntriesButton.Enabled = false;
            this.countLateEntriesButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(332, 304, true);
            this.countLateEntriesButton.Name = "countLateEntriesButton";
            this.countLateEntriesButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 23, true);
            this.countLateEntriesButton.TabIndex = 27;
            this.countLateEntriesButton.TabStop = false;
            this.countLateEntriesButton.ToolTipCaption = null;
            this.countLateEntriesButton.UseVisualStyleBackColor = true;
            // 
            // startOfPeriodDate
            // 
            this.startOfPeriodDate.AllowDrop = true;
            this.startOfPeriodDate.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.startOfPeriodDate, "StartOfPeriod");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.CDS.CDSFinalSupplementaryDeclarationHelper)(null)).StartOfPeriod)));
            this.startOfPeriodDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(427, 18, true);
            this.startOfPeriodDate.Name = "startOfPeriodDate";
            this.startOfPeriodDate.TabIndex = 3;
            // 
            // FindOnlyForDueDateCheckBox
            // 
            this.FindOnlyForDueDateCheckBox.AutoSize = true;
            this.BindingSource.SetBindingMember(this.FindOnlyForDueDateCheckBox, "FindOnlyForDueDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.GB.CDS.CDSFinalSupplementaryDeclarationHelper)(null)).FindOnlyForDueDate)));
            this.FindOnlyForDueDateCheckBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("3af2b5ab-e75f-4153-a2f7-dcc717450a5b", "Find only declarations with this due date");
            this.FindOnlyForDueDateCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(231, 44, true);
            this.FindOnlyForDueDateCheckBox.Name = "FindOnlyForDueDateCheckBox";
            this.FindOnlyForDueDateCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(214, 17, true);
            this.FindOnlyForDueDateCheckBox.TabIndex = 4;
            this.FindOnlyForDueDateCheckBox.UseVisualStyleBackColor = true;
            // 
            // FindForImporterLinkedOrgCheckBox
            // 
            this.FindForImporterLinkedOrgCheckBox.AutoSize = true;
            this.BindingSource.SetBindingMember(this.FindForImporterLinkedOrgCheckBox, "FindForImporterLinkedOrg");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.GB.CDS.CDSFinalSupplementaryDeclarationHelper)(null)).FindForImporterLinkedOrg)));
            this.FindForImporterLinkedOrgCheckBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("964f8d5d-bb0b-4c80-801c-9a936a16a202", "Also find declarations for linked importer organizations");
            this.FindForImporterLinkedOrgCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(231, 148, true);
            this.FindForImporterLinkedOrgCheckBox.Name = "FindForImporterLinkedOrgCheckBox";
            this.FindForImporterLinkedOrgCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(277, 17, true);
            this.FindForImporterLinkedOrgCheckBox.TabIndex = 9;
            this.FindForImporterLinkedOrgCheckBox.UseVisualStyleBackColor = true;
            // 
            // FindOnlyForAuthHolderCheckBox
            // 
            this.FindOnlyForAuthHolderCheckBox.AutoSize = true;
            this.BindingSource.SetBindingMember(this.FindOnlyForAuthHolderCheckBox, "FindOnlyForAuthHolder");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.GB.CDS.CDSFinalSupplementaryDeclarationHelper)(null)).FindOnlyForAuthHolder)));
            this.FindOnlyForAuthHolderCheckBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("3b37de16-cc2f-4330-a947-4302c53ab20c", "Find only entries with this authorization holder");
            this.FindOnlyForAuthHolderCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(231, 174, true);
            this.FindOnlyForAuthHolderCheckBox.Name = "FindOnlyForAuthHolderCheckBox";
            this.FindOnlyForAuthHolderCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 17, true);
            this.FindOnlyForAuthHolderCheckBox.TabIndex = 10;
            this.FindOnlyForAuthHolderCheckBox.UseVisualStyleBackColor = true;
            // 
            // FindOnlyForAuthTypeCheckBox
            // 
            this.FindOnlyForAuthTypeCheckBox.AutoSize = true;
            this.BindingSource.SetBindingMember(this.FindOnlyForAuthTypeCheckBox, "FindOnlyForAuthType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.GB.CDS.CDSFinalSupplementaryDeclarationHelper)(null)).FindOnlyForAuthType)));
            this.FindOnlyForAuthTypeCheckBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("8558d0d8-6b7e-4d16-af63-28d85a1b6f04", "Find only entries with this authorization type");
            this.FindOnlyForAuthTypeCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(231, 226, true);
            this.FindOnlyForAuthTypeCheckBox.Name = "FindOnlyForAuthTypeCheckBox";
            this.FindOnlyForAuthTypeCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(229, 17, true);
            this.FindOnlyForAuthTypeCheckBox.TabIndex = 12;
            this.FindOnlyForAuthTypeCheckBox.UseVisualStyleBackColor = true;
            // 
            // CdsFsdWizardForm
            // 
            this.CaptionRenderingEnabled = true;
            this.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("0e5d20cf-ab73-48d1-b3b3-11f2fcdb126a", "CDS FSD Wizard");
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(619, 565, true);
            this.Controls.Add(this.startOfPeriodDate);
            this.Controls.Add(this.countLateEntriesButton);
            this.Controls.Add(this.countTimelyEntriesButton);
            this.Controls.Add(this.lateDeclarationGroupBox);
            this.Controls.Add(this.FindOnlyForAuthTypeCheckBox);
            this.Controls.Add(this.FindOnlyForAuthHolderCheckBox);
            this.Controls.Add(this.FindForImporterLinkedOrgCheckBox);
            this.Controls.Add(this.FindOnlyForDueDateCheckBox);
            this.Controls.Add(this.DueDateDateEdit);
            this.Controls.Add(this.AuthorisationTypeFindBox);
            this.Controls.Add(this.AuthorisationHolderAddressControl);
            this.Controls.Add(this.DeclarantAddressControl);
            this.Controls.Add(this.ImporterFindBox);
            this.Controls.Add(this.NumberOfZDeclarationsDueCalcEdit);
            this.Controls.Add(this.NumberOfZDeclarationsSubmittedCalcEdit);
            this.Controls.Add(this.NumberOfYDeclarationsDueCalcEdit);
            this.Controls.Add(this.NumberOfYDeclarationsSubmittedCalcEdit);
            this.Controls.Add(this.CreateCdsFsdButton);
            this.DataSourceType = typeof(Enterprise.Customs.GB.CDS.CDSFinalSupplementaryDeclarationHelper);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "CdsFsdWizardForm";
            this.Controls.SetChildIndex(this.CreateCdsFsdButton, 0);
            this.Controls.SetChildIndex(this.NumberOfYDeclarationsSubmittedCalcEdit, 0);
            this.Controls.SetChildIndex(this.NumberOfYDeclarationsDueCalcEdit, 0);
            this.Controls.SetChildIndex(this.NumberOfZDeclarationsSubmittedCalcEdit, 0);
            this.Controls.SetChildIndex(this.NumberOfZDeclarationsDueCalcEdit, 0);
            this.Controls.SetChildIndex(this.ImporterFindBox, 0);
            this.Controls.SetChildIndex(this.DeclarantAddressControl, 0);
            this.Controls.SetChildIndex(this.AuthorisationHolderAddressControl, 0);
            this.Controls.SetChildIndex(this.AuthorisationTypeFindBox, 0);
            this.Controls.SetChildIndex(this.DueDateDateEdit, 0);
            this.Controls.SetChildIndex(this.FindOnlyForDueDateCheckBox, 0);
            this.Controls.SetChildIndex(this.FindForImporterLinkedOrgCheckBox, 0);
            this.Controls.SetChildIndex(this.FindOnlyForAuthHolderCheckBox, 0);
            this.Controls.SetChildIndex(this.FindOnlyForAuthTypeCheckBox, 0);
            this.Controls.SetChildIndex(this.lateDeclarationGroupBox, 0);
            this.Controls.SetChildIndex(this.MainStatusBar, 0);
            this.Controls.SetChildIndex(this.countTimelyEntriesButton, 0);
            this.Controls.SetChildIndex(this.countLateEntriesButton, 0);
            this.Controls.SetChildIndex(this.startOfPeriodDate, 0);
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ImporterFindBox.ResumeLayout(true);
            this.ImporterFindBox.PerformLayout();
            this.DeclarantAddressControl.ResumeLayout(true);
            this.DeclarantAddressControl.PerformLayout();
            this.AuthorisationHolderAddressControl.ResumeLayout(true);
            this.AuthorisationHolderAddressControl.PerformLayout();
            this.AuthorisationTypeFindBox.ResumeLayout(true);
            this.AuthorisationTypeFindBox.PerformLayout();
            this.DueDateDateEdit.ResumeLayout(true);
            this.DueDateDateEdit.PerformLayout();
            this.lateDeclarationGroupBox.ResumeLayout(false);
            this.lateDeclarationGroupBox.PerformLayout();
            this.LateChildGridPanel.ResumeLayout(false);
            this.LateChildGridPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LateChildGrid)).EndInit();
            this.LateChildGrid.ResumeLayout(false);
            this.LateChildGrid.PerformLayout();
            this.startOfPeriodDate.ResumeLayout(true);
            this.startOfPeriodDate.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZCalcEdit NumberOfYDeclarationsSubmittedCalcEdit;
		private ZArchitecture.ZCalcEdit NumberOfYDeclarationsDueCalcEdit;
		private ZArchitecture.ZCalcEdit NumberOfZDeclarationsSubmittedCalcEdit;
		private ZArchitecture.ZCalcEdit NumberOfZDeclarationsDueCalcEdit;
		private MasterFiles.GUI.ZOrganisationFindBox ImporterFindBox;
		private ZArchitecture.GUI.ZAddressControl DeclarantAddressControl;
		private ZArchitecture.GUI.ZAddressControl AuthorisationHolderAddressControl;
		private ZArchitecture.GUI.ZDropEdit AuthorisationTypeFindBox;
		private ZArchitecture.GUI.ZDateEdit DueDateDateEdit;
		private ZArchitecture.GUI.ZButton CreateCdsFsdButton;
		private ZArchitecture.GUI.ZPanel LateChildGridPanel;
		private ZArchitecture.ZGrid LateChildGrid;
		private Enterprise.ZArchitecture.GUI.ZGroupBox lateDeclarationGroupBox;
		private ZArchitecture.GUI.ZButton countTimelyEntriesButton;
		private ZArchitecture.GUI.ZButton countLateEntriesButton;
		private ZArchitecture.GUI.ZDateEdit startOfPeriodDate;

		private ZArchitecture.GUI.ZCheckBox FindOnlyForDueDateCheckBox;
		private ZArchitecture.GUI.ZCheckBox FindForImporterLinkedOrgCheckBox;
		private ZArchitecture.GUI.ZCheckBox FindOnlyForAuthHolderCheckBox;
		private ZArchitecture.GUI.ZCheckBox FindOnlyForAuthTypeCheckBox;
	}
}
