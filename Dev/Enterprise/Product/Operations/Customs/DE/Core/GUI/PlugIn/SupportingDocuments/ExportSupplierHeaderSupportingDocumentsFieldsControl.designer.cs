
namespace Enterprise.Customs.DE.GUI.PlugIn
{
	partial class ExportSupplierHeaderSupportingDocumentsFieldsControl
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
            this.CSI_ItemNumberCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.CSI_DateOfExpiryDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.CSI_DateOfIssueDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.SupportingDocumentsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.CSI_ReferenceNumberCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.CSI_AdditionalDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.CSI_FullTypeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.CSI_DateOfExpiryDateEdit.SuspendLayout();
            this.CSI_DateOfIssueDateEdit.SuspendLayout();
            this.SupportingDocumentsGroupBox.SuspendLayout();
            this.CSI_ReferenceNumberCodeFindBox.SuspendLayout();
            this.CSI_FullTypeCodeFindBox.SuspendLayout();
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
            this.BindingSource.SetBindingMember(this.CSI_ReferenceNumberTextBox, "Invoices.SupportingDocuments.CSI_ReferenceNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).Invoices)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_ReferenceNumber)));
            this.CSI_ReferenceNumberTextBox.CaptionResourceString = null;
            this.CSI_ReferenceNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.CSI_ReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 37, true);
            this.CSI_ReferenceNumberTextBox.Name = "CSI_ReferenceNumberTextBox";
            this.CSI_ReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(675, 15, true);
            this.CSI_ReferenceNumberTextBox.TabIndex = 1;
            // 
            // CSI_ItemNumberCalcEdit
            // 
            this.CSI_ItemNumberCalcEdit.BackColor = System.Drawing.SystemColors.Window;
            this.BindingSource.SetBindingMember(this.CSI_ItemNumberCalcEdit, "Invoices.SupportingDocuments.CSI_ItemNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.DE.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).Invoices)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_ItemNumber)));
            this.CSI_ItemNumberCalcEdit.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("D8DEA6B1-A70D-4418-B864-7CFF39021DD1", "Document Line Item Number");
            this.CSI_ItemNumberCalcEdit.DecimalPlaces = 0;
            this.CSI_ItemNumberCalcEdit.Decimals = 0;
            this.CSI_ItemNumberCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 103, true);
            this.CSI_ItemNumberCalcEdit.Name = "CSI_ItemNumberCalcEdit";
            this.CSI_ItemNumberCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 15, true);
            this.CSI_ItemNumberCalcEdit.TabIndex = 5;
            this.CSI_ItemNumberCalcEdit.Text = "0";
            this.CSI_ItemNumberCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // CSI_DateOfExpiryDateEdit
            // 
            this.CSI_DateOfExpiryDateEdit.AllowDrop = true;
            this.CSI_DateOfExpiryDateEdit.AutoCompleteMonthThreshold = 1;
            this.CSI_DateOfExpiryDateEdit.BackColor = System.Drawing.SystemColors.Window;
            this.BindingSource.SetBindingMember(this.CSI_DateOfExpiryDateEdit, "Invoices.SupportingDocuments.CSI_DateOfExpiry");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).Invoices)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_DateOfExpiry)));
            this.CSI_DateOfExpiryDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(365, 59, true);
            this.CSI_DateOfExpiryDateEdit.Name = "CSI_DateOfExpiryDateEdit";
            this.CSI_DateOfExpiryDateEdit.TabIndex = 3;
            // 
            // CSI_DateOfIssueDateEdit
            // 
            this.CSI_DateOfIssueDateEdit.AllowDrop = true;
            this.CSI_DateOfIssueDateEdit.AutoCompleteMonthThreshold = 1;
            this.CSI_DateOfIssueDateEdit.BackColor = System.Drawing.SystemColors.Window;
            this.BindingSource.SetBindingMember(this.CSI_DateOfIssueDateEdit, "Invoices.SupportingDocuments.CSI_DateOfIssue");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).Invoices)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_DateOfIssue)));
            this.CSI_DateOfIssueDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 59, true);
            this.CSI_DateOfIssueDateEdit.Name = "CSI_DateOfIssueDateEdit";
            this.CSI_DateOfIssueDateEdit.TabIndex = 2;
            // 
            // SupportingDocumentsGroupBox
            // 
            this.SupportingDocumentsGroupBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("DD8B5F98-57FD-45D0-9FBF-FBE008DA91DA", "[44] Supporting Documents");
            this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_ReferenceNumberCodeFindBox);
            this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_AdditionalDescriptionTextBox);
            this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_FullTypeCodeFindBox);
            this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_DateOfIssueDateEdit);
            this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_ReferenceNumberTextBox);
            this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_ItemNumberCalcEdit);
            this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_DateOfExpiryDateEdit);
            this.SupportingDocumentsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SupportingDocumentsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.SupportingDocumentsGroupBox.Name = "SupportingDocumentsGroupBox";
            this.SupportingDocumentsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(836, 132, true);
            this.SupportingDocumentsGroupBox.TabIndex = 11;
            this.SupportingDocumentsGroupBox.TabStop = false;
            // 
            // CSI_ReferenceNumberCodeFindBox
            // 
            this.CSI_ReferenceNumberCodeFindBox.AllowDrop = true;
            this.CSI_ReferenceNumberCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.CSI_ReferenceNumberCodeFindBox, "Invoices.SupportingDocuments.CSI_ReferenceNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).Invoices)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_ReferenceNumber)));
            this.CSI_ReferenceNumberCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 37, true);
            this.CSI_ReferenceNumberCodeFindBox.Name = "CSI_ReferenceNumberCodeFindBox";
            this.CSI_ReferenceNumberCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.CSI_ReferenceNumberCodeFindBox.ParentType = null;
            this.CSI_ReferenceNumberCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(675, 15, true);
            this.CSI_ReferenceNumberCodeFindBox.TabIndex = 1;
            // 
            // CSI_AdditionalDescriptionTextBox
            // 
            this.CSI_AdditionalDescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.CSI_AdditionalDescriptionTextBox, "Invoices.SupportingDocuments.CSI_AdditionalDescription");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).Invoices)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_AdditionalDescription)));
            this.CSI_AdditionalDescriptionTextBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("5E543347-C24B-4668-A72A-F4158990A9C6", "Issuing Authority");
            this.CSI_AdditionalDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.CSI_AdditionalDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 81, true);
            this.CSI_AdditionalDescriptionTextBox.Name = "CSI_AdditionalDescriptionTextBox";
            this.CSI_AdditionalDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(675, 15, true);
            this.CSI_AdditionalDescriptionTextBox.TabIndex = 4;
            // 
            // CSI_FullTypeCodeFindBox
            // 
            this.CSI_FullTypeCodeFindBox.AllowDrop = true;
            this.CSI_FullTypeCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.CSI_FullTypeCodeFindBox, "Invoices.SupportingDocuments.CSI_FullType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).Invoices)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_FullType)));
            this.CSI_FullTypeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 15, true);
            this.CSI_FullTypeCodeFindBox.Name = "CSI_FullTypeCodeFindBox";
            this.CSI_FullTypeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.CSI_FullTypeCodeFindBox.ParentType = null;
            this.CSI_FullTypeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(675, 15, true);
            this.CSI_FullTypeCodeFindBox.TabIndex = 0;
            // 
            // ExportSupplierHeaderSupportingDocumentsFieldsControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.SupportingDocumentsGroupBox);
            this.Name = "ExportSupplierHeaderSupportingDocumentsFieldsControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(836, 132, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.CSI_DateOfExpiryDateEdit.ResumeLayout(true);
            this.CSI_DateOfExpiryDateEdit.PerformLayout();
            this.CSI_DateOfIssueDateEdit.ResumeLayout(true);
            this.CSI_DateOfIssueDateEdit.PerformLayout();
            this.SupportingDocumentsGroupBox.ResumeLayout(false);
            this.SupportingDocumentsGroupBox.PerformLayout();
            this.CSI_ReferenceNumberCodeFindBox.ResumeLayout(true);
            this.CSI_ReferenceNumberCodeFindBox.PerformLayout();
            this.CSI_FullTypeCodeFindBox.ResumeLayout(true);
            this.CSI_FullTypeCodeFindBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox CSI_ReferenceNumberTextBox;
		private ZArchitecture.ZCalcEdit CSI_ItemNumberCalcEdit;
		private ZArchitecture.GUI.ZDateEdit CSI_DateOfExpiryDateEdit;
		private ZArchitecture.GUI.ZDateEdit CSI_DateOfIssueDateEdit;
		private ZArchitecture.GUI.ZGroupBox SupportingDocumentsGroupBox;
		private ZArchitecture.GUI.ZCodeFindBox CSI_FullTypeCodeFindBox;
		private ZArchitecture.ZTextBox CSI_AdditionalDescriptionTextBox;
		private ZArchitecture.GUI.ZCodeFindBox CSI_ReferenceNumberCodeFindBox;
	}
}
