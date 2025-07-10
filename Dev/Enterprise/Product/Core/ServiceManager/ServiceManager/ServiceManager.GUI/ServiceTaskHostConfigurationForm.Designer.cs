namespace Enterprise.ServiceManager.GUI
{
	partial class ServiceTaskHostConfigurationForm
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
			this.serviceTaskProxyConfigurationControl1 = new Enterprise.ServiceManager.GUI.ServiceTaskProxyConfigurationControl();
			this.postingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 339, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(437, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ServiceManager.Business.StmServiceHost);
			// 
			// serviceTaskProxyConfigurationControl1
			// 
			this.serviceTaskProxyConfigurationControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.serviceTaskProxyConfigurationControl1, ".");
			this.serviceTaskProxyConfigurationControl1.CaptionResourceString = null;
			this.serviceTaskProxyConfigurationControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 12, true);
			this.serviceTaskProxyConfigurationControl1.Name = "serviceTaskProxyConfigurationControl1";
			this.serviceTaskProxyConfigurationControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(412, 291, true);
			this.serviceTaskProxyConfigurationControl1.TabIndex = 1;
			// 
			// postingButtonsUserControl
			// 
			this.postingButtonsUserControl.AllowDrop = true;
			this.postingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.postingButtonsUserControl.CaptionResourceString = null;
			this.postingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(184, 310, true);
			this.postingButtonsUserControl.Name = "postingButtonsUserControl";
			this.postingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 23, true);
			this.postingButtonsUserControl.TabIndex = 4;
			// 
			// ServiceTaskHostConfigurationForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("ServiceTaskHostConfigurationForm|f06ce278-c1cd-4de6-948d-9d8be7a52898", "Host Configuration");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(437, 363, true);
			this.Controls.Add(this.serviceTaskProxyConfigurationControl1);
			this.Controls.Add(this.postingButtonsUserControl);
			this.DataSourceType = typeof(Enterprise.ServiceManager.Business.StmServiceHost);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "ServiceTaskHostConfigurationForm";
			this.Controls.SetChildIndex(this.postingButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.serviceTaskProxyConfigurationControl1, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private ServiceTaskProxyConfigurationControl serviceTaskProxyConfigurationControl1;
		private Core.Forms.ZPostingButtonsUserControl postingButtonsUserControl;



	}
}
