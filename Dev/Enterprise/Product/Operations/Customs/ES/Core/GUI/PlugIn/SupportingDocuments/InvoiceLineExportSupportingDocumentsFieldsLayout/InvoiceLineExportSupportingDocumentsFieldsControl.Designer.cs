namespace Enterprise.Customs.ES.GUI;

partial class InvoiceLineExportSupportingDocumentsFieldsControl
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
		this.ReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
		this.CodeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
		this.StatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
		this.ReferenceNumberCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
		this.AdditionalDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
		this.ItemNumberCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
		this.QuantityAndUnitUserControl = new Enterprise.Customs.ES.GUI.QuantityAndUnitUserControl();
		this.SecondQuantityAndUnitUserControl = new Enterprise.Customs.ES.GUI.SecondQuantityAndUnitUserControl();
		this.ValueAndCurrencyUserControl = new Enterprise.Customs.ES.GUI.ValueAndCurrencyUserControl();
		this.DateOfIssueAndExpiryUserControl = new Enterprise.Customs.ES.GUI.DateOfIssueAndExpiryUserControl();
		this.PackQuantityAndUnitUserControl = new Enterprise.Customs.ES.GUI.PackQuantityAndUnitUserControl();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		this.CodeCodeFindBox.SuspendLayout();
		this.StatusDropEdit.SuspendLayout();
		this.ReferenceNumberCodeFindBox.SuspendLayout();
		this.QuantityAndUnitUserControl.SuspendLayout();
		this.SecondQuantityAndUnitUserControl.SuspendLayout();
		this.ValueAndCurrencyUserControl.SuspendLayout();
		this.DateOfIssueAndExpiryUserControl.SuspendLayout();
		this.PackQuantityAndUnitUserControl.SuspendLayout();
		this.SuspendLayout();
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.Business.Declaration.JobDeclaration);
		// 
		// ReferenceNumberTextBox
		// 
		this.ReferenceNumberTextBox.BackColor = System.Drawing.SystemColors.Window;
		this.BindingSource.SetBindingMember(this.ReferenceNumberTextBox, "FilteredInvoiceLines.SupportingDocuments.CSI_ReferenceNumber");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.Declaration.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_ReferenceNumber)));
		this.ReferenceNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
		this.ReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 41, true);
		this.ReferenceNumberTextBox.Name = "ReferenceNumberTextBox";
		this.ReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(362, 20, true);
		this.ReferenceNumberTextBox.TabIndex = 1;
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
		this.CodeCodeFindBox.PreBoundMaxLength = 4;
		this.CodeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(362, 20, true);
		this.CodeCodeFindBox.TabIndex = 0;
		// 
		// StatusDropEdit
		// 
		this.StatusDropEdit.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.StatusDropEdit, "FilteredInvoiceLines.SupportingDocuments.CSI_Status");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.Business.Declaration.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_Status)));
		this.StatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 64, true);
		this.StatusDropEdit.Name = "StatusDropEdit";
		this.StatusDropEdit.PreBoundMaxLength = 3;
		this.StatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
		this.StatusDropEdit.TabIndex = 2;
		// 
		// ReferenceNumberCodeFindBox
		// 
		this.ReferenceNumberCodeFindBox.AllowDrop = true;
		this.ReferenceNumberCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
		this.BindingSource.SetBindingMember(this.ReferenceNumberCodeFindBox, "FilteredInvoiceLines.SupportingDocuments.CSI_ReferenceNumber");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.Declaration.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_ReferenceNumber)));
		this.ReferenceNumberCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 41, true);
		this.ReferenceNumberCodeFindBox.Name = "ReferenceNumberCodeFindBox";
		this.ReferenceNumberCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
		this.ReferenceNumberCodeFindBox.ParentType = null;
		this.ReferenceNumberCodeFindBox.PreBoundMaxLength = 4;
		this.ReferenceNumberCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(355, 20, true);
		this.ReferenceNumberCodeFindBox.TabIndex = 1;
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
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ES.Business.Declaration.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_ItemNumber)));
		this.ItemNumberCalcEdit.DecimalPlaces = 0;
		this.ItemNumberCalcEdit.Decimals = 0;
		this.ItemNumberCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 216, true);
		this.ItemNumberCalcEdit.Name = "ItemNumberCalcEdit";
		this.ItemNumberCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 20, true);
		this.ItemNumberCalcEdit.TabIndex = 8;
		this.ItemNumberCalcEdit.Text = "0";
		this.ItemNumberCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		// 
		// QuantityAndUnitUserControl
		// 
		this.QuantityAndUnitUserControl.AllowDrop = true;
		this.QuantityAndUnitUserControl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		this.BindingSource.SetBindingMember(this.QuantityAndUnitUserControl, ".");
		this.QuantityAndUnitUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 90, true);
		this.QuantityAndUnitUserControl.Name = "QuantityAndUnitUserControl";
		this.QuantityAndUnitUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(366, 20, true);
		this.QuantityAndUnitUserControl.TabIndex = 3;
		// 
		// SecondQuantityAndUnitUserControl
		// 
		this.SecondQuantityAndUnitUserControl.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.SecondQuantityAndUnitUserControl, ".");
		this.SecondQuantityAndUnitUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 116, true);
		this.SecondQuantityAndUnitUserControl.Name = "SecondQuantityAndUnitUserControl";
		this.SecondQuantityAndUnitUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(369, 20, true);
		this.SecondQuantityAndUnitUserControl.TabIndex = 4;
		// 
		// ValueAndCurrencyUserControl
		// 
		this.ValueAndCurrencyUserControl.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.ValueAndCurrencyUserControl, ".");
		this.ValueAndCurrencyUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 142, true);
		this.ValueAndCurrencyUserControl.Name = "ValueAndCurrencyUserControl";
		this.ValueAndCurrencyUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(395, 20, true);
		this.ValueAndCurrencyUserControl.TabIndex = 5;
		// 
		// DateOfIssueAndExpiryUserControl
		// 
		this.DateOfIssueAndExpiryUserControl.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.DateOfIssueAndExpiryUserControl, ".");
		this.DateOfIssueAndExpiryUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 168, true);
		this.DateOfIssueAndExpiryUserControl.Name = "DateOfIssueAndExpiryUserControl";
		this.DateOfIssueAndExpiryUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(335, 20, true);
		this.DateOfIssueAndExpiryUserControl.TabIndex = 6;
		// 
		// PackQuantityAndUnitUserControl
		// 
		this.PackQuantityAndUnitUserControl.AllowDrop = true;
		this.PackQuantityAndUnitUserControl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		this.BindingSource.SetBindingMember(this.PackQuantityAndUnitUserControl, ".");
		this.PackQuantityAndUnitUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 90, true);
		this.PackQuantityAndUnitUserControl.Name = "PackQuantityAndUnitUserControl";
		this.PackQuantityAndUnitUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(366, 20, true);
		this.PackQuantityAndUnitUserControl.TabIndex = 7;
		// 
		// ExportSupportingDocumentsFieldsControl
		// 
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.CaptionRenderingEnabled = true;
		this.Controls.Add(this.DateOfIssueAndExpiryUserControl);
		this.Controls.Add(this.ValueAndCurrencyUserControl);
		this.Controls.Add(this.SecondQuantityAndUnitUserControl);
		this.Controls.Add(this.QuantityAndUnitUserControl);
		this.Controls.Add(this.CodeCodeFindBox);
		this.Controls.Add(this.ReferenceNumberTextBox);
		this.Controls.Add(this.ReferenceNumberCodeFindBox);
		this.Controls.Add(this.StatusDropEdit);
		this.Controls.Add(this.AdditionalDescriptionTextBox);
		this.Controls.Add(this.ItemNumberCalcEdit);
		this.Controls.Add(this.PackQuantityAndUnitUserControl);
		this.Name = "ExportSupportingDocumentsFieldsControl";
		this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(607, 246, true);
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		this.CodeCodeFindBox.ResumeLayout(true);
		this.CodeCodeFindBox.PerformLayout();
		this.StatusDropEdit.ResumeLayout(true);
		this.StatusDropEdit.PerformLayout();
		this.ReferenceNumberCodeFindBox.ResumeLayout(true);
		this.ReferenceNumberCodeFindBox.PerformLayout();
		this.QuantityAndUnitUserControl.ResumeLayout(true);
		this.QuantityAndUnitUserControl.PerformLayout();
		this.SecondQuantityAndUnitUserControl.ResumeLayout(true);
		this.SecondQuantityAndUnitUserControl.PerformLayout();
		this.ValueAndCurrencyUserControl.ResumeLayout(true);
		this.ValueAndCurrencyUserControl.PerformLayout();
		this.DateOfIssueAndExpiryUserControl.ResumeLayout(true);
		this.DateOfIssueAndExpiryUserControl.PerformLayout();
		this.PackQuantityAndUnitUserControl.ResumeLayout(true);
		this.PackQuantityAndUnitUserControl.PerformLayout();
		this.ResumeLayout(false);
		this.PerformLayout();

	}

	#endregion
	internal ZArchitecture.ZTextBox ReferenceNumberTextBox;
	internal ZArchitecture.GUI.ZCodeFindBox CodeCodeFindBox;
	internal ZArchitecture.GUI.ZDropEdit StatusDropEdit;
	internal ZArchitecture.GUI.ZCodeFindBox ReferenceNumberCodeFindBox;
	internal ZArchitecture.ZTextBox AdditionalDescriptionTextBox;
	internal ZArchitecture.ZCalcEdit ItemNumberCalcEdit;
	internal QuantityAndUnitUserControl QuantityAndUnitUserControl;
	internal SecondQuantityAndUnitUserControl SecondQuantityAndUnitUserControl;
	internal ValueAndCurrencyUserControl ValueAndCurrencyUserControl;
	internal DateOfIssueAndExpiryUserControl DateOfIssueAndExpiryUserControl;
	internal PackQuantityAndUnitUserControl PackQuantityAndUnitUserControl;
}
