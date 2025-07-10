using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.NCTS.GUI;

partial class ArrivalMessageSendingForm
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
		this.MultipleMrnUserControl = new MultipleMrnMessageSendingDetailsUserControl();
		this.TopSplitContainer = new KSplitContainer();
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ArrivalMessageSendingForm));
        ((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
		this.TopSplitContainer.SuspendLayout();
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
		this.MultipleMrnUserControl.SuspendLayout();
		this.SuspendLayout();
		// 
		// SplitContainer
		//
		this.SplitContainer.Panel1.Controls.Add(this.TopSplitContainer);
		//
		// TopSplitContainer
		//
		this.TopSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
		this.TopSplitContainer.Name = "TopSplitContainer";
		this.TopSplitContainer.Panel2.Controls.Add(this.MultipleMrnUserControl);
		this.TopSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(60);
		this.TopSplitContainer.Orientation = Orientation.Horizontal;
		//
		// WarningSplitContainer
		//
		this.WarningSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(806, 240, true);
		// 
		// messageSendingObjectsGroupBox
		//
		this.messageSendingObjectsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(781, 140, true);
		//
		// MessageSendingObjectsGrid
		//
		this.MessageSendingObjectsGrid.Anchor = AnchorStyles.Top | AnchorStyles.Left;
		this.MessageSendingObjectsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
		this.MessageSendingObjectsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(778, 140, true);
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.NCTS.Business.NctsHeaderArrivalMessageSendingObjectParent);
		//
		// MultipleMrnUserControl
		//
		this.MultipleMrnUserControl.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		this.MultipleMrnUserControl.AllowDrop = true;
		this.MultipleMrnUserControl.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.MultipleMrnUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 54, true);
		this.MultipleMrnUserControl.Name = "MultipleMrnUserControl";
		this.MultipleMrnUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(786, 60, true);
		this.MultipleMrnUserControl.TabIndex = 2;
		this.MultipleMrnUserControl.TabStop = false;
		// 
		// ArrivalMessageSendingForm
		//
		this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(787, 487, true);
		this.DataSourceType = typeof(Enterprise.Customs.CH.NCTS.Business.NctsHeaderArrivalMessageSendingObjectParent);
		this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 510);
		this.Name = "ArrivalMessageSendingForm";
        this.SplitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
        this.SplitContainer.ResumeLayout(false);
        this.SplitContainer.PerformLayout();
		this.TopSplitContainer.ResumeLayout(false);
		this.TopSplitContainer.PerformLayout();
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
		this.MultipleMrnUserControl.ResumeLayout(true);
		this.MultipleMrnUserControl.PerformLayout();
		this.ResumeLayout(false);
        this.PerformLayout();
    }

	#endregion

	internal KSplitContainer TopSplitContainer;
	internal MultipleMrnMessageSendingDetailsUserControl MultipleMrnUserControl;
}
