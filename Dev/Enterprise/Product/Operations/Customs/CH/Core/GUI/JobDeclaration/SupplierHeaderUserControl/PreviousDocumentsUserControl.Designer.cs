
namespace Enterprise.Customs.CH.GUI.PlugIn;

partial class PreviousDocumentsUserControl
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
        Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
        Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
        Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
        this.PrevDocsGrid = new Enterprise.ZArchitecture.ZGrid();
        this.PrevDocsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
        this.PrevDocsTypeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
        this.PrevDocsAdditionalInformationTextBox = new Enterprise.ZArchitecture.ZTextBox();
        this.PrevDocsReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.PrevDocsGrid)).BeginInit();
        this.PrevDocsGrid.SuspendLayout();
        this.PrevDocsGroupBox.SuspendLayout();
        this.PrevDocsTypeFindBox.SuspendLayout();
        this.SuspendLayout();
        // 
        // BindingSource
        // 
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.Business.PreviousDocument);
        // 
        // PrevDocsGrid
        // 
        this.PrevDocsGrid.AllowNavigation = false;
        this.PrevDocsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.BindingSource.SetBindingMember(this.PrevDocsGrid, ".");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobComInvoiceHeader)(null)).PreviousDocuments)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobComInvoiceHeader)(null)).PreviousDocuments)).SyncRoot)).CSI_Code)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobComInvoiceHeader)(null)).PreviousDocuments)).SyncRoot)).CSI_ReferenceNumber)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobComInvoiceHeader)(null)).PreviousDocuments)).SyncRoot)).CSI_Description)));
        this.PrevDocsGrid.CaptionVisible = false;
        zCodeFindBoxColumnStyleInfo1.ColumnName = "CSI_Code";
        zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
        zTextBoxColumnStyleInfo1.ColumnName = "CSI_ReferenceNumber";
        zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
        zTextBoxColumnStyleInfo2.ColumnName = "CSI_Description";
        zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
        this.PrevDocsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
        this.PrevDocsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
        this.PrevDocsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
        this.PrevDocsGrid.GridId = "2a2146b7-fd7a-499c-b3f4-1d803d9a60f2";
        this.PrevDocsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
        this.PrevDocsGrid.LayoutKey = "PrevDocsGrid";
        this.PrevDocsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
        this.PrevDocsGrid.Name = "PrevDocsGrid";
        this.PrevDocsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(454, 165, true);
        this.PrevDocsGrid.TabIndex = 0;
        // 
        // PrevDocsGroupBox
        // 
        this.PrevDocsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.PrevDocsGroupBox.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("5319f254-6e9f-42e1-9ce1-c8baeda297e6", "[40] Previous Docs");
        this.PrevDocsGroupBox.Controls.Add(this.PrevDocsTypeFindBox);
        this.PrevDocsGroupBox.Controls.Add(this.PrevDocsAdditionalInformationTextBox);
        this.PrevDocsGroupBox.Controls.Add(this.PrevDocsReferenceTextBox);
        this.PrevDocsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 171, true);
        this.PrevDocsGroupBox.Name = "PrevDocsGroupBox";
        this.PrevDocsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(454, 108, true);
        this.PrevDocsGroupBox.TabIndex = 1;
        this.PrevDocsGroupBox.TabStop = false;
        // 
        // PrevDocsTypeFindBox
        // 
        this.PrevDocsTypeFindBox.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.PrevDocsTypeFindBox, "CSI_Code");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobComInvoiceHeader)(null)).PreviousDocuments)).SyncRoot)).CSI_Code)));
        this.PrevDocsTypeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 19, true);
        this.PrevDocsTypeFindBox.Name = "PrevDocsTypeFindBox";
        this.PrevDocsTypeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
        this.PrevDocsTypeFindBox.ParentType = null;
        this.PrevDocsTypeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
        this.PrevDocsTypeFindBox.TabIndex = 0;
        // 
        // PrevDocsAdditionalInformationTextBox
        // 
        this.BindingSource.SetBindingMember(this.PrevDocsAdditionalInformationTextBox, "CSI_Description");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobComInvoiceHeader)(null)).PreviousDocuments)).SyncRoot)).CSI_Description)));
        this.PrevDocsAdditionalInformationTextBox.CaptionResourceString = null;
        this.PrevDocsAdditionalInformationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 71, true);
        this.PrevDocsAdditionalInformationTextBox.Name = "PrevDocsAdditionalInformationTextBox";
        this.PrevDocsAdditionalInformationTextBox.ShouldEscapeAllSpecialCharacters = false;
        this.PrevDocsAdditionalInformationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
        this.PrevDocsAdditionalInformationTextBox.TabIndex = 2;
        // 
        // PrevDocsReferenceTextBox
        // 
        this.BindingSource.SetBindingMember(this.PrevDocsReferenceTextBox, "CSI_ReferenceNumber");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobComInvoiceHeader)(null)).PreviousDocuments)).SyncRoot)).CSI_ReferenceNumber)));
        this.PrevDocsReferenceTextBox.CaptionResourceString = null;
        this.PrevDocsReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 45, true);
        this.PrevDocsReferenceTextBox.Name = "PrevDocsReferenceTextBox";
        this.PrevDocsReferenceTextBox.ShouldEscapeAllSpecialCharacters = false;
        this.PrevDocsReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
        this.PrevDocsReferenceTextBox.TabIndex = 1;
        // 
        // PreviousDocumentsUserControl
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.CaptionRenderingEnabled = true;
        this.Controls.Add(this.PrevDocsGroupBox);
        this.Controls.Add(this.PrevDocsGrid);
        this.Name = "PreviousDocumentsUserControl";
        this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(454, 279, true);
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.PrevDocsGrid)).EndInit();
        this.PrevDocsGrid.ResumeLayout(false);
        this.PrevDocsGrid.PerformLayout();
        this.PrevDocsGroupBox.ResumeLayout(false);
        this.PrevDocsGroupBox.PerformLayout();
        this.PrevDocsTypeFindBox.ResumeLayout(true);
        this.PrevDocsTypeFindBox.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    internal ZArchitecture.ZGrid PrevDocsGrid;
    internal ZArchitecture.GUI.ZGroupBox PrevDocsGroupBox;
    internal ZArchitecture.ZTextBox PrevDocsAdditionalInformationTextBox;
    internal ZArchitecture.ZTextBox PrevDocsReferenceTextBox;
    internal ZArchitecture.GUI.ZCodeFindBox PrevDocsTypeFindBox;
}
