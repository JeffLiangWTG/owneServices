namespace Enterprise.ServiceManager.GUI
{
	partial class ServiceTaskLogViewerForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.RefreshButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.LogViewer = new Enterprise.ServiceManager.GUI.LogViewerControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			//
			// MainStatusBar
			//
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 550, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(865, 24, true);
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.ServiceManager.Business.ServiceTaskLogViewer);
			//
			// CloseButton
			//
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("ServiceTaskLogViewerForm|b6fb29ee-f5ba-47d6-a92f-54a1610b4953", "Close");
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(787, 521, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 2;
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			//
			// RefreshButton
			//
			this.RefreshButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.RefreshButton.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("ServiceTaskLogViewerForm|92555a75-0b6d-48b2-afcf-ad8d628bfbe3", "Refresh");
			this.RefreshButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(706, 521, true);
			this.RefreshButton.Name = "RefreshButton";
			this.RefreshButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.RefreshButton.TabIndex = 1;
			this.RefreshButton.UseVisualStyleBackColor = true;
			this.RefreshButton.Click += new System.EventHandler(this.RefreshButton_Click);
			//
			// LogViewer
			//
			this.LogViewer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.LogViewer, ".");
			this.LogViewer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LogViewer.Name = "LogViewer";
			this.LogViewer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(862, 500, true);
			this.LogViewer.TabIndex = 0;
			//
			// ServiceTaskLogViewerForm
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoScroll = true;
			this.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 200, true);
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("7a4d893d-cf71-4f01-8a79-7be2d56ef944", "Service Tasks Log Viewer");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(865, 574, true);
			this.Controls.Add(this.RefreshButton);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.LogViewer);
			this.DataSourceType = typeof(Enterprise.ServiceManager.Business.ServiceTaskLogViewer);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 270, true);
			this.Name = "ServiceTaskLogViewerForm";
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ServiceTaskLogViewerForm_KeyDown);
			this.Controls.SetChildIndex(this.LogViewer, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.RefreshButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		private Enterprise.ZArchitecture.GUI.ZButton RefreshButton;
		private LogViewerControl LogViewer;
	}
}
