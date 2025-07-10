namespace Enterprise.Customs.CH.GUI;

partial class InAndOutwardProcessingUserControl
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
        this.InAndOutwardProcessingSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
        this.DynamicInAndOutwardProcessingPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
        this.NotifyCustomsOfficesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
        this.NotifyCustomsOffice = new Enterprise.ZArchitecture.ZGrid();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.InAndOutwardProcessingSplitContainer)).BeginInit();
        this.InAndOutwardProcessingSplitContainer.Panel1.SuspendLayout();
        this.InAndOutwardProcessingSplitContainer.Panel2.SuspendLayout();
        this.InAndOutwardProcessingSplitContainer.SuspendLayout();
        this.NotifyCustomsOfficesGroupBox.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.NotifyCustomsOffice)).BeginInit();
        this.NotifyCustomsOffice.SuspendLayout();
        this.SuspendLayout();
        // 
        // BindingSource
        // 
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.Business.JobComInvoiceLine);
        // 
        // InAndOutwardProcessingSplitContainer
        // 
        this.InAndOutwardProcessingSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
        this.InAndOutwardProcessingSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
        this.InAndOutwardProcessingSplitContainer.IsSplitterFixed = true;
        this.InAndOutwardProcessingSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
        this.InAndOutwardProcessingSplitContainer.Name = "InAndOutwardProcessingSplitContainer";
        this.InAndOutwardProcessingSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
        // 
        // InAndOutwardProcessingSplitContainer.Panel1
        // 
        this.InAndOutwardProcessingSplitContainer.Panel1.Controls.Add(this.DynamicInAndOutwardProcessingPanel);
        this.InAndOutwardProcessingSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(441, 450, true);
        this.InAndOutwardProcessingSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(200);
        // 
        // InAndOutwardProcessingSplitContainer.Panel2
        // 
        this.InAndOutwardProcessingSplitContainer.Panel2.Controls.Add(this.NotifyCustomsOfficesGroupBox);
        this.InAndOutwardProcessingSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(33);
        this.InAndOutwardProcessingSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(200);
        this.InAndOutwardProcessingSplitContainer.SplitterWidth = 1;
        this.InAndOutwardProcessingSplitContainer.TabIndex = 0;
        // 
        // DynamicInAndOutwardProcessingPanel
        // 
        this.DynamicInAndOutwardProcessingPanel.AllowDrop = true;
        this.DynamicInAndOutwardProcessingPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        this.DynamicInAndOutwardProcessingPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
        this.DynamicInAndOutwardProcessingPanel.Name = "DynamicInAndOutwardProcessingPanel";
        this.DynamicInAndOutwardProcessingPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
        this.DynamicInAndOutwardProcessingPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(441, 200, true);
        this.DynamicInAndOutwardProcessingPanel.TabIndex = 0;
        // 
        // NotifyCustomsOfficesGroupBox
        // 
        this.NotifyCustomsOfficesGroupBox.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("3e2e523f-5ee3-4d77-a7a5-80c134dbb8fc", "Notify Customs Offices");
        this.NotifyCustomsOfficesGroupBox.Controls.Add(this.NotifyCustomsOffice);
        this.NotifyCustomsOfficesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
        this.NotifyCustomsOfficesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
        this.NotifyCustomsOfficesGroupBox.Name = "NotifyCustomsOfficesGroupBox";
        this.NotifyCustomsOfficesGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(6, 3, 6, 6, true);
        this.NotifyCustomsOfficesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(441, 249, true);
        this.NotifyCustomsOfficesGroupBox.TabIndex = 6;
        this.NotifyCustomsOfficesGroupBox.TabStop = false;
        // 
        // NotifyCustomsOffice
        // 
        this.NotifyCustomsOffice.AllowNavigation = false;
        this.BindingSource.SetBindingMember(this.NotifyCustomsOffice, "NotifyCustomsOffices");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobComInvoiceLine)(null)).NotifyCustomsOffices)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.NotifyCustomsOffice)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobComInvoiceLine)(null)).NotifyCustomsOffices)).SyncRoot)).CY_Data)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.NotifyCustomsOffice)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobComInvoiceLine)(null)).NotifyCustomsOffices)).SyncRoot)).DataDescription)));
        this.NotifyCustomsOffice.CaptionVisible = false;
        zCodeFindBoxColumnStyleInfo1.ColumnName = "CY_Data";
        zCodeFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
        zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
        zTextBoxColumnStyleInfo1.ColumnName = "DataDescription";
        zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
        zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
        this.NotifyCustomsOffice.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
        this.NotifyCustomsOffice.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
        this.NotifyCustomsOffice.Dock = System.Windows.Forms.DockStyle.Fill;
        this.NotifyCustomsOffice.GridId = "786c773e-a704-4e9b-b372-152174ce84c0";
        this.NotifyCustomsOffice.HeaderForeColor = System.Drawing.SystemColors.ControlText;
        this.NotifyCustomsOffice.LayoutKey = "zGrid1";
        this.NotifyCustomsOffice.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 15, true);
        this.NotifyCustomsOffice.Name = "NotifyCustomsOffice";
        this.NotifyCustomsOffice.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(429, 228, true);
        this.NotifyCustomsOffice.TabIndex = 0;
        // 
        // InAndOutwardProcessingUserControl
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.CaptionRenderingEnabled = true;
        this.Controls.Add(this.InAndOutwardProcessingSplitContainer);
        this.Name = "InAndOutwardProcessingUserControl";
        this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(441, 450, true);
        this.Controls.SetChildIndex(this.InAndOutwardProcessingSplitContainer, 0);
        this.Controls.SetChildIndex(this.RequiresMergeLabel, 0);
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        this.InAndOutwardProcessingSplitContainer.Panel1.ResumeLayout(false);
        this.InAndOutwardProcessingSplitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.InAndOutwardProcessingSplitContainer)).EndInit();
        this.InAndOutwardProcessingSplitContainer.ResumeLayout(false);
        this.InAndOutwardProcessingSplitContainer.PerformLayout();
        this.NotifyCustomsOfficesGroupBox.ResumeLayout(false);
        this.NotifyCustomsOfficesGroupBox.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.NotifyCustomsOffice)).EndInit();
        this.NotifyCustomsOffice.ResumeLayout(false);
        this.NotifyCustomsOffice.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    internal CargoWise.Windows.UI.KSplitContainer InAndOutwardProcessingSplitContainer;
    internal Enterprise.ZArchitecture.GUI.DynamicLayoutPanel DynamicInAndOutwardProcessingPanel;
    internal ZArchitecture.ZGrid NotifyCustomsOffice;
    internal ZArchitecture.GUI.ZGroupBox NotifyCustomsOfficesGroupBox;
}
