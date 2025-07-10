namespace Enterprise.Customs.CH.GUI;

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
        this.SupportingDocumentsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
        this.CSI_CodeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
        this.CSI_ReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
        this.CSI_ReferenceNumber2TextBox = new Enterprise.ZArchitecture.ZTextBox();
        this.CSI_DateOfIssueDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        this.SupportingDocumentsGroupBox.SuspendLayout();
        this.CSI_CodeCodeFindBox.SuspendLayout();
        this.CSI_DateOfIssueDateEdit.SuspendLayout();
        this.SuspendLayout();
        // 
        // BindingSource
        // 
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.Business.SupportingDocument);
        // 
        // SupportingDocumentsGroupBox
        // 
        this.SupportingDocumentsGroupBox.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("1F9EA381-EDC9-4F96-B0B3-B3FFF9CF8527", "[44] Supporting Documents");
        this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_CodeCodeFindBox);
        this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_ReferenceNumberTextBox);
        this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_ReferenceNumber2TextBox);
        this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_DateOfIssueDateEdit);
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
        this.BindingSource.SetBindingMember(this.CSI_CodeCodeFindBox, "CSI_Code");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_Code)));
        this.CSI_CodeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 15, true);
        this.CSI_CodeCodeFindBox.Name = "CSI_CodeCodeFindBox";
        this.CSI_CodeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
        this.CSI_CodeCodeFindBox.ParentType = null;
        this.CSI_CodeCodeFindBox.PreBoundMaxLength = 4;
        this.CSI_CodeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(675, 20, true);
        this.CSI_CodeCodeFindBox.TabIndex = 0;
        // 
        // CSI_ReferenceNumberTextBox
        // 
        this.CSI_ReferenceNumberTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.CSI_ReferenceNumberTextBox.BackColor = System.Drawing.SystemColors.Window;
        this.BindingSource.SetBindingMember(this.CSI_ReferenceNumberTextBox, "CSI_ReferenceNumber");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_ReferenceNumber)));
        this.CSI_ReferenceNumberTextBox.CaptionResourceString = null;
        this.CSI_ReferenceNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
        this.CSI_ReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 40, true);
        this.CSI_ReferenceNumberTextBox.Name = "CSI_ReferenceNumberTextBox";
        this.CSI_ReferenceNumberTextBox.ShouldEscapeAllSpecialCharacters = false;
        this.CSI_ReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(675, 20, true);
        this.CSI_ReferenceNumberTextBox.TabIndex = 1;
        // 
        // CSI_ReferenceNumber2TextBox
        // 
        this.CSI_ReferenceNumber2TextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.CSI_ReferenceNumber2TextBox.BackColor = System.Drawing.SystemColors.Window;
        this.BindingSource.SetBindingMember(this.CSI_ReferenceNumber2TextBox, "CSI_ReferenceNumber2");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_ReferenceNumber2)));
        this.CSI_ReferenceNumber2TextBox.CaptionResourceString = null;
        this.CSI_ReferenceNumber2TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
        this.CSI_ReferenceNumber2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 65, true);
        this.CSI_ReferenceNumber2TextBox.Name = "CSI_ReferenceNumber2TextBox";
        this.CSI_ReferenceNumber2TextBox.ShouldEscapeAllSpecialCharacters = false;
        this.CSI_ReferenceNumber2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(675, 20, true);
        this.CSI_ReferenceNumber2TextBox.TabIndex = 2;
        // 
        // CSI_DateOfIssueDateEdit
        // 
        this.CSI_DateOfIssueDateEdit.AllowDrop = true;
        this.CSI_DateOfIssueDateEdit.AutoCompleteMonthThreshold = 1;
        this.CSI_DateOfIssueDateEdit.AutoCompleteYear = true;
        this.CSI_DateOfIssueDateEdit.BackColor = System.Drawing.SystemColors.Window;
        this.BindingSource.SetBindingMember(this.CSI_DateOfIssueDateEdit, "CSI_DateOfIssue");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CH.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_DateOfIssue)));
        this.CSI_DateOfIssueDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 90, true);
        this.CSI_DateOfIssueDateEdit.Name = "CSI_DateOfIssueDateEdit";
        this.CSI_DateOfIssueDateEdit.TabIndex = 3;
        // 
        // SupportingDocumentsFieldsControl
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.CaptionRenderingEnabled = true;
        this.Controls.Add(this.SupportingDocumentsGroupBox);
        this.Name = "SupportingDocumentsFieldsControl";
        this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(836, 200, true);
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        this.SupportingDocumentsGroupBox.ResumeLayout(false);
        this.SupportingDocumentsGroupBox.PerformLayout();
        this.CSI_CodeCodeFindBox.ResumeLayout(true);
        this.CSI_CodeCodeFindBox.PerformLayout();
        this.CSI_DateOfIssueDateEdit.ResumeLayout(true);
        this.CSI_DateOfIssueDateEdit.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    internal ZArchitecture.GUI.ZGroupBox SupportingDocumentsGroupBox;
    internal ZArchitecture.GUI.ZCodeFindBox CSI_CodeCodeFindBox;
    internal ZArchitecture.ZTextBox CSI_ReferenceNumberTextBox;
    internal ZArchitecture.ZTextBox CSI_ReferenceNumber2TextBox;
    internal ZArchitecture.GUI.ZDateEdit CSI_DateOfIssueDateEdit;
}
