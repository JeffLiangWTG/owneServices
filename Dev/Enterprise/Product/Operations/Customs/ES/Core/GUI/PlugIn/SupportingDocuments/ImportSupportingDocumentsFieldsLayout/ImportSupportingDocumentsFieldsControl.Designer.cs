namespace Enterprise.Customs.ES.GUI;

partial class ImportSupportingDocumentsFieldsControl
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
		this.CodeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
		this.AdditionalDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
		this.ItemNumberCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
		this.NKCountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
		this.ImportSupportingDocumentReferenceNumberUserControl = new Enterprise.Customs.ES.GUI.ImportSupportingDocumentReferenceNumberUserControl();
		this.QuantityAndUnitUserControl = new Enterprise.Customs.ES.GUI.QuantityAndUnitUserControl();
		this.SecondQuantityAndUnitUserControl = new Enterprise.Customs.ES.GUI.SecondQuantityAndUnitUserControl();
		this.StatusAndProcedureUserControl = new Enterprise.Customs.ES.GUI.StatusAndProcedureUserControl();
		this.ValueAndCurrencyUserControl = new Enterprise.Customs.ES.GUI.ValueAndCurrencyUserControl();
		this.DateOfIssueAndExpiryUserControl = new Enterprise.Customs.ES.GUI.DateOfIssueAndExpiryUserControl();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		this.CodeCodeFindBox.SuspendLayout();
		this.AdditionalDescriptionTextBox.SuspendLayout();
		this.ItemNumberCalcEdit.SuspendLayout();
		this.NKCountryCodeFindBox.SuspendLayout();
		this.ImportSupportingDocumentReferenceNumberUserControl.SuspendLayout();
		this.QuantityAndUnitUserControl.SuspendLayout();
		this.SecondQuantityAndUnitUserControl.SuspendLayout();
		this.StatusAndProcedureUserControl.SuspendLayout();
		this.ValueAndCurrencyUserControl.SuspendLayout();
		this.DateOfIssueAndExpiryUserControl.SuspendLayout();
		this.SuspendLayout();
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.Business.Declaration.JobDeclaration);
		// 
		// CodeCodeFindBox
		// 
		this.CodeCodeFindBox.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.CodeCodeFindBox, "FilteredInvoiceLines.SupportingDocuments.CSI_Code");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.Declaration.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_Code)));
		this.CodeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 19, true);
		this.CodeCodeFindBox.Name = "CodeCodeFindBox";
		this.CodeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
		this.CodeCodeFindBox.ParentType = null;
		this.CodeCodeFindBox.PreBoundMaxLength = 5;
		this.CodeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(362, 20, true);
		this.CodeCodeFindBox.TabIndex = 0;
		// 
		// ImportSupportingDocumentReferenceNumberUserControl
		// 
		this.ImportSupportingDocumentReferenceNumberUserControl.AllowDrop = true;
		this.ImportSupportingDocumentReferenceNumberUserControl.AutoSize = true;
		this.ImportSupportingDocumentReferenceNumberUserControl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		this.BindingSource.SetBindingMember(this.ImportSupportingDocumentReferenceNumberUserControl, "FilteredInvoiceLines.SupportingDocuments");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.ES.Business.Declaration.SupportingDocument)(((Enterprise.Customs.ES.Business.Declaration.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)))));
		this.ImportSupportingDocumentReferenceNumberUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 41, true);
		this.ImportSupportingDocumentReferenceNumberUserControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
		this.ImportSupportingDocumentReferenceNumberUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
		this.ImportSupportingDocumentReferenceNumberUserControl.Name = "ImportSupportingDocumentReferenceNumberUserControl";
		this.ImportSupportingDocumentReferenceNumberUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(362, 20, true);
		this.ImportSupportingDocumentReferenceNumberUserControl.TabIndex = 1;
		// 
		// QuantityAndUnitUserControl
		// 
		this.QuantityAndUnitUserControl.AllowDrop = true;
		this.QuantityAndUnitUserControl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		this.BindingSource.SetBindingMember(this.QuantityAndUnitUserControl, ".");
		this.QuantityAndUnitUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 114, true);
		this.QuantityAndUnitUserControl.Name = "QuantityAndUnitUserControl";
		this.QuantityAndUnitUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(366, 20, true);
		this.QuantityAndUnitUserControl.TabIndex = 3;
		// 
		// SecondQuantityAndUnitUserControl
		// 
		this.SecondQuantityAndUnitUserControl.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.SecondQuantityAndUnitUserControl, ".");
		this.SecondQuantityAndUnitUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 146, true);
		this.SecondQuantityAndUnitUserControl.Name = "SecondQuantityAndUnitUserControl";
		this.SecondQuantityAndUnitUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(387, 20, true);
		this.SecondQuantityAndUnitUserControl.TabIndex = 4;
		// 
		// StatusAndProcedureUserControl
		// 
		this.StatusAndProcedureUserControl.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.StatusAndProcedureUserControl, ".");
		this.StatusAndProcedureUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 71, true);
		this.StatusAndProcedureUserControl.Name = "StatusAndProcedureUserControl";
		this.StatusAndProcedureUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(402, 20, true);
		this.StatusAndProcedureUserControl.TabIndex = 2;
		// 
		// ValueAndCurrencyUserControl
		// 
		this.ValueAndCurrencyUserControl.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.ValueAndCurrencyUserControl, ".");
		this.ValueAndCurrencyUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 180, true);
		this.ValueAndCurrencyUserControl.Name = "ValueAndCurrencyUserControl";
		this.ValueAndCurrencyUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(395, 20, true);
		this.ValueAndCurrencyUserControl.TabIndex = 5;
		// 
		// DateOfIssueAndExpiryUserControl
		// 
		this.DateOfIssueAndExpiryUserControl.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.DateOfIssueAndExpiryUserControl, ".");
		this.DateOfIssueAndExpiryUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 215, true);
		this.DateOfIssueAndExpiryUserControl.Name = "DateOfIssueAndExpiryUserControl";
		this.DateOfIssueAndExpiryUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(362, 20, true);
		this.DateOfIssueAndExpiryUserControl.TabIndex = 6;
		// 
		// AdditionalDescriptionTextBox
		// 
		this.BindingSource.SetBindingMember(this.AdditionalDescriptionTextBox, "FilteredInvoiceLines.SupportingDocuments.CSI_AdditionalDescription");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.Declaration.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_AdditionalDescription)));
		this.AdditionalDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
		this.AdditionalDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 195, true);
		this.AdditionalDescriptionTextBox.Name = "AdditionalDescriptionTextBox";
		this.AdditionalDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(362, 20, true);
		this.AdditionalDescriptionTextBox.TabIndex = 7;
		// 
		// ItemNumberCalcEdit
		// 
		this.ItemNumberCalcEdit.BackColor = System.Drawing.SystemColors.Window;
		this.BindingSource.SetBindingMember(this.ItemNumberCalcEdit, "FilteredInvoiceLines.SupportingDocuments.CSI_ItemNumber");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Customs.ES.Business.Declaration.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_ItemNumber)));
		this.ItemNumberCalcEdit.DecimalPlaces = 0;
		this.ItemNumberCalcEdit.Decimals = 0;
		this.ItemNumberCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 216, true);
		this.ItemNumberCalcEdit.Name = "ItemNumberCalcEdit";
		this.ItemNumberCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 20, true);
		this.ItemNumberCalcEdit.TabIndex = 8;
		this.ItemNumberCalcEdit.Text = "0";
		this.ItemNumberCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		// 
		// NKCountryCodeFindBox
		// 
		this.NKCountryCodeFindBox.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.NKCountryCodeFindBox, "FilteredInvoiceLines.SupportingDocuments.CSI_RN_NKCountryCode");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.Declaration.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_RN_NKCountryCode)));
		this.NKCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 19, true);
		this.NKCountryCodeFindBox.Name = "NKCountryCodeFindBox";
		this.NKCountryCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
		this.NKCountryCodeFindBox.ParentType = null;
		this.NKCountryCodeFindBox.PreBoundMaxLength = 5;
		this.NKCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(362, 20, true);
		this.NKCountryCodeFindBox.TabIndex = 9;
		// 
		// ImportSupportingDocumentsFieldsControl
		// 
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.CaptionRenderingEnabled = true;
		this.Controls.Add(this.DateOfIssueAndExpiryUserControl);
		this.Controls.Add(this.ValueAndCurrencyUserControl);
		this.Controls.Add(this.StatusAndProcedureUserControl);
		this.Controls.Add(this.SecondQuantityAndUnitUserControl);
		this.Controls.Add(this.QuantityAndUnitUserControl);
		this.Controls.Add(this.CodeCodeFindBox);
		this.Controls.Add(this.AdditionalDescriptionTextBox);
		this.Controls.Add(this.ItemNumberCalcEdit);
		this.Controls.Add(this.NKCountryCodeFindBox);
		this.Controls.Add(this.ImportSupportingDocumentReferenceNumberUserControl);
		this.Name = "ImportSupportingDocumentsFieldsControl";
		this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(607, 265, true);
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		this.CodeCodeFindBox.ResumeLayout(true);
		this.CodeCodeFindBox.PerformLayout();
		this.AdditionalDescriptionTextBox.ResumeLayout(true);
		this.AdditionalDescriptionTextBox.PerformLayout();
		this.ItemNumberCalcEdit.ResumeLayout(true);
		this.ItemNumberCalcEdit.PerformLayout();
		this.NKCountryCodeFindBox.ResumeLayout(true);
		this.NKCountryCodeFindBox.PerformLayout();
		this.ImportSupportingDocumentReferenceNumberUserControl.ResumeLayout(true);
		this.ImportSupportingDocumentReferenceNumberUserControl.PerformLayout();
		this.QuantityAndUnitUserControl.ResumeLayout(true);
		this.QuantityAndUnitUserControl.PerformLayout();
		this.SecondQuantityAndUnitUserControl.ResumeLayout(true);
		this.SecondQuantityAndUnitUserControl.PerformLayout();
		this.StatusAndProcedureUserControl.ResumeLayout(true);
		this.StatusAndProcedureUserControl.PerformLayout();
		this.ValueAndCurrencyUserControl.ResumeLayout(true);
		this.ValueAndCurrencyUserControl.PerformLayout();
		this.DateOfIssueAndExpiryUserControl.ResumeLayout(true);
		this.DateOfIssueAndExpiryUserControl.PerformLayout();
		this.ResumeLayout(false);
		this.PerformLayout();

	}

	#endregion
	internal ImportSupportingDocumentReferenceNumberUserControl ImportSupportingDocumentReferenceNumberUserControl;
	internal ZArchitecture.GUI.ZCodeFindBox CodeCodeFindBox;
	internal ZArchitecture.ZTextBox AdditionalDescriptionTextBox;
	internal ZArchitecture.ZCalcEdit ItemNumberCalcEdit;
	internal ZArchitecture.GUI.ZCodeFindBox NKCountryCodeFindBox;
	internal QuantityAndUnitUserControl QuantityAndUnitUserControl;
	internal SecondQuantityAndUnitUserControl SecondQuantityAndUnitUserControl;
	internal StatusAndProcedureUserControl StatusAndProcedureUserControl;
	internal ValueAndCurrencyUserControl ValueAndCurrencyUserControl;
	internal DateOfIssueAndExpiryUserControl DateOfIssueAndExpiryUserControl;
}
