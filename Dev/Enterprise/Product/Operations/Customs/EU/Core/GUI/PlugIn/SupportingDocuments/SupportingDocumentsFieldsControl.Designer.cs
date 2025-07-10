namespace Enterprise.Customs.EU.GUI.PlugIn
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
			this.SupDocReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SupDocTypeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.SupDocQuantityCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CSI_StatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CSI_Quantity2CalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CSI_UnitOfQuantity2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CSI_ValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CSI_RX_NKCurrencyCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CSI_DateOfExpiryDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.SupDocReferenceCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CSI_UnitOfQuantityTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CSI_DateOfIssueDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.SupportingDocumentsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SupDocTypeCodeFindBox.SuspendLayout();
			this.CSI_StatusDropEdit.SuspendLayout();
			this.CSI_RX_NKCurrencyCodeFindBox.SuspendLayout();
			this.CSI_DateOfExpiryDateEdit.SuspendLayout();
			this.SupDocReferenceCodeFindBox.SuspendLayout();
			this.CSI_DateOfIssueDateEdit.SuspendLayout();
			this.SupportingDocumentsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobDeclaration);
			// 
			// SupDocReferenceTextBox
			// 
			this.SupDocReferenceTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.SupDocReferenceTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.SupDocReferenceTextBox, "FilteredInvoiceLines.SupportingDocuments.CSI_ReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_ReferenceNumber)));
			this.SupDocReferenceTextBox.CaptionResourceString = null;
			this.SupDocReferenceTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SupDocReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 41, true);
			this.SupDocReferenceTextBox.Name = "SupDocReferenceTextBox";
			this.SupDocReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(438, 20, true);
			this.SupDocReferenceTextBox.TabIndex = 1;
			// 
			// SupDocTypeCodeFindBox
			// 
			this.SupDocTypeCodeFindBox.AllowDrop = true;
			this.SupDocTypeCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SupDocTypeCodeFindBox, "FilteredInvoiceLines.SupportingDocuments.CSI_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_Code)));
			this.SupDocTypeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 19, true);
			this.SupDocTypeCodeFindBox.Name = "SupDocTypeCodeFindBox";
			this.SupDocTypeCodeFindBox.PreBoundMaxLength = 4;
			this.SupDocTypeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(438, 20, true);
			this.SupDocTypeCodeFindBox.TabIndex = 0;
			// 
			// SupDocQuantityCalcEdit
			// 
			this.SupDocQuantityCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.SupDocQuantityCalcEdit, "FilteredInvoiceLines.SupportingDocuments.CSI_Quantity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_Quantity)));
			this.SupDocQuantityCalcEdit.CaptionResourceString = null;
			this.SupDocQuantityCalcEdit.DecimalPlaces = 5;
			this.SupDocQuantityCalcEdit.Decimals = 5;
			this.SupDocQuantityCalcEdit.Extra1LabelText = "EUAddInfoSupportingDocumentSchema.xml";
			this.SupDocQuantityCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 86, true);
			this.SupDocQuantityCalcEdit.Name = "SupDocQuantityCalcEdit";
			this.SupDocQuantityCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.SupDocQuantityCalcEdit.TabIndex = 3;
			this.SupDocQuantityCalcEdit.Text = "0.00000";
			this.SupDocQuantityCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CSI_StatusDropEdit
			// 
			this.CSI_StatusDropEdit.AllowDrop = true;
			this.CSI_StatusDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CSI_StatusDropEdit, "FilteredInvoiceLines.SupportingDocuments.CSI_Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_Status)));
			this.CSI_StatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 64, true);
			this.CSI_StatusDropEdit.Name = "CSI_StatusDropEdit";
			this.CSI_StatusDropEdit.PreBoundMaxLength = 3;
			this.CSI_StatusDropEdit.ShouldResizeByMaxLength = true;
			this.CSI_StatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(438, 20, true);
			this.CSI_StatusDropEdit.TabIndex = 2;
			// 
			// CSI_Quantity2CalcEdit
			// 
			this.CSI_Quantity2CalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CSI_Quantity2CalcEdit, "FilteredInvoiceLines.SupportingDocuments.CSI_Quantity2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_Quantity2)));
			this.CSI_Quantity2CalcEdit.CaptionResourceString = null;
			this.CSI_Quantity2CalcEdit.DecimalPlaces = 5;
			this.CSI_Quantity2CalcEdit.Decimals = 5;
			this.CSI_Quantity2CalcEdit.Extra1LabelText = "EUAddInfoSupportingDocumentSchema.xml";
			this.CSI_Quantity2CalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 108, true);
			this.CSI_Quantity2CalcEdit.Name = "CSI_Quantity2CalcEdit";
			this.CSI_Quantity2CalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.CSI_Quantity2CalcEdit.TabIndex = 5;
			this.CSI_Quantity2CalcEdit.Text = "0.00000";
			this.CSI_Quantity2CalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CSI_UnitOfQuantity2TextBox
			// 
			this.CSI_UnitOfQuantity2TextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CSI_UnitOfQuantity2TextBox, "FilteredInvoiceLines.SupportingDocuments.CSI_UnitOfQuantity2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_UnitOfQuantity2)));
			this.CSI_UnitOfQuantity2TextBox.CaptionResourceString = null;
			this.CSI_UnitOfQuantity2TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CSI_UnitOfQuantity2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(448, 108, true);
			this.CSI_UnitOfQuantity2TextBox.Name = "CSI_UnitOfQuantity2TextBox";
			this.CSI_UnitOfQuantity2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.CSI_UnitOfQuantity2TextBox.TabIndex = 6;
			// 
			// CSI_ValueCalcEdit
			// 
			this.CSI_ValueCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CSI_ValueCalcEdit, "FilteredInvoiceLines.SupportingDocuments.CSI_Value");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_Value)));
			this.CSI_ValueCalcEdit.CaptionResourceString = null;
			this.CSI_ValueCalcEdit.DecimalPlaces = 5;
			this.CSI_ValueCalcEdit.Decimals = 5;
			this.CSI_ValueCalcEdit.Extra1LabelText = "EUAddInfoSupportingDocumentSchema.xml";
			this.CSI_ValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 130, true);
			this.CSI_ValueCalcEdit.Name = "CSI_ValueCalcEdit";
			this.CSI_ValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.CSI_ValueCalcEdit.TabIndex = 7;
			this.CSI_ValueCalcEdit.Text = "0.00000";
			this.CSI_ValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CSI_RX_NKCurrencyCodeFindBox
			// 
			this.CSI_RX_NKCurrencyCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CSI_RX_NKCurrencyCodeFindBox, "FilteredInvoiceLines.SupportingDocuments.CSI_RX_NKCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_RX_NKCurrency)));
			this.CSI_RX_NKCurrencyCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(448, 130, true);
			this.CSI_RX_NKCurrencyCodeFindBox.Name = "CSI_RX_NKCurrencyCodeFindBox";
			this.CSI_RX_NKCurrencyCodeFindBox.PreBoundMaxLength = 4;
			this.CSI_RX_NKCurrencyCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 20, true);
			this.CSI_RX_NKCurrencyCodeFindBox.TabIndex = 8;
			// 
			// CSI_DateOfExpiryDateEdit
			// 
			this.CSI_DateOfExpiryDateEdit.AllowDrop = true;
			this.CSI_DateOfExpiryDateEdit.AutoCompleteMonthThreshold = 1;
			this.CSI_DateOfExpiryDateEdit.AutoCompleteYear = true;
			this.CSI_DateOfExpiryDateEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CSI_DateOfExpiryDateEdit, "FilteredInvoiceLines.SupportingDocuments.CSI_DateOfExpiry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_DateOfExpiry)));
			this.CSI_DateOfExpiryDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(448, 152, true);
			this.CSI_DateOfExpiryDateEdit.Name = "CSI_DateOfExpiryDateEdit";
			this.CSI_DateOfExpiryDateEdit.TabIndex = 10;
			// 
			// SupDocReferenceCodeFindBox
			// 
			this.SupDocReferenceCodeFindBox.AllowDrop = true;
			this.SupDocReferenceCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SupDocReferenceCodeFindBox, "FilteredInvoiceLines.SupportingDocuments.CSI_ReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_ReferenceNumber)));
			this.SupDocReferenceCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 41, true);
			this.SupDocReferenceCodeFindBox.Name = "SupDocReferenceCodeFindBox";
			this.SupDocReferenceCodeFindBox.PreBoundMaxLength = 4;
			this.SupDocReferenceCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(414, 20, true);
			this.SupDocReferenceCodeFindBox.TabIndex = 1;
			// 
			// CSI_UnitOfQuantityTextBox
			// 
			this.CSI_UnitOfQuantityTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CSI_UnitOfQuantityTextBox, "FilteredInvoiceLines.SupportingDocuments.CSI_UnitOfQuantity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_UnitOfQuantity)));
			this.CSI_UnitOfQuantityTextBox.CaptionResourceString = null;
			this.CSI_UnitOfQuantityTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CSI_UnitOfQuantityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(448, 86, true);
			this.CSI_UnitOfQuantityTextBox.Name = "CSI_UnitOfQuantityTextBox";
			this.CSI_UnitOfQuantityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.CSI_UnitOfQuantityTextBox.TabIndex = 4;
			// 
			// CSI_DateOfIssueDateEdit
			// 
			this.CSI_DateOfIssueDateEdit.AllowDrop = true;
			this.CSI_DateOfIssueDateEdit.AutoCompleteMonthThreshold = 1;
			this.CSI_DateOfIssueDateEdit.AutoCompleteYear = true;
			this.CSI_DateOfIssueDateEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CSI_DateOfIssueDateEdit, "FilteredInvoiceLines.SupportingDocuments.CSI_DateOfIssue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_DateOfIssue)));
			this.CSI_DateOfIssueDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 152, true);
			this.CSI_DateOfIssueDateEdit.Name = "CSI_DateOfIssueDateEdit";
			this.CSI_DateOfIssueDateEdit.TabIndex = 9;
			// 
			// SupportingDocumentsGroupBox
			// 
			this.SupportingDocumentsGroupBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("F5F297E2-9CAF-43F6-AC49-00360E94E990", "[44] Supporting Documents");
			this.SupportingDocumentsGroupBox.Controls.Add(this.SupDocTypeCodeFindBox);
			this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_ValueCalcEdit);
			this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_DateOfIssueDateEdit);
			this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_UnitOfQuantity2TextBox);
			this.SupportingDocumentsGroupBox.Controls.Add(this.SupDocReferenceTextBox);
			this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_RX_NKCurrencyCodeFindBox);
			this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_UnitOfQuantityTextBox);
			this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_Quantity2CalcEdit);
			this.SupportingDocumentsGroupBox.Controls.Add(this.SupDocQuantityCalcEdit);
			this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_DateOfExpiryDateEdit);
			this.SupportingDocumentsGroupBox.Controls.Add(this.SupDocReferenceCodeFindBox);
			this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_StatusDropEdit);
			this.SupportingDocumentsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SupportingDocumentsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SupportingDocumentsGroupBox.Name = "SupportingDocumentsGroupBox";
			this.SupportingDocumentsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(599, 181, true);
			this.SupportingDocumentsGroupBox.TabIndex = 11;
			this.SupportingDocumentsGroupBox.TabStop = false;
			// 
			// SupportingDocumentsFieldsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SupportingDocumentsGroupBox);
			this.Name = "SupportingDocumentsFieldsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(599, 181, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SupDocTypeCodeFindBox.ResumeLayout(true);
			this.SupDocTypeCodeFindBox.PerformLayout();
			this.CSI_StatusDropEdit.ResumeLayout(true);
			this.CSI_StatusDropEdit.PerformLayout();
			this.CSI_RX_NKCurrencyCodeFindBox.ResumeLayout(true);
			this.CSI_RX_NKCurrencyCodeFindBox.PerformLayout();
			this.CSI_DateOfExpiryDateEdit.ResumeLayout(true);
			this.CSI_DateOfExpiryDateEdit.PerformLayout();
			this.SupDocReferenceCodeFindBox.ResumeLayout(true);
			this.SupDocReferenceCodeFindBox.PerformLayout();
			this.CSI_DateOfIssueDateEdit.ResumeLayout(true);
			this.CSI_DateOfIssueDateEdit.PerformLayout();
			this.SupportingDocumentsGroupBox.ResumeLayout(false);
			this.SupportingDocumentsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public ZArchitecture.GUI.ZCodeFindBox CSI_RX_NKCurrencyCodeFindBox;
		protected ZArchitecture.ZTextBox SupDocReferenceTextBox;
		protected ZArchitecture.GUI.ZCodeFindBox SupDocTypeCodeFindBox;
		protected ZArchitecture.ZCalcEdit SupDocQuantityCalcEdit;
		protected ZArchitecture.GUI.ZDropEdit CSI_StatusDropEdit;
		protected ZArchitecture.ZCalcEdit CSI_Quantity2CalcEdit;
		protected ZArchitecture.ZTextBox CSI_UnitOfQuantity2TextBox;
		protected ZArchitecture.ZCalcEdit CSI_ValueCalcEdit;
		protected ZArchitecture.GUI.ZDateEdit CSI_DateOfExpiryDateEdit;
		protected ZArchitecture.GUI.ZCodeFindBox SupDocReferenceCodeFindBox;
		protected ZArchitecture.ZTextBox CSI_UnitOfQuantityTextBox;
		protected ZArchitecture.GUI.ZDateEdit CSI_DateOfIssueDateEdit;
		protected ZArchitecture.GUI.ZGroupBox SupportingDocumentsGroupBox;
	}
}
