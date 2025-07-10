namespace Enterprise.Customs.FR.GUI.PlugIn
{
	partial class SupportingDocumentsFieldsControl
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
			this.CSI_QuantityCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CSI_Quantity2CalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CSI_UnitOfQuantity2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CSI_ValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CSI_RX_NKCurrencyCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CSI_DateOfExpiryDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.CSI_UnitOfQuantityTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CSI_DateOfIssueDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.SupportingDocumentsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CSI_ItemNumberCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CSI_ReferenceNumberCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CSI_DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CSI_AdditionalDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CSI_LineNoCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CSI_UnitOfQuantityDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CSI_ItemNumberCalcEdit.SuspendLayout();
			this.CSI_CodeCodeFindBox.SuspendLayout();
			this.CSI_RX_NKCurrencyCodeFindBox.SuspendLayout();
			this.CSI_DateOfExpiryDateEdit.SuspendLayout();
			this.CSI_DateOfIssueDateEdit.SuspendLayout();
			this.SupportingDocumentsGroupBox.SuspendLayout();
			this.CSI_ReferenceNumberCodeFindBox.SuspendLayout();
			this.CSI_UnitOfQuantityDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.FR.Business.Declaration.JobDeclaration);
			// 
			// CSI_ReferenceNumberTextBox
			// 
			this.CSI_ReferenceNumberTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.CSI_ReferenceNumberTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CSI_ReferenceNumberTextBox, "FilteredInvoiceLines.SupportingDocuments.CSI_ReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.FR.Business.Declaration.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_ReferenceNumber)));
			this.CSI_ReferenceNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CSI_ReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 41, true);
			this.CSI_ReferenceNumberTextBox.Name = "CSI_ReferenceNumberTextBox";
			this.CSI_ReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(672, 17, true);
			this.CSI_ReferenceNumberTextBox.TabIndex = 1;
			// 
			// CSI_CodeCodeFindBox
			// 
			this.CSI_CodeCodeFindBox.AllowDrop = true;
			this.CSI_CodeCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CSI_CodeCodeFindBox, "FilteredInvoiceLines.SupportingDocuments.CSI_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.FR.Business.Declaration.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_Code)));
			this.CSI_CodeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 19, true);
			this.CSI_CodeCodeFindBox.Name = "CSI_CodeCodeFindBox";
			this.CSI_CodeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CSI_CodeCodeFindBox.ParentType = null;
			this.CSI_CodeCodeFindBox.PreBoundMaxLength = 4;
			this.CSI_CodeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(672, 17, true);
			this.CSI_CodeCodeFindBox.TabIndex = 0;
			// 
			// CSI_QuantityCalcEdit
			// 
			this.CSI_QuantityCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CSI_QuantityCalcEdit, "FilteredInvoiceLines.SupportingDocuments.CSI_Quantity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.FR.Business.Declaration.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_Quantity)));
			this.CSI_QuantityCalcEdit.DecimalPlaces = 2;
			this.CSI_QuantityCalcEdit.Extra1LabelText = "EUAddInfoSupportingDocumentSchema.xml";
			this.CSI_QuantityCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 85, true);
			this.CSI_QuantityCalcEdit.Name = "CSI_QuantityCalcEdit";
			this.CSI_QuantityCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.CSI_QuantityCalcEdit.TabIndex = 3;
			this.CSI_QuantityCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CSI_Quantity2CalcEdit
			// 
			this.CSI_Quantity2CalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CSI_Quantity2CalcEdit, "FilteredInvoiceLines.SupportingDocuments.CSI_Quantity2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.FR.Business.Declaration.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_Quantity2)));
			this.CSI_Quantity2CalcEdit.DecimalPlaces = 2;
			this.CSI_Quantity2CalcEdit.Extra1LabelText = "EUAddInfoSupportingDocumentSchema.xml";
			this.CSI_Quantity2CalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 107, true);
			this.CSI_Quantity2CalcEdit.Name = "CSI_Quantity2CalcEdit";
			this.CSI_Quantity2CalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.CSI_Quantity2CalcEdit.TabIndex = 6;
			this.CSI_Quantity2CalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CSI_UnitOfQuantity2TextBox
			// 
			this.CSI_UnitOfQuantity2TextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CSI_UnitOfQuantity2TextBox, "FilteredInvoiceLines.SupportingDocuments.CSI_UnitOfQuantity2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.FR.Business.Declaration.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_UnitOfQuantity2)));
			this.CSI_UnitOfQuantity2TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CSI_UnitOfQuantity2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(430, 107, true);
			this.CSI_UnitOfQuantity2TextBox.Name = "CSI_UnitOfQuantity2TextBox";
			this.CSI_UnitOfQuantity2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.CSI_UnitOfQuantity2TextBox.TabIndex = 7;
			// 
			// CSI_ValueCalcEdit
			// 
			this.CSI_ValueCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CSI_ValueCalcEdit, "FilteredInvoiceLines.SupportingDocuments.CSI_Value");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.FR.Business.Declaration.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_Value)));
			this.CSI_ValueCalcEdit.DecimalPlaces = 4;
			this.CSI_ValueCalcEdit.Decimals = 4;
			this.CSI_ValueCalcEdit.Extra1LabelText = "EUAddInfoSupportingDocumentSchema.xml";
			this.CSI_ValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 129, true);
			this.CSI_ValueCalcEdit.Name = "CSI_ValueCalcEdit";
			this.CSI_ValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.CSI_ValueCalcEdit.TabIndex = 9;
			this.CSI_ValueCalcEdit.Text = "0,0000";
			this.CSI_ValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CSI_RX_NKCurrencyCodeFindBox
			// 
			this.CSI_RX_NKCurrencyCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CSI_RX_NKCurrencyCodeFindBox, "FilteredInvoiceLines.SupportingDocuments.CSI_RX_NKCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.FR.Business.Declaration.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_RX_NKCurrency)));
			this.CSI_RX_NKCurrencyCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(430, 129, true);
			this.CSI_RX_NKCurrencyCodeFindBox.Name = "CSI_RX_NKCurrencyCodeFindBox";
			this.CSI_RX_NKCurrencyCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CSI_RX_NKCurrencyCodeFindBox.ParentType = null;
			this.CSI_RX_NKCurrencyCodeFindBox.PreBoundMaxLength = 4;
			this.CSI_RX_NKCurrencyCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.CSI_RX_NKCurrencyCodeFindBox.TabIndex = 10;
			// 
			// CSI_DateOfExpiryDateEdit
			// 
			this.CSI_DateOfExpiryDateEdit.AllowDrop = true;
			this.CSI_DateOfExpiryDateEdit.AutoCompleteMonthThreshold = 1;
			this.CSI_DateOfExpiryDateEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CSI_DateOfExpiryDateEdit, "FilteredInvoiceLines.SupportingDocuments.CSI_DateOfExpiry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.FR.Business.Declaration.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_DateOfExpiry)));
			this.CSI_DateOfExpiryDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(430, 151, true);
			this.CSI_DateOfExpiryDateEdit.Name = "CSI_DateOfExpiryDateEdit";
			this.CSI_DateOfExpiryDateEdit.TabIndex = 12;
			// 
			// CSI_UnitOfQuantityTextBox
			// 
			this.CSI_UnitOfQuantityTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CSI_UnitOfQuantityTextBox, "FilteredInvoiceLines.SupportingDocuments.CSI_UnitOfQuantity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.FR.Business.Declaration.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_UnitOfQuantity)));
			this.CSI_UnitOfQuantityTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CSI_UnitOfQuantityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(430, 85, true);
			this.CSI_UnitOfQuantityTextBox.Name = "CSI_UnitOfQuantityTextBox";
			this.CSI_UnitOfQuantityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.CSI_UnitOfQuantityTextBox.TabIndex = 4;
			// 
			// CSI_DateOfIssueDateEdit
			// 
			this.CSI_DateOfIssueDateEdit.AllowDrop = true;
			this.CSI_DateOfIssueDateEdit.AutoCompleteMonthThreshold = 1;
			this.CSI_DateOfIssueDateEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CSI_DateOfIssueDateEdit, "FilteredInvoiceLines.SupportingDocuments.CSI_DateOfIssue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.FR.Business.Declaration.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_DateOfIssue)));
			this.CSI_DateOfIssueDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 151, true);
			this.CSI_DateOfIssueDateEdit.Name = "CSI_DateOfIssueDateEdit";
			this.CSI_DateOfIssueDateEdit.TabIndex = 11;
			// 
			// SupportingDocumentsGroupBox
			// 
			this.SupportingDocumentsGroupBox.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("ABE69E80-549F-48F6-8CF3-EBBA5999BC57", "[44] Supporting Documents");
			this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_ItemNumberCalcEdit);
			this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_UnitOfQuantityDropEdit);
			this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_ReferenceNumberCodeFindBox);
			this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_DescriptionTextBox);
			this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_CodeCodeFindBox);
			this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_ValueCalcEdit);
			this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_DateOfIssueDateEdit);
			this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_AdditionalDescriptionTextBox);
			this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_UnitOfQuantity2TextBox);
			this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_ReferenceNumberTextBox);
			this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_RX_NKCurrencyCodeFindBox);
			this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_UnitOfQuantityTextBox);
			this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_Quantity2CalcEdit);
			this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_LineNoCalcEdit);
			this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_QuantityCalcEdit);
			this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_DateOfExpiryDateEdit);
			this.SupportingDocumentsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SupportingDocumentsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SupportingDocumentsGroupBox.Name = "SupportingDocumentsGroupBox";
			this.SupportingDocumentsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(848, 203, true);
			this.SupportingDocumentsGroupBox.TabIndex = 11;
			this.SupportingDocumentsGroupBox.TabStop = false;
			// 
			// CSI_ItemNumberCalcEdit
			//
			this.BindingSource.SetBindingMember(this.CSI_ItemNumberCalcEdit, "FilteredInvoiceLines.SupportingDocuments.CSI_ItemNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.FR.Business.Declaration.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_ItemNumber)));
			this.CSI_ItemNumberCalcEdit.DecimalPlaces = 0;
			this.CSI_ItemNumberCalcEdit.Decimals = 0;
			this.CSI_ItemNumberCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 197, true);
			this.CSI_ItemNumberCalcEdit.Name = "CSI_ItemNumberCalcEdit";
			this.CSI_ItemNumberCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 15, true);
			this.CSI_ItemNumberCalcEdit.TabIndex = 15;
			this.CSI_ItemNumberCalcEdit.Text = "0";
			this.CSI_ItemNumberCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CSI_ReferenceNumberCodeFindBox
			// 
			this.CSI_ReferenceNumberCodeFindBox.AllowDrop = true;
			this.CSI_ReferenceNumberCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CSI_ReferenceNumberCodeFindBox, "FilteredInvoiceLines.SupportingDocuments.CSI_ReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.FR.Business.Declaration.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_ReferenceNumber)));
			this.CSI_ReferenceNumberCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 41, true);
			this.CSI_ReferenceNumberCodeFindBox.Name = "CSI_ReferenceNumberCodeFindBox";
			this.CSI_ReferenceNumberCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CSI_ReferenceNumberCodeFindBox.ParentType = null;
			this.CSI_ReferenceNumberCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(672, 17, true);
			this.CSI_ReferenceNumberCodeFindBox.TabIndex = 1;
			// 
			// CSI_DescriptionTextBox
			// 
			this.CSI_DescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.CSI_DescriptionTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CSI_DescriptionTextBox, "FilteredInvoiceLines.SupportingDocuments.CSI_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.FR.Business.Declaration.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_Description)));
			this.CSI_DescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CSI_DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 63, true);
			this.CSI_DescriptionTextBox.Multiline = true;
			this.CSI_DescriptionTextBox.Name = "CSI_DescriptionTextBox";
			this.CSI_DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(672, 20, true);
			this.CSI_DescriptionTextBox.TabIndex = 2;
			// 
			// CSI_AdditionalDescriptionTextBox
			// 
			this.CSI_AdditionalDescriptionTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CSI_AdditionalDescriptionTextBox, "FilteredInvoiceLines.SupportingDocuments.CSI_AdditionalDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.FR.Business.Declaration.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_AdditionalDescription)));
			this.CSI_AdditionalDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CSI_AdditionalDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(430, 174, true);
			this.CSI_AdditionalDescriptionTextBox.Name = "CSI_AdditionalDescriptionTextBox";
			this.CSI_AdditionalDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.CSI_AdditionalDescriptionTextBox.TabIndex = 14;
			// 
			// CSI_LineNoCalcEdit
			// 
			this.CSI_LineNoCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CSI_LineNoCalcEdit, "FilteredInvoiceLines.SupportingDocuments.CSI_LineNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.FR.Business.Declaration.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_LineNo)));
			this.CSI_LineNoCalcEdit.DecimalPlaces = 0;
			this.CSI_LineNoCalcEdit.Decimals = 0;
			this.CSI_LineNoCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 174, true);
			this.CSI_LineNoCalcEdit.Name = "CSI_LineNoCalcEdit";
			this.CSI_LineNoCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.CSI_LineNoCalcEdit.TabIndex = 13;
			this.CSI_LineNoCalcEdit.Text = "0";
			this.CSI_LineNoCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CSI_UnitOfQuantityDropEdit
			// 
			this.CSI_UnitOfQuantityDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CSI_UnitOfQuantityDropEdit, "FilteredInvoiceLines.SupportingDocuments.CSI_UnitOfQuantity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.FR.Business.Declaration.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_UnitOfQuantity)));
			this.CSI_UnitOfQuantityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(430, 85, true);
			this.CSI_UnitOfQuantityDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CSI_UnitOfQuantityDropEdit.Name = "CSI_UnitOfQuantityDropEdit";
			this.CSI_UnitOfQuantityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 17, true);
			this.CSI_UnitOfQuantityDropEdit.TabIndex = 4;
			// 
			// SupportingDocumentsFieldsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SupportingDocumentsGroupBox);
			this.Name = "SupportingDocumentsFieldsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(848, 203, true);
			this.Controls.SetChildIndex(this.SupportingDocumentsGroupBox, 0);
			this.Controls.SetChildIndex(this.RequiresMergeLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CSI_CodeCodeFindBox.ResumeLayout(true);
			this.CSI_CodeCodeFindBox.PerformLayout();
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
			this.CSI_ItemNumberCalcEdit.ResumeLayout(true);
			this.CSI_ItemNumberCalcEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox CSI_ReferenceNumberTextBox;
		private ZArchitecture.GUI.ZCodeFindBox CSI_CodeCodeFindBox;
		private ZArchitecture.ZCalcEdit CSI_QuantityCalcEdit;
		private ZArchitecture.ZCalcEdit CSI_Quantity2CalcEdit;
		private ZArchitecture.ZTextBox CSI_UnitOfQuantity2TextBox;
		private ZArchitecture.ZCalcEdit CSI_ValueCalcEdit;
		private ZArchitecture.GUI.ZCodeFindBox CSI_RX_NKCurrencyCodeFindBox;
		private ZArchitecture.GUI.ZDateEdit CSI_DateOfExpiryDateEdit;
		private ZArchitecture.ZTextBox CSI_UnitOfQuantityTextBox;
		private ZArchitecture.GUI.ZDateEdit CSI_DateOfIssueDateEdit;
		private ZArchitecture.GUI.ZGroupBox SupportingDocumentsGroupBox;
		private ZArchitecture.ZTextBox CSI_DescriptionTextBox;
		private ZArchitecture.ZCalcEdit CSI_LineNoCalcEdit;
		private ZArchitecture.ZTextBox CSI_AdditionalDescriptionTextBox;
		private ZArchitecture.GUI.ZCodeFindBox CSI_ReferenceNumberCodeFindBox;
		private ZArchitecture.GUI.ZDropEdit CSI_UnitOfQuantityDropEdit;
		private ZArchitecture.ZCalcEdit CSI_ItemNumberCalcEdit;
	}
}
