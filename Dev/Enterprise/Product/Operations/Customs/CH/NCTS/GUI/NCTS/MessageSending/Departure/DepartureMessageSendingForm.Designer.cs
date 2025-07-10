using Enterprise.Customs.CH.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.NCTS.GUI;

partial class DepartureMessageSendingForm
{
    #region Component Designer generated code

    private new void InitializeComponent()
    {
        this.ReasonTextGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
        this.ReasonTextTextBox = new Enterprise.ZArchitecture.ZTextBox();
        this.NT141DetailsUserControl = new Enterprise.Customs.CH.NCTS.GUI.NT141MessageSendingDetailsUserControl();
        this.NC123DetailsUserControl = new Enterprise.Customs.CH.NCTS.GUI.NC123MessageSendingDetailsUserControl();
        ((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
        this.SplitContainer.Panel2.SuspendLayout();
        this.SplitContainer.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.WarningSplitContainer)).BeginInit();
        this.WarningSplitContainer.SuspendLayout();
        this.messageSendingObjectsGroupBox.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.MessageSendingObjectsGrid)).BeginInit();
        this.MessageSendingObjectsGrid.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        this.ReasonTextGroupBox.SuspendLayout();
        this.NT141DetailsUserControl.SuspendLayout();
        this.SuspendLayout();
        //
        // SplitContainer
        //
        //
        // SplitContainer.Panel2
        //
        this.SplitContainer.Panel2.Controls.Add(this.NT141DetailsUserControl);
        this.SplitContainer.Panel2.Controls.Add(this.NC123DetailsUserControl);
        this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(806, 381);
        //
        // WarningSplitContainer
        //
        this.WarningSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(806, 240, true);
        //
        // SendButton
        //
        this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(590, 433, true);
        //
        // CancelButton2
        //
        this.CancelButton2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(686, 433, true);
        //
        // messageSendingObjectsGroupBox
        //
        this.messageSendingObjectsGroupBox.Controls.Add(this.ReasonTextGroupBox);
        this.messageSendingObjectsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(781, 137, true);
        this.messageSendingObjectsGroupBox.Controls.SetChildIndex(this.MessageSendingObjectsGrid, 0);
        this.messageSendingObjectsGroupBox.Controls.SetChildIndex(this.ReasonTextGroupBox, 0);
        //
        // MessageSendingObjectsGrid
        //
        this.MessageSendingObjectsGrid.Dock = System.Windows.Forms.DockStyle.Top;
        this.MessageSendingObjectsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(778, 41, true);
        //
        // MainStatusBar
        //
        this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(787, 23, true);
        //
        // BindingSource
        //
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.NCTS.Business.NctsHeaderDepartureMessageSendingObjectParent);
        //
        // ReasonTextGroupBox
        //
        this.ReasonTextGroupBox.CaptionResourceString = Enterprise.Customs.CH.NCTS.GUI.Res.GetData("1028fb63-2bf7-45e0-961c-2bf698647c32", "Reason Text");
        this.ReasonTextGroupBox.Controls.Add(this.ReasonTextTextBox);
        this.ReasonTextGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
        this.ReasonTextGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 54, true);
        this.ReasonTextGroupBox.Name = "ReasonTextGroupBox";
        this.ReasonTextGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(778, 81, true);
        this.ReasonTextGroupBox.TabIndex = 2;
        this.ReasonTextGroupBox.TabStop = false;
        //
        // ReasonTextTextBox
        //
        this.BindingSource.SetBindingMember(this.ReasonTextTextBox, "SendingObjectsCollection.ReasonText");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.NCTS.Business.NctsHeaderDepartureMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.CH.NCTS.Business.NctsHeaderDepartureMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).ReasonText)));
        this.ReasonTextTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
        this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ReasonTextTextBox, false);
        this.ReasonTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 14, true);
        this.ReasonTextTextBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, 2, 1, 2, true);
        this.ReasonTextTextBox.Multiline = true;
        this.ReasonTextTextBox.Name = "ReasonTextTextBox";
        this.ReasonTextTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        this.ReasonTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(776, 66, true);
        this.ReasonTextTextBox.TabIndex = 0;
        //
        // NT141DetailsUserControl
        //
        this.NT141DetailsUserControl.AllowDrop = true;
        this.NT141DetailsUserControl.BackColor = System.Drawing.SystemColors.Control;
        this.BindingSource.SetBindingMember(this.NT141DetailsUserControl, ".");
        this.NT141DetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
        this.NT141DetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
        this.NT141DetailsUserControl.Name = "NT141DetailsUserControl";
        this.NT141DetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(786, 240, true);
        this.NT141DetailsUserControl.TabStop = false;
        this.NT141DetailsUserControl.Visible = false;
        //
        //
        this.NC123DetailsUserControl.AllowDrop = true;
        this.NC123DetailsUserControl.BackColor = System.Drawing.SystemColors.Control;
        this.BindingSource.SetBindingMember(this.NC123DetailsUserControl, "SendingObjectsCollection");
        this.NC123DetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
        this.NC123DetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
        this.NC123DetailsUserControl.Name = "NC123DetailsUserControl";
        this.NC123DetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(806, 240, true);
        this.NC123DetailsUserControl.TabStop = false;
        this.NC123DetailsUserControl.Visible = false;
        //
        // DepartureMessageSendingForm
        //
        this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(787, 487, true);
        this.DataSourceType = typeof(Enterprise.Customs.CH.NCTS.Business.NctsHeaderDepartureMessageSendingObjectParent);
        this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 510);
        this.Name = "DepartureMessageSendingForm";
        this.SplitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
        this.SplitContainer.ResumeLayout(false);
        this.SplitContainer.PerformLayout();
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
        this.NT141DetailsUserControl.ResumeLayout(true);
        this.NT141DetailsUserControl.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    internal ZGroupBox ReasonTextGroupBox;
    internal ZTextBox ReasonTextTextBox;
    protected NT141MessageSendingDetailsUserControl NT141DetailsUserControl;
    protected NC123MessageSendingDetailsUserControl NC123DetailsUserControl;
}
