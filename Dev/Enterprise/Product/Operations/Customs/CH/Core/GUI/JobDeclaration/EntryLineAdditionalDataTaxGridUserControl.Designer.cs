
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

partial class EntryLineAdditionalDataTaxGridUserControl : ZUserControl
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
        Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
        Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
        Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
        Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
        Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
        Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
        this.EntryLineDutyAndTaxGrid = new Enterprise.ZArchitecture.ZGrid();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.EntryLineDutyAndTaxGrid)).BeginInit();
        this.EntryLineDutyAndTaxGrid.SuspendLayout();
        this.SuspendLayout();
        // 
        // BindingSource
        // 
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.Business.CusEntryLineFee);
        // 
        // EntryLineDutyAndTaxGrid
        // 
        this.EntryLineDutyAndTaxGrid.AllowNavigation = false;
        this.BindingSource.SetBindingMember(this.EntryLineDutyAndTaxGrid, ".");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CH.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).Fees)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).Fees)).SyncRoot)).CF_ChargeType)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).Fees)).SyncRoot)).ChargeTypeDescription)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CH.Business.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).Fees)).SyncRoot)).CF_ChargeAmount)));
        this.EntryLineDutyAndTaxGrid.CaptionVisible = false;
        zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("4fa5db5a-729f-4d25-aa04-d20fdbee4955", "Type");
        zDropEditColumnStyleInfo1.ColumnName = "CF_ChargeType";
        zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
        zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("fc6165f7-3fc0-40b9-ad87-4adb591bce72", "Description");
        zTextBoxColumnStyleInfo1.ColumnName = "ChargeTypeDescription";
        zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
        zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("65379891-6BC1-4373-A06D-8560FC0987B6", "Action");
        zTextBoxColumnStyleInfo2.ColumnName = "CF_RateOverrideReasonCode";
        zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
        zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
        zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("5193C4FF-93AB-4145-A347-3AB4E854F117", "Base Value");
        zCalcEditColumnStyleInfo1.ColumnName = "CF_BaseValue";
        zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
        zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
        zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("B2419735-6459-4E15-953C-75B0F8C82E5D", "Tax Rate");
        zCalcEditColumnStyleInfo2.ColumnName = "CF_Rate";
        zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
        zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
        zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("f81b53c1-8a1c-4d60-bb2b-384262a80413", "Total Amount");
        zCalcEditColumnStyleInfo3.ColumnName = "CF_ChargeAmount";
        zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
        this.EntryLineDutyAndTaxGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
        this.EntryLineDutyAndTaxGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
        this.EntryLineDutyAndTaxGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
        this.EntryLineDutyAndTaxGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
        this.EntryLineDutyAndTaxGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
        this.EntryLineDutyAndTaxGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
        this.EntryLineDutyAndTaxGrid.Dock = System.Windows.Forms.DockStyle.Fill;
        this.EntryLineDutyAndTaxGrid.GridId = "d3959932-2a55-4c6b-8b0f-4b88aacf2aa8";
        this.EntryLineDutyAndTaxGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
        this.EntryLineDutyAndTaxGrid.ImeMode = System.Windows.Forms.ImeMode.Disable;
        this.EntryLineDutyAndTaxGrid.LayoutKey = "EntryLineDutyAndTaxGrid";
        this.EntryLineDutyAndTaxGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
        this.EntryLineDutyAndTaxGrid.Name = "EntryLineDutyAndTaxGrid";
        this.EntryLineDutyAndTaxGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(608, 135, true);
        this.EntryLineDutyAndTaxGrid.TabIndex = 0;
        // 
        // EntryLineAdditionalDataTaxGridUserControl
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.CaptionRenderingEnabled = true;
        this.Controls.Add(this.EntryLineDutyAndTaxGrid);
        this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
        this.Name = "EntryLineAdditionalDataTaxGridUserControl";
        this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(622, 171, true);
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.EntryLineDutyAndTaxGrid)).EndInit();
        this.EntryLineDutyAndTaxGrid.ResumeLayout(false);
        this.EntryLineDutyAndTaxGrid.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    internal ZArchitecture.ZGrid EntryLineDutyAndTaxGrid;
}
