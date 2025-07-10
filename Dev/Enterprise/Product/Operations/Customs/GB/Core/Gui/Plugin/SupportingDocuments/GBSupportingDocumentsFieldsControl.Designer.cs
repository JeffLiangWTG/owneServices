namespace Enterprise.Customs.GB.GUI.Plugin
{
	partial class GBSupportingDocumentsFieldsControl
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
            this.CSI_ReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.CSI_CodeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.CSI_ValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.CSI_AvailabilityDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.CSI_Quantity2CalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.CSI_UnitOfQuantity2TextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.CSI_RX_NKCurrencyCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.CSI_DateOfExpiryDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.CSI_DateOfIssueDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.SupportingDocumentsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.CSI_ReferenceNumberCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.CSI_UnitOfQuantityDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.CSI_ActionsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.CSI_ReferenceNumber2TextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.CSI_DescriptionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.CSI_SubTypeTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.CSI_QuantityCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.CSI_CodeCodeFindBox.SuspendLayout();
            this.CSI_AvailabilityDropEdit.SuspendLayout();
            this.CSI_RX_NKCurrencyCodeFindBox.SuspendLayout();
            this.CSI_DateOfExpiryDateEdit.SuspendLayout();
            this.CSI_DateOfIssueDateEdit.SuspendLayout();
            this.SupportingDocumentsGroupBox.SuspendLayout();
            this.CSI_ReferenceNumberCodeFindBox.SuspendLayout();
            this.CSI_UnitOfQuantityDropEdit.SuspendLayout();
            this.CSI_ActionsDropEdit.SuspendLayout();
            this.CSI_DescriptionDropEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.Business.Declaration.JobDeclaration);
            // 
            // CSI_ReferenceNumberTextBox
            // 
            this.CSI_ReferenceNumberTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CSI_ReferenceNumberTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.BindingSource.SetBindingMember(this.CSI_ReferenceNumberTextBox, "FilteredInvoiceLines.SupportingDocuments.CSI_ReferenceNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Business.Declaration.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_ReferenceNumber)));
            this.CSI_ReferenceNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.CSI_ReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 41, true);
            this.CSI_ReferenceNumberTextBox.Name = "CSI_ReferenceNumberTextBox";
            this.CSI_ReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(494, 20, true);
            this.CSI_ReferenceNumberTextBox.TabIndex = 1;
            // 
            // CSI_CodeCodeFindBox
            // 
            this.CSI_CodeCodeFindBox.AllowDrop = true;
            this.CSI_CodeCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.CSI_CodeCodeFindBox, "FilteredInvoiceLines.SupportingDocuments.CSI_Code");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Business.Declaration.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_Code)));
            this.CSI_CodeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 19, true);
            this.CSI_CodeCodeFindBox.Name = "CSI_CodeCodeFindBox";
            this.CSI_CodeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.CSI_CodeCodeFindBox.ParentType = null;
            this.CSI_CodeCodeFindBox.PreBoundMaxLength = 4;
            this.CSI_CodeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(494, 20, true);
            this.CSI_CodeCodeFindBox.TabIndex = 0;
            // 
            // CSI_ValueCalcEdit
            // 
            this.CSI_ValueCalcEdit.BackColor = System.Drawing.SystemColors.Window;
            this.BindingSource.SetBindingMember(this.CSI_ValueCalcEdit, "FilteredInvoiceLines.SupportingDocuments.CSI_Value");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.GB.Business.Declaration.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_Value)));
            this.CSI_ValueCalcEdit.DecimalPlaces = 5;
            this.CSI_ValueCalcEdit.Decimals = 5;
            this.CSI_ValueCalcEdit.Extra1LabelText = "EUAddInfoSupportingDocumentSchema.xml";
            this.CSI_ValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 133, true);
            this.CSI_ValueCalcEdit.Name = "CSI_ValueCalcEdit";
            this.CSI_ValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
            this.CSI_ValueCalcEdit.TabIndex = 10;
            this.CSI_ValueCalcEdit.Text = "0.00000";
            this.CSI_ValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.CSI_ValueCalcEdit.TrackDisposedAccess = true;
            // 
            // CSI_AvailabilityDropEdit
            // 
            this.CSI_AvailabilityDropEdit.AllowDrop = true;
            this.CSI_AvailabilityDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.CSI_AvailabilityDropEdit, "FilteredInvoiceLines.SupportingDocuments.CSI_Availability");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Business.Declaration.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_Availability)));
            this.CSI_AvailabilityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(477, 64, true);
            this.CSI_AvailabilityDropEdit.Name = "CSI_AvailabilityDropEdit";
            this.CSI_AvailabilityDropEdit.PreBoundMaxLength = 3;
            this.CSI_AvailabilityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(172, 20, true);
            this.CSI_AvailabilityDropEdit.TabIndex = 4;
            // 
            // CSI_Quantity2CalcEdit
            // 
            this.CSI_Quantity2CalcEdit.BackColor = System.Drawing.SystemColors.Window;
            this.BindingSource.SetBindingMember(this.CSI_Quantity2CalcEdit, "FilteredInvoiceLines.SupportingDocuments.CSI_Quantity2");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.GB.Business.Declaration.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_Quantity2)));
            this.CSI_Quantity2CalcEdit.DecimalPlaces = 5;
            this.CSI_Quantity2CalcEdit.Decimals = 5;
            this.CSI_Quantity2CalcEdit.Extra1LabelText = "EUAddInfoSupportingDocumentSchema.xml";
            this.CSI_Quantity2CalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 110, true);
            this.CSI_Quantity2CalcEdit.Name = "CSI_Quantity2CalcEdit";
            this.CSI_Quantity2CalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
            this.CSI_Quantity2CalcEdit.TabIndex = 8;
            this.CSI_Quantity2CalcEdit.Text = "0.00000";
            this.CSI_Quantity2CalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.CSI_Quantity2CalcEdit.TrackDisposedAccess = true;
            // 
            // CSI_UnitOfQuantity2TextBox
            // 
            this.CSI_UnitOfQuantity2TextBox.BackColor = System.Drawing.SystemColors.Window;
            this.BindingSource.SetBindingMember(this.CSI_UnitOfQuantity2TextBox, "FilteredInvoiceLines.SupportingDocuments.CSI_UnitOfQuantity2");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Business.Declaration.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_UnitOfQuantity2)));
            this.CSI_UnitOfQuantity2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(477, 110, true);
            this.CSI_UnitOfQuantity2TextBox.Name = "CSI_UnitOfQuantity2TextBox";
            this.CSI_UnitOfQuantity2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
            this.CSI_UnitOfQuantity2TextBox.TabIndex = 9;
            // 
            // CSI_RX_NKCurrencyCodeFindBox
            // 
            this.CSI_RX_NKCurrencyCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.CSI_RX_NKCurrencyCodeFindBox, "FilteredInvoiceLines.SupportingDocuments.CSI_RX_NKCurrency");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Business.Declaration.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_RX_NKCurrency)));
            this.CSI_RX_NKCurrencyCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(477, 133, true);
            this.CSI_RX_NKCurrencyCodeFindBox.Name = "CSI_RX_NKCurrencyCodeFindBox";
            this.CSI_RX_NKCurrencyCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.CSI_RX_NKCurrencyCodeFindBox.ParentType = null;
            this.CSI_RX_NKCurrencyCodeFindBox.PreBoundMaxLength = 4;
            this.CSI_RX_NKCurrencyCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(165, 20, true);
            this.CSI_RX_NKCurrencyCodeFindBox.TabIndex = 11;
            // 
            // CSI_DateOfExpiryDateEdit
            // 
            this.CSI_DateOfExpiryDateEdit.AllowDrop = true;
            this.CSI_DateOfExpiryDateEdit.AutoCompleteMonthThreshold = 1;
            this.CSI_DateOfExpiryDateEdit.BackColor = System.Drawing.SystemColors.Window;
            this.BindingSource.SetBindingMember(this.CSI_DateOfExpiryDateEdit, "FilteredInvoiceLines.SupportingDocuments.CSI_DateOfExpiry");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Business.Declaration.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_DateOfExpiry)));
            this.CSI_DateOfExpiryDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(477, 156, true);
            this.CSI_DateOfExpiryDateEdit.Name = "CSI_DateOfExpiryDateEdit";
            this.CSI_DateOfExpiryDateEdit.TabIndex = 13;
            // 
            // CSI_DateOfIssueDateEdit
            // 
            this.CSI_DateOfIssueDateEdit.AllowDrop = true;
            this.CSI_DateOfIssueDateEdit.AutoCompleteMonthThreshold = 1;
            this.CSI_DateOfIssueDateEdit.BackColor = System.Drawing.SystemColors.Window;
            this.BindingSource.SetBindingMember(this.CSI_DateOfIssueDateEdit, "FilteredInvoiceLines.SupportingDocuments.CSI_DateOfIssue");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Business.Declaration.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_DateOfIssue)));
            this.CSI_DateOfIssueDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 156, true);
            this.CSI_DateOfIssueDateEdit.Name = "CSI_DateOfIssueDateEdit";
            this.CSI_DateOfIssueDateEdit.TabIndex = 12;
            // 
            // SupportingDocumentsGroupBox
            // 
            this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_ReferenceNumberCodeFindBox);
            this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_UnitOfQuantityDropEdit);
            this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_ActionsDropEdit);
            this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_CodeCodeFindBox);
            this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_DateOfIssueDateEdit);
            this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_UnitOfQuantity2TextBox);
            this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_ReferenceNumber2TextBox);
            this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_DescriptionDropEdit);
            this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_ReferenceNumberTextBox);
            this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_RX_NKCurrencyCodeFindBox);
            this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_SubTypeTextBox);
            this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_Quantity2CalcEdit);
            this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_QuantityCalcEdit);
            this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_ValueCalcEdit);
            this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_DateOfExpiryDateEdit);
            this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_AvailabilityDropEdit);
            this.SupportingDocumentsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SupportingDocumentsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.SupportingDocumentsGroupBox.Name = "SupportingDocumentsGroupBox";
            this.SupportingDocumentsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(655, 230, true);
            this.SupportingDocumentsGroupBox.TabIndex = 1;
            this.SupportingDocumentsGroupBox.TabStop = false;
            // 
            // CSI_ReferenceNumberCodeFindBox
            // 
            this.CSI_ReferenceNumberCodeFindBox.AllowDrop = true;
            this.CSI_ReferenceNumberCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.CSI_ReferenceNumberCodeFindBox, "FilteredInvoiceLines.SupportingDocuments.CSI_ReferenceNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Business.Declaration.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_ReferenceNumber)));
            this.CSI_ReferenceNumberCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 41, true);
            this.CSI_ReferenceNumberCodeFindBox.Name = "CSI_ReferenceNumberCodeFindBox";
            this.CSI_ReferenceNumberCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.CSI_ReferenceNumberCodeFindBox.ParentType = null;
            this.CSI_ReferenceNumberCodeFindBox.PreBoundMaxLength = 4;
            this.CSI_ReferenceNumberCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(494, 20, true);
            this.CSI_ReferenceNumberCodeFindBox.TabIndex = 2;
            // 
            // CSI_UnitOfQuantityDropEdit
            // 
            this.CSI_UnitOfQuantityDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.CSI_UnitOfQuantityDropEdit, "FilteredInvoiceLines.SupportingDocuments.CSI_UnitOfQuantity");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Business.Declaration.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_UnitOfQuantity)));
            this.CSI_UnitOfQuantityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(477, 87, true);
            this.CSI_UnitOfQuantityDropEdit.Name = "CSI_UnitOfQuantityDropEdit";
            this.CSI_UnitOfQuantityDropEdit.ShowDescriptionBox = false;
            this.CSI_UnitOfQuantityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
            this.CSI_UnitOfQuantityDropEdit.TabIndex = 7;
            // 
            // CSI_ActionsDropEdit
            // 
            this.CSI_ActionsDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.CSI_ActionsDropEdit, "FilteredInvoiceLines.SupportingDocuments.CSI_Actions");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Business.Declaration.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_Actions)));
            this.CSI_ActionsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 64, true);
            this.CSI_ActionsDropEdit.Name = "CSI_ActionsDropEdit";
            this.CSI_ActionsDropEdit.PreBoundMaxLength = 3;
            this.CSI_ActionsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 20, true);
            this.CSI_ActionsDropEdit.TabIndex = 3;
            // 
            // CSI_ReferenceNumber2TextBox
            // 
            this.CSI_ReferenceNumber2TextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CSI_ReferenceNumber2TextBox.BackColor = System.Drawing.SystemColors.Window;
            this.BindingSource.SetBindingMember(this.CSI_ReferenceNumber2TextBox, "FilteredInvoiceLines.SupportingDocuments.CSI_ReferenceNumber2");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Business.Declaration.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_ReferenceNumber2)));
            this.CSI_ReferenceNumber2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 201, true);
            this.CSI_ReferenceNumber2TextBox.Name = "CSI_ReferenceNumber2TextBox";
            this.CSI_ReferenceNumber2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(494, 20, true);
            this.CSI_ReferenceNumber2TextBox.TabIndex = 15;
            // 
            // CSI_DescriptionDropEdit
            // 
            this.CSI_DescriptionDropEdit.AllowDrop = true;
            this.CSI_DescriptionDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CSI_DescriptionDropEdit.BackColor = System.Drawing.SystemColors.Window;
            this.BindingSource.SetBindingMember(this.CSI_DescriptionDropEdit, "FilteredInvoiceLines.SupportingDocuments.CSI_Description");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Business.Declaration.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_Description)));
            this.CSI_DescriptionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 178, true);
            this.CSI_DescriptionDropEdit.Name = "CSI_DescriptionDropEdit";
            this.CSI_DescriptionDropEdit.PreBoundMaxLength = 50;
            this.CSI_DescriptionDropEdit.ShouldResizeByMaxLength = false;
            this.CSI_DescriptionDropEdit.ShowDescriptionBox = false;
            this.CSI_DescriptionDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
            this.CSI_DescriptionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(319, 20, true);
            this.CSI_DescriptionDropEdit.TabIndex = 14;
            // 
            // CSI_SubTypeTextBox
            // 
            this.CSI_SubTypeTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.BindingSource.SetBindingMember(this.CSI_SubTypeTextBox, "FilteredInvoiceLines.SupportingDocuments.CSI_SubType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Business.Declaration.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_SubType)));
            this.CSI_SubTypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 87, true);
            this.CSI_SubTypeTextBox.Name = "CSI_SubTypeTextBox";
            this.CSI_SubTypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 20, true);
            this.CSI_SubTypeTextBox.TabIndex = 5;
            // 
            // CSI_QuantityCalcEdit
            // 
            this.CSI_QuantityCalcEdit.BackColor = System.Drawing.SystemColors.Window;
            this.BindingSource.SetBindingMember(this.CSI_QuantityCalcEdit, "FilteredInvoiceLines.SupportingDocuments.CSI_Quantity");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.GB.Business.Declaration.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_Quantity)));
            this.CSI_QuantityCalcEdit.DecimalPlaces = 3;
            this.CSI_QuantityCalcEdit.Decimals = 3;
            this.CSI_QuantityCalcEdit.Extra1LabelText = "EUAddInfoSupportingDocumentSchema.xml";
            this.CSI_QuantityCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(341, 87, true);
            this.CSI_QuantityCalcEdit.Name = "CSI_QuantityCalcEdit";
            this.CSI_QuantityCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(66, 20, true);
            this.CSI_QuantityCalcEdit.TabIndex = 6;
            this.CSI_QuantityCalcEdit.Text = "0.000";
            this.CSI_QuantityCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.CSI_QuantityCalcEdit.TrackDisposedAccess = true;
            // 
            // GBSupportingDocumentsFieldsControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.SupportingDocumentsGroupBox);
            this.Name = "GBSupportingDocumentsFieldsControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(655, 230, true);
            this.Controls.SetChildIndex(this.SupportingDocumentsGroupBox, 0);
            this.Controls.SetChildIndex(this.RequiresMergeLabel, 0);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.CSI_CodeCodeFindBox.ResumeLayout(true);
            this.CSI_CodeCodeFindBox.PerformLayout();
            this.CSI_AvailabilityDropEdit.ResumeLayout(true);
            this.CSI_AvailabilityDropEdit.PerformLayout();
            this.CSI_RX_NKCurrencyCodeFindBox.ResumeLayout(true);
            this.CSI_RX_NKCurrencyCodeFindBox.PerformLayout();
            this.CSI_DateOfExpiryDateEdit.ResumeLayout(true);
            this.CSI_DateOfExpiryDateEdit.PerformLayout();
            this.CSI_DateOfIssueDateEdit.ResumeLayout(true);
            this.CSI_DateOfIssueDateEdit.PerformLayout();
            this.SupportingDocumentsGroupBox.ResumeLayout(false);
            this.SupportingDocumentsGroupBox.PerformLayout();
            this.CSI_ReferenceNumberCodeFindBox.ResumeLayout(true);
            this.CSI_ReferenceNumberCodeFindBox.PerformLayout();
            this.CSI_UnitOfQuantityDropEdit.ResumeLayout(true);
            this.CSI_UnitOfQuantityDropEdit.PerformLayout();
            this.CSI_ActionsDropEdit.ResumeLayout(true);
            this.CSI_ActionsDropEdit.PerformLayout();
            this.CSI_DescriptionDropEdit.ResumeLayout(true);
            this.CSI_DescriptionDropEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox CSI_ReferenceNumberTextBox;
		private ZArchitecture.GUI.ZCodeFindBox CSI_CodeCodeFindBox;
		private ZArchitecture.ZCalcEdit CSI_ValueCalcEdit;
		private ZArchitecture.GUI.ZDropEdit CSI_AvailabilityDropEdit;
		private ZArchitecture.ZCalcEdit CSI_Quantity2CalcEdit;
		private ZArchitecture.ZTextBox CSI_UnitOfQuantity2TextBox;
		private ZArchitecture.GUI.ZCodeFindBox CSI_RX_NKCurrencyCodeFindBox;
		private ZArchitecture.GUI.ZDateEdit CSI_DateOfExpiryDateEdit;
		private ZArchitecture.GUI.ZDateEdit CSI_DateOfIssueDateEdit;
		private ZArchitecture.GUI.ZGroupBox SupportingDocumentsGroupBox;
		private ZArchitecture.GUI.ZDropEdit CSI_UnitOfQuantityDropEdit;
		private ZArchitecture.GUI.ZDropEdit CSI_ActionsDropEdit;
		private ZArchitecture.ZTextBox CSI_ReferenceNumber2TextBox;
		private ZArchitecture.GUI.ZDropEdit CSI_DescriptionDropEdit;
		private ZArchitecture.ZTextBox CSI_SubTypeTextBox;
		private ZArchitecture.ZCalcEdit CSI_QuantityCalcEdit;
		private ZArchitecture.GUI.ZCodeFindBox CSI_ReferenceNumberCodeFindBox;
	}
}
