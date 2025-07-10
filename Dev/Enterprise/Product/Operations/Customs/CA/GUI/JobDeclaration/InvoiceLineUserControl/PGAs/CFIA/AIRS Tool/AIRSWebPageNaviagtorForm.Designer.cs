namespace Enterprise.Customs.CA.GUI
{
	partial class AIRSWebPageNaviagtorForm
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
			this.SplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.WebBrowser = new Enterprise.ZArchitecture.GUI.ZWebBrowser();
			this.LoadingLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CopyValueButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 557, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1094, 22, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.AIRSWebpageNavigator);
			// 
			// SplitContainer
			// 
			this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.SplitContainer.IsSplitterFixed = true;
			this.SplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.SplitContainer.Name = "SplitContainer";
			this.SplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// SplitContainer.Panel1
			// 
			this.SplitContainer.Panel1.Controls.Add(this.WebBrowser);
			this.SplitContainer.Panel1.Controls.Add(this.LoadingLabel);
			this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(953, 631, true);
			this.SplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(500);
			// 
			// SplitContainer.Panel2
			//
			this.SplitContainer.Panel2.Controls.Add(this.CopyValueButton);
			this.SplitContainer.Panel2.Controls.Add(this.CloseButton);
			this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(953, 560, true);
			this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(531);
			this.SplitContainer.TabIndex = 1;
			// 
			// WebBrowser
			// 
			this.WebBrowser.AllowWebBrowserDrop = false;
			this.WebBrowser.IsWebBrowserContextMenuEnabled = false;
			this.WebBrowser.WebBrowserShortcutsEnabled = false;
			this.WebBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
			this.WebBrowser.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.WebBrowser.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
			this.WebBrowser.Name = "WebBrowser";
			this.WebBrowser.ScriptErrorsSuppressed = true;
			this.WebBrowser.Visible = false;
			this.WebBrowser.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(470, 601, true);
			this.WebBrowser.TabIndex = 1;
			// 
			// LoadingLabel
			// 
			this.LoadingLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LoadingLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.LoadingLabel.Name = "LoadingLabel";
			this.LoadingLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(470, 601, true);
			this.LoadingLabel.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("3AE0B3DA-2730-4508-8A22-32BB678D0CB3", "System is loading...");
			this.LoadingLabel.TabIndex = 1;
			// 
			// CopyValueButton
			// 
			this.CopyValueButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CopyValueButton.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("8BA2FEB5-24B0-48EB-AAB3-9AF1842920A8", "Copy to CFIA");
			this.CopyValueButton.IsCaptionOverridden = false;
			this.CopyValueButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(719, 1, true);
			this.CopyValueButton.Name = "CopyValueButton";
			this.CopyValueButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CopyValueButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 23, true);
			this.CopyValueButton.TabIndex = 2;
			this.CopyValueButton.Visible = false;
			this.CopyValueButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.CopyValueButton.ToolTipCaption = null;
			this.CopyValueButton.UseVisualStyleBackColor = true;
			this.CopyValueButton.Click += new System.EventHandler(this.CopyValueButton_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CCA2E3FA-E8F7-4B17-BC5D-8B9811CB1BEE", "Cancel");
			this.CloseButton.IsCaptionOverridden = false;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(829, 1, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 23, true);
			this.CloseButton.TabIndex = 3;
			this.CloseButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.CloseButton.ToolTipCaption = null;
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// AIRSWebPageNaviagtorForm
			//
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1094, 796, true);
			this.Controls.Add(this.SplitContainer);
			this.DataSourceType = typeof(Enterprise.Customs.CA.Business.AIRSWebpageNavigator);
			this.Name = "AIRSWebPageNaviagtorForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1094, 696, true);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
			this.SplitContainer.ResumeLayout(false);
			this.SplitContainer.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		private Enterprise.ZArchitecture.GUI.ZWebBrowser WebBrowser;
		private Enterprise.ZArchitecture.ZLabel LoadingLabel;
		private CargoWise.Windows.UI.KSplitContainer SplitContainer;
		internal ZArchitecture.GUI.ZButton CopyValueButton;
		internal ZArchitecture.GUI.ZButton CloseButton;
	}
}
