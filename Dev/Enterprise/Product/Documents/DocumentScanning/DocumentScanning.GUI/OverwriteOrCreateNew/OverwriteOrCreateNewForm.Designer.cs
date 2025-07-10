using System.ComponentModel;
using CargoWise.Common.Testing;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentScanning.GUI
{
	public partial class OverwriteOrCreateNewForm : ZChildForm
	{
		private Enterprise.ZArchitecture.GUI.ZButton CreateNewButton;
		private Enterprise.ZArchitecture.GUI.ZButton OverwriteButton;
		private Enterprise.ZArchitecture.GUI.ZButton CancelNewButton;
		private Enterprise.ZArchitecture.ZLabel NameLabel;

		new void InitializeComponent()
		{
			this.CreateNewButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OverwriteButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelNewButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.NameLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 85, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(326, 7, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 4;
			this.MainStatusBar.Visible = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(163);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(163);
			// 
			// CreateNewButton
			// 
			this.CreateNewButton.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("OverwriteOrCreateNewForm|01c6994d-bc08-4973-b68f-04640874ee25", "Create New File");
			this.CreateNewButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 52, true);
			this.CreateNewButton.Name = "CreateNewButton";
			this.CreateNewButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 21, true);
			this.CreateNewButton.TabIndex = 2;
			this.CreateNewButton.Click += new System.EventHandler(this.CreateNewButton_Click);
			// 
			// OverwriteButton
			// 
			this.OverwriteButton.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("OverwriteOrCreateNewForm|ebf5a331-2667-4c5c-8e9b-ccedf767d2ca", "Overwrite Existing File");
			this.OverwriteButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 52, true);
			this.OverwriteButton.Name = "OverwriteButton";
			this.OverwriteButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 21, true);
			this.OverwriteButton.TabIndex = 1;
			this.OverwriteButton.Click += new System.EventHandler(this.OverwriteButton_Click);
			// 
			// CancelNewButton
			// 
			this.CancelNewButton.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("OverwriteOrCreateNewForm|c6b29de7-7d28-4982-8e6d-4504aa8cc3b8", "Cancel");
			this.CancelNewButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(256, 52, true);
			this.CancelNewButton.Name = "CancelNewButton";
			this.CancelNewButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 21, true);
			this.CancelNewButton.TabIndex = 3;
			this.CancelNewButton.Click += new System.EventHandler(this.CancelNewButton_Click);
			// 
			// NameLabel
			// 
			this.NameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 7, true);
			this.NameLabel.Name = "NameLabel";
			this.NameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 38, true);
			this.NameLabel.TabIndex = 0;
			// 
			// OverwriteOrCreateNewForm
			// 

			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(326, 92, true);
			this.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("OverwriteOrCreateNewForm|4429c680-a586-4326-bc11-355b48afae0f", "Please select an option");
			this.Controls.Add(this.NameLabel);
			this.Controls.Add(this.CancelNewButton);
			this.Controls.Add(this.OverwriteButton);
			this.Controls.Add(this.CreateNewButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.Name = "OverwriteOrCreateNewForm";
			this.Controls.SetChildIndex(this.CreateNewButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.OverwriteButton, 0);
			this.Controls.SetChildIndex(this.CancelNewButton, 0);
			this.Controls.SetChildIndex(this.NameLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		private System.ComponentModel.Container components = null;
		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				if (components != null)
				{
					components.Dispose();
				}
				DisposableLeakListener.Instance.UnRegisterDisposable(this);
			}
			base.Dispose(isNotFinalizing);
		}
	}
}
