using CargoWiseOne.ResourceStrings;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.H7.GUI
{
	partial class UploadDocumentsForm
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
			this.SelectAllButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ValidationErrorsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
			this.SplitContainer.Panel1.SuspendLayout();
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
			this.SuspendLayout();
			// 
			// ValidationErrorsGroupBox
			// 
			this.ValidationErrorsGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, 2, 2, 23, true);
			this.ValidationErrorsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(973, 373, true);
			// 
			// SendWithValidationErrorsCheckBox
			// 
			this.SendWithValidationErrorsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 747, true);
			// 
			// SendWithAdditionalWarningCheckBox
			// 
			this.SendWithAdditionalWarningCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 773, true);
			// 
			// PreviewMessageCheckBox
			// 
			this.PreviewMessageCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Bottom;
			this.PreviewMessageCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 823, true);
			this.PreviewMessageCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(386, 22, true);
			PreviewMessageCheckBox.AllowOverlap(SplitContainer);
			// 
			// SplitContainer
			// 
			this.SplitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			// 
			// SplitContainer.Panel1
			// 
			this.SplitContainer.Panel1.Controls.Add(this.SelectAllButton);
			this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(973, 887, true);
			this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(367);
			// 
			// WarningSplitContainer
			// 
			this.WarningSplitContainer.Dock = System.Windows.Forms.DockStyle.Top;
			this.WarningSplitContainer.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, 2, 2, 20, true);
			// 
			// WarningSplitContainer.Panel2
			// 
			this.WarningSplitContainer.Panel2.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 0, 23, true);
			this.WarningSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(973, 516, true);
			this.WarningSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(373);
			this.WarningSplitContainer.Visible = false;
			// 
			// SendButton
			// 
			this.SendButton.Anchor = System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Bottom;
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(775, 823, true);
			SendButton.AllowOverlap(SplitContainer);
			// 
			// CancelButton2
			// 
			this.CancelButton2.Anchor = System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Bottom;
			this.CancelButton2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(874, 823, true);
			CancelButton2.AllowOverlap(SplitContainer);
			// 
			// messageSendingObjectsGroupBox
			// 
			this.messageSendingObjectsGroupBox.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.messageSendingObjectsGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 3, 3, 25, true);
			// 
			// MessageSendingObjectsGrid
			// 
			this.MessageSendingObjectsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 15, true);
			this.MessageSendingObjectsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(775, 111, true);
			BindingSource.SetBindingMember(MessageSendingObjectsGrid, "UploadDocumentsSendingActionFilteredCollection");
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 854, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(973, 33, true);
			// 
			// filterStripControl
			// 
			filterStripControl = new ZFilterStripBaseControl(MessageSendingObjectsGrid, GetUploadDocumentActionFilterBusinessObject());
			filterStripControl.Dock = DockStyle.Top;
			filterStripControl.PerformSearch += FilterStripControl_PerformSearch;
			filterStripControl.AllowDrop = true;
			filterStripControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			filterStripControl.Name = "filterStripControl";
			filterStripControl.AutoSize = true;
			filterStripControl.TabIndex = 0;
			filterStripControl.CaptionRenderingEnabled = true;
			messageSendingObjectsGroupBox.Controls.Add(filterStripControl);
			// 
			// SelectAllButton
			// 
			this.SelectAllButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SelectAllButton.CaptionResourceString = Enterprise.Customs.EU.H7.GUI.Res.GetData("4137acde-7f5a-46b0-a0c3-0e1c968e97a5", "Select/Deselect All");
			this.SelectAllButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(815, 344, true);
			this.SelectAllButton.Name = "SelectAllButton";
			this.SelectAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(149, 21, true);
			this.SelectAllButton.TabIndex = 6;
			this.SelectAllButton.ToolTipCaption = null;
			this.SelectAllButton.UseVisualStyleBackColor = true;
			this.SelectAllButton.Click += new System.EventHandler(this.SelectAllButton_Click);
			SelectAllButton.AllowOverlap(messageSendingObjectsGroupBox);
			// 
			// UploadDocumentsForm
			//
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(973, 887, true);
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(988, 924, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(988, 725, true);
			this.Name = "UploadDocumentsForm";
			this.Text = "UploadDocumentsForm";
			this.ValidationErrorsGroupBox.ResumeLayout(false);
			this.ValidationErrorsGroupBox.PerformLayout();
			this.SplitContainer.Panel1.ResumeLayout(false);
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
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZArchitecture.GUI.ZButton SelectAllButton;
	}
}
