using Enterprise.Client.UPE.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.UPE.GUI
{
	public partial class AlertForm : ZChildForm
	{
		private Enterprise.ZArchitecture.GUI.ZButton OKButton;
		private Enterprise.ZArchitecture.GUI.ZGroupBox AlertGroupBox;
		private Enterprise.ZArchitecture.ZLabel AlertsLabel;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AlertGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AlertsLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			this.AlertGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 276, true);
			this.MainStatusBar.Name = "MainStatusBar";
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(552, 26, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(276);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(276);
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(472, 252, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.TabIndex = 1;
			this.OKButton.Text = "&OK";
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// AlertGroupBox
			// 
			this.AlertGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
				| System.Windows.Forms.AnchorStyles.Left)
				| System.Windows.Forms.AnchorStyles.Right)));
			this.AlertGroupBox.Controls.Add(this.AlertsLabel);
			this.AlertGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AlertGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 6, true);
			this.AlertGroupBox.Name = "AlertGroupBox";
			this.AlertGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(545, 240, true);
			this.AlertGroupBox.TabIndex = 2;
			this.AlertGroupBox.TabStop = false;
			this.AlertGroupBox.Text = "Please Take Note:";
			// 
			// AlertsLabel
			// 
			this.AlertsLabel.BindTo = "Alerts";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.Alert)(null)).AlertsInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.Alert)(null)).Alerts)));
			this.AlertsLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AlertsLabel.ForeColor = System.Drawing.Color.Red;
			this.AlertsLabel.IsFontBold = true;
			this.AlertsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.AlertsLabel.Name = "AlertsLabel";
			this.AlertsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(539, 221, true);
			this.AlertsLabel.TabIndex = 0;
			this.AlertsLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// AlertForm
			// 
			this.MinimizeBox = false;
			this.MaximizeBox = false;

			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(552, 302, true);
			this.Controls.Add(this.AlertGroupBox);
			this.Controls.Add(this.OKButton);
			this.DataSourceAssemblyName = "ZClientUPE";
			this.DataSourceTypeName = "Enterprise.Client.UPE.Business.Alert";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "AlertForm";
			this.Text = "AlertForm";
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.AlertGroupBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			this.AlertGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);
		}
	}
}
