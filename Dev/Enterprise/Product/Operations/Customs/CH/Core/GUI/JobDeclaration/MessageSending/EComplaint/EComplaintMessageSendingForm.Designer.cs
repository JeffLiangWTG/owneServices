using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

partial class EComplaintMessageSendingForm
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

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private new void InitializeComponent()
    {
        Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
        Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
        Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
        Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
        this.LinesGrid = new Enterprise.ZArchitecture.ZGrid();
        this.LinesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
        this.LineDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
        this.LocationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
        this.EntryLineDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
        this.FieldNameDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
        this.RemarkTextBox = new Enterprise.ZArchitecture.ZTextBox();
        this.CorrectionReasonDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
        this.AttachedDeclarationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
        this.SendButton = new Enterprise.ZArchitecture.GUI.ZButton();
        this.CancelButton2 = new Enterprise.ZArchitecture.GUI.ZButton();
        ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.LinesGrid)).BeginInit();
        this.LinesGrid.SuspendLayout();
        this.LinesGroupBox.SuspendLayout();
        this.LineDetailsPanel.SuspendLayout();
        this.LocationDropEdit.SuspendLayout();
        this.EntryLineDropEdit.SuspendLayout();
        this.FieldNameDropEdit.SuspendLayout();
        this.CorrectionReasonDropEdit.SuspendLayout();
        this.SuspendLayout();
        // 
        // MainStatusBar
        // 
        this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 427, true);
        this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 23, true);
        // 
        // BindingSource
        // 
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.Business.EComplaintMessageSendingObject);
        // 
        // LinesGrid
        // 
        this.LinesGrid.AllowNavigation = false;
        this.BindingSource.SetBindingMember(this.LinesGrid, "SendingObjectLines");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CH.Business.EComplaintMessageSendingObject)(null)).SendingObjectLines)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.EComplaintMessageSendingObjectLine)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.EComplaintMessageSendingObject)(null)).SendingObjectLines)).SyncRoot)).Location)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CH.Business.EComplaintMessageSendingObjectLine)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.EComplaintMessageSendingObject)(null)).SendingObjectLines)).SyncRoot)).EntryLinePK)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.EComplaintMessageSendingObjectLine)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.EComplaintMessageSendingObject)(null)).SendingObjectLines)).SyncRoot)).FieldName)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.EComplaintMessageSendingObjectLine)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.EComplaintMessageSendingObject)(null)).SendingObjectLines)).SyncRoot)).Remark)));
        this.LinesGrid.CaptionVisible = false;
        zDropEditColumnStyleInfo1.ColumnName = "Location";
        zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
        zGuidDropEditColumnStyleInfo1.ColumnName = "EntryLinePK";
        zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
        zDropEditColumnStyleInfo2.ColumnName = "FieldName";
        zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
        zTextBoxColumnStyleInfo1.ColumnName = "Remark";
        zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
        this.LinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
        this.LinesGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
        this.LinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
        this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
        this.LinesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
        this.LinesGrid.GridId = "90D3D8B2-DF34-4563-AF07-9C86246685B3";
        this.LinesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
        this.LinesGrid.LayoutKey = "EComplaintMessageSendingObjectsGrid";
        this.LinesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
        this.LinesGrid.Name = "LinesGrid";
        this.LinesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(772, 115, true);
        this.LinesGrid.TabIndex = 1;
        // 
        // LinesGroupBox
        // 
        this.LinesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.LinesGroupBox.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("76FAFFDC-689B-4F0F-BFA6-CF1E927A54C2", "eCom Lines");
        this.LinesGroupBox.Controls.Add(this.LinesGrid);
        this.LinesGroupBox.Controls.Add(this.LineDetailsPanel);
        this.LinesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 68, true);
        this.LinesGroupBox.Name = "LinesGroupBox";
        this.LinesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(778, 316, true);
        this.LinesGroupBox.TabIndex = 1;
        this.LinesGroupBox.TabStop = false;
        // 
        // LineDetailsPanel
        // 
        this.LineDetailsPanel.Controls.Add(this.LocationDropEdit);
        this.LineDetailsPanel.Controls.Add(this.EntryLineDropEdit);
        this.LineDetailsPanel.Controls.Add(this.FieldNameDropEdit);
        this.LineDetailsPanel.Controls.Add(this.RemarkTextBox);
        this.LineDetailsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.LineDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 131, true);
        this.LineDetailsPanel.Name = "LineDetailsPanel";
        this.LineDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(772, 182, true);
        this.LineDetailsPanel.TabIndex = 0;
        // 
        // LocationDropEdit
        // 
        this.LocationDropEdit.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.LocationDropEdit, "SendingObjectLines.Location");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CH.Business.EComplaintMessageSendingObjectLine)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.EComplaintMessageSendingObject)(null)).SendingObjectLines)).SyncRoot)).Location)));
        this.LocationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 17, true);
        this.LocationDropEdit.Name = "LocationDropEdit";
        this.LocationDropEdit.PreBoundMaxLength = 1;
        this.LocationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
        this.LocationDropEdit.TabIndex = 0;
        // 
        // EntryLineDropEdit
        // 
        this.EntryLineDropEdit.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.EntryLineDropEdit, "SendingObjectLines.EntryLinePK");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CH.Business.EComplaintMessageSendingObjectLine)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.EComplaintMessageSendingObject)(null)).SendingObjectLines)).SyncRoot)).EntryLinePK)));
        this.EntryLineDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 43, true);
        this.EntryLineDropEdit.Name = "EntryLineDropEdit";
        this.EntryLineDropEdit.PreBoundMaxLength = 4;
        this.EntryLineDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
        this.EntryLineDropEdit.TabIndex = 1;
        // 
        // FieldNameDropEdit
        // 
        this.FieldNameDropEdit.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.FieldNameDropEdit, "SendingObjectLines.FieldName");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CH.Business.EComplaintMessageSendingObjectLine)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.EComplaintMessageSendingObject)(null)).SendingObjectLines)).SyncRoot)).FieldName)));
        this.FieldNameDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 69, true);
        this.FieldNameDropEdit.Name = "FieldNameDropEdit";
        this.FieldNameDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
        this.FieldNameDropEdit.TabIndex = 2;
        // 
        // RemarkTextBox
        // 
        this.RemarkTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)));
        this.BindingSource.SetBindingMember(this.RemarkTextBox, "SendingObjectLines.Remark");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.EComplaintMessageSendingObjectLine)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.EComplaintMessageSendingObject)(null)).SendingObjectLines)).SyncRoot)).Remark)));
        this.RemarkTextBox.CaptionResourceString = null;
        this.RemarkTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
        this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.RemarkTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
        this.LabelCaptionRenderProvider.SetLabelTop(this.RemarkTextBox, 0);
        this.RemarkTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 95, true);
        this.RemarkTextBox.Multiline = true;
        this.RemarkTextBox.Name = "RemarkTextBox";
        this.RemarkTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(696, 84, true);
        this.RemarkTextBox.TabIndex = 2;
        // 
        // CorrectionReasonDropEdit
        // 
        this.CorrectionReasonDropEdit.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.CorrectionReasonDropEdit, "CorrectionReason");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CH.Business.EComplaintMessageSendingObject)(null)).CorrectionReason)));
        this.CorrectionReasonDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 12, true);
        this.CorrectionReasonDropEdit.Name = "CorrectionReasonDropEdit";
        this.CorrectionReasonDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
        this.CorrectionReasonDropEdit.TabIndex = 4;
        // 
        // AttachedDeclarationCheckBox
        // 
        this.BindingSource.SetBindingMember(this.AttachedDeclarationCheckBox, "IsAttachedDeclaration");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CH.Business.EComplaintMessageSendingObject)(null)).IsAttachedDeclaration)));
        this.AttachedDeclarationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 38, true);
        this.AttachedDeclarationCheckBox.Name = "AttachedDeclarationCheckBox";
        this.AttachedDeclarationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(204, 24, true);
        this.AttachedDeclarationCheckBox.TabIndex = 4;
        // 
        // SendButton
        // 
        this.SendButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.SendButton.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("B7D68439-BE29-4957-A677-C6BA88F5FFAC", "&Send");
        this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(589, 390, true);
        this.SendButton.Name = "SendButton";
        this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 21, true);
        this.SendButton.TabIndex = 2;
        this.SendButton.ToolTipCaption = null;
        this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
        // 
        // CancelButton2
        // 
        this.CancelButton2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.CancelButton2.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("E8C029F5-FADB-46C4-8524-EB8CEC1D69E0", "&Cancel");
        this.CancelButton2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(683, 390, true);
        this.CancelButton2.Name = "CancelButton2";
        this.CancelButton2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 21, true);
        this.CancelButton2.TabIndex = 3;
        this.CancelButton2.ToolTipCaption = null;
        this.CancelButton2.Click += new System.EventHandler(this.CancelButton_Click);
        // 
        // EComplaintMessageSendingForm
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.AutoScroll = true;
        this.CaptionRenderingEnabled = true;
        this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 450, true);
        this.Controls.Add(this.CorrectionReasonDropEdit);
        this.Controls.Add(this.AttachedDeclarationCheckBox);
        this.Controls.Add(this.LinesGroupBox);
        this.Controls.Add(this.SendButton);
        this.Controls.Add(this.CancelButton2);
        this.DataSourceType = typeof(Enterprise.Customs.CH.Business.EComplaintMessageSendingObject);
        this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(816, 489, true);
        this.Name = "EComplaintMessageSendingForm";
        this.Text = "EComplaintMessageSendingForm";
        this.Controls.SetChildIndex(this.MainStatusBar, 0);
        this.Controls.SetChildIndex(this.CancelButton2, 0);
        this.Controls.SetChildIndex(this.SendButton, 0);
        this.Controls.SetChildIndex(this.LinesGroupBox, 0);
        this.Controls.SetChildIndex(this.AttachedDeclarationCheckBox, 0);
        this.Controls.SetChildIndex(this.CorrectionReasonDropEdit, 0);
        ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.LinesGrid)).EndInit();
        this.LinesGrid.ResumeLayout(false);
        this.LinesGrid.PerformLayout();
        this.LinesGroupBox.ResumeLayout(false);
        this.LinesGroupBox.PerformLayout();
        this.LineDetailsPanel.ResumeLayout(false);
        this.LineDetailsPanel.PerformLayout();
        this.LocationDropEdit.ResumeLayout(true);
        this.LocationDropEdit.PerformLayout();
        this.EntryLineDropEdit.ResumeLayout(true);
        this.EntryLineDropEdit.PerformLayout();
        this.FieldNameDropEdit.ResumeLayout(true);
        this.FieldNameDropEdit.PerformLayout();
        this.CorrectionReasonDropEdit.ResumeLayout(true);
        this.CorrectionReasonDropEdit.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    internal ZGroupBox LinesGroupBox;
    internal ZArchitecture.ZGrid LinesGrid;
    internal Enterprise.ZArchitecture.GUI.ZButton SendButton;
    internal Enterprise.ZArchitecture.GUI.ZButton CancelButton2;

    internal ZDropEdit CorrectionReasonDropEdit;
    internal ZCheckBox AttachedDeclarationCheckBox;
    internal ZPanel LineDetailsPanel;
    internal ZDropEdit LocationDropEdit;
    internal ZGuidDropEdit EntryLineDropEdit;
    internal ZDropEdit FieldNameDropEdit;
    internal ZTextBox RemarkTextBox;
}
