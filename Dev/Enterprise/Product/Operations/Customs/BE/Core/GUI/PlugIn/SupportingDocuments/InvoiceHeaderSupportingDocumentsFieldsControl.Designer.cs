
namespace Enterprise.Customs.BE.GUI.PlugIn
{
	partial class InvoiceHeaderSupportingDocumentsFieldsControl
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
			this.CSI_DateOfExpiryDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.SupportingDocumentsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CSI_CodeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CSI_AdditionalDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CSI_ItemNumberCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CSI_DateOfExpiryDateEdit.SuspendLayout();
			this.SupportingDocumentsGroupBox.SuspendLayout();
			this.CSI_CodeCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BE.Business.Declaration.JobDeclaration);
			// 
			// CSI_ReferenceNumberTextBox
			// 
			this.CSI_ReferenceNumberTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.CSI_ReferenceNumberTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CSI_ReferenceNumberTextBox, "FilteredInvoiceLines.SupportingDocuments.CSI_ReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BE.Business.Declaration.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.BE.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.BE.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_ReferenceNumber)));
			this.CSI_ReferenceNumberTextBox.CaptionResourceString = null;
			this.CSI_ReferenceNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CSI_ReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 37, true);
			this.CSI_ReferenceNumberTextBox.Name = "CSI_ReferenceNumberTextBox";
			this.CSI_ReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(675, 17, true);
			this.CSI_ReferenceNumberTextBox.TabIndex = 1;
			// 
			// CSI_DateOfExpiryDateEdit
			// 
			this.CSI_DateOfExpiryDateEdit.CaptionResourceString = Enterprise.Customs.BE.GUI.Res.GetData("451DCBC1-74CA-4DA4-97FE-22606C92E1DC", "Date of Validity");
			this.CSI_DateOfExpiryDateEdit.AllowDrop = true;
			this.CSI_DateOfExpiryDateEdit.AutoCompleteMonthThreshold = 1;
			this.CSI_DateOfExpiryDateEdit.AutoCompleteYear = true;
			this.CSI_DateOfExpiryDateEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CSI_DateOfExpiryDateEdit, "FilteredInvoiceLines.SupportingDocuments.CSI_DateOfExpiry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BE.Business.Declaration.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.BE.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.BE.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_DateOfExpiry)));
			this.CSI_DateOfExpiryDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 58, true);
			this.CSI_DateOfExpiryDateEdit.Name = "CSI_DateOfExpiryDateEdit";
			this.CSI_DateOfExpiryDateEdit.TabIndex = 3;
			// 
			// SupportingDocumentsGroupBox
			// 
			this.SupportingDocumentsGroupBox.CaptionResourceString = Enterprise.Customs.BE.GUI.Res.GetData("DC0B9050-A645-4B22-A8F1-A20DC89AAABA", "[44] Supporting Documents");
			this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_CodeCodeFindBox);
			this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_DateOfExpiryDateEdit);
			this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_ReferenceNumberTextBox);
			this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_AdditionalDescriptionTextBox);
			this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_ItemNumberCalcEdit);
			this.SupportingDocumentsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SupportingDocumentsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SupportingDocumentsGroupBox.Name = "SupportingDocumentsGroupBox";
			this.SupportingDocumentsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(836, 200, true);
			this.SupportingDocumentsGroupBox.TabIndex = 11;
			this.SupportingDocumentsGroupBox.TabStop = false;
			// 
			// CSI_CodeCodeFindBox
			// 
			this.CSI_CodeCodeFindBox.AllowDrop = true;
			this.CSI_CodeCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CSI_CodeCodeFindBox, "FilteredInvoiceLines.SupportingDocuments.CSI_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BE.Business.Declaration.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.BE.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.BE.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_Code)));
			this.CSI_CodeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 15, true);
			this.CSI_CodeCodeFindBox.Name = "CSI_CodeCodeFindBox";
			this.CSI_CodeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CSI_CodeCodeFindBox.ParentType = null;
			this.CSI_CodeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(675, 17, true);
			this.CSI_CodeCodeFindBox.TabIndex = 0;
			// 
			// CSI_AdditionalDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.CSI_AdditionalDescriptionTextBox, "FilteredInvoiceLines.SupportingDocuments.CSI_AdditionalDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BE.Business.Declaration.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.BE.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.BE.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_AdditionalDescription)));
			this.CSI_AdditionalDescriptionTextBox.CaptionResourceString = Enterprise.Customs.BE.GUI.Res.GetData("2D6A91CA-4A6E-487A-9DD7-451AB9D7D69A", "Issuing Authority");
			this.CSI_AdditionalDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CSI_AdditionalDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 81, true);
			this.CSI_AdditionalDescriptionTextBox.Name = "CSI_AdditionalDescriptionTextBox";
			this.CSI_AdditionalDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(408, 17, true);
			this.CSI_AdditionalDescriptionTextBox.TabIndex = 4;
			// 
			// CSI_ItemNumberCalcEdit
			// 
			this.CSI_ItemNumberCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CSI_ItemNumberCalcEdit, "FilteredInvoiceLines.SupportingDocuments.CSI_ItemNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.BE.Business.Declaration.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.BE.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.BE.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_ItemNumber)));
			this.CSI_ItemNumberCalcEdit.CaptionResourceString = Enterprise.Customs.BE.GUI.Res.GetData("9CA81780-5D5A-42FB-8488-BD4DEE3D161B", "Document Line Item Number");
			this.CSI_ItemNumberCalcEdit.DecimalPlaces = 0;
			this.CSI_ItemNumberCalcEdit.Decimals = 0;
			this.CSI_ItemNumberCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 104, true);
			this.CSI_ItemNumberCalcEdit.Name = "CSI_ItemNumberCalcEdit";
			this.CSI_ItemNumberCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 17, true);
			this.CSI_ItemNumberCalcEdit.TabIndex = 5;
			this.CSI_ItemNumberCalcEdit.Text = "0";
			this.CSI_ItemNumberCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// InvoiceHeaderSupportingDocumentsFieldsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SupportingDocumentsGroupBox);
			this.Name = "InvoiceHeaderSupportingDocumentsFieldsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(836, 200, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CSI_DateOfExpiryDateEdit.ResumeLayout(true);
			this.CSI_DateOfExpiryDateEdit.PerformLayout();
			this.SupportingDocumentsGroupBox.ResumeLayout(false);
			this.SupportingDocumentsGroupBox.PerformLayout();
			this.CSI_CodeCodeFindBox.ResumeLayout(true);
			this.CSI_CodeCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox CSI_ReferenceNumberTextBox;
		private ZArchitecture.GUI.ZDateEdit CSI_DateOfExpiryDateEdit;
		private ZArchitecture.GUI.ZGroupBox SupportingDocumentsGroupBox;
		private ZArchitecture.GUI.ZCodeFindBox CSI_CodeCodeFindBox;
		private ZArchitecture.ZTextBox CSI_AdditionalDescriptionTextBox;
		private ZArchitecture.ZCalcEdit CSI_ItemNumberCalcEdit;
	}
}
