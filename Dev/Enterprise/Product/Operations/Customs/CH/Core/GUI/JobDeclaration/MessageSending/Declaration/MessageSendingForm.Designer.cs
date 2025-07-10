using Enterprise.ZArchitecture;

namespace Enterprise.Customs.CH.GUI;

partial class MessageSendingForm
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
    private new void InitializeComponent()
    {
        this.Splitter = new CargoWise.Windows.UI.KSplitter();
        this.ReasonTextGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
        this.ReasonTextTextBox = new Enterprise.ZArchitecture.ZTextBox();
        this.NC123DetailsUserControl = new Enterprise.Customs.CH.GUI.NC123MessageSendingDetailsUserControl();
        this.ValidationErrorsGroupBox.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
        this.SplitContainer.Panel2.SuspendLayout();
        this.SplitContainer.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.WarningSplitContainer)).BeginInit();
        this.WarningSplitContainer.Panel1.SuspendLayout();
        this.WarningSplitContainer.SuspendLayout();
        this.messageSendingObjectsGroupBox.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.MessageSendingObjectsGrid)).BeginInit();
        this.MessageSendingObjectsGrid.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        this.ReasonTextGroupBox.SuspendLayout();
        this.NC123DetailsUserControl.SuspendLayout();
        this.SuspendLayout();
        // 
        // SplitContainer
        // 
        // 
        // SplitContainer.Panel2
        // 
        this.SplitContainer.Panel2.Controls.Add(this.NC123DetailsUserControl);
        // 
        // WarningSplitContainer
        // 
        // 
        // messageSendingObjectsGroupBox
        // 
        this.messageSendingObjectsGroupBox.Controls.Add(this.Splitter);
        this.messageSendingObjectsGroupBox.Controls.Add(this.ReasonTextGroupBox);
        this.messageSendingObjectsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(781, 137, true);
        this.messageSendingObjectsGroupBox.Controls.SetChildIndex(this.ReasonTextGroupBox, 0);
        this.messageSendingObjectsGroupBox.Controls.SetChildIndex(this.Splitter, 0);
        this.messageSendingObjectsGroupBox.Controls.SetChildIndex(this.MessageSendingObjectsGrid, 0);
        // 
        // MessageSendingObjectsGrid
        // 
        this.MessageSendingObjectsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(777, 65, true);
        // 
        // BindingSource
        // 
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.Business.ExportDeclarationMessageSendingObjectParent);
        // 
        // Splitter
        // 
        this.Splitter.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.Splitter.DoNotSaveSplitterLayout = false;
        this.Splitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 80, true);
        this.Splitter.Name = "Splitter";
        this.Splitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(777, 3, true);
        this.Splitter.TabIndex = 2;
        this.Splitter.TabStop = false;
        // 
        // ReasonTextGroupBox
        // 
        this.ReasonTextGroupBox.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("A82D7D19-C428-44BA-BEF6-90BB1F747424", "Reason Text");
        this.ReasonTextGroupBox.Controls.Add(this.ReasonTextTextBox);
        this.ReasonTextGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.ReasonTextGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 83, true);
        this.ReasonTextGroupBox.Name = "ReasonTextGroupBox";
        this.ReasonTextGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(777, 52, true);
        this.ReasonTextGroupBox.TabIndex = 3;
        this.ReasonTextGroupBox.TabStop = false;
        // 
        // ReasonTextTextBox
        // 
        this.BindingSource.SetBindingMember(this.ReasonTextTextBox, "SendingObjectsCollection.ReasonText");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.ExportDeclarationMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.ExportDeclarationMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).ReasonText)));
        this.ReasonTextTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
        this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ReasonTextTextBox, false);
        this.ReasonTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
        this.ReasonTextTextBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, 2, 1, 2, true);
        this.ReasonTextTextBox.Multiline = true;
        this.ReasonTextTextBox.Name = "ReasonTextTextBox";
        this.ReasonTextTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        this.ReasonTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(773, 35, true);
        this.ReasonTextTextBox.TabIndex = 1;
        // 
        // NC123DetailsUserControl
        // 
        this.NC123DetailsUserControl.AllowDrop = true;
        this.NC123DetailsUserControl.BackColor = System.Drawing.SystemColors.Control;
        this.BindingSource.SetBindingMember(this.NC123DetailsUserControl, ".");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.CH.Business.ExportDeclarationMessageSendingObject)(((Enterprise.Customs.CH.Business.ExportDeclarationMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.ExportDeclarationMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)))));
        this.NC123DetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
        this.NC123DetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
        this.NC123DetailsUserControl.Name = "NC123DetailsUserControl";
        this.NC123DetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(784, 240, true);
        this.NC123DetailsUserControl.TabIndex = 1;
        this.NC123DetailsUserControl.TabStop = false;
        this.NC123DetailsUserControl.Visible = false;
        // 
        // MessageSendingForm
        // 
        this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(786, 487, true);
        this.DataSourceType = typeof(Enterprise.Customs.CH.Business.ExportDeclarationMessageSendingObjectParent);
        this.Name = "MessageSendingForm";
        this.ValidationErrorsGroupBox.ResumeLayout(false);
        this.ValidationErrorsGroupBox.PerformLayout();
        this.SplitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
        this.SplitContainer.ResumeLayout(false);
        this.SplitContainer.PerformLayout();
        this.WarningSplitContainer.Panel1.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.WarningSplitContainer)).EndInit();
        this.WarningSplitContainer.ResumeLayout(false);
        this.WarningSplitContainer.PerformLayout();
        this.messageSendingObjectsGroupBox.ResumeLayout(false);
        this.messageSendingObjectsGroupBox.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.MessageSendingObjectsGrid)).EndInit();
        this.MessageSendingObjectsGrid.ResumeLayout(false);
        this.MessageSendingObjectsGrid.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        this.ReasonTextGroupBox.ResumeLayout(false);
        this.ReasonTextGroupBox.PerformLayout();
        this.NC123DetailsUserControl.ResumeLayout(true);
        this.NC123DetailsUserControl.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private CargoWise.Windows.UI.KSplitter Splitter;
    internal ZArchitecture.GUI.ZGroupBox ReasonTextGroupBox;
    internal ZTextBox ReasonTextTextBox;
    internal NC123MessageSendingDetailsUserControl NC123DetailsUserControl;
}
