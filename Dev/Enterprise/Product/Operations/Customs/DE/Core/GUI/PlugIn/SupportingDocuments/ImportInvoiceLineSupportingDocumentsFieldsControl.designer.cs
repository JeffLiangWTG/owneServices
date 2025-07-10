namespace Enterprise.Customs.DE.GUI.PlugIn
{
	partial class ImportInvoiceLineSupportingDocumentsFieldsControl
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
			this.CSI_QuantityCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.SupportingDocumentsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CSI_StatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CSI_ReferenceNumberCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CSI_UnitOfQuantityDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CSI_CodeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CSI_DateOfIssueDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SupportingDocumentsGroupBox.SuspendLayout();
			this.CSI_StatusDropEdit.SuspendLayout();
			this.CSI_ReferenceNumberCodeFindBox.SuspendLayout();
			this.CSI_UnitOfQuantityDropEdit.SuspendLayout();
			this.CSI_CodeCodeFindBox.SuspendLayout();
			this.CSI_DateOfIssueDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.Declaration.JobDeclaration);
			// 
			// CSI_ReferenceNumberTextBox
			// 
			this.CSI_ReferenceNumberTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.CSI_ReferenceNumberTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CSI_ReferenceNumberTextBox, "FilteredInvoiceLines.SupportingDocuments.CSI_ReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_ReferenceNumber)));
			this.CSI_ReferenceNumberTextBox.CaptionResourceString = null;
			this.CSI_ReferenceNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CSI_ReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 40, true);
			this.CSI_ReferenceNumberTextBox.Name = "CSI_ReferenceNumberTextBox";
			this.CSI_ReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(477, 20, true);
			this.CSI_ReferenceNumberTextBox.TabIndex = 1;
			// 
			// CSI_QuantityCalcEdit
			// 
			this.CSI_QuantityCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CSI_QuantityCalcEdit, "FilteredInvoiceLines.SupportingDocuments.CSI_Quantity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.DE.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_Quantity)));
			this.CSI_QuantityCalcEdit.CaptionResourceString = null;
			this.CSI_QuantityCalcEdit.DecimalPlaces = 3;
			this.CSI_QuantityCalcEdit.Decimals = 3;
			this.CSI_QuantityCalcEdit.Extra1LabelText = "EUAddInfoSupportingDocumentSchema.xml";
			this.CSI_QuantityCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 86, true);
			this.CSI_QuantityCalcEdit.Name = "CSI_QuantityCalcEdit";
			this.CSI_QuantityCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.CSI_QuantityCalcEdit.TabIndex = 3;
			this.CSI_QuantityCalcEdit.Text = "0.00000";
			this.CSI_QuantityCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// SupportingDocumentsGroupBox
			// 
			this.SupportingDocumentsGroupBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("E69940D6-16AA-47F3-AC06-F9F841682503", "[44] Supporting Documents");
			this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_StatusDropEdit);
			this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_ReferenceNumberCodeFindBox);
			this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_UnitOfQuantityDropEdit);
			this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_CodeCodeFindBox);
			this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_DateOfIssueDateEdit);
			this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_ReferenceNumberTextBox);
			this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_QuantityCalcEdit);
			this.SupportingDocumentsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SupportingDocumentsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SupportingDocumentsGroupBox.Name = "SupportingDocumentsGroupBox";
			this.SupportingDocumentsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(653, 137, true);
			this.SupportingDocumentsGroupBox.TabIndex = 11;
			this.SupportingDocumentsGroupBox.TabStop = false;
			// 
			// CSI_StatusDropEdit
			// 
			this.CSI_StatusDropEdit.AllowDrop = true;
			this.CSI_StatusDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CSI_StatusDropEdit, "FilteredInvoiceLines.SupportingDocuments.CSI_Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_Status)));
			this.CSI_StatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 63, true);
			this.CSI_StatusDropEdit.Name = "CSI_StatusDropEdit";
			this.CSI_StatusDropEdit.ShouldResizeByMaxLength = true;
			this.CSI_StatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(492, 20, true);
			this.CSI_StatusDropEdit.TabIndex = 2;
			// 
			// CSI_ReferenceNumberCodeFindBox
			// 
			this.CSI_ReferenceNumberCodeFindBox.AllowDrop = true;
			this.CSI_ReferenceNumberCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CSI_ReferenceNumberCodeFindBox, "FilteredInvoiceLines.SupportingDocuments.CSI_ReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_ReferenceNumber)));
			this.CSI_ReferenceNumberCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 40, true);
			this.CSI_ReferenceNumberCodeFindBox.Name = "CSI_ReferenceNumberCodeFindBox";
			this.CSI_ReferenceNumberCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(492, 20, true);
			this.CSI_ReferenceNumberCodeFindBox.TabIndex = 1;
			// 
			// CSI_UnitOfQuantityDropEdit
			// 
			this.CSI_UnitOfQuantityDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CSI_UnitOfQuantityDropEdit, "FilteredInvoiceLines.SupportingDocuments.CSI_UnitOfQuantity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_UnitOfQuantity)));
			this.CSI_UnitOfQuantityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(415, 86, true);
			this.CSI_UnitOfQuantityDropEdit.Name = "CSI_UnitOfQuantityDropEdit";
			this.CSI_UnitOfQuantityDropEdit.ShouldResizeByMaxLength = true;
			this.CSI_UnitOfQuantityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.CSI_UnitOfQuantityDropEdit.TabIndex = 4;
			// 
			// CSI_CodeCodeFindBox
			// 
			this.CSI_CodeCodeFindBox.AllowDrop = true;
			this.CSI_CodeCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CSI_CodeCodeFindBox, "FilteredInvoiceLines.SupportingDocuments.CSI_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_Code)));
			this.CSI_CodeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 17, true);
			this.CSI_CodeCodeFindBox.Name = "CSI_CodeCodeFindBox";
			this.CSI_CodeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(492, 20, true);
			this.CSI_CodeCodeFindBox.TabIndex = 0;
			// 
			// CSI_DateOfIssueDateEdit
			// 
			this.CSI_DateOfIssueDateEdit.AllowDrop = true;
			this.CSI_DateOfIssueDateEdit.AutoCompleteMonthThreshold = 1;
			this.CSI_DateOfIssueDateEdit.AutoCompleteYear = true;
			this.CSI_DateOfIssueDateEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CSI_DateOfIssueDateEdit, "FilteredInvoiceLines.SupportingDocuments.CSI_DateOfIssue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_DateOfIssue)));
			this.CSI_DateOfIssueDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 108, true);
			this.CSI_DateOfIssueDateEdit.Name = "CSI_DateOfIssueDateEdit";
			this.CSI_DateOfIssueDateEdit.TabIndex = 5;
			// 
			// ImportInvoiceLineSupportingDocumentsFieldsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SupportingDocumentsGroupBox);
			this.Name = "ImportInvoiceLineSupportingDocumentsFieldsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(653, 137, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SupportingDocumentsGroupBox.ResumeLayout(false);
			this.SupportingDocumentsGroupBox.PerformLayout();
			this.CSI_StatusDropEdit.ResumeLayout(true);
			this.CSI_StatusDropEdit.PerformLayout();
			this.CSI_ReferenceNumberCodeFindBox.ResumeLayout(true);
			this.CSI_ReferenceNumberCodeFindBox.PerformLayout();
			this.CSI_UnitOfQuantityDropEdit.ResumeLayout(true);
			this.CSI_UnitOfQuantityDropEdit.PerformLayout();
			this.CSI_CodeCodeFindBox.ResumeLayout(true);
			this.CSI_CodeCodeFindBox.PerformLayout();
			this.CSI_DateOfIssueDateEdit.ResumeLayout(true);
			this.CSI_DateOfIssueDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox CSI_ReferenceNumberTextBox;
		private ZArchitecture.ZCalcEdit CSI_QuantityCalcEdit;
		private ZArchitecture.GUI.ZGroupBox SupportingDocumentsGroupBox;
		private ZArchitecture.GUI.ZCodeFindBox CSI_CodeCodeFindBox;
		private ZArchitecture.GUI.ZDateEdit CSI_DateOfIssueDateEdit;
		private ZArchitecture.GUI.ZDropEdit CSI_UnitOfQuantityDropEdit;
		private ZArchitecture.GUI.ZCodeFindBox CSI_ReferenceNumberCodeFindBox;
		private ZArchitecture.GUI.ZDropEdit CSI_StatusDropEdit;
	}
}
