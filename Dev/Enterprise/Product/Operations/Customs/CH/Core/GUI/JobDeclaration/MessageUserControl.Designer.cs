
using System.Windows.Forms;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

partial class MessageUserControl
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
        Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
        Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
        Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
        Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
        Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
        Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
        Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
        Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
        Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
        Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
        Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
        Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
        Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
        Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
        Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
        Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
        Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
        Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
        Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
        Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
        Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
        Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
        Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo9 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
        Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo10 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
        Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo11 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
        Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
        this.entryLineAdditionalDataUserControl = new Enterprise.Customs.CH.GUI.EntryLineAdditionalDataUserControl();
        this.EComMessageTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
        this.EComMessageUserControl = new Enterprise.Customs.CH.GUI.EComMessagesTabUserControl();
        this.EntryHeaderChargesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
        this.ConfirmedDutyAndTaxGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
        this.EntryHeaderChargesGrid = new Enterprise.ZArchitecture.ZGrid();
        this.EntryLinesSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
        this.EntriesGroupBox.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.EntriesBoundGrid)).BeginInit();
        this.EntriesBoundGrid.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.MainHorizontalSplitContainer)).BeginInit();
        this.MainHorizontalSplitContainer.Panel1.SuspendLayout();
        this.MainHorizontalSplitContainer.Panel2.SuspendLayout();
        this.MainHorizontalSplitContainer.SuspendLayout();
        this.EntryLinesMessagesTabControl.SuspendLayout();
        this.EntryLinesTabPage.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.EntryLineGrid)).BeginInit();
        this.EntryLineGrid.SuspendLayout();
        this.EntryLinesSplitContainer.SuspendLayout();
        this.MessageTabPage.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.TopVerticalSplitContainer)).BeginInit();
        this.TopVerticalSplitContainer.Panel1.SuspendLayout();
        this.TopVerticalSplitContainer.SuspendLayout();
        this.BaseMessageUserControl.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        this.entryLineAdditionalDataUserControl.SuspendLayout();
        this.EComMessageTabPage.SuspendLayout();
        this.EComMessageUserControl.SuspendLayout();
        this.EntryHeaderChargesTabPage.SuspendLayout();
        this.ConfirmedDutyAndTaxGroupBox.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.EntryHeaderChargesGrid)).BeginInit();
        this.EntryHeaderChargesGrid.SuspendLayout();
        this.SuspendLayout();
        // 
        // EntriesBoundGrid
        // 
        zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("D79420C2-B3FF-4DE8-91FE-32C58A5B1B56", "MRN", "Movement Reference Number");
        zTextBoxColumnStyleInfo1.ColumnName = "MovementReferenceNumber";
        zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
        zTextBoxColumnStyleInfo2.ColumnName = "CH_EntryStatus";
        zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
        zTextBoxColumnStyleInfo3.ColumnName = "EntryHeaderStatusDescription";
        zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
        zTextBoxColumnStyleInfo4.ColumnName = "CH_Status";
        zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
        zTextBoxColumnStyleInfo5.ColumnName = "MessageStatusDescription";
        zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
        zTextBoxColumnStyleInfo6.ColumnName = "CH_PhaseStatus";
        zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
        zTextBoxColumnStyleInfo7.ColumnName = "PhaseStatusDescription";
        zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
        zDateEditColumnStyleInfo1.ColumnName = "CH_EntrySubmittedDate";
        zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
        zDateEditColumnStyleInfo2.ColumnName = "CH_EntryReleaseDate";
        zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
        zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
        zCalcEditColumnStyleInfo1.ColumnName = "TotalDutyAmount";
        zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
        zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
        zCalcEditColumnStyleInfo2.ColumnName = "GSTAmount";
        zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
        zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
        zCalcEditColumnStyleInfo3.ColumnName = "ConfirmedDuty";
        zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
        zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
        zCalcEditColumnStyleInfo4.ColumnName = "ConfirmedVAT";
        zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(88);
        zTextBoxColumnStyleInfo8.ColumnName = "EComMessageStatusDescription";
        zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
        this.EntriesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
        this.EntriesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
        this.EntriesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
        this.EntriesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
        this.EntriesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
        this.EntriesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
        this.EntriesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
        this.EntriesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
        this.EntriesBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
        this.EntriesBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
        this.EntriesBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
        this.EntriesBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
        this.EntriesBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
        this.EntriesBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
        // 
        // MainHorizontalSplitContainer
        // 
        // 
        // EntryLinesMessagesTabControl
        // 
        this.EntryLinesMessagesTabControl.Controls.Add(this.EComMessageTabPage);
        this.EntryLinesMessagesTabControl.Controls.Add(this.EntryHeaderChargesTabPage);
        this.EntryLinesMessagesTabControl.Controls.SetChildIndex(this.MessageTabPage, 0);
        this.EntryLinesMessagesTabControl.Controls.SetChildIndex(this.EComMessageTabPage, 0);
        this.EntryLinesMessagesTabControl.Controls.SetChildIndex(this.EntryHeaderChargesTabPage, 0);
        this.EntryLinesMessagesTabControl.Controls.SetChildIndex(this.EntryLinesTabPage, 0);
        //
        // EntryLinesSplitContainer
        //
        this.EntryLinesSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
        this.EntryLinesSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 4, true);
        this.EntryLinesSplitContainer.Name = "EntryLinesSplitContainer";
        this.EntryLinesSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
        // 
        // EntryLinesSpllitContainer.Panel1
        // 
        this.EntryLinesSplitContainer.Panel1.Controls.Add(this.EntryLineGrid);
        // 
        // EntryLinesSpllitContainer.Panel2
        // 
        this.EntryLinesSplitContainer.Panel2.Controls.Add(this.entryLineAdditionalDataUserControl);
        // 
        // EntryLinesTabPage
        // 
        this.EntryLinesTabPage.Controls.Remove(this.EntryLineGrid);
        this.EntryLinesTabPage.Controls.Remove(this.ExtendedInfoGroupBox);
        this.EntryLinesTabPage.Controls.Add(this.EntryLinesSplitContainer);
        // 
        // EntryLineGrid
        // 
        zTextBoxColumnStyleInfo10.ColumnName = "LineSubmissionStatusDescription";
        zTextBoxColumnStyleInfo10.IsReadOnly = true;
        zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
        zTextBoxColumnStyleInfo11.ColumnName = "FormattedTariff";
        zTextBoxColumnStyleInfo11.IsReadOnly = true;
        zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
        zTextBoxColumnStyleInfo12.ColumnName = "EffectiveDescription";
        zTextBoxColumnStyleInfo12.IsReadOnly = true;
        zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
        zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
        zCalcEditColumnStyleInfo6.ColumnName = "DutyAmount";
        zCalcEditColumnStyleInfo6.IsReadOnly = true;
        zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
        zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
        zCalcEditColumnStyleInfo7.ColumnName = "GSTVATAmount";
        zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
        zCalcEditColumnStyleInfo8.BindToDecimalPlaces = null;
        zCalcEditColumnStyleInfo8.ColumnName = "CL_CustomsValue";
        zCalcEditColumnStyleInfo8.IsReadOnly = true;
        zCalcEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
        zCalcEditColumnStyleInfo9.BindToDecimalPlaces = null;
        zCalcEditColumnStyleInfo9.ColumnName = "CL_StatisticalValue";
        zCalcEditColumnStyleInfo9.IsReadOnly = true;
        zCalcEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(67);
        zCalcEditColumnStyleInfo10.BindToDecimalPlaces = null;
        zCalcEditColumnStyleInfo10.ColumnName = "ConfirmedVAT";
        zCalcEditColumnStyleInfo10.IsReadOnly = true;
        zCalcEditColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
        zCalcEditColumnStyleInfo11.BindToDecimalPlaces = null;
        zCalcEditColumnStyleInfo11.ColumnName = "ConfirmedDuty";
        zCalcEditColumnStyleInfo11.IsReadOnly = true;
        zCalcEditColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
        this.EntryLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
        this.EntryLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
        this.EntryLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
        this.EntryLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
        this.EntryLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
        this.EntryLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
        this.EntryLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo9);
        this.EntryLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo10);
        this.EntryLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo11);
        this.EntryLineGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 127, true);
        // 
        // ExtendedInfoGroupBox
        // 
        this.ExtendedInfoGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 340, true);
        this.ExtendedInfoGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 113, true);
        // 
        // TopVerticalSplitContainer
        // 
        // 
        // BindingSource
        // 
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.Business.JobDeclaration);
        // 
        // entryLineAdditionalDataUserControl
        // 
        this.entryLineAdditionalDataUserControl.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.entryLineAdditionalDataUserControl, ".");
        this.entryLineAdditionalDataUserControl.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("55936A76-D471-4139-885F-65173E202B4D", "Extended Info");
        this.entryLineAdditionalDataUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
        this.entryLineAdditionalDataUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 130, true);
        this.entryLineAdditionalDataUserControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
        this.entryLineAdditionalDataUserControl.Name = "entryLineAdditionalDataUserControl";
        this.entryLineAdditionalDataUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 210, true);
        this.entryLineAdditionalDataUserControl.TabIndex = 3;
        // 
        // EComMessageTabPage
        // 
        this.EComMessageTabPage.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("7C214F7C-337F-4E60-993A-DDBA9B9B600F", "eCom Messages");
        this.EComMessageTabPage.Controls.Add(this.EComMessageUserControl);
        this.EComMessageTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
        this.EComMessageTabPage.Name = "EComMessageTabPage";
        this.EComMessageTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
        this.EComMessageTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(770, 456, true);
        this.EComMessageTabPage.TabIndex = 1;
        // 
        // EComMessageUserControl
        // 
        this.EComMessageUserControl.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.EComMessageUserControl, ".");
        this.EComMessageUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
        this.EComMessageUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
        this.EComMessageUserControl.Name = "EComMessageUserControl";
        this.EComMessageUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 450, true);
        this.EComMessageUserControl.TabIndex = 0;
        // 
        // EntryHeaderChargesTabPage
        // 
        this.EntryHeaderChargesTabPage.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("2B6B0E6C-BCDC-4567-9B81-C4435EA744F7", "Entry Tax Or Fee");
        this.EntryHeaderChargesTabPage.Controls.Add(this.ConfirmedDutyAndTaxGroupBox);
        this.EntryHeaderChargesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
        this.EntryHeaderChargesTabPage.Name = "EntryHeaderChargesTabPage";
        this.EntryHeaderChargesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
        this.EntryHeaderChargesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(770, 456, true);
        this.EntryHeaderChargesTabPage.TabIndex = 2;
        // 
        // ConfirmedDutyAndTaxGroupBox
        // 
        this.ConfirmedDutyAndTaxGroupBox.AutoSize = true;
        this.ConfirmedDutyAndTaxGroupBox.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("81373A84-B998-4730-B10A-3E739F2F1EC5", "Confirmed Duty And Tax");
        this.ConfirmedDutyAndTaxGroupBox.Controls.Add(this.EntryHeaderChargesGrid);
        this.ConfirmedDutyAndTaxGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
        this.ConfirmedDutyAndTaxGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
        this.ConfirmedDutyAndTaxGroupBox.Name = "ConfirmedDutyAndTaxGroupBox";
        this.ConfirmedDutyAndTaxGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 450, true);
        this.ConfirmedDutyAndTaxGroupBox.TabIndex = 1;
        this.ConfirmedDutyAndTaxGroupBox.TabStop = false;
        // 
        // EntryHeaderChargesGrid
        // 
        this.EntryHeaderChargesGrid.AllowNavigation = false;
        this.BindingSource.SetBindingMember(this.EntryHeaderChargesGrid, "CustomsEntryHeaders.ConfirmedCharges");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CH.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).ConfirmedCharges)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.CusEntryHeaderCharges)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).ConfirmedCharges)).SyncRoot)).C1_ChargeType)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.CusEntryHeaderCharges)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).ConfirmedCharges)).SyncRoot)).ChargeTypeDescription)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CH.Business.CusEntryHeaderCharges)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).ConfirmedCharges)).SyncRoot)).C1_ChargeAmount)));
        this.EntryHeaderChargesGrid.CaptionVisible = false;
        zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("EBF7E5A1-9D6E-4E9A-93D0-A0EA8379745D", "Type");
        zDropEditColumnStyleInfo1.ColumnName = "C1_ChargeType";
        zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
        zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("83BBB4A0-5FE9-4A50-9486-E09B5662E8F2", "Description");
        zTextBoxColumnStyleInfo9.ColumnName = "ChargeTypeDescription";
        zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
        zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
        zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("32612A0A-845C-44DE-AAEF-6B55A4B7FB3A", "Total Amount");
        zCalcEditColumnStyleInfo5.ColumnName = "C1_ChargeAmount";
        zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
        this.EntryHeaderChargesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
        this.EntryHeaderChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
        this.EntryHeaderChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
        this.EntryHeaderChargesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
        this.EntryHeaderChargesGrid.GridId = "289985E0-5F43-4CB0-A919-6621F8A1833B";
        this.EntryHeaderChargesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
        this.EntryHeaderChargesGrid.ImeMode = System.Windows.Forms.ImeMode.Disable;
        this.EntryHeaderChargesGrid.LayoutKey = "EntryHeaderChargesGrid";
        this.EntryHeaderChargesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 22, true);
        this.EntryHeaderChargesGrid.Name = "EntryHeaderChargesGrid";
        this.EntryHeaderChargesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 450, true);
        this.EntryHeaderChargesGrid.TabIndex = 0;
        // 
        // MessageUserControl
        // 
        this.Name = "MessageUserControl";
        this.EntriesGroupBox.ResumeLayout(false);
        this.EntriesGroupBox.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.EntriesBoundGrid)).EndInit();
        this.EntriesBoundGrid.ResumeLayout(false);
        this.EntriesBoundGrid.PerformLayout();
        this.MainHorizontalSplitContainer.Panel1.ResumeLayout(false);
        this.MainHorizontalSplitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.MainHorizontalSplitContainer)).EndInit();
        this.MainHorizontalSplitContainer.ResumeLayout(false);
        this.MainHorizontalSplitContainer.PerformLayout();
        this.EntryLinesMessagesTabControl.ResumeLayout(false);
        this.EntryLinesMessagesTabControl.PerformLayout();
        this.EntryLinesTabPage.ResumeLayout(false);
        this.EntryLinesTabPage.PerformLayout();
        this.EntryLinesSplitContainer.ResumeLayout(false);
        this.EntryLinesSplitContainer.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.EntryLineGrid)).EndInit();
        this.EntryLineGrid.ResumeLayout(false);
        this.EntryLineGrid.PerformLayout();
        this.MessageTabPage.ResumeLayout(false);
        this.MessageTabPage.PerformLayout();
        this.TopVerticalSplitContainer.Panel1.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.TopVerticalSplitContainer)).EndInit();
        this.TopVerticalSplitContainer.ResumeLayout(false);
        this.TopVerticalSplitContainer.PerformLayout();
        this.BaseMessageUserControl.ResumeLayout(true);
        this.BaseMessageUserControl.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        this.entryLineAdditionalDataUserControl.ResumeLayout(true);
        this.entryLineAdditionalDataUserControl.PerformLayout();
        this.EComMessageTabPage.ResumeLayout(false);
        this.EComMessageTabPage.PerformLayout();
        this.EComMessageUserControl.ResumeLayout(true);
        this.EComMessageUserControl.PerformLayout();
        this.EntryHeaderChargesTabPage.ResumeLayout(false);
        this.EntryHeaderChargesTabPage.PerformLayout();
        this.ConfirmedDutyAndTaxGroupBox.ResumeLayout(false);
        this.ConfirmedDutyAndTaxGroupBox.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.EntryHeaderChargesGrid)).EndInit();
        this.EntryHeaderChargesGrid.ResumeLayout(false);
        this.EntryHeaderChargesGrid.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    EntryLineAdditionalDataUserControl entryLineAdditionalDataUserControl;
    EComMessagesTabUserControl EComMessageUserControl;
    ZTabPage EComMessageTabPage;
    internal ZTabPage EntryHeaderChargesTabPage;
    internal CargoWise.Windows.UI.KSplitContainer EntryLinesSplitContainer;
    internal ZGrid EntryHeaderChargesGrid;
    internal ZArchitecture.GUI.ZGroupBox ConfirmedDutyAndTaxGroupBox;

    #endregion
}
