using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

partial class SupportingDocumentsUserControl
{
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
        Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
        this.SupportingDocumentsGrid = new Enterprise.ZArchitecture.ZGrid();
        this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
        this.SupportingDocumentsFieldsControl = new SupportingDocumentsFieldsControl();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.SupportingDocumentsGrid)).BeginInit();
        this.SupportingDocumentsGrid.SuspendLayout();
        this.SuspendLayout();
        // 
        // BindingSource
        // 
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.Business.SupportingDocument);
        // 
        // SupportingDocumentsGrid
        // 
        this.SupportingDocumentsGrid.AllowNavigation = false;
        this.BindingSource.SetBindingMember(this.SupportingDocumentsGrid, ".");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_Code)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_ReferenceNumber)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_ReferenceNumber2)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.CH.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_DateOfIssue)));
        this.SupportingDocumentsGrid.CaptionVisible = false;
        zCodeFindBoxColumnStyleInfo1.ColumnName = "CSI_Code";
        zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
        zTextBoxColumnStyleInfo1.ColumnName = "CSI_ReferenceNumber";
        zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
        zTextBoxColumnStyleInfo2.ColumnName = "CSI_ReferenceNumber2";
        zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
        zDateEditColumnStyleInfo1.ColumnName = "CSI_DateOfIssue";
        zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
        zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
        this.SupportingDocumentsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
        this.SupportingDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
        this.SupportingDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
        this.SupportingDocumentsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
        this.SupportingDocumentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
        this.SupportingDocumentsGrid.GridId = "3b710cba-62e3-49e2-a94f-8370b1f1a0ee";
        this.SupportingDocumentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
        this.SupportingDocumentsGrid.LayoutKey = "SupportingDocumentsGrid";
        this.SupportingDocumentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
        this.SupportingDocumentsGrid.Name = "SupportingDocumentsGrid";
        this.SupportingDocumentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(599, 155, true);
        this.SupportingDocumentsGrid.TabIndex = 0;
        // 
        // SupportingDocumentsFieldsControl
        //
        this.SupportingDocumentsFieldsControl.CaptionRenderingEnabled = true;
        this.SupportingDocumentsFieldsControl.Dock = System.Windows.Forms.DockStyle.Fill;
        // 
        // BottomPanel
        // 
        this.BottomPanel.Controls.Add(SupportingDocumentsFieldsControl);
        this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 158, true);
        this.BottomPanel.Name = "BottomPanel";
        this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(599, 120, true);
        this.BottomPanel.TabIndex = 2;
        // 
        // SupportingDocumentsUserControl
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.CaptionRenderingEnabled = true;
        this.Controls.Add(this.SupportingDocumentsGrid);
        this.Controls.Add(this.BottomPanel);
        this.Name = "SupportingDocumentsUserControl";
        this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(599, 379, true);
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.SupportingDocumentsGrid)).EndInit();
        this.SupportingDocumentsGrid.ResumeLayout(false);
        this.SupportingDocumentsGrid.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    protected Enterprise.ZArchitecture.GUI.ZPanel BottomPanel;
    internal Enterprise.ZArchitecture.ZGrid SupportingDocumentsGrid;
    internal ZUserControl SupportingDocumentsFieldsControl;
}
