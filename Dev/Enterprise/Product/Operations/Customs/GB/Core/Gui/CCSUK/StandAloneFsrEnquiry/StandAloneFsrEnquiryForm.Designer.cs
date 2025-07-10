using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	partial class StandAloneFsrEnquiryForm
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
		protected override void InitializeComponent()
		{
			this.zWebBrowser1 = new Enterprise.ZArchitecture.GUI.ZWebBrowser();
			this.zWebBrowser2 = new Enterprise.ZArchitecture.GUI.ZWebBrowser();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(835, 534, true);
			this.MainTabControl.SizeMode = System.Windows.Forms.TabSizeMode.FillToRight;
			// 
			// MainTabPage
			// 
			this.MainTabPage.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("de67c7a2-925f-4a94-bee0-39a1be355d57", "Message");
			this.MainTabPage.Controls.Add(this.zWebBrowser2);
			this.MainTabPage.Controls.Add(this.zWebBrowser1);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(827, 507, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(827, 507, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(835, 534, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(835, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.StandAloneFsrEnquiry);
			// 
			// zWebBrowser1
			// 
			this.zWebBrowser1.AllowWebBrowserDrop = false;
			this.zWebBrowser1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)));
			this.zWebBrowser1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.zWebBrowser1.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
			this.zWebBrowser1.Name = "zWebBrowser1";
			this.zWebBrowser1.ScriptErrorsSuppressed = true;
			this.zWebBrowser1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(362, 501, true);
			this.zWebBrowser1.TabIndex = 6;
			this.zWebBrowser1.Url = new System.Uri("about:blank", System.UriKind.Absolute);
			// 
			// zWebBrowser2
			// 
			this.zWebBrowser2.AllowWebBrowserDrop = false;
			this.zWebBrowser2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.zWebBrowser2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(371, 3, true);
			this.zWebBrowser2.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
			this.zWebBrowser2.Name = "zWebBrowser2";
			this.zWebBrowser2.ScriptErrorsSuppressed = true;
			this.zWebBrowser2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 501, true);
			this.zWebBrowser2.TabIndex = 9;
			this.zWebBrowser2.Url = new System.Uri("about:blank", System.UriKind.Absolute);
			// 
			// StandAloneFsrEnquiryForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("25f32173-f97d-4c62-b489-bdbb8b303493", "Community Database Enquiry (FSR)");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(835, 590, true);
			this.DataSourceType = typeof(Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.StandAloneFsrEnquiry);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(851, 628, true);
			this.Name = "StandAloneFsrEnquiryForm";
			this.ShouldSerializeTabPageMethods = false;
			this.MainTabControl.ResumeLayout(false);
			this.MainTabPage.ResumeLayout(false);
			this.MainPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private ZWebBrowser zWebBrowser1;
		private ZWebBrowser zWebBrowser2;
	}
}
