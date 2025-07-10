
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

partial class EntryLineAdditionalDataUserControl : ZUserControl
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
        this.components = new System.ComponentModel.Container();
        this.TaxOrFeeTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
        this.DutyAndTaxGridSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
        this.CalculatedDutyAndTaxGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
        this.CalculatedDutyAndTaxGrid = new Enterprise.Customs.CH.GUI.EntryLineAdditionalDataTaxGridUserControl();
        this.ConfirmedDutyAndTaxGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
        this.ConfirmedDutyAndTaxGrid = new Enterprise.Customs.CH.GUI.EntryLineAdditionalDataTaxGridUserControl();
        this.TabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
        this.ExtendedInformationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
        this.ExtendedInformationsUserControl = new Enterprise.Customs.CH.GUI.CusEntryLineExtendedInformationsUserControl();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        this.TaxOrFeeTabPage.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.DutyAndTaxGridSplitContainer)).BeginInit();
        this.DutyAndTaxGridSplitContainer.Panel1.SuspendLayout();
        this.DutyAndTaxGridSplitContainer.Panel2.SuspendLayout();
        this.DutyAndTaxGridSplitContainer.SuspendLayout();
        this.CalculatedDutyAndTaxGroupBox.SuspendLayout();
        this.CalculatedDutyAndTaxGrid.SuspendLayout();
        this.ConfirmedDutyAndTaxGroupBox.SuspendLayout();
        this.ConfirmedDutyAndTaxGrid.SuspendLayout();
        this.TabControl.SuspendLayout();
        this.ExtendedInformationTabPage.SuspendLayout();
        this.ExtendedInformationsUserControl.SuspendLayout();
        this.SuspendLayout();
        // 
        // BindingSource
        // 
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.Business.JobDeclaration);
        // 
        // TaxOrFeeTabPage
        // 
        this.TaxOrFeeTabPage.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("825164a1-f603-4da6-8e3e-ba0cfc9fd6cb", "Tax Or Fee");
        this.TaxOrFeeTabPage.Controls.Add(this.DutyAndTaxGridSplitContainer);
        this.TaxOrFeeTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
        this.TaxOrFeeTabPage.Name = "TaxOrFeeTabPage";
        this.TaxOrFeeTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
        this.TaxOrFeeTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(614, 141, true);
        this.TaxOrFeeTabPage.TabIndex = 0;
        this.TaxOrFeeTabPage.UseVisualStyleBackColor = true;
        // 
        // DutyAndTaxGridSplitContainer
        // 
        this.DutyAndTaxGridSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
        this.DutyAndTaxGridSplitContainer.Location = new System.Drawing.Point(3, 3);
        this.DutyAndTaxGridSplitContainer.Name = "DutyAndTaxGridSplitContainer";
        this.DutyAndTaxGridSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
        // 
        // DutyAndTaxGridSplitContainer.Panel1
        // 
        this.DutyAndTaxGridSplitContainer.Panel1.Controls.Add(this.CalculatedDutyAndTaxGroupBox);
        // 
        // DutyAndTaxGridSplitContainer.Panel2
        // 
        this.DutyAndTaxGridSplitContainer.Panel2.Controls.Add(this.ConfirmedDutyAndTaxGroupBox);
        this.DutyAndTaxGridSplitContainer.Size = new System.Drawing.Size(608, 135);
        this.DutyAndTaxGridSplitContainer.SplitterDistance = 67;
        this.DutyAndTaxGridSplitContainer.TabIndex = 0;
        // 
        // CalculatedDutyAndTaxGroupBox
        // 
        this.CalculatedDutyAndTaxGroupBox.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("B889048B-C993-42C5-A4CF-71D095C29E50", "Calculated Duty And Tax");
        this.CalculatedDutyAndTaxGroupBox.Controls.Add(this.CalculatedDutyAndTaxGrid);
        this.CalculatedDutyAndTaxGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
        this.CalculatedDutyAndTaxGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
        this.CalculatedDutyAndTaxGroupBox.Name = "CalculatedDutyAndTaxGroupBox";
        this.CalculatedDutyAndTaxGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(608, 67, true);
        this.CalculatedDutyAndTaxGroupBox.TabIndex = 1;
        this.CalculatedDutyAndTaxGroupBox.TabStop = false;
        // 
        // CalculatedDutyAndTaxGrid
        // 
        this.CalculatedDutyAndTaxGrid.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.CalculatedDutyAndTaxGrid, "CustomsEntryHeaders.AllEntryLines.Fees");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.CH.Business.CusEntryLineFee)(((Enterprise.Customs.CH.Business.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).Fees)).SyncRoot)))));
        this.CalculatedDutyAndTaxGrid.Dock = System.Windows.Forms.DockStyle.Fill;
        this.CalculatedDutyAndTaxGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
        this.CalculatedDutyAndTaxGrid.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
        this.CalculatedDutyAndTaxGrid.Name = "CalculatedDutyAndTaxGrid";
        this.CalculatedDutyAndTaxGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(602, 48, true);
        this.CalculatedDutyAndTaxGrid.TabIndex = 0;
        // 
        // ConfirmedDutyAndTaxGroupBox
        // 
        this.ConfirmedDutyAndTaxGroupBox.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("08581BA5-564D-452C-B387-108CF540F8E9", "Confirmed Duty And Tax");
        this.ConfirmedDutyAndTaxGroupBox.Controls.Add(this.ConfirmedDutyAndTaxGrid);
        this.ConfirmedDutyAndTaxGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
        this.ConfirmedDutyAndTaxGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
        this.ConfirmedDutyAndTaxGroupBox.Name = "ConfirmedDutyAndTaxGroupBox";
        this.ConfirmedDutyAndTaxGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(608, 64, true);
        this.ConfirmedDutyAndTaxGroupBox.TabIndex = 2;
        this.ConfirmedDutyAndTaxGroupBox.TabStop = false;
        // 
        // ConfirmedDutyAndTaxGrid
        // 
        this.ConfirmedDutyAndTaxGrid.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.ConfirmedDutyAndTaxGrid, "CustomsEntryHeaders.AllEntryLines.ConfirmedFees");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.CH.Business.CusEntryLineFee)(((Enterprise.Customs.CH.Business.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).ConfirmedFees)).SyncRoot)))));
        this.ConfirmedDutyAndTaxGrid.Dock = System.Windows.Forms.DockStyle.Fill;
        this.ConfirmedDutyAndTaxGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
        this.ConfirmedDutyAndTaxGrid.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
        this.ConfirmedDutyAndTaxGrid.Name = "ConfirmedDutyAndTaxGrid";
        this.ConfirmedDutyAndTaxGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(602, 45, true);
        this.ConfirmedDutyAndTaxGrid.TabIndex = 0;
        // 
        // TabControl
        // 
        this.TabControl.Controls.Add(this.ExtendedInformationTabPage);
        this.TabControl.Controls.Add(this.TaxOrFeeTabPage);
        this.TabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
        this.TabControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
        this.TabControl.Name = "TabControl";
        this.TabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(622, 168, true);
        this.TabControl.TabIndex = 0;
        // 
        // ExtendedInformationTabPage
        // 
        this.ExtendedInformationTabPage.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("d1bf9e2e-636e-4078-8a3d-8cc9d86f77ff", "Extended Information");
        this.ExtendedInformationTabPage.Controls.Add(this.ExtendedInformationsUserControl);
        this.ExtendedInformationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
        this.ExtendedInformationTabPage.Name = "ExtendedInformationTabPage";
        this.ExtendedInformationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(614, 141, true);
        this.ExtendedInformationTabPage.TabIndex = 1;
        // 
        // ExtendedInformationsUserControl
        // 
        this.ExtendedInformationsUserControl.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.ExtendedInformationsUserControl, "CustomsEntryHeaders.AllEntryLines");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.CH.Business.CusEntryLine)(((Enterprise.Customs.CH.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)))));
        this.ExtendedInformationsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
        this.ExtendedInformationsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
        this.ExtendedInformationsUserControl.Name = "ExtendedInformationsUserControl";
        this.ExtendedInformationsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(614, 141, true);
        this.ExtendedInformationsUserControl.TabIndex = 0;
        // 
        // EntryLineAdditionalDataUserControl
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.CaptionRenderingEnabled = true;
        this.Controls.Add(this.TabControl);
        this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
        this.Name = "EntryLineAdditionalDataUserControl";
        this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(622, 171, true);
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        this.TaxOrFeeTabPage.ResumeLayout(false);
        this.TaxOrFeeTabPage.PerformLayout();
        this.DutyAndTaxGridSplitContainer.Panel1.ResumeLayout(false);
        this.DutyAndTaxGridSplitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.DutyAndTaxGridSplitContainer)).EndInit();
        this.DutyAndTaxGridSplitContainer.ResumeLayout(false);
        this.CalculatedDutyAndTaxGroupBox.ResumeLayout(false);
        this.CalculatedDutyAndTaxGroupBox.PerformLayout();
        this.CalculatedDutyAndTaxGrid.ResumeLayout(true);
        this.CalculatedDutyAndTaxGrid.PerformLayout();
        this.ConfirmedDutyAndTaxGroupBox.ResumeLayout(false);
        this.ConfirmedDutyAndTaxGroupBox.PerformLayout();
        this.ConfirmedDutyAndTaxGrid.ResumeLayout(true);
        this.ConfirmedDutyAndTaxGrid.PerformLayout();
        this.TabControl.ResumeLayout(false);
        this.TabControl.PerformLayout();
        this.ExtendedInformationTabPage.ResumeLayout(false);
        this.ExtendedInformationTabPage.PerformLayout();
        this.ExtendedInformationsUserControl.ResumeLayout(true);
        this.ExtendedInformationsUserControl.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    internal ZTabControl TabControl;
    internal ZTabPage ExtendedInformationTabPage;
    internal ZTabPage TaxOrFeeTabPage;
    internal CusEntryLineExtendedInformationsUserControl ExtendedInformationsUserControl;
    internal CargoWise.Windows.UI.KSplitContainer DutyAndTaxGridSplitContainer;
    internal ZGroupBox ConfirmedDutyAndTaxGroupBox;
    internal ZGroupBox CalculatedDutyAndTaxGroupBox;
    internal EntryLineAdditionalDataTaxGridUserControl CalculatedDutyAndTaxGrid;
    internal EntryLineAdditionalDataTaxGridUserControl ConfirmedDutyAndTaxGrid;
}
