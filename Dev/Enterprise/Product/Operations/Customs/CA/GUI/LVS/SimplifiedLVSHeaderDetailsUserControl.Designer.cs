namespace Enterprise.Customs.CA.GUI
{
	partial class SimplifiedLVSHeaderDetailsUserControl
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
			this.PortOfClearanceCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.OtherReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CountryOfExportCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.USStateOfOriginDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.USStateOfExportDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TreatmentCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.LastPortDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.CountryOfOriginCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.IncoTermExplainButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.LVSIdentifierTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JZ_InvoiceAmountBoundCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
			this.JZ_IncoTermBoundDropDownEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SupplierDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.ImporterOrganisationControl = new Enterprise.MasterFiles.GUI.ZOrganisationControl();
			this.ShipperDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.TimeLimitCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TimeLimitUnitDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PortOfClearanceCodeFindBox.SuspendLayout();
			this.CountryOfExportCodeFindBox.SuspendLayout();
			this.USStateOfOriginDropEdit.SuspendLayout();
			this.USStateOfExportDropEdit.SuspendLayout();
			this.TreatmentCodeDropEdit.SuspendLayout();
			this.LastPortDateEdit.SuspendLayout();
			this.CountryOfOriginCodeFindBox.SuspendLayout();
			this.JZ_InvoiceAmountBoundCurrencyControl.SuspendLayout();
			this.JZ_IncoTermBoundDropDownEdit.SuspendLayout();
			this.SupplierDocAddressControl.SuspendLayout();
			this.ImporterOrganisationControl.SuspendLayout();
			this.ShipperDocAddressControl.SuspendLayout();
			this.TimeLimitUnitDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.JobComInvoiceHeader);
			// 
			// PortOfClearanceCodeFindBox
			// 
			this.PortOfClearanceCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PortOfClearanceCodeFindBox, "CA_PortOfClearance");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(null)).CA_PortOfClearance)));
			this.PortOfClearanceCodeFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("SimplifiedLVSHeaderDetailsUserControl|309e4dd8-600f-45e3-b5db-23eea14c8efa", "Port of Clearance");
			this.PortOfClearanceCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 235, true);
			this.PortOfClearanceCodeFindBox.Name = "PortOfClearanceCodeFindBox";
			this.PortOfClearanceCodeFindBox.PreBoundMaxLength = 4;
			this.PortOfClearanceCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(253, 20, true);
			this.PortOfClearanceCodeFindBox.TabIndex = 10;
			// 
			// OtherReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.OtherReferenceTextBox, "CA_OtherReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(null)).CA_OtherReference)));
			this.OtherReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 259, true);
			this.OtherReferenceTextBox.Multiline = true;
			this.OtherReferenceTextBox.Name = "OtherReferenceTextBox";
			this.OtherReferenceTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.OtherReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(253, 46, true);
			this.OtherReferenceTextBox.TabIndex = 11;
			// 
			// CountryOfExportCodeFindBox
			// 
			this.CountryOfExportCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryOfExportCodeFindBox, "CA_RN_NKExport");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(null)).CA_RN_NKExport)));
			this.CountryOfExportCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 163, true);
			this.CountryOfExportCodeFindBox.Name = "CountryOfExportCodeFindBox";
			this.CountryOfExportCodeFindBox.PreBoundMaxLength = 2;
			this.CountryOfExportCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(253, 20, true);
			this.CountryOfExportCodeFindBox.TabIndex = 7;
			// 
			// USStateOfOriginDropEdit
			// 
			this.USStateOfOriginDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.USStateOfOriginDropEdit, "JZ_RW_NKOriginState");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(null)).JZ_RW_NKOriginState)));
			this.USStateOfOriginDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 139, true);
			this.USStateOfOriginDropEdit.Name = "USStateOfOriginDropEdit";
			this.USStateOfOriginDropEdit.PreBoundMaxLength = 2;
			this.USStateOfOriginDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(253, 20, true);
			this.USStateOfOriginDropEdit.TabIndex = 6;
			// 
			// USStateOfExportDropEdit
			// 
			this.USStateOfExportDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.USStateOfExportDropEdit, "CA_USStateOfExport");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(null)).CA_USStateOfExport)));
			this.USStateOfExportDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 187, true);
			this.USStateOfExportDropEdit.Name = "USStateOfExportDropEdit";
			this.USStateOfExportDropEdit.PreBoundMaxLength = 2;
			this.USStateOfExportDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(253, 20, true);
			this.USStateOfExportDropEdit.TabIndex = 8;
			// 
			// TreatmentCodeDropEdit
			// 
			this.TreatmentCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TreatmentCodeDropEdit, "CA_TreatmentCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(null)).CA_TreatmentCode)));
			this.TreatmentCodeDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("SimplifiedLVSHeaderDetailsUserControl|b169353b-bea7-4563-9ba9-92ea7acd2064", "Treatment Code", "A means by which normal rates of duty may be modified according to the Customs Tariff. Refer to the Customs Tariff for information on the applicability of these tariff treatments.");
			this.TreatmentCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 211, true);
			this.TreatmentCodeDropEdit.Name = "TreatmentCodeDropEdit";
			this.TreatmentCodeDropEdit.PreBoundMaxLength = 2;
			this.TreatmentCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(253, 20, true);
			this.TreatmentCodeDropEdit.TabIndex = 9;
			// 
			// LastPortDateEdit
			// 
			this.LastPortDateEdit.AllowDrop = true;
			this.LastPortDateEdit.AutoCompleteMonthThreshold = 1;
			this.LastPortDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.LastPortDateEdit, "JZ_ValuationDateOverride");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(null)).JZ_ValuationDateOverride)));
			this.LastPortDateEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("SimplifiedLVSHeaderDetailsUserControl|1672d534-d0e5-4a96-8f29-4fa5327aef01", "Date", "Direct Ship. Date", "Date of Direct Shipment", "The date (per each invoice) the goods began their continuous journey to Canada.");
			this.LastPortDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 91, true);
			this.LastPortDateEdit.Name = "LastPortDateEdit";
			this.LastPortDateEdit.TabIndex = 4;
			// 
			// CountryOfOriginCodeFindBox
			// 
			this.CountryOfOriginCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryOfOriginCodeFindBox, "JZ_RN_NKDefaultOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(null)).JZ_RN_NKDefaultOrigin)));
			this.CountryOfOriginCodeFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("SimplifiedLVSHeaderDetailsUserControl|8d00c95d-5c7b-45b7-b078-c980c5c09e5a", "Ctry/Rgn. of Origin", "Country/Region of Origin", "Default Country/Region of Origin", "The country/region where the goods are grown, produced or manufactured.");
			this.CountryOfOriginCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 115, true);
			this.CountryOfOriginCodeFindBox.Name = "CountryOfOriginCodeFindBox";
			this.CountryOfOriginCodeFindBox.PreBoundMaxLength = 2;
			this.CountryOfOriginCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(253, 20, true);
			this.CountryOfOriginCodeFindBox.TabIndex = 5;
			// 
			// IncoTermExplainButton
			// 
			this.IncoTermExplainButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(344, 67, true);
			this.IncoTermExplainButton.Name = "IncoTermExplainButton";
			this.IncoTermExplainButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 20, true);
			this.IncoTermExplainButton.TabIndex = 3;
			this.IncoTermExplainButton.Text = "...";
			this.IncoTermExplainButton.Click += new System.EventHandler(this.IncoTermExplainButton_Click);
			// 
			// LVSIdentifierTextBox
			// 
			this.BindingSource.SetBindingMember(this.LVSIdentifierTextBox, "JZ_InvoiceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(null)).JZ_InvoiceNumber)));
			this.LVSIdentifierTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("SimplifiedLVSHeaderDetailsUserControl|8f712317-6138-4415-8535-9a57afc2cc2f", "LVS ID/Inv. #");
			this.LVSIdentifierTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 19, true);
			this.LVSIdentifierTextBox.Name = "LVSIdentifierTextBox";
			this.LVSIdentifierTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(253, 20, true);
			this.LVSIdentifierTextBox.TabIndex = 0;
			// 
			// JZ_InvoiceAmountBoundCurrencyControl
			// 
			this.JZ_InvoiceAmountBoundCurrencyControl.AllowDrop = true;
			this.JZ_InvoiceAmountBoundCurrencyControl.BindToAmount = "JZ_InvoiceAmount";
			this.JZ_InvoiceAmountBoundCurrencyControl.BindToUnit = "JZ_RX_NKInvoice_Currency";
			this.JZ_InvoiceAmountBoundCurrencyControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.JZ_InvoiceAmountBoundCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 43, true);
			this.JZ_InvoiceAmountBoundCurrencyControl.Name = "JZ_InvoiceAmountBoundCurrencyControl";
			this.JZ_InvoiceAmountBoundCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(253, 20, true);
			this.JZ_InvoiceAmountBoundCurrencyControl.TabIndex = 1;
			// 
			// JZ_IncoTermBoundDropDownEdit
			// 
			this.JZ_IncoTermBoundDropDownEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JZ_IncoTermBoundDropDownEdit, "JZ_IncoTerm");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(null)).JZ_IncoTerm)));
			this.JZ_IncoTermBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 67, true);
			this.JZ_IncoTermBoundDropDownEdit.Name = "JZ_IncoTermBoundDropDownEdit";
			this.JZ_IncoTermBoundDropDownEdit.PreBoundMaxLength = 3;
			this.JZ_IncoTermBoundDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(223, 20, true);
			this.JZ_IncoTermBoundDropDownEdit.TabIndex = 2;
			// 
			// SupplierDocAddressControl
			// 
			this.SupplierDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SupplierDocAddressControl, "SupplierDocumentaryAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(null)).SupplierDocumentaryAddress)));
			this.SupplierDocAddressControl.BindToOrganisations = "Vendors";
			this.SupplierDocAddressControl.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("SimplifiedLVSHeaderDetailsUserControl|a3be46a8-6e7f-4584-a5ab-2d3a27da6f76", "Vendor");
			this.SupplierDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(380, 155, true);
			this.SupplierDocAddressControl.Name = "SupplierDocAddressControl";
			this.SupplierDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.SupplierDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.SupplierDocAddressControl.TabIndex = 13;
			// 
			// ImporterOrganisationControl
			// 
			this.ImporterOrganisationControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ImporterOrganisationControl, "JZ_OH_Buyer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(null)).JZ_OH_Buyer)));
			this.ImporterOrganisationControl.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("SimplifiedLVSHeaderDetailsUserControl|E003A98D-C69F-437a-8F8F-5B708F43F0D0", "Importer");
			this.ImporterOrganisationControl.Captions = new string[] {
        "Importer"};
			this.ImporterOrganisationControl.IsCaptionOverridden = false;
			this.ImporterOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(380, 2, true);
			this.ImporterOrganisationControl.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 0, true);
			this.ImporterOrganisationControl.Name = "ImporterOrganisationControl";
			this.ImporterOrganisationControl.PopupCaption = "Vendors";
			this.ImporterOrganisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 152, true);
			this.ImporterOrganisationControl.TabIndex = 12;
			// 
			// ShipperDocAddressControl
			// 
			this.ShipperDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipperDocAddressControl, "SupplierPickupDeliveryAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(null)).SupplierPickupDeliveryAddress)));
			this.ShipperDocAddressControl.BindToOrganisations = "Suppliers";
			this.ShipperDocAddressControl.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("SimplifiedLVSHeaderDetailsUserControl|02999eeb-6890-4ff1-bf90-b9230abca241", "Shipper");
			this.ShipperDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(637, 155, true);
			this.ShipperDocAddressControl.Name = "ShipperDocAddressControl";
			this.ShipperDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.ShipperDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.ShipperDocAddressControl.TabIndex = 14;
			// 
			// TimeLimitCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TimeLimitCalcEdit, "CA_TimeLimit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(null)).CA_TimeLimit)));
			this.TimeLimitCalcEdit.DecimalPlaces = 2;
			this.TimeLimitCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportSupplierHeaderUserControl|61cc5035-b4b3-4fc8-9592-330399b755ed", "Time Limit");
			this.TimeLimitCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 312, true);
			this.TimeLimitCalcEdit.Name = "TimeLimitCalcEdit";
			this.TimeLimitCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.TimeLimitCalcEdit.TabIndex = 15;
			this.TimeLimitCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TimeLimitUnitDropEdit
			// 
			this.TimeLimitUnitDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TimeLimitUnitDropEdit, "CA_TimeLimitCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(null)).CA_TimeLimitCode)));
			this.TimeLimitUnitDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(219, 312, true);
			this.TimeLimitUnitDropEdit.Name = "TimeLimitUnitDropEdit";
			this.TimeLimitUnitDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(149, 20, true);
			this.TimeLimitUnitDropEdit.TabIndex = 16;
			// 
			// SimplifiedLVSHeaderDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TimeLimitUnitDropEdit);
			this.Controls.Add(this.TimeLimitCalcEdit);
			this.Controls.Add(this.ShipperDocAddressControl);
			this.Controls.Add(this.PortOfClearanceCodeFindBox);
			this.Controls.Add(this.OtherReferenceTextBox);
			this.Controls.Add(this.CountryOfExportCodeFindBox);
			this.Controls.Add(this.USStateOfOriginDropEdit);
			this.Controls.Add(this.USStateOfExportDropEdit);
			this.Controls.Add(this.TreatmentCodeDropEdit);
			this.Controls.Add(this.LastPortDateEdit);
			this.Controls.Add(this.CountryOfOriginCodeFindBox);
			this.Controls.Add(this.IncoTermExplainButton);
			this.Controls.Add(this.LVSIdentifierTextBox);
			this.Controls.Add(this.JZ_InvoiceAmountBoundCurrencyControl);
			this.Controls.Add(this.JZ_IncoTermBoundDropDownEdit);
			this.Controls.Add(this.ImporterOrganisationControl);
			this.Controls.Add(this.SupplierDocAddressControl);
			this.Name = "SimplifiedLVSHeaderDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(908, 343, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PortOfClearanceCodeFindBox.ResumeLayout(true);
			this.PortOfClearanceCodeFindBox.PerformLayout();
			this.CountryOfExportCodeFindBox.ResumeLayout(true);
			this.CountryOfExportCodeFindBox.PerformLayout();
			this.USStateOfOriginDropEdit.ResumeLayout(true);
			this.USStateOfOriginDropEdit.PerformLayout();
			this.USStateOfExportDropEdit.ResumeLayout(true);
			this.USStateOfExportDropEdit.PerformLayout();
			this.TreatmentCodeDropEdit.ResumeLayout(true);
			this.TreatmentCodeDropEdit.PerformLayout();
			this.LastPortDateEdit.ResumeLayout(true);
			this.LastPortDateEdit.PerformLayout();
			this.CountryOfOriginCodeFindBox.ResumeLayout(true);
			this.CountryOfOriginCodeFindBox.PerformLayout();
			this.JZ_InvoiceAmountBoundCurrencyControl.ResumeLayout(true);
			this.JZ_InvoiceAmountBoundCurrencyControl.PerformLayout();
			this.JZ_IncoTermBoundDropDownEdit.ResumeLayout(true);
			this.JZ_IncoTermBoundDropDownEdit.PerformLayout();
			this.SupplierDocAddressControl.ResumeLayout(true);
			this.SupplierDocAddressControl.PerformLayout();
			this.ImporterOrganisationControl.ResumeLayout(true);
			this.ImporterOrganisationControl.PerformLayout();
			this.ShipperDocAddressControl.ResumeLayout(true);
			this.ShipperDocAddressControl.PerformLayout();
			this.TimeLimitUnitDropEdit.ResumeLayout(true);
			this.TimeLimitUnitDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZDateEdit LastPortDateEdit;
		private ZArchitecture.GUI.ZButton IncoTermExplainButton;
		internal ZArchitecture.ZTextBox LVSIdentifierTextBox;
		private Customs.GUI.ConvertToLocalCurrencyControl JZ_InvoiceAmountBoundCurrencyControl;
		private ZArchitecture.GUI.ZDropEdit JZ_IncoTermBoundDropDownEdit;
		private ZArchitecture.GUI.ZDropEdit TreatmentCodeDropEdit;
		private ZArchitecture.GUI.ZCodeFindBox CountryOfOriginCodeFindBox;
		private ZArchitecture.GUI.ZCodeFindBox CountryOfExportCodeFindBox;
		private MasterFiles.GUI.ZOrganisationControl ImporterOrganisationControl;
		private ZArchitecture.GUI.ZDropEdit USStateOfOriginDropEdit;
		private ZArchitecture.GUI.ZDropEdit USStateOfExportDropEdit;
		private Enterprise.ZArchitecture.ZTextBox OtherReferenceTextBox;
		internal Enterprise.MasterFiles.GUI.ZDocAddressControl SupplierDocAddressControl;
		private ZArchitecture.GUI.ZCodeFindBox PortOfClearanceCodeFindBox;
		internal MasterFiles.GUI.ZDocAddressControl ShipperDocAddressControl;
		private ZArchitecture.ZCalcEdit TimeLimitCalcEdit;
		private ZArchitecture.GUI.ZDropEdit TimeLimitUnitDropEdit;
	}
}
