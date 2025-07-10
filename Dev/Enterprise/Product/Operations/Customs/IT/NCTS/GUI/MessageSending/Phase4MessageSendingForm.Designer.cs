namespace Enterprise.Customs.IT.NCTS.GUI
{
	partial class Phase4MessageSendingForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.SendSplitButton = new Enterprise.Customs.IT.GUI.SendingModeSplitButton();
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
			// SplitContainer
			// 
			// 
			// SendButton
			// 
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(588, 419, true);
			this.SendButton.Visible = false;
			// 
			// CancelButton2
			// 
			this.CancelButton2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(684, 419, true);
			this.CancelButton2.TabIndex = 4;
			// 
			// messageSendingObjectsGroupBox
			// 
			this.messageSendingObjectsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(781, 154, true);
			// 
			// MessageSendingObjectsGrid
			// 
			this.MessageSendingObjectsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(775, 135, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 448, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(784, 23, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IT.NCTS.Business.NctsHeaderDepartureMessageSendingObjectParent);
			// 
			// SendSplitButton
			// 
			this.SendSplitButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SendSplitButton.CaptionResourceString = Enterprise.Customs.IT.NCTS.GUI.Res.GetData("9a765b2e-418a-4d34-b270-8a31c61b9926", "&Send");
			this.SendSplitButton.IsCaptionOverridden = false;
			this.SendSplitButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(588, 419, true);
			this.SendSplitButton.Name = "SendSplitButton";
			this.SendSplitButton.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 10, 0, true);
			this.SendSplitButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SendSplitButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 21, true);
			this.SendSplitButton.TabIndex = 6;
			this.SendSplitButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.SendSplitButton.ToolTipCaption = null;
			// 
			// MessageSendingForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(784, 471, true);
			this.Controls.Add(this.SendSplitButton);
			this.DataSourceType = typeof(Enterprise.Customs.IT.NCTS.Business.NctsHeaderDepartureMessageSendingObjectParent);
			this.Name = "MessageSendingForm";
			this.Controls.SetChildIndex(this.SendWithValidationErrorsCheckBox, 0);
			this.Controls.SetChildIndex(this.SendWithAdditionalWarningCheckBox, 0);
			this.Controls.SetChildIndex(this.SplitContainer, 0);
			this.Controls.SetChildIndex(this.SendSplitButton, 0);
			this.Controls.SetChildIndex(this.SendButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CancelButton2, 0);
			this.Controls.SetChildIndex(this.messageSendingObjectsGroupBox, 0);
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

		internal IT.GUI.SendingModeSplitButton SendSplitButton;

		#endregion

		private System.ComponentModel.IContainer components;
	}
}
