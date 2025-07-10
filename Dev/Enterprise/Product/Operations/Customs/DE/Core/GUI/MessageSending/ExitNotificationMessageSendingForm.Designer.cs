namespace Enterprise.Customs.DE.GUI
{
	partial class ExitNotificationMessageSendingForm
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
		new void InitializeComponent()
		{
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
			this.SuspendLayout();
			// 
			// SendWithValidationErrorsCheckBox
			// 
			this.SendWithValidationErrorsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 458, true);
			// 
			// SendWithAdditionalWarningCheckBox
			// 
			this.SendWithAdditionalWarningCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 478, true);
			// 
			// SplitContainer
			// 
			this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(784, 452, true);
			this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(100);
			// 
			// WarningSplitContainer
			// 
			this.WarningSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(784, 348, true);
			this.WarningSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(79);
			// 
			// SendButton
			// 
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(600, 494, true);
			// 
			// CancelButton2
			// 
			this.CancelButton2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(696, 494, true);
			// 
			// messageSendingObjectsGroupBox
			// 
			this.messageSendingObjectsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(781, 100, true);
			// 
			// MessageSendingObjectsGrid
			// 
			this.MessageSendingObjectsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(775, 81, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 519, true);
			// 
			// ExitNotificationMessageSendingForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 542, true);
			this.Name = "ExitNotificationMessageSendingForm";
			this.Text = "ExitNotificationMessageSendingForm";
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
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}
