namespace Enterprise.BufferManagement.GUI
{
	partial class AcceptabilityBandForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
				
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.acceptabilityBandControl = new Enterprise.BufferManagement.GUI.AcceptabilityBandControl();
			this.MENTTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MENTTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.MENTTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(974, 943, true);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MENTTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.acceptabilityBandControl);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(966, 916, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(966, 916, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(974, 943, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(974, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.Business.BMComponentAcceptabilityBand);
			// 
			// acceptabilityBandControl
			// 
			this.acceptabilityBandControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.acceptabilityBandControl, ".");
			this.acceptabilityBandControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.acceptabilityBandControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.acceptabilityBandControl.Name = "acceptabilityBandControl";
			this.acceptabilityBandControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(966, 916, true);
			this.acceptabilityBandControl.TabIndex = 0;
			// 
			// MENTTabPage
			// 
			this.MENTTabPage.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("9d7d558d-dfdf-450e-aa3b-4390fe61532b", "Measurement");
			this.MENTTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MENTTabPage.Name = "MENTTabPage";
			this.MENTTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(966, 916, true);
			this.MENTTabPage.TabIndex = 3;
			// 
			// AcceptabilityBandForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(974, 999, true);
			this.DataSourceType = typeof(Enterprise.BufferManagement.Business.BMComponentAcceptabilityBand);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(990, 1038, true);
			this.Name = "AcceptabilityBandForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabPage.ResumeLayout(false);
			this.MainPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MENTTabPage.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private AcceptabilityBandControl acceptabilityBandControl;
		private ZArchitecture.GUI.ZTabPage MENTTabPage;
	}
}
