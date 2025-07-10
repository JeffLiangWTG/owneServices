using System.ComponentModel;

namespace Enterprise.Registry.GUI
{
	public partial class OrgCodeUpdaterForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private IContainer components;

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
			this.components = new System.ComponentModel.Container();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.UpdateProgressBar = new CargoWise.Windows.UI.KProgressBar();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 86, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(432, 24, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(217);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(217);
			// 
			// zLabel1
			// 
			this.zLabel1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("OrgCodeUpdaterForm|bcf9922d-4841-4c3e-b101-e6b34030039b", "Updating organization codes. This may take several minutes.");
			this.zLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(416, 22, true);
			this.zLabel1.TabIndex = 1;
			this.zLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// UpdateProgressBar
			// 
			this.UpdateProgressBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 45, true);
			this.UpdateProgressBar.Name = "UpdateProgressBar";
			this.UpdateProgressBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(416, 21, true);
			this.UpdateProgressBar.TabIndex = 2;
			// 
			// OrgCodeUpdaterForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("OrgCodeUpdaterForm|160170cb-a4f9-4dd2-9441-1e604f00806b", "Updating Organization Codes");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(432, 110, true);
			this.Controls.Add(this.UpdateProgressBar);
			this.Controls.Add(this.zLabel1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "OrgCodeUpdaterForm";
			this.Controls.SetChildIndex(this.zLabel1, 0);
			this.Controls.SetChildIndex(this.UpdateProgressBar, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZLabel zLabel1;
		private CargoWise.Windows.UI.KProgressBar UpdateProgressBar;
	}
}
