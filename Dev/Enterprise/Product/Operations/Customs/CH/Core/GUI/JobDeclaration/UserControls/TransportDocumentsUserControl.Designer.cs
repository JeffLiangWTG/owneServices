using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

partial class TransportDocumentsUserControl
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
        this.TransportDocumentsGrid = new Enterprise.ZArchitecture.ZGrid();
        this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
        this.TransportDocumentsDetailsUserControl = new TransportDocumentsDetailsUserControl();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.TransportDocumentsGrid)).BeginInit();
        this.TransportDocumentsGrid.SuspendLayout();
        this.SuspendLayout();
        // 
        // BindingSource
        // 
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.Business.TransportDocument);
        // 
        // TransportDocumentsGrid
        // 
        this.TransportDocumentsGrid.AllowNavigation = false;
        this.BindingSource.SetBindingMember(this.TransportDocumentsGrid, ".");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).TransportDocuments)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.TransportDocument)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).TransportDocuments)).SyncRoot)).CSI_Code)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.TransportDocument)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).TransportDocuments)).SyncRoot)).CSI_ReferenceNumber)));
        this.TransportDocumentsGrid.CaptionVisible = false;
        zCodeFindBoxColumnStyleInfo1.ColumnName = "CSI_Code";
        zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
        zTextBoxColumnStyleInfo1.ColumnName = "CSI_ReferenceNumber";
        zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
        this.TransportDocumentsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
        this.TransportDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
        this.TransportDocumentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
        this.TransportDocumentsGrid.GridId = "3b710cba-62e3-49e2-a94f-8370b1f1a0ee";
        this.TransportDocumentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
        this.TransportDocumentsGrid.LayoutKey = "TransportDocumentsGrid";
        this.TransportDocumentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
        this.TransportDocumentsGrid.Name = "TransportDocumentsGrid";
        this.TransportDocumentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(599, 155, true);
        this.TransportDocumentsGrid.TabIndex = 0;
        // 
        // TransportDocumentsFieldsControl
        //
        this.TransportDocumentsDetailsUserControl.CaptionRenderingEnabled = true;
        this.TransportDocumentsDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
        // 
        // BottomPanel
        // 
        this.BottomPanel.Controls.Add(TransportDocumentsDetailsUserControl);
        this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 158, true);
        this.BottomPanel.Name = "BottomPanel";
        this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(599, 120, true);
        this.BottomPanel.TabIndex = 2;
        // 
        // TransportDocumentsUserControl
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.CaptionRenderingEnabled = true;
        this.Controls.Add(this.TransportDocumentsGrid);
        this.Controls.Add(this.BottomPanel);
        this.Name = "TransportDocumentsUserControl";
        this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(599, 379, true);
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.TransportDocumentsGrid)).EndInit();
        this.TransportDocumentsGrid.ResumeLayout(false);
        this.TransportDocumentsGrid.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    protected Enterprise.ZArchitecture.GUI.ZPanel BottomPanel;
    internal Enterprise.ZArchitecture.ZGrid TransportDocumentsGrid;
    internal ZUserControl TransportDocumentsDetailsUserControl;
}
