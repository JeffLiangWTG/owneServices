namespace Enterprise.Accounting.GUI.Base
{
	partial class MultipleReversingBaseForm
	{
		System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		new void InitializeComponent()
		{
			this.PostingButtonsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.RemoveErrorTransactionsButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PostingButtons = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PostingButtonsPanel.SuspendLayout();
			this.PostingButtons.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 272, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(606, 24, true);
			this.MainStatusBar.TabIndex = 2;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.Base.Reversing.MultipleReversingProviderBase);
			// 
			// PostingButtonsPanel
			// 
			this.PostingButtonsPanel.Controls.Add(this.RemoveErrorTransactionsButton);
			this.PostingButtonsPanel.Controls.Add(this.PostingButtons);
			this.PostingButtonsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.PostingButtonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 237, true);
			this.PostingButtonsPanel.Name = "PostingButtonsPanel";
			this.PostingButtonsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(606, 35, true);
			this.PostingButtonsPanel.TabIndex = 1;
			// 
			// RemoveErrorTransactionsButton
			// 
			this.RemoveErrorTransactionsButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("MultipleReversingForm|686a7a6d-7f5f-4eee-9a9b-2aede9d0c84e", "Remove Transactions That Can\'t Be Reversed");
			this.RemoveErrorTransactionsButton.IsCaptionOverridden = false;
			this.RemoveErrorTransactionsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 6, true);
			this.RemoveErrorTransactionsButton.Name = "RemoveErrorTransactionsButton";
			this.RemoveErrorTransactionsButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.RemoveErrorTransactionsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(257, 23, true);
			this.RemoveErrorTransactionsButton.TabIndex = 1;
			this.RemoveErrorTransactionsButton.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.RemoveErrorTransactionsButton.ToolTipCaption = null;
			this.RemoveErrorTransactionsButton.UseVisualStyleBackColor = true;
			this.RemoveErrorTransactionsButton.Click += new System.EventHandler(this.RemoveErrorTransactionsButton_Click);
			// 
			// PostingButtons
			// 
			this.PostingButtons.AllowDrop = true;
			this.PostingButtons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtons.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(293, 6, true);
			this.PostingButtons.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtons.Name = "PostingButtons";
			this.PostingButtons.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(301, 25, true);
			this.PostingButtons.TabIndex = 0;
			// 
			// MultipleReversingBaseForm
			// 
			this.AutoAddPreviousNextButtons = false;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("fd0caafa-2264-4d49-bf15-28da222f0874", "Multiple Reversing Base Form");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(606, 296, true);
			this.Controls.Add(this.PostingButtonsPanel);
			this.DataSourceType = typeof(Enterprise.Accounting.Business.Base.Reversing.MultipleReversingProviderBase);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(560, 200, true);
			this.Name = "MultipleReversingBaseForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.PostingButtonsPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PostingButtonsPanel.ResumeLayout(false);
			this.PostingButtonsPanel.PerformLayout();
			this.PostingButtons.ResumeLayout(true);
			this.PostingButtons.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZPanel PostingButtonsPanel;
		protected Enterprise.Core.Forms.ZPostingButtonsUserControl PostingButtons;
		private Enterprise.ZArchitecture.GUI.ZButton RemoveErrorTransactionsButton;
	}
}
