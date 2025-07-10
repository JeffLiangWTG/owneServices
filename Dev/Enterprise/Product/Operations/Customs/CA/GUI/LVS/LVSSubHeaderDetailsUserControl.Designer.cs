namespace Enterprise.Customs.CA.GUI
{
	partial class LVSSubHeaderDetailsUserControl
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
			this.SplitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			this.LVSIdentifierTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ImporterOrganisationControl = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
			this.SupplierDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.JZ_InvoiceAmountBoundCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
			this.TotalValueForDutyCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.OtherReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.IsReadyForConsolidationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SplitContainer2 = new CargoWise.Windows.UI.KSplitContainer();
			this.PortOfClearanceCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.LastPortDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ReleaseDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.CountryOfOriginCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.USStateOfOriginDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TreatmentCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.JZ_InvoiceDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.TimeLimitUnitDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TimeLimitCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.JZ_IncoTermBoundDropDownEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.IncoTermExplainButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CountryOfExportCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.USStateOfExportDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.LVSCarrierCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer1)).BeginInit();
			this.SplitContainer1.Panel1.SuspendLayout();
			this.SplitContainer1.Panel2.SuspendLayout();
			this.SplitContainer1.SuspendLayout();
			this.ImporterOrganisationControl.SuspendLayout();
			this.SupplierDocAddressControl.SuspendLayout();
			this.JZ_InvoiceAmountBoundCurrencyControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer2)).BeginInit();
			this.SplitContainer2.Panel1.SuspendLayout();
			this.SplitContainer2.Panel2.SuspendLayout();
			this.SplitContainer2.SuspendLayout();
			this.PortOfClearanceCodeFindBox.SuspendLayout();
			this.LastPortDateEdit.SuspendLayout();
			this.ReleaseDateEdit.SuspendLayout();
			this.CountryOfOriginCodeFindBox.SuspendLayout();
			this.USStateOfOriginDropEdit.SuspendLayout();
			this.TreatmentCodeDropEdit.SuspendLayout();
			this.JZ_InvoiceDateEdit.SuspendLayout();
			this.TimeLimitUnitDropEdit.SuspendLayout();
			this.JZ_IncoTermBoundDropDownEdit.SuspendLayout();
			this.CountryOfExportCodeFindBox.SuspendLayout();
			this.USStateOfExportDropEdit.SuspendLayout();
			this.LVSCarrierCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.JobComInvoiceHeader);
			// 
			// SplitContainer1
			// 
			this.SplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitContainer1.IsSplitterFixed = true;
			this.SplitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SplitContainer1.Name = "SplitContainer1";
			// 
			// SplitContainer1.Panel1
			// 
			this.SplitContainer1.Panel1.Controls.Add(this.LVSIdentifierTextBox);
			this.SplitContainer1.Panel1.Controls.Add(this.ImporterOrganisationControl);
			this.SplitContainer1.Panel1.Controls.Add(this.SupplierDocAddressControl);
			this.SplitContainer1.Panel1.Controls.Add(this.JZ_InvoiceAmountBoundCurrencyControl);
			this.SplitContainer1.Panel1.Controls.Add(this.TotalValueForDutyCalcEdit);
			this.SplitContainer1.Panel1.Controls.Add(this.OtherReferenceTextBox);
			this.SplitContainer1.Panel1.Controls.Add(this.IsReadyForConsolidationCheckBox);
			// 
			// SplitContainer1.Panel2
			// 
			this.SplitContainer1.Panel2.Controls.Add(this.SplitContainer2);
			this.SplitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1053, 176, true);
			this.SplitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(401);
			this.SplitContainer1.TabIndex = 0;
			// 
			// LVSIdentifierTextBox
			// 
			this.LVSIdentifierTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.LVSIdentifierTextBox, "JZ_InvoiceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(null)).JZ_InvoiceNumber)));
			this.LVSIdentifierTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("LVSSubHeaderDetailsUserControl|8f712317-6138-4415-8535-9a57afc2cc2f", "LVS ID/Inv. #");
			this.LVSIdentifierTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 1, true);
			this.LVSIdentifierTextBox.Name = "LVSIdentifierTextBox";
			this.LVSIdentifierTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(292, 20, true);
			this.LVSIdentifierTextBox.TabIndex = 1;
			// 
			// ImporterOrganisationControl
			// 
			this.ImporterOrganisationControl.AllowDrop = true;
			this.ImporterOrganisationControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ImporterOrganisationControl, "JZ_OH_Buyer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(null)).JZ_OH_Buyer)));
			this.ImporterOrganisationControl.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("LVSSubHeaderDetailsUserControl|E003A98D-C69F-437a-8F8F-5B708F43F0D0", "Importer");
			this.ImporterOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 23, true);
			this.ImporterOrganisationControl.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.ImporterOrganisationControl.Name = "ImporterOrganisationControl";
			this.ImporterOrganisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(292, 20, true);
			this.ImporterOrganisationControl.TabIndex = 2;
			// 
			// SupplierDocAddressControl
			// 
			this.SupplierDocAddressControl.AllowDrop = true;
			this.SupplierDocAddressControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SupplierDocAddressControl, "SupplierDocumentaryAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(null)).SupplierDocumentaryAddress)));
			this.SupplierDocAddressControl.BindToOrganisations = "Lookups.SuppliersList";
			this.SupplierDocAddressControl.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("LVSSubHeaderDetailsUserControl|5dda7351-a453-4802-99d6-6a31364ad659", "Vendor");
			this.SupplierDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
			this.SupplierDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 45, true);
			this.SupplierDocAddressControl.Name = "SupplierDocAddressControl";
			this.SupplierDocAddressControl.ReadOnly = false;
			this.SupplierDocAddressControl.SingleLineNoGroupBoxPanelWidth = 292;
			this.SupplierDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(292, 20, true);
			this.SupplierDocAddressControl.TabIndex = 3;
			this.SupplierDocAddressControl.ValidationJustForced = false;
			// 
			// JZ_InvoiceAmountBoundCurrencyControl
			// 
			this.JZ_InvoiceAmountBoundCurrencyControl.AllowDrop = true;
			this.JZ_InvoiceAmountBoundCurrencyControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.JZ_InvoiceAmountBoundCurrencyControl.BindToAmount = "JZ_InvoiceAmount";
			this.JZ_InvoiceAmountBoundCurrencyControl.BindToUnit = "JZ_RX_NKInvoice_Currency";
			this.JZ_InvoiceAmountBoundCurrencyControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.JZ_InvoiceAmountBoundCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 67, true);
			this.JZ_InvoiceAmountBoundCurrencyControl.Name = "JZ_InvoiceAmountBoundCurrencyControl";
			this.JZ_InvoiceAmountBoundCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
			this.JZ_InvoiceAmountBoundCurrencyControl.TabIndex = 4;
			// 
			// TotalValueForDutyCalcEdit
			// 
			this.TotalValueForDutyCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TotalValueForDutyCalcEdit, "TotalValueForDuty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(null)).TotalValueForDuty)));
			this.TotalValueForDutyCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("1e3afd61-9603-4881-955c-820e82dc0628", "Total VFD");
			this.TotalValueForDutyCalcEdit.DecimalPlaces = 2;
			this.TotalValueForDutyCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(348, 67, true);
			this.TotalValueForDutyCalcEdit.Name = "TotalValueForDutyCalcEdit";
			this.TotalValueForDutyCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(49, 20, true);
			this.TotalValueForDutyCalcEdit.TabIndex = 5;
			this.TotalValueForDutyCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OtherReferenceTextBox
			// 
			this.OtherReferenceTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.OtherReferenceTextBox, "CA_OtherReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(null)).CA_OtherReference)));
			this.OtherReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 89, true);
			this.OtherReferenceTextBox.Multiline = true;
			this.OtherReferenceTextBox.Name = "OtherReferenceTextBox";
			this.OtherReferenceTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.OtherReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(292, 20, true);
			this.OtherReferenceTextBox.TabIndex = 6;
			// 
			// IsReadyForConsolidationCheckBox
			// 
			this.IsReadyForConsolidationCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsReadyForConsolidationCheckBox, "CA_ReadyForConsolidation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(null)).CA_ReadyForConsolidation)));
			this.IsReadyForConsolidationCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("0353255E-EF4D-4457-9320-88D359C3372B", "Ready for Consolidation?");
			this.IsReadyForConsolidationCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsReadyForConsolidationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 111, true);
			this.IsReadyForConsolidationCheckBox.Name = "IsReadyForConsolidationCheckBox";
			this.IsReadyForConsolidationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(134, 17, true);
			this.IsReadyForConsolidationCheckBox.TabIndex = 7;
			this.IsReadyForConsolidationCheckBox.UseVisualStyleBackColor = true;
			this.IsReadyForConsolidationCheckBox.CheckState = System.Windows.Forms.CheckState.Unchecked;
			this.IsReadyForConsolidationCheckBox.Click += new System.EventHandler(this.IsReadyForConsolidationCheckBox_Click);
			// 
			// SplitContainer2
			// 
			this.SplitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitContainer2.IsSplitterFixed = true;
			this.SplitContainer2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SplitContainer2.Name = "SplitContainer2";
			// 
			// SplitContainer2.Panel1
			// 
			this.SplitContainer2.Panel1.Controls.Add(this.PortOfClearanceCodeFindBox);
			this.SplitContainer2.Panel1.Controls.Add(this.LastPortDateEdit);
			this.SplitContainer2.Panel1.Controls.Add(this.ReleaseDateEdit);
			this.SplitContainer2.Panel1.Controls.Add(this.CountryOfOriginCodeFindBox);
			this.SplitContainer2.Panel1.Controls.Add(this.USStateOfOriginDropEdit);
			this.SplitContainer2.Panel1.Controls.Add(this.TreatmentCodeDropEdit);
			// 
			// SplitContainer2.Panel2
			// 
			this.SplitContainer2.Panel2.AutoScroll = true;
			this.SplitContainer2.Panel2.Controls.Add(this.JZ_InvoiceDateEdit);
			this.SplitContainer2.Panel2.Controls.Add(this.TimeLimitUnitDropEdit);
			this.SplitContainer2.Panel2.Controls.Add(this.TimeLimitCalcEdit);
			this.SplitContainer2.Panel2.Controls.Add(this.JZ_IncoTermBoundDropDownEdit);
			this.SplitContainer2.Panel2.Controls.Add(this.IncoTermExplainButton);
			this.SplitContainer2.Panel2.Controls.Add(this.CountryOfExportCodeFindBox);
			this.SplitContainer2.Panel2.Controls.Add(this.USStateOfExportDropEdit);
			this.SplitContainer2.Panel2.Controls.Add(this.LVSCarrierCodeFindBox);
			this.SplitContainer2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(648, 176, true);
			this.SplitContainer2.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(312);
			this.SplitContainer2.TabIndex = 0;
			// 
			// PortOfClearanceCodeFindBox
			// 
			this.PortOfClearanceCodeFindBox.AllowDrop = true;
			this.PortOfClearanceCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PortOfClearanceCodeFindBox, "CA_PortOfClearance");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(null)).CA_PortOfClearance)));
			this.PortOfClearanceCodeFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("LVSSubHeaderDetailsUserControl|309e4dd8-600f-45e3-b5db-23eea14c8efa", "Port of Clearance");
			this.PortOfClearanceCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 1, true);
			this.PortOfClearanceCodeFindBox.Name = "PortOfClearanceCodeFindBox";
			this.PortOfClearanceCodeFindBox.PreBoundMaxLength = 4;
			this.PortOfClearanceCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(198, 20, true);
			this.PortOfClearanceCodeFindBox.TabIndex = 7;
			// 
			// LastPortDateEdit
			// 
			this.LastPortDateEdit.AllowDrop = true;
			this.LastPortDateEdit.AutoCompleteMonthThreshold = 1;
			this.LastPortDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.LastPortDateEdit, "JZ_ValuationDateOverride");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(null)).JZ_ValuationDateOverride)));
			this.LastPortDateEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("LVSSubHeaderDetailsUserControl|1672d534-d0e5-4a96-8f29-4fa5327aef01", "Date", "Direct Ship. Date", "Date of Direct Shipment", "The date (per each invoice) the goods began their continuous journey to Canada.");
			this.LastPortDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 23, true);
			this.LastPortDateEdit.Name = "LastPortDateEdit";
			this.LastPortDateEdit.TabIndex = 8;
			// 
			// ReleaseDateEdit
			// 
			this.ReleaseDateEdit.AllowDrop = true;
			this.ReleaseDateEdit.AutoCompleteMonthThreshold = 1;
			this.ReleaseDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ReleaseDateEdit, "JobDeclaration.JE_EntryAuthorisationDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.ReleaseDateEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("LVSSubHeaderDetailsUserControl|9296DD24-247D-4249-8D8D-714D7B411068", "Release", "Release Date", "Date of Direct Shipment");
			this.ReleaseDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 45, true);
			this.ReleaseDateEdit.Name = "ReleaseDateEdit";
			this.ReleaseDateEdit.TabIndex = 9;
			// 
			// CountryOfOriginCodeFindBox
			// 
			this.CountryOfOriginCodeFindBox.AllowDrop = true;
			this.CountryOfOriginCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CountryOfOriginCodeFindBox, "JZ_RN_NKDefaultOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(null)).JZ_RN_NKDefaultOrigin)));
			this.CountryOfOriginCodeFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("LVSSubHeaderDetailsUserControl|8d00c95d-5c7b-45b7-b078-c980c5c09e5a", "Ctry/Rgn. of Origin", "Country/Region of Origin", "Default Country/Region of Origin", "The country/region where the goods are grown, produced or manufactured.");
			this.CountryOfOriginCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 67, true);
			this.CountryOfOriginCodeFindBox.Name = "CountryOfOriginCodeFindBox";
			this.CountryOfOriginCodeFindBox.PreBoundMaxLength = 2;
			this.CountryOfOriginCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(198, 20, true);
			this.CountryOfOriginCodeFindBox.TabIndex = 10;
			// 
			// USStateOfOriginDropEdit
			// 
			this.USStateOfOriginDropEdit.AllowDrop = true;
			this.USStateOfOriginDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.USStateOfOriginDropEdit, "JZ_RW_NKOriginState");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(null)).JZ_RW_NKOriginState)));
			this.USStateOfOriginDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 89, true);
			this.USStateOfOriginDropEdit.Name = "USStateOfOriginDropEdit";
			this.USStateOfOriginDropEdit.PreBoundMaxLength = 2;
			this.USStateOfOriginDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(198, 20, true);
			this.USStateOfOriginDropEdit.TabIndex = 11;
			// 
			// TreatmentCodeDropEdit
			// 
			this.TreatmentCodeDropEdit.AllowDrop = true;
			this.TreatmentCodeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TreatmentCodeDropEdit, "CA_TreatmentCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(null)).CA_TreatmentCode)));
			this.TreatmentCodeDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("LVSSubHeaderDetailsUserControl|b169353b-bea7-4563-9ba9-92ea7acd2064", "Treatment Code", "A means by which normal rates of duty may be modified according to the Customs Tariff. Refer to the Customs Tariff for information on the applicability of these tariff treatments.");
			this.TreatmentCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 111, true);
			this.TreatmentCodeDropEdit.Name = "TreatmentCodeDropEdit";
			this.TreatmentCodeDropEdit.PreBoundMaxLength = 2;
			this.TreatmentCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(198, 20, true);
			this.TreatmentCodeDropEdit.TabIndex = 12;
			// 
			// JZ_InvoiceDateEdit
			// 
			this.JZ_InvoiceDateEdit.AllowDrop = true;
			this.JZ_InvoiceDateEdit.AutoCompleteMonthThreshold = 1;
			this.JZ_InvoiceDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JZ_InvoiceDateEdit, "JZ_InvoiceDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(null)).JZ_InvoiceDate)));
			this.JZ_InvoiceDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 23, true);
			this.JZ_InvoiceDateEdit.Name = "JZ_InvoiceDateEdit";
			this.JZ_InvoiceDateEdit.TabIndex = 15;
			// 
			// TimeLimitUnitDropEdit
			// 
			this.TimeLimitUnitDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TimeLimitUnitDropEdit, "CA_TimeLimitCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(null)).CA_TimeLimitCode)));
			this.TimeLimitUnitDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(286, 23, true);
			this.TimeLimitUnitDropEdit.Name = "TimeLimitUnitDropEdit";
			this.TimeLimitUnitDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 20, true);
			this.TimeLimitUnitDropEdit.TabIndex = 17;
			// 
			// TimeLimitCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TimeLimitCalcEdit, "CA_TimeLimit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(null)).CA_TimeLimit)));
			this.TimeLimitCalcEdit.DecimalPlaces = 2;
			this.TimeLimitCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(231, 23, true);
			this.TimeLimitCalcEdit.Name = "TimeLimitCalcEdit";
			this.TimeLimitCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(49, 20, true);
			this.TimeLimitCalcEdit.TabIndex = 16;
			this.TimeLimitCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// JZ_IncoTermBoundDropDownEdit
			// 
			this.JZ_IncoTermBoundDropDownEdit.AllowDrop = true;
			this.JZ_IncoTermBoundDropDownEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.JZ_IncoTermBoundDropDownEdit, "JZ_IncoTerm");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(null)).JZ_IncoTerm)));
			this.JZ_IncoTermBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 1, true);
			this.JZ_IncoTermBoundDropDownEdit.Name = "JZ_IncoTermBoundDropDownEdit";
			this.JZ_IncoTermBoundDropDownEdit.PreBoundMaxLength = 3;
			this.JZ_IncoTermBoundDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.JZ_IncoTermBoundDropDownEdit.TabIndex = 13;
			// 
			// IncoTermExplainButton
			// 
			this.IncoTermExplainButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.IncoTermExplainButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(354, 1, true);
			this.IncoTermExplainButton.Name = "IncoTermExplainButton";
			this.IncoTermExplainButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.IncoTermExplainButton.TabIndex = 14;
			this.IncoTermExplainButton.Text = "...";
			this.IncoTermExplainButton.Click += new System.EventHandler(this.IncoTermExplainButton_Click);
			// 
			// CountryOfExportCodeFindBox
			// 
			this.CountryOfExportCodeFindBox.AllowDrop = true;
			this.CountryOfExportCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CountryOfExportCodeFindBox, "CA_RN_NKExport");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(null)).CA_RN_NKExport)));
			this.CountryOfExportCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 45, true);
			this.CountryOfExportCodeFindBox.Name = "CountryOfExportCodeFindBox";
			this.CountryOfExportCodeFindBox.PreBoundMaxLength = 2;
			this.CountryOfExportCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 20, true);
			this.CountryOfExportCodeFindBox.TabIndex = 18;
			// 
			// USStateOfExportDropEdit
			// 
			this.USStateOfExportDropEdit.AllowDrop = true;
			this.USStateOfExportDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.USStateOfExportDropEdit, "CA_USStateOfExport");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(null)).CA_USStateOfExport)));
			this.USStateOfExportDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 67, true);
			this.USStateOfExportDropEdit.Name = "USStateOfExportDropEdit";
			this.USStateOfExportDropEdit.PreBoundMaxLength = 2;
			this.USStateOfExportDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 20, true);
			this.USStateOfExportDropEdit.TabIndex = 19;
			// 
			// LVSCarrierCodeFindBox
			// 
			this.LVSCarrierCodeFindBox.AllowDrop = true;
			this.LVSCarrierCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.LVSCarrierCodeFindBox, "CA_LVSCarrier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(null)).CA_LVSCarrier)));
			this.LVSCarrierCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 89, true);
			this.LVSCarrierCodeFindBox.Name = "LVSCarrierCodeFindBox";
			this.LVSCarrierCodeFindBox.PreBoundMaxLength = 2;
			this.LVSCarrierCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 20, true);
			this.LVSCarrierCodeFindBox.TabIndex = 20;
			// 
			// LVSSubHeaderDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.SplitContainer1);
			this.CaptionRenderingEnabled = true;
			this.Name = "LVSSubHeaderDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1053, 176, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SplitContainer1.Panel1.ResumeLayout(false);
			this.SplitContainer1.Panel1.PerformLayout();
			this.SplitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer1)).EndInit();
			this.SplitContainer1.ResumeLayout(false);
			this.SplitContainer1.PerformLayout();
			this.ImporterOrganisationControl.ResumeLayout(true);
			this.ImporterOrganisationControl.PerformLayout();
			this.SupplierDocAddressControl.ResumeLayout(true);
			this.SupplierDocAddressControl.PerformLayout();
			this.JZ_InvoiceAmountBoundCurrencyControl.ResumeLayout(true);
			this.JZ_InvoiceAmountBoundCurrencyControl.PerformLayout();
			this.SplitContainer2.Panel1.ResumeLayout(false);
			this.SplitContainer2.Panel2.ResumeLayout(false);
			this.SplitContainer2.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer2)).EndInit();
			this.SplitContainer2.ResumeLayout(false);
			this.SplitContainer2.PerformLayout();
			this.PortOfClearanceCodeFindBox.ResumeLayout(true);
			this.PortOfClearanceCodeFindBox.PerformLayout();
			this.LastPortDateEdit.ResumeLayout(true);
			this.LastPortDateEdit.PerformLayout();
			this.ReleaseDateEdit.ResumeLayout(true);
			this.ReleaseDateEdit.PerformLayout();
			this.CountryOfOriginCodeFindBox.ResumeLayout(true);
			this.CountryOfOriginCodeFindBox.PerformLayout();
			this.USStateOfOriginDropEdit.ResumeLayout(true);
			this.USStateOfOriginDropEdit.PerformLayout();
			this.TreatmentCodeDropEdit.ResumeLayout(true);
			this.TreatmentCodeDropEdit.PerformLayout();
			this.JZ_InvoiceDateEdit.ResumeLayout(true);
			this.JZ_InvoiceDateEdit.PerformLayout();
			this.TimeLimitUnitDropEdit.ResumeLayout(true);
			this.TimeLimitUnitDropEdit.PerformLayout();
			this.JZ_IncoTermBoundDropDownEdit.ResumeLayout(true);
			this.JZ_IncoTermBoundDropDownEdit.PerformLayout();
			this.CountryOfExportCodeFindBox.ResumeLayout(true);
			this.CountryOfExportCodeFindBox.PerformLayout();
			this.USStateOfExportDropEdit.ResumeLayout(true);
			this.USStateOfExportDropEdit.PerformLayout();
			this.LVSCarrierCodeFindBox.ResumeLayout(true);
			this.LVSCarrierCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer SplitContainer1;
		private ZArchitecture.GUI.ZDateEdit LastPortDateEdit;
		private ZArchitecture.GUI.ZDateEdit ReleaseDateEdit;
		private ZArchitecture.GUI.ZButton IncoTermExplainButton;
		internal ZArchitecture.ZTextBox LVSIdentifierTextBox;
		private Customs.GUI.ConvertToLocalCurrencyControl JZ_InvoiceAmountBoundCurrencyControl;
		private ZArchitecture.GUI.ZDropEdit JZ_IncoTermBoundDropDownEdit;
		private CargoWise.Windows.UI.KSplitContainer SplitContainer2;
		private ZArchitecture.GUI.ZDropEdit TreatmentCodeDropEdit;
		private ZArchitecture.GUI.ZCodeFindBox CountryOfOriginCodeFindBox;
		private ZArchitecture.GUI.ZCodeFindBox CountryOfExportCodeFindBox;
		private MasterFiles.GUI.ZOrganisationFindBox ImporterOrganisationControl;
		private ZArchitecture.GUI.ZDropEdit USStateOfOriginDropEdit;
		private ZArchitecture.GUI.ZDropEdit USStateOfExportDropEdit;
		private Enterprise.ZArchitecture.ZTextBox OtherReferenceTextBox;
		internal Enterprise.ZArchitecture.GUI.ZCheckBox IsReadyForConsolidationCheckBox;
		private Enterprise.MasterFiles.GUI.ZDocAddressControl SupplierDocAddressControl;
		private ZArchitecture.GUI.ZCodeFindBox PortOfClearanceCodeFindBox;
		private ZArchitecture.GUI.ZCodeFindBox LVSCarrierCodeFindBox;
		internal ZArchitecture.ZCalcEdit TotalValueForDutyCalcEdit;
		private ZArchitecture.ZCalcEdit TimeLimitCalcEdit;
		private ZArchitecture.GUI.ZDropEdit TimeLimitUnitDropEdit;
		private ZArchitecture.GUI.ZDateEdit JZ_InvoiceDateEdit;
	}
}
